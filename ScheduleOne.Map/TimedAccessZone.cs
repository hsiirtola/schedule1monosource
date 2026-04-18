using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Map;

public class TimedAccessZone : AccessZone
{
	[Header("Timing Settings")]
	public int OpenTime = 600;

	public int CloseTime = 1800;

	protected virtual void Start()
	{
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnUncappedMinPass);
	}

	protected virtual void OnUncappedMinPass()
	{
		SetIsOpen(GetIsOpen());
	}

	protected virtual bool GetIsOpen()
	{
		return NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(OpenTime, CloseTime);
	}
}
