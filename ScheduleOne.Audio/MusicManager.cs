using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ScheduleOne.Audio;

public class MusicManager : PersistentSingleton<MusicManager>
{
	private const float TrackUpdateInterval = 0.2f;

	[SerializeField]
	[FormerlySerializedAs("DefaultSnapshot")]
	private AudioMixerSnapshot _defaultSnapshot;

	[SerializeField]
	[FormerlySerializedAs("DistortedSnapshot")]
	private AudioMixerSnapshot _distortedSnapshot;

	private List<MusicTrack> _tracks = new List<MusicTrack>();

	private MusicTrack _currentTrack;

	public bool IsAnyTrackPlaying
	{
		get
		{
			if ((Object)(object)_currentTrack != (Object)null)
			{
				return _currentTrack.IsPlaying;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (!((Object)(object)Singleton<MusicManager>.Instance == (Object)null) && !((Object)(object)Singleton<MusicManager>.Instance != (Object)(object)this))
		{
			_tracks = new List<MusicTrack>(((Component)this).GetComponentsInChildren<MusicTrack>());
			((MonoBehaviour)this).InvokeRepeating("UpdateTracks", 0f, 0.2f);
			_defaultSnapshot.TransitionTo(0.1f);
		}
	}

	protected override void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		base.Start();
		Singleton<LoadManager>.Instance.onPreSceneChange.AddListener((UnityAction)delegate
		{
			SetMusicDistorted(distorted: false, 0.5f);
		});
	}

	public void SetMusicDistorted(bool distorted, float transition = 5f)
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

	public void SetTrackEnabled(string trackName, bool enabled)
	{
		MusicTrack musicTrack = _tracks.Find((MusicTrack t) => t.TrackName == trackName);
		if ((Object)(object)musicTrack == (Object)null)
		{
			Console.LogWarning("Music track not found: " + trackName);
		}
		else if (enabled)
		{
			musicTrack.Enable();
		}
		else
		{
			musicTrack.Disable();
		}
	}

	public bool TryGetTrack(string trackName, out MusicTrack track)
	{
		track = _tracks.Find((MusicTrack t) => t.TrackName == trackName);
		return (Object)(object)track != (Object)null;
	}

	public void StopTrack(string trackName)
	{
		MusicTrack musicTrack = _tracks.Find((MusicTrack t) => t.TrackName == trackName);
		if ((Object)(object)musicTrack == (Object)null)
		{
			Console.LogWarning("Music track not found: " + trackName);
		}
		else
		{
			musicTrack.Stop();
		}
	}

	public void StopAndDisableTracks()
	{
		foreach (MusicTrack track in _tracks)
		{
			track.Disable();
			track.Stop();
		}
	}

	private void UpdateTracks()
	{
		if ((Object)(object)_currentTrack != (Object)null && !_currentTrack.IsPlaying)
		{
			_currentTrack = null;
		}
		MusicTrack musicTrack = null;
		foreach (MusicTrack track in _tracks)
		{
			if (track.Enabled && ((Object)(object)musicTrack == (Object)null || track.Priority > musicTrack.Priority))
			{
				musicTrack = track;
			}
		}
		if ((Object)(object)_currentTrack != (Object)(object)musicTrack && (Object)(object)musicTrack != (Object)null)
		{
			if ((Object)(object)_currentTrack != (Object)null)
			{
				_currentTrack.Stop();
			}
			_currentTrack = musicTrack;
			if ((Object)(object)_currentTrack != (Object)null)
			{
				_currentTrack.Play();
			}
		}
	}
}
