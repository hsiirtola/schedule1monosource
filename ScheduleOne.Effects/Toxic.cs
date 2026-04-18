using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Toxic", menuName = "Properties/Toxic Property")]
public class Toxic : Effect
{
	[ColorUsage(true, true)]
	[SerializeField]
	public Color TintColor = Color.white;

	public override void ApplyToNPC(NPC npc)
	{
		npc.Avatar.Effects.TriggerSick();
		npc.Avatar.EmotionManager.AddEmotionOverride("Concerned", "toxic", 30f, 1);
	}

	public override void ApplyToPlayer(Player player)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		player.Avatar.Effects.TriggerSick();
		player.Avatar.EmotionManager.AddEmotionOverride("Concerned", "toxic", 30f, 1);
		if (((NetworkBehaviour)player).Owner.IsLocalClient)
		{
			Singleton<PostProcessingManager>.Instance.ColorFilterController.AddOverride(TintColor, Tier, "Toxic");
		}
	}

	public override void ClearFromNPC(NPC npc)
	{
	}

	public override void ClearFromPlayer(Player player)
	{
		if (((NetworkBehaviour)player).Owner.IsLocalClient)
		{
			Singleton<PostProcessingManager>.Instance.ColorFilterController.RemoveOverride("Toxic");
		}
	}
}
