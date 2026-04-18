using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.StationFramework;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class SpawnStationLoader : GridItemLoader
{
	public override string ItemType => typeof(SpawnStationData).Name;

	public override void Load(DynamicSaveData data)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
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
		MushroomSpawnStation station = gridItem as MushroomSpawnStation;
		if ((Object)(object)station == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to spawn station");
			return;
		}
		if (data.TryExtractBaseData<SpawnStationData>(out var data3))
		{
			data3.Contents.LoadTo(station.ItemSlots);
		}
		if (data.TryGetData("Configuration", out SpawnStationConfigurationData configData))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			(station.Configuration as SpawnStationConfiguration).Name.Load(configData.Name);
			(station.Configuration as SpawnStationConfiguration).Destination.Load(configData.Destination);
		}
	}
}
