using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class BrickPressBehaviour : Behaviour
{
	public const float BASE_PACKAGING_TIME = 15f;

	private Coroutine packagingRoutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBrickPressBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBrickPressBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public BrickPress Press { get; protected set; }

	public bool PackagingInProgress { get; protected set; }

	public override void Activate()
	{
		base.Activate();
		StartPackaging();
	}

	public override void Resume()
	{
		base.Resume();
		StartPackaging();
	}

	public override void Pause()
	{
		base.Pause();
		if (PackagingInProgress)
		{
			StopPackaging();
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
		if (PackagingInProgress)
		{
			StopPackaging();
		}
		if (InstanceFinder.IsServer && (Object)(object)Press != (Object)null && (Object)(object)Press.NPCUserObject == (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject)
		{
			Press.SetNPCUser(null);
		}
	}

	public override void OnActiveTick()
	{
		base.OnActiveTick();
		if (!InstanceFinder.IsServer || PackagingInProgress)
		{
			return;
		}
		if (IsStationReady(Press))
		{
			if (!base.Npc.Movement.IsMoving)
			{
				if (IsAtStation())
				{
					BeginPackaging();
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

	private void StartPackaging()
	{
		if (InstanceFinder.IsServer)
		{
			if (!IsStationReady(Press))
			{
				Console.LogWarning(base.Npc.fullName + " has no station to work with");
				Disable_Networked(null);
			}
			else
			{
				Press.SetNPCUser(((NetworkBehaviour)base.Npc).NetworkObject);
			}
		}
	}

	public void AssignStation(BrickPress press)
	{
		Press = press;
	}

	public bool IsAtStation()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return base.Npc.Movement.IsAsCloseAsPossible(Press.StandPoint.position);
	}

	public void GoToStation()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		SetDestination(Press.StandPoint.position);
	}

	[ObserversRpc(RunLocally = true)]
	public void BeginPackaging()
	{
		RpcWriter___Observers_BeginPackaging_2166136261();
		RpcLogic___BeginPackaging_2166136261();
	}

	private void StopPackaging()
	{
		if (packagingRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(packagingRoutine);
		}
		PackagingInProgress = false;
	}

	public bool IsStationReady(BrickPress press)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)press == (Object)null)
		{
			return false;
		}
		if (press.GetState() != PackagingStation.EState.CanBegin)
		{
			return false;
		}
		if (((IUsable)press).IsInUse && (Object)(object)press.NPCUserObject != (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject)
		{
			return false;
		}
		if (!base.Npc.Movement.CanGetTo(press.StandPoint.position))
		{
			return false;
		}
		return true;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBrickPressBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBrickPressBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_BeginPackaging_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBrickPressBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBrickPressBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_BeginPackaging_2166136261()
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

	public void RpcLogic___BeginPackaging_2166136261()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!PackagingInProgress && !((Object)(object)Press == (Object)null))
		{
			PackagingInProgress = true;
			base.Npc.Movement.FaceDirection(Press.StandPoint.forward);
			packagingRoutine = ((MonoBehaviour)this).StartCoroutine(Package());
		}
		IEnumerator Package()
		{
			yield return (object)new WaitForEndOfFrame();
			base.Npc.Avatar.Animation.SetBool("UsePackagingStation", value: true);
			float packageTime = 15f / (base.Npc as Packager).PackagingSpeedMultiplier;
			packageTime /= (base.Npc as Employee).CurrentWorkSpeed;
			for (float i = 0f; i < packageTime; i += Time.deltaTime)
			{
				base.Npc.Avatar.LookController.OverrideLookTarget(Press.uiPoint.position, 0);
				yield return (object)new WaitForEndOfFrame();
			}
			base.Npc.Avatar.Animation.SetBool("UsePackagingStation", value: false);
			yield return (object)new WaitForSeconds(0.2f);
			base.Npc.Avatar.Animation.SetTrigger("GrabItem");
			Press.PlayPressAnim();
			yield return (object)new WaitForSeconds(1f);
			if (InstanceFinder.IsServer && Press.HasSufficientProduct(out var product))
			{
				Press.CompletePress(product);
			}
			PackagingInProgress = false;
			packagingRoutine = null;
		}
	}

	private void RpcReader___Observers_BeginPackaging_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___BeginPackaging_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
