using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Misc;
using UnityEngine;

namespace ScheduleOne.Lighting;

[RequireComponent(typeof(ToggleableLight))]
public class LightTimer : MonoBehaviour
{
	[Header("Timing")]
	public int StartTime = 600;

	public int EndTime = 1800;

	public int StartTimeOffset;

	private ToggleableLight toggleableLight;

	protected virtual void Awake()
	{
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(UpdateState);
		toggleableLight = ((Component)this).GetComponent<ToggleableLight>();
	}

	private void Start()
	{
		UpdateState();
	}

	protected virtual void UpdateState()
	{
		SetState(NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime + StartTimeOffset, EndTime));
	}

	private void OnDrawGizmos()
	{
	}

	private void SetState(bool on)
	{
		toggleableLight.isOn = on;
	}
}
