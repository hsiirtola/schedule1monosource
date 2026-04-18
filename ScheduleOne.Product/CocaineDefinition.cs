using System;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Effects;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Product;

[Serializable]
[CreateAssetMenu(fileName = "CocaineDefinition", menuName = "ScriptableObjects/Item Definitions/CocaineDefinition", order = 1)]
public class CocaineDefinition : ProductDefinition
{
	[Header("Materials")]
	public Material RockMaterial;

	public CocaineAppearanceSettings AppearanceSettings { get; private set; }

	public override ItemInstance GetDefaultInstance(int quantity = 1)
	{
		return new CocaineInstance(this, quantity, EQuality.Standard);
	}

	public void Initialize(List<Effect> properties, List<EDrugType> drugTypes, CocaineAppearanceSettings _appearance)
	{
		Initialize(properties, drugTypes);
		if (_appearance == null || _appearance.IsUnintialized())
		{
			Console.LogWarning("Coke definition " + ((BaseItemDefinition)this).Name + " has no or uninitialized appearance settings! Generating new");
			GenerateAppearanceSettings();
		}
		else
		{
			AppearanceSettings = _appearance;
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
		return new CocaineProductData(((BaseItemDefinition)this).Name, ((BaseItemDefinition)this).ID, DrugTypes[0].DrugType, array, AppearanceSettings);
	}

	public override void GenerateAppearanceSettings()
	{
		base.GenerateAppearanceSettings();
		AppearanceSettings = GetAppearanceSettings(Properties);
		ApplyAppearanceSettings();
	}

	private void ApplyAppearanceSettings()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		RockMaterial = new Material(RockMaterial);
		RockMaterial.color = Color32.op_Implicit(AppearanceSettings.MainColor);
		((Object)RockMaterial).name = ((BaseItemDefinition)this).Name + "_RockMat";
	}

	public static CocaineAppearanceSettings GetAppearanceSettings(List<Effect> properties)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
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
		Color32 val = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		Color32 val2 = default(Color32);
		((Color32)(ref val2))._002Ector(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		Color32 mainColor = Color32.Lerp(val, list[0], (float)properties[0].Tier * 0.13f);
		Color32 secondaryColor = Color32.Lerp(val2, Color32.Lerp(list[0], list[1], 0.5f), (properties.Count > 1) ? ((float)properties[1].Tier * 0.2f) : 0.5f);
		return new CocaineAppearanceSettings
		{
			MainColor = mainColor,
			SecondaryColor = secondaryColor
		};
	}
}
