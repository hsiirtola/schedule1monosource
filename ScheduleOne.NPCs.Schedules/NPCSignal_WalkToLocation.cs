using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules;

public class NPCSignal_WalkToLocation : NPCSignal
{
	public Transform Destination;

	public bool FaceDestinationDir = true;

	public float DestinationThreshold = 1f;

	public bool WarpIfSkipped;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WalkToLocationAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WalkToLocationAssembly_002DCSharp_002Edll_Excuted;

	public new string ActionName => "Walk to location";

	public override string GetName()
	{
		return ActionName + " (" + ((Object)Destination).name + ")";
	}

	public override void Started()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		base.Started();
		SetDestination(Destination.position);
	}

	public override void ActiveUpdate()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.ActiveUpdate();
		if (!npc.Movement.IsMoving && !IsAtDestination())
		{
			SetDestination(Destination.position);
		}
	}

	public override void LateStarted()
	{
		base.LateStarted();
	}

	public override void Interrupt()
	{
		base.Interrupt();
		if (npc.Movement.IsMoving)
		{
			npc.Movement.Stop();
		}
	}

	public override void Resume()
	{
		base.Resume();
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
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(npc.Movement.FootPosition, Destination.position) < DestinationThreshold;
	}

	protected override void WalkCallback(NPCMovement.WalkResult result)
	{
		base.WalkCallback(result);
		if (base.IsActive)
		{
			if (result != NPCMovement.WalkResult.Success)
			{
				Debug.LogWarning((object)"NPC walk to location not successful");
				return;
			}
			ReachedDestination();
			End();
		}
	}

	[ObserversRpc]
	private void ReachedDestination()
	{
		RpcWriter___Observers_ReachedDestination_2166136261();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WalkToLocationAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WalkToLocationAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_ReachedDestination_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WalkToLocationAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WalkToLocationAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_ReachedDestination_2166136261()
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

	private void RpcLogic___ReachedDestination_2166136261()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (FaceDestinationDir)
		{
			npc.Movement.FaceDirection(Destination.forward);
		}
	}

	private void RpcReader___Observers_ReachedDestination_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReachedDestination_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
