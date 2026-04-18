using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Map;

public class ScheduledMaterialChange : MonoBehaviour
{
	private enum EOnState
	{
		Undecided,
		On,
		Off
	}

	public MeshRenderer[] Renderers;

	public int MaterialIndex;

	[Header("Settings")]
	public bool Enabled = true;

	public bool LogState;

	public Material OutsideTimeRangeMaterial;

	public Material InsideTimeRangeMaterial;

	public int TimeRangeMin;

	public int TimeRangeMax;

	public int TimeRangeShift;

	public int TimeRangeRandomization;

	[Range(0f, 1f)]
	public float TurnOnChance = 1f;

	[Range(0f, 1f)]
	public float TurnOffChance = 0.85f;

	private bool appliedInsideTimeRange;

	private EOnState onState;

	private int randomShift;

	private bool _shouldTurnOn;

	private bool _shouldTurnOff;

	private EOnState _lastOnState;

	protected virtual void Start()
	{
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnUncappedMinPass);
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepEnd = (Action)Delegate.Combine(instance.onSleepEnd, new Action(Reset));
		SetMaterial(insideTimeRange: false);
		appliedInsideTimeRange = false;
		SetOnOffStatus();
		onState = (_shouldTurnOn ? EOnState.On : EOnState.Off);
		if (LogState)
		{
			Debug.Log((object)$"[ScheduledMaterialChange] Active Time Range. onState: {onState}");
		}
	}

	private void Reset()
	{
		SetOnOffStatus();
	}

	protected virtual void OnUncappedMinPass()
	{
		if (!Enabled && appliedInsideTimeRange)
		{
			SetMaterial(insideTimeRange: false);
		}
		int min = TimeManager.AddMinutesTo24HourTime(TimeRangeMin, TimeRangeShift + randomShift);
		int num = TimeManager.AddMinutesTo24HourTime(TimeRangeMax, TimeRangeShift + randomShift);
		int min2 = num;
		int max = 400;
		if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(min, num))
		{
			onState = (_shouldTurnOn ? EOnState.On : EOnState.Off);
			if (LogState)
			{
				Debug.Log((object)$"[ScheduledMaterialChange] Active Time Range. onState: {onState}");
			}
		}
		else if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(min2, max))
		{
			onState = (_shouldTurnOff ? EOnState.Off : onState);
			if (LogState)
			{
				Debug.Log((object)$"[ScheduledMaterialChange] Active Time Range. onState: {onState}");
			}
		}
		else
		{
			onState = EOnState.Off;
			if (LogState)
			{
				Debug.Log((object)$"[ScheduledMaterialChange] Active Time Range. onState: {onState} Time: {NetworkSingleton<TimeManager>.Instance.CurrentTime}");
			}
		}
		if (_lastOnState != onState)
		{
			SetMaterial(onState == EOnState.On);
			_lastOnState = onState;
		}
	}

	private void SetOnOffStatus()
	{
		randomShift = Random.Range(-TimeRangeRandomization, TimeRangeRandomization);
		_shouldTurnOn = Random.Range(0f, 1f) < TurnOnChance;
		_shouldTurnOff = Random.Range(0f, 1f) < TurnOffChance;
	}

	private void SetMaterial(bool insideTimeRange)
	{
		if (Renderers != null && Renderers.Length != 0)
		{
			appliedInsideTimeRange = insideTimeRange;
			Material val = (insideTimeRange ? InsideTimeRangeMaterial : OutsideTimeRangeMaterial);
			MeshRenderer[] renderers = Renderers;
			foreach (MeshRenderer obj in renderers)
			{
				Material[] materials = ((Renderer)obj).materials;
				materials[MaterialIndex] = val;
				((Renderer)obj).materials = materials;
			}
		}
	}
}
