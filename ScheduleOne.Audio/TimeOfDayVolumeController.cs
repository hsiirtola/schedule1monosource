using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Audio;

[RequireComponent(typeof(AudioSourceController))]
public class TimeOfDayVolumeController : MonoBehaviour
{
	private const float MinVolume = 0.3f;

	private const float FadeSpeed = 0.25f;

	[SerializeField]
	[FormerlySerializedAs("VolumeCurve")]
	private AnimationCurve _timeOfDayVolumeCurve;

	[SerializeField]
	[FormerlySerializedAs("FadeDuringMusic")]
	private bool _reduceVolumeWhenSoundtrackPlaying = true;

	private AudioSourceController _audioSourceController;

	private float _volumeMultiplier = 1f;

	private void Awake()
	{
		_audioSourceController = ((Component)this).GetComponent<AudioSourceController>();
	}

	private void Update()
	{
		if (_reduceVolumeWhenSoundtrackPlaying)
		{
			_volumeMultiplier = Mathf.Lerp(_volumeMultiplier, Singleton<MusicManager>.Instance.IsAnyTrackPlaying ? 0.3f : 1f, Time.deltaTime / 0.25f);
		}
		else
		{
			_volumeMultiplier = 1f;
		}
		float num = _timeOfDayVolumeCurve.Evaluate(NetworkSingleton<TimeManager>.Instance.NormalizedTimeOfDay);
		_audioSourceController.VolumeMultiplier = num * _volumeMultiplier;
	}
}
