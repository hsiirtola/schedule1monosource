using System;

namespace ScheduleOne.Weather;

public static class EnvironmentHandler
{
	private static WeatherChangeHandler _onWeatherChange;

	private static WeatherEntityHandler _onRegisterWeatherEntity;

	private static WeatherEntityHandler _onUnregisterWeatherEntity;

	public static void RaiseWeatherChange(WeatherConditions newConditions)
	{
		_onWeatherChange?.Invoke(newConditions);
	}

	public static void RegisterWeatherEntity(IWeatherEntity entity)
	{
		_onRegisterWeatherEntity?.Invoke(entity);
	}

	public static void UnregisterWeatherEntity(IWeatherEntity entity)
	{
		_onUnregisterWeatherEntity?.Invoke(entity);
	}

	public static void SubscribeToWeatherChange(WeatherChangeHandler handler)
	{
		_onWeatherChange = (WeatherChangeHandler)Delegate.Combine(_onWeatherChange, handler);
	}

	public static void UnsubscribeFromWeatherChange(WeatherChangeHandler handler)
	{
		_onWeatherChange = (WeatherChangeHandler)Delegate.Remove(_onWeatherChange, handler);
	}

	public static void SubscribeToOnRegisterWeatherEntity(WeatherEntityHandler handler)
	{
		_onRegisterWeatherEntity = (WeatherEntityHandler)Delegate.Combine(_onRegisterWeatherEntity, handler);
	}

	public static void UnsubscribeFromOnRegisterWeatherEntity(WeatherEntityHandler handler)
	{
		_onRegisterWeatherEntity = (WeatherEntityHandler)Delegate.Remove(_onRegisterWeatherEntity, handler);
	}

	public static void SubscribeToOnUnregisterWeatherEntity(WeatherEntityHandler handler)
	{
		_onUnregisterWeatherEntity = (WeatherEntityHandler)Delegate.Combine(_onUnregisterWeatherEntity, handler);
	}

	public static void UnsubscribeFromOnUnregisterWeatherEntity(WeatherEntityHandler handler)
	{
		_onUnregisterWeatherEntity = (WeatherEntityHandler)Delegate.Remove(_onUnregisterWeatherEntity, handler);
	}
}
