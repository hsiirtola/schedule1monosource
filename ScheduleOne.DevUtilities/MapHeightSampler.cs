using ScheduleOne.GamePhysics;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

public static class MapHeightSampler
{
	private const float SampleHeight = 100f;

	private const float SampleDistance = 200f;

	public static bool TrySample(float x, float z, out Vector3 hitPoint)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		hitPoint = new Vector3(x, 0f, z);
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(new Vector3(x, 100f, z), Vector3.down, ref val, 200f, LayerMask.op_Implicit(NetworkSingleton<PhysicsManager>.Instance.GroundDetectionLayerMask), (QueryTriggerInteraction)1))
		{
			hitPoint.y = ((RaycastHit)(ref val)).point.y;
			return true;
		}
		return false;
	}
}
