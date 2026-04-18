using ScheduleOne.DevUtilities;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Map;

public class Dealership : MonoBehaviour
{
	public Transform[] SpawnPoints;

	public void SpawnVehicle(string vehicleCode)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Transform val = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
		NetworkSingleton<VehicleManager>.Instance.SpawnVehicle(vehicleCode, val.position, val.rotation, playerOwned: true);
	}
}
