using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ScheduleOne.Graffiti;

public class Drawing
{
	private class DrawData
	{
		public List<DrawPixels> DrawPixels = new List<DrawPixels>();

		public void Add(DrawPixels drawPixels)
		{
			DrawPixels.Add(drawPixels);
		}

		public bool IsEmpty()
		{
			return DrawPixels.Count == 0;
		}

		public void Clear()
		{
			DrawPixels.Clear();
		}
	}

	private class DrawPixels
	{
		public int BottomLeftX;

		public int BottomLeftY;

		public int BlockWidth;

		public Color[] Colors;

		public DrawPixels(int bottomLeftX, int bottomLeftY, int blockWidth, Color[] colors)
		{
			BottomLeftX = bottomLeftX;
			BottomLeftY = bottomLeftY;
			BlockWidth = blockWidth;
			Colors = colors;
		}
	}

	private List<SprayStroke> strokes = new List<SprayStroke>();

	private Texture2DArray _historyTextureArray;

	private int[] PaintedPixelHistory = new int[11];

	private int[] _strokeHistory = new int[10];

	private const int MAX_UNDO_STATES = 10;

	public Action onTextureChanged;

	private int _width { get; set; }

	private int _height { get; set; }

	public int TextureWidth => Mathf.NextPowerOfTwo(_width);

	public int TextureHeight => Mathf.NextPowerOfTwo(_height);

	public Texture2D OutputTexture { get; private set; }

	public int StrokeCount => strokes.Count;

	public int PaintedPixelCount { get; set; }

	public int HistoryIndex { get; private set; } = -1;

	public int HistoryCount { get; private set; }

	public List<SprayStroke> GetStrokes()
	{
		return strokes;
	}

	public Drawing(int width, int height, bool initPixels)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		_width = width;
		_height = height;
		OutputTexture = new Texture2D(TextureWidth, TextureHeight, (TextureFormat)4, false);
		_historyTextureArray = new Texture2DArray(TextureWidth, TextureHeight, 11, (TextureFormat)4, false);
		if (initPixels)
		{
			Color[] array = (Color[])(object)new Color[TextureWidth * TextureHeight];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Color.clear;
			}
			OutputTexture.SetPixels(array);
			OutputTexture.Apply();
		}
	}

	public Drawing GetCopy()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		Drawing drawing = new Drawing(_width, _height, initPixels: false);
		drawing.OutputTexture = new Texture2D(TextureWidth, TextureHeight, (TextureFormat)4, false);
		Graphics.CopyTexture((Texture)(object)OutputTexture, 0, 0, (Texture)(object)drawing.OutputTexture, 0, 0);
		drawing.OutputTexture.Apply();
		drawing.strokes = new List<SprayStroke>();
		drawing.strokes.AddRange(strokes);
		drawing.PaintedPixelCount = PaintedPixelCount;
		drawing.HistoryIndex = HistoryIndex;
		drawing.HistoryCount = HistoryCount;
		return drawing;
	}

	public void DrawPaintedPixel(PixelData data, bool applyTexture)
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		PaintedPixelCount++;
		UShort2 uShort = new UShort2((ushort)(data.Coordinate.X - data.StrokeRadiusRoundedDown), (ushort)(data.Coordinate.Y - data.StrokeRadiusRoundedDown));
		if (!IsCoordinateInBounds(uShort.X, uShort.Y) || !IsCoordinateInBounds(uShort.X + data.StrokeSize - 1, uShort.Y + data.StrokeSize - 1))
		{
			UShort2 coordinate = data.Coordinate;
			Console.LogError("Pixel out of bounds: " + coordinate.ToString() + " with stroke size " + data.StrokeSize);
			return;
		}
		Color[] pixels = OutputTexture.GetPixels((int)uShort.X, (int)uShort.Y, (int)data.StrokeSize, (int)data.StrokeSize);
		Color color = data.Color.GetColor();
		int num = 0;
		for (int i = 0; i < data.StrokeSize; i++)
		{
			for (int j = 0; j < data.StrokeSize; j++)
			{
				float pixelStrength = data.GetPixelStrength(num);
				num++;
				if (!(pixelStrength <= 0.05f))
				{
					Color val = LerpUnclampedFast(pixels[j * data.StrokeSize + i], color, pixelStrength);
					pixels[j * data.StrokeSize + i] = val;
				}
			}
		}
		OutputTexture.SetPixels((int)uShort.X, (int)uShort.Y, (int)data.StrokeSize, (int)data.StrokeSize, pixels);
		if (applyTexture)
		{
			ApplyTexture();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Color LerpUnclampedFast(Color a, Color b, float t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t);
	}

	private void ApplyTexture()
	{
		OutputTexture.Apply();
		if (onTextureChanged != null)
		{
			onTextureChanged();
		}
	}

	private bool IsCoordinateInBounds(int x, int y)
	{
		if (x >= 0 && x < _width && y >= 0)
		{
			return y < _height;
		}
		return false;
	}

	public void AddStroke(SprayStroke stroke)
	{
		strokes.Add(stroke);
		List<PixelData> pixelsFromStroke = stroke.GetPixelsFromStroke();
		for (int i = 0; i < pixelsFromStroke.Count; i++)
		{
			DrawPaintedPixel(pixelsFromStroke[i], applyTexture: false);
		}
		ApplyTexture();
	}

	public void AddStrokes(List<SprayStroke> newStrokes)
	{
		if (newStrokes == null || newStrokes.Count == 0)
		{
			return;
		}
		strokes.AddRange(newStrokes);
		foreach (SprayStroke newStroke in newStrokes)
		{
			List<PixelData> pixelsFromStroke = newStroke.GetPixelsFromStroke();
			for (int i = 0; i < pixelsFromStroke.Count; i++)
			{
				DrawPaintedPixel(pixelsFromStroke[i], applyTexture: false);
			}
		}
		ApplyTexture();
	}

	public bool CanUndo()
	{
		return HistoryCount > 0;
	}

	public void Undo()
	{
		Color[] pixels = _historyTextureArray.GetPixels(HistoryIndex);
		OutputTexture.SetPixels(pixels);
		OutputTexture.Apply();
		PaintedPixelCount = PaintedPixelHistory[HistoryIndex];
		int count = strokes.Count - _strokeHistory[HistoryIndex];
		strokes.RemoveRange(_strokeHistory[HistoryIndex], count);
		HistoryIndex = (HistoryIndex - 1 + 10) % 10;
		HistoryCount--;
	}

	public void CacheDrawing()
	{
		PaintedPixelCount = PaintedPixelHistory[10];
		Color[] pixels = OutputTexture.GetPixels();
		_historyTextureArray.SetPixels(pixels, 10);
		_historyTextureArray.Apply();
	}

	public void RestoreFromCache()
	{
		PaintedPixelHistory[10] = PaintedPixelCount;
		Color[] pixels = _historyTextureArray.GetPixels(10);
		OutputTexture.SetPixels(pixels);
		OutputTexture.Apply();
	}

	public void AddTextureToHistory(bool saveToCache = false)
	{
		HistoryIndex = (HistoryIndex + 1 + 10) % 10;
		Color[] pixels = OutputTexture.GetPixels();
		_historyTextureArray.SetPixels(pixels, HistoryIndex);
		_historyTextureArray.SetPixels(pixels, 10);
		_historyTextureArray.Apply();
		PaintedPixelHistory[HistoryIndex] = PaintedPixelCount;
		_strokeHistory[HistoryIndex] = strokes.Count;
		HistoryCount = Mathf.Min(HistoryCount + 1, 10);
	}
}
