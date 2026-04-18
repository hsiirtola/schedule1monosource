using ScheduleOne.DevUtilities;
using ScheduleOne.UI.MainMenu;
using UnityEngine;

namespace ScheduleOne.UI.Settings;

public class VSyncToggle : SettingsToggle
{
	protected virtual void OnEnable()
	{
		SetIsOnWithoutNotify(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.DisplaySettings.VSync);
	}

	protected override void OnValueChanged(bool value)
	{
		base.OnValueChanged(value);
		Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings.VSync = value;
		((Component)this).GetComponentInParent<SettingsScreen>().DisplayChanged();
	}
}
