using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "CalorieDense", menuName = "Properties/CalorieDense Property")]
public class CalorieDense : Effect
{
	public override void ApplyToNPC(NPC npc)
	{
		npc.Avatar.Effects.AddAdditionalWeightOverride(1f, 6, "calorie dense");
	}

	public override void ApplyToPlayer(Player player)
	{
		player.Avatar.Effects.AddAdditionalWeightOverride(1f, 6, "calorie dense");
	}

	public override void ClearFromNPC(NPC npc)
	{
		npc.Avatar.Effects.RemoveAdditionalWeightOverride("calorie dense");
	}

	public override void ClearFromPlayer(Player player)
	{
		player.Avatar.Effects.RemoveAdditionalWeightOverride("calorie dense");
	}
}
