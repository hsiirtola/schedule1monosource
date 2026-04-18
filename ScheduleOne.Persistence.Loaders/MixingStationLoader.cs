using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class MixingStationLoader : GridItemLoader
{
	public override string ItemType => typeof(MixingStationData).Name;

	public override void Load(string mainPath)
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		GridItem gridItem = LoadAndCreate(mainPath);
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		MixingStation station = gridItem as MixingStation;
		if ((Object)(object)station == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to mixing station");
			return;
		}
		MixingStationData data = GetData<MixingStationData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load mixing station data");
			return;
		}
		data.ProductContents.LoadTo(station.ProductSlot);
		data.MixerContents.LoadTo(station.MixerSlot);
		data.OutputContents.LoadTo(station.OutputSlot);
		if (data.CurrentMixOperation != null)
		{
			station.SetMixOperation(null, data.CurrentMixOperation, data.CurrentMixTime);
		}
		MixingStationConfigurationData configData;
		if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
		{
			configData = JsonUtility.FromJson<MixingStationConfigurationData>(contents);
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
			(station.Configuration as MixingStationConfiguration).Name.Load(configData.Name);
			(station.Configuration as MixingStationConfiguration).Destination.Load(configData.Destination);
			(station.Configuration as MixingStationConfiguration).StartThrehold.Load(configData.Threshold);
		}
	}

	public override void Load(DynamicSaveData data)
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
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
		MixingStation station = gridItem as MixingStation;
		if ((Object)(object)station == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to mixing station");
			return;
		}
		if (data.TryExtractBaseData<MixingStationData>(out var data3))
		{
			data3.ProductContents.LoadTo(station.ProductSlot);
			data3.MixerContents.LoadTo(station.MixerSlot);
			data3.OutputContents.LoadTo(station.OutputSlot);
			if (data3.CurrentMixOperation != null)
			{
				station.SetMixOperation(null, data3.CurrentMixOperation, data3.CurrentMixTime);
			}
		}
		if (data.TryGetData("Configuration", out MixingStationConfigurationData configData))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			(station.Configuration as MixingStationConfiguration).Name.Load(configData.Name);
			(station.Configuration as MixingStationConfiguration).Destination.Load(configData.Destination);
			(station.Configuration as MixingStationConfiguration).StartThrehold.Load(configData.Threshold);
		}
	}
}
