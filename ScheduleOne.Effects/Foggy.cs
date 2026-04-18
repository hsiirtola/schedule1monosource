using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Foggy", menuName = "Properties/Foggy Property")]
public class Foggy : Effect
{
	public override void ApplyToNPC(NPC npc)
	{
		npc.Avatar.Effects.SetFoggy(active: true);
	}

	public override void ApplyToPlayer(Player player)
	{
		player.Avatar.Effects.SetFoggy(active: true);
		if (player.IsLocalPlayer)
		{
			Singleton<EnvironmentFX>.Instance.FogEndDistanceController.AddOverride(0.1f, Tier, ((Object)this).name);
		}
	}

	public override void ClearFromNPC(NPC npc)
	{
		npc.Avatar.Effects.SetFoggy(active: false);
	}

	public override void ClearFromPlayer(Player player)
	{
		player.Avatar.Effects.SetFoggy(active: false);
		if (player.IsLocalPlayer)
		{
			Singleton<EnvironmentFX>.Instance.FogEndDistanceController.RemoveOverride(((Object)this).name);
		}
	}
}
