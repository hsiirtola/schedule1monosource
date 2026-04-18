using System.Collections;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools;

public class DelayedUnityEvent : MonoBehaviour
{
	public float Delay = 1f;

	public UnityEvent onDelayStart;

	public UnityEvent onDelayedExecute;

	[Button]
	public void Execute()
	{
		if (Singleton<CoroutineService>.InstanceExists)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Wait());
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(Wait());
		}
		IEnumerator Wait()
		{
			if (onDelayStart != null)
			{
				onDelayStart.Invoke();
			}
			yield return (object)new WaitForSeconds(Delay);
			if (onDelayedExecute != null)
			{
				onDelayedExecute.Invoke();
			}
		}
	}
}
