using System;
using System.Collections;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

public class StaggeredCallbackUtility : Singleton<StaggeredCallbackUtility>
{
	public void InvokeStaggered(int totalCalls, float totalTime, Action<int> callback, Action onComplete = null)
	{
		int callsPerSecond = Mathf.CeilToInt((float)totalCalls / totalTime);
		InvokeStaggered(totalCalls, callsPerSecond, callback, onComplete);
	}

	public void InvokeStaggered(int totalCalls, int callsPerSecond, Action<int> callback, Action onComplete = null)
	{
		((MonoBehaviour)this).StartCoroutine(InvokeStaggeredRoutine(callback, totalCalls, callsPerSecond));
		IEnumerator InvokeStaggeredRoutine(Action<int> cb, int total, int perSecond)
		{
			float interval = 1f / (float)perSecond;
			float timeOnLastCall = Time.realtimeSinceStartup;
			int i = 0;
			while (i < total)
			{
				if (cb == null)
				{
					yield break;
				}
				while (Time.realtimeSinceStartup - timeOnLastCall > interval && i < total)
				{
					if (cb == null)
					{
						yield break;
					}
					timeOnLastCall += interval;
					cb(i);
					i++;
				}
				yield return (object)new WaitForEndOfFrame();
			}
			if (onComplete != null)
			{
				onComplete();
			}
		}
	}
}
