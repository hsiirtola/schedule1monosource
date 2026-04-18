using System;
using UnityEngine;

namespace ScheduleOne.Weather;

[Serializable]
public class WeatherConditions
{
	[Range(0f, 1f)]
	public float Sunny;

	[Range(0f, 1f)]
	public float Cloudy;

	[Range(0f, 1f)]
	public float Rainy;

	[Range(0f, 1f)]
	public float Stormy;

	[Range(0f, 1f)]
	public float Snowy;

	[Range(0f, 1f)]
	public float Foggy;

	[Range(0f, 1f)]
	public float Windy;

	[Range(0f, 1f)]
	public float Hail;

	[Range(0f, 1f)]
	public float Sleet;

	public void Set(WeatherConditions conditions)
	{
		Sunny = conditions.Sunny;
		Cloudy = conditions.Cloudy;
		Rainy = conditions.Rainy;
		Stormy = conditions.Stormy;
		Snowy = conditions.Snowy;
		Foggy = conditions.Foggy;
		Windy = conditions.Windy;
		Hail = conditions.Hail;
		Sleet = conditions.Sleet;
	}
}
