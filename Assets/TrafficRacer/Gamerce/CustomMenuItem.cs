#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class CustomMenuItem : MonoBehaviour {

	[MenuItem("Custom/PlayerPrefs/UnlockAllLevels")]
	public static void UnlockAllLevels()
	{
		for(int i = 0; i < 100; ++i)
		{
			string key = string.Format("Level.{0:000}.StarsCount", i + 1);
			PlayerPrefs.SetInt(key, 1);
		}
		Debug.Log("All levels unlocked!");
	}

	[MenuItem("Custom/PlayerPrefs/DeleteAll")]
	public static void DeletAll()
	{
		PlayerPrefs.DeleteAll();
		Debug.Log("PlayerPrefs deleted");
	}



}
#endif