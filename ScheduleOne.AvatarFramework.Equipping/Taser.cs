using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Equipping;

public class Taser : AvatarRangedWeapon
{
	public const float TaseDuration = 2f;

	public const float TaseMoveSpeedMultiplier = 0.5f;

	[Header("References")]
	public GameObject FlashObject;

	public AudioSourceController ChargeSound;

	[Header("Prefabs")]
	public GameObject RayPrefab;

	private Coroutine flashRoutine;

	public override void Equip(Avatar _avatar)
	{
		base.Equip(_avatar);
		FlashObject.gameObject.SetActive(false);
	}

	protected override void Shoot(Vector3 endPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.Shoot(endPoint);
		if (flashRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(flashRoutine);
		}
		ChargeSound.Stop();
		flashRoutine = ((MonoBehaviour)this).StartCoroutine(Flash(endPoint));
	}

	public override void ApplyHitToDamageable(IDamageable damageable, Vector3 hitPoint)
	{
		if (damageable is Player)
		{
			(damageable as Player).Taze();
		}
	}

	public override void SetIsRaised(bool raised)
	{
		base.SetIsRaised(raised);
		if (base.IsRaised)
		{
			ChargeSound.Play();
		}
		else
		{
			ChargeSound.Stop();
		}
	}

	private IEnumerator Flash(Vector3 endPoint)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.2f;
		FlashObject.gameObject.SetActive(true);
		Transform transform = Object.Instantiate<GameObject>(RayPrefab, GameObject.Find("_Temp").transform).transform;
		Object.Destroy((Object)(object)((Component)transform).gameObject, num);
		((Component)transform).transform.position = (MuzzlePoint.position + endPoint) / 2f;
		((Component)transform).transform.LookAt(endPoint);
		((Component)transform).transform.localScale = new Vector3(1f, 1f, Vector3.Distance(MuzzlePoint.position, endPoint));
		yield return (object)new WaitForSeconds(0.2f);
		FlashObject.gameObject.SetActive(false);
	}

	public override float GetIdealUseRange()
	{
		return MinUseRange;
	}
}
