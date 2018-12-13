/***********************************************************************************************************
 * Produced by Madfireon:               https://www.madfireongames.com/									   *
 * Facebook:                            https://www.facebook.com/madfireon/								   *
 * Contact us:                          https://www.madfireongames.com/contact							   *
 * Madfireon Unity Asset Store catalog: http://bit.ly/sellmyapp_store									   *
 * Developed by Swapnil Rane:           https://in.linkedin.com/in/swapnilrane24                           *
 ***********************************************************************************************************/

/***********************************************************************************************************
* NOTE:- This script controls car shop menu                                                                *
***********************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class CarShop : MonoBehaviour {

    public static CarShop instance;

	public Text coinText;

    [SerializeField] [Header("Car Shop UI Elements")]
    private CarShopUI carShopUI;
    [SerializeField] [Header("-----------------------")] [Header("Car Data")]
    private CarData[] carDatas;



    private int currentIndex = 0;

    // Use this for initialization
    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        SetGameData();
	}

	private void OnEnable()
	{
		SetGameData();
	}

	public void InitializeCarShop()
    {
        SetCarDetails();
    }

    void SetGameData()
    {
        //here we set the gamemanagers values as per our selected car
        GameManager.Instance.normalSpeed        = carDatas[GameManager.Instance.selectedCar].carSpeed;
        GameManager.Instance.turboSpeed         = carDatas[GameManager.Instance.selectedCar].carTurboSpeed;
        GameManager.Instance.fuel               = carDatas[GameManager.Instance.selectedCar].carFuel;
        GuiManager.Instance.DistanceMultiplier  = GameManager.Instance.normalSpeed;
    }

    void SetCarDetails()                                                                        //method which set the car details in the car shop
    {
        SoundManager.instance.PlayFX("BtnClick");                                                       //play sound
        carShopUI.carNameText.text      = carDatas[currentIndex].carName;                       //set car name text
        carDatas[currentIndex].carLevel = GameManager.Instance.carDatas[currentIndex].carLevel; //set car level
        if (GameManager.Instance.carDatas[currentIndex].unlocked == true)                       //if selected car is unlocked
        {
			carShopUI.selectButton.interactable = true;
            if (GameManager.Instance.selectedCar == currentIndex)                               //if its selected car
                carShopUI.selectText.text = "Selected";                                         //set Select Btn text to Selected
            else if (GameManager.Instance.selectedCar != currentIndex)                          //if iits not the selected car
                carShopUI.selectText.text = "Select";                                           //set Select Btn text to Select

            if (GameManager.Instance.carDatas[currentIndex].carLevel >= 3)                      //if car level is more than 3
            {
                carShopUI.upgradeInfoText.text = "Upgrade maxed";                               //set the text
                carShopUI.upgradeCostText.text = "Max";
                carShopUI.upgradeButton.interactable = false;                                   //make upgrade button interactable false
            }
            else if (GameManager.Instance.carDatas[currentIndex].carLevel < 3)                  //if car level is less than 3
            {
				int cost = 200 * (carDatas[currentIndex].carLevel + 1);
				carShopUI.upgradeInfoText.text = "Upgrade to lvl" + (carDatas[currentIndex].carLevel + 1);  //set the upgrade info text
                carShopUI.upgradeCostText.text = "" + cost.ToString();  //set the cost text
                carShopUI.upgradeButton.interactable = GameManager.Instance.coinAmount >= cost;                                    //make upgrade button interactable true
            }
        }
        else if (GameManager.Instance.carDatas[currentIndex].unlocked == false)                 //if car is not unlocked
        {
            carShopUI.selectText.text = carDatas[currentIndex].carPrice + " Coins";             //set the select button to price of car
			carShopUI.selectButton.interactable = carDatas[currentIndex].carPrice < GameManager.Instance.coinAmount;
            carShopUI.upgradeButton.interactable = false;                                       //make upgrade button interactable false

            carShopUI.upgradeInfoText.text = "Upgrade to lvl" + (carDatas[currentIndex].carLevel + 1);  //set the upgrade info text
            carShopUI.upgradeCostText.text = "" + 200 * (carDatas[currentIndex].carLevel + 1);
        }

        carShopUI.carImage.sprite = carDatas[currentIndex].carSprite;                           //set the car image
                                                                                                //set speed bar value
        carShopUI.speedBar.fillAmount = (carDatas[currentIndex].carSpeed + carDatas[currentIndex].carLevel * carDatas[currentIndex].carSpeedIncreaser) / 15;
                                                                                                //set fuel bar value
        carShopUI.fuelBar.fillAmount = (carDatas[currentIndex].carFuel + carDatas[currentIndex].carLevel * carDatas[currentIndex].carFuelIncreaser) / 15;
                                                                                                //set turbo speed bar value
        carShopUI.turboSpeedBar.fillAmount = (carDatas[currentIndex].carTurboSpeed + carDatas[currentIndex].carLevel * carDatas[currentIndex].carTurboSpeedIncreaser) / 8;

		coinText.text = GameManager.Instance.coinAmount.ToString();
	}

    public void NextCar()                                                                       //next button method
    {
        if (currentIndex < carDatas.Length - 1)                                                 //if current Index is less than total cars
        {
            currentIndex++;                                                                     //increase it by 1
            SetCarDetails();                                                                    //set car details
        }
    }

    public void PreviousCar()                                                                   //previous button method
    {
        if (currentIndex > 0)                                                                   //if current Index is more than 0
        {
            currentIndex--;                                                                     //decrease it by 1
            SetCarDetails();                                                                    //set car details
        }
    }

    public void UpgradeCarBtn()                                                                 //upgrade button method
    {
        if (carDatas[currentIndex].carLevel < 3)                                                //if car level is less than 3
        {
            if (GameManager.Instance.coinAmount >= (200 * (carDatas[currentIndex].carLevel + 1)))   //if we have enough coins to upgrade
            {
                GameManager.Instance.coinAmount -= (200 * (carDatas[currentIndex].carLevel + 1));   //reduce the coins
                GameManager.Instance.carDatas[currentIndex].carLevel++;                             //upgrade the level
                GameManager.Instance.Save();                                                        //save it
                GuiManager.Instance.UpdateTotalCoins();                                             //update the total coins text
                SetCarDetails();                                                                    //set the car details
            }
        }
    }

    public void SelectCarBtn()                                                                  //car select button
    {
        if (currentIndex != GameManager.Instance.selectedCar)                                   //if currentIndex is not equal to selected car
        {
            if (GameManager.Instance.carDatas[currentIndex].unlocked == true)                   //if the car is unlocked
            {
                GameManager.Instance.selectedCar = currentIndex;                                //set the GameManager.instance.selectedCar to currentIndex    
                GameManager.Instance.Save();                                                    //Save it
                SetCarDetails();                                                                //set car details
                PlayerController.instance.SetCarSprite();                                       //change player car sprite
            }
            else if (GameManager.Instance.carDatas[currentIndex].unlocked == false)             //if the car is not unlocked
            {
                if (GameManager.Instance.coinAmount >= carDatas[currentIndex].carPrice)         //we check if we have enough coins
                {
                    GameManager.Instance.coinAmount -= carDatas[currentIndex].carPrice;         //we reduce the coins
                    GameManager.Instance.carDatas[currentIndex].unlocked = true;                //we unlock it
                    GameManager.Instance.selectedCar = currentIndex;                            //set it as selected car
                    GameManager.Instance.Save();                                                //save it
                    SetCarDetails();                                                            //set car details
                    GuiManager.Instance.UpdateTotalCoins();                                     //update the total coins text
                    PlayerController.instance.SetCarSprite();                                   //change player car sprite
                }
            }
            SetGameData();                                                                      //set game data
        }
    }


























    [System.Serializable]
    protected struct CarShopUI
    {
        public Text carNameText, upgradeInfoText, upgradeCostText, selectText;
        public Image carImage;
        public Image speedBar, fuelBar, turboSpeedBar;
        public Button upgradeButton, selectButton;
    }

    [System.Serializable]
    protected struct CarData
    {
        public string carName;
        public Sprite carSprite;
        public int carPrice;
        public float carSpeed, carFuel, carTurboSpeed;
        public float carSpeedIncreaser, carFuelIncreaser, carTurboSpeedIncreaser;
        [HideInInspector] public int carLevel;
        [HideInInspector] public bool unlocked;
    }

}
