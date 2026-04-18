using ScheduleOne.Employees;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Focused", menuName = "Properties/Focused Property")]
public class Focused : Effect
{
	public const float WorkSpeedMultiplier = 1.3f;

	public override void ApplyToNPC(NPC npc)
	{
	}

	public override void ApplyToPlayer(Player player)
	{
	}

	public override void ClearFromNPC(NPC npc)
	{
	}

	public override void ClearFromPlayer(Player player)
	{
	}

	protected override void ApplyToEmployee(Employee employee)
	{
		base.ApplyToEmployee(employee);
		employee.WorkSpeedController.Add(new FloatStack.StackEntry(Name, 1.3f, FloatStack.EStackMode.Multiplicative, Tier));
	}

	protected override void ClearFromEmployee(Employee employee)
	{
		base.ClearFromEmployee(employee);
		employee.WorkSpeedController.Remove(Name);
	}
}
