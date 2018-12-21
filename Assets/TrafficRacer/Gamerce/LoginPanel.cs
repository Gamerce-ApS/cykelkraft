using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour {


	[Header("Panels")]
	public GameObject loggedIn;
	public GameObject loggedOut;
	public GameObject register;
	public GameObject newsLetter;
	public GameObject newsLetterAcceptedPopup;

	public Material greyScaleMaterial;

	[Header("Login")]
	public Text password;
	public Text username;
	public GameObject errorText;



	[Header("LoggedIn")]
	public Text t_logedInText;
	public Gamerce_StarHandler starHandler;
	public Text pointsText;


	[Header("Register")]
	public Text usernameInput;
	public Text passwordInput;
	public Text emailInput;
	public Text usernameError;
	public Text passwordError;
	public Text emailError;
	public Toggle TermsAndCondition;
	public bool acceptTermsAndConditions;
	public Text acceptTermsError;
	bool isRegistering = false;


	[Header("Newsletter")]
	public GameObject gamerceToggle;
	public bool gamerceToggleBool;
	public GameObject ckToggle;
	public bool ckBool;
	public Image acceptButtonImage;
	public Button acceptButton;

	[Header("Loading")]
	public LoadingText loadingText;

	[Header("Info bubble")]
	public RectTransform loginInfoBubble;
	public RectTransform gamerceBubble;
	public RectTransform rosemundeBubble;
	public RectTransform registerInfoBubble;
	public RectTransform newsletterInfoBubble;

	private void OnEnable()
	{
		OpenGamercePanel();
	}

	public void OpenGamercePanel()
	{
		if (PlayerPrefs.HasKey("G_Username") && PlayerPrefs.HasKey("G_Password"))
		{
			OpenLoggedInPanel();
		}
		else
		{
			OpenLoginPanel();
		}
		newsLetterAcceptedPopup.SetActive(false);
	}

	public void OpenLoginPanel()
	{
		loggedIn.SetActive(false);
		loggedOut.SetActive(true);
		register.SetActive(false);
		username.transform.parent.GetComponent<InputField>().text = "";
		password.transform.parent.GetComponent<InputField>().text = "";
		errorText.SetActive(false);
		newsLetter.SetActive(false);

	}

	public void OpenLoggedInPanel()
	{
		loggedIn.SetActive(true);
		loggedOut.SetActive(false);
		register.SetActive(false);
		t_logedInText.text = "" + PlayerPrefs.GetString("G_Username");
		errorText.SetActive(false);
		newsLetter.SetActive(false);
		UpdateStars();
	}

	public void Login()
	{
		GameAnalyticsManager.instance.ClickedLoggedIn();
		string usernameText = username.transform.parent.GetComponent<InputField>().text;
		string passwordText = password.transform.parent.GetComponent<InputField>().text;
		if(string.IsNullOrEmpty(usernameText) == true || string.IsNullOrEmpty(passwordText))
		{
			GameAnalyticsManager.instance.LogginFailed();
			errorText.SetActive(true);
		}
		else
		{
			ShowLoading("Logging in");
			//MenuManager.Instance.OpenLoadingWindow("Loggingin");
			GamerceInit.instance.PressedLogin(usernameText, passwordText, OnLoggedIn);
		}

	}

	void OnLoggedIn(bool aSuccess)
	{
		if (aSuccess)
		{
			OpenLoggedInPanel();
			//MenuManager.Instance.CloseLoadingWindow();
			GameAnalyticsManager.instance.LogginSuccessful();
			CloseLoading();
		}
		else
		{
			errorText.SetActive(true);
			//MenuManager.Instance.CloseLoadingWindow();
			CloseLoading();
			GameAnalyticsManager.instance.LogginFailed();
		}
	}

	public void GoToRegisterPanel()
	{
		loggedIn.SetActive(false);
		loggedOut.SetActive(false);
		register.SetActive(true);
		newsLetter.SetActive(false);
		EmptyRegisterFields();
		TermsAndCondition.isOn = false;
		acceptTermsAndConditions = TermsAndCondition.isOn;
		GameAnalyticsManager.instance.GoToRegisterFromLogin();
	}

	void EmptyRegisterFields()
	{
		usernameInput.transform.parent.GetComponent<InputField>().text = "";
		passwordInput.transform.parent.GetComponent<InputField>().text = "";
		emailInput.transform.parent.GetComponent<InputField>().text = "";

		usernameError.text = "";
		passwordError.text = "";
		emailError.text = "";
		acceptTermsError.text = "";
	}

	public void LogOut()
	{
		GameAnalyticsManager.instance.PressedLoggedOut();
		ShowLoading("Loading");
		//MenuManager.Instance.OpenLoadingWindow("Loading");
		GamerceInit.instance.PressedLogout(() => 
		{
			OpenLoginPanel();
			CloseLoading();
			//MenuManager.Instance.CloseLoadingWindow();
		});

	}

	void UpdateStars()
	{
		float timePlayed = 0;
		if (PlayerPrefs.HasKey("PlayedForTime"))
			timePlayed = PlayerPrefs.GetFloat("PlayedForTime");

		//pointsText.GetComponent<Text>().text = "Your Points: " + (int)timePlayed + "/" + GamerceInit.instance.MaxGamercePoints.ToString();
		if (timePlayed >= GamerceInit.instance.ThreeStarPoints)
			starHandler.SetActiveStars(3);
		else if (timePlayed >= GamerceInit.instance.TwoStarPoints)
			starHandler.SetActiveStars(2);
		else if (timePlayed >= GamerceInit.instance.OneStarPoints)
			starHandler.SetActiveStars(1);
		else
			starHandler.SetActiveStars(0);
	}

	public void Register()
	{
		string username = usernameInput.transform.parent.GetComponent<InputField>().text;
		string password = passwordInput.transform.parent.GetComponent<InputField>().text;
		string email = emailInput.transform.parent.GetComponent<InputField>().text;
		GameAnalyticsManager.instance.ClickedRegister();
		if (string.IsNullOrEmpty(username) == false && string.IsNullOrEmpty(password) == false && string.IsNullOrEmpty(email) == false && acceptTermsAndConditions == true)
		{
			//MenuManager.Instance.OpenLoadingWindow("Registering");
			ShowLoading("Registering");
			GamerceInit.instance.Register(username, password, email, OnRegisterSuccess, OnRegisterFail);
		}
		else
		{
			GameAnalyticsManager.instance.RegisterFailed();
			if(string.IsNullOrEmpty(username))
			{
				usernameError.text = "Please eneter a username";
			}
			if(string.IsNullOrEmpty(password))
			{
				passwordError.text = "Please eneter a password";
			}
			if (string.IsNullOrEmpty(email))
			{
				emailError.text = "Please eneter an email";
			}
			if (acceptTermsAndConditions == false)
			{
				acceptTermsError.text = "You have to accept our terms and conditions";
			}
		}

	}

	void OnRegisterSuccess(bool aSuccess)
	{
		GameAnalyticsManager.instance.RegisteredSuccessful();
		isRegistering = true;
		//MenuManager.Instance.CloseLoadingWindow();
		CloseLoading();
		string username = PlayerPrefs.GetString("G_Username", "");
		string password = PlayerPrefs.GetString("G_Password", "");
		GamerceInit.instance.PressedLogin(username, password, null);
		OpenNewsLetter();
		//OpenLoggedInPanel();
	}

	void OnRegisterFail(RegisterResponse aErrorMessage)
	{
		CloseLoading();
		//MenuManager.Instance.CloseLoadingWindow();
		GameAnalyticsManager.instance.RegisterFailed();
		if (aErrorMessage == null)
			return;

		if (string.IsNullOrEmpty(aErrorMessage.wp_usersname) == false)
			usernameError.text = aErrorMessage.wp_usersname;
		else if (string.IsNullOrEmpty(aErrorMessage.existing_user_login) == false)
			usernameError.text = aErrorMessage.existing_user_login;
		else if (string.IsNullOrEmpty(aErrorMessage.user_login_too_long) == false)
			usernameError.text = aErrorMessage.user_login_too_long;
		else if (string.IsNullOrEmpty(aErrorMessage.invalid_username) == false)
			usernameError.text = aErrorMessage.invalid_username;
		else
			usernameError.text = "";

		passwordError.text = aErrorMessage.wpu_password;

		if (string.IsNullOrEmpty(aErrorMessage.wpu_email) == false)
			emailError.text = aErrorMessage.wpu_email;
		else if (string.IsNullOrEmpty(aErrorMessage.existing_user_email) == false)
			emailError.text = aErrorMessage.existing_user_email;
		else
			emailError.text = "";


	}

	public void ToggleTermsAndConditions(bool aValue)
	{
		acceptTermsAndConditions = aValue;
	}

	public void UsePoints()
	{
		if (InAppBrowser.IsInAppBrowserOpened() == false)
		{
			GameAnalyticsManager.instance.ClickedUsePoints();
			InAppBrowser.DisplayOptions displayOptions = new InAppBrowser.DisplayOptions
			{
				displayURLAsPageTitle = false,
				backButtonText = "Back",
				pageTitle = "Gamerce Platform"
			};
			string url = "https://gamerce.net/gamerce_gotoprofile/?username=" + PlayerPrefs.GetString("G_Username") + "&password=" + PlayerPrefs.GetString("G_Password");
			InAppBrowser.OpenURL(url, displayOptions);

		}
	}


	public void AcceptNewsletter()
	{
		ShowLoading("Loading");
		//MenuManager.Instance.OpenLoadingWindow("Loading");
		GamerceInit.instance.AcceptNewsLetter(gamerceToggleBool, ckBool, () =>
		{
			CloseLoading();
			//MenuManager.Instance.CloseLoadingWindow();
			newsLetter.SetActive(false);
			newsLetterAcceptedPopup.SetActive(true);
			string whereIsShowing = "";
			if (isRegistering == true)
				whereIsShowing = "Register";
			else
			{
				whereIsShowing = "Level" + PlayerPrefs.GetInt("OpenLevel", 0).ToString();
			}
			if (gamerceToggleBool == true)
			{
				if (PlayerPrefs.GetInt("HaveAcceptedGamerceNL", 0) <= 0)
				{
					
					GameAnalyticsManager.instance.SubscribsNewsletter("Gamerce", whereIsShowing);
				}
				PlayerPrefs.SetInt("HaveAcceptedGamerceNL", 1);
			}
			if(ckBool == true)
			{
				if (PlayerPrefs.GetInt("HaveAcceptedCKNL", 0) <= 0)
					GameAnalyticsManager.instance.SubscribsNewsletter("CK", whereIsShowing);
				PlayerPrefs.SetInt("HaveAcceptedCKNL", 1);

			}
		},
		() => 
		{
			CloseLoading();
			//MenuManager.Instance.CloseLoadingWindow();
		});
	}

	public void CloseNewsLetterPopup()
	{
		string whereIsShowing = "";
		if (isRegistering == true)
			whereIsShowing = "Register";
		else
		{
			whereIsShowing = "Level"+PlayerPrefs.GetInt("OpenLevel", 0).ToString();
		}
		GameAnalyticsManager.instance.ClosesNewsletterPopup(whereIsShowing);
	}

	public void OpenNewsLetter()
	{
		newsLetter.SetActive(true);
		register.SetActive(false);
		loggedIn.SetActive(false);
		loggedOut.SetActive(false);

		gamerceToggle.GetComponent<Toggle>().isOn = false;

		int haveAcceptedGamerceNL = PlayerPrefs.GetInt("HaveAcceptedGamerceNL", 0);
		if (haveAcceptedGamerceNL == 0)
			gamerceToggle.SetActive(true);
		else
			gamerceToggle.SetActive(false);
		gamerceToggleBool = false;

		ckToggle.GetComponent<Toggle>().isOn = false;

		int haveAcceptedCKNL = PlayerPrefs.GetInt("HaveAcceptedCKNL", 0);
		if (haveAcceptedCKNL == 0)
			ckToggle.SetActive(true);
		else
			ckToggle.SetActive(false);
		ckBool = false;
		UpdateAcceptButton();
		//acceptButton.SetActive(false);
	}

	public void OnToggleGamerce(bool aValue)
	{
		gamerceToggleBool = aValue;
		UpdateAcceptButton();
	}

	public void OnToggleRosemunde(bool aValue)
	{
		ckBool = aValue;
		UpdateAcceptButton();
	}

	void UpdateAcceptButton()
	{
		if(gamerceToggleBool == false && ckBool == false)
		{
			acceptButton.interactable = false;
			acceptButtonImage.material = greyScaleMaterial;
		}
		else
		{
			acceptButton.interactable = true;
			acceptButtonImage.material = null;
		}
	}

	public void ClosePopup()
	{
		newsLetterAcceptedPopup.SetActive(false);
		if (isRegistering)
		{
			OpenGamerceDiscountWindowWeb();
			OpenLoggedInPanel();
		}
		else
			gameObject.SetActive(false);
	}

	public void OpenGamerceDiscountWindowWeb()
	{
		string latestDiscountAmount = GamerceInit.instance.GetLatestDiscountPercent();
		if (string.IsNullOrEmpty(latestDiscountAmount))
			return;
		string url = "http://gamerce.net/gameunlocks/rosemunde/unlocked_discount.php?pro="+ latestDiscountAmount;

		InAppBrowser.DisplayOptions displayOptions = new InAppBrowser.DisplayOptions();
		displayOptions.displayURLAsPageTitle = false;
		displayOptions.backButtonText = "Back";
		displayOptions.pageTitle = "Gamerce";
		InAppBrowser.OpenURL(url, displayOptions);
	}

	public void GoToRosemunde()
	{
		GameAnalyticsManager.instance.ClickedGoShop();
		string discountCode = GamerceInit.instance.GetLatestDiscountCodeAndPercent();
		if(string.IsNullOrEmpty(discountCode))
		{
			discountCode = "Play to unlock discount codes!";
		}
		string latestDiscountAmount = GamerceInit.instance.GetLatestDiscountPercent();
		if (string.IsNullOrEmpty (latestDiscountAmount))
			latestDiscountAmount = "0";
		if (latestDiscountAmount=="5")
			latestDiscountAmount = "1";
		if (latestDiscountAmount=="10")
			latestDiscountAmount = "2";
		if (latestDiscountAmount=="15")
			latestDiscountAmount = "3";
		if (latestDiscountAmount=="20")
			latestDiscountAmount = "3";

		string onlyCode = GamerceInit.instance.GetLatestDiscountCode();
		string url = "https://gamerce.net/gameunlocks/rosemunde/unlock_overview.php?pro="+latestDiscountAmount+"&code="+onlyCode;
#if InAppBrowser
		InAppBrowser.DisplayOptions displayOptions = new InAppBrowser.DisplayOptions();
		displayOptions.displayURLAsPageTitle = false;
		displayOptions.backButtonText = "Back";
		displayOptions.pageTitle = discountCode;
		InAppBrowser.OpenURL(url, displayOptions);
#endif
	}

	public void ClickedGamerceInfo()
	{
		gamerceBubble.gameObject.SetActive(true);
		gamerceBubble.parent.gameObject.SetActive(true);

	}

	public void ClickedRosemundeInfo()
	{
		rosemundeBubble.gameObject.SetActive(true);
		rosemundeBubble.parent.gameObject.SetActive(true);
	}

	public void ClickedLoginInfoButton()
	{
		loginInfoBubble.gameObject.SetActive(true);
		loginInfoBubble.parent.gameObject.SetActive(true);
	}

	public void ClickedRegisterInfoButton()
	{
		registerInfoBubble.gameObject.SetActive(true);
		registerInfoBubble.parent.gameObject.SetActive(true);
	}

	public void ClickedNewsletterInfoButton()
	{
		newsletterInfoBubble.gameObject.SetActive(true);
		newsletterInfoBubble.parent.gameObject.SetActive(true);
	}

	public void DisableInfoBubble()
	{
		loginInfoBubble.gameObject.SetActive(false);
		loginInfoBubble.parent.gameObject.SetActive(false);

		registerInfoBubble.gameObject.SetActive(false);
		registerInfoBubble.parent.gameObject.SetActive(false);

		newsletterInfoBubble.gameObject.SetActive(false);
		newsletterInfoBubble.parent.gameObject.SetActive(false);

		rosemundeBubble.gameObject.SetActive(false);
		gamerceBubble.gameObject.SetActive(false);
		gamerceBubble.parent.gameObject.SetActive(false);
	}

	public void ShowLoading(string aText)
	{
		loadingText.transform.parent.gameObject.SetActive(true);
		loadingText.SetText(aText);
	}

	public void CloseLoading()
	{
		loadingText.transform.parent.gameObject.SetActive(false);
	}
}