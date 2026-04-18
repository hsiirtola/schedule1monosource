using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class DryingRackLoader : GridItemLoader
{
	public override string ItemType => typeof(DryingRackData).Name;

	public override void Load(string mainPath)
	{
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Expected O, but got Unknown
		GridItem gridItem = LoadAndCreate(mainPath);
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		DryingRack station = gridItem as DryingRack;
		if ((Object)(object)station == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to DryingRack");
			return;
		}
		DryingRackData data = GetData<DryingRackData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load DryingRack data");
			return;
		}
		data.Input.LoadTo(station.InputSlot);
		data.Output.LoadTo(station.OutputSlot);
		for (int i = 0; i < data.DryingOperations.Length; i++)
		{
			if (data.DryingOperations[i] != null && data.DryingOperations[i].Quantity > 0 && !string.IsNullOrEmpty(data.DryingOperations[i].ItemID))
			{
				station.DryingOperations.Add(data.DryingOperations[i]);
			}
		}
		station.RefreshHangingVisuals();
		DryingRackConfigurationData configData;
		if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
		{
			configData = JsonUtility.FromJson<DryingRackConfigurationData>(contents);
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
			(station.Configuration as DryingRackConfiguration).Name.Load(configData.Name);
			(station.Configuration as DryingRackConfiguration).TargetQuality.Load(configData.TargetQuality);
			(station.Configuration as DryingRackConfiguration).Destination.Load(configData.Destination);
		}
	}

	public override void Load(DynamicSaveData data)
	{
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected O, but got Unknown
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
		DryingRack station = gridItem as DryingRack;
		if ((Object)(object)station == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to DryingRack");
			return;
		}
		if (data.TryExtractBaseData<DryingRackData>(out var data3))
		{
			data3.Input.LoadTo(station.InputSlot);
			data3.Output.LoadTo(station.OutputSlot);
			for (int i = 0; i < data3.DryingOperations.Length; i++)
			{
				if (data3.DryingOperations[i] != null && data3.DryingOperations[i].Quantity > 0 && !string.IsNullOrEmpty(data3.DryingOperations[i].ItemID))
				{
					station.DryingOperations.Add(data3.DryingOperations[i]);
				}
			}
			station.RefreshHangingVisuals();
		}
		if (data.TryGetData("Configuration", out DryingRackConfigurationData configData))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			(station.Configuration as DryingRackConfiguration).Name.Load(configData.Name);
			(station.Configuration as DryingRackConfiguration).TargetQuality.Load(configData.TargetQuality);
			(station.Configuration as DryingRackConfiguration).Destination.Load(configData.Destination);
			(station.Configuration as DryingRackConfiguration).StartThreshold.Load(configData.StartThreshold);
		}
	}
}
