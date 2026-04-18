using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Graffiti;

[CreateAssetMenu(fileName = "Graffiti Drawing", menuName = "Serialized Graffiti Drawing", order = 1)]
public class SerializedGraffitiDrawing : ScriptableObject
{
	[field: SerializeField]
	public string DrawingName { get; private set; } = "New Graffiti Drawing";

	[field: SerializeField]
	public int Width { get; private set; }

	[field: SerializeField]
	public int Height { get; private set; }

	[field: SerializeField]
	public List<SprayStroke> Strokes { get; private set; } = new List<SprayStroke>();

	public void SetDrawingName(string name)
	{
		DrawingName = name;
	}

	public void SetStrokes(List<SprayStroke> strokes)
	{
		Strokes = strokes;
		RecalculateSize();
	}

	private void RecalculateSize()
	{
		SprayStroke.GetBounds(Strokes, out var min, out var max);
		foreach (SprayStroke stroke in Strokes)
		{
			GetStrokeBounds(stroke, out var min2, out var max2);
			max.X = (ushort)Mathf.Max((int)max.X, (int)max2.X);
			max.Y = (ushort)Mathf.Max((int)max.Y, (int)max2.Y);
			min.X = (ushort)Mathf.Min((int)min.X, (int)min2.X);
			min.Y = (ushort)Mathf.Min((int)min.Y, (int)min2.Y);
		}
		Width = Mathf.Clamp(0, max.X - min.X, 65535);
		Height = Mathf.Clamp(0, max.Y - min.Y, 65535);
		Debug.Log((object)$"Recalculated drawing size: {Width}x{Height}");
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
