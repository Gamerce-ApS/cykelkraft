/***********************************************************************************************************
 * Produced by Madfireon:               https://www.madfireongames.com/									   *
 * Facebook:                            https://www.facebook.com/madfireon/								   *
 * Contact us:                          https://www.madfireongames.com/contact							   *
 * Madfireon Unity Asset Store catalog: http://bit.ly/sellmyapp_store									   *
 * Developed by Swapnil Rane:           https://in.linkedin.com/in/swapnilrane24                           *
 ***********************************************************************************************************/

/***********************************************************************************************************
* NOTE:- This script controls power up shop menu                                                           *
***********************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class PowerUpShop : MonoBehaviour {

    [SerializeField]
    private PowerUpUI powerUpUI;                        //ref to powerup shop UI elements

	public Text coinText;


	// Use this for initialization
	void Start ()
    {
        SetData();                                      //set the powerups data
    }

	private void OnEnable()
	{
		SetData();
	}


	void SetData()
    {
        powerUpUI.turboLevelText.text = "Level " + (GameManager.Instance.turboUpgrade + 1);                     //set the turbo level
		if (GameManager.Instance.turboUpgrade == 4)
		{
			powerUpUI.turboUpgradeButton.interactable = false;
			powerUpUI.turboCostText.text = "Max";                                                                   //if level is equal to 4, set Cost text to MAX
		}
		else
		{
			int price = 200 * (GameManager.Instance.turboUpgrade + 1);
			powerUpUI.turboCostText.text = "" + price.ToString();                 //else set Cost text to coins of upgrade
			powerUpUI.turboUpgradeButton.interactable = price <= GameManager.Instance.coinAmount;
		}
        powerUpUI.turboBar.fillAmount = (GameManager.Instance.turboUpgrade + 1 )/ 5f;                           //set the turboBar fill amount
        GameManager.Instance.turboTime = 8f + (GameManager.Instance.turboUpgrade * powerUpUI.turboTimeIncr);    //set the turbo time

        powerUpUI.doubleLevelText.text = "Level " + (GameManager.Instance.doubleCoinUpgrade + 1);               //set the double level
		if (GameManager.Instance.doubleCoinUpgrade == 4)
		{
			powerUpUI.doubleUpgradeButton.interactable = false;
			powerUpUI.doubleCostText.text = "Max";                 //if level is equal to 4, set Cost text to MAX
		}
		else
		{
			int price = 200 * (GameManager.Instance.doubleCoinUpgrade + 1);
			powerUpUI.doubleCostText.text = "" + price.ToString();           //else set Cost text to coins of upgrade
			powerUpUI.doubleUpgradeButton.interactable = price <= GameManager.Instance.coinAmount;
		}
        powerUpUI.doubleBar.fillAmount = (GameManager.Instance.doubleCoinUpgrade + 1) / 5f;                     //set the doubleBar fill amount
        GameManager.Instance.doubleCoinTime = 10f + (GameManager.Instance.doubleCoinUpgrade * powerUpUI.doubleTimeIncr);    //set the turbo time

        powerUpUI.magnetLevelText.text = "Level " + (GameManager.Instance.magnetUpgrade + 1);                   //set the magnet level
		if (GameManager.Instance.magnetUpgrade == 4)
		{
			powerUpUI.magnetUpgradeButton.interactable = false;
			powerUpUI.magnetCostText.text = "Max";                     //if level is equal to 4, set Cost text to MAX
		}
		else
		{
			int price = 200 * (GameManager.Instance.magnetUpgrade + 1);
			powerUpUI.magnetCostText.text = "" + 200 * (GameManager.Instance.magnetUpgrade + 1);               //else set Cost text to coins of upgrade
			powerUpUI.magnetUpgradeButton.interactable = price <= GameManager.Instance.coinAmount;
		}
		powerUpUI.magnetBar.fillAmount = (GameManager.Instance.magnetUpgrade + 1 )/ 5f;                         //set the magnetBar fill amount
        GameManager.Instance.magnetTime = 10f + (GameManager.Instance.magnetUpgrade * powerUpUI.magnetTimeIncr);//set the magnet time

		coinText.text = GameManager.Instance.coinAmount.ToString();
	}

    public void UpgradeTurbo()                                                                                  //method for turbo upgrade buttons
    {
        if (GameManager.Instance.turboUpgrade < 4)                                                              //if upgrade is less than 4
        {
            if (GameManager.Instance.coinAmount >= 200 * (GameManager.Instance.turboUpgrade + 1))               //we check if we have enough coins to upgrade
            {
                GameManager.Instance.coinAmount -= 200 * (GameManager.Instance.turboUpgrade + 1);               //reduce the coins by upgrade cost
                GameManager.Instance.turboUpgrade++;                                                            //increase turbo level
                GameManager.Instance.Save();                                                                    //save it

                SetData();                                                                                      //set the data
                GuiManager.Instance.UpdateTotalCoins();                                                         //update total coins text
            }
        }
    }

    public void UpgradeDoubleCoin()                                                                             //method for DoubleCoin upgrade buttons
    {
        if (GameManager.Instance.doubleCoinUpgrade < 4)
        {
            if (GameManager.Instance.coinAmount >= 200 * (GameManager.Instance.doubleCoinUpgrade + 1))
            {
                GameManager.Instance.coinAmount -= 200 * (GameManager.Instance.doubleCoinUpgrade + 1);
                GameManager.Instance.doubleCoinUpgrade++;
                GameManager.Instance.Save();

                SetData();
                GuiManager.Instance.UpdateTotalCoins();
            }
        }
    }

    public void UpgradeMagnet()                                                                                 //method for Magnet upgrade buttons
    {
        if (GameManager.Instance.magnetUpgrade < 4)
        {
            if (GameManager.Instance.coinAmount >= 200 * (GameManager.Instance.magnetUpgrade + 1))
            {
                GameManager.Instance.coinAmount -= 200 * (GameManager.Instance.magnetUpgrade + 1);
                GameManager.Instance.magnetUpgrade++;
                GameManager.Instance.Save();

                SetData();
                GuiManager.Instance.UpdateTotalCoins();
            }
        }
    }



















    [System.Serializable]
    protected struct PowerUpUI
    {
		public Button turboUpgradeButton, doubleUpgradeButton, magnetUpgradeButton;
        public Text turboLevelText, doubleLevelText, magnetLevelText, turboCostText, doubleCostText, magnetCostText;
        public Image turboBar, doubleBar, magnetBar;
        public float turboTimeIncr, doubleTimeIncr, magnetTimeIncr;
    }
}
