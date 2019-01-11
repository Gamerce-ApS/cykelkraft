using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using System;

public class GameAnalyticsManager : MonoBehaviour {


	public static GameAnalyticsManager instance = null;

	public enum ItemTypes
	{
		RowBreaker1,
		ColumnBreaker1,
		Breaker1,
		RowBreaker5,
		ColumnBreaker5,
		Breaker5,
		ExtraMoves,
		IAP,
		DailyReward,
		None
	}

	void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
	}

	public static void LevelComplete(int aLevel)
	{
		GameAnalytics.NewDesignEvent("LevelStatus:Complete:Level"+aLevel.ToString());
	}

	public void LevelStarted(int aLevelIndex)
	{
		GameAnalytics.NewDesignEvent("Levels:Started:Level" + aLevelIndex.ToString());
	}

	public static void LevelFail(int aLevel)
	{
		GameAnalytics.NewDesignEvent("LevelStatus:Fail:Level" + aLevel.ToString());
	}

	public static void ClaimingDiscountCode(int discountPercent)
	{
		GameAnalytics.NewDesignEvent("ClaimedDiscount:Discount" + discountPercent.ToString());
	}

	public static void BoughtExtraMoves(int aLevel)
	{
		GameAnalytics.NewDesignEvent("BoughtExtraMoves:Level"+aLevel.ToString());
	}

	public static void BoosterBought(string aBooster)
	{
		GameAnalytics.NewDesignEvent("BoughtBoosters:" + aBooster);
	}

	public void TimeSpentInGame(int aLevelIndex, int aTimeInMinutes)
	{
		GameAnalytics.NewDesignEvent("Level:TimeSpent:Level" + aLevelIndex.ToString() + ":" + aTimeInMinutes);

	}

	public static void RestartLevel(int aLevel)
	{
		GameAnalytics.NewDesignEvent("RestartLevel:Level"+aLevel.ToString());
	}

	public static void RetryLevel(int aLevel)
	{
		GameAnalytics.NewDesignEvent("RetryLevel:Level" + aLevel.ToString());
	}

	public static void UsedBooster(string aBooster)
	{
		GameAnalytics.NewDesignEvent("BoosterUsed:" + aBooster.ToString());
	}



	public static void SpendCoins(float aAmount, ItemTypes itemType, string itemId)
	{
		GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "Coins", aAmount, itemType.ToString(), itemId);
	}

	public static void GainCoins(float aAmount, ItemTypes itemType, string itemId)
	{
		GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "Coins", aAmount, itemType.ToString(), itemId);
	}

	public void ClickedLoggedIn()
	{
		GameAnalytics.NewDesignEvent("Gamerce:PressedLoggedIn");
	}

	public void LoggedOutSuccess()
	{
		GameAnalytics.NewDesignEvent("Gamerce:LoggedOutSuccess");
	}

	public void LoggedOutFail()
	{
		GameAnalytics.NewDesignEvent("Gamerce:LoggedOutFailed");
	}

	public void ClickedRegister()
	{
		GameAnalytics.NewDesignEvent("Gamerce:ClickedRegister");
	}

	public void RegisterFailed()
	{
		GameAnalytics.NewDesignEvent("Gamerce:RegisterFailed");

	}

	public void RegisteredSuccessful()
	{
		GameAnalytics.NewDesignEvent("Gamerce:RegisterSuccess");
	}

	public void ClickedUsePoints()
	{
		GameAnalytics.NewDesignEvent("Gamerce:ClickedUsePoints");
	}

	public void SubscribsNewsletter(string aName, string aWhere)
	{
		GameAnalytics.NewDesignEvent("Gamerce:SubscribedNewsLetter:" + aName + ":" + aWhere);
	}

	public void LogginSuccessful()
	{
		GameAnalytics.NewDesignEvent("Gamerce:LoginSuccess");
	}

	public void ClosesNewsletterPopup(string aShowLocation)
	{
		GameAnalytics.NewDesignEvent("Gamerce:CloseNewsLetter:" + aShowLocation);
	}

	public void ClickedOnClothes()
	{
	}

	public void LogginFailed()
	{
		GameAnalytics.NewDesignEvent("Gamerce:LoginFailed");
	}

	public void PressedLoggedOut()
	{
		GameAnalytics.NewDesignEvent("Gamerce:PressedLogedOut");
	}

	public void GoToRegisterFromLogin()
	{
		GameAnalytics.NewDesignEvent("Gamerce:ClickedToGoRegisterFromLogin");
	}

	internal void ClickedGoShop()
	{
		
	}
}
