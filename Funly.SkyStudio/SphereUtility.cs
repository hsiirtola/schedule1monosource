using System;
using UnityEngine;

namespace Funly.SkyStudio;

public abstract class SphereUtility
{
	private const float k_HalfPI = (float)Math.PI / 2f;

	public static Vector2 DirectionToSphericalCoordinate(Vector3 direction)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Vector3 normalized = ((Vector3)(ref direction)).normalized;
		float num = Atan2Positive(normalized.z, normalized.x);
		float num2 = 0f;
		float num3 = Vector3.Angle(direction, Vector3.up) * ((float)Math.PI / 180f);
		num2 = ((!(num3 <= (float)Math.PI / 2f)) ? (-1f * (num3 - (float)Math.PI / 2f)) : ((float)Math.PI / 2f - num3));
		return new Vector2(num, num2);
	}

	public static Vector3 SphericalCoordinateToDirection(Vector2 coord)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Cos(coord.y);
		float num2 = Mathf.Sin(coord.y);
		float num3 = num;
		float num4 = 0f;
		num = num3 * Mathf.Cos(coord.x);
		num4 = num3 * Mathf.Sin(coord.x);
		return new Vector3(num, num2, num4);
	}

	public static float RadiusAtHeight(float yPos)
	{
		return Mathf.Abs(Mathf.Cos(Mathf.Asin(yPos)));
	}

	public static Vector3 SphericalToPoint(float yPosition, float radAngle)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		float num = RadiusAtHeight(yPosition);
		return new Vector3(num * Mathf.Cos(radAngle), yPosition, num * Mathf.Sin(radAngle));
	}

	public static float RadAngleToPercent(float radAngle)
	{
		return radAngle / ((float)Math.PI * 2f);
	}

	public static float PercentToRadAngle(float percent)
	{
		return percent * ((float)Math.PI * 2f);
	}

	public static float HeightToPercent(float yValue)
	{
		return yValue / 2f + 0.5f;
	}

	public static float PercentToHeight(float hPercent)
	{
		return Mathf.Lerp(-1f, 1f, hPercent);
	}

	public static float AngleToReachTarget(Vector2 point, float targetAngle)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		float num = Atan2Positive(point.y, point.x);
		return (float)Math.PI * 2f - num + targetAngle;
	}

	public static float Atan2Positive(float y, float x)
	{
		float num = Mathf.Atan2(y, x);
		if (num < 0f)
		{
			num = (float)Math.PI + ((float)Math.PI + num);
		}
		return num;
	}

	public static Vector3 RotateAroundXAxis(Vector3 point, float angle)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Rotate2d(new Vector2(point.z, point.y), angle);
		return new Vector3(point.x, val.y, val.x);
	}

	public static Vector3 RotateAroundYAxis(Vector3 point, float angle)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Rotate2d(new Vector2(point.x, point.z), angle);
		return new Vector3(val.x, point.y, val.y);
	}

	public static Vector3 RotatePoint(Vector3 point, float xAxisRotation, float yAxisRotation)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return RotateAroundXAxis(RotateAroundYAxis(point, yAxisRotation), xAxisRotation);
	}

	public static Vector2 Rotate2d(Vector2 pos, float angle)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return Matrix2x2Mult(new Vector4(Mathf.Cos(angle), 0f - Mathf.Sin(angle), Mathf.Sin(angle), Mathf.Cos(angle)), pos);
	}

	public static Vector2 Matrix2x2Mult(Vector4 matrix, Vector2 pos)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(((Vector4)(ref matrix))[0] * ((Vector2)(ref pos))[0] + ((Vector4)(ref matrix))[1] * ((Vector2)(ref pos))[1], ((Vector4)(ref matrix))[2] * ((Vector2)(ref pos))[0] + ((Vector4)(ref matrix))[3] * ((Vector2)(ref pos))[1]);
	}

	public static void CalculateStarRotation(Vector3 star, out float xRotationAngle, out float yRotationAngle)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(star.x, star.y, star.z);
		yRotationAngle = AngleToReachTarget(new Vector2(val.x, val.z), (float)Math.PI / 2f);
		val = RotateAroundYAxis(val, yRotationAngle);
		xRotationAngle = AngleToReachTarget(Vector2.op_Implicit(new Vector3(val.z, val.y)), 0f);
	}

	public static Vector2 ConvertUVToSphericalCoordinate(Vector2 uv)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(Mathf.Lerp(0f, (float)Math.PI * 2f, uv.x), Mathf.Lerp(-(float)Math.PI / 2f, (float)Math.PI / 2f, uv.y));
	}

	public static Vector2 ConvertSphericalCoordateToUV(Vector2 sphereCoord)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(sphereCoord.x / ((float)Math.PI * 2f), (sphereCoord.y + (float)Math.PI / 2f) / (float)Math.PI);
	}
}
