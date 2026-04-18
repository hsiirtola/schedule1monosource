using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class CauldronLoader : GridItemLoader
{
	public override string ItemType => typeof(CauldronData).Name;

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
		Cauldron station = gridItem as Cauldron;
		if ((Object)(object)station == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to Cauldron");
			return;
		}
		CauldronData data = GetData<CauldronData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load cauldron data");
			return;
		}
		data.Ingredients.LoadTo(station.IngredientSlots);
		data.Liquid.LoadTo(station.LiquidSlot);
		data.Output.LoadTo(station.OutputSlot);
		if (data.RemainingCookTime > 0)
		{
			station.StartCookOperation(null, data.RemainingCookTime, data.InputQuality);
		}
		CauldronConfigurationData configData;
		if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
		{
			configData = JsonUtility.FromJson<CauldronConfigurationData>(contents);
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
			(station.Configuration as CauldronConfiguration).Name.Load(configData.Name);
			(station.Configuration as CauldronConfiguration).Destination.Load(configData.Destination);
		}
	}

	public override void Load(DynamicSaveData data)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
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
		Cauldron station = gridItem as Cauldron;
		if ((Object)(object)station == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to Cauldron");
			return;
		}
		if (data.TryExtractBaseData<CauldronData>(out var data3))
		{
			if (data3 == null)
			{
				Console.LogWarning("Failed to load cauldron data");
				return;
			}
			data3.Ingredients.LoadTo(station.IngredientSlots);
			data3.Liquid.LoadTo(station.LiquidSlot);
			data3.Output.LoadTo(station.OutputSlot);
			if (data3.RemainingCookTime > 0)
			{
				station.StartCookOperation(null, data3.RemainingCookTime, data3.InputQuality);
			}
		}
		if (data.TryGetData("Configuration", out CauldronConfigurationData configData))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			(station.Configuration as CauldronConfiguration).Name.Load(configData.Name);
			(station.Configuration as CauldronConfiguration).Destination.Load(configData.Destination);
		}
	}
}
