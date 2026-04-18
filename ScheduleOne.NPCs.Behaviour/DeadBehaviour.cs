using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class DeadBehaviour : Behaviour
{
	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDeadBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDeadBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public bool IsInMedicalCenter => (Object)(object)base.Npc.CurrentBuilding == (Object)(object)Singleton<ScheduleOne.Map.Map>.Instance.MedicalCentre;

	private void Start()
	{
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepStart = (Action)Delegate.Combine(instance.onSleepStart, new Action(SleepStart));
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onSleepStart = (Action)Delegate.Remove(instance.onSleepStart, new Action(SleepStart));
		}
	}

	public override void Activate()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		base.Activate();
		if (Singleton<LoadManager>.Instance.IsLoading)
		{
			if (base.Npc.Behaviour.RagdollBehaviour.Enabled)
			{
				base.Npc.Behaviour.RagdollBehaviour.Disable();
			}
			EnterMedicalCentre();
		}
		else
		{
			if (!base.Npc.Avatar.Ragdolled)
			{
				base.Npc.Movement.ActivateRagdoll(Vector3.zero, Vector3.zero, 0f);
			}
			base.Npc.Movement.SetRagdollDraggable(draggable: true);
		}
		base.Npc.DialogueHandler.HideWorldspaceDialogue();
		base.Npc.Awareness.SetAwarenessActive(active: false);
		base.Npc.Avatar.EmotionManager.ClearOverrides();
		base.Npc.Avatar.EmotionManager.AddEmotionOverride("Sleeping", "Dead", 0f, 20);
		base.Npc.PlayVO(EVOLineType.Die);
	}

	public override void OnActiveTick()
	{
		base.OnActiveTick();
		if (!IsInMedicalCenter && !base.Npc.Avatar.Ragdolled)
		{
			if (base.Npc.Movement.IsMoving)
			{
				base.Npc.Movement.Stop();
			}
			EnterMedicalCentre();
		}
	}

	private void SleepStart()
	{
		if (base.Active && !IsInMedicalCenter)
		{
			EnterMedicalCentre();
		}
	}

	private void EnterMedicalCentre()
	{
		Console.Log(base.Npc.fullName + " entering medical center");
		base.Npc.Movement.DeactivateRagdoll();
		base.Npc.Movement.SetRagdollDraggable(draggable: false);
		base.Npc.EnterBuilding(null, Singleton<ScheduleOne.Map.Map>.Instance.MedicalCentre.GUID.ToString(), 0);
	}

	public override void Deactivate()
	{
		base.Deactivate();
		base.Npc.Awareness.SetAwarenessActive(active: true);
		base.Npc.Avatar.EmotionManager.RemoveEmotionOverride("Dead");
		base.Npc.Movement.DeactivateRagdoll();
		base.Npc.Movement.SetRagdollDraggable(draggable: false);
		if (IsInMedicalCenter)
		{
			base.Npc.ExitBuilding();
		}
	}

	public override void Disable()
	{
		base.Disable();
		Deactivate();
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDeadBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDeadBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDeadBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDeadBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
