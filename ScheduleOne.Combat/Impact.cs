using System;
using FishNet.Object;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Combat;

[Serializable]
public class Impact
{
	public Vector3 HitPoint;

	public Vector3 ImpactForceDirection;

	public float ImpactForce;

	public float ImpactDamage;

	public EImpactType ImpactType;

	public NetworkObject ImpactSource;

	public int ImpactID;

	public EExplosionType ExplosionType;

	public Impact(Vector3 hitPoint, Vector3 impactForceDirection, float impactForce, float impactDamage, EImpactType impactType, NetworkObject impactSource, int impactID)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		HitPoint = hitPoint;
		ImpactForceDirection = impactForceDirection;
		ImpactForce = impactForce;
		ImpactDamage = impactDamage;
		ImpactType = impactType;
		if ((Object)(object)impactSource != (Object)null)
		{
			ImpactSource = impactSource;
		}
		if (impactID == 0)
		{
			impactID = Random.Range(int.MinValue, int.MaxValue);
		}
		ImpactID = impactID;
	}

	public Impact(Vector3 hitPoint, Vector3 impactForceDirection, float impactForce, float impactDamage, EImpactType impactType, NetworkObject impactSource)
		: this(hitPoint, impactForceDirection, impactForce, impactDamage, impactType, impactSource, 0)
	{
	}//IL_0001: Unknown result type (might be due to invalid IL or missing references)
	//IL_0002: Unknown result type (might be due to invalid IL or missing references)


	public Impact()
	{
	}

	public static bool IsLethal(EImpactType impactType)
	{
		if (impactType == EImpactType.SharpMetal || impactType == EImpactType.Bullet || impactType == EImpactType.Explosion)
		{
			return true;
		}
		return false;
	}

	public bool IsPlayerImpact(out Player player)
	{
		if ((Object)(object)ImpactSource == (Object)null)
		{
			player = null;
			return false;
		}
		player = ((Component)ImpactSource).GetComponent<Player>();
		return (Object)(object)player != (Object)null;
	}
}
