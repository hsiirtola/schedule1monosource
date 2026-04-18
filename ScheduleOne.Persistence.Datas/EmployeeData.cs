using System;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

[Serializable]
public class EmployeeData : NPCData
{
	public string AssignedProperty;

	public string FirstName;

	public string LastName;

	public bool IsMale;

	public int AppearanceIndex;

	public Vector3 Position;

	public Quaternion Rotation;

	public string GUID;

	public bool PaidForToday;

	public EmployeeData(string id, string assignedProperty, string firstName, string lastName, bool isMale, int appearanceIndex, Vector3 position, Quaternion rotation, Guid guid, bool paidForToday)
		: base(id)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		AssignedProperty = assignedProperty;
		FirstName = firstName;
		LastName = lastName;
		IsMale = isMale;
		AppearanceIndex = appearanceIndex;
		Position = position;
		Rotation = rotation;
		GUID = guid.ToString();
		PaidForToday = paidForToday;
	}
}
