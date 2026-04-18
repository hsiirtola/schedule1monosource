using ScheduleOne.AvatarFramework;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "Electrifying", menuName = "Properties/Electrifying Property")]
public class Electrifying : Effect
{
	private static Color32 EyeColor = Color32.op_Implicit(new Color(112f, 217f, 255f, 255f));

	public override void ApplyToNPC(NPC npc)
	{
		ApplyToAvatar(npc.Avatar);
	}

	public override void ApplyToPlayer(Player player)
	{
		ApplyToAvatar(player.Avatar);
	}

	public override void ClearFromNPC(NPC npc)
	{
		ClearFromAvatar(npc.Avatar);
	}

	public override void ClearFromPlayer(Player player)
	{
		ClearFromAvatar(player.Avatar);
	}

	public static void ApplyToAvatar(Avatar avatar)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		avatar.Effects.SetZapped(zapped: true);
		avatar.Effects.OverrideEyeColor(Color32.op_Implicit(EyeColor), 0.5f);
	}

	public static void ClearFromAvatar(Avatar avatar)
	{
		avatar.Effects.SetZapped(zapped: false);
		avatar.Effects.ResetEyeColor();
	}
}
