using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class CleanerLoader : EmployeeLoader
{
	public override string NPCType => typeof(CleanerData).Name;

	public override void Load(DynamicSaveData saveData)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		Employee employee = base.CreateAndLoadEmployee(saveData);
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
		if (saveData.TryGetData("Configuration", out CleanerConfigurationData configData) && configData != null)
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		if (DynamicLoader.TryExtractBaseData<CleanerData>(saveData, out var data))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration2));
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
