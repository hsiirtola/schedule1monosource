using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Audio;

[RequireComponent(typeof(AudioSourceController))]
public class MusicTrack : MonoBehaviour
{
	public bool Enabled;

	[SerializeField]
	[FormerlySerializedAs("TrackName")]
	private string _trackName = "Track";

	[SerializeField]
	[FormerlySerializedAs("Priority")]
	private int _priority = 1;

	[SerializeField]
	[FormerlySerializedAs("VolumeMultiplier")]
	protected float _volumeMultiplier = 1f;

	[FormerlySerializedAs("FadeInTime")]
	[SerializeField]
	protected float _fadeInTime = 1f;

	[SerializeField]
	[FormerlySerializedAs("FadeOutTime")]
	protected float _fadeOutTime = 2f;

	[SerializeField]
	[FormerlySerializedAs("AutoFadeOut")]
	protected bool _autoFadeOut = true;

	protected AudioSourceController _audioSource;

	protected float _fadeVolumeMultiplier = 1f;

	public bool IsPlaying { get; private set; }

	public string TrackName => _trackName;

	public int Priority => _priority;

	protected virtual void Awake()
	{
		_audioSource = ((Component)this).GetComponent<AudioSourceController>();
		_fadeVolumeMultiplier = 0f;
	}

	private void OnValidate()
	{
		((Object)((Component)this).gameObject).name = TrackName + " (" + Priority + ")";
	}

	public void Enable()
	{
		Enabled = true;
	}

	public void Disable()
	{
		Enabled = false;
	}

	public virtual void Play()
	{
		IsPlaying = true;
		_audioSource.Play();
	}

	public virtual void Stop()
	{
		IsPlaying = false;
	}

	protected virtual void Update()
	{
		if (IsPlaying && _audioSource.Time >= _audioSource.Clip.length - _fadeOutTime && _autoFadeOut)
		{
			Stop();
			Disable();
		}
		if (IsPlaying)
		{
			_fadeVolumeMultiplier = Mathf.Min(_fadeVolumeMultiplier + Time.deltaTime / _fadeInTime, 1f);
			_audioSource.VolumeMultiplier = _fadeVolumeMultiplier * _volumeMultiplier;
			return;
		}
		_fadeVolumeMultiplier = Mathf.Max(_fadeVolumeMultiplier - Time.deltaTime / _fadeOutTime, 0f);
		_audioSource.VolumeMultiplier = _fadeVolumeMultiplier * _volumeMultiplier;
		if (_audioSource.VolumeMultiplier == 0f)
		{
			_audioSource.Stop();
		}
	}
}
