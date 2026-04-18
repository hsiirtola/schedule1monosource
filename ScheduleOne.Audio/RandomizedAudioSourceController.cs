using UnityEngine;

namespace ScheduleOne.Audio;

public class RandomizedAudioSourceController : AudioSourceController
{
	public AudioClip[] Clips;

	public override void Play()
	{
		if (Clips.Length == 0)
		{
			Console.LogWarning("RandomizedAudioSourceController: No clips to play");
			return;
		}
		int num = Random.Range(0, Clips.Length);
		SetClip(Clips[num]);
		base.Play();
	}

	public override void PlayOneShot()
	{
		if (Clips.Length == 0)
		{
			Console.LogWarning("RandomizedAudioSourceController: No clips to play");
			return;
		}
		int num = Random.Range(0, Clips.Length);
		SetClip(Clips[num]);
		base.PlayOneShot();
	}
}
