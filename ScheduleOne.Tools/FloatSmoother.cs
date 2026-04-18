using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Tools;

[Serializable]
public class FloatSmoother
{
	public class Override
	{
		public float Value;

		public int Priority;

		public string Label;
	}

	[SerializeField]
	private float DefaultValue = 1f;

	[SerializeField]
	private float SmoothingSpeed = 1f;

	private List<Override> overrides = new List<Override>();

	private Override activeOverride;

	public float CurrentValue { get; private set; }

	public float Multiplier { get; private set; } = 1f;

	public void Initialize()
	{
		SetDefault(DefaultValue);
	}

	public void SetDefault(float value, bool apply = true)
	{
		AddOverride(value, 0, "Default");
		if (apply)
		{
			CurrentValue = value;
		}
	}

	public void SetMultiplier(float value)
	{
		Multiplier = value;
	}

	public void SetSmoothingSpeed(float value)
	{
		SmoothingSpeed = value;
	}

	public void AddOverride(float value, int priority, string label)
	{
		if (overrides.Count == 0 && NetworkSingleton<TimeManager>.InstanceExists)
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onFixedUpdate = (Action)Delegate.Combine(instance.onFixedUpdate, new Action(Update));
		}
		Override obj = overrides.Find((Override x) => x.Label.ToLower() == label.ToLower());
		if (obj == null)
		{
			obj = new Override();
			obj.Label = label;
			overrides.Add(obj);
		}
		obj.Value = value;
		obj.Priority = priority;
		overrides.Sort((Override x, Override y) => y.Priority.CompareTo(x.Priority));
		activeOverride = overrides[0];
	}

	public void RemoveOverride(string label)
	{
		Override obj = overrides.Find((Override x) => x.Label.ToLower() == label.ToLower());
		if (obj != null)
		{
			overrides.Remove(obj);
		}
		overrides.Sort((Override x, Override y) => y.Priority.CompareTo(x.Priority));
		if (overrides.Count > 0)
		{
			activeOverride = overrides[0];
			return;
		}
		activeOverride = null;
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onFixedUpdate = (Action)Delegate.Remove(instance.onFixedUpdate, new Action(Update));
		}
	}

	public void Update()
	{
		CurrentValue = Mathf.Lerp(CurrentValue, activeOverride.Value, SmoothingSpeed * Time.fixedDeltaTime) * Multiplier;
	}
}
