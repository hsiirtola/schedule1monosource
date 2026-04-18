using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Explosive", menuName = "Properties/Explosive Property")]
public class Explosive : Effect
{
	public override void ApplyToNPC(NPC npc)
	{
		npc.Avatar.Effects.TriggerCountdownExplosion(mirror: false);
	}

	public override void ApplyToPlayer(Player player)
	{
		player.Avatar.Effects.TriggerCountdownExplosion(mirror: false);
	}

	public override void ClearFromNPC(NPC npc)
	{
		npc.Avatar.Effects.StopCountdownExplosion(mirror: false);
	}

	public override void ClearFromPlayer(Player player)
	{
		player.Avatar.Effects.StopCountdownExplosion(mirror: false);
	}
}
