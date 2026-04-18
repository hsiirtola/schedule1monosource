using System;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Configuration;

public class ConfigurationService : PersistentSingleton<ConfigurationService>
{
	[SerializeField]
	private BaseConfiguration[] _configurations;

	public BaseConfiguration[] Configurations => _configurations;

	protected override void Awake()
	{
		base.Awake();
		ResetConfigurations();
	}

	private void ResetConfigurations()
	{
		BaseConfiguration[] configurations = _configurations;
		for (int i = 0; i < configurations.Length; i++)
		{
			configurations[i].ResetConfigurationToDefault();
		}
	}

	public bool TryGetConfiguration<T>(out T configuration) where T : BaseConfiguration
	{
		BaseConfiguration[] configurations = _configurations;
		for (int i = 0; i < configurations.Length; i++)
		{
			if (configurations[i] is T val)
			{
				configuration = val;
				return true;
			}
		}
		configuration = null;
		return false;
	}

	public bool TryGetConfiguration(string configurationName, out BaseConfiguration configuration)
	{
		configuration = Array.Find(_configurations, (BaseConfiguration config) => ((Object)config).name == configurationName);
		return (Object)(object)configuration != (Object)null;
	}

	public void GetConfigurationAndListenForChanges<T>(Action<BaseConfiguration> onConfigChanged) where T : BaseConfiguration
	{
		if (TryGetConfiguration<T>(out var configuration))
		{
			onConfigChanged?.Invoke(configuration);
			T val = configuration;
			val.OnConfigurationChanged = (Action<BaseConfiguration>)Delegate.Combine(val.OnConfigurationChanged, onConfigChanged);
		}
		else
		{
			Debug.LogError((object)("GetConfigurationAndListenForChanges: No configuration of type " + typeof(T).Name + " found."));
		}
	}

	public void UnsubscribeFromConfigurationChanges<T>(Action<BaseConfiguration> onConfigChanged) where T : BaseConfiguration
	{
		if (TryGetConfiguration<T>(out var configuration))
		{
			T val = configuration;
			val.OnConfigurationChanged = (Action<BaseConfiguration>)Delegate.Remove(val.OnConfigurationChanged, onConfigChanged);
		}
		else
		{
			Debug.LogError((object)("UnsubscribeFromConfigurationChanges: No configuration of type " + typeof(T).Name + " found."));
		}
	}
}
