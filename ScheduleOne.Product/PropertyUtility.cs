using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects;
using ScheduleOne.Persistence;
using UnityEngine;

namespace ScheduleOne.Product;

public class PropertyUtility : Singleton<PropertyUtility>
{
	[Serializable]
	public class PropertyData
	{
		public EProperty Property;

		public string Name;

		public string Description;

		public Color Color;
	}

	[Serializable]
	public class DrugTypeData
	{
		public EDrugType DrugType;

		public string Name;

		public Color Color;
	}

	public List<PropertyData> PropertyDatas = new List<PropertyData>();

	public List<DrugTypeData> DrugTypeDatas = new List<DrugTypeData>();

	public List<Effect> AllProperties = new List<Effect>();

	[Header("Test Mixing")]
	public List<ProductDefinition> Products = new List<ProductDefinition>();

	public List<PropertyItemDefinition> Properties = new List<PropertyItemDefinition>();

	private Dictionary<string, Effect> PropertiesDict = new Dictionary<string, Effect>();

	protected override void Awake()
	{
		base.Awake();
		foreach (Effect allProperty in AllProperties)
		{
			PropertiesDict.Add(allProperty.ID, allProperty);
		}
	}

	protected override void Start()
	{
		base.Start();
	}

	public List<Effect> GetProperties(int tier)
	{
		bool excludePostMixingRework = false;
		if (SaveManager.GetVersionNumber(Singleton<MetadataManager>.Instance.CreationVersion) < 27f)
		{
			excludePostMixingRework = true;
		}
		return AllProperties.FindAll((Effect x) => x.Tier == tier && (!excludePostMixingRework || x.ImplementedPriorMixingRework));
	}

	public List<Effect> GetProperties(List<string> ids)
	{
		List<Effect> list = new List<Effect>();
		foreach (string id in ids)
		{
			if ((Object)(object)AllProperties.FirstOrDefault((Effect x) => x.ID == id) == (Object)null)
			{
				Console.LogWarning("PropertyUtility: Property ID '" + id + "' not found!");
			}
			else
			{
				list.Add(PropertiesDict[id]);
			}
		}
		return AllProperties.FindAll((Effect x) => ids.Contains(x.ID));
	}

	public static PropertyData GetPropertyData(EProperty property)
	{
		return Singleton<PropertyUtility>.Instance.PropertyDatas.Find((PropertyData x) => x.Property == property);
	}

	public static DrugTypeData GetDrugTypeData(EDrugType drugType)
	{
		return Singleton<PropertyUtility>.Instance.DrugTypeDatas.Find((DrugTypeData x) => x.DrugType == drugType);
	}

	public static List<Color32> GetOrderedPropertyColors(List<Effect> properties)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		properties.Sort((Effect x, Effect y) => x.Tier.CompareTo(y.Tier));
		List<Color32> list = new List<Color32>();
		foreach (Effect property in properties)
		{
			list.Add(Color32.op_Implicit(property.ProductColor));
		}
		return list;
	}
}
