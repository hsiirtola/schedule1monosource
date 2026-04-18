using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "TropicThunder", menuName = "Properties/TropicThunder Property")]
public class TropicThunder : Effect
{
	public override void ApplyToNPC(NPC npc)
	{
		npc.Avatar.Effects.SetSkinColorInverted(inverted: true);
	}

	public override void ApplyToPlayer(Player player)
	{
		player.Avatar.Effects.SetSkinColorInverted(inverted: true);
	}

	public override void ClearFromNPC(NPC npc)
	{
		npc.Avatar.Effects.SetSkinColorInverted(inverted: false);
	}

	public override void ClearFromPlayer(Player player)
	{
		player.Avatar.Effects.SetSkinColorInverted(inverted: false);
	}
}
