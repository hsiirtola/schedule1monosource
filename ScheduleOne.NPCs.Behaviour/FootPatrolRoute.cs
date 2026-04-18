using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class FootPatrolRoute : MonoBehaviour
{
	[Header("Settings")]
	public string RouteName = "Foot patrol route";

	public Color PathColor = Color.red;

	public Transform[] Waypoints;

	public int StartWaypointIndex;

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(((Component)this).transform.position + Vector3.up * 0.5f, 0.5f);
		Gizmos.color = Color.yellow;
		for (int i = 0; i < Waypoints.Length; i++)
		{
			if (!((Object)(object)Waypoints[i] == (Object)null))
			{
				Gizmos.DrawWireSphere(Waypoints[i].position + Vector3.up * 0.5f, 0.5f);
			}
		}
		Gizmos.color = PathColor;
		for (int j = 0; j < Waypoints.Length - 1; j++)
		{
			if (!((Object)(object)Waypoints[j] == (Object)null))
			{
				Gizmos.DrawLine(Waypoints[j].position + Vector3.up * 0.5f, Waypoints[j + 1].position + Vector3.up * 0.5f);
			}
		}
	}

	private void OnValidate()
	{
		UpdateWaypoints();
	}

	private void UpdateWaypoints()
	{
		Waypoints = ((Component)((Component)this).transform).GetComponentsInChildren<Transform>();
		Waypoints = ArrayExt.Remove<Transform>(Waypoints, ((Component)this).transform);
	}
}
