using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Net;
using System.Collections;

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

	public int MaxGamercePoints;
	public float OneStarPoints;
	public float TwoStarPoints;
	public float ThreeStarPoints;
	public float MaxPointsPerLevel;
	public float DiscountPlayTime;
	public int NumberOfTimesToAskForNewsLetter;


	public int TotalLevels;
	public bool shouldShowDiscountWindow = false;
	public bool shouldShowNewsletterWindow;
	public bool shouldShowLoginWindow;

	public List<string> discountPercents;

	float startingPoints;

	public PlayerData playerData;

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

			});

		string username = PlayerPrefs.GetString("G_Username", "");
		string password = PlayerPrefs.GetString("G_Password", "");

		if (string.IsNullOrEmpty(username) == false && string.IsNullOrEmpty(password) == false)
		{
			PressedLogin(username, password, null);

		}
		else
		{
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


	//public string[] GetRandomClothItem()
	//{
	//	if(clothPath != null && clothPath.data != null && clothPath.data.GetLength(0) > 0)
	//	{
	//		int randomIndex = UnityEngine.Random.Range(0, clothPath.data.GetLength(0));

	//		return new string[] { clothPath.data[randomIndex, 0], clothPath.data[randomIndex, 1] };
	//	}
	//	return new string[] {"", ""};
	//}

	//IEnumerator DownladClothTextures()
	//{
	//	for(int i = 0; i < clothPath.data.GetLength(0); ++i)
	//	{
	//		if (clothTextures.ContainsKey(clothPath.data[i, 1]))
	//		{
	//			continue;
	//		}
 //  			using (WWW www = new WWW(clothPath.data[i, 0]))
	//		{
	//			yield return www;

	//			clothTextures.Add(clothPath.data[i, 1],www.texture);
	//		}
	//	}
		
	//}


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
				}
				internalDataRecieved = true;

			},
			error => 
			{
				SetUpDefaultInternalData();
			});
	}

	public void UpdateTimePlayedPlayfab()
	{ 

		var request = new ExecuteCloudScriptRequest
		{
			FunctionName = "UpdateTimePlayed"
		};
		float timePlayed = PlayerPrefs.GetFloat("PlayedForTime", 0f);
		request.FunctionParameter = new Dictionary<string, object> {
			{ "points", timePlayed }
		
		};
		//request.FunctionParameter = TimePlayed/60f;
		request.GeneratePlayStreamEvent = true;

		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			/*if (result.FunctionResult != null)
			{
				float time = ReturnValue.CreateFromJSON(result.FunctionResult.ToString()).totalTime;
				PlayerPrefs.SetFloat("PlayedForTime", time);
				Debug.Log("Data Sent Result:" + time);
			}*/


		}, error =>
		{


		});
	}

	public void PressedLogin(string username, string pw, System.Action<bool> onLogedIn)
	{
		//if (hasInternet == false)
		//{
		//	if (onLogedIn != null)
		//		onLogedIn(false);
		//	return;
		//}

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
					//Points points = JsonUtility.FromJson<Points>(result.FunctionResult.ToString());
					PlayerPrefs.SetString("G_Username", username);
					PlayerPrefs.SetString("G_Password", pw);
					playerData = JsonConvert.DeserializeObject<PlayerData>(result.FunctionResult.ToString());
					float points = playerData.GetPoints();
					
					PlayerPrefs.SetFloat("PlayedForTime", points);
					int discountsEarned = 0;
					if (points >= ThreeStarPoints)
					{
						discountsEarned = 3;
						PlayerPrefs.SetInt("DiscountWindowShowed", 3);
					}
					else if (points >= TwoStarPoints)
					{
						discountsEarned = 2;
						PlayerPrefs.SetInt("DiscountWindowShowed", 2);
					}
					else if (points >= OneStarPoints)
					{
						discountsEarned = 1;
						PlayerPrefs.SetInt("DiscountWindowShowed", 1);
					}
					if (discountsEarned > 0 && playerData.GetAmountOfDiscounts() < discountsEarned)
					{
						AddDiscounts(username, pw);
					}

					//float pointsToSync = PlayerPrefs.GetFloat("PointsToSync", 0f);
					//if (pointsToSync > 0)
					//	SendTime(pointsToSync, (b) => 
					//	{
					//		PlayerPrefs.SetFloat("PointsToSync", 0f);
					//	}, true);

					if (onLogedIn != null)
						onLogedIn(true);

					return;
				}
				catch(Exception e)
				{
					if (onLogedIn != null)
						onLogedIn(false);
					return;
				}

			}
			if (onLogedIn != null)
				onLogedIn(false);

		}, error =>
		{
			onLogedIn(false);

		});
	}

	public void Register(string aUsername, string aPassword, string aEmail, Action onSuccess = null, Action<RegisterResponse> onFail = null)
	{
		//if (hasInternet == false)
		//{
		//	if (onFail != null)
		//		onFail(null);
		//	return;
		//}
		if (IsLogedInToPlayfab == false)
		{
			LoginToPlayfab();
			return;
		}
		var request = new ExecuteCloudScriptRequest();
		request.FunctionName = "Register";
		request.FunctionParameter = new Dictionary<string, string> {
			{ "username", aUsername},
			{ "password", aPassword },
			{ "email", aEmail}

		};
		request.GeneratePlayStreamEvent = true;

		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			if (result.FunctionResult != null)
			{
				RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(result.FunctionResult.ToString());
				if (response.Points != -1)
				{
					if (onSuccess != null)
					{
						PlayerPrefs.SetString("G_Username", aUsername);
						PlayerPrefs.SetString("G_Password", aPassword);
						PlayerPrefs.SetFloat("PlayedForTime", response.Points);
						if(onSuccess != null)
							onSuccess();
					}
				}
				else
				{
					if (onFail != null)
						onFail(response);
				}
			}
			else
			{
				if (onFail != null)
					onFail(null);
				//MenuManager.Instance.CloseLoadingWindow();
			}


		}, error =>
		{
			if (onFail != null)
				onFail(null);
		});
	}

	internal string GetLatestDiscountCodeAndPercent()
	{
		string discountCode = string.Empty;
		if (playerData != null && playerData.games != null && playerData.games.CykelKraft != null)
		{
			Dictionary<string, string> discounts = playerData.games.CykelKraft.discounts;
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

	public void PressedSignup()
	{
#if InAppBrowser
		if (InAppBrowser.IsInAppBrowserOpened() == false)
		{
			InAppBrowser.DisplayOptions displayOptions = new InAppBrowser.DisplayOptions();
			displayOptions.displayURLAsPageTitle = false;
			displayOptions.backButtonText = "Back";
			displayOptions.pageTitle = "Gamerce Platform";
			//GamerceSettings.instance.
			InAppBrowser.OpenURL("https://gamerce.pbertilsson.se/user/?wpu_action=register", displayOptions);
		}
#endif

	}

	internal string GetLatestDiscountCode()
	{
		string discountCode = string.Empty;
		if (playerData != null && playerData.games != null && playerData.games.CykelKraft != null)
		{
			Dictionary<string, string> discounts = playerData.games.CykelKraft.discounts;
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
			PlayerPrefs.DeleteKey("HaveAcceptedGamerceNL");
			PlayerPrefs.DeleteKey("HaveAcceptedBubbleroomNL");
			PlayerPrefs.DeleteKey("DiscountWindowShowed");
			PlayerPrefs.DeleteKey("PlayedForTime");
			PlayerPrefs.DeleteKey("TimesShowedNewsLetter");
			PlayerPrefs.DeleteKey("NewsLetter");
			PlayerPrefs.DeleteKey("HaveSeenReward");

			if (onSuccess != null)
				onSuccess();
			GameAnalyticsManager.instance.LoggedOutSuccess();
		},
		error =>
		{
			GameAnalyticsManager.instance.LoggedOutFail();
		});

	}

	public void AcceptNewsLetter(bool acceptGamerce, bool acceptBubbleroom, Action onSuccess, Action onFail)
	{
		//if (hasInternet == false)
		//{
		//	if (onFail != null)
		//		onFail();
		//	return;
		//}
		if (IsLogedInToPlayfab == false)
		{
			LoginToPlayfab();
			return;
		}

		var request = new ExecuteCloudScriptRequest();
		request.FunctionName = "AcceptNewsLetter";
		request.GeneratePlayStreamEvent = true;
		Dictionary<string, bool> parameters = new Dictionary<string, bool>();
		if(acceptGamerce == true)
		{
			parameters.Add("Gamerce", true);
		}
		if(acceptBubbleroom == true)
		{
			parameters.Add("Bubbleroom", true);
		}
		request.FunctionParameter = parameters;

		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			if (onSuccess != null)
				onSuccess();
		},
		error =>
		{
			if (onFail != null)
				onFail();
		});
	}

	public string GetDiscountHeader()
	{
		string discountCode = "";
		if (playerData != null && playerData.games != null && playerData.games.CykelKraft != null)
		{	
			Dictionary<string, string> discounts = playerData.games.CykelKraft.discounts;
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
		if (playerData != null && playerData.games != null && playerData.games.CykelKraft != null)
		{
			Dictionary<string, string> discounts = playerData.games.CykelKraft.discounts;
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
		//request.FunctionParameter = TimePlayed/60f;
		request.GeneratePlayStreamEvent = true;
		// "{\"games\":{\"Rosemunde\":{\"points\":\"60\",\"discounts\":{\"xx03\":\"15%\",\"x1x1\":\"35%\",\"x01x\":\"55%\"}},\"VeraVega\":{\"discounts\":{\"x1x1\":\"15%\"}}}}\n\n"

		PlayFabClientAPI.ExecuteCloudScript(request, result =>
		{
			if (result.FunctionResult != null)
			{
				//float time = ReturnValue.CreateFromJSON(result.FunctionResult.ToString()).totalTime;
				float points = PlayerPrefs.GetFloat("PlayedForTime", 0f);
				
				try
				{
					playerData = JsonConvert.DeserializeObject<PlayerData>(result.FunctionResult.ToString());
					points = playerData.GetPoints();
				}
				catch (Exception e)
				{
					points = ReturnValue.CreateFromJSON(result.FunctionResult.ToString()).totalTime;
				}

				//if (GamerceInit.instance.IsLoggedIn())
				//{
				//	GamerceInit.instance.playerData = JsonConvert.DeserializeObject<PlayerData>(result.FunctionResult.ToString());
				//	points = GamerceInit.instance.playerData.GetPoints();
				//}
				//else
				//{
				//	points = ReturnValue.CreateFromJSON(result.FunctionResult.ToString()).totalTime;
				//}

				PlayerPrefs.SetFloat("PlayedForTime", points);
				Debug.Log("Data Sent Result:" + points);
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
	public GGame games;

	public float GetPoints()
	{
		if(games != null)
			return games.GetPoints();
		return 0f;
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
	public CGameData CykelKraft;

	public float GetPoints()
	{
		if(CykelKraft != null)
			return CykelKraft.GetPoints();
		return 0f;
	}

	public int GetAmountOfDiscounts()
	{
		if (CykelKraft == null)
			return 0;
		return CykelKraft.GetAmountOfDiscounts();
	}
}

[Serializable]
public class CGameData
{
	public float points;
	public Dictionary<string,string> discounts;

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