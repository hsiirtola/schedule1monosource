using System;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.Trash;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScheduleOne.PlayerTasks;

[RequireComponent(typeof(Accelerometer))]
public class Pourable : Draggable
{
	public Action onInitialPour;

	[Header("Pourable settings")]
	public bool Unlimited;

	public float StartQuantity = 10f;

	public float PourRate_L = 0.25f;

	public float AngleFromUpToPour = 90f;

	[Tooltip("Multiplier for pour rate when pourable is shaken up and down")]
	public float ShakeBoostRate = 1.35f;

	public bool AffectsCoverage;

	[Header("Particles")]
	public float ParticleMinMultiplier = 0.8f;

	public float ParticleMaxMultiplier = 1.5f;

	[Header("Pourable References")]
	public ParticleSystem[] PourParticles;

	public Transform PourPoint;

	public AudioSourceController PourLoop;

	[Header("Trash")]
	public TrashItem TrashItem;

	[HideInInspector]
	public GrowContainer TargetGrowContainer;

	protected bool hasPoured;

	protected bool autoSetCurrentQuantity = true;

	private float[] particleMinSizes;

	private float[] particleMaxSizes;

	private AverageAcceleration accelerometer;

	public bool IsPouring { get; protected set; }

	public float NormalizedPourRate { get; private set; }

	public float CurrentQuantity { get; protected set; }

	protected virtual void Start()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		if (autoSetCurrentQuantity)
		{
			CurrentQuantity = StartQuantity;
		}
		accelerometer = ((Component)this).GetComponent<AverageAcceleration>();
		if ((Object)(object)accelerometer == (Object)null)
		{
			accelerometer = ((Component)this).gameObject.AddComponent<AverageAcceleration>();
		}
		particleMinSizes = new float[PourParticles.Length];
		particleMaxSizes = new float[PourParticles.Length];
		for (int i = 0; i < PourParticles.Length; i++)
		{
			float[] array = particleMinSizes;
			int num = i;
			MainModule main = PourParticles[i].main;
			MinMaxCurve startSize = ((MainModule)(ref main)).startSize;
			array[num] = ((MinMaxCurve)(ref startSize)).constantMin;
			float[] array2 = particleMaxSizes;
			int num2 = i;
			main = PourParticles[i].main;
			startSize = ((MainModule)(ref main)).startSize;
			array2[num2] = ((MinMaxCurve)(ref startSize)).constantMax;
		}
	}

	protected override void Update()
	{
		base.Update();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		UpdatePouring();
	}

	protected virtual void UpdatePouring()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Angle(Vector3.up, PourPoint.forward);
		IsPouring = num > AngleFromUpToPour && CanPour();
		NormalizedPourRate = 0f;
		if (IsPouring && CurrentQuantity > 0f)
		{
			float num2 = (NormalizedPourRate = (0.3f + 0.7f * (num - AngleFromUpToPour) / (180f - AngleFromUpToPour)) * GetShakeBoost());
			if ((Object)(object)PourLoop != (Object)null)
			{
				PourLoop.VolumeMultiplier = num2 - 0.3f;
				if (!PourLoop.IsPlaying)
				{
					PourLoop.Play();
				}
			}
			PourAmount(PourRate_L * num2 * Time.deltaTime);
			for (int i = 0; i < PourParticles.Length; i++)
			{
				MainModule main = PourParticles[i].main;
				float num4 = ParticleMinMultiplier * num2 * particleMinSizes[i];
				float num5 = ParticleMaxMultiplier * num2 * particleMaxSizes[i];
				((MainModule)(ref main)).startSize = new MinMaxCurve(num4, num5);
			}
			if (!PourParticles[0].isEmitting && CurrentQuantity > 0f)
			{
				for (int j = 0; j < PourParticles.Length; j++)
				{
					PourParticles[j].Play();
				}
			}
		}
		else
		{
			if ((Object)(object)PourLoop != (Object)null && PourLoop.IsPlaying)
			{
				PourLoop.Stop();
			}
			if (PourParticles[0].isEmitting)
			{
				for (int k = 0; k < PourParticles.Length; k++)
				{
					PourParticles[k].Stop(false, (ParticleSystemStopBehavior)1);
				}
			}
		}
		if (CurrentQuantity == 0f && PourParticles[0].isEmitting)
		{
			for (int l = 0; l < PourParticles.Length; l++)
			{
				PourParticles[l].Stop(false, (ParticleSystemStopBehavior)1);
			}
		}
	}

	private float GetShakeBoost()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Lerp(1f, ShakeBoostRate, Mathf.Clamp(accelerometer.Acceleration.y / 0.75f, 0f, 1f));
	}

	protected virtual void PourAmount(float amount)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (!Unlimited)
		{
			CurrentQuantity = Mathf.Clamp(CurrentQuantity - amount, 0f, StartQuantity);
		}
		if (AffectsCoverage && IsPourPointOverPot())
		{
			TargetGrowContainer.SurfaceCover.QueuePour(PourPoint.position + PourPoint.forward * 0.05f);
		}
		if (!hasPoured)
		{
			if (onInitialPour != null)
			{
				onInitialPour();
			}
			hasPoured = true;
		}
	}

	protected bool IsPourPointOverPot()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return TargetGrowContainer.IsPointAboveGrowSurface(PourPoint.position);
	}

	protected virtual bool CanPour()
	{
		return true;
	}
}
