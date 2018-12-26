/***********************************************************************************************************
 * Produced by Madfireon:               https://www.madfireongames.com/									   *
 * Facebook:                            https://www.facebook.com/madfireon/								   *
 * Contact us:                          https://www.madfireongames.com/contact							   *
 * Madfireon Unity Asset Store catalog: http://bit.ly/sellmyapp_store									   *
 * Developed by Swapnil Rane:           https://in.linkedin.com/in/swapnilrane24                           *
 ***********************************************************************************************************/

/***********************************************************************************************************
* NOTE:- This script controls game menu                                                                    *
***********************************************************************************************************/

#define InAppBrowser

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GuiManager : MonoBehaviour {

	public static GuiManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<GuiManager>();
				instance.Init();
				//DontDestroyOnLoad(instance.gameObject);
			}
			return instance;
		}

	}

	private static GuiManager instance;


	#region Serialized Variables
	[SerializeField] [Header("Game Menu Elements")]
    private GameMenu gameMenu;                                                                  //reference to game menu UI elements
    [SerializeField][Header("-----------------------")][Header("Main Menu Elements")]
    private MainMenu mainMenu;                                                                  //reference to main menu UI elements
    [SerializeField] [Header("-----------------------")] [Header("GameOver Menu Elements")]
    private GameOverMenu gameOverMenu;                                                          //reference to game over menu UI elements
    [SerializeField] [Header("-----------------------")] [Header("Tutorial Menu Elements")]
    private Tutorial tutorialElements;                                                          //reference to tutorial elements
    [SerializeField] [Header("-----------------------")] [Header("Facebook Menu Elements")]
    public FacebookPanel facebookElements;                                                      //reference to facebook elements
	[SerializeField] [Header("-----------------------")] [Header("Speed Increaser")]
    private SpeedIncreaser speedIncreaser;                                                      //reference to speed increasing values

    [SerializeField] [Header("-----------------------")] private GameObject mainMenuObj;        //ref to mainMenu panel
                                                                                                //ref to other Object
    [SerializeField] private GameObject gameMenuObj, gameoverMenu, reviveMenu, carShopPanel, turboEffect, powerupShop, gdprAdmobPanel, gamercePanel;
    [SerializeField] private ScrollTexture road;                                                //ref to ScrollTexture component on road gameobject
    [SerializeField] private float currentSpeed;                                                //speed of game
    [SerializeField] private Button noAdsbtn;                                                   //ref to remove ads button
	#endregion

	bool shouldOpenGamerceLogin;

	/*---------------------------------------------------Private Variables------------------------------------------------------------*/

	#region Private Variables
	private float currentFuel, maxEnemyCarGap = 4;                                              //floats to store values
    private float currentMagnetTime, currentTurboTime, currentDoubleCoinTime;                   //floats to store values
    private float countDown = 4, distanceMultiplier, giftBarTime;                               //floats to store values
    private bool fuelSpawned = false, startCountDown, revived = false, giftBarActive = false;   //bools
    private bool magnetActive, turboActive, doubleCoinActive, shieldActive;                     //bools
    private bool magnetSpawned, turboSpawned, doubleCoinSpawned, shieldSpawned;                 //bools
    private GameObject coinObject, coinUIObj;                                                   //gameobject variable to store reference to object 
    private int coinIncreaser = 1;                                                              //coin multipliers
    private int currentTipIndex = 0;
    #endregion

    /*----------------------------------------------------Getter And Setter----------------------------------------------------------*/

    #region Getter And Setter
    public Button NoAdsBtn          { get { return noAdsbtn;       } }
    public GameObject GDPRpanel     { get { return gdprAdmobPanel; } }
    public float DistanceMultiplier { get { return distanceMultiplier; } set { distanceMultiplier = value; } }
    public float CurrentFuel        { get { return currentFuel;    } }
    public float CurrentSpeed       { get { return currentSpeed; }       set { currentSpeed       = value; } }
    public float MaxEnemyCarGap     { get { return maxEnemyCarGap; }     set { maxEnemyCarGap     = value; } }
    public bool  FuelSpawned        { get { return fuelSpawned; }        set { fuelSpawned        = value; } }

    public bool MagnetActive        { get { return magnetActive;     } }
    public bool TurboActive         { get { return turboActive;      } }
    public bool DoubleCoinActive    { get { return doubleCoinActive; } }
    public bool ShieldActive        { get { return shieldActive;     }   set { shieldActive       = value; } }

    public bool MagnetSpawned       { get { return magnetSpawned; }      set { magnetSpawned      = value; } }
    public bool TurboSpawned        { get { return turboSpawned; }       set { turboSpawned       = value; } }
    public bool DoubleCoinSpawned   { get { return doubleCoinSpawned; }  set { doubleCoinSpawned  = value; } }
    public bool ShieldSpawned       { get { return shieldSpawned; }      set { shieldSpawned      = value; } }
    #endregion

    [HideInInspector]
    public managerVars vars;

	static int count = 0;

    void Awake ()
    {
		count++;
		gameObject.name += count.ToString();
		if (Instance == this)
		{
			return;
		}
		else if(Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		if (Instance == null)
		{
            instance = this;
			Init();
		}
	}

	private void OnDestroy()
	{
		Debug.Log(gameObject.name + " was destroyed!");
	}

	void Init()
	{
		//DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += SceneLoaded;
		vars = Resources.Load<managerVars>("managerVarsContainer");         //loading data from managerVars
	}

	private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		if (shouldOpenGamerceLogin)
		{
			MenuBtns("gamerce");
			shouldOpenGamerceLogin = false;
		}
	}

	private void Start()
    {
        tutorialElements.previousButton.SetActive(false);

        //if GDPRConset is zero and canShowAds is true
        /*if (GameManager.instance.GDPRConset == 0 && GameManager.instance.canShowAds == true)
        {
            gdprAdmobPanel.SetActive(true);                                                             //activate gdprAdmobPanel
        }

        if (GameManager.instance.canShowAds == false)                                                   //if canShowAds is false
        {
            noAdsbtn.interactable = false;                                                              //make noAdsbtn non interactable

#if AdmobDef
            AdsManager.instance.HideBannerAds();                                                        //hide banner ads
#endif
        }*/

        //sound button
        if (GameManager.Instance.isMusicOn == true)                                                     //if mousicOn is true
        {
            AudioListener.volume = 1;                                                                   //set volume to 1
            mainMenu.soundBtnImg.sprite = mainMenu.soundOff;                                            //set the soundOff icon
        }
        else
        {
            AudioListener.volume = 0;                                                                   //else set volume to 0
            mainMenu.soundBtnImg.sprite = mainMenu.soundOn;                                             //set the soundOn icon
        }

        GameManager.Instance.gameStarted = false;                                                       //set gameStarted to false
        GameManager.Instance.gameOver = false;                                                          //set gameOver to false
        GameManager.Instance.currentCoinsEarned = 0;                                                    //set currentCoinsEarned to 0
        gameMenu.coinText.text = "" + GameManager.Instance.currentCoinsEarned;                          //set the coinText to currentCoinsEarned
        speedIncreaser.milestone = speedIncreaser.milestoneIncreaser;                                   //set milestone to milestoneIncreaser
        MoveUI(mainMenuObj.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0f, Ease.OutFlash);  //slide in mainMenu
        SoundManager.instance.PlayFX("PanelSlide");                                                     //play PanelSlide sound
        GameManager.Instance.playerCar = GameObject.FindGameObjectWithTag("Player");                    //get reference to player car
        GameManager.Instance.currentDistance = 0;                                                       //set currentDistance to 0
        gameMenu.distanceText.text = "" + GameManager.Instance.currentDistance;                         //set the distance text
        road.ScrollSpeed = currentSpeed;                                                                //set road scroll speed
        mainMenu.coinText.text = "" + GameManager.Instance.coinAmount;                                  //set cointext
        PlayerController.instance.SetCarSprite();                                                       //set player car sprite

        if (GameManager.Instance.retry == true)                                                         //if retry is true
        {
            GameManager.Instance.retry = false;                                                         //set retry to false
            PlayBtn();                                                                                  //call playBtn method
        }
    }

	public void ResetGameText()
	{
		GameManager.Instance.currentDistance = 0;
		gameMenu.distanceText.text = "" + GameManager.Instance.currentDistance;
		GameManager.Instance.currentCoinsEarned = 0;
		gameMenu.coinText.text = "" + GameManager.Instance.currentCoinsEarned;
		GameManager.Instance.gameStarted = false;
		countDown = 4;
	}

	public void RemoveObjects()
	{
		Spawner.instance.RemoveObjects();
		GameManager.Instance.gameOver = false;
	}

	public void SetPlayerVisible()
	{
		GameManager.Instance.playerCar.SetActive(true);
		Vector3 playerPos = GameManager.Instance.playerCar.transform.position;
		GameManager.Instance.playerCar.transform.position = new Vector3(0, playerPos.y, playerPos.z);
	}

	void Update ()
    {

#if UNITY_ANDROID
        if (Input.GetKeyDown(KeyCode.Escape))                                                           //if escape key is press
            Application.Quit();                                                                         //quit game
#endif
        //if countDown is more then 0 , startCountDown is true and gameStarted is false
        if (countDown > 0 && startCountDown == true && GameManager.Instance.gameStarted == false)
        {
            countDown -= Time.deltaTime;                                                                //reduce countDown by Time.deltaTime
            if (countDown <= 4 && countDown > 3)                                                        //if countdown is 4
            {
                if (gameMenu.countDownText.text != "3")                                                 //check if text is not 3
                    SoundManager.instance.PlayFX("CD3");                                                //play sound

                gameMenu.countDownText.text = "3";                                                      //set text to 3
            }   
            else if (countDown <= 3 && countDown > 2)                                                   //if countdown is 3
            {
                if (gameMenu.countDownText.text != "2")                                                 //check if text is not 2
                    SoundManager.instance.PlayFX("CD2");                                                //play sound

                gameMenu.countDownText.text = "2";                                                      //set text to 2
            }
            else if (countDown <= 2 && countDown > 1)                                                   //if countdown is 2
            {
                if (gameMenu.countDownText.text != "1")                                                 //check if text is not 1
                    SoundManager.instance.PlayFX("CD1");                                                //play sound

                gameMenu.countDownText.text = "1";                                                      //set text to 1
            }
            else if (countDown <= 1)
            {
                if (gameMenu.countDownText.text != "GO!!")
                    SoundManager.instance.PlayFX("CDGO");

                gameMenu.countDownText.text = "GO!!";
            }

            if (countDown <= 0)                                                                         //if countdown is less than or equal to 0
            {   
                SoundManager.instance.PlayNarrationFX("GO");                                            //play sound
                startCountDown = false;                                                                 //set startCountDown to false
                gameMenu.countDownText.gameObject.SetActive(false);                                     //deactivate countDownText 
                GameManager.Instance.gameStarted = true;                                                //gameStarted is true
                Spawner.instance.SpawnObjects();                                                        //spawn object (enemy car or pickup)
            }
        }

        if (GameManager.Instance.gameOver == true && giftBarActive == true)                             //if game is over and gift bar is active
        {
            GiftBar();                                                                                  //call giftBar method
        }

        if (GameManager.Instance.gameStarted == true && GameManager.Instance.gameOver == false)         //if gameOver is false and gameStarted is true
        {
            GameManager.Instance.currentDistance += Time.deltaTime * distanceMultiplier;                //increase distance
            gameMenu.distanceText.text = "" + Mathf.RoundToInt(GameManager.Instance.currentDistance);   //set distance text

            if (currentFuel > 0)                                                                        //if currentFuel is moe than zero
            {
                currentFuel -= Time.deltaTime;                                                          //reduce it with time
            }

            //if (currentFuel <= 0 && GameManager.Instance.gameOver == false)                             //if currentFuel is less than 0 and gameOver is false
            //{
            //    SoundManager.instance.PlayNarrationFX("GameOver");                                      //play sound
            //    GameManager.Instance.gameOver = true;                                                   //gameOver is set to true
            //    GameOverMethod();                                                                       //call GameOverMethod
            //}

            gameMenu.fuelSlider.value = currentFuel / GameManager.Instance.fuel;                        //set fuelSlider value
            IncreaseSpeed();                                                                            

            if (magnetActive) MagnetBar();                                                              //if magnet is active call MagnetBar method
            if (turboActive)    TurboBar();                                                             //if turbo is active call TurboBar method
            if (doubleCoinActive) DoubleCoinBar();                                                      //if doubleCoin is active call DoubleCoinBar method
        }
	}

    /*-------------------------------------------------- Button Methods--------------------------------------------------------------*/

    #region Button Methods
    public void PlayBtn()                                                                               //called by Play Button
    {
        SoundManager.instance.PlayFX("BtnClick");                                                       //play the sound
        MoveUI(mainMenuObj.GetComponent<RectTransform>(), new Vector2(-2048, 0), 0.5f, 0f, Ease.OutFlash);   //slide out menuPanel
        MoveUI(gameMenuObj.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0f, Ease.OutFlash);      //slide in gamemenu panel
        SoundManager.instance.PlayGameMusic();                                                          //play game music

        if (GameManager.Instance.tutorialShowned == false)
            tutorialElements.tutorialPanel.SetActive(true);
        else
        {
            PlayMethod();
        }
    }

    void PlayMethod()
    {
		gameMenu.countDownText.gameObject.SetActive(true);                                              //activate countDownText
		currentFuel = GameManager.Instance.fuel;     
        startCountDown = true;                                                                          //set startCountDown to true
		GamerceInit.instance.StartTicker();
	}

    public void TipButton(string value)
    {
        if (value == "Next")
        {
            if (currentTipIndex < tutorialElements.tips.Length - 1)
            {
                currentTipIndex++;
                for (int i = 0; i < tutorialElements.tips.Length; i++)
                {
                    tutorialElements.previousButton.SetActive(true);
                    tutorialElements.tipImage.sprite = tutorialElements.tips[i];
                }
                tutorialElements.tipImage.sprite = tutorialElements.tips[currentTipIndex];

                if (currentTipIndex == tutorialElements.tips.Length - 1)
                {
                    tutorialElements.nextbutton.SetActive(false);
                    tutorialElements.closeBtn.SetActive(true);
                }

                if (currentTipIndex < tutorialElements.tips.Length - 1)
                {
                    tutorialElements.nextbutton.SetActive(true);
                    tutorialElements.closeBtn.SetActive(false);
                }
            }
        }

        if (value == "Back")
        {
            if (currentTipIndex > 0)
            {
                currentTipIndex--;
                for (int i = 0; i < tutorialElements.tips.Length; i++)
                {
                    tutorialElements.closeBtn.SetActive(false);
                    tutorialElements.nextbutton.SetActive(true);
                    tutorialElements.tipImage.sprite = tutorialElements.tips[i];
                }
                tutorialElements.tipImage.sprite = tutorialElements.tips[currentTipIndex];
            }

            if (currentTipIndex <= 0)
            {
                tutorialElements.previousButton.SetActive(false);
            }

            if (currentTipIndex > 0)
                tutorialElements.previousButton.SetActive(true);
        }

        if (value == "Close")
        {
            tutorialElements.tutorialPanel.SetActive(false);
            GameManager.Instance.tutorialShowned = true;
            GameManager.Instance.Save();
            PlayMethod();
        }
    }




    public void GDPRConsetBtn(int value)                                                                //called by GDPR Button
    {
        GameManager.Instance.GDPRConset = value;                                                        //set GDPRConset to value
        GameManager.Instance.Save();                                                                    //save it
        gdprAdmobPanel.SetActive(false);                                                                //deactivate panel
    }

    public void MenuBtns(string value)                                                                  //buttons functions
    {
        SoundManager.instance.PlayFX("BtnClick");                                                       //play sound
        if (value == "facebook")                                                                        //if value is facebook
        {
            //Application.OpenURL(vars.facebookUrl);                                                      //open facebook page 
            FacebookScript.instance.FacebookLogin();
        }
        else if (value == "sound")                                                                      //if value is sound
        {
            //sound button
            if (GameManager.Instance.isMusicOn == true)                                                 //if isMusicOn is true
            {
                GameManager.Instance.isMusicOn = false;                                                 //set isMusicOn t0 false
                AudioListener.volume = 0;                                                               //set volume to 0
                mainMenu.soundBtnImg.sprite = mainMenu.soundOn;                                         //set the soundBtn sprite to soundOn
            }
            else                                                                                        //else
            {
                GameManager.Instance.isMusicOn = true;                                                  //set isMusicOn to true
                AudioListener.volume = 1;                                                               //set volume to 1
                mainMenu.soundBtnImg.sprite = mainMenu.soundOff;                                        //set the soundBtn sprite to soundOff
            }
            GameManager.Instance.Save();                                                                //save the data
        }
        else if (value == "moregames")                                                                  //if value is moregames
        {   
            Application.OpenURL(vars.moregamesUrl);                                                     //open moregames page 
        }
        else if (value == "rate")                                                                       //if value is rate
        {   
            Application.OpenURL(vars.rateButtonUrl);                                                    //open rate page 
        }
        //else if (value == "revive")                                                                     //if value is revive
        //{
        //    UnityAds.instance.rewardType = RewardType.reborn;                                           //set reward ads type
        //    UnityAds.instance.ShowRewardedAd();                                                         //show reward ads
        //}
        else if (value == "gameover")                                                                   //if value is gameover
        {
            GameOver();                                                                                 //call GameOver method
            MoveUI(reviveMenu.GetComponent<RectTransform>(), new Vector2(0, -2500), 0.5f, 0f, Ease.OutFlash);    //slide out reviveMenu
            MoveUI(gameoverMenu.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0f, Ease.OutFlash);     //slide in gameoverMenu
		}
        else if (value == "replay")                                                                     //if value is replay
        {
            GameManager.Instance.retry = true;                                                          //if retry is true
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);                                 //reload the current scene
			GamerceInit.instance.StartTicker();
        }
        else if (value == "home")                                                                       //if value is home
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);                                 //load the current scene
        }
        else if (value == "share")                                                                      //if value is share
        {
            ShareScreenShot.instance.ShareTextMethod();                                                 //share the text
        }
        else if (value == "openShop")                                                                   //if value is openShop
        {
            CarShop.instance.InitializeCarShop();                                                       //call InitializeCarShop method
            SoundManager.instance.PlayFX("BtnClick");                                                   //play sound
			MoveUI(mainMenuObj.GetComponent<RectTransform>(), new Vector2(-2048, 0), 0.5f, 0f, Ease.OutFlash, false);   //slide out main menu panel
			carShopPanel.SetActive(true);
            MoveUI(carShopPanel.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0f, Ease.OutFlash);     //slide in car shop panel
        }
        else if (value == "closeShop")                                                                  //if value is closeShop
        {
            SoundManager.instance.PlayFX("BtnClick");                                                   //play sound
            MoveUI(carShopPanel.GetComponent<RectTransform>(), new Vector2(2048, 0), 0.5f, 0f, Ease.OutFlash, false);   //slide out car shop panel
			mainMenuObj.SetActive(true);
            MoveUI(mainMenuObj.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0f, Ease.OutFlash);      //slide in main menu panel
        }
        else if (value == "openPWShop")                                                                 //if value is openPWShop
        {
			SoundManager.instance.PlayFX("BtnClick");
            MoveUI(mainMenuObj.GetComponent<RectTransform>(), new Vector2(2048, 0), 0.5f, 0f, Ease.OutFlash, false);    //slide out main menu panel
			powerupShop.SetActive(true);
            MoveUI(powerupShop.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0f, Ease.OutFlash);      //slide in powerupShop panel
        }
        else if (value == "closePWShop")                                                                //if value is closePWShop
        {
            SoundManager.instance.PlayFX("BtnClick");                                                   //play sound
            MoveUI(powerupShop.GetComponent<RectTransform>(), new Vector2(-2048, 0), 0.5f, 0f, Ease.OutFlash, false);   //slide out powerupShop panel
			mainMenuObj.SetActive(true);
            MoveUI(mainMenuObj.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0f, Ease.OutFlash);      //slide in main menu panel
        }
        else if (value == "opengiftPanel")                                                              //if value is opengiftPanel
        {
            SoundManager.instance.PlayFX("BtnClick");                                                   //play sound
            GameManager.Instance.giftPoints = 0;                                                        //set giftPoints to 0
            GameManager.Instance.Save();                                                                //save it
            gameOverMenu.giftPanel.SetActive(true);                                                     //activate giftPanel
        }
        else if (value == "closegiftPanel")                                                             //if value is closegiftPanel
        {
            gameOverMenu.giftbtn.interactable = true;                                                   //set giftbtn interactable to true
            SoundManager.instance.PlayFX("BtnClick");                                                   //play sound
            gameOverMenu.giftPanel.SetActive(false);                                                    //deactivate giftPanel
        }   
        else if (value == "collectGift")                                                                //if value is collectGift
        {
            SoundManager.instance.PlayFX("BtnClick");                                                   //play sound    
            gameOverMenu.collectBtn.interactable = false;                                               //set collectBtn interactable to false

            coinUIObj = ObjectPooling.instance.GetPickUpFXPooledObject("CoinUIFX");                     //get coinUI fx
            coinUIObj.transform.position = gameOverMenu.collectBtn.transform.position;                  //set its transform
            coinUIObj.SetActive(true);                                                                  //activate it
            float r = Random.Range(0.25f, 0.8f);                                                        //get random number
            coinUIObj.transform.DOMove(gameOverMenu.coinImg.transform.position, r).OnComplete(DeactivateUICoin).SetEase(Ease.Linear); //move the coin 
        }
		else if(value == "gamerce")
		{
			MoveUI(mainMenuObj.GetComponent<RectTransform>(), new Vector2(0, 2500), 0.5f, 0f, Ease.OutFlash, false);    //slide out reviveMenu
			gamercePanel.SetActive(true);
			MoveUI(gamercePanel.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0f, Ease.OutFlash);     //slide in gameoverMenu
		}
		else if(value == "closeGamerce")
		{
			MoveUI(gamercePanel.GetComponent<RectTransform>(), new Vector2(0, -2500), 0.5f, 0f, Ease.OutFlash, false);     //slide out gamerce
			mainMenuObj.SetActive(true);
			MoveUI(mainMenuObj.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0f, Ease.OutFlash);    //slide in mainMenu
			//gamercePanel.SetActive(true);
		}
		else if(value == "closeDiscountWindow")
		{
			MoveUI(gameOverMenu.discountWindow.GetComponent<RectTransform>(), new Vector2(0, 2500), 0.5f, 0f, Ease.OutFlash);
		}
    }

    #endregion

    /*--------------------------------------------------Basic Methods----------------------------------------------------------------*/

    #region Basic Methods

    public void ActivateMagnet()
    {
        currentMagnetTime = GameManager.Instance.magnetTime;                //set the currentMagnetTime to GameManager.instance.magnetTime
        gameMenu.magnetUI.SetActive(true);                                                              //activate magnetUI
        magnetActive = true;                                                                            //set magnetActive to true
    }

    void MagnetBar()
    {
        if (currentMagnetTime > 0)                                                                      //if currentMagnetTime is more than 0
        {
            currentMagnetTime -= Time.deltaTime;                                                        //reduce currentMagnetTime by Time.DeltaTime
            gameMenu.magnetImg.fillAmount = currentMagnetTime / GameManager.Instance.magnetTime;        //set fillAmount
        }
            
        if (currentMagnetTime <= 0)                                                                     //if currentMagnetTime is less than 0
        {
            PlayerController.instance.DeactivateEffects("magnet");                                      //deactivate magnet effect
            gameMenu.magnetUI.SetActive(false);                                                         //deactivate magnetUI
            magnetActive = false;                                                                       //set magnetActive to false
        }
    }

    public void ActivateTurbo()
    {
        currentSpeed += 1.5f;                                                                           //increase currentSpeed
        road.ScrollSpeed = currentSpeed;                                                                //set road scrollSpeed
        turboEffect.SetActive(true);                                                                    //activate turboEffect
        currentTurboTime = GameManager.Instance.turboTime;                  //set the currentTurboTime to GameManager.instance.turboTime
        gameMenu.turboUI.SetActive(true);                                                               //activate turboUI
        turboActive = true;                                                                             //set turboActive to true
        distanceMultiplier += GameManager.Instance.turboSpeed;                                          //increase distanceMultiplier
    }

    void TurboBar()
    {
        if (currentTurboTime > 0)                                                                       //if currentTurboTime is more than 0
        {
            currentTurboTime -= Time.deltaTime;                                                         //reduce currentTurboTime by Time.DeltaTime
            gameMenu.turboImg.fillAmount = currentTurboTime / GameManager.Instance.turboTime;           //set fillAmount
        }

        if (currentTurboTime <= 0)                                                                      //if currentTurboTime is less than 0
        {
            currentSpeed -= 1.5f;                                                                       //decrease currentSpeed
            road.ScrollSpeed = currentSpeed;                                                            //set road scrollSpeed
            turboEffect.SetActive(false);                                                               //deactivate turboEffect
            distanceMultiplier = GameManager.Instance.normalSpeed;                                      //set distanceMultiplier
            PlayerController.instance.DeactivateEffects("turbo");                                       //deactivate player turboEffect
            gameMenu.turboUI.SetActive(false);                                                          //deactivate turboUI
            turboActive = false;                                                                        //set turboActive to false
        }
    }

    public void ActivateDoubleCoin()
    {
        coinIncreaser = 2;                                                                              //set coinIncreaser to 2
        currentDoubleCoinTime = GameManager.Instance.doubleCoinTime;       //set the currentDoubleCoinTime to GameManager.instance.doubleCoinTime
        gameMenu.doubleCoinUI.SetActive(true);                                                          //activate doubleCoinUI
        doubleCoinActive = true;                                                                        //set doubleCoinActive to true
    }

    void DoubleCoinBar()
    {
        if (currentDoubleCoinTime > 0)                                                                  //if currentDoubleCoinTime is more than 0
        {
            currentDoubleCoinTime -= Time.deltaTime;                                                    //reduce currentDoubleCoinTime by Time.DeltaTime
            gameMenu.doubleCoinImg.fillAmount = currentDoubleCoinTime / GameManager.Instance.doubleCoinTime;    //set fillAmount
        }

        if (currentDoubleCoinTime <= 0)                                                                 //if currentDoubleCoinTime is less than 0
        {
            coinIncreaser = 1;                                                                          //set coinIncreaser to 1
            gameMenu.doubleCoinUI.SetActive(false);                                                     //deactivate doubleCoinUI
            doubleCoinActive = false;                                                                   //set doubleCoinActive to false
        }
    }

    void IncreaseSpeed()                                                                                //this method increase speed to increase difficulty
    {
        if (GameManager.Instance.currentDistance >= speedIncreaser.milestone)                           //if currentDistance is >= to milestone
        {
            speedIncreaser.milestone += speedIncreaser.milestoneIncreaser;                              //increase milestone by milestoneIncreaser

            if (currentSpeed < speedIncreaser.maxSpeed)                                                 //if currentSpeed is less than maxSpeed
            {
                currentSpeed = currentSpeed * speedIncreaser.speedMultiplier;                           //increase currentSpeed
                road.ScrollSpeed = currentSpeed;                                                        //set road ScrollSpeed
            }

            if (maxEnemyCarGap > 2)                                                                     //if car gap is more than 2
                maxEnemyCarGap -= 0.5f;                                                                 //reduce it by 0.5f
        }
    }
                                                                                                        //called when player pckups coin
    public void IncreaseCoin(int value, GameObject _coinObject)                                         //take 2 variables as input 
    {   
        GameManager.Instance.currentCoinsEarned += coinIncreaser * value;                               //increase the currentCoins Earned
        gameMenu.coinText.text = "" + GameManager.Instance.currentCoinsEarned;                          //set the text
        coinObject = _coinObject;                                                                       //store reference to the coin
        coinObject.transform.DOMove(gameMenu.coinImg.transform.position, 0.5f).OnComplete(DeactivateCoin).SetEase(Ease.Linear); //move the coin 
    }

    public void UpdateTotalCoins()
    {
        mainMenu.coinText.text = "" + GameManager.Instance.coinAmount;                                  //update coin text
    }

    void DeactivateCoin()
    {
        coinObject.SetActive(false);
    }

    void DeactivateUICoin()
    {
        SoundManager.instance.CarFX("Coin");                                                            //play coin sound
        GameManager.Instance.coinAmount += 50;                                                          //add 50 coins
        GameManager.Instance.Save();                                                                    //save
        coinUIObj.SetActive(false);                                                                     //deactivate coinUIObj
        gameOverMenu.coinText.text = "" + GameManager.Instance.coinAmount;
        UpdateTotalCoins();                                                                             //update total coin text
    }

    public void RestoreFuel()                                                                           //call when player pickup fuel
    {   
        currentFuel = GameManager.Instance.fuel;                                                        //reset currentFuel to max fuel value                                              
        fuelSpawned = false;                                                                            //fuelSpawned is set to false
    }

    void GiftBar()
    {
        giftBarTime += Time.deltaTime;                                                                  //increase giftBarTime with Time.deltaTime
        var percent = giftBarTime / 1f;                                                                 //set the percent
        float barFill = GameManager.Instance.giftPoints / 500f;                                         //set barFill ratio

        gameOverMenu.giftBar.fillAmount = Mathf.Lerp(0, barFill, percent);                              //set giftBar fillAmount
    }

    public void GameOverMethod()
    {
        MoveUI(gameMenuObj.GetComponent<RectTransform>(), new Vector2(2048,0), 0.5f, 0f, Ease.OutFlash); //slide out gameMenuObj
        /*if (UnityAds.instance.RewardAdReady() == true)                                                  //we check if reward ad is ready
        {
            if (revived == false)                                                                           //if revive is false
                MoveUI(reviveMenu.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0.25f, Ease.OutFlash);    //slide in reviveMenu
            else if (revived == true)                                                                       //if revive is true
            {
                GameOver();                                                                                 //call GameOver method
                MoveUI(gameoverMenu.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0.25f, Ease.OutFlash);  //slide in gameoverMenu
            }
        }
        else
        {
            GameOver();                                                                                 //call GameOver method
            MoveUI(gameoverMenu.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0.25f, Ease.OutFlash);  //slide in gameoverMenu
        }*/

		GameOver();                                                                                 //call GameOver method
		MoveUI(gameoverMenu.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0.25f, Ease.OutFlash, ()=> 
		{
			ShouldShowDiscountWindow();

			if (GamerceInit.instance.shouldShowDiscountWindow == true)
			{
				string rewardText = "You have unlocked a special reward!";
				if (GamerceInit.instance.IsLoggedIn() == false)
				{
					rewardText += " Login to claim it!";
				}
				MoveUI(gameOverMenu.discountWindow.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0, Ease.OutFlash);
			}
		});  //slide in gameoverMenu

		SoundManager.instance.PlayFX("PanelSlide");                                                     //play slide in sound
    }

	void ShouldShowDiscountWindow()
	{
		//bool isLoggedInToPlayfab = GamerceInit.instance.IsLogedInToPlayfab;
		bool haveInternet = GamerceInit.instance.CheckInternetAvailability();

		int discountWindowShowed = PlayerPrefs.GetInt("DiscountWindowShowed", 0);
		float points = PlayerPrefs.GetFloat("PlayedForTime");
		if (discountWindowShowed == 2 && points >= GamerceInit.instance.ThreeStarPoints && haveInternet == true)
		{
			GamerceInit.instance.shouldShowDiscountWindow = true;
			PlayerPrefs.SetInt("DiscountWindowShowed", 3);
		}
		else if (discountWindowShowed == 1 && points >= GamerceInit.instance.TwoStarPoints)
		{
			GamerceInit.instance.shouldShowDiscountWindow = true;
			PlayerPrefs.SetInt("DiscountWindowShowed", 2);
		}
		else if (discountWindowShowed == 0 && points >= GamerceInit.instance.OneStarPoints)
		{
			PlayerPrefs.SetInt("DiscountWindowShowed", 1);
			GamerceInit.instance.shouldShowDiscountWindow = true;
		}

		if (haveInternet == false /*|| isLoggedInToPlayfab == false*/)
		{
			GamerceInit.instance.shouldShowDiscountWindow = false;
		}
	}

	void GameOver()
    {
		//if (GameManager.instance.canShowAds == true)
		//{
		//    if (GameManager.instance.gamesPlayed >= vars.showInterstitialAfter)
		//    {
		//        GameManager.instance.gamesPlayed = 0;
		//        UnityAds.instance.ShowAd();
		//    }

		//    GameManager.instance.gamesPlayed++;
		//}

		ResetGameText();

		gameOverMenu.coinText.text = "" + GameManager.Instance.coinAmount;                              //set gameOverMenu coinText
        gameOverMenu.coinEarnedText.text = "+" + GameManager.Instance.currentCoinsEarned;               //set coinEarnedText
        gameOverMenu.scoreText.text = "" + Mathf.CeilToInt(GameManager.Instance.currentDistance);       //set scoreText

        if (GameManager.Instance.currentDistance > GameManager.Instance.bestDistance)                   //if currentDistance is more than bestDistance
            GameManager.Instance.bestDistance = Mathf.CeilToInt(GameManager.Instance.currentDistance);  //set bestDistance to currentDistance

        GameManager.Instance.lastDistance = Mathf.CeilToInt(GameManager.Instance.currentDistance);      //set lastDistance to currentDistance

        GameManager.Instance.giftPoints += GameManager.Instance.lastDistance;                           //increase giftPoints

        if (GameManager.Instance.giftPoints > 500)                                                      //if giftPoints is more than 500
        {
            GameManager.Instance.giftPoints = 500;                                                      //set giftPoints to 500
            gameOverMenu.giftbtn.interactable = true;                                                   //make giftbtn interactable
        }   

        GameManager.Instance.coinAmount += GameManager.Instance.currentCoinsEarned;                     //increase coinAmount by currentCoinsEarned
        GameManager.Instance.Save();                                                                    //save the data
        giftBarActive = true;                                                                           //set giftBarActive to true
        gameOverMenu.giftInfoText.text = GameManager.Instance.giftPoints + "/500 To Next Gift";         //set the text
        gameOverMenu.hiScoreText.text = "" + GameManager.Instance.bestDistance;                         //set hiScoreText
		GamerceInit.instance.EndTicker();

	}

	public void Claim()
	{
		bool isLoggedIn = GamerceInit.instance.IsLoggedIn();
		if (isLoggedIn == true)
		{
#if UNITY_ANDROID
			string latestDiscount = GamerceInit.instance.GetLatestDiscountPercent();
			string url = "http://gamerce.net/gameunlocks/rosemunde/unlocked_discount.php?pro=" + latestDiscount;
			Application.OpenURL(url);
			MoveUI(gameOverMenu.discountWindow.GetComponent<RectTransform>(), new Vector2(0, 2500), 0.5f, 0f, Ease.OutFlash);
			MoveUI(gameoverMenu.GetComponent<RectTransform>(), new Vector2(0, 2500), 0.5f, 0f, Ease.OutFlash);
			MoveUI(gamercePanel.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0f, Ease.OutFlash);
#elif UNITY_IOS
			if (InAppBrowser.IsInAppBrowserOpened() == false)
			{
				//GameAnalyticsManager.instance.ClickedClaimLoggedIn(latestDiscount);
				string latestDiscount = GamerceInit.instance.GetLatestDiscountPercent();
				string url = "http://gamerce.net/gameunlocks/rosemunde/unlocked_discount.php?pro=" + latestDiscount;
				InAppBrowser.DisplayOptions displayOptions = new InAppBrowser.DisplayOptions();
				displayOptions.displayURLAsPageTitle = false;
				displayOptions.backButtonText = "Back";
				displayOptions.pageTitle = "Gamerce";
				InAppBrowser.OpenURL(url, displayOptions);

				//MenuManager.Instance.OpenLogin();
			}
#endif

		}
		else
		{
			shouldOpenGamerceLogin = true;
			//SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			//MenuBtns("gamerce");
			RemoveObjects();
			SetPlayerVisible();
			gamercePanel.SetActive(true);
			MoveUI(gameOverMenu.discountWindow.GetComponent<RectTransform>(), new Vector2(0, 2500), 0.5f, 0f, Ease.OutFlash);
			MoveUI(gameoverMenu.GetComponent<RectTransform>(), new Vector2(0, 2500), 0.5f, 0f, Ease.OutFlash);
			MoveUI(gamercePanel.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0f, Ease.OutFlash);
			//GameAnalyticsManager.instance.ClickedClaimLoggedOut(latestDiscount);
			//MenuManager.Instance.OpenLogin();
		}

		//MoveUI(gameOverMenu.discountWindow.GetComponent<RectTransform>(), new Vector2(0, 2500), 0.5f, 0f, Ease.OutFlash);
	}

	public void Revive()
    {
        foreach (GameObject enemyCar in GameObject.FindGameObjectsWithTag("Enemy"))                     //loop through all the nemy car in the scene
        {   
            enemyCar.SetActive(false);                                                                  //and deactivate them
        }

        MoveUI(reviveMenu.GetComponent<RectTransform>(), new Vector2(0, -800), 0.5f, 0f, Ease.OutFlash);    //slideOut reviveMenu
        MoveUI(gameMenuObj.GetComponent<RectTransform>(), new Vector2(0, 0), 0.5f, 0.25f, Ease.OutFlash);   //slide in gameMenuObj
        GameManager.Instance.playerCar.SetActive(true);                                                 //activate player car
        gameMenu.countDownText.gameObject.SetActive(true);                                              //activate countDownText
        currentFuel = GameManager.Instance.fuel;                                                        //set currentFuel to fuel
        gameMenu.fuelSlider.value = currentFuel / GameManager.Instance.fuel;                            //set fuelSlider value
        revived = true;                                                                                 //set revived to true
        GameManager.Instance.gameStarted = false;                                                       //gameStarted to false
        countDown = 4;                                                                                  //countDown to 4
        startCountDown = true;                                                                          //set startCountDown to true
        GameManager.Instance.gameOver = false;                                                          //gameOver to false
    }

    public void PickUpPop(string value)
    {
        gameMenu.pickUpPopText.text = value;                                                            //set the text
        MoveUI(gameMenu.pickUpPopUI.GetComponent<RectTransform>(), new Vector3(0, 100, 0), 0.5f, 0f, Ease.OutExpo);     //slide in pickUpPopUI
        MoveUI(gameMenu.pickUpPopUI.GetComponent<RectTransform>(), new Vector3(500, 100, 0), 0.5f, 1f, Ease.OutExpo);   //slide out pickUpPopUI after 1s of delay
    }

#endregion

    /*--------------------------------------------------Tween Code-------------------------------------------------------------------*/

#region TweenCode

    void MoveUI(RectTransform _transform, Vector2 position, float moveTime, float delayTime, Ease ease, bool aActiveState)
    {
        _transform.DOAnchorPos(position, moveTime).SetDelay(delayTime).SetEase(ease).OnComplete(() =>
		{
			if (_transform.gameObject.activeInHierarchy == aActiveState)
				return;
			_transform.gameObject.SetActive(aActiveState);
		});
    }

	void MoveUI(RectTransform _transform, Vector2 position, float moveTime, float delayTime, Ease ease, System.Action onComplete)
	{
		_transform.DOAnchorPos(position, moveTime).SetDelay(delayTime).SetEase(ease).OnComplete(() =>
		{
			if (onComplete != null)
				onComplete();
		});
	}

	void MoveUI(RectTransform _transform, Vector2 position, float moveTime, float delayTime, Ease ease)
	{
		_transform.DOAnchorPos(position, moveTime).SetDelay(delayTime).SetEase(ease);
	}

#endregion

	/*--------------------------------------------------Struct-----------------------------------------------------------------------*/

#region Struct
	[System.Serializable]
    protected class GameMenu
    {
        public Image coinImg, magnetImg, turboImg, doubleCoinImg;                                       //ref to image objects
        public Slider fuelSlider;                                                                       //ref to slider
        public Text coinText, distanceText, countDownText, pickUpPopText;                               //ref to text
        public GameObject pickUpPopUI, magnetUI, doubleCoinUI, turboUI;                                 //ref to gameobjects
    }


    [System.Serializable]
    protected class MainMenu
    {
        public Image soundBtnImg;                                                                       //ref to image object
        public Text coinText;                                                                           //ref to text
        public Sprite soundOn, soundOff;                                                                //ref to sprites for sound button
    }

    [System.Serializable]
    protected class GameOverMenu
    {
        public Image soundBtnImg, giftBar;                                                              //ref to image objects
        public Text coinText, scoreText, hiScoreText, coinEarnedText, giftInfoText, rewardText;         //ref to text
        public Button rewardAdsbtn, giftbtn, collectBtn;                                                //ref to buttons
        public GameObject giftPanel, coinImg, discountWindow;                                                           //ref to gameobjects
    }

    [System.Serializable]
    protected class SpeedIncreaser
    {
        public float milestone, milestoneIncreaser, maxSpeed, speedMultiplier;                          //speed multipliers variables
    }

    [System.Serializable]
    protected class Tutorial
    {
        public GameObject tutorialPanel, closeBtn, nextbutton, previousButton;
        public Image tipImage;
        public Sprite[] tips;
    }

    [System.Serializable]
    public class FacebookPanel
    {
        public Text shareBtnText, likeBtnText, coinInfoText;
        public GameObject fbPanel, coinRewardPanel;
        public Image profileImage;
    }
#endregion

}
