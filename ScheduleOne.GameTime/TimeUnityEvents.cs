using System;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.GameTime;

public class TimeUnityEvents : MonoBehaviour
{
	public UnityEvent onHourPass;

	public UnityEvent onDayPass;

	public UnityEvent onSleepStart;

	public UnityEvent onSleepEnd;

	private void Start()
	{
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onHourPass = (Action)Delegate.Remove(instance.onHourPass, new Action(HourPass));
		TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
		instance2.onHourPass = (Action)Delegate.Combine(instance2.onHourPass, new Action(HourPass));
		TimeManager instance3 = NetworkSingleton<TimeManager>.Instance;
		instance3.onDayPass = (Action)Delegate.Remove(instance3.onDayPass, new Action(DayPass));
		TimeManager instance4 = NetworkSingleton<TimeManager>.Instance;
		instance4.onDayPass = (Action)Delegate.Combine(instance4.onDayPass, new Action(DayPass));
		TimeManager instance5 = NetworkSingleton<TimeManager>.Instance;
		instance5.onSleepStart = (Action)Delegate.Remove(instance5.onSleepStart, new Action(SleepStart));
		TimeManager instance6 = NetworkSingleton<TimeManager>.Instance;
		instance6.onSleepStart = (Action)Delegate.Combine(instance6.onSleepStart, new Action(SleepStart));
		TimeManager instance7 = NetworkSingleton<TimeManager>.Instance;
		instance7.onSleepEnd = (Action)Delegate.Remove(instance7.onSleepEnd, new Action(SleepEnd));
		TimeManager instance8 = NetworkSingleton<TimeManager>.Instance;
		instance8.onSleepEnd = (Action)Delegate.Combine(instance8.onSleepEnd, new Action(SleepEnd));
	}

	private void HourPass()
	{
		if (onHourPass != null)
		{
			onHourPass.Invoke();
		}
	}

	private void DayPass()
	{
		if (onDayPass != null)
		{
			onDayPass.Invoke();
		}
	}

	private void SleepStart()
	{
		if (onSleepStart != null)
		{
			onSleepStart.Invoke();
		}
	}

	private void SleepEnd()
	{
		if (onSleepEnd != null)
		{
			onSleepEnd.Invoke();
		}
	}
}
