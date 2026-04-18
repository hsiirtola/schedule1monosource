using System;
using UnityEngine;

namespace ScheduleOne.VoiceOver;

[Serializable]
public class VODatabaseEntry
{
	public EVOLineType LineType;

	public AudioClip[] Clips;

	private AudioClip lastClip;

	public float VolumeMultiplier = 1f;

	public AudioClip GetRandomClip()
	{
		if (Clips.Length == 0)
		{
			return null;
		}
		AudioClip val = Clips[Random.Range(0, Clips.Length)];
		int num = 0;
		while ((Object)(object)val == (Object)(object)lastClip && Clips.Length != 1 && num <= 5)
		{
			val = Clips[Random.Range(0, Clips.Length)];
			num++;
		}
		lastClip = val;
		return val;
	}
}
