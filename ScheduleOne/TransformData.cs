using UnityEngine;

namespace ScheduleOne;

public struct TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
{
	public Vector3 Position = position;

	public Quaternion Rotation = rotation;

	public Vector3 Scale = scale;

	public void ApplyToWorldTransform(Transform transform)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		transform.position = Position;
		transform.rotation = Rotation;
	}

	public void ApplyToLocalTransform(Transform transform, bool setScale = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		transform.localPosition = Position;
		transform.localRotation = Rotation;
		if (setScale)
		{
			transform.localScale = Scale;
		}
	}

	public static TransformData FromTransform(Transform transform)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		return new TransformData
		{
			Position = transform.position,
			Rotation = transform.rotation,
			Scale = transform.localScale
		};
	}

	public static TransformData Lerp(TransformData a, TransformData b, float t)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		return new TransformData
		{
			Position = Vector3.Lerp(a.Position, b.Position, t),
			Rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t),
			Scale = Vector3.Lerp(a.Scale, b.Scale, t)
		};
	}
}
