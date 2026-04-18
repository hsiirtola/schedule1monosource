using System;
using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Vehicles.Sound;

public class VehicleSound : MonoBehaviour
{
	public const float COLLISION_SOUND_COOLDOWN = 0.5f;

	public const float AUDIO_LERP_SPEED = 2f;

	public const float MinCollisionMomentum = 4000f;

	public const float MaxCollisionMomentum = 25000f;

	public const float MinCollisionVolume = 0.2f;

	public const float MaxCollisionVolume = 0.8f;

	public const float MinCollisionPitch = 0.6f;

	public const float MaxCollisionPitch = 1.1f;

	public float EngineVolumeMultiplier = 1f;

	public float EnginePitchMultiplier = 1f;

	[Header("References")]
	public AudioSourceController EngineStartSource;

	public AudioSourceController EngineIdleSource;

	public AudioSourceController EngineLoopSource;

	public AudioSourceController HandbrakeSource;

	public AudioSourceController ImpactSound;

	[Header("Engine Loop Settings")]
	public AnimationCurve EngineLoopPitchCurve;

	public AnimationCurve EngineLoopVolumeCurve;

	private float lastCollisionTime;

	private float lastCollisionMomentum;

	private Coroutine volumeRoutine;

	public LandVehicle Vehicle { get; private set; }

	protected virtual void Awake()
	{
		Vehicle = ((Component)this).GetComponentInParent<LandVehicle>();
		if (!((Object)(object)Vehicle == (Object)null))
		{
			LandVehicle vehicle = Vehicle;
			vehicle.onVehicleStart = (Action)Delegate.Combine(vehicle.onVehicleStart, new Action(EngineStart));
			LandVehicle vehicle2 = Vehicle;
			vehicle2.onHandbrakeApplied = (Action)Delegate.Combine(vehicle2.onHandbrakeApplied, new Action(HandbrakeApplied));
			LandVehicle vehicle3 = Vehicle;
			vehicle3.onCollision = (Action<Collision>)Delegate.Combine(vehicle3.onCollision, new Action<Collision>(OnCollision));
			EngineIdleSource.VolumeMultiplier = 0f;
			EngineLoopSource.VolumeMultiplier = 0f;
		}
	}

	private void EngineStart()
	{
		EngineStartSource.VolumeMultiplier = EngineVolumeMultiplier;
		EngineStartSource.Play();
		StartUpdateVolume();
	}

	private void HandbrakeApplied()
	{
		if (Vehicle.IsOccupied)
		{
			HandbrakeSource.Play();
		}
	}

	private void StartUpdateVolume()
	{
		if (volumeRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(volumeRoutine);
		}
		volumeRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (!EngineLoopSource.IsPlaying)
			{
				EngineLoopSource.Play();
			}
			if (!EngineIdleSource.IsPlaying)
			{
				EngineIdleSource.Play();
			}
			while (Vehicle.IsOccupied)
			{
				yield return (object)new WaitForEndOfFrame();
				UpdateIdle(engineRunning: true);
				UpdateEngineLoop(engineRunning: true, Mathf.Abs(Vehicle.Speed_Kmh / Vehicle.TopSpeed));
			}
			while (EngineIdleSource.VolumeMultiplier > 0.01f || EngineLoopSource.VolumeMultiplier > 0.01f)
			{
				yield return (object)new WaitForEndOfFrame();
				UpdateIdle(engineRunning: false);
				UpdateEngineLoop(engineRunning: false, 0f);
			}
			EngineLoopSource.Stop();
			EngineIdleSource.Stop();
			volumeRoutine = null;
		}
	}

	private void UpdateIdle(bool engineRunning)
	{
		if (engineRunning)
		{
			EngineIdleSource.VolumeMultiplier = Mathf.MoveTowards(EngineIdleSource.VolumeMultiplier, EngineVolumeMultiplier, Time.deltaTime * 2f);
		}
		else
		{
			EngineIdleSource.VolumeMultiplier = Mathf.MoveTowards(EngineIdleSource.VolumeMultiplier, 0f, Time.deltaTime * 2f);
		}
		EngineIdleSource.PitchMultiplier = EnginePitchMultiplier;
	}

	private void UpdateEngineLoop(bool engineRunning, float normalizedspeed)
	{
		float num = EngineLoopPitchCurve.Evaluate(normalizedspeed) * EnginePitchMultiplier;
		float num2 = EngineLoopVolumeCurve.Evaluate(normalizedspeed) * EngineVolumeMultiplier;
		if (!engineRunning)
		{
			num2 = 0f;
			num = EngineLoopPitchCurve.Evaluate(0f) * EnginePitchMultiplier;
		}
		EngineLoopSource.VolumeMultiplier = Mathf.MoveTowards(EngineLoopSource.VolumeMultiplier, num2, Time.deltaTime * 2f);
		EngineLoopSource.PitchMultiplier = Mathf.MoveTowards(EngineLoopSource.PitchMultiplier, num, Time.deltaTime * 2f);
	}

	private void OnCollision(Collision collision)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		Vector3 relativeVelocity = collision.relativeVelocity;
		float num = ((Vector3)(ref relativeVelocity)).magnitude * Vehicle.Rb.mass;
		if (collision.gameObject.layer == LayerMask.NameToLayer("NPC"))
		{
			num *= 0.2f;
		}
		if (!(num < 4000f) && (!(Time.time - lastCollisionTime < 0.5f) || !(num < lastCollisionMomentum)))
		{
			float num2 = Mathf.InverseLerp(4000f, 25000f, num);
			ImpactSound.VolumeMultiplier = Mathf.Lerp(0.2f, 0.8f, num2);
			ImpactSound.PitchMultiplier = Mathf.Lerp(1.1f, 0.6f, num2);
			((Component)ImpactSound).transform.position = ((ContactPoint)(ref collision.contacts[0])).point;
			ImpactSound.Play();
			lastCollisionTime = Time.time;
			lastCollisionMomentum = num;
		}
	}
}
