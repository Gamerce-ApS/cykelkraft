using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingText : MonoBehaviour {

	public Text textObj;
	public int secondsPerDot;
	string text;
	Coroutine loading;

	private void OnEnable()
	{
		loading = StartCoroutine(Loading());
	}

	void OnDisable()
	{
		StopCoroutine(loading);
	}

	public void SetText(string aText)
	{
		textObj.text = aText;
		text = aText;
	}

	IEnumerator Loading()
	{
		while(true)
		{
			textObj.text = text;
			yield return new WaitForSeconds(secondsPerDot);
			textObj.text = text + ".";
			yield return new WaitForSeconds(secondsPerDot);
			textObj.text = text + "..";
			yield return new WaitForSeconds(secondsPerDot);
			textObj.text = text + "...";
			yield return new WaitForSeconds(secondsPerDot);

		}
	}


}
