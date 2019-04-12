using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RateUsPanel : MonoBehaviour {

	private void OnEnable()
	{
		PlayerPrefs.SetInt("HaveSeenRateUs", 1);
	}

	public void ClosePanel()
	{
		GuiManager.Instance.MenuBtns("closeRateUs");
	}

	public void RateUs()
	{
		Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
		ClosePanel();
	}
}
