using System;
using ScheduleOne.Configuration;
using ScheduleOne.Core.Equipping.Framework;
using ScheduleOne.Core.Settings;
using UnityEngine;

namespace ScheduleOne.Equipping.Framework;

[CreateAssetMenu(fileName = "EquipConfiguration", menuName = "ScheduleOne/Configurations/EquipConfiguration", order = 1)]
public class EquipConfiguration : Configuration<EquipSettings>
{
	[SerializeField]
	public EquippedItemHandler[] Handlers;

	public bool TryGetHandlerForData(Type handlerType, out IEquippedItemHandler handler)
	{
		if (!typeof(IEquippedItemHandler).IsAssignableFrom(handlerType))
		{
			Debug.LogError((object)$"Type {handlerType} is not a valid equipped item handler type.");
			handler = null;
			return false;
		}
		EquippedItemHandler[] handlers = Handlers;
		foreach (EquippedItemHandler equippedItemHandler in handlers)
		{
			if (((object)equippedItemHandler).GetType() == handlerType)
			{
				handler = (IEquippedItemHandler)(object)equippedItemHandler;
				return true;
			}
		}
		handler = null;
		return false;
	}
}
