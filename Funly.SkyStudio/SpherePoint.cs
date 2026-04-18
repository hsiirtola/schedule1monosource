using System;
using UnityEngine;

namespace Funly.SkyStudio;

[Serializable]
public class SpherePoint
{
	public float horizontalRotation;

	public float verticalRotation;

	public const float MinHorizontalRotation = -(float)Math.PI;

	public const float MaxHorizontalRotation = (float)Math.PI;

	public const float MinVerticalRotation = -(float)Math.PI / 2f;

	public const float MaxVerticalRotation = (float)Math.PI / 2f;

	public SpherePoint(float horizontalRotation, float verticalRotation)
	{
		this.horizontalRotation = horizontalRotation;
		this.verticalRotation = verticalRotation;
	}

	public SpherePoint(Vector3 worldDirection)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = SphereUtility.DirectionToSphericalCoordinate(worldDirection);
		horizontalRotation = val.x;
		verticalRotation = val.y;
	}

	public void SetFromWorldDirection(Vector3 worldDirection)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = SphereUtility.DirectionToSphericalCoordinate(worldDirection);
		horizontalRotation = val.x;
		verticalRotation = val.y;
	}

	public Vector3 GetWorldDirection()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return SphereUtility.SphericalCoordinateToDirection(new Vector2(horizontalRotation, verticalRotation));
	}
}
