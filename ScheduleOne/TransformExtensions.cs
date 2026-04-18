using UnityEngine;

namespace ScheduleOne;

public static class TransformExtensions
{
	public static TransformData GetWorldTransformData(this Transform transform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return new TransformData(transform.position, transform.rotation, transform.lossyScale);
	}

	public static TransformData GetLocalTransformData(this Transform transform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return new TransformData(transform.localPosition, transform.localRotation, transform.localScale);
	}

	public static void SetLocalTransformData(this Transform transform, TransformData data, bool setScale = true)
	{
		data.ApplyToLocalTransform(transform, setScale);
	}

	public static void SetWorldTransformData(this Transform transform, TransformData data)
	{
		data.ApplyToWorldTransform(transform);
	}
}
