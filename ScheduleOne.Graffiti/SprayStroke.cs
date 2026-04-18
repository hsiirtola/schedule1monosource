using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ScheduleOne.Graffiti;

[Serializable]
public class SprayStroke
{
	public const int MinStrokeLength = 6;

	public const int AngleThreshold_Degrees = 10;

	public const float MaxStrokeDeviation = 5f;

	public const int ForwardSampleCount = 5;

	public const byte StrokeSize_LegacyDefault = 16;

	public const byte StrokeSize_Small = 10;

	public const byte StrokeSize_Medium = 16;

	public const byte StrokeSize_Large = 24;

	public const byte StrokeSize_ExtraLarge = 32;

	public static readonly byte[] StrokeSizePresets = new byte[4] { 10, 16, 24, 32 };

	public const byte StrokeSize_Min = 10;

	public const byte StrokeSize_Max = 32;

	public UShort2 Start;

	public UShort2 End;

	public ESprayColor Color;

	public byte StrokeSize;

	public SprayStroke(UShort2 start, UShort2 end, ESprayColor color, byte strokeSize)
	{
		Start = start;
		End = end;
		Color = color;
		StrokeSize = strokeSize;
	}

	public SprayStroke GetCopy()
	{
		return new SprayStroke(Start, End, Color, StrokeSize);
	}

	public SprayStroke()
	{
	}

	public List<PixelData> GetPixelsFromStroke()
	{
		List<PixelData> list = new List<PixelData>();
		int num = End.X - Start.X;
		int num2 = End.Y - Start.Y;
		int num3 = Mathf.Max(Mathf.Abs(num), Mathf.Abs(num2));
		if (num3 == 0)
		{
			list.Add(new PixelData(Start, Color, StrokeSize));
			return list;
		}
		for (int i = 0; i <= num3; i++)
		{
			float num4 = (float)i / (float)num3;
			int num5 = Mathf.RoundToInt(Mathf.Lerp((float)(int)Start.X, (float)(int)End.X, num4));
			int num6 = Mathf.RoundToInt(Mathf.Lerp((float)(int)Start.Y, (float)(int)End.Y, num4));
			if (num5 > 0 || num6 > 0)
			{
				list.Add(new PixelData(new UShort2((ushort)num5, (ushort)num6), Color, StrokeSize));
			}
		}
		return list;
	}

	public static List<SprayStroke> GetStrokesFromPixels(List<UShort2> coords, ESprayColor color, byte strokeSize, SpraySurface surface)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		List<SprayStroke> list = new List<SprayStroke>();
		if (coords.Count < 2)
		{
			if (coords.Count == 0)
			{
				return list;
			}
			list.Add(new SprayStroke(coords[0], coords[0], color, strokeSize));
			return list;
		}
		Vector2[] array = (Vector2[])(object)new Vector2[coords.Count];
		for (int i = 0; i < coords.Count; i++)
		{
			Vector2 val = coords[i];
			Vector2 val2 = Vector2.zero;
			int num = 0;
			for (int j = 1; j <= 5 && i + j < coords.Count; j++)
			{
				val2 += (Vector2)coords[i + j] - val;
				num++;
			}
			array[i] = val2 / (float)Mathf.Max(1, num);
		}
		UShort2 uShort = coords[0];
		int num2 = 0;
		float num3 = 0f;
		for (int k = 1; k < coords.Count - 1; k++)
		{
			int num4 = k - num2 + 1;
			if (num4 >= 6)
			{
				Vector2 val3 = (Vector2)coords[k] - (Vector2)uShort;
				Vector2 val4 = array[k];
				Vector2 val5 = (Vector2)coords[k] - (Vector2)uShort;
				Vector2 val6 = new Vector2(0f - val3.y, val3.x);
				float num5 = Mathf.Abs(Vector2.Dot(val5, ((Vector2)(ref val6)).normalized));
				if (num5 > num3)
				{
					num3 = num5;
				}
				float num6 = Mathf.Lerp(60f, 10f, Mathf.Clamp01((float)num4 / 10f));
				float num7 = Vector2.Angle(val3, val4);
				if (num3 > 5f || num7 > num6)
				{
					SprayStroke item = new SprayStroke(uShort, coords[k], color, strokeSize);
					uShort = coords[k];
					num2 = k;
					num3 = 0f;
					list.Add(item);
				}
			}
		}
		SprayStroke item2 = new SprayStroke(uShort, coords[coords.Count - 1], color, strokeSize);
		list.Add(item2);
		return list;
	}

	public void Serialize(BinaryWriter writer)
	{
		writer.Write(Start.X);
		writer.Write(Start.Y);
		writer.Write(End.X);
		writer.Write(End.Y);
		writer.Write((byte)Color);
	}

	public static SprayStroke Deserialize(BinaryReader reader)
	{
		return new SprayStroke
		{
			Start = new UShort2(reader.ReadUInt16(), reader.ReadUInt16()),
			End = new UShort2(reader.ReadUInt16(), reader.ReadUInt16()),
			Color = (ESprayColor)reader.ReadByte()
		};
	}

	public static List<SprayStroke> CopyAndShiftStrokes(List<SprayStroke> strokes, UShort2 shift)
	{
		List<SprayStroke> list = new List<SprayStroke>();
		foreach (SprayStroke stroke in strokes)
		{
			SprayStroke copy = stroke.GetCopy();
			copy.Start = new UShort2((ushort)(copy.Start.X + shift.X), (ushort)(copy.Start.Y + shift.Y));
			copy.End = new UShort2((ushort)(copy.End.X + shift.X), (ushort)(copy.End.Y + shift.Y));
			list.Add(copy);
		}
		return list;
	}

	public static void GetBounds(List<SprayStroke> strokes, out UShort2 min, out UShort2 max)
	{
		max = new UShort2(0, 0);
		min = new UShort2(ushort.MaxValue, ushort.MaxValue);
		foreach (SprayStroke stroke in strokes)
		{
			GetStrokeBounds(stroke, out var min2, out var max2);
			max.X = (ushort)Mathf.Max((int)max.X, (int)max2.X);
			max.Y = (ushort)Mathf.Max((int)max.Y, (int)max2.Y);
			min.X = (ushort)Mathf.Min((int)min.X, (int)min2.X);
			min.Y = (ushort)Mathf.Min((int)min.Y, (int)min2.Y);
		}
		static void GetStrokeBounds(SprayStroke stroke, out UShort2 reference, out UShort2 reference2)
		{
			reference = new UShort2(ushort.MaxValue, ushort.MaxValue);
			reference2 = new UShort2(0, 0);
			ushort num = (ushort)Mathf.CeilToInt((float)(int)stroke.StrokeSize / 2f);
			if (stroke.Start.X - num < reference.X)
			{
				reference.X = (ushort)Mathf.Max(0, stroke.Start.X - num);
			}
			if (stroke.Start.Y - num < reference.Y)
			{
				reference.Y = (ushort)Mathf.Max(0, stroke.Start.Y - num);
			}
			if (stroke.Start.X + num > reference2.X)
			{
				reference2.X = (ushort)(stroke.Start.X + num);
			}
			if (stroke.Start.Y + num > reference2.Y)
			{
				reference2.Y = (ushort)(stroke.Start.Y + num);
			}
			if (stroke.End.X - num < reference.X)
			{
				reference.X = (ushort)Mathf.Max(0, stroke.End.X - num);
			}
			if (stroke.End.Y - num < reference.Y)
			{
				reference.Y = (ushort)Mathf.Max(0, stroke.End.Y - num);
			}
			if (stroke.End.X + num > reference2.X)
			{
				reference2.X = (ushort)(stroke.End.X + num);
			}
			if (stroke.End.Y + num > reference2.Y)
			{
				reference2.Y = (ushort)(stroke.End.Y + num);
			}
		}
	}
}
