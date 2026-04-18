using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Glowie", menuName = "Properties/Glowie Property")]
public class Glowie : Effect
{
	[ColorUsage(true, true)]
	[SerializeField]
	public Color GlowColor = Color.white;

	public override void ApplyToNPC(NPC npc)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		npc.Avatar.Effects.SetGlowingOn(GlowColor);
	}

	public override void ApplyToPlayer(Player player)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		player.Avatar.Effects.SetGlowingOn(GlowColor);
	}

	public override void ClearFromNPC(NPC npc)
	{
		npc.Avatar.Effects.SetGlowingOff();
	}

	public override void ClearFromPlayer(Player player)
	{
		player.Avatar.Effects.SetGlowingOff();
	}
}
