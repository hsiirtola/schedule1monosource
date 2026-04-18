using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Vehicles.AI;

[RequireComponent(typeof(BoxCollider))]
public class FunnelZone : MonoBehaviour
{
	public static List<FunnelZone> funnelZones = new List<FunnelZone>();

	public BoxCollider col;

	public Transform entryPoint;

	protected virtual void Awake()
	{
		funnelZones.Add(this);
	}

	public static FunnelZone GetFunnelZone(Vector3 point)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < funnelZones.Count; i++)
		{
			Bounds bounds = ((Collider)funnelZones[i].col).bounds;
			if (((Bounds)(ref bounds)).Contains(point))
			{
				return funnelZones[i];
			}
		}
		return null;
	}

	private void OnDrawGizmos()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.5f);
		Gizmos.DrawCube(((Component)this).transform.TransformPoint(col.center), new Vector3(col.size.x, col.size.y, col.size.z));
		Gizmos.DrawLine(((Component)this).transform.position, entryPoint.position);
	}
}
