using System.Collections;
using System.Linq;
using FishNet.Object;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using ScheduleOne.Vision;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Equipping;

public class AvatarRangedWeapon : AvatarWeapon
{
	[Header("Weapon Settings")]
	public int MagazineSize = -1;

	public float ReloadTime = 2f;

	public float MaxFireRate = 0.5f;

	public float EquipTime = 1f;

	public float RaiseTime = 1f;

	public float Damage = 35f;

	public float ImpactForce = 10f;

	public bool CanShootWhileMoving;

	public int MaxMovingShotsBeforeReposition = 3;

	public int MaxStationaryShotsBeforeReposition = 3;

	public bool RepositionAfterHit = true;

	[Header("Accuracy")]
	public float HitChance_MinRange = 0.6f;

	public float HitChance_MaxRange = 0.1f;

	[Header("Aiming")]
	public float AimTime_Min = 1f;

	public float AimTime_Max = 2.5f;

	[Header("References")]
	public Transform MuzzlePoint;

	public AudioSourceController FireSound;

	[Header("Animation Settings")]
	public string LoweredAnimationTrigger;

	public string RaisedAnimationTrigger;

	public string RecoilAnimationTrigger;

	private bool isReloading;

	private float timeEquipped;

	private float timeRaised;

	private float timeSinceLastShot = 1000f;

	private int currentAmmo;

	public bool IsRaised { get; protected set; }

	public override void Equip(Avatar _avatar)
	{
		base.Equip(_avatar);
		if (MagazineSize != -1)
		{
			currentAmmo = MagazineSize;
		}
	}

	public override void Unequip()
	{
		base.Unequip();
		if (IsRaised)
		{
			SetIsRaised(raised: false);
		}
	}

	public virtual void SetIsRaised(bool raised)
	{
		if (IsRaised != raised)
		{
			IsRaised = raised;
			timeRaised = 0f;
			if (IsRaised)
			{
				ResetTrigger(LoweredAnimationTrigger);
				SetTrigger(RaisedAnimationTrigger);
			}
			else
			{
				ResetTrigger(RaisedAnimationTrigger);
				SetTrigger(LoweredAnimationTrigger);
			}
		}
	}

	private void Update()
	{
		timeEquipped += Time.deltaTime;
		timeSinceLastShot += Time.deltaTime;
		if (IsRaised)
		{
			timeRaised += Time.deltaTime;
		}
	}

	public override void ReceiveMessage(string message, object data)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		base.ReceiveMessage(message, data);
		if (message == "Shoot")
		{
			Shoot((Vector3)data);
		}
		if (message == "Lower")
		{
			SetIsRaised(raised: false);
		}
		if (message == "Raise")
		{
			SetIsRaised(raised: true);
		}
	}

	public bool CanShoot()
	{
		if ((currentAmmo > 0 || MagazineSize == -1) && timeEquipped > EquipTime && !isReloading && timeSinceLastShot > MaxFireRate)
		{
			return timeRaised > RaiseTime;
		}
		return false;
	}

	protected virtual void Shoot(Vector3 endPoint)
	{
		base.Attack();
		if (timeSinceLastShot > 0f)
		{
			FireSound.DuplicateAndPlayOneShot();
			if (RecoilAnimationTrigger != string.Empty)
			{
				ResetTrigger(RecoilAnimationTrigger);
				SetTrigger(RecoilAnimationTrigger);
			}
		}
		timeSinceLastShot = 0f;
		Player componentInParent = ((Component)this).GetComponentInParent<Player>();
		if (!((Object)(object)componentInParent != (Object)null) || !((NetworkBehaviour)componentInParent).IsOwner)
		{
			currentAmmo--;
			if (currentAmmo <= 0 && MagazineSize != -1)
			{
				((MonoBehaviour)this).StartCoroutine(Reload());
			}
		}
	}

	public virtual void ApplyHitToDamageable(IDamageable damageable, Vector3 hitPoint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Impact impact = new Impact(hitPoint, damageable.gameObject.transform.position - MuzzlePoint.position, ImpactForce, Damage, EImpactType.Bullet, ((Component)this).GetComponentInParent<NetworkObject>());
		damageable.SendImpact(impact);
	}

	private IEnumerator Reload()
	{
		isReloading = true;
		yield return (object)new WaitForSeconds(ReloadTime);
		currentAmmo = MagazineSize;
		isReloading = false;
	}

	public bool IsTargetInLoS(ICombatTargetable target)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = MuzzlePoint.position;
		Vector3 val = target.CenterPoint - MuzzlePoint.position;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(position, ((Vector3)(ref val)).normalized, ref val2, Vector3.Distance(MuzzlePoint.position, target.CenterPoint), LayerMask.op_Implicit(NetworkSingleton<CombatManager>.Instance.RangedWeaponLayerMask)))
		{
			if (((Component)((RaycastHit)(ref val2)).collider).GetComponentInParent<ISightable>() == target)
			{
				return true;
			}
			LandVehicle componentInParent = ((Component)((RaycastHit)(ref val2)).collider).GetComponentInParent<LandVehicle>();
			if ((Object)(object)((Component)((RaycastHit)(ref val2)).collider).GetComponentInParent<LandVehicle>() != (Object)null)
			{
				Player player = target as Player;
				NPC nPC = target as NPC;
				if ((Object)(object)nPC != (Object)null && componentInParent.OccupantNPCs.Contains(nPC))
				{
					return true;
				}
				if ((Object)(object)player != (Object)null && componentInParent.OccupantPlayers.Contains(player))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public virtual float GetIdealUseRange()
	{
		return Mathf.Lerp(MinUseRange, MaxUseRange, 0.4f);
	}
}
