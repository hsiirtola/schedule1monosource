using System.Collections;
using System.Linq;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.FX;

public class FXManager : Singleton<FXManager>
{
	public AudioClip[] PunchImpactsClips;

	public AudioClip[] SlashImpactClips;

	[Header("References")]
	public AudioSourceController[] ImpactSources;

	[Header("Particle Prefabs")]
	public GameObject PunchParticlePrefab;

	[Header("Trails")]
	public TrailRenderer BulletTrail;

	protected override void Start()
	{
		base.Start();
	}

	public void CreateImpactFX(Impact impact, IDamageable target)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		AudioClip impactSound = GetImpactSound(impact, target);
		if ((Object)(object)impactSound != (Object)null)
		{
			PlayImpact(impactSound, impact.HitPoint, Mathf.Clamp01(impact.ImpactForce / 400f));
		}
		GameObject impactParticles = GetImpactParticles(impact, target);
		if ((Object)(object)impactParticles != (Object)null)
		{
			PlayParticles(impactParticles, impact.HitPoint, Quaternion.LookRotation(-impact.ImpactForceDirection));
		}
	}

	public void CreateBulletTrail(Vector3 start, Vector3 dir, float speed, float range, LayerMask mask)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		TrailRenderer trail = Object.Instantiate<TrailRenderer>(BulletTrail, NetworkSingleton<GameManager>.Instance.Temp);
		((Component)trail).transform.position = start;
		((Component)trail).transform.forward = dir;
		float maxDistance = range;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(start, dir, ref val, range, LayerMask.op_Implicit(mask)))
		{
			maxDistance = ((RaycastHit)(ref val)).distance;
		}
		Debug.DrawRay(start, dir * maxDistance, Color.red, 5f);
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			do
			{
				yield return null;
				Transform transform = ((Component)trail).transform;
				transform.position += ((Component)trail).transform.forward * speed * Time.deltaTime;
			}
			while (!(Vector3.Distance(start, ((Component)trail).transform.position) > maxDistance));
			((Component)trail).transform.position = start + ((Component)trail).transform.forward * maxDistance;
			yield return (object)new WaitForEndOfFrame();
			((Component)trail).transform.position = start + ((Component)trail).transform.forward * maxDistance;
			yield return (object)new WaitForSeconds(1f);
			Object.Destroy((Object)(object)((Component)trail).gameObject);
		}
	}

	private void PlayImpact(AudioClip clip, Vector3 position, float volume)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		AudioSourceController source = GetSource();
		if ((Object)(object)source == (Object)null)
		{
			Console.LogWarning("No available audio source controller found");
			return;
		}
		((Component)source).transform.position = position;
		source.SetClip(clip);
		source.VolumeMultiplier = volume;
		source.Play();
	}

	private void PlayParticles(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Object.Destroy((Object)(object)Object.Instantiate<GameObject>(prefab, position, rotation), 2f);
	}

	private AudioClip GetImpactSound(Impact impact, IDamageable target)
	{
		if (target is NPC || target is Player)
		{
			if (impact.ImpactType == EImpactType.SharpMetal)
			{
				return GetRandomClip(SlashImpactClips);
			}
			return GetRandomClip(PunchImpactsClips);
		}
		return null;
	}

	private GameObject GetImpactParticles(Impact impact, IDamageable target)
	{
		if (target is NPC || target is Player)
		{
			return PunchParticlePrefab;
		}
		return null;
	}

	private AudioSourceController GetSource()
	{
		return ImpactSources.FirstOrDefault((AudioSourceController x) => !x.IsPlaying);
	}

	private static AudioClip GetRandomClip(AudioClip[] clips)
	{
		return clips[Random.Range(0, clips.Length)];
	}
}
