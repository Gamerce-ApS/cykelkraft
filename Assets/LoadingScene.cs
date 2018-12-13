using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
	public LoadingText loadingText;

	private void Start()
	{
		loadingText.SetText("Loading");

		StartCoroutine(Loading());

	}

	IEnumerator Loading()
	{
		yield return new WaitUntil(() => { return GamerceInit.instance.internalDataRecieved == true; });

		SceneManager.LoadScene(1);
	}
}
