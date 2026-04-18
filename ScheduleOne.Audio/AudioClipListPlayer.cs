using System;
using System.Collections.Generic;
using GameKit.Utilities;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Audio;

[RequireComponent(typeof(AudioSourceController))]
public class AudioClipListPlayer : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("Clips")]
	private List<AudioClip> _clips = new List<AudioClip>();

	[SerializeField]
	private bool _shuffleOnAwake = true;

	private AudioSourceController _audioSource;

	private int _currentClipIndex;

	private void Awake()
	{
		_audioSource = ((Component)this).GetComponent<AudioSourceController>();
		if (_shuffleOnAwake)
		{
			Arrays.Shuffle<AudioClip>(_clips);
		}
	}

	private void Start()
	{
		_audioSource.Play();
		NetworkSingleton<TimeManager>.Instance.onTick += new Action(OnTick);
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onTick -= new Action(OnTick);
		}
	}

	private void OnTick()
	{
		if (!_audioSource.IsPlaying)
		{
			_currentClipIndex = (_currentClipIndex + 1) % _clips.Count;
			_audioSource.SetClip(_clips[_currentClipIndex]);
			_audioSource.Play();
		}
	}
}
