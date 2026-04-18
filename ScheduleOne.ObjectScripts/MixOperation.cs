using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.ObjectScripts;

[Serializable]
public class MixOperation
{
	public string ProductID;

	public EQuality ProductQuality;

	public string IngredientID;

	public int Quantity;

	public MixOperation(string productID, EQuality productQuality, string ingredientID, int quantity)
	{
		ProductID = productID;
		ProductQuality = productQuality;
		IngredientID = ingredientID;
		Quantity = quantity;
	}

	public MixOperation()
	{
	}

	public EDrugType GetOutput(out List<Effect> properties)
	{
		ProductDefinition item = Registry.GetItem<ProductDefinition>(ProductID);
		if ((Object)(object)item == (Object)null)
		{
			Console.LogError("MixOperation GetOutput: Invalid product ID: " + ProductID);
			properties = new List<Effect>();
			return EDrugType.Marijuana;
		}
		PropertyItemDefinition item2 = Registry.GetItem<PropertyItemDefinition>(IngredientID);
		if ((Object)(object)item2 == (Object)null || item2.Properties.Count == 0)
		{
			Console.LogError("MixOperation GetOutput: Invalid ingredient ID or no properties: " + IngredientID);
			properties = new List<Effect>();
			return EDrugType.Marijuana;
		}
		properties = EffectMixCalculator.MixProperties(item.Properties, item2.Properties[0], item.DrugType);
		return item.DrugType;
	}

	public bool IsOutputKnown(out ProductDefinition knownProduct)
	{
		List<Effect> properties;
		EDrugType output = GetOutput(out properties);
		knownProduct = NetworkSingleton<ProductManager>.Instance.GetKnownProduct(output, properties);
		return (Object)(object)knownProduct != (Object)null;
	}
}
