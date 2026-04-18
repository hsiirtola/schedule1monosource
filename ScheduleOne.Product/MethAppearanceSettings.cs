using System;
using UnityEngine;

namespace ScheduleOne.Product;

[Serializable]
public class MethAppearanceSettings
{
	public Color32 MainColor;

	public Color32 SecondaryColor;

	public MethAppearanceSettings(Color32 mainColor, Color32 secondaryColor)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		MainColor = mainColor;
		SecondaryColor = secondaryColor;
	}

	public MethAppearanceSettings()
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
		if (!(Color32.op_Implicit(MainColor) == Color.clear))
		{
			return Color32.op_Implicit(SecondaryColor) == Color.clear;
		}
		return true;
	}
}
