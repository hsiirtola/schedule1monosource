using System;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class ShroomProductLoader : Loader
{
	public override void Load(string mainPath)
	{
		if (TryLoadFile(mainPath, out var contents, autoAddExtension: false))
		{
			ShroomProductData shroomProductData = null;
			try
			{
				shroomProductData = JsonUtility.FromJson<ShroomProductData>(contents);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Error loading product data: " + ex.Message));
			}
			if (shroomProductData != null)
			{
				NetworkSingleton<ProductManager>.Instance.CreateShroom_Server(shroomProductData.Name, shroomProductData.ID, shroomProductData.DrugType, shroomProductData.Properties.ToList(), shroomProductData.AppearanceSettings);
			}
		}
	}
}
