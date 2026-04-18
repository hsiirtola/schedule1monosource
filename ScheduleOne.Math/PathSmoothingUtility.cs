using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.Curvy;
using UnityEngine;

namespace ScheduleOne.Math;

public static class PathSmoothingUtility
{
	public class SmoothedPath
	{
		public const float MARGIN = 10f;

		public List<Vector3> vectorPath = new List<Vector3>();

		public List<Bounds> segmentBounds = new List<Bounds>();

		public void InitializePath()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			segmentBounds.Clear();
			for (int i = 0; i < vectorPath.Count - 1; i++)
			{
				Vector3 val = vectorPath[i];
				Vector3 val2 = vectorPath[i + 1];
				Vector3 val3 = Vector3.Min(val, val2);
				Vector3 val4 = Vector3.Max(val, val2);
				Bounds item = default(Bounds);
				((Bounds)(ref item)).SetMinMax(val3 - Vector3.one * 10f, val4 + Vector3.one * 10f);
				segmentBounds.Add(item);
			}
		}
	}

	private const float MinControlPointDistance = 0.5f;

	private static CurvySpline _spline;

	public static void EnsureSplineInitialized()
	{
		if ((Object)(object)_spline == (Object)null)
		{
			_spline = CurvySpline.Create();
			_spline.Interpolation = (CurvyInterpolation)4;
			_spline.BSplineDegree = 5;
			_spline.Orientation = (CurvyOrientation)0;
			_spline.CacheDensity = 40;
		}
	}

	public static SmoothedPath CalculateSmoothedPath(List<Vector3> controlPoints, float maxCPDistance = 5f)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (controlPoints.Count < 2)
		{
			Debug.LogWarning((object)"Smoothing requires at least 2 control points.");
			return new SmoothedPath
			{
				vectorPath = controlPoints
			};
		}
		EnsureSplineInitialized();
		for (int i = 1; i < controlPoints.Count; i++)
		{
			if (Vector3.Distance(controlPoints[i], controlPoints[i - 1]) < 0.5f)
			{
				controlPoints.RemoveAt(i);
				i--;
			}
		}
		SmoothedPath smoothedPath = new SmoothedPath();
		controlPoints = InsertIntermediatePoints(controlPoints, maxCPDistance);
		_spline.Clear(false);
		_spline.Add(controlPoints.ToArray(), (Space)0);
		_spline.Refresh();
		List<Vector3> collection = _spline.GetApproximation((Space)1).ToList();
		smoothedPath.vectorPath.AddRange(collection);
		return smoothedPath;
	}

	private static void DrawPath(SmoothedPath path, Color col, float duration)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i < path.vectorPath.Count; i++)
		{
			Debug.DrawLine(path.vectorPath[i - 1], path.vectorPath[i], col, duration);
		}
	}

	private static List<Vector3> InsertIntermediatePoints(List<Vector3> points, float maxDistance)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < points.Count - 1; i++)
		{
			Vector3 val = points[i];
			Vector3 val2 = points[i + 1];
			float num = Vector3.Distance(val, val2);
			if (num > maxDistance)
			{
				int num2 = Mathf.FloorToInt(num / maxDistance);
				for (int j = 0; j < num2; j++)
				{
					Vector3 item = Vector3.Lerp(val, val2, (float)(j + 1) * (1f / (float)(num2 + 1)));
					points.Insert(i + (j + 1), item);
				}
			}
		}
		return points;
	}
}
