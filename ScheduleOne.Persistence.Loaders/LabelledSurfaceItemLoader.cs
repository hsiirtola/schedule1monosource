using ScheduleOne.EntityFramework;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class LabelledSurfaceItemLoader : SurfaceItemLoader
{
	public override string ItemType => typeof(LabelledSurfaceItemData).Name;

	public override void Load(string mainPath)
	{
		SurfaceItem surfaceItem = LoadAndCreate(mainPath);
		if ((Object)(object)surfaceItem == (Object)null)
		{
			Console.LogWarning("Failed to load surface item");
			return;
		}
		LabelledSurfaceItemData data = GetData<LabelledSurfaceItemData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load LabelledSurfaceItemData");
			return;
		}
		LabelledSurfaceItem labelledSurfaceItem = surfaceItem as LabelledSurfaceItem;
		if ((Object)(object)labelledSurfaceItem != (Object)null)
		{
			labelledSurfaceItem.SetMessage(null, data.Message);
		}
	}

	public override void Load(DynamicSaveData data)
	{
		SurfaceItem surfaceItem = null;
		if (data.TryExtractBaseData<SurfaceItemData>(out var data2))
		{
			surfaceItem = LoadAndCreate(data2);
		}
		LabelledSurfaceItemData data3;
		if ((Object)(object)surfaceItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
		}
		else if (data.TryExtractBaseData<LabelledSurfaceItemData>(out data3))
		{
			LabelledSurfaceItem labelledSurfaceItem = surfaceItem as LabelledSurfaceItem;
			if ((Object)(object)labelledSurfaceItem != (Object)null)
			{
				labelledSurfaceItem.SetMessage(null, data3.Message);
			}
		}
	}
}
