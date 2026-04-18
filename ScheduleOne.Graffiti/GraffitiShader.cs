using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ScheduleOne.Graffiti;

public class GraffitiShader
{
	public struct StrokeData
	{
		public uint2 Start;

		public uint2 End;

		public uint Color;

		public uint Size;

		public static int Stride => 24;
	}

	private int _kernal;

	private ComputeShader _shader;

	private Texture2D _texture;

	private int _width;

	private int _height;

	private List<StrokeData> _strokes = new List<StrokeData>();

	private float[] _falloffTable;

	public void Initialise(Texture2D texture, int minStrokeSize, int maxStrokeSize, AnimationCurve falloffCurve)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected O, but got Unknown
		_shader = Resources.Load<ComputeShader>("Shaders/GraffitiShader");
		_kernal = _shader.FindKernel("CSMain");
		_texture = texture;
		_width = ((Texture)texture).width;
		_height = ((Texture)texture).height;
		CreateFalloffTables(minStrokeSize / 2, maxStrokeSize / 2, falloffCurve);
		_shader.SetInt("Width", _width);
		_shader.SetInt("Height", _height);
		_shader.SetInt("MinStrokeSize", minStrokeSize);
		_shader.SetInt("MaxStrokeSize", maxStrokeSize);
		_shader.SetBuffer(_kernal, "FalloffTable", new ComputeBuffer(_falloffTable.Length, 4));
		_shader.SetTexture(_kernal, "Result", (Texture)(object)texture);
	}

	public void Draw()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		ComputeBuffer val = new ComputeBuffer(_strokes.Count, StrokeData.Stride);
		val.SetData((Array)_strokes.ToArray());
		int num = Mathf.NextPowerOfTwo(_strokes.Count);
		_shader.SetBuffer(_kernal, "Strokes", val);
		_shader.Dispatch(_kernal, _width / 8, _height / 8, num / 8);
		val.Dispose();
	}

	public void ClearStrokes()
	{
		_strokes.Clear();
	}

	public void AddStrokes(List<SprayStroke> strokes)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (_strokes == null)
		{
			_strokes = new List<StrokeData>();
		}
		foreach (SprayStroke stroke in strokes)
		{
			StrokeData item = new StrokeData
			{
				Start = new uint2((uint)stroke.Start.X, (uint)stroke.Start.Y),
				End = new uint2((uint)stroke.End.X, (uint)stroke.End.Y),
				Color = (uint)stroke.Color,
				Size = stroke.StrokeSize
			};
			_strokes.Add(item);
		}
	}

	public void RemoveStrokes(int count)
	{
		if (_strokes != null && _strokes.Count != 0)
		{
			int num = Mathf.Min(count, _strokes.Count);
			_strokes.RemoveRange(_strokes.Count - num, num);
		}
	}

	private void CreateFalloffTables(int minFalloff, int maxFalloff, AnimationCurve falloffCurve)
	{
		int num = maxFalloff * maxFalloff * 4;
		int num2 = maxFalloff - minFalloff + 1;
		_falloffTable = new float[num2 * num];
		for (int i = 0; i < num2; i++)
		{
			int num3 = minFalloff + i;
			int num4 = i * num;
			int num5 = 0;
			for (int j = -num3; j < num3; j++)
			{
				for (int k = -num3; k < num3; k++)
				{
					float num6 = Mathf.Sqrt((float)(j * j + k * k)) / (float)num3;
					float num7 = ((num6 <= 1f) ? falloffCurve.Evaluate(num6) : 0f);
					if (num7 < 0.01f)
					{
						num7 = 0f;
					}
					_falloffTable[num4 + num5] = num7;
					num5++;
				}
			}
		}
	}
}
