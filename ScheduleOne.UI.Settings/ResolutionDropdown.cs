using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.MainMenu;
using UnityEngine;

namespace ScheduleOne.UI.Settings;

public class ResolutionDropdown : SettingsDropdown
{
	protected virtual void OnEnable()
	{
		ScheduleOne.DevUtilities.Settings instance = Singleton<ScheduleOne.DevUtilities.Settings>.Instance;
		instance.onDisplaySettingsApplied = (Action)Delegate.Combine(instance.onDisplaySettingsApplied, new Action(RegenerateOptions));
		ScheduleOne.DevUtilities.Settings instance2 = Singleton<ScheduleOne.DevUtilities.Settings>.Instance;
		instance2.onUnappliedDisplayIndexChanged = (Action)Delegate.Combine(instance2.onUnappliedDisplayIndexChanged, new Action(RegenerateOptions));
		RegenerateOptions();
	}

	protected virtual void OnDisable()
	{
		ScheduleOne.DevUtilities.Settings instance = Singleton<ScheduleOne.DevUtilities.Settings>.Instance;
		instance.onDisplaySettingsApplied = (Action)Delegate.Remove(instance.onDisplaySettingsApplied, new Action(RegenerateOptions));
		ScheduleOne.DevUtilities.Settings instance2 = Singleton<ScheduleOne.DevUtilities.Settings>.Instance;
		instance2.onUnappliedDisplayIndexChanged = (Action)Delegate.Remove(instance2.onUnappliedDisplayIndexChanged, new Action(RegenerateOptions));
	}

	protected override void OnValueChanged(int value)
	{
		base.OnValueChanged(value);
		Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings.ResolutionIndex = value;
		((Component)this).GetComponentInParent<SettingsScreen>().DisplayChanged();
	}

	private void RegenerateOptions()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		ClearOptions();
		Resolution[] array = DisplaySettings.GetResolutions().ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Resolution val = array[i];
			AddOption(((Resolution)(ref val)).width + "x" + ((Resolution)(ref val)).height);
		}
		SetValueWithoutNotify(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings.ResolutionIndex);
	}
}
