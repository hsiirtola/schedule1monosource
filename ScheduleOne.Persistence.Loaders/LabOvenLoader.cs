using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class LabOvenLoader : GridItemLoader
{
	public override string ItemType => typeof(LabOvenData).Name;

	public override void Load(string mainPath)
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		GridItem gridItem = LoadAndCreate(mainPath);
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		LabOven station = gridItem as LabOven;
		if ((Object)(object)station == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to lab oven");
			return;
		}
		LabOvenData data = GetData<LabOvenData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load lab oven data");
			return;
		}
		data.InputContents.LoadTo(station.ItemSlots);
		data.OutputContents.LoadTo(station.OutputSlot);
		if (data.CurrentIngredientID != string.Empty)
		{
			OvenCookOperation operation = new OvenCookOperation(data.CurrentIngredientID, data.CurrentIngredientQuality, data.CurrentIngredientQuantity, data.CurrentProductID, data.CurrentCookProgress);
			station.SetCookOperation(null, operation, playButtonPress: false);
		}
		LabOvenConfigurationData configData;
		if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
		{
			configData = JsonUtility.FromJson<LabOvenConfigurationData>(contents);
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
			(station.Configuration as LabOvenConfiguration).Name.Load(configData.Name);
			(station.Configuration as LabOvenConfiguration).Destination.Load(configData.Destination);
		}
	}

	public override void Load(DynamicSaveData data)
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Expected O, but got Unknown
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
		LabOven station = gridItem as LabOven;
		if ((Object)(object)station == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to lab oven");
			return;
		}
		if (data.TryExtractBaseData<LabOvenData>(out var data3))
		{
			data3.InputContents.LoadTo(station.ItemSlots);
			data3.OutputContents.LoadTo(station.OutputSlot);
			if (data3.CurrentIngredientID != string.Empty)
			{
				OvenCookOperation operation = new OvenCookOperation(data3.CurrentIngredientID, data3.CurrentIngredientQuality, data3.CurrentIngredientQuantity, data3.CurrentProductID, data3.CurrentCookProgress);
				station.SetCookOperation(null, operation, playButtonPress: false);
			}
		}
		if (data.TryGetData("Configuration", out LabOvenConfigurationData configData))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			(station.Configuration as LabOvenConfiguration).Name.Load(configData.Name);
			(station.Configuration as LabOvenConfiguration).Destination.Load(configData.Destination);
		}
	}
}
