using System;
using System.Collections.Generic;
using ScheduleOne.Core;
using ScheduleOne.Core.Equipping.Framework;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class EquippableDataRegistry : PersistentSingleton<EquippableDataRegistry>
{
	[ReadOnly]
	[SerializeField]
	private List<EquippableData> _equippableDataList = new List<EquippableData>();

	public EquippableData GetEquippableData(Guid guid)
	{
		foreach (EquippableData equippableData in _equippableDataList)
		{
			if (((IdentifiedScriptableObject)equippableData).GUID == guid)
			{
				return equippableData;
			}
		}
		Guid guid2 = guid;
		Debug.LogError((object)("No EquippableData found with GUID: " + guid2.ToString()));
		return null;
	}

	private void RegisterEquippableData(EquippableData data)
	{
		if (!((Object)(object)data == (Object)null))
		{
			if (_equippableDataList.Contains(data))
			{
				Debug.LogWarning((object)("Attempted to register duplicate EquippableData: " + ((Object)data).name));
			}
			else
			{
				_equippableDataList.Add(data);
			}
		}
	}
}
