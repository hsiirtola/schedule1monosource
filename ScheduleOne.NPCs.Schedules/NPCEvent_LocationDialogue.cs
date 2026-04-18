using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Dialogue;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules;

public class NPCEvent_LocationDialogue : NPCEvent
{
	public Transform Destination;

	public bool FaceDestinationDir = true;

	public float DestinationThreshold = 1f;

	public bool WarpIfSkipped;

	[Header("Dialogue Settings")]
	public int GreetingOverrideToEnable = -1;

	public int ChoiceToEnable = -1;

	public DialogueContainer DialogueOverride;

	protected bool IsActionStarted;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_LocationDialogueAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_LocationDialogueAssembly_002DCSharp_002Edll_Excuted;

	public new string ActionName => "Location-based dialogue";

	public override string GetName()
	{
		if ((Object)(object)Destination == (Object)null)
		{
			return ActionName + " (No destination set)";
		}
		string actionName = ActionName;
		Transform destination = Destination;
		return actionName + " (" + ((destination != null) ? ((Object)destination).name : null) + ")";
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (base.IsActive && IsActionStarted)
		{
			StartAction(connection);
		}
	}

	public override void Started()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		base.Started();
		if (IsAtDestination())
		{
			StartAction(null);
		}
		else
		{
			SetDestination(Destination.position);
		}
	}

	public override void OnActiveTick()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (!InstanceFinder.IsServer || (Object)(object)Destination == (Object)null)
		{
			return;
		}
		if (npc.Movement.IsMoving)
		{
			if (Vector3.Distance(npc.Movement.CurrentDestination, Destination.position) > DestinationThreshold)
			{
				SetDestination(Destination.position);
			}
		}
		else if (IsAtDestination())
		{
			if (FaceDestinationDir && !npc.Movement.FaceDirectionInProgress && Vector3.Angle(((Component)this).transform.forward, Destination.forward) > 5f)
			{
				npc.Movement.FaceDirection(Destination.forward);
			}
		}
		else
		{
			SetDestination(Destination.position);
		}
	}

	public override void LateStarted()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		base.LateStarted();
		if (IsAtDestination())
		{
			WalkCallback(NPCMovement.WalkResult.Success);
		}
		else
		{
			SetDestination(Destination.position);
		}
	}

	public override void JumpTo()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		base.JumpTo();
		if (!IsAtDestination())
		{
			if (npc.Movement.IsMoving)
			{
				npc.Movement.Stop();
			}
			if (InstanceFinder.IsServer)
			{
				npc.Movement.Warp(Destination.position);
			}
		}
		if (InstanceFinder.IsServer)
		{
			WalkCallback(NPCMovement.WalkResult.Success);
		}
	}

	public override void End()
	{
		base.End();
		if (IsActionStarted)
		{
			EndAction();
		}
	}

	public override void Interrupt()
	{
		base.Interrupt();
		if (npc.Movement.IsMoving)
		{
			npc.Movement.Stop();
		}
		if (IsActionStarted)
		{
			EndAction();
		}
	}

	public override void Resume()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		base.Resume();
		if (IsAtDestination())
		{
			WalkCallback(NPCMovement.WalkResult.Success);
		}
		else
		{
			SetDestination(Destination.position);
		}
	}

	public override void Skipped()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		base.Skipped();
		if (WarpIfSkipped)
		{
			npc.Movement.Warp(Destination.position);
		}
	}

	private bool IsAtDestination()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Destination == (Object)null)
		{
			return true;
		}
		return Vector3.Distance(npc.Movement.FootPosition, Destination.position) < DestinationThreshold;
	}

	protected override void WalkCallback(NPCMovement.WalkResult result)
	{
		base.WalkCallback(result);
		if (base.IsActive && result == NPCMovement.WalkResult.Success && InstanceFinder.IsServer)
		{
			StartAction(null);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	protected virtual void StartAction(NetworkConnection conn)
	{
		if (conn == null)
		{
			RpcWriter___Observers_StartAction_328543758(conn);
			RpcLogic___StartAction_328543758(conn);
		}
		else
		{
			RpcWriter___Target_StartAction_328543758(conn);
		}
	}

	[ObserversRpc(RunLocally = true)]
	protected virtual void EndAction()
	{
		RpcWriter___Observers_EndAction_2166136261();
		RpcLogic___EndAction_2166136261();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_LocationDialogueAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_LocationDialogueAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_StartAction_328543758));
			((NetworkBehaviour)this).RegisterTargetRpc(1u, new ClientRpcDelegate(RpcReader___Target_StartAction_328543758));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_EndAction_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_LocationDialogueAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_LocationDialogueAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_StartAction_328543758(NetworkConnection conn)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	protected virtual void RpcLogic___StartAction_328543758(NetworkConnection conn)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (IsActionStarted)
		{
			return;
		}
		if (FaceDestinationDir && (Object)(object)Destination != (Object)null)
		{
			npc.Movement.FaceDirection(Destination.forward);
		}
		IsActionStarted = true;
		DialogueController component = ((Component)npc.DialogueHandler).GetComponent<DialogueController>();
		if ((Object)(object)DialogueOverride != (Object)null)
		{
			component.OverrideContainer = DialogueOverride;
			return;
		}
		component.OverrideContainer = null;
		if (component.GreetingOverrides.Count > GreetingOverrideToEnable && GreetingOverrideToEnable >= 0)
		{
			component.GreetingOverrides[GreetingOverrideToEnable].ShouldShow = true;
		}
		if (component.Choices.Count > ChoiceToEnable && ChoiceToEnable >= 0)
		{
			component.Choices[ChoiceToEnable].Enabled = true;
		}
	}

	private void RpcReader___Observers_StartAction_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartAction_328543758(null);
		}
	}

	private void RpcWriter___Target_StartAction_328543758(NetworkConnection conn)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendTargetRpc(1u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_StartAction_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___StartAction_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	private void RpcWriter___Observers_EndAction_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	protected virtual void RpcLogic___EndAction_2166136261()
	{
		if (!IsActionStarted)
		{
			return;
		}
		IsActionStarted = false;
		DialogueController component = ((Component)npc.DialogueHandler).GetComponent<DialogueController>();
		if ((Object)(object)DialogueOverride != (Object)null)
		{
			component.OverrideContainer = null;
			return;
		}
		if (component.GreetingOverrides.Count > GreetingOverrideToEnable && GreetingOverrideToEnable >= 0)
		{
			component.GreetingOverrides[GreetingOverrideToEnable].ShouldShow = false;
		}
		if (component.Choices.Count > ChoiceToEnable && ChoiceToEnable >= 0)
		{
			component.Choices[ChoiceToEnable].Enabled = false;
		}
	}

	private void RpcReader___Observers_EndAction_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___EndAction_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
