using System;
using System.Collections;
using System.IO;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.AvatarFramework;

public class MugshotGenerator : Singleton<MugshotGenerator>
{
	public string OutputPath;

	public AvatarSettings Settings;

	[Header("References")]
	public Avatar MugshotRig;

	public IconGenerator Generator;

	public AvatarSettings DefaultSettings;

	public Transform LookAtPosition;

	private Texture2D finalTexture;

	private bool generate;

	protected override void Awake()
	{
		base.Awake();
		((Component)MugshotRig).gameObject.SetActive(false);
	}

	private void LateUpdate()
	{
		if (generate)
		{
			generate = false;
			FinalizeMugshot();
		}
	}

	private void FinalizeMugshot()
	{
		finalTexture = Generator.GetTexture(((Component)MugshotRig).transform);
		Debug.Log((object)"Mugshot capture");
	}

	[Button]
	public void GenerateMugshot()
	{
		GenerateMugshot(Settings, fileToFile: true, null);
	}

	public void GenerateMugshot(AvatarSettings settings, bool fileToFile, Action<Texture2D> callback)
	{
		finalTexture = null;
		Debug.Log((object)"Mugshot start");
		AvatarSettings avatarSettings = Object.Instantiate<AvatarSettings>(settings);
		avatarSettings.Height = 1f;
		((Component)MugshotRig).gameObject.SetActive(true);
		MugshotRig.LoadAvatarSettings(avatarSettings);
		LayerUtility.SetLayerRecursively(((Component)MugshotRig).gameObject, LayerMask.NameToLayer("IconGeneration"));
		SkinnedMeshRenderer[] componentsInChildren = ((Component)MugshotRig).GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].updateWhenOffscreen = true;
		}
		generate = true;
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => (Object)(object)finalTexture != (Object)null));
			if (fileToFile)
			{
				string text = OutputPath + "/" + ((Object)settings).name + "_Mugshot.png";
				byte[] bytes = ImageConversion.EncodeToPNG(finalTexture);
				Debug.Log((object)("Writing to: " + text));
				File.WriteAllBytes(text, bytes);
			}
			if (callback != null)
			{
				callback(finalTexture);
			}
			MugshotRig.LoadAvatarSettings(DefaultSettings);
			((Component)MugshotRig).gameObject.SetActive(false);
		}
	}
}
