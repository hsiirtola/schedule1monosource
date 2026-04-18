using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class UnconsciousBehaviour : Behaviour
{
	public const float SnoreInterval = 6f;

	private const float SnoreChance = 0.5f;

	public ParticleSystem Particles;

	public bool PlaySnoreSounds = true;

	private float timeOnLastSnore;

	private bool _shouldPlaySnoreSounds;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EUnconsciousBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EUnconsciousBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public override void Activate()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		base.Activate();
		base.Npc.Behaviour.RagdollBehaviour.Disable();
		if (!base.Npc.Avatar.Ragdolled)
		{
			base.Npc.Movement.ActivateRagdoll(Vector3.zero, Vector3.zero, 0f);
		}
		_shouldPlaySnoreSounds = PlaySnoreSounds && Random.value < 0.5f;
		base.Npc.Movement.SetRagdollDraggable(draggable: true);
		base.Npc.DialogueHandler.HideWorldspaceDialogue();
		base.Npc.Awareness.SetAwarenessActive(active: false);
		base.Npc.Avatar.EmotionManager.ClearOverrides();
		base.Npc.Avatar.EmotionManager.AddEmotionOverride("Sleeping", "Dead", 0f, 20);
		Particles.Play();
		base.Npc.PlayVO(EVOLineType.Die);
		timeOnLastSnore = Time.time;
	}

	public override void Deactivate()
	{
		base.Deactivate();
		base.Npc.Awareness.SetAwarenessActive(active: true);
		base.Npc.Avatar.EmotionManager.RemoveEmotionOverride("Dead");
		if (!base.Npc.Behaviour.DeadBehaviour.Enabled)
		{
			base.Npc.Movement.DeactivateRagdoll();
		}
		base.Npc.Movement.SetRagdollDraggable(draggable: false);
		Particles.Stop();
	}

	public override void OnActiveTick()
	{
		base.OnActiveTick();
		if (_shouldPlaySnoreSounds && Time.time - timeOnLastSnore > 6f)
		{
			base.Npc.PlayVO(EVOLineType.Snore);
			timeOnLastSnore = Time.time;
		}
	}

	public override void Disable()
	{
		base.Disable();
		Deactivate();
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EUnconsciousBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EUnconsciousBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EUnconsciousBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EUnconsciousBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
