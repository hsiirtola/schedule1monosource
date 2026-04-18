using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using UnityEngine;

namespace ScheduleOne.Cartel;

public class CartelActivity : MonoBehaviour
{
	[Header("Settings")]
	[Range(0f, 1f)]
	public float InfluenceRequirement;

	public Action onActivated;

	public Action onDeactivated;

	public bool IsActive { get; protected set; }

	public int MinsSinceActivation { get; protected set; }

	public EMapRegion Region { get; protected set; }

	private void Start()
	{
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onHourPass = (Action)Delegate.Combine(instance.onHourPass, new Action(HourPassed));
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPassed);
	}

	public virtual void Activate(EMapRegion region)
	{
		Console.Log($"Activating activity {((object)this).GetType().Name} in region {region}");
		IsActive = true;
		Region = region;
		MinsSinceActivation = 0;
		if (onActivated != null)
		{
			onActivated();
		}
	}

	protected virtual void MinPassed()
	{
		if (IsActive)
		{
			MinsSinceActivation++;
		}
	}

	protected virtual void HourPassed()
	{
	}

	protected virtual void Deactivate()
	{
		Console.Log($"Deactivating activity {((object)this).GetType().Name} in region {Region}");
		IsActive = false;
		MinsSinceActivation = 0;
		if (onDeactivated != null)
		{
			onDeactivated();
		}
	}

	public virtual bool IsRegionValidForActivity(EMapRegion region)
	{
		if (!((Behaviour)this).enabled)
		{
			return false;
		}
		return NetworkSingleton<Cartel>.Instance.Influence.GetInfluence(region) >= InfluenceRequirement;
	}
}
