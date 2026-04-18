using System;
using UnityEngine;

namespace ScheduleOne.Graffiti;

[Serializable]
public struct UShort2(ushort x, ushort y)
{
	public ushort X = x;

	public ushort Y = y;

	public override string ToString()
	{
		return $"({X}, {Y})";
	}

	public static UShort2 operator +(UShort2 a, UShort2 b)
	{
		return new UShort2((ushort)(a.X + b.X), (ushort)(a.Y + b.Y));
	}

	public static UShort2 operator -(UShort2 a, UShort2 b)
	{
		return new UShort2((ushort)(a.X - b.X), (ushort)(a.Y - b.Y));
	}

	public static implicit operator Vector2(UShort2 uShort2)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2((float)(int)uShort2.X, (float)(int)uShort2.Y);
	}
}
