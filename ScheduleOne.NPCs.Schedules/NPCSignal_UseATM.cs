using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules;

public class NPCSignal_UseATM : NPCSignal
{
	private const float destinationThreshold = 2f;

	public ATM ATM;

	private Coroutine purchaseCoroutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseATMAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseATMAssembly_002DCSharp_002Edll_Excuted;

	public new string ActionName => "Use ATM";

	public override string GetName()
	{
		return ActionName;
	}

	public override void Started()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		base.Started();
		if ((Object)(object)ATM == (Object)null)
		{
			Debug.LogWarning((object)"No ATM found for NPC to use");
			End();
		}
		else
		{
			SetDestination(ATM.AccessPoint.position);
		}
	}

	public override void OnActiveTick()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		MinPassed();
		if ((Object)(object)ATM == (Object)null)
		{
			End();
		}
		else
		{
			if (purchaseCoroutine != null || npc.Movement.IsMoving)
			{
				return;
			}
			if (IsAtDestination())
			{
				if (purchaseCoroutine == null)
				{
					Purchase();
				}
			}
			else
			{
				Debug.DrawLine(npc.Movement.FootPosition, ATM.AccessPoint.position, Color.red, 1f);
				SetDestination(ATM.AccessPoint.position);
			}
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
		if ((Object)(object)ATM == (Object)null)
		{
			return false;
		}
		return Vector3.Distance(npc.Movement.FootPosition, ATM.AccessPoint.position) < 2f;
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

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseATMAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseATMAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_Purchase_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseATMAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_UseATMAssembly_002DCSharp_002Edll_Excuted = true;
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
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (purchaseCoroutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(purchaseCoroutine);
		}
		npc.Movement.FaceDirection(ATM.AccessPoint.forward);
		purchaseCoroutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Purchase());
		IEnumerator Purchase()
		{
			if (ATM.IsBroken)
			{
				End();
				purchaseCoroutine = null;
			}
			else
			{
				yield return (object)new WaitForSeconds(2f);
				npc.SetAnimationTrigger_Networked(null, "GrabItem");
				yield return (object)new WaitForSeconds(1f);
				End();
				purchaseCoroutine = null;
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
