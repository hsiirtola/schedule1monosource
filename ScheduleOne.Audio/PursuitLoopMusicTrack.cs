using System.Collections;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Audio;

public class PursuitLoopMusicTrack : PursuitMusicTrack
{
	public AudioSourceController LoopSound;

	protected override void Awake()
	{
		base.Awake();
		_autoFadeOut = false;
	}

	public override void Stop()
	{
		base.Stop();
		LoopSound.SetLoop(loop: true);
	}

	protected override void Update()
	{
		base.Update();
		if (base.IsPlaying)
		{
			if (!_audioSource.IsPlaying && !LoopSound.IsPlaying)
			{
				LoopSound.Play();
			}
			LoopSound.VolumeMultiplier = _fadeVolumeMultiplier * _volumeMultiplier;
		}
		else
		{
			LoopSound.VolumeMultiplier = _fadeVolumeMultiplier * _volumeMultiplier;
			if (LoopSound.VolumeMultiplier == 0f)
			{
				LoopSound.Stop();
			}
		}
	}

	public override void Play()
	{
		base.Play();
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(WaitForStart());
		IEnumerator WaitForStart()
		{
			while (true)
			{
				if (!base.IsPlaying)
				{
					yield break;
				}
				if (_audioSource.Clip.length - _audioSource.Time <= Time.deltaTime)
				{
					break;
				}
				yield return (object)new WaitForEndOfFrame();
			}
			LoopSound.Play();
		}
	}
}
