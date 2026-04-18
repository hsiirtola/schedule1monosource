using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class CameraCaptureToPNG : MonoBehaviour
{
	public Camera targetCamera;

	public int width = 1920;

	public int height = 1080;

	public KeyCode captureKey = (KeyCode)112;

	private void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown(captureKey))
		{
			((MonoBehaviour)this).StartCoroutine(CaptureCameraView());
		}
	}

	private IEnumerator CaptureCameraView()
	{
		yield return (object)new WaitForEndOfFrame();
		if ((Object)(object)targetCamera == (Object)null)
		{
			Debug.LogError((object)"No targetCamera assigned.");
			yield break;
		}
		RenderTexture val = new RenderTexture(width, height, 24, (RenderTextureFormat)0);
		targetCamera.targetTexture = val;
		targetCamera.Render();
		RenderTexture.active = val;
		Texture2D val2 = new Texture2D(width, height, (TextureFormat)4, false);
		val2.ReadPixels(new Rect(0f, 0f, (float)width, (float)height), 0, 0);
		val2.Apply();
		targetCamera.targetTexture = null;
		RenderTexture.active = null;
		Object.Destroy((Object)(object)val);
		byte[] bytes = ImageConversion.EncodeToPNG(val2);
		string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CameraCapture.png");
		File.WriteAllBytes(text, bytes);
		Debug.Log((object)("✅ Camera view saved to " + text));
	}
}
