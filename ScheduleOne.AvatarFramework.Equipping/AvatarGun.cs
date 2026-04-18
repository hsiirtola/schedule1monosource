using System.Collections;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Equipping;

public class AvatarGun : AvatarRangedWeapon
{
	[Header("References")]
	public Animation Anim;

	public ParticleSystem ShellParticles;

	public ParticleSystem SmokeParticles;

	public Transform FlashObject;

	[Header("Prefabs")]
	public GameObject RayPrefab;

	private Coroutine flashRoutine;

	protected override void Shoot(Vector3 endPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		base.Shoot(endPoint);
		if ((Object)(object)Anim != (Object)null)
		{
			Anim.Play();
		}
		if ((Object)(object)ShellParticles != (Object)null)
		{
			ShellParticles.Play();
		}
		if ((Object)(object)SmokeParticles != (Object)null)
		{
			SmokeParticles.Play();
		}
		Player componentInParent = ((Component)this).GetComponentInParent<Player>();
		if (!((Object)(object)componentInParent != (Object)null) || !((NetworkBehaviour)componentInParent).IsOwner)
		{
			if (flashRoutine != null)
			{
				((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(flashRoutine);
			}
			flashRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Flash(endPoint));
		}
	}

	private IEnumerator Flash(Vector3 endPoint)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.06f;
		FlashObject.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
		((Component)FlashObject).gameObject.SetActive(true);
		Transform transform = Object.Instantiate<GameObject>(RayPrefab, GameObject.Find("_Temp").transform).transform;
		Object.Destroy((Object)(object)((Component)transform).gameObject, num);
		((Component)transform).transform.position = (MuzzlePoint.position + endPoint) / 2f;
		((Component)transform).transform.LookAt(endPoint);
		((Component)transform).transform.localScale = new Vector3(1f, 1f, Vector3.Distance(MuzzlePoint.position, endPoint));
		yield return (object)new WaitForSeconds(num);
		if ((Object)(object)FlashObject != (Object)null)
		{
			((Component)FlashObject).gameObject.SetActive(false);
		}
	}
}
