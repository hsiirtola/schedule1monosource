using ScheduleOne.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Audio;

public class HeartbeatSoundController : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("VolumeController")]
	private FloatSmoother _volumeController;

	[SerializeField]
	[FormerlySerializedAs("PitchController")]
	private FloatSmoother _pitchController;

	[SerializeField]
	[FormerlySerializedAs("sound")]
	private AudioSourceController _sound;

	public FloatSmoother VolumeController => _volumeController;

	public FloatSmoother PitchController => _pitchController;

	private void Awake()
	{
		_volumeController.Initialize();
		_volumeController.SetDefault(0f);
		_pitchController.Initialize();
		_pitchController.SetDefault(1f);
	}

	private void Update()
	{
		_sound.VolumeMultiplier = _volumeController.CurrentValue;
		_sound.PitchMultiplier = _pitchController.CurrentValue;
		if (_sound.VolumeMultiplier > 0f)
		{
			if (!_sound.IsPlaying)
			{
				_sound.Play();
			}
		}
		else if (_sound.IsPlaying)
		{
			_sound.Stop();
		}
	}
}
