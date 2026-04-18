using ScheduleOne.DevUtilities;

namespace ScheduleOne.UI.Settings;

public class AutoBackupSavesToggle : SettingsToggle
{
	protected virtual void Start()
	{
		SetIsOnWithoutNotify(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.OtherSettings.AutoBackupSaves);
	}

	protected override void OnValueChanged(bool value)
	{
		base.OnValueChanged(value);
		Singleton<ScheduleOne.DevUtilities.Settings>.Instance.OtherSettings.AutoBackupSaves = value;
		Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ReloadOtherSettings();
		Singleton<ScheduleOne.DevUtilities.Settings>.Instance.WriteOtherSettings(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.OtherSettings);
	}
}
