using System;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Effects;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Product;

[Serializable]
[CreateAssetMenu(fileName = "ShroomDefinition", menuName = "ScriptableObjects/Item Definitions/ShroomDefinition", order = 1)]
public class ShroomDefinition : ProductDefinition
{
	[field: SerializeField]
	public Material ShroomMaterial { get; private set; }

	[field: SerializeField]
	public Material BulkMaterial { get; private set; }

	[field: SerializeField]
	public Material EyeballMaterial { get; private set; }

	public ShroomAppearanceSettings AppearanceSettings { get; private set; }

	public override void ValidateDefinition()
	{
		((BaseItemDefinition)this).ValidateDefinition();
	}

	public void Initialize(List<Effect> properties, List<EDrugType> drugTypes, ShroomAppearanceSettings _appearance)
	{
		Initialize(properties, drugTypes);
		if (_appearance == null || _appearance.IsUnintialized())
		{
			Console.LogWarning("Shroom definition " + ((BaseItemDefinition)this).Name + " has no or uninitialized appearance settings! Generating new");
			GenerateAppearanceSettings();
		}
		else
		{
			AppearanceSettings = _appearance;
		}
		GenerateMaterials();
	}

	public override ItemInstance GetDefaultInstance(int quantity = 1)
	{
		return new ShroomInstance(this, quantity, EQuality.Standard);
	}

	public override ProductData GetSaveData()
	{
		string[] array = new string[Properties.Count];
		for (int i = 0; i < Properties.Count; i++)
		{
			array[i] = Properties[i].ID;
		}
		return new ShroomProductData(((BaseItemDefinition)this).Name, ((BaseItemDefinition)this).ID, DrugTypes[0].DrugType, array, AppearanceSettings);
	}

	public override void GenerateAppearanceSettings()
	{
		base.GenerateAppearanceSettings();
		AppearanceSettings = GetAppearanceSettings(Properties);
		GenerateMaterials();
	}

	private void GenerateMaterials()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		ShroomMaterial = new Material(ShroomMaterial);
		ShroomMaterial.SetColor("_CapInnerColor", Color32.op_Implicit(AppearanceSettings.PrimaryColor));
		ShroomMaterial.SetColor("_CapOuterColor", Color.Lerp(Color32.op_Implicit(AppearanceSettings.PrimaryColor), Color32.op_Implicit(AppearanceSettings.SecondaryColor), 0.7f));
		ShroomMaterial.SetColor("_StemLowerColor", Color32.op_Implicit(AppearanceSettings.PrimaryColor));
		ShroomMaterial.SetColor("_StemUpperColor", Color.Lerp(Color32.op_Implicit(AppearanceSettings.SecondaryColor), Color.white, 0.3f));
		ShroomMaterial.SetColor("_UnderColor", Color.Lerp(Color32.op_Implicit(AppearanceSettings.PrimaryColor), Color32.op_Implicit(new Color32((byte)80, (byte)80, (byte)80, byte.MaxValue)), 0.6f));
		if (AppearanceSettings.HasSpots)
		{
			ShroomMaterial.SetColor("_SpotsColor", Color32.op_Implicit(AppearanceSettings.SpotsColor));
		}
		else
		{
			ShroomMaterial.SetColor("_SpotsColor", Color.clear);
		}
		BulkMaterial = new Material(BulkMaterial);
		BulkMaterial.SetColor("_PrimaryColor", Color32.op_Implicit(AppearanceSettings.PrimaryColor));
		BulkMaterial.SetColor("_SecondaryColor", Color32.op_Implicit(AppearanceSettings.SecondaryColor));
		if (AppearanceSettings.HasSpots)
		{
			BulkMaterial.SetColor("_SpotsColor", Color32.op_Implicit(AppearanceSettings.SpotsColor));
		}
		else
		{
			BulkMaterial.SetColor("_SpotsColor", Color.clear);
		}
	}

	public static ShroomAppearanceSettings GetAppearanceSettings(List<Effect> properties)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		if (properties.Count == 0)
		{
			return new ShroomAppearanceSettings(ShroomAppearanceSettings.DefaultPrimaryColor, ShroomAppearanceSettings.DefaultSecondaryColor, hasSpots: false, Color32.op_Implicit(Color.clear));
		}
		properties.Sort((Effect x, Effect y) => x.Tier.CompareTo(y.Tier));
		Color32 val = Color32.op_Implicit(Color.Lerp(Color32.op_Implicit(ShroomAppearanceSettings.DefaultPrimaryColor), properties[0].ProductColor, Mathf.Clamp01((float)properties[0].Tier / 5f)));
		Color32 secondary = Color32.op_Implicit(Color.Lerp(Color32.op_Implicit(val), Color.white, 0.7f));
		if (properties.Count > 1)
		{
			secondary = Color32.op_Implicit(Color.Lerp(Color32.op_Implicit(ShroomAppearanceSettings.DefaultSecondaryColor), properties[1].ProductColor, Mathf.Clamp01((float)properties[1].Tier / 5f)));
		}
		bool hasSpots = false;
		Color32 spotsColor = ShroomAppearanceSettings.DefaultSpotsColor;
		if (properties.Count > 2)
		{
			hasSpots = true;
			spotsColor = Color32.op_Implicit(Color.Lerp(Color32.op_Implicit(ShroomAppearanceSettings.DefaultSpotsColor), properties[2].ProductColor, Mathf.Clamp01((float)properties[2].Tier / 3f)));
		}
		return new ShroomAppearanceSettings(val, secondary, hasSpots, spotsColor);
	}
}
