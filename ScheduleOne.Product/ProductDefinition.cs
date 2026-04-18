using System;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.Product.Packaging;
using ScheduleOne.StationFramework;
using UnityEngine;

namespace ScheduleOne.Product;

[Serializable]
[CreateAssetMenu(fileName = "ProductDefinition", menuName = "ScriptableObjects/ProductDefinition", order = 1)]
public class ProductDefinition : PropertyItemDefinition, ISaveable
{
	[Header("Product Settings")]
	public List<DrugTypeContainer> DrugTypes;

	public float LawIntensityChange = 1f;

	public float BasePrice = 1f;

	public float MarketValue = 1f;

	public FunctionalProduct FunctionalProduct;

	public int NPCEffectDuration = 180;

	public int PlayerEffectDuration = 180;

	[Range(0f, 1f)]
	public float BaseAddictiveness;

	[Header("Packaging that can be applied to this product. MUST BE ORDERED FROm LOWEST TO HIGHEST QUANTITY")]
	public PackagingDefinition[] ValidPackaging;

	[Header("Product References")]
	public ProductConsumeAnimation ConsumeAnimation;

	public EDrugType DrugType => DrugTypes[0].DrugType;

	public float Price => NetworkSingleton<ProductManager>.Instance.GetPrice(this);

	public List<StationRecipe> Recipes { get; private set; } = new List<StationRecipe>();

	public string SaveFolderName => SaveManager.SanitizeFileName(((BaseItemDefinition)this).ID);

	public string SaveFileName => SaveManager.SanitizeFileName(((BaseItemDefinition)this).ID);

	public Loader Loader => null;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public override ItemInstance GetDefaultInstance(int quantity = 1)
	{
		return new ProductItemInstance(this, quantity, EQuality.Standard);
	}

	public void OnValidate()
	{
		MarketValue = ProductManager.CalculateProductValue(this, BasePrice);
		CleanRecipes();
	}

	public void Initialize(List<Effect> properties, List<EDrugType> drugTypes)
	{
		base.Initialize(properties);
		DrugTypes = new List<DrugTypeContainer>();
		for (int i = 0; i < drugTypes.Count; i++)
		{
			DrugTypes.Add(new DrugTypeContainer
			{
				DrugType = drugTypes[i]
			});
		}
		CleanRecipes();
		MarketValue = ProductManager.CalculateProductValue(this, BasePrice);
		InitializeSaveable();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public float GetAddictiveness()
	{
		float num = BaseAddictiveness;
		for (int i = 0; i < Properties.Count; i++)
		{
			num += Properties[i].Addictiveness;
		}
		return Mathf.Clamp01(num);
	}

	public void CleanRecipes()
	{
		for (int num = Recipes.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)Recipes[num] == (Object)null)
			{
				Recipes.RemoveAt(num);
			}
		}
	}

	public void AddRecipe(StationRecipe recipe)
	{
		if ((Object)(object)recipe.Product.Item != (Object)(object)this)
		{
			Debug.LogError((object)"Recipe product does not match this product.");
		}
		else if (!Recipes.Contains(recipe))
		{
			Recipes.Add(recipe);
		}
	}

	public virtual void GenerateAppearanceSettings()
	{
	}

	public virtual ProductData GetSaveData()
	{
		string[] array = new string[Properties.Count];
		for (int i = 0; i < Properties.Count; i++)
		{
			array[i] = Properties[i].ID;
		}
		return new ProductData(((BaseItemDefinition)this).Name, ((BaseItemDefinition)this).ID, DrugTypes[0].DrugType, array);
	}

	public virtual string GetSaveString()
	{
		return GetSaveData().GetJson();
	}
}
