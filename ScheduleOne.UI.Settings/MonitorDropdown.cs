using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.MainMenu;
using UnityEngine;

namespace ScheduleOne.UI.Settings;

public class MonitorDropdown : SettingsDropdown
{
	protected override void Awake()
	{
		base.Awake();
		Display[] displays = Display.displays;
		for (int i = 0; i < displays.Length; i++)
		{
			AddOption("Monitor " + (i + 1));
		}
	}

	protected virtual void OnEnable()
	{
		ScheduleOne.DevUtilities.Settings instance = Singleton<ScheduleOne.DevUtilities.Settings>.Instance;
		instance.onDisplaySettingsApplied = (Action)Delegate.Combine(instance.onDisplaySettingsApplied, new Action(SetCurrent));
		SetCurrent();
	}

	protected virtual void OnDisable()
	{
		ScheduleOne.DevUtilities.Settings instance = Singleton<ScheduleOne.DevUtilities.Settings>.Instance;
		instance.onDisplaySettingsApplied = (Action)Delegate.Remove(instance.onDisplaySettingsApplied, new Action(SetCurrent));
	}

	protected override void OnValueChanged(int value)
	{
		base.OnValueChanged(value);
		Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings.ActiveDisplayIndex = value;
		if (Singleton<ScheduleOne.DevUtilities.Settings>.Instance.onUnappliedDisplayIndexChanged != null)
		{
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.onUnappliedDisplayIndexChanged();
		}
		((Component)this).GetComponentInParent<SettingsScreen>().DisplayChanged();
	}

	public static int GetCurrentDisplayNumber()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		List<DisplayInfo> list = new List<DisplayInfo>();
		Screen.GetDisplayLayout(list);
		return list.IndexOf(Screen.mainWindowDisplayInfo);
	}

	private void SetCurrent()
	{
		Debug.Log((object)("Active Monitor Index: " + GetCurrentDisplayNumber()));
		SetValueWithoutNotify(GetCurrentDisplayNumber());
	}
}
