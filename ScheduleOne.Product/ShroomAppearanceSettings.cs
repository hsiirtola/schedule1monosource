using System;
using UnityEngine;

namespace ScheduleOne.Product;

[Serializable]
public class ShroomAppearanceSettings
{
	public static readonly Color32 DefaultPrimaryColor = new Color32((byte)168, (byte)125, (byte)43, byte.MaxValue);

	public static readonly Color32 DefaultSecondaryColor = new Color32(byte.MaxValue, (byte)243, (byte)232, byte.MaxValue);

	public static readonly Color32 DefaultSpotsColor = Color32.op_Implicit(Color.clear);

	public Color32 PrimaryColor { get; private set; }

	public Color32 SecondaryColor { get; private set; }

	public bool HasSpots { get; private set; }

	public Color32 SpotsColor { get; private set; }

	public ShroomAppearanceSettings(Color32 primary, Color32 secondary, bool hasSpots, Color32 spotsColor)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		PrimaryColor = primary;
		SecondaryColor = secondary;
		HasSpots = hasSpots;
		SpotsColor = spotsColor;
	}

	public ShroomAppearanceSettings()
	{
	}

	public bool IsUnintialized()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!(Color32.op_Implicit(PrimaryColor) == Color.clear))
		{
			return Color32.op_Implicit(SecondaryColor) == Color.clear;
		}
		return true;
	}
}
