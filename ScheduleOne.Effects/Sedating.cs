using FishNet.Object;
using ScheduleOne.AvatarFramework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Sedating", menuName = "Properties/Sedating Property")]
public class Sedating : Effect
{
	public const float WorkSpeedMultiplier = 0.8f;

	public override void ApplyToNPC(NPC npc)
	{
		npc.Avatar.Eyes.OverrideEyeLids(new Eye.EyeLidConfiguration
		{
			bottomLidOpen = 0.18f,
			topLidOpen = 0.18f
		});
		npc.Avatar.Eyes.ForceBlink();
		npc.Movement.SpeedController.SpeedMultiplier = 0.6f;
	}

	public override void ApplyToPlayer(Player player)
	{
		player.Avatar.Eyes.OverrideEyeLids(new Eye.EyeLidConfiguration
		{
			bottomLidOpen = 0.18f,
			topLidOpen = 0.18f
		});
		player.Avatar.Eyes.ForceBlink();
		if (((NetworkBehaviour)player).IsOwner)
		{
			Singleton<EyelidOverlay>.Instance.OpenMultiplier.AddOverride(0.7f, 6, "sedating");
			PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.AddOverride(-8f, Tier, "sedating");
			PlayerSingleton<PlayerCamera>.Instance.SmoothLookSmoother.AddOverride(0.8f, Tier, "sedating");
		}
	}

	public override void ClearFromNPC(NPC npc)
	{
		npc.Avatar.Eyes.ResetEyeLids();
		npc.Avatar.Eyes.ForceBlink();
		npc.Movement.SpeedController.SpeedMultiplier = 1f;
	}

	public override void ClearFromPlayer(Player player)
	{
		player.Avatar.Eyes.ResetEyeLids();
		player.Avatar.Eyes.ForceBlink();
		if (((NetworkBehaviour)player).IsOwner)
		{
			Singleton<EyelidOverlay>.Instance.OpenMultiplier.RemoveOverride("sedating");
			PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.RemoveOverride("sedating");
			PlayerSingleton<PlayerCamera>.Instance.SmoothLookSmoother.RemoveOverride("sedating");
		}
	}

	protected override void ApplyToEmployee(Employee employee)
	{
		base.ApplyToEmployee(employee);
		employee.WorkSpeedController.Add(new FloatStack.StackEntry(Name, 0.8f, FloatStack.EStackMode.Multiplicative, Tier));
	}

	protected override void ClearFromEmployee(Employee employee)
	{
		base.ClearFromEmployee(employee);
		employee.WorkSpeedController.Remove(Name);
	}
}
