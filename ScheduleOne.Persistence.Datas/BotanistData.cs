using System;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

[Serializable]
public class BotanistData : EmployeeData
{
	public MoveItemData MoveItemData;

	public BotanistData(string id, string assignedProperty, string firstName, string lastName, bool male, int appearanceIndex, Vector3 position, Quaternion rotation, Guid guid, bool paidForToday, MoveItemData moveItemData)
		: base(id, assignedProperty, firstName, lastName, male, appearanceIndex, position, rotation, guid, paidForToday)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		MoveItemData = moveItemData;
	}
}
