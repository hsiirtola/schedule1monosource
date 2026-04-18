using ScheduleOne.EntityFramework;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class ToggleableItemLoader : GridItemLoader
{
	public override string ItemType => typeof(ToggleableItemData).Name;

	public override void Load(string mainPath)
	{
		GridItem gridItem = LoadAndCreate(mainPath);
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		ToggleableItemData data = GetData<ToggleableItemData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load toggleableitem data");
			return;
		}
		ToggleableItem toggleableItem = gridItem as ToggleableItem;
		if ((Object)(object)toggleableItem != (Object)null && data.IsOn)
		{
			toggleableItem.TurnOn();
		}
	}

	public override void Load(DynamicSaveData data)
	{
		GridItem gridItem = null;
		if (data.TryExtractBaseData<GridItemData>(out var data2))
		{
			gridItem = LoadAndCreate(data2);
		}
		ToggleableItemData data3;
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
		}
		else if (data.TryExtractBaseData<ToggleableItemData>(out data3))
		{
			ToggleableItem toggleableItem = gridItem as ToggleableItem;
			if ((Object)(object)toggleableItem != (Object)null && data3.IsOn)
			{
				toggleableItem.TurnOn();
			}
		}
	}
}
