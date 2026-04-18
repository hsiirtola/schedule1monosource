using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using ScheduleOne.Vision;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Sneaky", menuName = "Properties/Sneaky Property")]
public class Sneaky : Effect
{
	public const float SPEED_MULTIPLIER = 0.85f;

	public const float FOOTSTEP_VOL_MULTIPLIER = 0.4f;

	private VisibilityAttribute visibilityAttribute;

	public override void ApplyToNPC(NPC npc)
	{
		npc.Avatar.Animation.FootstepDetector.VolumeMultiplier = 0.4f;
	}

	public override void ApplyToPlayer(Player player)
	{
		player.Sneaky = true;
		visibilityAttribute = new VisibilityAttribute("sneaky", 0f, 0.6f);
		player.Avatar.Animation.FootstepDetector.VolumeMultiplier = 0.4f;
		if (((NetworkBehaviour)player).IsOwner)
		{
			Singleton<EyelidOverlay>.Instance.OpenMultiplier.AddOverride(0.8f, 6, "sneaky");
			PlayerSingleton<PlayerMovement>.Instance.MoveSpeedMultiplierStack.Add(new FloatStack.StackEntry("sneaky", 0.85f, FloatStack.EStackMode.Multiplicative, Tier));
		}
	}

	public override void ClearFromNPC(NPC npc)
	{
		npc.Avatar.Animation.FootstepDetector.VolumeMultiplier = 1f;
	}

	public override void ClearFromPlayer(Player player)
	{
		player.Sneaky = true;
		visibilityAttribute.Delete();
		player.Avatar.Animation.FootstepDetector.VolumeMultiplier = 1f;
		if (((NetworkBehaviour)player).IsOwner)
		{
			Singleton<EyelidOverlay>.Instance.OpenMultiplier.RemoveOverride("sneaky");
			PlayerSingleton<PlayerMovement>.Instance.MoveSpeedMultiplierStack.Remove("sneaky");
		}
	}
}
