using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasFitter : MonoBehaviour {

	public RectTransform narrowViewScaler;
	public bool NarrowAspectLayout { get { return Camera.main.aspect < 0.54f; } }

	private void Awake()
	{
		//ScaleToIphoneX();
	}

	private void Start()
	{
		ScaleToIphoneX();
	}

	void ScaleToIphoneX()
	{
		if (NarrowAspectLayout)
		{
			RectTransform rect = GetComponent<RectTransform>();
			var sa = Screen.safeArea;
			//var sa = new Rect(10, 20, Screen.width - 20, Screen.height - 30);
			sa.x /= Screen.width;
			sa.width /= Screen.width;
			sa.y /= Screen.height;
			sa.height /= Screen.height;

			float w = 1152;
			float h = 2048;
			var r = narrowViewScaler.rect;
			var scale = h / r.height;

			//var w = r.width;
			// 9:16 with 2048 height requires 1152 width!
			scale = r.width / w;
			r.width = w;

			//Get full screen height
			float f = (w / Screen.width) * Screen.height;

			Vector2 size = new Vector2(w * sa.width, f * sa.height);
			Vector2 center = new Vector2(-w / 2 + w * sa.center.x, -f / 2 + f * sa.center.y);

			narrowViewScaler.anchorMin = new Vector2(0.5f, 0.5f);
			narrowViewScaler.anchorMax = new Vector2(0.5f, 0.5f);
			narrowViewScaler.pivot = new Vector2(0.5f, 0.5f);
			narrowViewScaler.sizeDelta = size;
			narrowViewScaler.localScale = Vector3.one * scale;
			narrowViewScaler.localPosition = center;
		}
	}
}
