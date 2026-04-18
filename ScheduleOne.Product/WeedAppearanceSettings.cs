using System;
using UnityEngine;

namespace ScheduleOne.Product;

[Serializable]
public class WeedAppearanceSettings
{
	public enum EWeedAppearanceType
	{
		Main,
		Secondary,
		Leaf,
		Stem
	}

	public Color32 MainColor;

	public Color32 SecondaryColor;

	public Color32 LeafColor;

	public Color32 StemColor;

	public WeedAppearanceSettings(Color32 mainColor, Color32 secondaryColor, Color32 leafColor, Color32 stemColor)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		MainColor = mainColor;
		SecondaryColor = secondaryColor;
		LeafColor = leafColor;
		StemColor = stemColor;
	}

	public WeedAppearanceSettings()
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
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!(Color32.op_Implicit(MainColor) == Color.clear) && !(Color32.op_Implicit(SecondaryColor) == Color.clear) && !(Color32.op_Implicit(LeafColor) == Color.clear))
		{
			return Color32.op_Implicit(StemColor) == Color.clear;
		}
		return true;
	}
}
