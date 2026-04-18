using System.Collections.Generic;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vehicles;

[RequireComponent(typeof(Rigidbody))]
public class ObstructionDetector : MonoBehaviour
{
	private LandVehicle vehicle;

	public List<LandVehicle> vehicles = new List<LandVehicle>();

	public List<NPC> npcs = new List<NPC>();

	public List<PlayerMovement> players = new List<PlayerMovement>();

	public List<VehicleObstacle> vehicleObstacles = new List<VehicleObstacle>();

	public float closestObstructionDistance;

	public float range;

	protected virtual void Awake()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		vehicle = ((Component)this).gameObject.GetComponentInParent<LandVehicle>();
		range = ((Component)((Component)this).transform.Find("Collider")).transform.localScale.z;
	}

	protected virtual void FixedUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		closestObstructionDistance = float.MaxValue;
		for (int i = 0; i < vehicles.Count; i++)
		{
			if (Vector3.Distance(((Component)this).transform.position, ((Component)vehicles[i]).transform.position) < closestObstructionDistance)
			{
				closestObstructionDistance = Vector3.Distance(((Component)this).transform.position, ((Component)vehicles[i]).transform.position);
			}
		}
		for (int j = 0; j < npcs.Count; j++)
		{
			if (Vector3.Distance(((Component)this).transform.position, ((Component)npcs[j]).transform.position) < closestObstructionDistance)
			{
				closestObstructionDistance = Vector3.Distance(((Component)this).transform.position, ((Component)npcs[j]).transform.position);
			}
		}
		for (int k = 0; k < players.Count; k++)
		{
			if (Vector3.Distance(((Component)this).transform.position, ((Component)players[k]).transform.position) < closestObstructionDistance)
			{
				closestObstructionDistance = Vector3.Distance(((Component)this).transform.position, ((Component)players[k]).transform.position);
			}
		}
		for (int l = 0; l < vehicleObstacles.Count; l++)
		{
			if (Vector3.Distance(((Component)this).transform.position, ((Component)vehicleObstacles[l]).transform.position) < closestObstructionDistance)
			{
				closestObstructionDistance = Vector3.Distance(((Component)this).transform.position, ((Component)vehicleObstacles[l]).transform.position);
			}
		}
		vehicles.Clear();
		npcs.Clear();
		players.Clear();
		vehicleObstacles.Clear();
		_ = closestObstructionDistance;
		_ = float.MaxValue;
	}

	private void OnTriggerStay(Collider other)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		LandVehicle componentInParent = ((Component)other).GetComponentInParent<LandVehicle>();
		NPC componentInParent2 = ((Component)other).GetComponentInParent<NPC>();
		PlayerMovement componentInParent3 = ((Component)other).GetComponentInParent<PlayerMovement>();
		VehicleObstacle componentInParent4 = ((Component)other).GetComponentInParent<VehicleObstacle>();
		if ((Object)(object)componentInParent != (Object)null && (Object)(object)componentInParent != (Object)(object)vehicle && !vehicles.Contains(componentInParent))
		{
			vehicles.Add(componentInParent);
		}
		if ((Object)(object)componentInParent2 != (Object)null && !npcs.Contains(componentInParent2))
		{
			npcs.Add(componentInParent2);
		}
		if ((Object)(object)componentInParent3 != (Object)null && !players.Contains(componentInParent3))
		{
			players.Add(componentInParent3);
		}
		if ((Object)(object)componentInParent4 != (Object)null && (componentInParent4.twoSided || Vector3.Angle(-((Component)componentInParent4).transform.forward, ((Component)this).transform.forward) < 90f) && !vehicleObstacles.Contains(componentInParent4))
		{
			vehicleObstacles.Add(componentInParent4);
		}
	}
}
