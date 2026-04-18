using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class LegacyPackagerLoader : LegacyEmployeeLoader
{
	public override string NPCType => typeof(PackagerData).Name;

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
		Packager packager = employee as Packager;
		if ((Object)(object)packager == (Object)null)
		{
			Console.LogWarning("Failed to cast employee to packager");
			return;
		}
		PackagerConfigurationData configData;
		if (TryLoadFile(mainPath, "Configuration", out var contents))
		{
			configData = JsonUtility.FromJson<PackagerConfigurationData>(contents);
			if (configData != null)
			{
				Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
			}
		}
		PackagerData data;
		if (TryLoadFile(mainPath, "NPC", out var contents2))
		{
			data = null;
			try
			{
				data = JsonUtility.FromJson<PackagerData>(contents2);
			}
			catch (Exception ex)
			{
				Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
			}
			if (data == null)
			{
				Console.LogWarning("Failed to load packager data");
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
			PackagerConfiguration obj = packager.Configuration as PackagerConfiguration;
			obj.Home.Load(configData.Bed);
			obj.Stations.Load(configData.Stations);
			obj.Routes.Load(configData.Routes);
		}
		void LoadConfiguration2()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration2));
			packager.MoveItemBehaviour.Load(data.MoveItemData);
		}
	}
}
