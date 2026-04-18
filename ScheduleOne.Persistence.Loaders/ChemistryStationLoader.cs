using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class ChemistryStationLoader : GridItemLoader
{
	public override string ItemType => typeof(ChemistryStationData).Name;

	public override void Load(string mainPath)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		GridItem gridItem = LoadAndCreate(mainPath);
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		ChemistryStation station = gridItem as ChemistryStation;
		if ((Object)(object)station == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to chemistry station");
			return;
		}
		ChemistryStationData data = GetData<ChemistryStationData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load chemistry station data");
			return;
		}
		data.InputContents.LoadTo(station.ItemSlots);
		data.OutputContents.LoadTo(station.OutputSlot);
		if (data.CurrentRecipeID != string.Empty)
		{
			ChemistryCookOperation operation = new ChemistryCookOperation(data.CurrentRecipeID, data.ProductQuality, data.StartLiquidColor, data.LiquidLevel, data.CurrentTime);
			station.SetCookOperation(null, operation);
		}
		ChemistryStationConfigurationData configData;
		if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
		{
			configData = JsonUtility.FromJson<ChemistryStationConfigurationData>(contents);
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
			(station.Configuration as ChemistryStationConfiguration).Name.Load(configData.Name);
			(station.Configuration as ChemistryStationConfiguration).Recipe.Load(configData.Recipe);
			(station.Configuration as ChemistryStationConfiguration).Destination.Load(configData.Destination);
		}
	}

	public override void Load(DynamicSaveData data)
	{
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
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
		ChemistryStation station = gridItem as ChemistryStation;
		if ((Object)(object)station == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to chemistry station");
			return;
		}
		if (data.TryExtractBaseData<ChemistryStationData>(out var data3))
		{
			if (data3 == null)
			{
				Console.LogWarning("Failed to load chemistry station data");
				return;
			}
			data3.InputContents.LoadTo(station.ItemSlots);
			data3.OutputContents.LoadTo(station.OutputSlot);
			if (data3.CurrentRecipeID != string.Empty)
			{
				ChemistryCookOperation operation = new ChemistryCookOperation(data3.CurrentRecipeID, data3.ProductQuality, data3.StartLiquidColor, data3.LiquidLevel, data3.CurrentTime);
				station.SetCookOperation(null, operation);
			}
		}
		if (data.TryGetData("Configuration", out ChemistryStationConfigurationData configData))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			(station.Configuration as ChemistryStationConfiguration).Name.Load(configData.Name);
			(station.Configuration as ChemistryStationConfiguration).Recipe.Load(configData.Recipe);
			(station.Configuration as ChemistryStationConfiguration).Destination.Load(configData.Destination);
		}
	}
}
