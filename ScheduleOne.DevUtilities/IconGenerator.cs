using System;
using System.Collections.Generic;
using System.IO;
using ScheduleOne.Core;
using ScheduleOne.FX;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.DevUtilities;

public class IconGenerator : Singleton<IconGenerator>
{
	[Serializable]
	public class PackagingVisuals
	{
		public string PackagingID;

		public MultiTypeVisualsSetter ProductVisuals;

		public Transform TopLevelTransform;
	}

	public int IconSize = 512;

	public string OutputPath;

	public bool ModifyLighting = true;

	[Header("References")]
	public Registry Registry;

	public Camera CameraPosition;

	public Transform MainContainer;

	public Transform ItemContainer;

	public GameObject Canvas;

	public List<PackagingVisuals> Visuals;

	public UniversalRendererData rendererData;

	protected override void Awake()
	{
		base.Awake();
		Canvas.gameObject.SetActive(false);
		((Component)CameraPosition).gameObject.SetActive(false);
		CameraPosition.clearFlags = (CameraClearFlags)2;
		if ((Object)(object)Registry == (Object)null)
		{
			Registry = Singleton<Registry>.Instance;
		}
	}

	[Button]
	public void GenerateIcon()
	{
		LayerUtility.SetLayerRecursively(((Component)ItemContainer).gameObject, LayerMask.NameToLayer("IconGeneration"));
		Transform val = null;
		for (int i = 0; i < ((Component)ItemContainer).transform.childCount; i++)
		{
			if (((Component)((Component)ItemContainer).transform.GetChild(i)).gameObject.activeSelf)
			{
				val = ((Component)ItemContainer).transform.GetChild(i);
			}
		}
		string text = OutputPath + "/" + ((Object)val).name + " Icon.png";
		Texture2D texture = GetTexture(((Component)val).transform);
		Debug.Log((object)("Writing to: " + text));
		byte[] bytes = ImageConversion.EncodeToPNG(texture);
		File.WriteAllBytes(text, bytes);
	}

	public Texture2D GeneratePackagingIcon(string packagingID, string productID)
	{
		if ((Object)(object)Singleton<Registry>.Instance != (Object)null)
		{
			Registry = Singleton<Registry>.Instance;
		}
		PackagingVisuals packagingVisuals = Visuals.Find((PackagingVisuals x) => packagingID == x.PackagingID);
		if (packagingVisuals == null)
		{
			Debug.LogError((object)("Failed to find visuals for packaging (" + packagingID + ") containing product (" + productID + ")"));
			return null;
		}
		ItemDefinition itemDefinition = Registry._GetItem(productID);
		if (Application.isPlaying)
		{
			itemDefinition = Singleton<Registry>.Instance._GetItem(productID);
		}
		ProductDefinition productDefinition = itemDefinition as ProductDefinition;
		if ((Object)(object)productDefinition == (Object)null)
		{
			Debug.LogError((object)("Failed to find product definition for product (" + productID + ")"));
			return null;
		}
		ProductItemInstance itemInstance = productDefinition.GetDefaultInstance() as ProductItemInstance;
		packagingVisuals.ProductVisuals.ApplyVisuals(itemInstance);
		((Component)packagingVisuals.TopLevelTransform).gameObject.SetActive(true);
		Texture2D texture = GetTexture(((Component)packagingVisuals.ProductVisuals).transform.parent);
		((Component)packagingVisuals.TopLevelTransform).gameObject.SetActive(false);
		return texture;
	}

	public Texture2D GetTexture(Transform model)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		((Component)MainContainer).gameObject.SetActive(true);
		bool activeSelf = ((Component)ItemContainer).gameObject.activeSelf;
		((Component)ItemContainer).gameObject.SetActive(true);
		if (ModifyLighting)
		{
			RenderSettings.ambientMode = (AmbientMode)3;
			RenderSettings.ambientLight = Color.white;
		}
		PsychedelicFullScreenFeature psychedelicFullScreenFeature = ((ScriptableRendererData)rendererData).rendererFeatures.Find((ScriptableRendererFeature x) => ((Object)x).name == "PsychedelicFullScreenFeature") as PsychedelicFullScreenFeature;
		bool flag = false;
		if ((Object)(object)psychedelicFullScreenFeature != (Object)null && ((ScriptableRendererFeature)psychedelicFullScreenFeature).isActive)
		{
			flag = true;
			((ScriptableRendererFeature)psychedelicFullScreenFeature).SetActive(false);
		}
		RuntimePreviewGenerator.CamPos = ((Component)CameraPosition).transform.position;
		RuntimePreviewGenerator.CamRot = ((Component)CameraPosition).transform.rotation;
		RuntimePreviewGenerator.Padding = 0f;
		RuntimePreviewGenerator.UseLocalBounds = true;
		RuntimePreviewGenerator.BackgroundColor = Color32.op_Implicit(new Color32((byte)0, (byte)0, (byte)0, (byte)0));
		Texture2D result = RuntimePreviewGenerator.GenerateModelPreview(model, IconSize, IconSize, false, true);
		if ((Object)(object)psychedelicFullScreenFeature != (Object)null && flag)
		{
			((ScriptableRendererFeature)psychedelicFullScreenFeature).SetActive(true);
		}
		RenderSettings.ambientMode = (AmbientMode)1;
		((Component)MainContainer).gameObject.SetActive(false);
		((Component)ItemContainer).gameObject.SetActive(activeSelf);
		return result;
	}
}
