using System.Collections;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.StationFramework;

[RequireComponent(typeof(Draggable))]
public class IngredientPiece : MonoBehaviour
{
	public const float LIQUID_FRICTION = 100f;

	[Header("References")]
	public Transform ModelContainer;

	public ParticleSystem DissolveParticles;

	[Header("Settings")]
	public bool DetectLiquid = true;

	public bool DisableInteractionInLiquid = true;

	[Range(0f, 2f)]
	public float LiquidFrictionMultiplier = 1f;

	private Draggable draggable;

	private float defaultDrag;

	private Coroutine dissolveParticleRoutine;

	public float CurrentDissolveAmount { get; private set; }

	public LiquidContainer CurrentLiquidContainer { get; private set; }

	private void Start()
	{
		((MonoBehaviour)this).InvokeRepeating("CheckLiquid", 0f, 0.05f);
		draggable = ((Component)this).GetComponent<Draggable>();
		defaultDrag = draggable.NormalRBDrag;
	}

	private void Update()
	{
		if (DisableInteractionInLiquid && (Object)(object)CurrentLiquidContainer != (Object)null)
		{
			draggable.ClickableEnabled = false;
		}
	}

	private void FixedUpdate()
	{
		UpdateDrag();
	}

	private void UpdateDrag()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)CurrentLiquidContainer != (Object)null)
		{
			Vector3 velocity = draggable.Rb.velocity;
			Vector3 val = -((Vector3)(ref velocity)).normalized;
			float viscosity = CurrentLiquidContainer.Viscosity;
			velocity = draggable.Rb.velocity;
			float num = viscosity * ((Vector3)(ref velocity)).magnitude * 100f * LiquidFrictionMultiplier;
			draggable.Rb.AddForce(val * num, (ForceMode)5);
		}
	}

	private void CheckLiquid()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		CurrentLiquidContainer = null;
		if (!DetectLiquid)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(((Component)this).transform.position, 0.001f, 1 << LayerMask.NameToLayer("Task"), (QueryTriggerInteraction)2);
		LiquidVolumeCollider liquidVolumeCollider = default(LiquidVolumeCollider);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].isTrigger && ((Component)array[i]).TryGetComponent<LiquidVolumeCollider>(ref liquidVolumeCollider))
			{
				CurrentLiquidContainer = liquidVolumeCollider.LiquidContainer;
				break;
			}
		}
	}

	public void DissolveAmount(float amount, bool showParticles = true)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentDissolveAmount >= 1f)
		{
			return;
		}
		CurrentDissolveAmount = Mathf.Clamp01(CurrentDissolveAmount + amount);
		((Component)ModelContainer).transform.localScale = Vector3.one * (1f - CurrentDissolveAmount);
		if (showParticles)
		{
			if (!DissolveParticles.isPlaying)
			{
				DissolveParticles.Play();
			}
			if (dissolveParticleRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(dissolveParticleRoutine);
			}
			dissolveParticleRoutine = ((MonoBehaviour)this).StartCoroutine(DissolveParticlesRoutine());
		}
		IEnumerator DissolveParticlesRoutine()
		{
			yield return (object)new WaitForSeconds(0.2f);
			DissolveParticles.Stop();
			dissolveParticleRoutine = null;
		}
	}
}
