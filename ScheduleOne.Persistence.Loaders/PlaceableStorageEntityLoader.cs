using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class PlaceableStorageEntityLoader : GridItemLoader
{
	public override string ItemType => typeof(PlaceableStorageData).Name;

	public override void Load(string mainPath)
	{
		GridItem gridItem = LoadAndCreate(mainPath);
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		PlaceableStorageEntity placeableStorageEntity = gridItem as PlaceableStorageEntity;
		if ((Object)(object)placeableStorageEntity == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to rack");
			return;
		}
		PlaceableStorageData data = GetData<PlaceableStorageData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load storage rack data");
		}
		else
		{
			data.Contents.LoadTo(placeableStorageEntity.StorageEntity.ItemSlots);
		}
	}

	public override void Load(DynamicSaveData data)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		GridItem gridItem = null;
		if (data.TryExtractBaseData<GridItemData>(out var data2))
		{
			gridItem = LoadAndCreate(data2);
		}
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		PlaceableStorageEntity entity = gridItem as PlaceableStorageEntity;
		if ((Object)(object)entity == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to rack");
			return;
		}
		if (data.TryExtractBaseData<PlaceableStorageData>(out var data3))
		{
			data3.Contents.LoadTo(entity.StorageEntity.ItemSlots);
		}
		if (data.TryGetData("Configuration", out RenamableConfigurationData configData))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			entity.Configuration.Name.Load(configData.Name);
		}
	}
}
