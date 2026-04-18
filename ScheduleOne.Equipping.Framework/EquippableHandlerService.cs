using System;
using System.Collections.Generic;
using ScheduleOne.Configuration;
using ScheduleOne.Core.Equipping.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using UnityEngine;

namespace ScheduleOne.Equipping.Framework;

public static class EquippableHandlerService
{
	private class HandlerInfo
	{
		public Type DataType;

		public Type HandlerType;

		public HandlerInfo(Type dataType, Type handlerType)
		{
			if (!typeof(EquippableData).IsAssignableFrom(dataType))
			{
				Console.LogError("Data type " + dataType.Name + " does not inherit from EquippableData!");
			}
			if (!typeof(IEquippedItemHandler).IsAssignableFrom(handlerType))
			{
				Console.LogError("Handler type " + handlerType.Name + " does not implement IEquippedItemHandler!");
			}
			DataType = dataType;
			HandlerType = handlerType;
		}
	}

	private static EquipConfiguration _configuration;

	private static List<HandlerInfo> _defaultHandlers;

	static EquippableHandlerService()
	{
		_defaultHandlers = new List<HandlerInfo>();
		LoadManager.onLoadConfigurations += LoadConfig;
		LoadConfig();
		static void LoadConfig()
		{
			Singleton<ConfigurationService>.Instance.GetConfigurationAndListenForChanges<EquipConfiguration>(SetConfig);
			static void SetConfig(BaseConfiguration config)
			{
				if (!(config is EquipConfiguration configuration))
				{
					Console.LogError("Received config of wrong type: " + ((object)config).GetType().Name);
				}
				else
				{
					_configuration = configuration;
					SetupHandlerKeys();
				}
			}
		}
	}

	private static void SetupHandlerKeys()
	{
		_defaultHandlers.Clear();
		_defaultHandlers.Add(new HandlerInfo(typeof(EquippableData), typeof(EquippedItemHandler)));
		_defaultHandlers.Add(new HandlerInfo(typeof(CustomHandlerEquippableData), typeof(EquippedItemHandler)));
	}

	public static IEquippedItemHandler GetHandlerPrefab(EquippableData equippedData)
	{
		if ((Object)(object)_configuration == (Object)null)
		{
			Debug.LogError((object)"EquipConfiguration is not loaded! Cannot get handler for equippable data.");
			return null;
		}
		if (equippedData is CustomHandlerEquippableData customHandlerEquippableData)
		{
			if ((Object)(object)customHandlerEquippableData.Handler != (Object)null)
			{
				return (IEquippedItemHandler)(object)customHandlerEquippableData.Handler;
			}
			Debug.LogWarning((object)("CustomHandlerEquippableData " + ((Object)equippedData).name + " does not have a handler assigned. Falling back to default handler lookup."));
		}
		HandlerInfo handlerInfo = _defaultHandlers.Find((HandlerInfo h) => h.DataType == ((object)equippedData).GetType());
		if (handlerInfo == null)
		{
			Debug.Log((object)("Falling back to default handler for equippable data of type: " + ((object)equippedData).GetType().Name));
			handlerInfo = _defaultHandlers[0];
		}
		if (_configuration.TryGetHandlerForData(handlerInfo.HandlerType, out var handler))
		{
			return handler;
		}
		Debug.LogError((object)("No handler found for equippable data of type: " + ((object)equippedData).GetType().Name));
		return null;
	}
}
