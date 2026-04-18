using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Vehicles;

[RequireComponent(typeof(BoxCollider))]
public class SpeedZone : MonoBehaviour
{
	public static List<SpeedZone> speedZones = new List<SpeedZone>();

	public BoxCollider col;

	public float speed = 20f;

	private static List<SpeedZone> query = new List<SpeedZone>();

	public virtual void Awake()
	{
		speedZones.Add(this);
	}

	public static IEnumerable<SpeedZone> GetSpeedZones(Vector3 point)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		query.Clear();
		for (int i = 0; i < speedZones.Count; i++)
		{
			Bounds bounds = ((Collider)speedZones[i].col).bounds;
			if (((Bounds)(ref bounds)).Contains(point))
			{
				query.Add(speedZones[i]);
			}
		}
		return query;
	}

	private void OnDrawGizmos()
	{
	}
}
