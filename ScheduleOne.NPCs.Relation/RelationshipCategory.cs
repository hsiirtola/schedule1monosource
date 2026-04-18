using UnityEngine;

namespace ScheduleOne.NPCs.Relation;

public class RelationshipCategory
{
	public static Color32 Hostile_Color = new Color32((byte)173, (byte)63, (byte)63, byte.MaxValue);

	public static Color32 Unfriendly_Color = new Color32((byte)227, (byte)136, (byte)55, byte.MaxValue);

	public static Color32 Neutral_Color = new Color32((byte)208, (byte)208, (byte)208, byte.MaxValue);

	public static Color32 Friendly_Color = new Color32((byte)61, (byte)181, (byte)243, byte.MaxValue);

	public static Color32 Loyal_Color = new Color32((byte)63, (byte)211, (byte)63, byte.MaxValue);

	public static ERelationshipCategory GetCategory(float delta)
	{
		if (delta >= 4f)
		{
			return ERelationshipCategory.Loyal;
		}
		if (delta >= 3f)
		{
			return ERelationshipCategory.Friendly;
		}
		if (delta >= 2f)
		{
			return ERelationshipCategory.Neutral;
		}
		if (delta >= 1f)
		{
			return ERelationshipCategory.Unfriendly;
		}
		return ERelationshipCategory.Hostile;
	}

	public static Color32 GetColor(ERelationshipCategory category)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		switch (category)
		{
		case ERelationshipCategory.Hostile:
			return Hostile_Color;
		case ERelationshipCategory.Unfriendly:
			return Unfriendly_Color;
		case ERelationshipCategory.Neutral:
			return Neutral_Color;
		case ERelationshipCategory.Friendly:
			return Friendly_Color;
		case ERelationshipCategory.Loyal:
			return Loyal_Color;
		default:
			Console.LogError("Failed to find relationship category color");
			return Color32.op_Implicit(Color.white);
		}
	}
}
