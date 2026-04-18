using System;
using System.Collections;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace ScheduleOne.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceController : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private string _id;

	[SerializeField]
	[FormerlySerializedAs("AudioType")]
	private EAudioType _audioType = EAudioType.FX;

	[Header("Volume")]
	[SerializeField]
	[FormerlySerializedAs("DefaultVolume")]
	[Range(0f, 1f)]
	private float _defaultBaseVolume = 1f;

	[SerializeField]
	[FormerlySerializedAs("VolumeMultiplier")]
	[Range(0f, 2f)]
	private float _volumeMultiplier = 1f;

	[Header("Pitch")]
	[SerializeField]
	[Range(0.1f, 3f)]
	private float _defaultBasePitch = 1f;

	[SerializeField]
	[FormerlySerializedAs("PitchMultiplier")]
	[Range(0f, 2f)]
	private float _pitchMultiplier = 1f;

	[SerializeField]
	[FormerlySerializedAs("RandomizePitch")]
	private bool _randomizePitch;

	[SerializeField]
	[FormerlySerializedAs("MinPitch")]
	[Conditional("_randomizePitch", false)]
	private float _minRandomPitch = 0.9f;

	[SerializeField]
	[FormerlySerializedAs("MaxPitch")]
	[Conditional("_randomizePitch", false)]
	private float _maxRandomPitch = 1.1f;

	[SerializeField]
	[FormerlySerializedAs("LowPassFilter")]
	[Conditional("_lowPassFilter", false)]
	private AudioLowPassFilter _lowPassFilter;

	protected AudioSource _audioSource;

	protected float _baseVolume = 1f;

	protected float _basePitch = 1f;

	public bool IsPlaying
	{
		get
		{
			if (!((Object)(object)_audioSource != (Object)null))
			{
				return false;
			}
			return _audioSource.isPlaying;
		}
	}

	public float Time
	{
		get
		{
			if (!((Object)(object)_audioSource != (Object)null))
			{
				return 0f;
			}
			return _audioSource.time;
		}
	}

	public AudioClip Clip
	{
		get
		{
			if (!((Object)(object)_audioSource != (Object)null))
			{
				return null;
			}
			return _audioSource.clip;
		}
	}

	public string Id => _id;

	public float VolumeMultiplier
	{
		get
		{
			return _volumeMultiplier;
		}
		set
		{
			_volumeMultiplier = value;
			ApplyVolume();
		}
	}

	public float PitchMultiplier
	{
		get
		{
			return _pitchMultiplier;
		}
		set
		{
			_pitchMultiplier = value;
			ApplyPitch();
		}
	}

	private void Awake()
	{
		_audioSource = ((Component)this).GetComponent<AudioSource>();
		_lowPassFilter = ((Component)this).GetComponent<AudioLowPassFilter>();
		SetBaseVolume(_defaultBaseVolume);
		SetBasePitch(_defaultBasePitch);
		_audioSource.volume = 0f;
		ApplyMixer();
	}

	private void OnEnable()
	{
		if (Singleton<PauseMenu>.InstanceExists)
		{
			PauseMenu instance = Singleton<PauseMenu>.Instance;
			instance.onPause = (Action)Delegate.Combine(instance.onPause, new Action(OnPause));
			PauseMenu instance2 = Singleton<PauseMenu>.Instance;
			instance2.onResume = (Action)Delegate.Combine(instance2.onResume, new Action(OnUnpause));
		}
		if (Singleton<AudioManager>.InstanceExists)
		{
			AudioManager instance3 = Singleton<AudioManager>.Instance;
			instance3.onVolumeSettingsChanged = (Action)Delegate.Combine(instance3.onVolumeSettingsChanged, new Action(ApplyVolume));
		}
		ApplyVolume();
		ApplyMixer();
	}

	private void OnDisable()
	{
		if (Singleton<PauseMenu>.InstanceExists)
		{
			PauseMenu instance = Singleton<PauseMenu>.Instance;
			instance.onPause = (Action)Delegate.Remove(instance.onPause, new Action(OnPause));
			PauseMenu instance2 = Singleton<PauseMenu>.Instance;
			instance2.onResume = (Action)Delegate.Remove(instance2.onResume, new Action(OnUnpause));
		}
		if (Singleton<AudioManager>.InstanceExists)
		{
			AudioManager instance3 = Singleton<AudioManager>.Instance;
			instance3.onVolumeSettingsChanged = (Action)Delegate.Remove(instance3.onVolumeSettingsChanged, new Action(ApplyVolume));
		}
	}

	private void ApplyMixer()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (_audioType != EAudioType.Music)
		{
			Scene scene = ((Component)this).gameObject.scene;
			if (!(((Scene)(ref scene)).name == "Main"))
			{
				scene = ((Component)this).gameObject.scene;
				if (!(((Scene)(ref scene)).name == "DontDestroyOnLoad"))
				{
					_audioSource.outputAudioMixerGroup = Singleton<AudioManager>.Instance.MenuMixer;
					return;
				}
			}
			_audioSource.outputAudioMixerGroup = Singleton<AudioManager>.Instance.MainGameMixer;
		}
		else
		{
			_audioSource.outputAudioMixerGroup = Singleton<AudioManager>.Instance.MusicMixer;
		}
	}

	private void OnPause()
	{
		_audioSource.Pause();
	}

	private void OnUnpause()
	{
		ApplyVolume();
		_audioSource.UnPause();
	}

	public void SetBaseVolume(float baseVolume)
	{
		_baseVolume = baseVolume;
		ApplyVolume();
	}

	protected void ApplyVolume()
	{
		if (Singleton<AudioManager>.InstanceExists && !((Object)(object)_audioSource == (Object)null))
		{
			_audioSource.volume = _baseVolume * Singleton<AudioManager>.Instance.GetVolume(_audioType) * _volumeMultiplier;
		}
	}

	public void SetBasePitch(float basePitch)
	{
		_basePitch = basePitch;
		ApplyPitch();
	}

	private void ApplyPitch()
	{
		if (!((Object)(object)_audioSource == (Object)null))
		{
			if (_randomizePitch)
			{
				_audioSource.pitch = Random.Range(_minRandomPitch, _maxRandomPitch) * _pitchMultiplier;
			}
			else
			{
				_audioSource.pitch = _basePitch * _pitchMultiplier;
			}
		}
	}

	public virtual void Play()
	{
		if ((Object)(object)_audioSource == (Object)null)
		{
			Debug.LogWarning((object)"AudioSourceController _audioSource is null", (Object)(object)((Component)this).gameObject);
			return;
		}
		ApplyPitch();
		ApplyVolume();
		if (((Behaviour)_audioSource).enabled && ((Component)_audioSource).gameObject.activeInHierarchy)
		{
			_audioSource.Play();
		}
	}

	public virtual void PlayOneShot()
	{
		if (_randomizePitch)
		{
			_audioSource.pitch = Random.Range(_minRandomPitch, _maxRandomPitch) * _pitchMultiplier;
		}
		ApplyVolume();
		_audioSource.PlayOneShot(_audioSource.clip, 1f);
	}

	public void PlayOneShotDelayed(float delay)
	{
		Delay(delay, delegate
		{
			PlayOneShot();
		});
	}

	public void DuplicateAndPlayOneShot()
	{
		DuplicateAndPlayOneShot(null);
	}

	public virtual void DuplicateAndPlayOneShot(Transform parent)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)parent == (Object)null)
		{
			parent = NetworkSingleton<GameManager>.Instance.Temp;
		}
		AudioSourceController component = Object.Instantiate<GameObject>(((Component)this).gameObject, parent).gameObject.GetComponent<AudioSourceController>();
		((Component)component).transform.position = ((Component)this).transform.position;
		component.Play();
		if ((Object)(object)component._audioSource.clip != (Object)null)
		{
			Object.Destroy((Object)(object)component, component._audioSource.clip.length + 0.1f);
		}
		else
		{
			Object.Destroy((Object)(object)component, 5f);
		}
	}

	protected void Delay(float delay, Action callback)
	{
		((MonoBehaviour)this).StartCoroutine(DelayIE(delay, callback));
	}

	protected IEnumerator DelayIE(float delay, Action callback)
	{
		yield return (object)new WaitForSeconds(delay);
		callback?.Invoke();
	}

	public void ApplyAudioSettings(AudioSettingsWrapper settings)
	{
		_audioType = settings.AudioType;
		SetBaseVolume(settings.Volume);
		_volumeMultiplier = settings.VolumeMultiplier;
		_pitchMultiplier = settings.PitchMultiplier;
		_randomizePitch = settings.RandomizePitch;
		_minRandomPitch = settings.MinMaxPitch.x;
		_maxRandomPitch = settings.MinMaxPitch.y;
		if ((Object)(object)_lowPassFilter != (Object)null)
		{
			_lowPassFilter.cutoffFrequency = settings.LowPassCutoffFrequency;
		}
		ApplyPitch();
		ApplyVolume();
	}

	public AudioSettingsWrapper ExtractAudioSettings()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		return new AudioSettingsWrapper
		{
			AudioType = _audioType,
			Volume = _baseVolume,
			VolumeMultiplier = _volumeMultiplier,
			MinMaxPitch = new Vector2(_minRandomPitch, _maxRandomPitch),
			PitchMultiplier = _pitchMultiplier,
			RandomizePitch = _randomizePitch,
			LowPassCutoffFrequency = (((Object)(object)_lowPassFilter != (Object)null) ? ((int)_lowPassFilter.cutoffFrequency) : 22000)
		};
	}

	public void SetTime(float time)
	{
		if ((Object)(object)_audioSource == (Object)null)
		{
			Debug.LogWarning((object)"AudioSourceController _audioSource is null", (Object)(object)((Component)this).gameObject);
		}
		else
		{
			_audioSource.time = time;
		}
	}

	public void SetClip(AudioClip clip)
	{
		if ((Object)(object)_audioSource == (Object)null)
		{
			Debug.LogWarning((object)"AudioSourceController _audioSource is null", (Object)(object)((Component)this).gameObject);
		}
		else
		{
			_audioSource.clip = clip;
		}
	}

	public void SetLoop(bool loop)
	{
		if ((Object)(object)_audioSource == (Object)null)
		{
			Debug.LogWarning((object)"AudioSourceController _audioSource is null", (Object)(object)((Component)this).gameObject);
		}
		else
		{
			_audioSource.loop = loop;
		}
	}

	public void Stop()
	{
		if (!((Object)(object)_audioSource == (Object)null))
		{
			_audioSource.Stop();
		}
	}
}
