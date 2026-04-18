using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Audio;

public class AudioZone : PolygonalZone
{
	private const float VolumeChangeRate = 1f;

	private const float UpdateInterval = 0.25f;

	[Range(1f, 200f)]
	[FormerlySerializedAs("MaxDistance")]
	[SerializeField]
	private float _maximumAudibleDistance = 100f;

	[SerializeField]
	[FormerlySerializedAs("Tracks")]
	private List<AudioZoneTrack> _tracks = new List<AudioZoneTrack>();

	private float _localCameraDistance;

	private float _currentVolume;

	private List<IAudioZoneModifier> _modifiers = new List<IAudioZoneModifier>();

	protected override void Awake()
	{
		base.Awake();
		((MonoBehaviour)this).InvokeRepeating("RecalculateCameraDistance", 0f, 0.25f);
	}

	private void Start()
	{
		foreach (AudioZoneTrack track in _tracks)
		{
			track.Init();
		}
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnUncappedMinPass);
	}

	private void OnUncappedMinPass()
	{
		foreach (AudioZoneTrack track in _tracks)
		{
			track.UpdateTimeMultiplier(NetworkSingleton<TimeManager>.Instance.CurrentTime);
		}
	}

	private void Update()
	{
		_currentVolume = Mathf.Lerp(_currentVolume, GetFalloffFactor(_localCameraDistance) * GetModifierMultiplier(), 1f * Time.deltaTime);
		foreach (AudioZoneTrack track in _tracks)
		{
			track.Update(_currentVolume);
		}
	}

	private float GetModifierMultiplier()
	{
		float num = 1f;
		foreach (IAudioZoneModifier modifier in _modifiers)
		{
			num *= modifier.VolumeMultiplier;
		}
		return num;
	}

	private void RecalculateCameraDistance()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			_localCameraDistance = GetDistanceToClosestPointOnZone(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position);
		}
	}

	public void AddModifier(IAudioZoneModifier modifier)
	{
		if (modifier == null)
		{
			Debug.LogWarning((object)"Attempted to add a null modifier to an audio zone. This is not allowed and will be ignored.");
		}
		else if (!_modifiers.Contains(modifier))
		{
			_modifiers.Add(modifier);
		}
	}

	public void RemoveModifier(IAudioZoneModifier modifier)
	{
		if (_modifiers.Contains(modifier))
		{
			_modifiers.Remove(modifier);
		}
	}

	private float GetFalloffFactor(float distance)
	{
		return Mathf.Lerp(1f, 0f, Mathf.Clamp01(distance / _maximumAudibleDistance));
	}
}
