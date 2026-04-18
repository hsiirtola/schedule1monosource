using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Vehicles;

public class VehicleRecoveryPoint : MonoBehaviour
{
	public static List<VehicleRecoveryPoint> recoveryPoints = new List<VehicleRecoveryPoint>();

	protected virtual void Awake()
	{
		recoveryPoints.Add(this);
	}

	public static VehicleRecoveryPoint GetClosestRecoveryPoint(Vector3 pos)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		VehicleRecoveryPoint vehicleRecoveryPoint = null;
		for (int i = 0; i < recoveryPoints.Count; i++)
		{
			if ((Object)(object)vehicleRecoveryPoint == (Object)null || Vector3.Distance(((Component)recoveryPoints[i]).transform.position, pos) < Vector3.Distance(((Component)vehicleRecoveryPoint).transform.position, pos))
			{
				vehicleRecoveryPoint = recoveryPoints[i];
			}
		}
		return vehicleRecoveryPoint;
	}
}
