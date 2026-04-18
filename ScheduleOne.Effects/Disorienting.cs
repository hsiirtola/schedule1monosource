using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Disorienting", menuName = "Properties/Disorienting Property")]
public class Disorienting : Effect
{
	public override void ApplyToNPC(NPC npc)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		npc.Movement.Disoriented = true;
		npc.Avatar.Eyes.leftEye.AngleOffset = new Vector2(20f, 10f);
		npc.Avatar.EmotionManager.AddEmotionOverride("Concerned", "disoriented");
	}

	public override void ApplyToPlayer(Player player)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		player.Disoriented = true;
		player.Avatar.Eyes.leftEye.AngleOffset = new Vector2(20f, 10f);
		if (((NetworkBehaviour)player).IsOwner)
		{
			PlayerSingleton<PlayerCamera>.Instance.SmoothLookSmoother.AddOverride(0.8f, Tier, "disoriented");
		}
	}

	public override void ClearFromNPC(NPC npc)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		npc.Movement.Disoriented = false;
		npc.Avatar.Eyes.leftEye.AngleOffset = Vector2.zero;
		npc.Avatar.Eyes.rightEye.AngleOffset = Vector2.zero;
		npc.Avatar.EmotionManager.RemoveEmotionOverride("disoriented");
	}

	public override void ClearFromPlayer(Player player)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		player.Disoriented = false;
		player.Avatar.Eyes.leftEye.AngleOffset = Vector2.zero;
		player.Avatar.Eyes.rightEye.AngleOffset = Vector2.zero;
		if (((NetworkBehaviour)player).IsOwner)
		{
			PlayerSingleton<PlayerCamera>.Instance.SmoothLookSmoother.RemoveOverride("disoriented");
		}
	}
}
