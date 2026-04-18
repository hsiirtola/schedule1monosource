using System.Collections.Generic;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Audio;

public class AmbientTrackGroup : MonoBehaviour
{
	private const float AmbientTrackCooldown = 540f;

	private static float TimeOnLastAmbientTrackStart;

	private static AmbientTrackGroup LastPlayedTrackGroup;

	private static bool IsAnyTrackGroupQueued;

	[SerializeField]
	[FormerlySerializedAs("Tracks")]
	private List<MusicTrack> _trackList = new List<MusicTrack>();

	[SerializeField]
	[FormerlySerializedAs("MinTime")]
	private int _windowStartTime;

	[SerializeField]
	[FormerlySerializedAs("MaxTime")]
	private int _windowEndTime;

	[SerializeField]
	[FormerlySerializedAs("Chance")]
	[Range(0f, 1f)]
	private float _chanceToPlay = 0.3f;

	private int _startTime;

	private bool _playTrack;

	private bool _trackRandomized;

	private void Awake()
	{
		for (int i = 0; i < _trackList.Count; i++)
		{
			int index = Random.Range(i, _trackList.Count);
			MusicTrack value = _trackList[index];
			_trackList[index] = _trackList[i];
			_trackList[i] = value;
		}
	}

	[Button]
	public void ForcePlay()
	{
		LastPlayedTrackGroup = this;
		TimeOnLastAmbientTrackStart = Time.unscaledTime;
		_playTrack = false;
		IsAnyTrackGroupQueued = false;
		_trackList[0].Enable();
		_trackList.Add(_trackList[0]);
		_trackList.RemoveAt(0);
	}

	public void Stop()
	{
		_trackList[0].Disable();
		_trackList[0].Stop();
	}

	private void Update()
	{
		if (!NetworkSingleton<TimeManager>.InstanceExists)
		{
			_trackRandomized = false;
			IsAnyTrackGroupQueued = false;
			return;
		}
		int currentTime = NetworkSingleton<TimeManager>.Instance.CurrentTime;
		if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(_windowStartTime, _windowEndTime))
		{
			if (!_trackRandomized)
			{
				_playTrack = Random.value < _chanceToPlay && Time.unscaledTime - TimeOnLastAmbientTrackStart > 540f && (Object)(object)LastPlayedTrackGroup != (Object)(object)this && !IsAnyTrackGroupQueued && _trackList.Count > 0 && Time.timeSinceLevelLoad > 20f && !GameManager.IS_TUTORIAL && CanPlayNow();
				_startTime = TimeManager.AddMinutesTo24HourTime(currentTime, Random.Range(0, 120));
				if (_playTrack)
				{
					Console.Log("Will play " + _trackList[0].TrackName + " at " + _startTime);
					IsAnyTrackGroupQueued = true;
					TimeOnLastAmbientTrackStart = Time.unscaledTime;
				}
				_trackRandomized = true;
			}
			if (_playTrack && !_trackList[0].Enabled && NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(_startTime, _windowEndTime))
			{
				LastPlayedTrackGroup = this;
				TimeOnLastAmbientTrackStart = Time.unscaledTime;
				_playTrack = false;
				IsAnyTrackGroupQueued = false;
				_trackList[0].Enable();
				_trackList.Add(_trackList[0]);
				_trackList.RemoveAt(0);
			}
			return;
		}
		_trackRandomized = false;
		_playTrack = false;
		foreach (MusicTrack track in _trackList)
		{
			track.Disable();
		}
	}

	protected virtual bool CanPlayNow()
	{
		if ((Object)(object)Player.Local == (Object)null)
		{
			return false;
		}
		if ((Object)(object)Player.Local.CurrentProperty != (Object)null)
		{
			foreach (Jukebox item in Player.Local.CurrentProperty.GetBuildablesOfType<Jukebox>())
			{
				if (item.IsPlaying && item.CurrentVolume > 0)
				{
					return false;
				}
			}
		}
		return true;
	}
}
