using System;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Effects;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Product;

[Serializable]
[CreateAssetMenu(fileName = "WeedDefinition", menuName = "ScriptableObjects/Item Definitions/WeedDefinition", order = 1)]
public class WeedDefinition : ProductDefinition
{
	[Header("Weed Materials")]
	public Material MainMat;

	public Material SecondaryMat;

	public Material LeafMat;

	public Material StemMat;

	private WeedAppearanceSettings appearance;

	public override ItemInstance GetDefaultInstance(int quantity = 1)
	{
		return new WeedInstance(this, quantity, EQuality.Standard);
	}

	public void Initialize(List<Effect> properties, List<EDrugType> drugTypes, WeedAppearanceSettings _appearance)
	{
		Initialize(properties, drugTypes);
		if (_appearance == null || _appearance.IsUnintialized())
		{
			Console.LogWarning("Weed definition " + ((BaseItemDefinition)this).Name + " has no or uninitialized appearance settings! Generating new");
			GenerateAppearanceSettings();
		}
		else
		{
			appearance = _appearance;
		}
		ApplyAppearanceSettings();
	}

	public override ProductData GetSaveData()
	{
		string[] array = new string[Properties.Count];
		for (int i = 0; i < Properties.Count; i++)
		{
			array[i] = Properties[i].ID;
		}
		return new WeedProductData(((BaseItemDefinition)this).Name, ((BaseItemDefinition)this).ID, DrugTypes[0].DrugType, array, appearance);
	}

	public override void GenerateAppearanceSettings()
	{
		base.GenerateAppearanceSettings();
		appearance = GetAppearanceSettings(Properties);
		ApplyAppearanceSettings();
	}

	private void ApplyAppearanceSettings()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		MainMat = new Material(MainMat);
		MainMat.color = Color32.op_Implicit(appearance.MainColor);
		((Object)MainMat).name = ((BaseItemDefinition)this).Name + "_MainMat";
		SecondaryMat = new Material(SecondaryMat);
		SecondaryMat.color = Color32.op_Implicit(appearance.SecondaryColor);
		((Object)SecondaryMat).name = ((BaseItemDefinition)this).Name + "_SecondaryMat";
		LeafMat = new Material(LeafMat);
		LeafMat.color = Color32.op_Implicit(appearance.LeafColor);
		((Object)LeafMat).name = ((BaseItemDefinition)this).Name + "_LeafMat";
		StemMat = new Material(StemMat);
		StemMat.color = Color32.op_Implicit(appearance.StemColor);
		((Object)StemMat).name = ((BaseItemDefinition)this).Name + "_StemMat";
	}

	public static WeedAppearanceSettings GetAppearanceSettings(List<Effect> properties)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		properties.Sort((Effect x, Effect y) => x.Tier.CompareTo(y.Tier));
		List<Color32> list = new List<Color32>();
		foreach (Effect property in properties)
		{
			list.Add(Color32.op_Implicit(property.ProductColor));
		}
		if (list.Count == 1)
		{
			list.Add(list[0]);
		}
		Color32 val = new Color32((byte)90, (byte)100, (byte)70, byte.MaxValue);
		Color32 val2 = default(Color32);
		((Color32)(ref val2))._002Ector((byte)120, (byte)120, (byte)80, byte.MaxValue);
		Color32 val3 = Color32.Lerp(val, list[0], (float)properties[0].Tier * 0.15f);
		Color32 val4 = Color32.Lerp(val2, Color32.Lerp(list[0], list[1], 0.5f), (properties.Count > 1) ? ((float)properties[1].Tier * 0.2f) : 0.5f);
		Color32 val5 = default(Color32);
		((Color32)(ref val5))._002Ector((byte)0, (byte)0, (byte)0, byte.MaxValue);
		return new WeedAppearanceSettings
		{
			MainColor = val3,
			SecondaryColor = val4,
			LeafColor = Color32.Lerp(val3, val4, 0.5f),
			StemColor = Color32.Lerp(val5, val3, 0.8f)
		};
	}

	public Material GetMaterial(WeedAppearanceSettings.EWeedAppearanceType type)
	{
		return (Material)(type switch
		{
			WeedAppearanceSettings.EWeedAppearanceType.Main => MainMat, 
			WeedAppearanceSettings.EWeedAppearanceType.Secondary => SecondaryMat, 
			WeedAppearanceSettings.EWeedAppearanceType.Leaf => LeafMat, 
			WeedAppearanceSettings.EWeedAppearanceType.Stem => StemMat, 
			_ => null, 
		});
	}
}
