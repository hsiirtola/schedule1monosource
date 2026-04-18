using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class EmployeeLoader : NPCLoader
{
	public override string NPCType => typeof(EmployeeData).Name;

	public override void Load(DynamicSaveData saveData)
	{
		CreateAndLoadEmployee(saveData);
		base.Load(saveData);
	}

	protected virtual Employee CreateAndLoadEmployee(DynamicSaveData saveData)
	{
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		EmployeeData employeeData = DynamicLoader.ExtractBaseData<EmployeeData>(saveData);
		if (employeeData != null)
		{
			ScheduleOne.Property.Property property = Singleton<PropertyManager>.Instance.GetProperty(employeeData.AssignedProperty);
			EEmployeeType type = EEmployeeType.Botanist;
			if (employeeData.DataType == typeof(PackagerData).Name)
			{
				type = EEmployeeType.Handler;
			}
			else if (employeeData.DataType == typeof(BotanistData).Name)
			{
				type = EEmployeeType.Botanist;
			}
			else if (employeeData.DataType == typeof(ChemistData).Name)
			{
				type = EEmployeeType.Chemist;
			}
			else if (employeeData.DataType == typeof(CleanerData).Name)
			{
				type = EEmployeeType.Cleaner;
			}
			else
			{
				Console.LogError("Failed to recognize employee type: " + employeeData.DataType);
			}
			if (GUIDManager.IsGUIDAlreadyRegistered(new Guid(employeeData.GUID)))
			{
				Console.LogWarning("Employee GUID " + employeeData.GUID + " is already registered. Skipping creation.");
				return null;
			}
			Employee employee = NetworkSingleton<EmployeeManager>.Instance.CreateEmployee_Server(property, type, employeeData.FirstName, employeeData.LastName, employeeData.ID, employeeData.IsMale, employeeData.AppearanceIndex, employeeData.Position, employeeData.Rotation, employeeData.GUID);
			if ((Object)(object)employee == (Object)null)
			{
				Console.LogWarning("Failed to create employee");
			}
			if (employeeData.PaidForToday)
			{
				employee.SetIsPaid();
			}
			return employee;
		}
		return null;
	}
}
