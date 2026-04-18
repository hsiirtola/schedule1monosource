using System;
using System.Collections;
using ScheduleOne.Audio;
using UnityEngine;

namespace ScheduleOne.VoiceOver;

public class PoliceChatterVO : VOEmitter
{
	public AudioSourceController StartBeep;

	public AudioSourceController StartEndBeep;

	public AudioSourceController Static;

	private Coroutine chatterRoutine;

	public override void Play(EVOLineType lineType)
	{
		if (lineType == EVOLineType.PoliceChatter)
		{
			PlayChatter();
		}
		else
		{
			base.Play(lineType);
		}
	}

	private void PlayChatter()
	{
		if (chatterRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(chatterRoutine);
		}
		chatterRoutine = ((MonoBehaviour)this).StartCoroutine(Play());
		IEnumerator Play()
		{
			StartBeep.Play();
			Static.Play();
			yield return (object)new WaitForSeconds(0.25f);
			base.Play(EVOLineType.PoliceChatter);
			yield return (object)new WaitForSeconds(0.1f);
			yield return (object)new WaitUntil((Func<bool>)(() => !audioSourceController.IsPlaying));
			StartEndBeep.Play();
			Static.Stop();
			chatterRoutine = null;
		}
	}
}
