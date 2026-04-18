using System;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Clothing;

public static class ClothingColorExtensions
{
	public static Color GetActualColor(this EClothingColor color)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Singleton<ClothingUtility>.Instance.GetColorData(color).ActualColor;
	}

	public static Color GetLabelColor(this EClothingColor color)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Singleton<ClothingUtility>.Instance.GetColorData(color).LabelColor;
	}

	public static string GetLabel(this EClothingColor color)
	{
		return color.ToString();
	}

	public unsafe static EClothingColor GetClothingColor(Color color)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		foreach (EClothingColor value in Enum.GetValues(typeof(EClothingColor)))
		{
			if (ColorEquals(value.GetActualColor(), color))
			{
				return value;
			}
		}
		Color val = color;
		Console.LogError("Could not find clothing color for color " + ((object)(*(Color*)(&val))/*cast due to .constrained prefix*/).ToString());
		return EClothingColor.White;
	}

	public static bool ColorEquals(Color a, Color b, float tolerance = 0.004f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (a.r > b.r + tolerance)
		{
			return false;
		}
		if (a.g > b.g + tolerance)
		{
			return false;
		}
		if (a.b > b.b + tolerance)
		{
			return false;
		}
		if (a.r < b.r - tolerance)
		{
			return false;
		}
		if (a.g < b.g - tolerance)
		{
			return false;
		}
		if (a.b < b.b - tolerance)
		{
			return false;
		}
		return true;
	}
}
