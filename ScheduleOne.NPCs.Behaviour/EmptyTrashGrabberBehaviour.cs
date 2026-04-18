using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Trash;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Behaviour;

public class EmptyTrashGrabberBehaviour : Behaviour
{
	public const float ACTION_MAX_DISTANCE = 2f;

	public const string EQUIPPABLE_ASSET_PATH = "Tools/TrashGrabber/Bin_AvatarEquippable";

	private Coroutine actionCoroutine;

	public UnityEvent onPerfomAction;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EEmptyTrashGrabberBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EEmptyTrashGrabberBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public TrashContainerItem TargetTrashCan { get; private set; }

	private Cleaner Cleaner => (Cleaner)base.Npc;

	public void SetTargetTrashCan(TrashContainerItem trashCan)
	{
		TargetTrashCan = trashCan;
	}

	public override void Activate()
	{
		base.Activate();
		StartAction();
	}

	public override void Resume()
	{
		base.Resume();
		StartAction();
	}

	private void StartAction()
	{
		if ((Object)(object)base.Npc.Avatar.CurrentEquippable == (Object)null || base.Npc.Avatar.CurrentEquippable.AssetPath != "Tools/TrashGrabber/Bin_AvatarEquippable")
		{
			base.Npc.SetEquippable_Return("Tools/TrashGrabber/Bin_AvatarEquippable");
		}
	}

	public override void Pause()
	{
		base.Pause();
		StopAllActions();
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
		StopAllActions();
	}

	private void StopAllActions()
	{
		if (base.Npc.Movement.IsMoving)
		{
			base.Npc.Movement.Stop();
		}
		if (actionCoroutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(actionCoroutine);
			actionCoroutine = null;
		}
	}

	public override void OnActiveTick()
	{
		base.OnActiveTick();
		if (InstanceFinder.IsServer && !base.Npc.Movement.IsMoving && actionCoroutine == null)
		{
			if (!AreActionConditionsMet(checkAccess: false))
			{
				Disable_Networked(null);
			}
			else if (IsAtDestination())
			{
				PerformAction();
			}
			else
			{
				GoToTarget();
			}
		}
	}

	private void GoToTarget()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!AreActionConditionsMet(checkAccess: true))
		{
			Disable_Networked(null);
			return;
		}
		Transform reachableAccessPoint = NavMeshUtility.GetReachableAccessPoint(TargetTrashCan, base.Npc);
		if ((Object)(object)reachableAccessPoint == (Object)null)
		{
			Disable_Networked(null);
		}
		else
		{
			SetDestination(reachableAccessPoint.position);
		}
	}

	[ObserversRpc(RunLocally = true)]
	private void PerformAction()
	{
		RpcWriter___Observers_PerformAction_2166136261();
		RpcLogic___PerformAction_2166136261();
	}

	private bool IsAtDestination()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(((Component)base.Npc).transform.position, ((Component)TargetTrashCan).transform.position) <= 2f;
	}

	private bool AreActionConditionsMet(bool checkAccess)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TargetTrashCan == (Object)null)
		{
			return false;
		}
		if (TargetTrashCan.Container.NormalizedTrashLevel >= 1f)
		{
			return false;
		}
		if (Cleaner.trashGrabberInstance.GetTotalSize() == 0)
		{
			return false;
		}
		if (checkAccess && !base.Npc.Movement.CanGetTo(((Component)TargetTrashCan).transform.position, 2f))
		{
			return false;
		}
		return true;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EEmptyTrashGrabberBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EEmptyTrashGrabberBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_PerformAction_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EEmptyTrashGrabberBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EEmptyTrashGrabberBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_PerformAction_2166136261()
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

	private void RpcLogic___PerformAction_2166136261()
	{
		if (actionCoroutine == null)
		{
			actionCoroutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Action());
		}
		IEnumerator Action()
		{
			if (InstanceFinder.IsServer && !AreActionConditionsMet(checkAccess: false))
			{
				Disable_Networked(null);
			}
			else
			{
				if (InstanceFinder.IsServer)
				{
					base.Npc.Movement.FacePoint(((Component)TargetTrashCan).transform.position);
				}
				yield return (object)new WaitForSeconds(0.3f);
				if (onPerfomAction != null)
				{
					onPerfomAction.Invoke();
				}
				yield return (object)new WaitForSeconds(0.4f);
				if (InstanceFinder.IsServer)
				{
					while (AreActionConditionsMet(checkAccess: false))
					{
						List<string> trashIDs = Cleaner.trashGrabberInstance.GetTrashIDs();
						string id = trashIDs[trashIDs.Count - 1];
						Cleaner.trashGrabberInstance.RemoveTrash(id, 1);
						NetworkSingleton<TrashManager>.Instance.CreateTrashItem(id, ((Component)TargetTrashCan).transform.position + Vector3.up * 1.5f, Random.rotation);
						yield return (object)new WaitForSeconds(0.5f);
					}
				}
				yield return (object)new WaitForSeconds(0.2f);
				actionCoroutine = null;
				Disable_Networked(null);
			}
		}
	}

	private void RpcReader___Observers_PerformAction_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___PerformAction_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
