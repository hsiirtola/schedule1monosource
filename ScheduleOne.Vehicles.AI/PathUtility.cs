using ScheduleOne.Math;
using UnityEngine;

namespace ScheduleOne.Vehicles.AI;

public static class PathUtility
{
	public static Vector3 GetAverageAheadPoint(PathSmoothingUtility.SmoothedPath path, Vector3 referencePoint, int sampleCount, float stepSize)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		GetClosestPointOnPath(path, referencePoint, out var startPointIndex, out var _, out var pointLerp);
		Vector3 val = Vector3.zero;
		for (int i = 1; i <= sampleCount; i++)
		{
			val += GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, (float)i * stepSize);
		}
		return val / (float)sampleCount;
	}

	public static Vector3 GetAheadPoint(PathSmoothingUtility.SmoothedPath path, Vector3 referencePoint, float distance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		GetClosestPointOnPath(path, referencePoint, out var startPointIndex, out var _, out var pointLerp);
		return GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, distance);
	}

	public static Vector3 GetAheadPoint(PathSmoothingUtility.SmoothedPath path, Vector3 referencePoint, float distance, int startPointIndex, float pointLerp)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, distance);
	}

	public static Vector3 GetPointAheadOfPathPoint(PathSmoothingUtility.SmoothedPath path, int startPointIndex, float pointLerp, float distanceAhead)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (path == null || path.vectorPath.Count < 2)
		{
			return Vector3.zero;
		}
		if (path.vectorPath.Count == startPointIndex + 1)
		{
			return path.vectorPath[startPointIndex];
		}
		float num = distanceAhead;
		Vector3 zero = Vector3.zero;
		int num2 = startPointIndex;
		while (num > 0f)
		{
			Vector3 val = path.vectorPath[num2] + (path.vectorPath[num2 + 1] - path.vectorPath[num2]) * pointLerp;
			pointLerp = 0f;
			Vector3 val2 = path.vectorPath[num2 + 1];
			if (Vector3.Distance(val, val2) > num)
			{
				Vector3 val3 = val2 - val;
				return val + ((Vector3)(ref val3)).normalized * num;
			}
			num -= Vector3.Distance(val, val2);
			num2++;
			if (path.vectorPath.Count <= num2 + 1)
			{
				return val2;
			}
		}
		return zero;
	}

	public static float CalculateAngleChangeOverPath(PathSmoothingUtility.SmoothedPath path, int startPointIndex, float pointLerp, float distanceAhead)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (path.vectorPath.Count == startPointIndex + 1)
		{
			return 0f;
		}
		float num = distanceAhead;
		int num2 = startPointIndex;
		float num3 = 0f;
		while (num > 0f)
		{
			Vector3 val = path.vectorPath[num2] + (path.vectorPath[num2 + 1] - path.vectorPath[num2]) * pointLerp;
			pointLerp = 0f;
			if (path.vectorPath.Count <= num2 + 2)
			{
				break;
			}
			Vector3 val2 = path.vectorPath[num2 + 1];
			Vector3 val3 = path.vectorPath[num2 + 2];
			if (Vector3.Distance(val, val2) > num)
			{
				break;
			}
			num -= Vector3.Distance(val, val2);
			num2++;
			float num4 = num3;
			Vector3 val4 = val3 - val2;
			Vector3 normalized = ((Vector3)(ref val4)).normalized;
			val4 = val2 - val;
			num3 = num4 + Vector3.Angle(normalized, ((Vector3)(ref val4)).normalized);
			if (path.vectorPath.Count <= num2 + 2)
			{
				break;
			}
		}
		return num3;
	}

	public static float CalculateCTE(Vector3 flatCarPos, Transform vehicleTransform, Vector3 wp_from, Vector3 wp_to, PathSmoothingUtility.SmoothedPath path)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		new Vector3(wp_from.x, flatCarPos.y, wp_from.z);
		new Vector3(wp_to.x, flatCarPos.y, wp_to.z);
		int startPointIndex;
		int endPointIndex;
		float pointLerp;
		Vector3 closestPointOnPath = GetClosestPointOnPath(path, flatCarPos, out startPointIndex, out endPointIndex, out pointLerp);
		Debug.DrawLine(flatCarPos, closestPointOnPath, Color.red);
		Vector3 val = closestPointOnPath - flatCarPos;
		return 0f - vehicleTransform.InverseTransformVector(Vector3.Project(val, vehicleTransform.right)).x;
	}

	public static Vector3 GetClosestPointOnPath(PathSmoothingUtility.SmoothedPath path, Vector3 point, out int startPointIndex, out int endPointIndex, out float pointLerp)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		startPointIndex = 0;
		endPointIndex = 0;
		pointLerp = 0f;
		if (path == null || path.vectorPath == null || path.vectorPath.Count < 2)
		{
			return Vector3.zero;
		}
		float num = float.MaxValue;
		Vector3 result = Vector3.zero;
		for (int i = 0; i < path.vectorPath.Count - 1; i++)
		{
			Bounds val = path.segmentBounds[i];
			if (((Bounds)(ref val)).Contains(point))
			{
				Vector3 val2 = path.vectorPath[i];
				Vector3 val3 = path.vectorPath[i + 1];
				Vector3 closestPointOnLine = GetClosestPointOnLine(point, val2, val3);
				Vector3 val4 = closestPointOnLine - point;
				float sqrMagnitude = ((Vector3)(ref val4)).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = closestPointOnLine;
					num = sqrMagnitude;
					startPointIndex = i;
					Vector3 val5 = val3 - val2;
					pointLerp = Vector3.Dot(closestPointOnLine - val2, ((Vector3)(ref val5)).normalized) / ((Vector3)(ref val5)).magnitude;
				}
			}
		}
		endPointIndex = startPointIndex + 1;
		return result;
	}

	public static Vector3 GetAheadPointDirection(PathSmoothingUtility.SmoothedPath path, Vector3 referencePoint, float distanceAhead)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		GetClosestPointOnPath(path, referencePoint, out var startPointIndex, out var _, out var pointLerp);
		Vector3 pointAheadOfPathPoint = GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, distanceAhead);
		Vector3 val = GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, distanceAhead + 0.01f) - pointAheadOfPathPoint;
		return ((Vector3)(ref val)).normalized;
	}

	private static Vector3 GetClosestPointOnLine(Vector3 point, Vector3 line_start, Vector3 line_end, bool clamp = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = line_end - line_start;
		float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
		if (sqrMagnitude < Mathf.Epsilon)
		{
			return line_start;
		}
		float num = Vector3.Dot(point - line_start, val) / sqrMagnitude;
		if (clamp)
		{
			num = Mathf.Clamp01(num);
		}
		return line_start + num * val;
	}
}
