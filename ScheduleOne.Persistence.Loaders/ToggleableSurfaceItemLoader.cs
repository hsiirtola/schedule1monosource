using ScheduleOne.EntityFramework;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class ToggleableSurfaceItemLoader : SurfaceItemLoader
{
	public override string ItemType => typeof(ToggleableSurfaceItemData).Name;

	public override void Load(string mainPath)
	{
		SurfaceItem surfaceItem = LoadAndCreate(mainPath);
		if ((Object)(object)surfaceItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		ToggleableSurfaceItemData data = GetData<ToggleableSurfaceItemData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load ToggleableSurfaceItemData");
			return;
		}
		ToggleableSurfaceItem toggleableSurfaceItem = surfaceItem as ToggleableSurfaceItem;
		if ((Object)(object)toggleableSurfaceItem != (Object)null && data.IsOn)
		{
			toggleableSurfaceItem.TurnOn();
		}
	}

	public override void Load(DynamicSaveData data)
	{
		SurfaceItem surfaceItem = null;
		if (data.TryExtractBaseData<SurfaceItemData>(out var data2))
		{
			surfaceItem = LoadAndCreate(data2);
		}
		ToggleableSurfaceItemData data3;
		if ((Object)(object)surfaceItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
		}
		else if (data.TryExtractBaseData<ToggleableSurfaceItemData>(out data3))
		{
			ToggleableSurfaceItem toggleableSurfaceItem = surfaceItem as ToggleableSurfaceItem;
			if ((Object)(object)toggleableSurfaceItem != (Object)null && data3.IsOn)
			{
				toggleableSurfaceItem.TurnOn();
			}
		}
	}
}
