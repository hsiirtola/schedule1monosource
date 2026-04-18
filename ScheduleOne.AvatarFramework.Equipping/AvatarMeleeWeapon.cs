using System;
using System.Collections;
using FishNet.Object;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.Vehicles;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Equipping;

public class AvatarMeleeWeapon : AvatarWeapon
{
	[Serializable]
	public class MeleeAttack
	{
		public float RangeMultiplier = 1f;

		public float DamageMultiplier = 1f;

		public string AnimationTrigger = string.Empty;

		public float DamageDelay = 0.4f;

		public float AttackSoundDelay;

		public AudioClip[] AttackClips;

		public AudioClip[] HitClips;
	}

	[Header("References")]
	public AudioSourceController AttackSound;

	public AudioSourceController HitSound;

	[Header("Melee Weapon settings")]
	public EImpactType ImpactType;

	public float AttackRange = 1.5f;

	public float AttackRadius = 0.25f;

	public float Damage = 25f;

	public float ImpactForce = 10f;

	public MeleeAttack[] Attacks;

	public float GruntChance = 0.4f;

	private Coroutine attackRoutine;

	public override void Unequip()
	{
		if (attackRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(attackRoutine);
			attackRoutine = null;
		}
		base.Unequip();
	}

	public override bool IsReadyToAttack()
	{
		if (Attacks.Length == 0)
		{
			return false;
		}
		return base.IsReadyToAttack();
	}

	public override void Attack()
	{
		base.Attack();
		MeleeAttack attack = Attacks[Random.Range(0, Attacks.Length)];
		NPC npc = ((Component)avatar).GetComponentInParent<NPC>();
		avatar.Animation.ResetTrigger(attack.AnimationTrigger);
		avatar.Animation.SetTrigger(attack.AnimationTrigger);
		attackRoutine = ((MonoBehaviour)this).StartCoroutine(AttackRoutine());
		IEnumerator AttackRoutine()
		{
			yield return (object)new WaitForSeconds(attack.AttackSoundDelay);
			if (attack.AttackClips.Length != 0)
			{
				AttackSound.SetClip(attack.AttackClips[Random.Range(0, attack.AttackClips.Length)]);
				AttackSound.Play();
			}
			if (Random.value < GruntChance && (Object)(object)npc != (Object)null)
			{
				npc.PlayVO(EVOLineType.Grunt);
			}
			yield return (object)new WaitForSeconds(attack.DamageDelay - attack.AttackSoundDelay);
			Vector3 centerPoint = avatar.CenterPoint;
			Vector3 forward = ((Component)avatar).transform.forward;
			RaycastHit[] array = Physics.RaycastAll(centerPoint, forward, AttackRange, LayerMask.op_Implicit(NetworkSingleton<CombatManager>.Instance.MeleeLayerMask));
			IDamageable componentInParent = ((Component)this).GetComponentInParent<IDamageable>();
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit val = array2[i];
				IDamageable componentInParent2 = ((Component)((RaycastHit)(ref val)).collider).GetComponentInParent<IDamageable>();
				if (componentInParent2 != null && componentInParent2 != componentInParent && (componentInParent2 == null || !((Object)(object)componentInParent2.gameObject.GetComponent<LandVehicle>() != (Object)null)))
				{
					Vector3 point = ((RaycastHit)(ref val)).point;
					Vector3 val2 = ((RaycastHit)(ref val)).point - centerPoint;
					Impact impact = new Impact(point, ((Vector3)(ref val2)).normalized, ImpactForce * attack.DamageMultiplier, Damage * attack.DamageMultiplier, ImpactType, ((Component)this).GetComponentInParent<NetworkObject>());
					componentInParent2.SendImpact(impact);
					if (attack.HitClips.Length != 0)
					{
						HitSound.SetClip(attack.HitClips[Random.Range(0, attack.HitClips.Length)]);
						((Component)HitSound).transform.position = ((RaycastHit)(ref val)).point;
						HitSound.DuplicateAndPlayOneShot();
					}
					break;
				}
			}
		}
	}
}
