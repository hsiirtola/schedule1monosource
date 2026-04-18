using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Effects;

[CreateAssetMenu(fileName = "BrightEyed", menuName = "Properties/BrightEyed Property")]
public class BrightEyed : Effect
{
	public Color EyeColor;

	public float Emission = 0.5f;

	public float LightIntensity = 1f;

	public override void ApplyToNPC(NPC npc)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		npc.Avatar.Effects.OverrideEyeColor(EyeColor, Emission);
		npc.Avatar.Effects.SetEyeLightEmission(LightIntensity, EyeColor);
	}

	public override void ApplyToPlayer(Player player)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		player.Avatar.Effects.OverrideEyeColor(EyeColor, Emission);
		player.Avatar.Effects.SetEyeLightEmission(LightIntensity, EyeColor);
	}

	public override void ClearFromNPC(NPC npc)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		npc.Avatar.Effects.ResetEyeColor();
		npc.Avatar.Effects.SetEyeLightEmission(0f, EyeColor);
	}

	public override void ClearFromPlayer(Player player)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		player.Avatar.Effects.ResetEyeColor();
		player.Avatar.Effects.SetEyeLightEmission(0f, EyeColor);
	}
}
