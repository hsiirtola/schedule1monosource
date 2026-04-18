using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Smelly", menuName = "Properties/Smelly Property")]
public class Smelly : Effect
{
	public override void ApplyToNPC(NPC npc)
	{
		npc.Avatar.Effects.SetStinkParticlesActive(active: true);
	}

	public override void ApplyToPlayer(Player player)
	{
		player.Avatar.Effects.SetStinkParticlesActive(active: true);
		if (((NetworkBehaviour)player).Owner.IsLocalClient)
		{
			PlayerSingleton<PlayerCamera>.Instance.Flies.Play();
		}
	}

	public override void ClearFromNPC(NPC npc)
	{
		npc.Avatar.Effects.SetStinkParticlesActive(active: false);
	}

	public override void ClearFromPlayer(Player player)
	{
		player.Avatar.Effects.SetStinkParticlesActive(active: false);
		if (((NetworkBehaviour)player).Owner.IsLocalClient)
		{
			PlayerSingleton<PlayerCamera>.Instance.Flies.Stop();
		}
	}
}
