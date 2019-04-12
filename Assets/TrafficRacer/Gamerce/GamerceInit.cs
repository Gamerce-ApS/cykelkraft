using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Net;
using System.Collections;
using UnityEngine.Networking;

public class GamerceInit : MonoBehaviour
{

	[System.Serializable]
	public class ReturnValue
	{
		public float totalTime;

		public static ReturnValue CreateFromJSON(string jsonString)
		{
			return JsonUtility.FromJson<ReturnValue>(jsonString);
		}
	}

	public static GamerceInit instance;

	public bool internalDataRecieved;
	public bool isLoggingIn;

	public int MaxGamercePoints;
	public float OneStarPoints;
	public float TwoStarPoints;
	public float ThreeStarPoints;
	public float MaxPointsPerLevel;
	public float DiscountPlayTime;
	public int NumberOfTimesToAskForNewsLetter;
	public float PointsPerSecond;


	public int TotalLevels;
	public bool shouldShowDiscountWindow = false;
	public bool shouldShowNewsletterWindow;
	public bool shouldShowLoginWindow;

	public List<string> discountPercents;

	float startingPoints;
	public int gameOverCounter = 0;

	public PlayerData playerData;

	string gamerceLink;
	string termsAndConditionLink;
	string privacyPolicyLink;

	public string GamerceLink
	{
		get
		{
			if (string.IsNullOrEmpty(gamerceLink))
			{
				return "https://gamerce.net";
			}
			return gamerceLink;
		}
	}

	public string TermsAndConditionLink
	{
		get
		{
			if (string.IsNullOrEmpty(termsAndConditionLink))
			{
				return "https://gamerce.net";
			}
			return termsAndConditionLink;
		}
	}

	public string PrivacyPolicyLink
	{
		get
		{
			if (string.IsNullOrEmpty(privacyPolicyLink))
			{
				return "https://gamerce.net";
			}
			return privacyPolicyLink;
		}
	}

	bool isWaitingForPlayfabTicker;

	public bool IsWaitingForPlayfabTicker
	{
		set { isWaitingForPlayfabTicker = value; }
		get { return isWaitingForPlayfabTicker; }
	}

	public string SceneToLoad;

	public bool hasWon;

	public string playfabId;
	public string countryCode;

	bool hasInternet;
	bool isLogedInToPlayfab;
	public bool IsLogedInToPlayfab
	{
		get
		{
			return isLogedInToPlayfab;
		}
	}

	public Dictionary<string,Texture2D> clothTextures;

	public PlayFabTicker playfabTicker;

	public bool ShouldShowGamerce;

	private void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
			return;
		}
		instance = this;
		hasWon = false;
		isWaitingForPlayfabTicker = false;
		hasInternet = false;
		isLogedInToPlayfab = false;
		internalDataRecieved = false;
		MaxGamercePoints = 0;
		OneStarPoints = 0f;
		TwoStarPoints = 0f;
		ThreeStarPoints = 0f;
		MaxPointsPerLevel = 0f;
		DiscountPlayTime = 0f;
		NumberOfTimesToAskForNewsLetter = 3;
		shouldShowDiscountWindow = false;
		shouldShowNewsletterWindow = false;
		shouldShowLoginWindow = false;
		SceneToLoad = "";
		discountPercents = new List<string>();
		clothTextures = new Dictionary<string, Texture2D>();
		ShouldShowGamerce = false;
	}

	public void Start()
	{
		startingPoints = PlayerPrefs.GetFloat("PlayedForTime", 0f);
		//Facebook.Unity.FB.Init();
		DontDestroyOnLoad(gameObject);
		playerData = new PlayerData();
		GameAnalyticsSDK.GameAnalytics.Initialize();

		hasInternet = CheckInternetAvailability();
		/*if (hasInternet == false)
		{
			//PlayerPrefs.DeleteKey("G_Username");
			//PlayerPrefs.DeleteKey("G_Password");
			

			//return;
		}*/

		LoginToPlayfab();
		
	}

	public void StartTicker()
	{
		playfabTicker.StartTicker();
	}

	public void EndTicker()
	{
		playfabTicker.EndTicker();
	}

	public void LoginToPlayfab(Action onLogin = null)
	{
		if (Application.isEditor)
		{
			var request = new LoginWithCustomIDRequest {
				CustomId = SystemInfo.deviceUniqueIdentifier,
				CreateAccount = true
			};
			PlayFabClientAPI.LoginWithCustomID(request, (result)=> 
			{
				if (onLogin != null)
					onLogin();
				OnLoginSuccess(result);
			}, OnLoginFailure);
		}
		else if (Application.isMobilePlatform)
		{
#if UNITY_ANDROID
			var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			var currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
			var contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
			var secure = new AndroidJavaClass("android.provider.Settings$Secure");
			var android_id = secure.CallStatic<string>("getString", contentResolver, "android_id");

			var requestAndroid = new LoginWithAndroidDeviceIDRequest
			{
				TitleId = PlayFabSettings.TitleId,
				AndroidDeviceId = android_id,
				CreateAccount = true
			};
			PlayFabClientAPI.LoginWithAndroidDeviceID(requestAndroid, OnLoginSuccess, OnLoginFailure);
#else

			var requestIOS = new LoginWithIOSDeviceIDRequest { DeviceId=SystemInfo.deviceUniqueIdentifier , TitleId=PlayFabSettings.TitleId , CreateAccount = true};


			PlayFabClientAPI.LoginWithIOSDeviceID(requestIOS, OnLoginSuccess, OnLoginFailure);

#endif
		}
	}

	void SetUpDefaultInternalData()
	{
		if(internalDataRecieved == false)
		{
			internalDataRecieved = true;

			MaxGamercePoints = 60;
			OneStarPoints = 0.1f;
			TwoStarPoints = 3f;
			ThreeStarPoints = 5f;
			MaxPointsPerLevel = 5f;
			TotalLevels = PlayerPrefs.GetInt("TotalLevels", 60);
			NumberOfTimesToAskForNewsLetter = 3;
		}
	}

	public bool IsLoggedIn()
	{
		string username = PlayerPrefs.GetString("G_Username","");
		string password = PlayerPrefs.GetString("G_Password", "");
		//bool hasUsername = PlayerPrefs.HasKey("G_Username");
		//bool hasPassword = PlayerPrefs.HasKey("G_Password");
		//return hasUsername == true && hasPassword == true;
		if (string.IsNullOrEmpty(username) == false && string.IsNullOrEmpty(password) == false)
			return true;
		return false;
	}

	private void OnLoginSuccess(LoginResult result2)
	{
		isLogedInToPlayfab = true;
		playfabId = result2.PlayFabId;
		var request = new ExecuteCloudScriptRequest();
		request.FunctionName = "GetTimePlayed";
		request.GeneratePlayStreamEvent = true;

		PlayFabClientAPI.ExecuteCloudScript(request, result =>
			{
				//playerData = JsonConvert.DeserializeObject<PlayerData>(result.FunctionResult.ToString());
				float time = ReturnValue.CreateFromJSON(result.FunctionResult.ToString()).totalTime;

				PlayerPrefs.SetFloat("PlayedForTime", time);

				float pointsToSync = PlayerPrefs.GetFloat("PointsToSync", 0f);
				if (pointsToSync > 0)
					SendTime(pointsToSync, (b) =>
					{
						PlayerPrefs.SetFloat("PointsToSync", 0f);
					}, true);



			}, error =>
			{

			}
		);

		string username = PlayerPrefs.GetString("G_Username", "");
		string password = PlayerPrefs.GetString("G_Password", "");

		if (string.IsNullOrEmpty(username) == false && string.IsNullOrEmpty(password) == false)
		{
			PressedLogin(username, password, null);
			isLoggingIn = true;
		}
		else
		{
			PressedLogout(null);
			isLoggingIn = false;
			//shouldShowLoginWindow = true;
		}
		GetPlayerData();
		GetInternalData();
	}

	private void GetPlayerData()
	{
		GetPlayerProfileRequest req = new GetPlayerProfileRequest
		{
			PlayFabId = playfabId,
			ProfileConstraints = new PlayerProfileViewConstraints
			{
				ShowLocations = true
			}
		};

		PlayFabClientAPI.GetPlayerProfile(req, result =>
		{
			var locations = result.PlayerProfile.Locations;
			foreach(var location in locations)
			{
				countryCode = location.CountryCode.ToString();
			}
		}, 
		error => 
		{
		});
	}


	private void OnLoginFailure(PlayFabError error)
	{
		isLogedInToPlayfab = false;
		SetUpDefaultInternalData();
	}

	public void GetInternalData()
	{
		PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
			result =>
			{
				if (result.Data != null)
				{
					if(result.Data.ContainsKey("MaxPoints"))
					{
						MaxGamercePoints = int.Parse(result.Data["MaxPoints"]);
					}
					if(result.Data.ContainsKey("OneStarPoints"))
					{
						OneStarPoints = float.Parse(result.Data["OneStarPoints"]);
					}
					if (result.Data.ContainsKey("TwoStarPoints"))
					{
						TwoStarPoints = float.Parse(result.Data["TwoStarPoints"]);
					}
					if (result.Data.ContainsKey("ThreeStarPoints"))
					{
						ThreeStarPoints = float.Parse(result.Data["ThreeStarPoints"]);
					}
					if (result.Data.ContainsKey("MaxPointsPerLevel"))
					{
						MaxPointsPerLevel = float.Parse(result.Data["MaxPointsPerLevel"]);
					}
					if (result.Data.ContainsKey("TotalLevels"))
					{
						TotalLevels = int.Parse(result.Data["TotalLevels"]);
						PlayerPrefs.SetInt("TotalLevels", TotalLevels);
					}
					if (result.Data.ContainsKey("NumberOfTimesToAskForNewsLetter"))
					{
						NumberOfTimesToAskForNewsLetter = int.Parse(result.Data["NumberOfTimesToAskForNewsLetter"]);
					}
					if(result.Data.ContainsKey("Discount"))
					{
						discountPercents.Add(result.Data["Discount"]);
					}
					if(result.Data.ContainsKey("DiscountPlayTime"))
					{
						DiscountPlayTime = float.Parse(result.Data["DiscountPlayTime"]);
					}
					if (result.Data.ContainsKey("TermsAndConditionLink"))
					{
						termsAndConditionLink = result.Data["TermsAndConditionLink"];
					}
					if (result.Data.ContainsKey("GamerceLink"))
					{
						gamerceLink = result.Data["GamerceLink"];
					}
					if (result.Data.ContainsKey("PrivacyPolicyLink"))
					{
						privacyPolicyLink = result.Data["PrivacyPolicyLink"];
					}
					if (result.Data.ContainsKey("PointsPerSecond"))
					{
						PointsPerSecond = float.Parse(result.Data["PointsPerSecond"]);
					}
				}
				internalDataRecieved = true;

			},
			error => 
			{
				SetUpDefaultInternalData();
			});
	}

	public void PressedLogin(string username, string pw, System.Action<bool> onLogedIn, bool aShouldSendPlayfabId = false)
	{
		if(IsLogedInToPlayfab == false)
		{
			LoginToPlayfab();
			return;
		}
		var request = new ExecuteCloudScriptRequest();
		request.FunctionName = "Login";
		request.FunctionParameter = new Dictionary<string, string> {
			{ "username", username},
			{ "password", pw }

		};
		request.GeneratePlayStreamEvent = true;

		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			if (result.FunctionResult != null)
			{
				try
				{
					
					playerData = JsonConvert.DeserializeObject<PlayerData>(result.FunctionResult.ToString());
					StartCoroutine(LoginRoutine(isSuccess =>
					{
						if (isSuccess == true)
						{
							PlayerPrefs.SetString("G_Username", username);
							PlayerPrefs.SetString("G_Password", pw);
							if (onLogedIn != null)
								onLogedIn(true);
						}
						else
						{
							if (onLogedIn != null)
								onLogedIn(false);
						}
					}, username, pw, aShouldSendPlayfabId));


				}
				catch(Exception e)
				{
					if (onLogedIn != null)
						onLogedIn(false);
				}

			}

		}, error =>
		{
			onLogedIn(false);

		});
	}

	IEnumerator LoginRoutine(Action<bool> onComplete, string aUsername, string aPassword, bool aShouldSendPlayfabId)
	{
		string url = "https://gamerce.net/gamerce_login/?username=" + aUsername + "&password=" + aPassword + "&points=" + playerData.GetPoints().ToString() + "&game=Cykelkraft";
		if (playerData.GetAmountOfDiscounts() > 0)
		{
			url += "&discount_code=" + GetAllDiscountCodes() + "&discount_amount=" + GetAllDiscountPercent();
		}
		if (aShouldSendPlayfabId == true)
		{
			url += "&playfabId=" + playfabId;
		}
		Debug.Log(url);
		UnityWebRequest www = UnityWebRequest.Get(url);
		yield return www.SendWebRequest();


		if (www.isNetworkError || www.isHttpError)
		{
			//MenuManager.Instance.CloseLoadingWindow();
			//MenuManager.Instance.ShowErrorMessage("An error has occurred. Try again");
			Debug.Log(www.error);
		}
		else
		{
			Debug.Log(www.downloadHandler.text);
			if (www.downloadHandler.text == "error")
			{
				if (onComplete != null)
					onComplete(false);
			}
			else
			{
				try
				{
					PlayerPrefs.SetString("G_Username", aUsername);
					PlayerPrefs.SetString("G_Password", aPassword);
					string data = www.downloadHandler.text.Substring(1);
					playerData = JsonConvert.DeserializeObject<PlayerData>(data);
					PlayerPrefs.SetString("G_Email", playerData.email);
					float points = playerData.GetPoints();
					PlayerPrefs.SetFloat("PlayedForTime", points / PointsPerSecond);

					if (points >= ThreeStarPoints)
					{
						PlayerPrefs.SetInt("DiscountWindowShowed", 3);
					}
					else if (points >= TwoStarPoints)
					{
						PlayerPrefs.SetInt("DiscountWindowShowed", 2);
					}
					else if (points >= OneStarPoints)
					{
						PlayerPrefs.SetInt("DiscountWindowShowed", 1);
					}

					Debug.Log(playerData.GetPoints());
					ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest();
					request.FunctionName = "UpdateTimePlayed";
					request.FunctionParameter = new Dictionary<string, string> {
						{ "points", points.ToString()}

					};
					request.GeneratePlayStreamEvent = true;

					PlayFabClientAPI.ExecuteCloudScript(request, result => {
						isLoggingIn = false;
						if (onComplete != null)
							onComplete(true);

					}, error => { });
				}
				catch (Exception e)
				{

				}
			}

		}
	}

	public void Register(string aUsername, string aPassword, string aEmail, Action<bool> onSuccess = null, Action<RegisterResponse> onFail = null)
	{

		if (IsLogedInToPlayfab == false)
		{
			LoginToPlayfab();
			return;
		}
		var request = new ExecuteCloudScriptRequest();
		request.FunctionName = "Register";
		request.GeneratePlayStreamEvent = true;

		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			if (result.FunctionResult != null)
			{
				ReturnValue returnValue = ReturnValue.CreateFromJSON(result.FunctionResult.ToString());
				StartCoroutine(RegisterRoutine(aUsername, aPassword, aEmail, returnValue.totalTime, onSuccess, onFail));
			}

		}, error =>
		{
			if (onFail != null)
				onFail(null);
		});
	}

	IEnumerator RegisterRoutine(string aUsername, string aPassword, string aEmail, float points, Action<bool> onSuccess = null, Action<RegisterResponse> onFail = null)
	{
		UnityWebRequest www = UnityWebRequest.Get("https://gamerce.net/gamerce_register/?username=" + aUsername + "&password=" + aPassword + "&email=" + aEmail + "&game=Cykelkraft&points=" + points + "&playfabId=" + playfabId);
		yield return www.SendWebRequest();


		if (www.isNetworkError || www.isHttpError)
		{
			//MenuManager.Instance.CloseLoadingWindow();
			//MenuManager.Instance.ShowErrorMessage("An error has occurred. Try again");
			Debug.Log(www.error);
		}
		else
		{
			try
			{
				//
				//if(response.IsError() == true)
				//{
				//	Debug.Log("Error!");
				//}
				//else
				//{
				string responseString = www.downloadHandler.text;
				string data = responseString.Substring(1);
				if (responseString.StartsWith("1"))//Success
				{
					PlayerPrefs.SetString("G_Username", aUsername);
					PlayerPrefs.SetString("G_Password", aPassword);
					PlayerPrefs.SetString("G_Email", aEmail);
					playerData = JsonConvert.DeserializeObject<PlayerData>(data);
					var request = new ExecuteCloudScriptRequest();
					request.FunctionName = "LoginAfterRegister";
					request.FunctionParameter = new Dictionary<string, string> {
						{ "username", aUsername},
						{ "password", aPassword },
						{ "points", playerData.GetPoints().ToString()}

					};
					request.GeneratePlayStreamEvent = true;

					PlayFabClientAPI.ExecuteCloudScript(request, result =>
					{
						if (result.FunctionResult != null)
						{
							playerData = JsonConvert.DeserializeObject<PlayerData>(result.FunctionResult.ToString());
							StartCoroutine(AddDiscountRoutine(onSuccess, 0f));
						}
					},
					error =>
					{

					});
				}
				else if (responseString.StartsWith("2"))//error
				{
					RegisterResponse response = JsonConvert.DeserializeObject<RegisterResponse>(data);
					if (onFail != null)
						onFail(response);
				}

			}
			catch (Exception e)
			{

			}
		}
	}

	internal string GetLatestDiscountCodeAndPercent()
	{
		string discountCode = string.Empty;
		if (playerData != null && playerData.games != null && playerData.games.Cykelkraft != null)
		{
			Dictionary<string, string> discounts = playerData.games.Cykelkraft.discounts;
			if (discounts != null)
			{
				int highestPercent = -1;
				foreach (KeyValuePair<string, string> discount in discounts)
				{
					string percent = discount.Value.Remove(discount.Value.IndexOf("%"));
					int percentAmount = -1;

					if (int.TryParse(percent, out percentAmount) == true)
					{
						if (percentAmount > highestPercent)
						{
							highestPercent = percentAmount;
							discountCode = discount.Value + " = " + discount.Key;
						}
					}
				}
			}
		}
		Debug.Log(discountCode);

		return discountCode;
	}

	internal string GetLatestDiscountCode()
	{
		string discountCode = string.Empty;
		if (playerData != null && playerData.games != null && playerData.games.Cykelkraft != null)
		{
			Dictionary<string, string> discounts = playerData.games.Cykelkraft.discounts;
			if (discounts != null)
			{
				int highestPercent = -1;
				foreach (KeyValuePair<string, string> discount in discounts)
				{
					string percent = discount.Value.Remove(discount.Value.IndexOf("%"));
					int percentAmount = -1;

					if (int.TryParse(percent, out percentAmount) == true)
					{
						if (percentAmount > highestPercent)
						{
							highestPercent = percentAmount;
							discountCode = discount.Key;
						}
					}
				}
			}
		}
		Debug.Log(discountCode);

		return discountCode;
	}

	public void PressedLogout(Action onSuccess)
	{
		//if (hasInternet == false)
		//{
		//	return;
		//}
		if (IsLogedInToPlayfab == false)
		{
			LoginToPlayfab();
			return;
		}
		var request = new ExecuteCloudScriptRequest();
		request.FunctionName = "Logout";
		request.GeneratePlayStreamEvent = true;

		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			PlayerPrefs.DeleteKey("G_Username");
			PlayerPrefs.DeleteKey("G_Password");
			PlayerPrefs.DeleteKey("G_Email");
			PlayerPrefs.DeleteKey("HaveAcceptedGamerceNL");
			PlayerPrefs.DeleteKey("HaveAcceptedCKNL");
			PlayerPrefs.DeleteKey("DiscountWindowShowed");
			PlayerPrefs.DeleteKey("PlayedForTime");
			PlayerPrefs.DeleteKey("TimesShowedNewsLetter");
			PlayerPrefs.DeleteKey("NewsLetter");
			PlayerPrefs.DeleteKey("HaveSeenReward");
			playerData = null;
			playerData = new PlayerData();
			if (onSuccess != null)
				onSuccess();
		},
		error =>
		{
			GameAnalyticsManager.instance.LoggedOutFail();
		});

	}

	public void AcceptNewsLetter(bool acceptGamerce, bool acceptCK, Action onSuccess, Action onFail)
	{
		StartCoroutine(SendNewsLetterCor(acceptGamerce, acceptCK, onSuccess, onFail));
	}

	IEnumerator SendNewsLetterCor(bool acceptGamerce, bool acceptRosemunde, Action onSuccess, Action onFail)
	{
		if (acceptRosemunde)
		{
			UnityWebRequest www2 = UnityWebRequest.Get("https://gamerce.net/gamerce_newslettersignup/?email=" + PlayerPrefs.GetString("G_Email", "") + "&permission=CykelKraft&source=CykelKraft");
			yield return www2.SendWebRequest();
		}

		if (acceptGamerce)
		{
			UnityWebRequest www3 = UnityWebRequest.Get("https://gamerce.net/gamerce_newslettersignup/?email=" + PlayerPrefs.GetString("G_Email", "") + "&permission=Gamerce&source=CykelKraft");
			yield return www3.SendWebRequest();
		}

		if (onSuccess != null)
			onSuccess();
	}

	public string GetDiscountEmailFormat()
	{
		string discountCode = "";
		if (playerData != null && playerData.games != null && playerData.games.Cykelkraft != null)
		{
			Dictionary<string, string> discounts = playerData.games.Cykelkraft.discounts;
			int discountsAmount = 0;
			if (discounts != null)
			{
				int count = 0;
				foreach (KeyValuePair<string, string> discount in discounts)
				{
					if (count > 0)
						discountCode += "<br>";
					discountCode += discount.Value + "=" + discount.Key + "  ";
					count++;
				}
				discountsAmount = discounts.Count;
			}

		}
		Debug.Log(discountCode);

		return discountCode;
	}

	public string GetDiscountHeader()
	{
		string discountCode = "";
		if (playerData != null && playerData.games != null && playerData.games.Cykelkraft != null)
		{	
			Dictionary<string, string> discounts = playerData.games.Cykelkraft.discounts;
			int discountsAmount = 0;
			if (discounts != null)
			{
				foreach (KeyValuePair<string, string> discount in discounts)
				{
					discountCode += discount.Value + ": " + discount.Key + "  ";
				}
				discountsAmount = discounts.Count;
			}
			for(int i = discountsAmount; i < discountPercents.Count; ++i)
			{
				discountCode += discountPercents[i] + ": \ud83d\udd12 ";
			}
		}
		Debug.Log(discountCode);

		return discountCode;
	}

	public string GetLatestDiscountPercent()
	{
		string discountCode = "";
		if (playerData != null && playerData.games != null && playerData.games.Cykelkraft != null)
		{
			Dictionary<string, string> discounts = playerData.games.Cykelkraft.discounts;
			if (discounts != null)
			{
				int highestPercent = -1;
				foreach (KeyValuePair<string, string> discount in discounts)
				{
					string percent = discount.Value.Remove(discount.Value.IndexOf("%"));
					int percentAmount = -1;

					if (int.TryParse(percent, out percentAmount) == true)
					{
						if (percentAmount > highestPercent)
						{
							highestPercent = percentAmount;
							discountCode = highestPercent.ToString();
						}
					}
				}
			}
		}
		Debug.Log(discountCode);

		return discountCode;
	}

	public string GetAllDiscountPercent()
	{
		string discountCode = string.Empty;
		if (playerData != null && playerData.games != null && playerData.games.Cykelkraft != null)
		{
			Dictionary<string, string> discounts = playerData.games.Cykelkraft.discounts;
			if (discounts != null)
			{
				foreach (KeyValuePair<string, string> discount in discounts)
				{
					string percent = discount.Value.Remove(discount.Value.IndexOf("%"));
					int percentAmount = -1;

					if (int.TryParse(percent, out percentAmount) == true)
					{
						discountCode += percentAmount.ToString() + ",";
					}
				}
			}
		}
		discountCode = discountCode.Substring(0, discountCode.Length - 1);
		Debug.Log(discountCode);

		return discountCode;
	}

	public string GetAllDiscountCodes()
	{
		string discountCode = string.Empty;
		if (playerData != null && playerData.games != null && playerData.games.Cykelkraft != null)
		{
			Dictionary<string, string> discounts = playerData.games.Cykelkraft.discounts;
			if (discounts != null)
			{
				foreach (KeyValuePair<string, string> discount in discounts)
				{
					discountCode += discount.Key + ",";
				}
			}
		}
		discountCode = discountCode.Substring(0, discountCode.Length - 1);
		Debug.Log(discountCode);

		return discountCode;
	}

	public void AddDiscounts(string aUsername, string aPassword)
	{
		if(IsLogedInToPlayfab == false)
		{
			LoginToPlayfab();
			return;
		}

		var request = new ExecuteCloudScriptRequest();
		request.FunctionName = "AddDiscountCodes";
		request.GeneratePlayStreamEvent = true;
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add("username", aUsername);
		parameters.Add("password", aPassword);
		request.FunctionParameter = parameters;

		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			if (result.FunctionResult != null)
			{
				try
				{
					playerData = JsonConvert.DeserializeObject<PlayerData>(result.FunctionResult.ToString());
				}
				catch (Exception e)
				{

				}

			}
		},
		error =>
		{
		});
	}

	public void SendTime(float aTimePlayed, Action<bool> onComplete, bool sync)
	{
		if(IsLogedInToPlayfab == false)
		{
			LoginToPlayfab();
			onComplete(false);
			return;
		}
		var request = new ExecuteCloudScriptRequest();
		request.FunctionName = "AddPlayedForTime";
		//if(sync == false)
		//{
		//	if (aTimePlayed > MaxPointsPerLevel)
		//		aTimePlayed = MaxPointsPerLevel;
		//}


		Debug.Log("TimePlayed: " + aTimePlayed);
		Dictionary<string, object> para = new Dictionary<string, object> {
			{ "TimePlayed", aTimePlayed },
			{
				"username",
				PlayerPrefs.GetString ("G_Username")
			},
			{
				"password",
				PlayerPrefs.GetString ("G_Password")
			}
		};
		if (sync)
			para.Add("Sync", true);
		request.FunctionParameter = para;
		request.GeneratePlayStreamEvent = true;

		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			if (result.FunctionResult != null)
			{
				//float time = ReturnValue.CreateFromJSON(result.FunctionResult.ToString()).totalTime;
				float points = PlayerPrefs.GetFloat("PlayedForTime", 0f);
				
				try
				{
					var pd = JsonConvert.DeserializeObject<PlayerData>(result.FunctionResult.ToString());
					playerData.games.Cykelkraft.discounts = pd.games.Cykelkraft.discounts;
					playerData.AddPoints(pd.GetPoints());
					points = playerData.GetPoints();
					StartCoroutine(AddDiscountRoutine(null, pd.GetPoints()));

				}
				catch (Exception e)
				{
					points = ReturnValue.CreateFromJSON(result.FunctionResult.ToString()).totalTime;
				}

				float newPlayedTime = playerData.GetPoints() / PointsPerSecond;
				PlayerPrefs.SetFloat("PlayedForTime", newPlayedTime);

				if (onComplete != null)
				{
					onComplete(true);
				}
			}
			else
			{
				if (onComplete != null)
				{
					onComplete(false);
				}
			}
		}, error =>
		{
			if (onComplete != null)
			{
				onComplete(false);
			}


		});
	}

	IEnumerator AddDiscountRoutine(Action<bool> onComplete, float aTimePlayed)
	{
		string username = PlayerPrefs.GetString("G_Username", "");
		string password = PlayerPrefs.GetString("G_Password", "");


		string url = "https://gamerce.net/gamerce_addgamediscount/?username=" + username + "&password=" + password + "&game=Cykelkraft&points=" + aTimePlayed/*playerData.GetPoints()*/;
		if (playerData.GetAmountOfDiscounts() > 0)
		{
			url += "&discount_code=" + GetAllDiscountCodes() + "&discount_amount=" + GetAllDiscountPercent();
		}

		UnityWebRequest www = UnityWebRequest.Get(url);
		yield return www.SendWebRequest();


		if (www.isNetworkError || www.isHttpError)
		{
			//MenuManager.Instance.CloseLoadingWindow();
			if (onComplete != null)
				onComplete(false);
			Debug.Log(www.error);
		}
		else
		{
			if (www.downloadHandler.text.StartsWith("1") == true)
			{
				string returnString = www.downloadHandler.text.Substring(1);
				try
				{
					playerData = JsonConvert.DeserializeObject<PlayerData>(returnString);
					//PlayerPrefs.SetFloat("PlayedForTime", playerData.GetPoints());
					if (onComplete != null)
						onComplete(true);
				}
				catch (Exception e)
				{

				}
			}
		}
	}

	public bool CheckInternetAvailability()
	{
		try
		{
			using (var client = new WebClient())
			{
				using (client.OpenRead("http://clients3.google.com/generate_204"))
				{
					client.Dispose();
					hasInternet = true;
					return true;
				}
			}
		}
		catch
		{
			hasInternet = false;
			return false;
		}
	}

	public bool HasInternet()
	{
		return hasInternet;
	}

	private void OnApplicationQuit()
	{
		if(PlayerPrefs.GetFloat("PointsToSync", 0f) > 0f)
		{
			SendTime(PlayerPrefs.GetFloat("PointsToSync", 0f), (a) => 
			{
				if(a == true)
					PlayerPrefs.SetFloat("PointsToSync", 0f);
			}, true);

		}
		Debug.Log("Sending played time");
	}
}


[System.Serializable]
public class RegisterResponse
{
	public float Points = -1;
	public string wp_usersname = "";
	public string existing_user_login = "";
	public string user_login_too_long = "";
	public string invalid_username = "";
	public string wpu_password = "";
	public string wpu_email = "";
	public string existing_user_email = "";
}

[System.Serializable]
public class Points
{
	public float points;
}

[Serializable]
public class PlayerData
{
	public string email;
	public GGame games;

	public PlayerData()
	{
		games = new GGame();
	}


	public float GetPoints()
	{
		if(games != null)
			return games.GetPoints();
		return 0f;
	}

	public void AddPoints(float aPoints)
	{
		if (games != null && games.Cykelkraft != null)
		{
			games.Cykelkraft.points += aPoints;
		}
	}

	public int GetAmountOfDiscounts()
	{
		if (games == null)
			return 0;
		return games.GetAmountOfDiscounts();
	}

}

[Serializable]
public class GGame
{
	public CGameData Cykelkraft;

	public GGame()
	{
		Cykelkraft = new CGameData();
	}


	public float GetPoints()
	{
		if(Cykelkraft != null)
			return Cykelkraft.GetPoints();
		return 0f;
	}

	public int GetAmountOfDiscounts()
	{
		if (Cykelkraft == null)
			return 0;
		return Cykelkraft.GetAmountOfDiscounts();
	}
}

[Serializable]
public class CGameData
{
	public float points;
	public Dictionary<string,string> discounts;

	public CGameData()
	{
		discounts = new Dictionary<string, string>();
		points = 0;
	}

	public float GetPoints()
	{
		return points;
	}

	public int GetAmountOfDiscounts()
	{
		if (discounts == null)
			return 0;
		return discounts.Count;
	}
}

[Serializable]
public class Discounts
{
}