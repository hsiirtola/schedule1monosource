using UnityEngine;

namespace VLB;

public static class TransformUtils
{
	public struct Packed
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 lossyScale;

		public bool IsSame(Transform transf)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			if (transf.position == position && transf.rotation == rotation)
			{
				return transf.lossyScale == lossyScale;
			}
			return false;
		}
	}

	public static Packed GetWorldPacked(this Transform self)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		return new Packed
		{
			position = self.position,
			rotation = self.rotation,
			lossyScale = self.lossyScale
		};
	}
}
