using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

public class PowerLineUtility
{
	public static int MinSegmentCount = 3;

	public static int MaxSegmentCount = 10;

	public static int GetSegmentCount(Vector3 startPoint, Vector3 endPoint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Distance(startPoint, endPoint);
		int num2 = (int)((float)(MaxSegmentCount - MinSegmentCount) * Mathf.Clamp(num / 20f, 0f, 1f));
		return MinSegmentCount + num2;
	}

	public static void DrawPowerLine(Vector3 startPoint, Vector3 endPoint, List<Transform> segments, float lengthFactor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		PositionSegments(GetCatenaryPoints(startPoint, endPoint, segments.Count, lengthFactor), segments);
	}

	private static void PositionSegments(List<Vector3> points, List<Transform> segments)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < segments.Count; i++)
		{
			((Component)segments[i]).transform.position = (points[i] + points[i + 1]) / 2f;
			((Component)segments[i]).transform.forward = points[i + 1] - points[i];
			segments[i].localScale = new Vector3(segments[i].localScale.x, segments[i].localScale.y, Vector3.Distance(points[i], points[i + 1]));
		}
	}

	private static List<Vector3> GetCatenaryPoints(Vector3 startPoint, Vector3 endPoint, int pointCount, float l)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = startPoint;
		Vector3 val2 = endPoint;
		List<Vector3> list = new List<Vector3>();
		l *= Vector3.Distance(startPoint, endPoint);
		Vector3 val3 = endPoint - startPoint;
		val3.y = 0f;
		val3 = ((Vector3)(ref val3)).normalized;
		_ = Vector3.up;
		endPoint.y -= startPoint.y;
		endPoint.x = Vector3.Distance(new Vector3(startPoint.x, 0f, startPoint.z), new Vector3(endPoint.x, 0f, endPoint.z));
		startPoint = Vector3.zero;
		float num = endPoint.y - startPoint.y;
		float num2 = endPoint.x - startPoint.x;
		int num3 = 0;
		float num4 = 0.01f * Mathf.Pow(Mathf.Clamp(Vector3.Distance(val, val2), 1f, float.MaxValue), 2f);
		float num5 = 1f;
		do
		{
			num5 += num4;
			num3++;
		}
		while ((double)Mathf.Sqrt(Mathf.Pow(l, 2f) - Mathf.Pow(num, 2f)) < (double)(2f * num5) * System.Math.Sinh(num2 / (2f * num5)));
		int num6 = 0;
		float num7 = 0.001f;
		float num8 = num5 - num4;
		float num9 = num5;
		do
		{
			num6++;
			num5 = (num8 + num9) / 2f;
			if ((double)Mathf.Sqrt(Mathf.Pow(l, 2f) - Mathf.Pow(num, 2f)) < (double)(2f * num5) * System.Math.Sinh(num2 / (2f * num5)))
			{
				num8 = num5;
			}
			else
			{
				num9 = num5;
			}
		}
		while (num9 - num8 > num7);
		float num10 = (startPoint.x + endPoint.x - num5 * Mathf.Log((l + num) / (l - num))) / 2f;
		float num11 = (float)(System.Math.Cosh((double)num2 / (2.0 * (double)num5)) / System.Math.Sinh((double)num2 / (2.0 * (double)num5)));
		float num12 = (startPoint.y + endPoint.y - l * num11) / 2f;
		float num13 = endPoint.x / (float)pointCount;
		List<Vector2> list2 = new List<Vector2>();
		for (int i = 0; i <= pointCount; i++)
		{
			float num14 = num13 * (float)i;
			float num15 = (float)((double)num5 * System.Math.Cosh((num14 - num10) / num5) + (double)num12);
			list2.Add(new Vector2(num14, num15));
		}
		for (int j = 0; j < list2.Count; j++)
		{
			Vector3 item = val + val3 * list2[j].x;
			item.y = val.y + list2[j].y;
			list.Add(item);
		}
		return list;
	}
}
