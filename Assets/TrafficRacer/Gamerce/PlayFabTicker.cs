using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using System;
using UnityEngine.SceneManagement;

public class PlayFabTicker : MonoBehaviour {

	public float TimePlayed = 0;
	public bool Running = false;

	[System.Serializable]
	public class ReturnValue
	{
		public float totalTime;

		public static ReturnValue CreateFromJSON(string jsonString)
		{
			return JsonUtility.FromJson<ReturnValue>(jsonString);
		}
	}

	private void OnEnable()
	{
	}

	// Use this for initialization
	void Start () 
	{
		//StartTicker ();
	}
	void OnDestroy()
	{
		//EndTicker ();
	}
	// Update is called once per frame
	void Update () 
	{
		if(Running)
			TimePlayed += Time.deltaTime;

	}
	public void StartTicker()
	{
		Running = true;
	}
	public void EndTicker(System.Action<bool> onComplete = null)
	{
		if(Running == true)
		{
			SendData(onComplete);
			Running = false;
		}
	}

	public bool IsRunning()
	{
		return Running;
	}

	public void SendData(System.Action<bool> onComplete = null)
	{
		TimePlayed = TimePlayed / 60f;
		//FIX THIS!!!
		//GameAnalyticsManager.instance.TimeSpentInGame(PlayerPrefs.GetInt("OpenLevel", 0), (int)TimePlayed);
		if (GamerceInit.instance.IsLogedInToPlayfab == false || GamerceInit.instance.CheckInternetAvailability() == false)
		{
			GamerceInit.instance.LoginToPlayfab();
			//TimePlayed += PlayerPrefs.GetFloat("PlayedForTime", 0f);
			Debug.Log("PointsToSyncWhenInternet: " + TimePlayed);
			PlayerPrefs.SetFloat("PointsToSync", TimePlayed + PlayerPrefs.GetFloat("PointsToSync",0f));
			if (onComplete != null)
				onComplete(true);
			return;
		}

		bool sync = false;
		float pointsToSync = PlayerPrefs.GetFloat("PointsToSync", 0f);
		if (pointsToSync > 0)
			sync = true;
		GamerceInit.instance.SendTime(TimePlayed + pointsToSync, (a)=> {

			float pointsToSyncOnComplete = 0f;
			if (a == false)
			{
				pointsToSyncOnComplete = TimePlayed;
			}
			PlayerPrefs.SetFloat("PointsToSync", 0f);
			if (onComplete != null)
				onComplete(a);
		}, sync);

	}

	private void OnDisable()
	{
		//EndTicker();	
	}

}