using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Spicy", menuName = "Properties/Spicy Property")]
public class Spicy : Effect
{
	[ColorUsage(true, true)]
	[SerializeField]
	public Color TintColor = Color.white;

	public override void ApplyToNPC(NPC npc)
	{
		npc.Avatar.Effects.SetFireActive(active: true);
	}

	public override void ApplyToPlayer(Player player)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		player.Avatar.Effects.SetFireActive(active: true);
		if (((NetworkBehaviour)player).Owner.IsLocalClient)
		{
			Singleton<PostProcessingManager>.Instance.ColorFilterController.AddOverride(TintColor, Tier, ((Object)this).name);
		}
	}

	public override void ClearFromNPC(NPC npc)
	{
		npc.Avatar.Effects.SetFireActive(active: false);
	}

	public override void ClearFromPlayer(Player player)
	{
		player.Avatar.Effects.SetFireActive(active: false);
		if (((NetworkBehaviour)player).Owner.IsLocalClient)
		{
			Singleton<PostProcessingManager>.Instance.ColorFilterController.RemoveOverride(((Object)this).name);
		}
	}
}
