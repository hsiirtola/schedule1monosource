using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class PackagerLoader : EmployeeLoader
{
	public override string NPCType => typeof(PackagerData).Name;

	public override void Load(DynamicSaveData saveData)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		Employee employee = base.CreateAndLoadEmployee(saveData);
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
		if (saveData.TryGetData("Configuration", out PackagerConfigurationData configData))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		if (DynamicLoader.TryExtractBaseData<PackagerData>(saveData, out var data))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration2));
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
