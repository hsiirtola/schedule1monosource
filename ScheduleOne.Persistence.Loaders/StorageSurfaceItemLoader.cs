using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class StorageSurfaceItemLoader : SurfaceItemLoader
{
	public override string ItemType => typeof(StorageSurfaceItemData).Name;

	public override void Load(string mainPath)
	{
		SurfaceItem surfaceItem = LoadAndCreate(mainPath);
		if ((Object)(object)surfaceItem == (Object)null)
		{
			Console.LogWarning("Failed to load surface item");
			return;
		}
		SurfaceStorageEntity surfaceStorageEntity = surfaceItem as SurfaceStorageEntity;
		if ((Object)(object)surfaceStorageEntity == (Object)null)
		{
			Console.LogWarning("Failed to cast surface item to storage entity");
			return;
		}
		StorageSurfaceItemData data = GetData<StorageSurfaceItemData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load storage surface item data");
		}
		else
		{
			data.Contents.LoadTo(surfaceStorageEntity.StorageEntity.ItemSlots);
		}
	}

	public override void Load(DynamicSaveData data)
	{
		SurfaceItem surfaceItem = null;
		if (data.TryExtractBaseData<SurfaceItemData>(out var data2))
		{
			surfaceItem = LoadAndCreate(data2);
		}
		if ((Object)(object)surfaceItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		SurfaceStorageEntity surfaceStorageEntity = surfaceItem as SurfaceStorageEntity;
		StorageSurfaceItemData data3;
		if ((Object)(object)surfaceStorageEntity == (Object)null)
		{
			Console.LogWarning("Failed to cast surface item to storage entity");
		}
		else if (data.TryExtractBaseData<StorageSurfaceItemData>(out data3))
		{
			data3.Contents.LoadTo(surfaceStorageEntity.StorageEntity.ItemSlots);
		}
	}
}
