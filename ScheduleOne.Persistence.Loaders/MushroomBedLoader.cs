using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class MushroomBedLoader : GridItemLoader
{
	public override string ItemType => typeof(MushroomBedData).Name;

	public override void Load(DynamicSaveData data)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
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
		MushroomBed mushroomBed = gridItem as MushroomBed;
		if ((Object)(object)mushroomBed == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to msuhroom bed");
			return;
		}
		if (data.TryExtractBaseData<MushroomBedData>(out var data3))
		{
			mushroomBed.Load(data3);
		}
		if (data.TryGetData("Configuration", out MushroomBedConfigurationData configData))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			MushroomBedConfiguration obj = mushroomBed.Configuration as MushroomBedConfiguration;
			obj.Name.Load(configData.Name);
			obj.Spawn.Load(configData.Spawn);
			obj.Additive1.Load(configData.Additive1);
			obj.Additive2.Load(configData.Additive2);
			obj.Additive3.Load(configData.Additive3);
			obj.Destination.Load(configData.Destination);
		}
	}
}
