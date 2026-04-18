using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne;
using ScheduleOne.DevUtilities;
using UnityEngine;

public class ActionList
{
	private readonly List<Action> list;

	private bool _shuffleCallbackList;

	private bool _shuffleBeforeNextInvoke;

	public ActionList(bool shuffleCallbackList = false)
	{
		list = new List<Action>();
		_shuffleCallbackList = shuffleCallbackList;
	}

	public List<Action> GetInvocationList()
	{
		return list;
	}

	public void InvokeAll()
	{
		Action[] array = list.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
	}

	public void InvokeAllStaggered(float staggerTime)
	{
		if (_shuffleBeforeNextInvoke)
		{
			list.Shuffle();
			_shuffleBeforeNextInvoke = false;
		}
		Action[] listCache = list.ToArray();
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(StaggeredInvoke(staggerTime));
		IEnumerator StaggeredInvoke(float num)
		{
			int listenerCount = listCache.Length;
			float perDelay = num / (float)listenerCount;
			_ = Time.timeSinceLevelLoad;
			float waitOverflow = 0f;
			_ = Time.timeSinceLevelLoad;
			int loopsSinceLastWait = 0;
			for (int i = 0; i < listenerCount; i++)
			{
				loopsSinceLastWait++;
				float num2 = perDelay - waitOverflow;
				float timeOnWaitStart = Time.timeSinceLevelLoad;
				if (num2 > 0f)
				{
					loopsSinceLastWait = 0;
					yield return (object)new WaitForSeconds(num2);
				}
				float num3 = Time.timeSinceLevelLoad - timeOnWaitStart - perDelay;
				waitOverflow += num3;
				if (i >= listCache.Length)
				{
					break;
				}
				if (listCache[i] != null)
				{
					try
					{
						list[i]();
					}
					catch (Exception ex)
					{
						ScheduleOne.Console.LogError("Error invoking StaggeredInvoke: " + ex.Message + "\nSite:" + ex.StackTrace);
					}
				}
			}
		}
	}

	public void Clear()
	{
		list.Clear();
	}

	private void Add(Action action)
	{
		list.Add(action);
		if (_shuffleCallbackList)
		{
			_shuffleBeforeNextInvoke = true;
		}
	}

	private void Remove(Action action)
	{
		list.Remove(action);
	}

	public static ActionList operator +(ActionList list, Action action)
	{
		list.Add(action);
		return list;
	}

	public static ActionList operator -(ActionList list, Action action)
	{
		if (list == null || action == null)
		{
			return list;
		}
		list.Remove(action);
		return list;
	}
}
