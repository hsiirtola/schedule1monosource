using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class BrickPressLoader : GridItemLoader
{
	public override string ItemType => typeof(BrickPressData).Name;

	public override void Load(string mainPath)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		GridItem gridItem = LoadAndCreate(mainPath);
		if ((Object)(object)gridItem == (Object)null)
		{
			Console.LogWarning("Failed to load grid item");
			return;
		}
		BrickPress brickPress = gridItem as BrickPress;
		if ((Object)(object)brickPress == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to brick press");
			return;
		}
		BrickPressData data = GetData<BrickPressData>(mainPath);
		if (data == null)
		{
			Console.LogWarning("Failed to load brick press data");
			return;
		}
		data.Contents.LoadTo(brickPress.ItemSlots);
		BrickPressConfigurationData configData;
		if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
		{
			configData = JsonUtility.FromJson<BrickPressConfigurationData>(contents);
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
			(brickPress.Configuration as BrickPressConfiguration).Name.Load(configData.Name);
			(brickPress.Configuration as BrickPressConfiguration).Destination.Load(configData.Destination);
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
		BrickPress brickPress = gridItem as BrickPress;
		if ((Object)(object)brickPress == (Object)null)
		{
			Console.LogWarning("Failed to cast grid item to brick press");
			return;
		}
		if (data.TryExtractBaseData<BrickPressData>(out var data3))
		{
			data3.Contents.LoadTo(brickPress.ItemSlots);
		}
		if (data.TryGetData("Configuration", out BrickPressConfigurationData configData))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			(brickPress.Configuration as BrickPressConfiguration).Name.Load(configData.Name);
			(brickPress.Configuration as BrickPressConfiguration).Destination.Load(configData.Destination);
		}
	}
}
