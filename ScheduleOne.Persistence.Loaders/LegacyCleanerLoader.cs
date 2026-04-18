using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class LegacyCleanerLoader : LegacyEmployeeLoader
{
	public override string NPCType => typeof(CleanerData).Name;

	public override void Load(string mainPath)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Expected O, but got Unknown
		Employee employee = LoadAndCreateEmployee(mainPath);
		if ((Object)(object)employee == (Object)null)
		{
			return;
		}
		Cleaner cleaner = employee as Cleaner;
		if ((Object)(object)cleaner == (Object)null)
		{
			Console.LogWarning("Failed to cast employee to Cleaner");
			return;
		}
		CleanerConfigurationData configData;
		if (TryLoadFile(mainPath, "Configuration", out var contents))
		{
			configData = JsonUtility.FromJson<CleanerConfigurationData>(contents);
			if (configData != null)
			{
				Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
			}
		}
		CleanerData data;
		if (TryLoadFile(mainPath, "NPC", out var contents2))
		{
			data = null;
			try
			{
				data = JsonUtility.FromJson<CleanerData>(contents2);
			}
			catch (Exception ex)
			{
				Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
			}
			if (data == null)
			{
				Console.LogWarning("Failed to load cleaner data");
			}
			else
			{
				Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration2));
			}
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			CleanerConfiguration obj = cleaner.Configuration as CleanerConfiguration;
			obj.Home.Load(configData.Bed);
			obj.Bins.Load(configData.Bins);
		}
		void LoadConfiguration2()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration2));
			cleaner.MoveItemBehaviour.Load(data.MoveItemData);
		}
	}
}
