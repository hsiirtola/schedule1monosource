using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules;

public class NPCSignal_UseVendingMachine : NPCSignal
{
	private const float destinationThreshold = 1f;

	public VendingMachine MachineOverride;

	private VendingMachine TargetMachine;

	private Coroutine purchaseCoroutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseVendingMachineAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseVendingMachineAssembly_002DCSharp_002Edll_Excuted;

	public new string ActionName => "Use Vending Machine";

	public override string GetName()
	{
		return ActionName;
	}

	public override void Started()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		base.Started();
		if (InstanceFinder.IsServer)
		{
			TargetMachine = GetTargetMachine();
			if ((Object)(object)TargetMachine == (Object)null)
			{
				Debug.LogWarning((object)"No vending machine found for NPC to use");
				End();
			}
			else
			{
				SetDestination(TargetMachine.AccessPoint.position);
			}
		}
	}

	public override void MinPassed()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		base.MinPassed();
		if (!base.IsActive || npc.Movement.IsMoving)
		{
			return;
		}
		if ((Object)(object)TargetMachine == (Object)null)
		{
			TargetMachine = GetTargetMachine();
		}
		if ((Object)(object)TargetMachine == (Object)null)
		{
			Debug.LogWarning((object)"No vending machine found for NPC to use");
			End();
		}
		else if ((Object)(object)TargetMachine.AccessPoint == (Object)null)
		{
			Debug.LogWarning((object)"Vending machine has no access point");
			End();
		}
		else if (IsAtDestination())
		{
			if (purchaseCoroutine == null)
			{
				Purchase();
			}
		}
		else if (npc.Movement.CanGetTo(TargetMachine.AccessPoint.position))
		{
			SetDestination(TargetMachine.AccessPoint.position);
		}
		else
		{
			Debug.LogWarning((object)"Unable to reach vending machine");
			End();
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
		if (purchaseCoroutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(purchaseCoroutine);
			purchaseCoroutine = null;
		}
	}

	public override void Resume()
	{
		base.Resume();
	}

	public override void Skipped()
	{
		base.Skipped();
	}

	private bool IsAtDestination()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TargetMachine == (Object)null)
		{
			return false;
		}
		return Vector3.Distance(npc.Movement.FootPosition, TargetMachine.AccessPoint.position) < 1f;
	}

	private VendingMachine GetTargetMachine()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)MachineOverride != (Object)null && base.movement.CanGetTo(MachineOverride.AccessPoint.position))
		{
			return MachineOverride;
		}
		VendingMachine result = null;
		float num = float.MaxValue;
		foreach (VendingMachine allMachine in VendingMachine.AllMachines)
		{
			if (base.movement.CanGetTo(allMachine.AccessPoint.position))
			{
				float num2 = Vector3.Distance(npc.Movement.FootPosition, allMachine.AccessPoint.position);
				if (num2 < num)
				{
					result = allMachine;
					num = num2;
				}
			}
		}
		return result;
	}

	protected override void WalkCallback(NPCMovement.WalkResult result)
	{
		base.WalkCallback(result);
		if (base.IsActive && result == NPCMovement.WalkResult.Success)
		{
			Purchase();
		}
	}

	[ObserversRpc(RunLocally = true)]
	public void Purchase()
	{
		RpcWriter___Observers_Purchase_2166136261();
		RpcLogic___Purchase_2166136261();
	}

	private bool CheckItem()
	{
		if ((Object)(object)TargetMachine.lastDroppedItem == (Object)null || (Object)(object)((Component)TargetMachine.lastDroppedItem).gameObject == (Object)null)
		{
			ItemWasStolen();
			End();
			return false;
		}
		return true;
	}

	private void ItemWasStolen()
	{
		npc.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "drinkstolen", 20f);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseVendingMachineAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseVendingMachineAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_Purchase_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseVendingMachineAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseVendingMachineAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_Purchase_2166136261()
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

	public void RpcLogic___Purchase_2166136261()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (purchaseCoroutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(purchaseCoroutine);
		}
		if ((Object)(object)TargetMachine == (Object)null)
		{
			TargetMachine = GetTargetMachine();
		}
		if ((Object)(object)TargetMachine != (Object)null)
		{
			npc.Movement.FaceDirection(TargetMachine.AccessPoint.forward);
		}
		purchaseCoroutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Purchase());
		IEnumerator Purchase()
		{
			yield return (object)new WaitForSeconds(1f);
			if ((Object)(object)TargetMachine == (Object)null || TargetMachine.IsBroken)
			{
				purchaseCoroutine = null;
				End();
			}
			else
			{
				TargetMachine.PurchaseRoutine();
				yield return (object)new WaitForSeconds(1f);
				if (!CheckItem())
				{
					purchaseCoroutine = null;
					End();
				}
				else
				{
					npc.SetAnimationTrigger_Networked(null, "GrabItem");
					yield return (object)new WaitForSeconds(0.4f);
					if (!CheckItem())
					{
						purchaseCoroutine = null;
						End();
					}
					else
					{
						TargetMachine.RemoveLastDropped();
						yield return (object)new WaitForSeconds(0.5f);
						End();
						purchaseCoroutine = null;
						npc.Avatar.EmotionManager.AddEmotionOverride("Cheery", "energydrink", 5f);
					}
				}
			}
		}
	}

	private void RpcReader___Observers_Purchase_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Purchase_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
