using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vehicles.AI;

public class Sensor : MonoBehaviour
{
	public bool Enabled;

	public Collider obstruction;

	public float obstructionDistance;

	public const float checkRate = 0.33f;

	[Header("Settings")]
	[SerializeField]
	protected float minDetectionRange = 3f;

	[SerializeField]
	protected float maxDetectionRange = 12f;

	public float checkRadius = 1f;

	public LayerMask checkMask;

	private LandVehicle vehicle;

	[HideInInspector]
	public float calculatedDetectionRange;

	private RaycastHit hit;

	private List<RaycastHit> hits = new List<RaycastHit>();

	protected virtual void Start()
	{
		vehicle = ((Component)this).GetComponentInParent<LandVehicle>();
		((MonoBehaviour)this).InvokeRepeating("Check", 0f, 0.33f);
	}

	public void Check()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		if (!Enabled || NetworkSingleton<TimeManager>.Instance.IsSleepInProgress || vehicle.Agent.KinematicMode)
		{
			return;
		}
		if ((Object)(object)vehicle != (Object)null)
		{
			calculatedDetectionRange = Mathf.Lerp(minDetectionRange, maxDetectionRange, Mathf.Clamp01(vehicle.Speed_Kmh / vehicle.TopSpeed));
		}
		else
		{
			calculatedDetectionRange = maxDetectionRange;
		}
		Vector3 val = ((Component)this).transform.position - ((Component)this).transform.forward * checkRadius;
		hits = Physics.SphereCastAll(val, checkRadius, ((Component)this).transform.forward, calculatedDetectionRange, LayerMask.op_Implicit(checkMask), (QueryTriggerInteraction)2).ToList();
		hits = hits.OrderBy((RaycastHit x) => Vector3.Distance(((Component)this).transform.position, ((RaycastHit)(ref x)).point)).ToList();
		bool flag = false;
		for (int num = 0; num < hits.Count; num++)
		{
			RaycastHit val2;
			if ((Object)(object)vehicle != (Object)null)
			{
				val2 = hits[num];
				if (((Component)((RaycastHit)(ref val2)).collider).transform.IsChildOf(((Component)vehicle).transform))
				{
					continue;
				}
			}
			val2 = hits[num];
			VehicleObstacle componentInParent = ((Component)((Component)((RaycastHit)(ref val2)).collider).transform).GetComponentInParent<VehicleObstacle>();
			val2 = hits[num];
			LandVehicle componentInParent2 = ((Component)((Component)((RaycastHit)(ref val2)).collider).transform).GetComponentInParent<LandVehicle>();
			val2 = hits[num];
			NPC componentInParent3 = ((Component)((Component)((RaycastHit)(ref val2)).collider).transform).GetComponentInParent<NPC>();
			val2 = hits[num];
			Player componentInParent4 = ((Component)((Component)((RaycastHit)(ref val2)).collider).transform).GetComponentInParent<Player>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				if (!componentInParent.twoSided && Vector3.Angle(-((Component)componentInParent).transform.forward, ((Component)this).transform.forward) > 90f)
				{
					continue;
				}
			}
			else if (!((Object)(object)componentInParent2 != (Object)null) && !((Object)(object)componentInParent3 != (Object)null))
			{
				_ = (Object)(object)componentInParent4 != (Object)null;
			}
			flag = true;
			hit = hits[num];
			break;
		}
		if (flag)
		{
			obstruction = ((RaycastHit)(ref hit)).collider;
			obstructionDistance = Vector3.Distance(((Component)this).transform.position, ((RaycastHit)(ref hit)).point);
		}
		else
		{
			obstruction = null;
			obstructionDistance = float.MaxValue;
		}
	}
}
