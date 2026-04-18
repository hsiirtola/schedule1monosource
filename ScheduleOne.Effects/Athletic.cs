using FishNet.Object;
using ScheduleOne.AvatarFramework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.FX;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Athletic", menuName = "Properties/Athletic Property")]
public class Athletic : Effect
{
	public const float SPEED_MULTIPLIER = 1.3f;

	public const float NPC_SPEED_MULTIPLIER = 1.8f;

	public const float WorkSpeedMultiplier = 1.05f;

	[ColorUsage(true, true)]
	[SerializeField]
	public Color TintColor = Color.white;

	public override void ApplyToNPC(NPC npc)
	{
		npc.Avatar.Eyes.OverrideEyeLids(new Eye.EyeLidConfiguration
		{
			bottomLidOpen = 0.7f,
			topLidOpen = 0.8f
		});
		npc.Avatar.Eyes.ForceBlink();
		npc.Movement.SpeedController.SpeedMultiplier = 1.8f;
	}

	public override void ApplyToPlayer(Player player)
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		player.Avatar.Eyes.OverrideEyeLids(new Eye.EyeLidConfiguration
		{
			bottomLidOpen = 0.7f,
			topLidOpen = 0.8f
		});
		player.Avatar.Eyes.ForceBlink();
		if (((NetworkBehaviour)player).IsOwner)
		{
			PlayerSingleton<PlayerMovement>.Instance.MoveSpeedMultiplierStack.Add(new FloatStack.StackEntry("athletic", 1.3f, FloatStack.EStackMode.Multiplicative, Tier));
			PlayerSingleton<PlayerMovement>.Instance.ForceSprint = true;
			PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.AddOverride(10f, Tier, "athletic");
			PlayerSingleton<PlayerCamera>.Instance.HeartbeatSoundController.VolumeController.AddOverride(0.5f, Tier, "athletic");
			PlayerSingleton<PlayerCamera>.Instance.HeartbeatSoundController.PitchController.AddOverride(1.7f, Tier, "athletic");
			Singleton<PostProcessingManager>.Instance.ColorFilterController.AddOverride(TintColor, Tier, "athletic");
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
			PlayerSingleton<PlayerMovement>.Instance.MoveSpeedMultiplierStack.Remove("athletic");
			PlayerSingleton<PlayerMovement>.Instance.ForceSprint = false;
			PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.RemoveOverride("athletic");
			PlayerSingleton<PlayerCamera>.Instance.HeartbeatSoundController.VolumeController.RemoveOverride("athletic");
			PlayerSingleton<PlayerCamera>.Instance.HeartbeatSoundController.PitchController.RemoveOverride("athletic");
			Singleton<PostProcessingManager>.Instance.ColorFilterController.RemoveOverride("athletic");
		}
	}

	protected override void ApplyToEmployee(Employee employee)
	{
		base.ApplyToEmployee(employee);
		employee.WorkSpeedController.Add(new FloatStack.StackEntry(Name, 1.05f, FloatStack.EStackMode.Multiplicative, Tier));
	}

	protected override void ClearFromEmployee(Employee employee)
	{
		base.ClearFromEmployee(employee);
		employee.WorkSpeedController.Remove(Name);
	}
}
