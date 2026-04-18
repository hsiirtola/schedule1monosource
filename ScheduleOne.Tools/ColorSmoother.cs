using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Tools;

[Serializable]
public class ColorSmoother
{
	[Serializable]
	public class Override
	{
		public Color Value;

		public int Priority;

		public string Label;
	}

	[SerializeField]
	private Color DefaultValue = Color.white;

	[SerializeField]
	private float SmoothingSpeed = 1f;

	[SerializeField]
	private List<Override> overrides = new List<Override>();

	private Override activeOverride;

	public Color CurrentValue { get; private set; } = Color.white;

	public float Multiplier { get; private set; } = 1f;

	public Color Default => DefaultValue;

	public void Initialize()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		SetDefault(DefaultValue);
	}

	public void SetDefault(Color value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		AddOverride(value, 0, "Default");
		CurrentValue = value;
	}

	public void SetMultiplier(float value)
	{
		Multiplier = value;
	}

	public void AddOverride(Color value, int priority, string label)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		CurrentValue = Color.Lerp(CurrentValue, activeOverride.Value, SmoothingSpeed * Time.fixedDeltaTime) * Multiplier;
	}
}
