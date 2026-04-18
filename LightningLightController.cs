using System;
using System.Collections;
using UnityEngine;

public class LightningLightController : MonoBehaviour
{
	[Serializable]
	public class LightEntry
	{
		public Light light;

		public AnimationCurve flashCurve;

		public float maxIntensity = 8f;

		public float strikeDuration = 0.6f;

		public float startDelay;
	}

	[Header("Light Entries (max 4)")]
	public LightEntry[] lightEntries;

	[Header("Auto-Strike Timing")]
	public float minTimeBetweenStrikes = 3f;

	public float maxTimeBetweenStrikes = 10f;

	private Coroutine _strikeCo;

	private void OnEnable()
	{
		Debug.Log((object)"LightningLightController enabled, starting strike routine.");
		LightEntry[] array = lightEntries;
		foreach (LightEntry lightEntry in array)
		{
			if ((Object)(object)lightEntry.light != (Object)null)
			{
				lightEntry.light.intensity = 0f;
			}
		}
		if (_strikeCo != null)
		{
			((MonoBehaviour)this).StopCoroutine(_strikeCo);
		}
		_strikeCo = ((MonoBehaviour)this).StartCoroutine(DoStrikeRoutine());
	}

	private void OnDisable()
	{
		Debug.Log((object)"LightningLightController disabled, stopping strike routine.");
		if (_strikeCo != null)
		{
			((MonoBehaviour)this).StopCoroutine(_strikeCo);
			_strikeCo = null;
		}
	}

	private IEnumerator DoStrikeRoutine()
	{
		float elapsedTime = 0f;
		do
		{
			for (int i = 0; i < lightEntries.Length; i++)
			{
				LightEntry lightEntry = lightEntries[i];
				if (lightEntry != null && (Object)(object)lightEntry.light != (Object)null)
				{
					float num = Mathf.Min(Mathf.Max(0f, elapsedTime - lightEntry.startDelay) / lightEntry.strikeDuration, 1f);
					float num2 = lightEntry.flashCurve.Evaluate(num);
					lightEntry.light.intensity = num2 * lightEntry.maxIntensity;
				}
			}
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		while (!(elapsedTime > 20f));
	}
}
