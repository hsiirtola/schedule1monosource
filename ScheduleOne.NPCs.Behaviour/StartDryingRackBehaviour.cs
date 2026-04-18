using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class StartDryingRackBehaviour : Behaviour
{
	public const float TIME_PER_ITEM = 1f;

	private Coroutine workRoutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartDryingRackBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartDryingRackBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public DryingRack Rack { get; protected set; }

	public bool WorkInProgress { get; protected set; }

	public override void Activate()
	{
		base.Activate();
		StartWork();
	}

	public override void Resume()
	{
		base.Resume();
		StartWork();
	}

	public override void Pause()
	{
		base.Pause();
		if (WorkInProgress)
		{
			StopCauldron();
		}
		if (InstanceFinder.IsServer && (Object)(object)Rack != (Object)null && (Object)(object)Rack.NPCUserObject == (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject)
		{
			Rack.SetNPCUser(null);
		}
	}

	public override void Disable()
	{
		base.Disable();
		if (base.Active)
		{
			Deactivate();
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (WorkInProgress)
		{
			StopCauldron();
		}
		if (InstanceFinder.IsServer && (Object)(object)Rack != (Object)null && (Object)(object)Rack.NPCUserObject == (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject)
		{
			Rack.SetNPCUser(null);
		}
	}

	public override void OnActiveTick()
	{
		base.OnActiveTick();
		if (!InstanceFinder.IsServer || WorkInProgress)
		{
			return;
		}
		if (IsRackReady(Rack))
		{
			if (!base.Npc.Movement.IsMoving)
			{
				if (IsAtStation())
				{
					BeginAction();
				}
				else
				{
					GoToStation();
				}
			}
		}
		else
		{
			Disable_Networked(null);
		}
	}

	private void StartWork()
	{
		if (InstanceFinder.IsServer)
		{
			if (!IsRackReady(Rack))
			{
				Console.LogWarning(base.Npc.fullName + " has no station to work with");
				Disable_Networked(null);
			}
			else
			{
				Rack.SetNPCUser(((NetworkBehaviour)base.Npc).NetworkObject);
			}
		}
	}

	public void AssignRack(DryingRack rack)
	{
		if (!((Object)(object)Rack == (Object)(object)rack))
		{
			if ((Object)(object)Rack != (Object)null && (Object)(object)Rack.NPCUserObject == (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject)
			{
				Rack.SetNPCUser(null);
			}
			Rack = rack;
		}
	}

	public bool IsAtStation()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return base.Npc.Movement.IsAsCloseAsPossible(NavMeshUtility.GetReachableAccessPoint(Rack, base.Npc).position);
	}

	public void GoToStation()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		SetDestination(NavMeshUtility.GetReachableAccessPoint(Rack, base.Npc).position);
	}

	[ObserversRpc(RunLocally = true)]
	public void BeginAction()
	{
		RpcWriter___Observers_BeginAction_2166136261();
		RpcLogic___BeginAction_2166136261();
	}

	private void StopCauldron()
	{
		if (workRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(workRoutine);
		}
		WorkInProgress = false;
	}

	public bool IsRackReady(DryingRack rack)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rack == (Object)null)
		{
			return false;
		}
		if (((IUsable)rack).IsInUse && ((Object)(object)rack.PlayerUserObject != (Object)null || (Object)(object)rack.NPCUserObject != (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject))
		{
			return false;
		}
		if (rack.InputSlot.Quantity <= 0)
		{
			return false;
		}
		if (rack.GetTotalDryingItems() >= rack.ItemCapacity)
		{
			return false;
		}
		if ((float)rack.InputSlot.Quantity < ((DryingRackConfiguration)rack.Configuration).StartThreshold.Value)
		{
			return false;
		}
		if (!base.Npc.Movement.CanGetTo(((Component)rack).transform.position))
		{
			return false;
		}
		return true;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartDryingRackBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartDryingRackBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_BeginAction_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartDryingRackBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartDryingRackBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_BeginAction_2166136261()
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

	public void RpcLogic___BeginAction_2166136261()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!WorkInProgress && !((Object)(object)Rack == (Object)null))
		{
			WorkInProgress = true;
			base.Npc.Movement.FacePoint(Rack.uiPoint.position);
			workRoutine = ((MonoBehaviour)this).StartCoroutine(Package());
		}
		IEnumerator Package()
		{
			yield return (object)new WaitForEndOfFrame();
			Rack.InputSlot.ItemInstance.GetCopy(1);
			int itemCount = 0;
			while ((Object)(object)Rack != (Object)null && Rack.InputSlot.Quantity > itemCount && Rack.GetTotalDryingItems() + itemCount < Rack.ItemCapacity)
			{
				base.Npc.Avatar.Animation.SetTrigger("GrabItem");
				yield return (object)new WaitForSeconds(1f / (base.Npc as Employee).CurrentWorkSpeed);
				itemCount++;
			}
			if (InstanceFinder.IsServer)
			{
				Rack.StartOperation();
			}
			WorkInProgress = false;
			workRoutine = null;
		}
	}

	private void RpcReader___Observers_BeginAction_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___BeginAction_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
