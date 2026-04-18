using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class PackagingStationLoader : GridItemLoader
{
	public override string ItemType => typeof(PackagingStationData).Name;

	public override void Load(string mainPath)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		GridItem gridItem = LoadAndCreate(mainPath);
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		PackagingStation station = gridItem as PackagingStation;
		if ((Object)(object)station == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to pot");
			return;
		}
		PackagingStationData data = GetData<PackagingStationData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load packaging station data data");
			return;
		}
		data.Contents.LoadTo(station.ItemSlots);
		station.UpdatePackagingVisuals();
		station.UpdateProductVisuals();
		PackagingStationConfigurationData configData;
		if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
		{
			configData = JsonUtility.FromJson<PackagingStationConfigurationData>(contents);
			if (configData != null)
			{
				Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
			}
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			(station.Configuration as PackagingStationConfiguration).Name.Load(configData.Name);
			(station.Configuration as PackagingStationConfiguration).Destination.Load(configData.Destination);
		}
	}

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
		PackagingStation station = gridItem as PackagingStation;
		if ((Object)(object)station == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to pot");
			return;
		}
		if (data.TryExtractBaseData<PackagingStationData>(out var data3))
		{
			data3.Contents.LoadTo(station.ItemSlots);
		}
		if (data.TryGetData("Configuration", out PackagingStationConfigurationData configData))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			(station.Configuration as PackagingStationConfiguration).Name.Load(configData.Name);
			(station.Configuration as PackagingStationConfiguration).Destination.Load(configData.Destination);
		}
	}
}
