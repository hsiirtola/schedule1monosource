using UnityEngine;

namespace ScheduleOne.DevUtilities;

public static class TransformUtilities
{
	public static string GetScenePath(this Transform transform)
	{
		string text = ((Object)transform).name;
		while ((Object)(object)transform.parent != (Object)null)
		{
			transform = transform.parent;
			text = ((Object)transform).name + "/" + text;
		}
		return text;
	}

	public static Vector2 XZ(this Vector3 vector)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(vector.x, vector.z);
	}
}
