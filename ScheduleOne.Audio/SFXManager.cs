using System.Collections.Generic;
using ScheduleOne.Configuration;
using ScheduleOne.Core;
using ScheduleOne.Core.Audio;
using ScheduleOne.Core.Settings.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Audio;

public class SFXManager : Singleton<SFXManager>
{
	private static float ImpactSoundMaxRangeSquared;

	private List<AudioSourceController> _soundPool = new List<AudioSourceController>();

	private List<AudioSourceController> _soundsInUse = new List<AudioSourceController>();

	private SFXConfiguration _configuration;

	protected override void Awake()
	{
		base.Awake();
		Singleton<ConfigurationService>.Instance.GetConfigurationAndListenForChanges<SFXConfiguration>(SetConfiguration);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Singleton<ConfigurationService>.InstanceExists)
		{
			Singleton<ConfigurationService>.Instance.UnsubscribeFromConfigurationChanges<SFXConfiguration>(SetConfiguration);
		}
	}

	private void Update()
	{
		for (int num = _soundsInUse.Count - 1; num >= 0; num--)
		{
			if (!_soundsInUse[num].IsPlaying)
			{
				_soundPool.Add(_soundsInUse[num]);
				_soundsInUse.RemoveAt(num);
			}
		}
	}

	public unsafe void PlayImpactSound(EImpactSound material, Vector3 position, float momentum)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerSingleton<PlayerCamera>.InstanceExists || !(Vector3.SqrMagnitude(position - ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position) > ImpactSoundMaxRangeSquared))
		{
			AudioSourceController source;
			if (!_configuration.TryGetImpactTypeData(material, out var data))
			{
				Console.LogWarning("No impact type found for material: " + ((object)(*(EImpactSound*)(&material))/*cast due to .constrained prefix*/).ToString());
			}
			else if (TryPullAudioSource(out source))
			{
				float num = Mathf.Clamp01(momentum / 100f);
				((Component)source).transform.position = position;
				source.PitchMultiplier = Mathf.Lerp(data.PitchAtMaximumMomentum, data.PitchAtMinimumMomentum, num);
				source.VolumeMultiplier = Mathf.Lerp(data.VolumeAtMinimumMomentum, data.VolumeAtMaximumMomentum, Mathf.Sqrt(num));
				source.SetClip(data.Clips[Random.Range(0, data.Clips.Length)]);
				source.Play();
			}
		}
	}

	public unsafe void PlayFootstepSound(EMaterialType materialType, float volume, Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		AudioSourceController source;
		if (!_configuration.TryGetFootstepSoundGroup(materialType, out var group))
		{
			Console.LogWarning("No footstep sound group found for material type: " + ((object)(*(EMaterialType*)(&materialType))/*cast due to .constrained prefix*/).ToString());
		}
		else if (TryPullAudioSource(out source))
		{
			((Component)source).transform.position = position;
			source.PitchMultiplier = Random.Range(group.MinPitch, group.MaxPitch);
			source.VolumeMultiplier = volume;
			source.SetClip(group.GetRandomClip());
			source.Play();
		}
	}

	public void SetConfiguration(BaseConfiguration baseConfiguration)
	{
		SFXConfiguration sFXConfiguration = baseConfiguration as SFXConfiguration;
		if ((Object)(object)sFXConfiguration == (Object)null)
		{
			Console.LogError("Cannot set SFXManager configuration to null!");
			return;
		}
		_configuration = sFXConfiguration;
		ImpactSoundMaxRangeSquared = ((SettingsField<float>)(object)_configuration.Settings.ImpactSoundMaxRange).Value * ((SettingsField<float>)(object)_configuration.Settings.ImpactSoundMaxRange).Value;
		SetupSoundPool();
	}

	private void SetupSoundPool()
	{
		foreach (AudioSourceController item2 in _soundPool)
		{
			if ((Object)(object)item2 != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)item2).gameObject);
			}
		}
		foreach (AudioSourceController item3 in _soundsInUse)
		{
			if ((Object)(object)item3 != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)item3).gameObject);
			}
		}
		_soundPool.Clear();
		_soundsInUse.Clear();
		for (int i = 0; i < ((SettingsField<int>)(object)_configuration.Settings.AudioSourcePoolSize).Value; i++)
		{
			AudioSourceController item = Object.Instantiate<AudioSourceController>(_configuration.ImpactSoundPrefab, ((Component)this).transform);
			_soundPool.Add(item);
		}
		Debug.Log((object)("SFXManager: Sound pool initialized with " + _soundPool.Count + " sources."));
	}

	private bool TryPullAudioSource(out AudioSourceController source)
	{
		if (_soundPool.Count == 0)
		{
			Console.LogWarning("SFXManager: No source available");
			source = null;
			return false;
		}
		source = _soundPool[0];
		_soundPool.RemoveAt(0);
		_soundsInUse.Add(source);
		return true;
	}
}
