using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "LongFaced", menuName = "Properties/LongFaced Property")]
public class LongFaced : Effect
{
	public override void ApplyToNPC(NPC npc)
	{
		npc.Avatar.Effects.SetGiraffeActive(active: true);
	}

	public override void ApplyToPlayer(Player player)
	{
		player.Avatar.Effects.SetGiraffeActive(active: true);
		if (((NetworkBehaviour)player).IsOwner)
		{
			PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.AddOverride(15f, Tier, "longfaced");
		}
	}

	public override void ClearFromNPC(NPC npc)
	{
		npc.Avatar.Effects.SetGiraffeActive(active: false);
	}

	public override void ClearFromPlayer(Player player)
	{
		player.Avatar.Effects.SetGiraffeActive(active: false);
		if (((NetworkBehaviour)player).IsOwner)
		{
			PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.RemoveOverride("longfaced");
		}
	}
}
