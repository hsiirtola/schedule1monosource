using ScheduleOne.Audio;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.StationFramework;

public class PourableModule : ItemModule
{
	[Header("Settings")]
	public string LiquidType = "Liquid";

	public float PourRate = 0.2f;

	public float AngleFromUpToPour = 90f;

	public bool OnlyEmptyOverFillable = true;

	public float LiquidCapacity_L = 0.25f;

	public Color LiquidColor;

	public float DefaultLiquid_L = 1f;

	[Header("References")]
	public ParticleSystem[] PourParticles;

	public Transform PourPoint;

	public LiquidContainer LiquidContainer;

	public Draggable Draggable;

	public DraggableConstraint DraggableConstraint;

	public AudioSourceController PourSound;

	[Header("Particles")]
	public Color PourParticlesColor;

	public float ParticleMinMultiplier = 0.8f;

	public float ParticleMaxMultiplier = 1.5f;

	private float[] particleMinSizes;

	private float[] particleMaxSizes;

	private Fillable activeFillable;

	private float timeSinceFillableHit = 10f;

	public bool IsPouring { get; protected set; }

	public float NormalizedPourRate { get; private set; }

	public float LiquidLevel { get; protected set; } = 1f;

	public float NormalizedLiquidLevel => LiquidLevel / LiquidCapacity_L;

	protected virtual void Start()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
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
			CollisionModule collision = PourParticles[i].collision;
			LayerMask collidesWith = ((CollisionModule)(ref collision)).collidesWith;
			collidesWith = LayerMask.op_Implicit(LayerMask.op_Implicit(collidesWith) | (1 << LayerMask.NameToLayer("Task")));
			((CollisionModule)(ref collision)).collidesWith = collidesWith;
			((CollisionModule)(ref collision)).sendCollisionMessages = true;
			((Component)PourParticles[i]).gameObject.AddComponent<ParticleCollisionDetector>().onCollision.AddListener((UnityAction<GameObject>)ParticleCollision);
		}
		if ((Object)(object)LiquidContainer != (Object)null)
		{
			SetLiquidLevel(DefaultLiquid_L);
		}
	}

	public override void ActivateModule(StationItem item)
	{
		base.ActivateModule(item);
		if ((Object)(object)DraggableConstraint != (Object)null)
		{
			DraggableConstraint.SetContainer(((Component)item).transform.parent);
		}
		if ((Object)(object)Draggable != (Object)null)
		{
			Draggable.ClickableEnabled = true;
		}
	}

	protected virtual void FixedUpdate()
	{
		if (base.IsModuleActive)
		{
			UpdatePouring();
			UpdatePourSound();
			if (timeSinceFillableHit > 0.25f)
			{
				activeFillable = null;
			}
			timeSinceFillableHit += Time.fixedDeltaTime;
		}
	}

	protected virtual void UpdatePouring()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Angle(Vector3.up, PourPoint.forward);
		IsPouring = num > AngleFromUpToPour && CanPour();
		NormalizedPourRate = 0f;
		if (IsPouring && NormalizedLiquidLevel > 0f)
		{
			float num2 = (NormalizedPourRate = 0.3f + 0.7f * (num - AngleFromUpToPour) / (180f - AngleFromUpToPour));
			PourAmount(num2 * PourRate * Time.deltaTime);
			for (int i = 0; i < PourParticles.Length; i++)
			{
				MainModule main = PourParticles[i].main;
				float num4 = 1f;
				if ((Object)(object)LiquidContainer != (Object)null)
				{
					num4 = Mathf.Clamp(LiquidContainer.CurrentLiquidLevel, 0.3f, 1f);
				}
				float num5 = ParticleMinMultiplier * num2 * particleMinSizes[i] * num4;
				float num6 = ParticleMaxMultiplier * num2 * particleMaxSizes[i] * num4;
				((MainModule)(ref main)).startSize = new MinMaxCurve(num5, num6);
				((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(PourParticlesColor);
			}
			if (!PourParticles[0].isEmitting && NormalizedLiquidLevel > 0f)
			{
				for (int j = 0; j < PourParticles.Length; j++)
				{
					PourParticles[j].Play();
				}
			}
		}
		else if (PourParticles[0].isEmitting)
		{
			for (int k = 0; k < PourParticles.Length; k++)
			{
				PourParticles[k].Stop(false, (ParticleSystemStopBehavior)1);
			}
		}
		if (NormalizedLiquidLevel == 0f && PourParticles[0].isEmitting)
		{
			for (int l = 0; l < PourParticles.Length; l++)
			{
				PourParticles[l].Stop(false, (ParticleSystemStopBehavior)1);
			}
		}
	}

	private void UpdatePourSound()
	{
		if ((Object)(object)PourSound == (Object)null)
		{
			return;
		}
		if (NormalizedPourRate > 0f)
		{
			PourSound.VolumeMultiplier = NormalizedPourRate;
			if (!PourSound.IsPlaying)
			{
				PourSound.Play();
			}
		}
		else if (PourSound.IsPlaying)
		{
			PourSound.Stop();
		}
	}

	public virtual void ChangeLiquidLevel(float change)
	{
		LiquidLevel = Mathf.Clamp(LiquidLevel + change, 0f, LiquidCapacity_L);
		if ((Object)(object)LiquidContainer != (Object)null)
		{
			LiquidContainer.SetLiquidLevel(NormalizedLiquidLevel);
		}
	}

	public virtual void SetLiquidLevel(float level)
	{
		LiquidLevel = Mathf.Clamp(level, 0f, LiquidCapacity_L);
		if ((Object)(object)LiquidContainer != (Object)null)
		{
			LiquidContainer.SetLiquidLevel(NormalizedLiquidLevel);
		}
	}

	protected virtual void PourAmount(float amount)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		Physics.RaycastAll(PourPoint.position, Vector3.down, 1f, 1 << LayerMask.NameToLayer("Task"));
		if (!OnlyEmptyOverFillable || ((Object)(object)activeFillable != (Object)null && activeFillable.FillableEnabled))
		{
			ChangeLiquidLevel(0f - amount);
			if ((Object)(object)activeFillable != (Object)null)
			{
				activeFillable.AddLiquid(LiquidType, amount, LiquidColor);
			}
		}
	}

	private void ParticleCollision(GameObject other)
	{
		Fillable componentInParent = other.GetComponentInParent<Fillable>();
		if ((Object)(object)componentInParent != (Object)null && ((Behaviour)componentInParent).enabled)
		{
			timeSinceFillableHit = 0f;
			activeFillable = componentInParent;
		}
	}

	protected virtual bool CanPour()
	{
		return true;
	}
}
