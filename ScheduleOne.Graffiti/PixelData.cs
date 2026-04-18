using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Graffiti;

public class PixelData
{
	public UShort2 Coordinate;

	public ESprayColor Color;

	public byte StrokeSize;

	public byte StrokeRadiusRoundedUp => (byte)Mathf.CeilToInt((float)(int)StrokeSize / 2f);

	public byte StrokeRadiusRoundedDown => (byte)Mathf.FloorToInt((float)(int)StrokeSize / 2f);

	public PixelData(UShort2 coordinate, ESprayColor color, byte strokeSize)
	{
		Coordinate = coordinate;
		Color = color;
		StrokeSize = strokeSize;
	}

	public override string ToString()
	{
		return $"PixelData(Coordinate: {Coordinate}, Color: {Color})";
	}

	public float GetPixelStrength(int pixelIndex)
	{
		return NetworkSingleton<GraffitiManager>.Instance.GetPixelStrength(StrokeSize, pixelIndex);
	}
}
