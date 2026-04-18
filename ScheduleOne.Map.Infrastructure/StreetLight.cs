using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Misc;
using UnityEngine;

namespace ScheduleOne.Map.Infrastructure;

public class StreetLight : MonoBehaviour
{
	private static Vector3 PowerOrigin = new Vector3(150f, 0f, -150f);

	[Header("References")]
	[SerializeField]
	protected ToggleableLight _light;

	[Header("Timing")]
	public int StartTime = 1800;

	public int EndTime = 600;

	private int _startTimeOffset;

	protected virtual void Awake()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onTimeChanged = (Action)Delegate.Combine(instance.onTimeChanged, new Action(UpdateState));
		_startTimeOffset = (int)(Vector3.Distance(((Component)this).transform.position, PowerOrigin) / 50f);
	}

	private void Start()
	{
		UpdateState();
	}

	private void UpdateState()
	{
		SetState(NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(TimeManager.AddMinutesTo24HourTime(StartTime, _startTimeOffset), TimeManager.AddMinutesTo24HourTime(EndTime, _startTimeOffset)));
	}

	private void SetState(bool on)
	{
		_light.isOn = on;
	}
}
