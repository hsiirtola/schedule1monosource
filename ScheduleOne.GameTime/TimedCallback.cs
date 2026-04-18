using System;
using ScheduleOne.DevUtilities;

namespace ScheduleOne.GameTime;

public class TimedCallback
{
	private int _remainingMinutes;

	private Action _callback;

	private int _initialRemainingMinutes;

	public TimedCallback(Action callback, int durationMinutes, bool tickAtEndOfDay = true, bool tickOnTimeSkip = true)
	{
		if (!NetworkSingleton<TimeManager>.InstanceExists)
		{
			Console.LogError("TimedCallback requires a TimeManager instance to function. Please ensure that a TimeManager is present in the scene.");
			return;
		}
		_remainingMinutes = durationMinutes;
		_initialRemainingMinutes = _remainingMinutes;
		_callback = callback;
		if (tickAtEndOfDay)
		{
			NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(Tick);
		}
		else
		{
			NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(Tick);
		}
		if (tickOnTimeSkip)
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onTimeSkip = (Action<int>)Delegate.Combine(instance.onTimeSkip, new Action<int>(OnTimeSkip));
		}
	}

	public void Cancel()
	{
		Cleanup();
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(Tick);
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(Tick);
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onTimeSkip = (Action<int>)Delegate.Remove(instance.onTimeSkip, new Action<int>(OnTimeSkip));
	}

	public void Reset()
	{
		_remainingMinutes = _initialRemainingMinutes;
	}

	private void OnTimeSkip(int skippedMinutes)
	{
		_remainingMinutes -= skippedMinutes;
		if (_remainingMinutes <= 0)
		{
			Execute();
		}
	}

	private void Tick()
	{
		_remainingMinutes--;
		if (_remainingMinutes <= 0)
		{
			Execute();
		}
	}

	private void Execute()
	{
		_callback?.Invoke();
		Cleanup();
	}

	private void Cleanup()
	{
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(Tick);
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(Tick);
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onTimeSkip = (Action<int>)Delegate.Remove(instance.onTimeSkip, new Action<int>(OnTimeSkip));
	}
}
