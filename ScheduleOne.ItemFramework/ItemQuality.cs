using System;
using UnityEngine;

namespace ScheduleOne.ItemFramework;

public static class ItemQuality
{
	public const float Heavenly_Threshold = 0.9f;

	public const float Premium_Threshold = 0.75f;

	public const float Standard_Threshold = 0.4f;

	public const float Poor_Threshold = 0.25f;

	public static Color Heavenly_Color = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)200, (byte)50, byte.MaxValue));

	public static Color Premium_Color = Color32.op_Implicit(new Color32((byte)225, (byte)75, byte.MaxValue, byte.MaxValue));

	public static Color Standard_Color = Color32.op_Implicit(new Color32((byte)100, (byte)190, byte.MaxValue, byte.MaxValue));

	public static Color Poor_Color = Color32.op_Implicit(new Color32((byte)80, (byte)145, (byte)50, byte.MaxValue));

	public static Color Trash_Color = Color32.op_Implicit(new Color32((byte)125, (byte)50, (byte)50, byte.MaxValue));

	public static EQuality GetQuality(float qualityScalar)
	{
		if (qualityScalar > 0.9f)
		{
			return EQuality.Heavenly;
		}
		if (qualityScalar > 0.75f)
		{
			return EQuality.Premium;
		}
		if (qualityScalar > 0.4f)
		{
			return EQuality.Standard;
		}
		if (qualityScalar > 0.25f)
		{
			return EQuality.Poor;
		}
		return EQuality.Trash;
	}

	public static EQuality ShiftQuality(EQuality baseQuality, int shiftAmount)
	{
		return (EQuality)Mathf.Clamp((int)(baseQuality + shiftAmount), 0, Enum.GetValues(typeof(EQuality)).Length - 1);
	}

	public static Color GetColor(EQuality quality)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		switch (quality)
		{
		case EQuality.Heavenly:
			return Heavenly_Color;
		case EQuality.Premium:
			return Premium_Color;
		case EQuality.Standard:
			return Standard_Color;
		case EQuality.Poor:
			return Poor_Color;
		case EQuality.Trash:
			return Trash_Color;
		default:
			Console.LogWarning("Quality color not found!");
			return Color.white;
		}
	}
}
