using ScheduleOne.PlayerScripts;
using ScheduleOne.Vision;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Audio;

[RequireComponent(typeof(AudioSourceController))]
public class SpottedTremolo : MonoBehaviour
{
	private const float MinVolume = 0f;

	private const float MaxVolume = 1f;

	private const float MinPitch = 1.2f;

	private const float MaxPitch = 1.3f;

	private const float SmoothTime = 0.25f;

	[SerializeField]
	[FormerlySerializedAs("PlayerVisibility")]
	private EntityVisibility _visibilityComponent;

	private AudioSourceController _audio;

	private float _targetIntensity;

	private float _smoothedIntensity;

	private void Awake()
	{
		_audio = ((Component)this).GetComponent<AudioSourceController>();
	}

	private void Update()
	{
		_targetIntensity = 0f;
		VisionEvent highestProgressionEvent = ((ISightable)Player.Local).HighestProgressionEvent;
		if (highestProgressionEvent != null && highestProgressionEvent.playTremolo)
		{
			_targetIntensity = highestProgressionEvent.NormalizedNoticeLevel;
		}
		if (_targetIntensity > _smoothedIntensity)
		{
			_smoothedIntensity = Mathf.MoveTowards(_smoothedIntensity, _targetIntensity, Time.deltaTime / 0.25f);
		}
		else
		{
			_smoothedIntensity = Mathf.MoveTowards(_smoothedIntensity, _targetIntensity, Time.deltaTime / 3f);
		}
		float num = Mathf.Lerp(0f, 1f, _smoothedIntensity);
		_audio.VolumeMultiplier = num;
		_audio.PitchMultiplier = Mathf.Lerp(1.2f, 1.3f, _smoothedIntensity);
		if (num > 0f && !_audio.IsPlaying)
		{
			_audio.Play();
		}
		else if (num <= 0f && _audio.IsPlaying)
		{
			_audio.Stop();
		}
	}
}
