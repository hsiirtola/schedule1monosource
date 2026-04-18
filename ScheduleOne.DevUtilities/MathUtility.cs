using UnityEngine;

namespace ScheduleOne.DevUtilities;

public static class MathUtility
{
	public static bool PointInsideCube(Vector3 point, Vector3 center, Vector3 halfExtents)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = point - center;
		if (Mathf.Abs(val.x) <= halfExtents.x && Mathf.Abs(val.y) <= halfExtents.y)
		{
			return Mathf.Abs(val.z) <= halfExtents.z;
		}
		return false;
	}

	public static bool PointInsideRectangle(Vector2 point, Vector2 center, Vector2 halfExtents)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = point - center;
		if (Mathf.Abs(val.x) <= halfExtents.x)
		{
			return Mathf.Abs(val.y) <= halfExtents.y;
		}
		return false;
	}

	public static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = b - a;
		float num = Vector2.Dot(val, val);
		if (num <= Mathf.Epsilon)
		{
			return a;
		}
		float num2 = Vector2.Dot(point - a, val) / num;
		num2 = Mathf.Clamp01(num2);
		return a + val * num2;
	}

	public static Vector3 ClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = b - a;
		float num = Vector3.Dot(val, val);
		if (num <= Mathf.Epsilon)
		{
			return a;
		}
		float num2 = Vector3.Dot(point - a, val) / num;
		num2 = Mathf.Clamp01(num2);
		return a + val * num2;
	}

	public static float GetNormalizedPositionAlongSegment(Vector2 a, Vector2 b, Vector2 c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector2.Dot(c - a, b - a);
		Vector2 val = b - a;
		return Mathf.Clamp01(num / ((Vector2)(ref val)).sqrMagnitude);
	}

	public static float GetNormalizedPositionAlongSegment(Vector3 a, Vector3 b, Vector3 c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(c - a, b - a);
		Vector3 val = b - a;
		return Mathf.Clamp01(num / ((Vector3)(ref val)).sqrMagnitude);
	}

	public static int GetWrappedIndex(int index, int change, int size)
	{
		return (index + change + size) % size;
	}

	public static bool BetweenValues(float value, float min, float max, bool maxInclusive = false, bool minInclusive = false)
	{
		if (!minInclusive)
		{
			if (!maxInclusive)
			{
				if (value < max)
				{
					return value > min;
				}
				return false;
			}
			if (value <= max)
			{
				return value > min;
			}
			return false;
		}
		if (!maxInclusive)
		{
			if (value < max)
			{
				return value >= min;
			}
			return false;
		}
		if (value <= max)
		{
			return value >= min;
		}
		return false;
	}

	public static float Normalise(float value, float min, float max)
	{
		return (value - min) / (max - min);
	}

	public static float SqrDistance(Vector3 a, Vector3 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = a - b;
		return val.x * val.x + val.y * val.y + val.z * val.z;
	}

	public static float InverseDistance01(Vector3 a, Vector3 b, float minDist, float maxDist)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = a - b;
		float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
		float num = minDist * minDist;
		float num2 = maxDist * maxDist;
		if (sqrMagnitude <= num)
		{
			return 1f;
		}
		if (sqrMagnitude >= num2)
		{
			return 0f;
		}
		return (num2 - sqrMagnitude) / (num2 - num);
	}

	public static float InverseDistance01(float sqrDist, float minDist, float maxDist)
	{
		float num = minDist * minDist;
		float num2 = maxDist * maxDist;
		if (sqrDist <= num)
		{
			return 1f;
		}
		if (sqrDist >= num2)
		{
			return 0f;
		}
		return (num2 - sqrDist) / (num2 - num);
	}

	public static bool NearlyEqual(float a, float b, float tolerance)
	{
		return Mathf.Abs(a - b) < tolerance;
	}

	public static float LogLerp(float a, float b, float t)
	{
		return Mathf.Exp(Mathf.Lerp(Mathf.Log(a), Mathf.Log(b), t));
	}

	public static Plane CreatePlaneFromPoints(Vector3 p1, Vector3 p2, Vector3 p3)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Cross(p2 - p1, p3 - p1);
		return new Plane(((Vector3)(ref val)).normalized, p1);
	}

	public static Vector3 ClosestPointOnPlane(in Plane plane, in Vector3 point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Plane val = plane;
		float num = Vector3.Dot(((Plane)(ref val)).normal, point);
		val = plane;
		float num2 = num + ((Plane)(ref val)).distance;
		Vector3 val2 = point;
		val = plane;
		return val2 - ((Plane)(ref val)).normal * num2;
	}

	public static Vector3 ClosestPointOnPlane(in Vector3 normal, float distance, in Vector3 point)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		float num = normal.x * point.x + normal.y * point.y + normal.z * point.z + distance;
		return new Vector3(point.x - normal.x * num, point.y - normal.y * num, point.z - normal.z * num);
	}

	public static Vector3 ClosestPointOnQuad(Vector3 point, Vector3 origin, Vector3 axisU, Vector3 axisV, float halfU, float halfV)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = point - origin;
		float num = Mathf.Clamp(Vector3.Dot(val, axisU), 0f - halfU, halfU);
		float num2 = Mathf.Clamp(Vector3.Dot(val, axisV), 0f - halfV, halfV);
		return origin + axisU * num + axisV * num2;
	}
}
