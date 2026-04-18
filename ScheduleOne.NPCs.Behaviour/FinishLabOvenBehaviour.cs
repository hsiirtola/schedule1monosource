using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Employees;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class FinishLabOvenBehaviour : Behaviour
{
	public const float HARVEST_TIME = 10f;

	private Chemist chemist;

	private Coroutine actionRoutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public LabOven targetOven { get; private set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public void SetTargetOven(LabOven oven)
	{
		targetOven = oven;
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if ((Object)(object)targetOven != (Object)null)
		{
			targetOven.Door.SetPosition(0f);
			targetOven.ClearShards();
			targetOven.RemoveTrayAnimation.Stop();
			targetOven.ResetSquareTray();
		}
		Disable();
	}

	public override void OnActiveTick()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (actionRoutine == null && InstanceFinder.IsServer && !base.Npc.Movement.IsMoving)
		{
			if (IsAtStation())
			{
				StartAction();
			}
			else
			{
				SetDestination(GetStationAccessPoint());
			}
		}
	}

	public override void BehaviourUpdate()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.BehaviourUpdate();
		if (actionRoutine != null)
		{
			base.Npc.Avatar.LookController.OverrideLookTarget(targetOven.UIPoint.position, 5);
		}
	}

	[ObserversRpc(RunLocally = true)]
	private void StartAction()
	{
		RpcWriter___Observers_StartAction_2166136261();
		RpcLogic___StartAction_2166136261();
	}

	private bool CanActionStart()
	{
		if ((Object)(object)targetOven == (Object)null)
		{
			return false;
		}
		if (((IUsable)targetOven).IsInUse && (Object)(object)((IUsable)targetOven).NPCUserObject != (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject)
		{
			return false;
		}
		if (targetOven.CurrentOperation == null)
		{
			return false;
		}
		if (!targetOven.CurrentOperation.IsReady())
		{
			return false;
		}
		if (!targetOven.CanOutputSpaceFitCurrentOperation())
		{
			return false;
		}
		return true;
	}

	private void StopAction()
	{
		targetOven.SetNPCUser(null);
		base.Npc.SetEquippable_Client(null, string.Empty);
		base.Npc.SetAnimationBool_Networked(null, "UseHammer", value: false);
		if (actionRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(actionRoutine);
			actionRoutine = null;
		}
	}

	private Vector3 GetStationAccessPoint()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetOven == (Object)null)
		{
			return ((Component)base.Npc).transform.position;
		}
		return ((ITransitEntity)targetOven).AccessPoints[0].position;
	}

	private bool IsAtStation()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetOven == (Object)null)
		{
			return false;
		}
		return Vector3.Distance(((Component)base.Npc).transform.position, GetStationAccessPoint()) < 1f;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_StartAction_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_StartAction_2166136261()
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

	private void RpcLogic___StartAction_2166136261()
	{
		if (actionRoutine == null && !((Object)(object)targetOven == (Object)null))
		{
			actionRoutine = ((MonoBehaviour)this).StartCoroutine(ActionRoutine());
		}
		IEnumerator ActionRoutine()
		{
			targetOven.SetNPCUser(((NetworkBehaviour)base.Npc).NetworkObject);
			base.Npc.Movement.FacePoint(((Component)targetOven).transform.position);
			yield return (object)new WaitForSeconds(0.5f);
			if (!CanActionStart())
			{
				StopAction();
				Deactivate_Networked(null);
			}
			else
			{
				base.Npc.SetEquippable_Client(null, "Avatar/Equippables/Hammer");
				targetOven.Door.SetPosition(1f);
				targetOven.WireTray.SetPosition(1f);
				yield return (object)new WaitForSeconds(0.5f);
				targetOven.SquareTray.SetParent(((Component)targetOven).transform);
				targetOven.RemoveTrayAnimation.Play();
				yield return (object)new WaitForSeconds(0.1f);
				targetOven.Door.SetPosition(0f);
				yield return (object)new WaitForSeconds(1f);
				base.Npc.SetAnimationBool_Networked(null, "UseHammer", value: true);
				float num = 10f / (base.Npc as Employee).CurrentWorkSpeed;
				yield return (object)new WaitForSeconds(num);
				base.Npc.SetAnimationBool_Networked(null, "UseHammer", value: false);
				targetOven.Shatter(targetOven.CurrentOperation.Cookable.ProductQuantity, ((Component)targetOven.CurrentOperation.Cookable.ProductShardPrefab).gameObject);
				yield return (object)new WaitForSeconds(1f);
				ItemInstance productItem = targetOven.CurrentOperation.GetProductItem(targetOven.CurrentOperation.Cookable.ProductQuantity * targetOven.CurrentOperation.IngredientQuantity);
				targetOven.OutputSlot.AddItem(productItem);
				targetOven.SendCookOperation(null);
				StopAction();
				Deactivate_Networked(null);
			}
		}
	}

	private void RpcReader___Observers_StartAction_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartAction_2166136261();
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EFinishLabOvenBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		chemist = base.Npc as Chemist;
	}
}
