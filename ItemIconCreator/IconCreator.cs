using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ItemIconCreator;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class IconCreator : MonoBehaviour
{
	public enum SaveLocation
	{
		persistentDataPath,
		dataPath,
		projectFolder,
		custom
	}

	public enum Mode
	{
		Automatic,
		Manual
	}

	protected bool isCreatingIcons;

	public bool useDafaultName;

	public bool includeResolutionInFileName;

	public string iconFileName;

	public SaveLocation pathLocation;

	public Mode mode;

	public string folderName = "Screenshots";

	public bool useTransparency = true;

	public bool lookAtObjectCenter;

	public bool dynamicFov;

	public float fovOffset;

	protected string finalPath;

	private Vector3 mousePostion;

	public KeyCode nextIconKey = (KeyCode)32;

	protected bool CanMove;

	public bool preview = true;

	protected Camera whiteCam;

	protected Camera blackCam;

	public Camera mainCam;

	protected Texture2D texBlack;

	protected Texture2D texWhite;

	protected Texture2D finalTexture;

	private CameraClearFlags originalClearFlags;

	protected Transform currentObject;

	private void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		mainCam = ((Component)this).gameObject.GetComponent<Camera>();
		originalClearFlags = mainCam.clearFlags;
		if ((Object)(object)IconCreatorCanvas.instance != (Object)null)
		{
			IconCreatorCanvas.instance.SetInfo(0, 0, "", isRecording: false, nextIconKey);
		}
	}

	protected void Initialize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		mainCam.clearFlags = originalClearFlags;
		isCreatingIcons = true;
		Camera[] array = Object.FindObjectsOfType<Camera>();
		foreach (Camera val in array)
		{
			if (!((Object)(object)val == (Object)(object)mainCam))
			{
				((Component)val).gameObject.SetActive(false);
			}
		}
		if (useTransparency)
		{
			CreateBlackAndWhiteCameras();
		}
		CacheAndInitialiseFields();
	}

	protected void DeleteCameras()
	{
		if ((Object)(object)whiteCam != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)whiteCam).gameObject);
		}
		if ((Object)(object)blackCam != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)blackCam).gameObject);
		}
		isCreatingIcons = false;
	}

	public virtual void BuildIcons()
	{
		Debug.LogError((object)"Not implemented");
	}

	protected IEnumerator CaptureFrame(string objectName, int i)
	{
		if ((Object)(object)whiteCam != (Object)null)
		{
			((Behaviour)whiteCam).enabled = true;
		}
		if ((Object)(object)blackCam != (Object)null)
		{
			((Behaviour)blackCam).enabled = true;
		}
		yield return (object)new WaitForEndOfFrame();
		if (useTransparency)
		{
			RenderCamToTexture(blackCam, texBlack);
			RenderCamToTexture(whiteCam, texWhite);
			CalculateOutputTexture();
		}
		else
		{
			RenderCamToTexture(mainCam, finalTexture);
		}
		SavePng(objectName, i);
		((Behaviour)mainCam).enabled = true;
	}

	protected virtual void Update()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (mode == Mode.Automatic || !CanMove)
		{
			return;
		}
		if (Input.GetMouseButtonDown(0))
		{
			mousePostion = Input.mousePosition;
		}
		if (Input.GetMouseButton(0))
		{
			Vector3 val = mousePostion - Input.mousePosition;
			currentObject.Rotate(new Vector3(0f - val.y, val.x, val.z) * Time.deltaTime * 40f, (Space)0);
			mousePostion = Input.mousePosition;
			if (dynamicFov)
			{
				UpdateFOV(((Component)currentObject).gameObject);
			}
			if (lookAtObjectCenter)
			{
				LookAtTargetCenter(((Component)currentObject).gameObject);
			}
		}
		UpdateFOV(Input.mouseScrollDelta.y * -2.5f);
	}

	private void RenderCamToTexture(Camera cam, Texture2D tex)
	{
		((Behaviour)cam).enabled = true;
		cam.Render();
		WriteScreenImageToTexture(tex);
		((Behaviour)cam).enabled = false;
	}

	private void CreateBlackAndWhiteCameras()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		mainCam.clearFlags = (CameraClearFlags)2;
		GameObject val = new GameObject();
		((Object)val).name = "White Background Camera";
		whiteCam = val.AddComponent<Camera>();
		whiteCam.CopyFrom(mainCam);
		whiteCam.backgroundColor = Color.white;
		val.transform.SetParent(((Component)this).gameObject.transform, true);
		val = new GameObject();
		((Object)val).name = "Black Background Camera";
		blackCam = val.AddComponent<Camera>();
		blackCam.CopyFrom(mainCam);
		blackCam.backgroundColor = Color.black;
		val.transform.SetParent(((Component)this).gameObject.transform, true);
	}

	protected void CreateNewFolderForIcons()
	{
		finalPath = GetFinalFolder();
		if (Directory.Exists(finalPath))
		{
			int num = 1;
			while (Directory.Exists(finalPath + " " + num))
			{
				num++;
			}
			finalPath = finalPath + " " + num;
		}
		Directory.CreateDirectory(finalPath);
	}

	public string GetFinalFolder()
	{
		if (!string.IsNullOrWhiteSpace(GetBaseLocation()))
		{
			return Path.Combine(GetBaseLocation(), folderName);
		}
		return folderName;
	}

	private void WriteScreenImageToTexture(Texture2D tex)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		tex.ReadPixels(new Rect(0f, 0f, (float)Screen.width, (float)Screen.width), 0, 0);
		tex.Apply();
	}

	private void CalculateOutputTexture()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ((Texture)finalTexture).height; i++)
		{
			for (int j = 0; j < ((Texture)finalTexture).width; j++)
			{
				float num = texWhite.GetPixel(j, i).r - texBlack.GetPixel(j, i).r;
				num = 1f - num;
				Color val = ((num != 0f) ? (texBlack.GetPixel(j, i) / num) : Color.clear);
				val.a = num;
				finalTexture.SetPixel(j, i, val);
			}
		}
	}

	private void SavePng(string name, int i)
	{
		string fileName = GetFileName(name, i);
		string text = finalPath + "/" + fileName;
		Debug.Log((object)("Writing to: " + text));
		byte[] bytes = ImageConversion.EncodeToPNG(finalTexture);
		File.WriteAllBytes(text, bytes);
	}

	public string GetFileName(string name, int i)
	{
		string text = ((!useDafaultName) ? iconFileName : name);
		text += " Icon";
		if (includeResolutionInFileName)
		{
			text = text + " " + mainCam.scaledPixelHeight + "x";
		}
		return text + ".png";
	}

	private void CacheAndInitialiseFields()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		texBlack = new Texture2D(mainCam.pixelWidth, mainCam.pixelHeight, (TextureFormat)3, false);
		texWhite = new Texture2D(mainCam.pixelWidth, mainCam.pixelHeight, (TextureFormat)3, false);
		finalTexture = new Texture2D(mainCam.pixelWidth, mainCam.pixelHeight, (TextureFormat)5, false);
	}

	protected void UpdateFOV(GameObject targetItem)
	{
		float targetFov = GetTargetFov(targetItem);
		if (useTransparency && isCreatingIcons)
		{
			whiteCam.fieldOfView = targetFov;
			blackCam.fieldOfView = targetFov;
		}
		mainCam.fieldOfView = targetFov;
	}

	protected void UpdateFOV(float value)
	{
		if (value != 0f)
		{
			value = mainCam.fieldOfView * value / 100f;
			dynamicFov = false;
			if (useTransparency)
			{
				Camera obj = whiteCam;
				obj.fieldOfView += value;
				Camera obj2 = blackCam;
				obj2.fieldOfView += value;
			}
			Camera obj3 = mainCam;
			obj3.fieldOfView += value;
		}
	}

	protected void LookAtTargetCenter(GameObject targetItem)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Vector3 meshCenter = GetMeshCenter(targetItem);
		((Component)mainCam).transform.LookAt(meshCenter);
		if ((Object)(object)whiteCam != (Object)null)
		{
			((Component)whiteCam).transform.LookAt(meshCenter);
		}
		if ((Object)(object)blackCam != (Object)null)
		{
			((Component)blackCam).transform.LookAt(meshCenter);
		}
	}

	private float GetTargetFov(GameObject a)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.one * 30000f;
		Vector3 val2 = Vector3.zero;
		List<Renderer> renderers = GetRenderers(a);
		for (int i = 0; i < renderers.Count; i++)
		{
			Vector3 zero = Vector3.zero;
			Bounds bounds = renderers[i].bounds;
			if (Vector3.Distance(zero, ((Bounds)(ref bounds)).min) < Vector3.Distance(Vector3.zero, val))
			{
				bounds = renderers[i].bounds;
				val = ((Bounds)(ref bounds)).min;
			}
			Vector3 zero2 = Vector3.zero;
			bounds = renderers[i].bounds;
			if (Vector3.Distance(zero2, ((Bounds)(ref bounds)).max) > Vector3.Distance(Vector3.zero, val2))
			{
				bounds = renderers[i].bounds;
				val2 = ((Bounds)(ref bounds)).max;
			}
		}
		Vector3 val3 = (val + val2) / 2f;
		Vector3 val4 = val2 - val;
		float num = ((Vector3)(ref val4)).magnitude / 2f;
		float num2 = Vector3.Distance(val3, ((Component)this).transform.position);
		float num3 = Mathf.Sqrt(num * num + num2 * num2);
		return Mathf.Asin(num / num3) * 57.29578f * 2f + fovOffset;
	}

	private List<Renderer> GetRenderers(GameObject obj)
	{
		List<Renderer> list = new List<Renderer>();
		if (obj.GetComponents<Renderer>() != null)
		{
			list.AddRange(obj.GetComponents<Renderer>());
		}
		if (obj.GetComponentsInChildren<Renderer>() != null)
		{
			list.AddRange(obj.GetComponentsInChildren<Renderer>());
		}
		return list;
	}

	private Vector3 GetMeshCenter(GameObject a)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		List<Renderer> renderers = GetRenderers(a);
		if (renderers == null)
		{
			Debug.LogError((object)("No mesh was founded in object " + ((Object)a).name));
			return a.transform.position;
		}
		for (int i = 0; i < renderers.Count; i++)
		{
			Vector3 val2 = val;
			Bounds bounds = renderers[i].bounds;
			val = val2 + ((Bounds)(ref bounds)).center;
		}
		return val / (float)renderers.Count;
	}

	protected void RevealInFinder()
	{
	}

	public virtual bool CheckConditions()
	{
		if (pathLocation == SaveLocation.custom && !Directory.Exists(folderName))
		{
			Debug.LogError((object)("Folder " + folderName + " does not exists"));
			return false;
		}
		if (!useDafaultName && string.IsNullOrWhiteSpace(iconFileName))
		{
			Debug.LogError((object)"Invalid icon file name");
			return false;
		}
		return true;
	}

	private string GetBaseLocation()
	{
		if (pathLocation == SaveLocation.dataPath)
		{
			return Application.dataPath;
		}
		if (pathLocation == SaveLocation.persistentDataPath)
		{
			return Application.persistentDataPath;
		}
		if (pathLocation == SaveLocation.projectFolder)
		{
			return Path.GetDirectoryName(Application.dataPath);
		}
		return "";
	}

	private void OnValidate()
	{
		if ((Object)(object)mainCam == (Object)null)
		{
			mainCam = ((Component)this).GetComponent<Camera>();
		}
	}
}
