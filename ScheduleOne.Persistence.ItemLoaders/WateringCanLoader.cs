using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.ItemLoaders;

public class WateringCanLoader : ItemLoader
{
	public override string ItemType => typeof(WateringCanData).Name;

	public override ItemInstance LoadItem(string itemString)
	{
		WateringCanData wateringCanData = LoadData<WateringCanData>(itemString);
		if (wateringCanData == null)
		{
			Console.LogWarning("Failed loading item data from " + itemString);
			return null;
		}
		if (wateringCanData.ID == string.Empty)
		{
			return null;
		}
		ItemDefinition item = Registry.GetItem(wateringCanData.ID);
		if ((Object)(object)item == (Object)null)
		{
			Console.LogWarning("Failed to find item definition for " + wateringCanData.ID);
			return null;
		}
		return new WaterContainerInstance(item, wateringCanData.Quantity, wateringCanData.CurrentFillAmount);
	}
}
