using Pathfinding;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

public class AstarUtility : MonoBehaviour
{
	public static Vector3 GetClosestPointOnGraph(Vector3 point, string GraphName)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		NNConstraint val = new NNConstraint();
		val.graphMask = GraphMask.FromGraphName(GraphName);
		return AstarPath.active.GetNearest(point, val).position;
	}
}
