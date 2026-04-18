using System;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Audio;

[Serializable]
public class AudioZoneTrack
{
	public AudioSourceController Source;

	[Range(0.01f, 2f)]
	public float Volume = 1f;

	public int StartTime;

	public int EndTime;

	public int FadeTime = 60;

	private float timeVolMultiplier;

	private int fadeInStart;

	private int fadeInEnd;

	private int fadeOutStart;

	private int fadeOutEnd;

	private int fadeInStartMinSum;

	private int fadeInEndMinSum;

	private int fadeOutStartMinSum;

	private int fadeOutEndMinSum;

	public void Init()
	{
		fadeInStart = TimeManager.AddMinutesTo24HourTime(StartTime, -FadeTime / 2);
		fadeInEnd = TimeManager.AddMinutesTo24HourTime(StartTime, FadeTime / 2);
		fadeOutStart = TimeManager.AddMinutesTo24HourTime(EndTime, -FadeTime / 2);
		fadeOutEnd = TimeManager.AddMinutesTo24HourTime(EndTime, FadeTime / 2);
		fadeInStartMinSum = TimeManager.GetMinSumFrom24HourTime(fadeInStart);
		fadeInEndMinSum = TimeManager.GetMinSumFrom24HourTime(fadeInEnd);
		fadeOutStartMinSum = TimeManager.GetMinSumFrom24HourTime(fadeOutStart);
		fadeOutEndMinSum = TimeManager.GetMinSumFrom24HourTime(fadeOutEnd);
	}

	public void Update(float multiplier)
	{
		float num = Volume * multiplier * timeVolMultiplier;
		Source.SetBaseVolume(num);
		if (num > 0f)
		{
			if (!Source.IsPlaying)
			{
				Source.Play();
			}
		}
		else if (Source.IsPlaying)
		{
			Source.Stop();
		}
	}

	public void UpdateTimeMultiplier(int time)
	{
		int minSumFrom24HourTime = TimeManager.GetMinSumFrom24HourTime(time);
		if (TimeManager.IsGivenTimeWithinRange(time, fadeInEnd, fadeOutStart))
		{
			timeVolMultiplier = 1f;
		}
		else if (TimeManager.IsGivenTimeWithinRange(time, fadeInStart, fadeInEnd))
		{
			timeVolMultiplier = (float)(minSumFrom24HourTime - fadeInStartMinSum) / (float)(fadeInEndMinSum - fadeInStartMinSum);
		}
		else if (TimeManager.IsGivenTimeWithinRange(time, fadeOutStart, fadeOutEnd))
		{
			timeVolMultiplier = 1f - (float)(minSumFrom24HourTime - fadeOutStartMinSum) / (float)(fadeOutEndMinSum - fadeOutStartMinSum);
		}
		else
		{
			timeVolMultiplier = 0f;
		}
	}
}
