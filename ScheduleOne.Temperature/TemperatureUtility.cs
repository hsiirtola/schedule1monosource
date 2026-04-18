using ScheduleOne.DevUtilities;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.Temperature;

public static class TemperatureUtility
{
	public static bool TemperatureSystemEnabled
	{
		get
		{
			if (NetworkSingleton<VariableDatabase>.InstanceExists)
			{
				return NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("TemperatureSystemEnabled");
			}
			return false;
		}
	}

	public static float ToFahrenheit(float celsius)
	{
		return celsius * 9f / 5f + 32f;
	}

	public static string FormatCelsiusTemperature(float celsius, int decimalPoints)
	{
		float num = (float)Mathf.RoundToInt(celsius * (float)(decimalPoints + 1)) / (float)(decimalPoints + 1);
		return $"{num} °C";
	}

	public static string FormatFahrenheitTemperature(float fahrenheit, int decimalPoints)
	{
		float num = (float)Mathf.RoundToInt(fahrenheit * (float)(decimalPoints + 1)) / (float)(decimalPoints + 1);
		return $"{num} °F";
	}

	public static string FormatTemperatureWithAppropriateUnit(float celsius, int decimalPoints = 1)
	{
		if (Singleton<Settings>.InstanceExists && Singleton<Settings>.Instance.UnitType == Settings.EUnitType.Imperial)
		{
			return FormatFahrenheitTemperature(ToFahrenheit(celsius), decimalPoints);
		}
		return FormatCelsiusTemperature(celsius, decimalPoints);
	}

	public static float NormalizeTemperature(float celsius)
	{
		return Mathf.InverseLerp(0f, 40f, celsius);
	}
}
