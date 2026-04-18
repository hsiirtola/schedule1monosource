using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence.Loaders;

public class BotanistLoader : EmployeeLoader
{
	public override string NPCType => typeof(BotanistData).Name;

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
		Botanist botanist = employee as Botanist;
		if ((Object)(object)botanist == (Object)null)
		{
			Console.LogWarning("Failed to cast employee to botanist");
			return;
		}
		if (saveData.TryGetData("Configuration", out BotanistConfigurationData configData) && configData != null)
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration));
		}
		if (DynamicLoader.TryExtractBaseData<BotanistData>(saveData, out var data))
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(LoadConfiguration2));
		}
		void LoadConfiguration()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration));
			BotanistConfiguration obj = botanist.Configuration as BotanistConfiguration;
			obj.Home.Load(configData.Bed);
			obj.Supplies.Load(configData.Supplies);
			obj.Assigns.Load(configData.Pots);
		}
		void LoadConfiguration2()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(LoadConfiguration2));
			botanist.MoveItemBehaviour.Load(data.MoveItemData);
		}
	}
}
