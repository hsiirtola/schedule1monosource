using System.Collections.Generic;
using FishNet;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Noise;
using UnityEngine;

namespace ScheduleOne.Combat;

public class Explosion : MonoBehaviour
{
	public AudioSourceController Sound;

	public unsafe void Initialize(Vector3 origin, ExplosionData data)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = origin;
		Sound.Play();
		float num = Mathf.Max(data.DamageRadius, data.PushForceRadius);
		NoiseUtility.EmitNoise(origin, ENoiseType.Explosion, num * 3f, ((Component)this).gameObject);
		List<IDamageable> list = new List<IDamageable>();
		if (InstanceFinder.IsServer)
		{
			Collider[] array = Physics.OverlapSphere(origin, num);
			foreach (Collider val in array)
			{
				IDamageable componentInParent = ((Component)val).GetComponentInParent<IDamageable>();
				if (componentInParent == null || list.Contains(componentInParent))
				{
					continue;
				}
				RaycastHit val2 = default(RaycastHit);
				if (data.CheckLoS)
				{
					if (Vector3.Distance(origin, ((Component)val).transform.position) < 1f)
					{
						((RaycastHit)(ref val2)).point = origin;
					}
					else
					{
						if (!Physics.Raycast(origin, ((Component)val).transform.position - origin, ref val2, num, LayerMask.op_Implicit(NetworkSingleton<CombatManager>.Instance.ExplosionLayerMask)))
						{
							Debug.DrawLine(origin, ((Component)val).transform.position, Color.green, 5f);
							continue;
						}
						Debug.DrawLine(origin, ((RaycastHit)(ref val2)).point, Color.red, 5f);
						if ((Object)(object)((RaycastHit)(ref val2)).collider != (Object)(object)val && ((Component)((RaycastHit)(ref val2)).collider).GetComponentInParent<IDamageable>() != componentInParent)
						{
							continue;
						}
					}
				}
				else
				{
					((RaycastHit)(ref val2)).point = ((Component)val).transform.position;
				}
				string obj = componentInParent?.ToString();
				Vector3 val3 = ((Component)val).transform.position;
				Console.Log("Explosion hit " + obj + " at " + ((object)(*(Vector3*)(&val3))/*cast due to .constrained prefix*/).ToString());
				list.Add(componentInParent);
				float num2 = Vector3.Distance(origin, ((Component)val).transform.position);
				float impactDamage = Mathf.Lerp(data.MaxDamage, 0f, Mathf.Clamp01(num2 / data.DamageRadius));
				float impactForce = Mathf.Lerp(data.MaxPushForce, 0f, Mathf.Clamp01(num2 / data.PushForceRadius));
				val3 = ((RaycastHit)(ref val2)).point - origin;
				Vector3 normalized = ((Vector3)(ref val3)).normalized;
				if (componentInParent is ICombatTargetable combatTargetable)
				{
					val3 = combatTargetable.CenterPoint - origin;
					normalized = ((Vector3)(ref val3)).normalized;
				}
				Impact impact = new Impact(((RaycastHit)(ref val2)).point, normalized, impactForce, impactDamage, EImpactType.Explosion, null, Random.Range(0, int.MaxValue));
				impact.ExplosionType = data.ExplosionType;
				componentInParent.ReceiveImpact(impact);
			}
		}
		Console.Log("Explosion hit " + list.Count + " damageables.");
	}
}
