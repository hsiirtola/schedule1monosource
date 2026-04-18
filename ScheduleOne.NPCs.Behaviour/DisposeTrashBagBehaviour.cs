using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Trash;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class DisposeTrashBagBehaviour : Behaviour
{
	public string TRASH_BAG_ASSET_PATH = "Avatar/Equippables/TrashBag";

	public const float GRAB_MAX_DISTANCE = 2f;

	private TrashContent heldTrash;

	private Coroutine grabRoutine;

	private Coroutine dropRoutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDisposeTrashBagBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDisposeTrashBagBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public TrashBag TargetBag { get; private set; }

	private Cleaner Cleaner => (Cleaner)base.Npc;

	public void SetTargetBag(TrashBag bag)
	{
		TargetBag = bag;
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
		if (grabRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(grabRoutine);
			grabRoutine = null;
		}
		if (dropRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(dropRoutine);
			dropRoutine = null;
		}
		if ((Object)(object)base.Npc.Avatar.CurrentEquippable != (Object)null && base.Npc.Avatar.CurrentEquippable.AssetPath == TRASH_BAG_ASSET_PATH)
		{
			base.Npc.SetEquippable_Return(string.Empty);
		}
	}

	public override void OnActiveTick()
	{
		base.OnActiveTick();
		if (!InstanceFinder.IsServer || base.Npc.Movement.IsMoving || grabRoutine != null || dropRoutine != null)
		{
			return;
		}
		if (!AreActionConditionsMet(checkAccess: false))
		{
			Disable_Networked(null);
		}
		else if (heldTrash == null)
		{
			if (IsAtDestination())
			{
				GrabTrash();
			}
			else
			{
				GoToTarget();
			}
		}
		else if (IsAtDestination())
		{
			DropTrash();
		}
		else
		{
			GoToTarget();
		}
	}

	private void GoToTarget()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!AreActionConditionsMet(checkAccess: true))
		{
			Disable_Networked(null);
		}
		else if (heldTrash == null)
		{
			SetDestination(((Component)TargetBag).transform.position);
		}
		else
		{
			SetDestination(Cleaner.AssignedProperty.DisposalArea.StandPoint.position);
		}
	}

	[ObserversRpc(RunLocally = true)]
	private void GrabTrash()
	{
		RpcWriter___Observers_GrabTrash_2166136261();
		RpcLogic___GrabTrash_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void DropTrash()
	{
		RpcWriter___Observers_DropTrash_2166136261();
		RpcLogic___DropTrash_2166136261();
	}

	private bool IsAtDestination()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (heldTrash == null)
		{
			return Vector3.Distance(((Component)base.Npc).transform.position, ((Component)TargetBag).transform.position) <= 2f;
		}
		return Vector3.Distance(((Component)base.Npc).transform.position, Cleaner.AssignedProperty.DisposalArea.StandPoint.position) <= 2f;
	}

	private bool AreActionConditionsMet(bool checkAccess)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (heldTrash == null)
		{
			if ((Object)(object)TargetBag == (Object)null)
			{
				return false;
			}
			if (TargetBag.Draggable.IsBeingDragged)
			{
				return false;
			}
			if (checkAccess && !base.Npc.Movement.CanGetTo(((Component)TargetBag).transform.position, 2f))
			{
				return false;
			}
		}
		else if (checkAccess && !base.Npc.Movement.CanGetTo(Cleaner.AssignedProperty.DisposalArea.StandPoint.position, 2f))
		{
			return false;
		}
		return true;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDisposeTrashBagBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDisposeTrashBagBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_GrabTrash_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_DropTrash_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDisposeTrashBagBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDisposeTrashBagBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_GrabTrash_2166136261()
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

	private void RpcLogic___GrabTrash_2166136261()
	{
		if (grabRoutine == null)
		{
			grabRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Action());
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
					base.Npc.Movement.FacePoint(((Component)TargetBag).transform.position);
				}
				yield return (object)new WaitForSeconds(0.3f);
				base.Npc.SetAnimationTrigger("GrabItem");
				if (InstanceFinder.IsServer)
				{
					if (!AreActionConditionsMet(checkAccess: false))
					{
						Disable_Networked(null);
						grabRoutine = null;
						yield break;
					}
					base.Npc.SetEquippable_Client(null, TRASH_BAG_ASSET_PATH);
					heldTrash = TargetBag.Content;
					TargetBag.DestroyTrash();
				}
				yield return (object)new WaitForSeconds(0.2f);
				grabRoutine = null;
			}
		}
	}

	private void RpcReader___Observers_GrabTrash_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___GrabTrash_2166136261();
		}
	}

	private void RpcWriter___Observers_DropTrash_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___DropTrash_2166136261()
	{
		if (dropRoutine == null)
		{
			dropRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Action());
		}
		IEnumerator Action()
		{
			if (InstanceFinder.IsServer && !AreActionConditionsMet(checkAccess: false))
			{
				Disable_Networked(null);
			}
			else
			{
				base.Npc.Movement.FaceDirection(Cleaner.AssignedProperty.DisposalArea.StandPoint.forward);
				yield return (object)new WaitForSeconds(0.5f);
				if (InstanceFinder.IsServer)
				{
					Transform trashDropPoint = Cleaner.AssignedProperty.DisposalArea.TrashDropPoint;
					NetworkSingleton<TrashManager>.Instance.CreateTrashBag("trashbag", trashDropPoint.position, Random.rotation, heldTrash.GetData());
					heldTrash = null;
					base.Npc.SetEquippable_Client(null, string.Empty);
				}
				yield return (object)new WaitForSeconds(0.2f);
				dropRoutine = null;
				Disable_Networked(null);
			}
		}
	}

	private void RpcReader___Observers_DropTrash_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___DropTrash_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
