using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gamerce_StarHandler : MonoBehaviour {

	public List<GameObject> starsEnabled = new List<GameObject>();
	public List<GameObject> starsDisabled = new List<GameObject>();

	public void SetActiveStars(int aAmount)
	{
		for(int i = 0; i < starsEnabled.Count; ++i)
		{
			starsEnabled[i].SetActive(aAmount > i);
			//starsDisabled[i].SetActive(starsEnabled[i].activeSelf == false);
		}
	}
}
