using Pathfinding;
using UnityEngine;

namespace ScheduleOne.Vehicles.AI;

[RequireComponent(typeof(LandVehicle))]
public class VehicleTeleporter : MonoBehaviour
{
	public void MoveToGraph(bool resetRotation = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		NNConstraint val = new NNConstraint();
		val.graphMask = GraphMask.FromGraphName("General Vehicle Graph");
		NNInfo nearest = AstarPath.active.GetNearest(((Component)this).transform.position, val);
		((Component)this).transform.position = nearest.position + ((Component)this).transform.up * ((Component)this).GetComponent<LandVehicle>().BoundingBoxDimensions.y / 2f;
		if (resetRotation)
		{
			((Component)this).transform.rotation = Quaternion.identity;
		}
	}

	public void MoveToRoadNetwork(bool resetRotation = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		NNConstraint val = new NNConstraint();
		val.graphMask = GraphMask.FromGraphName("Road Nodes");
		NNInfo nearest = AstarPath.active.GetNearest(((Component)this).transform.position, val);
		((Component)this).transform.position = nearest.position + ((Component)this).transform.up * ((Component)this).GetComponent<LandVehicle>().BoundingBoxDimensions.y / 2f;
		if (resetRotation)
		{
			((Component)this).transform.rotation = Quaternion.identity;
		}
	}
}
