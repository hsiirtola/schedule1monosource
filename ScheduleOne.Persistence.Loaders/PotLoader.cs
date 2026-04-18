using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class PotLoader : GridItemLoader
{
	public override string ItemType => typeof(PotData).Name;

	public override void Load(string mainPath)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		GridItem gridItem = LoadAndCreate(mainPath);
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		Pot pot = gridItem as Pot;
		if ((Object)(object)pot == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to pot");
			return;
		}
		PotData data = GetData<PotData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load pot data");
			return;
		}
		pot.Load(data);
		PotConfigurationData configData;
		if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
		{
			configData = JsonUtility.FromJson<PotConfigurationData>(contents);
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
			PotConfiguration obj = pot.Configuration as PotConfiguration;
			obj.Name.Load(configData.Name);
			obj.Seed.Load(configData.Seed);
			obj.Additive1.Load(configData.Additive1);
			obj.Additive2.Load(configData.Additive2);
			obj.Additive3.Load(configData.Additive3);
			obj.Destination.Load(configData.Destination);
		}
	}

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
		Pot pot = gridItem as Pot;
		if ((Object)(object)pot == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to pot");
			return;
		}
		if (data.TryExtractBaseData<PotData>(out var data3))
		{
			pot.Load(data3);
		}
		if (data.TryGetData("Configuration", out PotConfigurationData configData))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			PotConfiguration obj = pot.Configuration as PotConfiguration;
			obj.Name.Load(configData.Name);
			obj.Seed.Load(configData.Seed);
			obj.Additive1.Load(configData.Additive1);
			obj.Additive2.Load(configData.Additive2);
			obj.Additive3.Load(configData.Additive3);
			obj.Destination.Load(configData.Destination);
		}
	}
}
