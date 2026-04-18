using System.Collections.Generic;
using FishNet;
using ScheduleOne.Employees;
using UnityEngine;

namespace ScheduleOne.Quests;

public abstract class Quest_Employees : Quest
{
	public EEmployeeType EmployeeType;

	public QuestEntry AssignBedEntry;

	public QuestEntry PayEntry;

	public abstract List<Employee> GetEmployees();

	protected override void OnUncappedMinPass()
	{
		base.OnUncappedMinPass();
		if (InstanceFinder.IsServer)
		{
			if (AssignBedEntry.State == EQuestState.Active && AreAnyEmployeesAssignedBeds())
			{
				AssignBedEntry.Complete();
			}
			if (PayEntry.State == EQuestState.Active && AreAnyEmployeesPaid())
			{
				PayEntry.Complete();
			}
		}
	}

	protected bool AreAnyEmployeesAssignedBeds()
	{
		foreach (Employee employee in GetEmployees())
		{
			if ((Object)(object)employee.GetHome() != (Object)null)
			{
				return true;
			}
		}
		return false;
	}

	protected bool AreAnyEmployeesPaid()
	{
		foreach (Employee employee in GetEmployees())
		{
			if (employee.PaidForToday)
			{
				return true;
			}
		}
		return false;
	}
}
