using System;
using UnityEngine;

namespace ScheduleOne.Weather;

[CreateAssetMenu(fileName = "WeatherBasedObjectProvider", menuName = "ScriptableObjects/Weather/Weather Based Object Provider")]
public class WeatherBasedObjectProvider : ScriptableObject
{
	[Serializable]
	public enum EvaluationType
	{
		LessThan,
		Equals,
		GreaterThan,
		Blend
	}

	[Flags]
	public enum ConditionFlags
	{
		None = 0,
		Sunny = 1,
		Cloudy = 2,
		Rainy = 4,
		Stormy = 8,
		Snowy = 0x10,
		Foggy = 0x20,
		Windy = 0x40,
		Hail = 0x80,
		Sleet = 0x100
	}

	[SerializeField]
	private ConditionFlags _selectedConditions;

	[SerializeField]
	private WeatherConditions _conditions;

	[SerializeField]
	private EvaluationType _evaluationType;

	[SerializeField]
	private Object _object;

	public Object Object => _object;

	public bool DoesSatisfyConditions(WeatherConditions activeConditions)
	{
		if (_conditions.Sunny != -1f && !EvaluateConditions(activeConditions.Sunny, _conditions.Sunny))
		{
			return false;
		}
		if (_conditions.Cloudy != -1f && !EvaluateConditions(activeConditions.Cloudy, _conditions.Cloudy))
		{
			return false;
		}
		if (_conditions.Rainy != -1f && !EvaluateConditions(activeConditions.Rainy, _conditions.Rainy))
		{
			return false;
		}
		if (_conditions.Stormy != -1f && !EvaluateConditions(activeConditions.Stormy, _conditions.Stormy))
		{
			return false;
		}
		if (_conditions.Snowy != -1f && !EvaluateConditions(activeConditions.Snowy, _conditions.Snowy))
		{
			return false;
		}
		if (_conditions.Foggy != -1f && !EvaluateConditions(activeConditions.Foggy, _conditions.Foggy))
		{
			return false;
		}
		if (_conditions.Windy != -1f && !EvaluateConditions(activeConditions.Windy, _conditions.Windy))
		{
			return false;
		}
		if (_conditions.Hail != -1f && !EvaluateConditions(activeConditions.Hail, _conditions.Hail))
		{
			return false;
		}
		if (_conditions.Sleet != -1f && !EvaluateConditions(activeConditions.Sleet, _conditions.Sleet))
		{
			return false;
		}
		return true;
	}

	public float GetAverageBlend(WeatherConditions activeConditions)
	{
		return (GetConditionBlendValue(activeConditions.Sunny, _conditions.Sunny) + GetConditionBlendValue(activeConditions.Cloudy, _conditions.Cloudy) + GetConditionBlendValue(activeConditions.Rainy, _conditions.Rainy) + GetConditionBlendValue(activeConditions.Stormy, _conditions.Stormy) + GetConditionBlendValue(activeConditions.Snowy, _conditions.Snowy) + GetConditionBlendValue(activeConditions.Foggy, _conditions.Foggy) + GetConditionBlendValue(activeConditions.Windy, _conditions.Windy) + GetConditionBlendValue(activeConditions.Hail, _conditions.Hail) + GetConditionBlendValue(activeConditions.Sleet, _conditions.Sleet)) / 9f;
	}

	private float GetConditionBlendValue(float activeValue, float condition)
	{
		if (condition == -1f)
		{
			return 0f;
		}
		return Mathf.InverseLerp(0f, condition, activeValue);
	}

	private bool EvaluateConditions(float conditionValue, float conditionThreshold)
	{
		return _evaluationType switch
		{
			EvaluationType.LessThan => conditionValue < conditionThreshold, 
			EvaluationType.Equals => conditionValue == conditionThreshold, 
			EvaluationType.GreaterThan => conditionValue > conditionThreshold, 
			_ => false, 
		};
	}
}
