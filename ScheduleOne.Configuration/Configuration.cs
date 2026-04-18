using System.Linq;
using ScheduleOne.Core.Settings.Framework;
using UnityEngine;

namespace ScheduleOne.Configuration;

public abstract class Configuration<T> : BaseConfiguration where T : Settings
{
	public T Settings { get; private set; }

	private T DefaultSettings { get; set; }

	public override void ValidateConfiguration()
	{
		base.ValidateConfiguration();
		SettingsObject[] settingsObjects = ((Settings)DefaultSettings).GetSettingsObjects();
		int i;
		for (i = 0; i < settingsObjects.Length; i++)
		{
			if (settingsObjects.Count((SettingsObject x) => x.Name == settingsObjects[i].Name) > 1)
			{
				Debug.LogError((object)("Duplicate SettingsObject name '" + settingsObjects[i].Name + "' found in DefaultSettings of configuration " + ((Object)this).name + ". Each SettingsObject must have a unique name."), (Object)(object)DefaultSettings);
			}
		}
	}

	public override void ResetConfigurationToDefault()
	{
		Object.Destroy((Object)(object)Settings);
		Settings = Object.Instantiate<T>(DefaultSettings);
	}

	public override Settings GetSettings()
	{
		return (Settings)(object)Settings;
	}

	public void ApplySettings(T newSettings)
	{
		if ((Object)(object)newSettings == (Object)null)
		{
			Debug.LogError((object)("Cannot apply null settings to configuration " + ((Object)this).name));
			return;
		}
		ApplyOverwrites(newSettings, Settings);
		OnConfigurationChanged?.Invoke(this);
	}

	private static void ApplyOverwrites(T from, T to)
	{
		SettingsObject[] settingsObjects = ((Settings)from).GetSettingsObjects();
		SettingsObject[] settingsObjects2 = ((Settings)to).GetSettingsObjects();
		foreach (SettingsObject toObj in settingsObjects2)
		{
			SettingsObject val = settingsObjects.FirstOrDefault((SettingsObject obj) => obj.Name == toObj.Name);
			if (val != null)
			{
				toObj.TryOverwriteWith(val);
			}
		}
	}
}
