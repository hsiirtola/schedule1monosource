using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using UnityEngine;

namespace ScheduleOne.Persistence.ItemLoaders;

public class ShroomLoader : ItemLoader
{
	public override string ItemType => typeof(ShroomData).Name;

	public override ItemInstance LoadItem(string itemString)
	{
		ShroomData shroomData = LoadData<ShroomData>(itemString);
		if (shroomData == null)
		{
			Console.LogWarning("Failed loading item data from " + itemString);
			return null;
		}
		if (shroomData.ID == string.Empty)
		{
			return null;
		}
		ItemDefinition item = Registry.GetItem(shroomData.ID);
		if ((Object)(object)item == (Object)null)
		{
			Console.LogWarning("Failed to find item definition for " + shroomData.ID);
			return null;
		}
		EQuality result;
		EQuality quality = (Enum.TryParse<EQuality>(shroomData.Quality, out result) ? result : EQuality.Standard);
		PackagingDefinition packaging = null;
		if (shroomData.PackagingID != string.Empty)
		{
			ItemDefinition item2 = Registry.GetItem(shroomData.PackagingID);
			if ((Object)(object)item != (Object)null)
			{
				packaging = item2 as PackagingDefinition;
			}
		}
		return new ShroomInstance(item, shroomData.Quantity, quality, packaging);
	}
}
