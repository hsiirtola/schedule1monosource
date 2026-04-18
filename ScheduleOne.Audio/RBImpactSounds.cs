using System;
using ScheduleOne.Combat;
using ScheduleOne.Core.Audio;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Audio;

[RequireComponent(typeof(Rigidbody))]
public class RBImpactSounds : MonoBehaviour
{
	public const float MinImpactMomentum = 4f;

	public const float SoundCooldown = 0.25f;

	[SerializeField]
	[FormerlySerializedAs("Material")]
	private EImpactSound _material;

	private float _lastImpactTime;

	private Rigidbody _rb;

	private void Awake()
	{
		PhysicsDamageable component = ((Component)this).GetComponent<PhysicsDamageable>();
		if ((Object)(object)component != (Object)null)
		{
			component.onImpacted = (Action<Impact>)Delegate.Combine(component.onImpacted, new Action<Impact>(OnImpacted));
		}
		_rb = ((Component)this).GetComponent<Rigidbody>();
	}

	private void OnImpacted(Impact impact)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (Singleton<SFXManager>.InstanceExists && !(Time.time - _lastImpactTime < 0.25f) && !(impact.ImpactForce < 4f))
		{
			Singleton<SFXManager>.Instance.PlayImpactSound(_material, impact.HitPoint, impact.ImpactForce);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (Singleton<SFXManager>.InstanceExists && !(Time.time - _lastImpactTime < 0.25f))
		{
			Rigidbody rigidbody = collision.rigidbody;
			float num = _rb.mass;
			if ((Object)(object)rigidbody != (Object)null)
			{
				num = Mathf.Min(num, rigidbody.mass);
			}
			Vector3 relativeVelocity = collision.relativeVelocity;
			float num2 = ((Vector3)(ref relativeVelocity)).magnitude * num;
			if (!(num2 < 4f))
			{
				_lastImpactTime = Time.time;
				Singleton<SFXManager>.Instance.PlayImpactSound(_material, ((ContactPoint)(ref collision.contacts[0])).point, num2);
			}
		}
	}
}
