using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Audio;

[RequireComponent(typeof(AudioSourceController))]
public class AmbientOneShot : MonoBehaviour
{
	private enum EPlayTime
	{
		All,
		Day,
		Night
	}

	[Header("Settings")]
	[SerializeField]
	[FormerlySerializedAs("Volume")]
	[Range(0f, 1f)]
	private float _volume = 0.2f;

	[SerializeField]
	[FormerlySerializedAs("ChancePerHour")]
	[Range(0f, 1f)]
	private float _playChancePerHour = 0.2f;

	[SerializeField]
	[FormerlySerializedAs("CooldownTime")]
	private int _cooldownTime = 60;

	[SerializeField]
	[FormerlySerializedAs("PlayTime")]
	private EPlayTime _playTime;

	[SerializeField]
	[FormerlySerializedAs("MinDistance")]
	private float _minDistanceFromCameraToPlay = 20f;

	[SerializeField]
	[FormerlySerializedAs("MaxDistance")]
	private float _maxDistanceFromCameraToPlay = 100f;

	[SerializeField]
	[FormerlySerializedAs("PlayWhileInSewer")]
	private bool _canPlayWhilePlayerInSewer;

	private int _timeSinceLastPlay;

	private AudioSourceController _audioSource;

	private void Awake()
	{
		_audioSource = ((Component)this).GetComponent<AudioSourceController>();
	}

	private void Start()
	{
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnUncappedMinPass);
	}

	private void OnUncappedMinPass()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		_timeSinceLastPlay++;
		if (_timeSinceLastPlay >= _cooldownTime && !NetworkSingleton<TimeManager>.Instance.IsSleepInProgress && (_playTime != EPlayTime.Day || !NetworkSingleton<TimeManager>.Instance.IsNight) && (_playTime != EPlayTime.Night || NetworkSingleton<TimeManager>.Instance.IsNight) && (_canPlayWhilePlayerInSewer || !Singleton<SewerCameraPresense>.InstanceExists || !(Singleton<SewerCameraPresense>.Instance.SmoothedCameraPresenceInSewerArea > 0f)))
		{
			float num = Vector3.Distance(((Component)this).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position);
			if (!(num < _minDistanceFromCameraToPlay) && !(num > _maxDistanceFromCameraToPlay) && Random.value < _playChancePerHour / 60f)
			{
				Play();
			}
		}
	}

	private void Play()
	{
		_timeSinceLastPlay = 0;
		_audioSource.SetBaseVolume(_volume);
		_audioSource.Play();
	}
}
