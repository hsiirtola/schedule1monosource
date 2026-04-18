using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace ScheduleOne.Audio;

public class AudioManager : PersistentSingleton<AudioManager>
{
	private const float MinGameVolume = 0.0001f;

	private const float MaxGameVolume = 1f;

	private const float GameVolumeLerpSpeed = 2f;

	public Action onVolumeSettingsChanged;

	[SerializeField]
	private AudioMixerSnapshot _defaultSnapshot;

	[SerializeField]
	private AudioMixerSnapshot _distortedSnapshot;

	private float _masterVolume = 1f;

	private float _ambientVolume = 1f;

	private float _footstepsVolume = 1f;

	private float _fxVolume = 1f;

	private float _uiVolume = 1f;

	private float _musicVolume = 1f;

	private float _voiceVolume = 1f;

	private float _weatherVolume = 1f;

	private float _currentMainMixerVolume = 1f;

	public float MasterVolume => _masterVolume;

	[field: SerializeField]
	public AudioMixerGroup MainGameMixer { get; private set; }

	[field: SerializeField]
	public AudioMixerGroup MenuMixer { get; private set; }

	[field: SerializeField]
	public AudioMixerGroup MusicMixer { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		if (!((Object)(object)Singleton<AudioManager>.Instance == (Object)null) && !((Object)(object)Singleton<AudioManager>.Instance != (Object)(object)this))
		{
			SetMainMixerVolume(0f);
		}
	}

	protected override void Start()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		base.Start();
		if (!((Object)(object)Singleton<AudioManager>.Instance == (Object)null) && !((Object)(object)Singleton<AudioManager>.Instance != (Object)(object)this))
		{
			Singleton<LoadManager>.Instance.onPreSceneChange.AddListener((UnityAction)delegate
			{
				SetDistorted(distorted: false, 0.5f);
			});
		}
	}

	private void Update()
	{
		if (Singleton<LoadManager>.Instance.IsGameLoaded)
		{
			if (_currentMainMixerVolume < 1f)
			{
				SetMainMixerVolume(_currentMainMixerVolume + Time.deltaTime * 2f);
			}
		}
		else if (_currentMainMixerVolume > 0f)
		{
			SetMainMixerVolume(_currentMainMixerVolume - Time.deltaTime * 2f);
		}
	}

	public void SetDistorted(bool distorted, float transition = 5f)
	{
		if (distorted)
		{
			_distortedSnapshot.TransitionTo(transition);
		}
		else
		{
			_defaultSnapshot.TransitionTo(transition);
		}
	}

	public float GetVolume(EAudioType audioType, bool scaled = true)
	{
		switch (audioType)
		{
		case EAudioType.Ambient:
			return _ambientVolume * (scaled ? _masterVolume : 1f);
		case EAudioType.Footsteps:
			return _footstepsVolume * (scaled ? _masterVolume : 1f);
		case EAudioType.FX:
			return _fxVolume * (scaled ? _masterVolume : 1f);
		case EAudioType.UI:
			return _uiVolume * (scaled ? _masterVolume : 1f);
		case EAudioType.Music:
			return _musicVolume * (scaled ? _masterVolume : 1f);
		case EAudioType.Voice:
			return _voiceVolume * (scaled ? _masterVolume : 1f);
		case EAudioType.Weather:
			return _weatherVolume * (scaled ? _masterVolume : 1f);
		default:
			Debug.LogError((object)$"AudioManager: GetVolume called with invalid audio type {audioType}");
			return 1f;
		}
	}

	public void SetMasterVolume(float volume)
	{
		Debug.Log((object)$"AudioManager: Setting master volume to {volume}");
		_masterVolume = volume;
		if (onVolumeSettingsChanged != null)
		{
			onVolumeSettingsChanged();
		}
	}

	public void SetVolume(EAudioType type, float volume)
	{
		Debug.Log((object)$"AudioManager: Setting {type} volume to {volume}");
		switch (type)
		{
		case EAudioType.Ambient:
			_ambientVolume = volume;
			break;
		case EAudioType.Footsteps:
			_footstepsVolume = volume;
			break;
		case EAudioType.FX:
			_fxVolume = volume;
			break;
		case EAudioType.UI:
			_uiVolume = volume;
			break;
		case EAudioType.Music:
			_musicVolume = volume;
			break;
		case EAudioType.Voice:
			_voiceVolume = volume;
			break;
		case EAudioType.Weather:
			_weatherVolume = volume;
			break;
		}
		if (onVolumeSettingsChanged != null)
		{
			onVolumeSettingsChanged();
		}
	}

	private void SetMainMixerVolume(float value)
	{
		_currentMainMixerVolume = Mathf.Clamp01(value);
		MainGameMixer.audioMixer.SetFloat("MasterVolume", ValueToVolume(_currentMainMixerVolume));
	}

	private static float ValueToVolume(float value)
	{
		return Mathf.Log10(Mathf.Lerp(0.0001f, 1f, value)) * 20f;
	}
}
