using System;
using System.Linq;
using UnityEngine;

namespace ScheduleOne.Audio;

public class PolygonalZone : MonoBehaviour
{
	public Transform PointContainer;

	public bool IsClosed = true;

	public float VerticalSize = 5f;

	[Header("Debug")]
	public Color ZoneColor = Color.white;

	protected Vector3[] points;

	protected virtual void Awake()
	{
		points = GetPoints();
	}

	private void OnDrawGizmos()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		if (PointContainer.childCount >= 2)
		{
			Vector3[] array = GetPoints();
			for (int i = 0; i < array.Length - 1; i++)
			{
				Vector3 val = array[i];
				Vector3 val2 = array[i + 1];
				Debug.DrawLine(val, val2, ZoneColor);
				Debug.DrawLine(val + Vector3.up * VerticalSize, val2 + Vector3.up * VerticalSize, ZoneColor);
				Debug.DrawLine(val, val + Vector3.up * VerticalSize, ZoneColor);
				Debug.DrawLine(val2, val2 + Vector3.up * VerticalSize, ZoneColor);
				Gizmos.color = ZoneColor;
				Gizmos.DrawSphere(val, 0.5f);
			}
			if (IsClosed)
			{
				Debug.DrawLine(array[array.Length - 1], array[0], ZoneColor);
				Debug.DrawLine(array[array.Length - 1] + Vector3.up * VerticalSize, array[0] + Vector3.up * VerticalSize, ZoneColor);
			}
		}
	}

	public bool IsPointInsidePolygon(Vector3 point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (!DoBoundsContainPoint(point))
		{
			return false;
		}
		Vector2[] array = (Vector2[])(object)new Vector2[points.Length];
		for (int i = 0; i < points.Length; i++)
		{
			array[i] = new Vector2(points[i].x, points[i].z);
		}
		return CalculateWindingNumber(array, new Vector2(point.x, point.z)) != 0;
	}

	public bool IsPointInsideZone(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (point.y < ((Component)this).transform.position.y || point.y > ((Component)this).transform.position.y + VerticalSize)
		{
			return false;
		}
		return IsPointInsidePolygon(point);
	}

	public float GetDistanceToClosestPointOnZone(Vector3 source)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (IsClosed && IsPointInsidePolygon(source))
		{
			return 0f;
		}
		Vector3 closestPointOnPolygon = GetClosestPointOnPolygon(points, source);
		closestPointOnPolygon.y = source.y;
		float num = Vector3.Distance(closestPointOnPolygon, source);
		float num2 = source.y - (((Component)this).transform.position.y + VerticalSize / 2f);
		float num3 = Mathf.Max(0f, num2 - VerticalSize);
		return Mathf.Sqrt(Mathf.Pow(num, 2f) + Mathf.Pow(num3, 2f));
	}

	protected Vector3[] GetPoints()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PointContainer == (Object)null)
		{
			return (Vector3[])(object)new Vector3[0];
		}
		Vector3[] array = (Vector3[])(object)new Vector3[PointContainer.childCount];
		for (int i = 0; i < PointContainer.childCount; i++)
		{
			array[i] = PointContainer.GetChild(i).position;
		}
		return array;
	}

	protected bool DoBoundsContainPoint(Vector3 point)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Tuple<Vector3, Vector3> boundingPoints = GetBoundingPoints();
		if (point.x >= boundingPoints.Item1.x && point.x <= boundingPoints.Item2.x && point.z >= boundingPoints.Item1.z)
		{
			return point.z <= boundingPoints.Item2.z;
		}
		return false;
	}

	protected Tuple<Vector3, Vector3> GetBoundingPoints()
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] source = GetPoints();
		float num = source.Select((Vector3 p) => p.x).Max();
		float num2 = source.Select((Vector3 p) => p.x).Min();
		float num3 = source.Select((Vector3 p) => p.z).Max();
		float num4 = source.Select((Vector3 p) => p.z).Min();
		return new Tuple<Vector3, Vector3>(new Vector3(num2, 0f, num4), new Vector3(num, VerticalSize, num3));
	}

	protected int CalculateWindingNumber(Vector2[] polygon, Vector2 point)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < polygon.Length; i++)
		{
			Vector2 val = polygon[i];
			Vector2 val2 = polygon[(i + 1) % polygon.Length];
			if (IsPointOnSegment(val, val2, point))
			{
				return 0;
			}
			if (val.y <= point.y)
			{
				if (val2.y > point.y && IsLeft(val, val2, point) > 0)
				{
					num++;
				}
			}
			else if (val2.y <= point.y && IsLeft(val, val2, point) < 0)
			{
				num--;
			}
		}
		return num;
		static float CrossProduct(Vector2 start, Vector2 end, Vector2 val3)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			return (val3.x - start.x) * (end.y - start.y) - (val3.y - start.y) * (end.x - start.x);
		}
		static float DotProduct(Vector2 start, Vector2 end, Vector2 val3)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			return (val3.x - start.x) * (end.x - start.x) + (val3.y - start.y) * (end.y - start.y);
		}
		static int IsLeft(Vector2 start, Vector2 end, Vector2 point2)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			float num2 = CrossProduct(start, end, point2);
			if (Mathf.Abs(num2) < 0.001f)
			{
				return 0;
			}
			if (num2 > 0f)
			{
				return 1;
			}
			return -1;
		}
		static bool IsPointOnSegment(Vector2 start, Vector2 end, Vector2 point2)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (Mathf.Abs(CrossProduct(start, end, point2)) > 0.001f)
			{
				return false;
			}
			float num2 = DotProduct(start, end, point2);
			if (num2 < 0f)
			{
				return false;
			}
			Vector2 val3 = end - start;
			float sqrMagnitude = ((Vector2)(ref val3)).sqrMagnitude;
			if (num2 > sqrMagnitude)
			{
				return false;
			}
			return true;
		}
	}

	protected Vector3 GetClosestPointOnPolygon(Vector3[] polyPoints, Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result = Vector3.zero;
		float num = float.PositiveInfinity;
		for (int i = 0; i < polyPoints.Length - 1; i++)
		{
			Vector3 lineStart = polyPoints[i];
			Vector3 lineEnd = polyPoints[i + 1];
			Vector3 val = ProjectPointOnLineSegment(lineStart, lineEnd, point);
			float num2 = Vector3.Distance(point, val);
			if (num2 < num)
			{
				num = num2;
				result = val;
			}
		}
		if (IsClosed)
		{
			Vector3 lineStart2 = polyPoints[polyPoints.Length - 1];
			Vector3 lineEnd2 = polyPoints[0];
			Vector3 val2 = ProjectPointOnLineSegment(lineStart2, lineEnd2, point);
			float num3 = Vector3.Distance(point, val2);
			if (num3 < num)
			{
				num = num3;
				result = val2;
			}
		}
		return result;
		static Vector3 ProjectPointOnLineSegment(Vector3 val5, Vector3 val4, Vector3 val6)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val3 = val4 - val5;
			float magnitude = ((Vector3)(ref val3)).magnitude;
			((Vector3)(ref val3)).Normalize();
			float num4 = Vector3.Dot(val6 - val5, val3);
			num4 = Mathf.Clamp(num4, 0f, magnitude);
			return val5 + val3 * num4;
		}
	}
}
