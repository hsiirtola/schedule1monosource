using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Combat;

public class PhysicsDamageable : MonoBehaviour, IDamageable
{
	public const int VELOCITY_HISTORY_LENGTH = 4;

	public Rigidbody Rb;

	public float ForceMultiplier = 1f;

	private List<int> impactHistory = new List<int>();

	public Action<Impact> onImpacted;

	private List<Vector3> velocityHistory = new List<Vector3>();

	public Vector3 averageVelocity { get; private set; } = Vector3.zero;

	public void OnValidate()
	{
		if ((Object)(object)Rb == (Object)null)
		{
			Rb = ((Component)this).GetComponent<Rigidbody>();
		}
	}

	public virtual void SendImpact(Impact impact)
	{
		ReceiveImpact(impact);
	}

	public virtual void ReceiveImpact(Impact impact)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (!impactHistory.Contains(impact.ImpactID))
		{
			impactHistory.Add(impact.ImpactID);
			if (onImpacted != null)
			{
				onImpacted(impact);
			}
			if ((Object)(object)Rb != (Object)null)
			{
				Rb.AddForceAtPosition(impact.ImpactForceDirection * impact.ImpactForce * ForceMultiplier, impact.HitPoint, (ForceMode)1);
			}
		}
	}
}
