using ScheduleOne.Audio;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework.Equipping;

public class AvatarWeapon : AvatarEquippable
{
	[Header("Range settings")]
	public float MinUseRange;

	public float MaxUseRange = 1f;

	[Header("Cooldown settings")]
	public float CooldownDuration = 1f;

	[Header("Equipping")]
	public AudioClip[] EquipClips;

	public AudioSourceController EquipSound;

	public float EquipDuration = 0.25f;

	public UnityEvent onSuccessfulHit;

	private float _timeOnEquip;

	public float LastUseTime { get; private set; }

	public override void Equip(Avatar _avatar)
	{
		base.Equip(_avatar);
		if (EquipClips.Length != 0 && (Object)(object)EquipSound != (Object)null)
		{
			EquipSound.SetClip(EquipClips[Random.Range(0, EquipClips.Length)]);
			EquipSound.Play();
		}
		_timeOnEquip = Time.time;
	}

	public virtual void Attack()
	{
		LastUseTime = Time.time;
	}

	public virtual bool IsReadyToAttack()
	{
		if (Time.time - LastUseTime > CooldownDuration)
		{
			return Time.time - _timeOnEquip > EquipDuration;
		}
		return false;
	}
}
