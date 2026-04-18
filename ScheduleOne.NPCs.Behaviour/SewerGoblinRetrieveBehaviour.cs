using System;
using System.Collections;
using FishNet;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.PlayerScripts;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.NPCs.Behaviour;

public class SewerGoblinRetrieveBehaviour : Behaviour
{
	public const float PROXIMITY_THRESHOLD = 2f;

	public const float TIMEOUT = 20f;

	private SewerGoblin sewerGoblin;

	public Action onRetrieveComplete;

	public Action onRetrieveCancelled;

	private float timeSinceStart;

	private bool grabbing;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ESewerGoblinRetrieveBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ESewerGoblinRetrieveBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public Player Target => sewerGoblin.TargetPlayer;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002ESewerGoblinRetrieveBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void Activate()
	{
		base.Activate();
		StartBehaviour();
	}

	public override void Resume()
	{
		base.Resume();
		StartBehaviour();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		StopBehaviour();
	}

	public override void Pause()
	{
		base.Pause();
		StopBehaviour();
	}

	private void StartBehaviour()
	{
		base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("SewerGoblinRetrieveBehaviour", 5, 0.12f));
		base.Npc.PlayVO(EVOLineType.Surprised);
		timeSinceStart = 0f;
		grabbing = false;
	}

	private void StopBehaviour()
	{
		Console.Log("Stopping SewerGoblinRetrieveBehaviour");
		base.Npc.Movement.SpeedController.RemoveSpeedControl("SewerGoblinRetrieveBehaviour");
	}

	public void CancelRetrieve()
	{
		Console.Log("Retrieve cancelled");
		if (onRetrieveCancelled != null)
		{
			onRetrieveCancelled();
		}
		Disable_Server();
	}

	private void CompleteRetrieve()
	{
		Console.Log("Retrieve completed");
		grabbing = true;
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			base.Npc.SetAnimationTrigger_Networked(null, "GrabItem");
			base.Npc.Movement.FacePoint(((Component)Target).transform.position);
			yield return (object)new WaitForSeconds(0.5f);
			if (!sewerGoblin.IsPlayerValidTarget(Target))
			{
				CancelRetrieve();
			}
			else if (!sewerGoblin.IsPlayerHoldingPacifyItem(Target))
			{
				CancelRetrieve();
			}
			else
			{
				Target.RemoveEquippedItemFromInventory(((BaseItemDefinition)sewerGoblin.PacifyItem).ID, 1);
				if (onRetrieveComplete != null)
				{
					onRetrieveComplete();
				}
				Disable_Server();
			}
		}
	}

	public override void BehaviourUpdate()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		base.BehaviourUpdate();
		if (!InstanceFinder.IsServer || grabbing)
		{
			return;
		}
		if (!sewerGoblin.IsPlayerValidTarget(Target))
		{
			CancelRetrieve();
			return;
		}
		if (!sewerGoblin.IsPlayerHoldingPacifyItem(Target))
		{
			CancelRetrieve();
			return;
		}
		timeSinceStart += Time.deltaTime;
		if (timeSinceStart > 20f)
		{
			sewerGoblin.cancelledRetrieveAttempts = 999;
			CancelRetrieve();
		}
		else if (WithinRangeOfTarget() && timeSinceStart > 0.5f)
		{
			CompleteRetrieve();
		}
		else if (!IsTargetDestinationValid())
		{
			if (GetNewDestination(out var dest))
			{
				base.Npc.Movement.SetDestination(dest);
			}
			else
			{
				CancelRetrieve();
			}
		}
	}

	private bool IsTargetDestinationValid()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Npc.Movement.IsMoving)
		{
			return false;
		}
		if (Vector3.Distance(base.Npc.Movement.CurrentDestination, ((Component)Target).transform.position) > 2f)
		{
			return false;
		}
		if (base.Npc.Movement.Agent.path == null)
		{
			return false;
		}
		return true;
	}

	private bool GetNewDestination(out Vector3 dest)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		dest = ((Component)Target).transform.position + ((Component)Target).transform.forward * 0.75f;
		if (NavMeshUtility.SamplePosition(dest, out var hit, 10f, -1))
		{
			dest = ((NavMeshHit)(ref hit)).position;
			return true;
		}
		Console.LogError("Failed to find valid destination for RequestProductBehaviour: stopping");
		return false;
	}

	private bool WithinRangeOfTarget()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(((Component)base.Npc).transform.position, ((Component)Target).transform.position) <= 2f;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ESewerGoblinRetrieveBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ESewerGoblinRetrieveBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ESewerGoblinRetrieveBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ESewerGoblinRetrieveBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002ESewerGoblinRetrieveBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		sewerGoblin = base.Npc as SewerGoblin;
	}
}
