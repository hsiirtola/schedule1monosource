using ScheduleOne.DevUtilities;
using ScheduleOne.UI.MainMenu;
using UnityEngine;

namespace ScheduleOne.UI.Settings;

public class UnitsModeDropdown : SettingsDropdown
{
	protected override void Awake()
	{
		base.Awake();
		AddOption("Metric");
		AddOption("Imperial");
	}

	protected virtual void OnEnable()
	{
		SetValueWithoutNotify((int)Singleton<ScheduleOne.DevUtilities.Settings>.Instance.DisplaySettings.UnitType);
	}

	protected override void OnValueChanged(int value)
	{
		base.OnValueChanged(value);
		Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings.UnitType = ((value != 0) ? ScheduleOne.DevUtilities.Settings.EUnitType.Imperial : ScheduleOne.DevUtilities.Settings.EUnitType.Metric);
		((Component)this).GetComponentInParent<SettingsScreen>().DisplayChanged();
	}
}
