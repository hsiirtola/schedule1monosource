using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Gingeritis", menuName = "Properties/Gingeritis Property")]
public class Gingeritis : Effect
{
	public static Color32 Color = new Color32((byte)198, (byte)113, (byte)34, byte.MaxValue);

	public override void ApplyToNPC(NPC npc)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		npc.Avatar.Effects.OverrideHairColor(Color32.op_Implicit(Color));
	}

	public override void ApplyToPlayer(Player player)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		player.Avatar.Effects.OverrideHairColor(Color32.op_Implicit(Color));
	}

	public override void ClearFromNPC(NPC npc)
	{
		npc.Avatar.Effects.ResetHairColor();
	}

	public override void ClearFromPlayer(Player player)
	{
		player.Avatar.Effects.ResetHairColor();
	}
}
