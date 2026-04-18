using System;
using System.Collections;
using FishNet;
using FishNet.Object;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Trash;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public abstract class GrowContainerBehaviour : Behaviour
{
	protected enum EState
	{
		Idle,
		Walking,
		GrabbingSupplies,
		PerformingAction
	}

	private Coroutine _walkRoutine;

	private Coroutine _grabRoutine;

	private Coroutine _performActionRoutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted;

	protected GrowContainer _growContainer { get; private set; }

	protected EState _currentState { get; private set; }

	protected Botanist _botanist { get; private set; }

	protected BotanistConfiguration _botanistConfiguration => _botanist.Configuration as BotanistConfiguration;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EGrowContainerBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public virtual void AssignAndEnable(GrowContainer growContainer)
	{
		_growContainer = growContainer;
		Enable_Networked();
	}

	public override void Activate()
	{
		base.Activate();
		_currentState = EState.Idle;
	}

	public override void Resume()
	{
		base.Resume();
		_currentState = EState.Idle;
	}

	public override void Pause()
	{
		base.Pause();
		StopAllRoutines();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		StopAllRoutines();
	}

	public virtual bool AreTaskConditionsMetForContainer(GrowContainer container)
	{
		if ((Object)(object)container == (Object)null)
		{
			return false;
		}
		if (((IUsable)container).IsInUse && !((IUsable)container).IsInUseByNPC((NPC)_botanist))
		{
			return false;
		}
		return true;
	}

	public bool DoesBotanistHaveAccessToRequiredSupplies(GrowContainer container)
	{
		if (!IsRequiredItemInInventory(container))
		{
			return DoSuppliesContainRequiredItem(container);
		}
		return true;
	}

	public override void OnActiveTick()
	{
		base.OnActiveTick();
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (base.Npc.Behaviour.DEBUG_MODE)
		{
			Console.Log("Current state: " + _currentState);
		}
		if (_currentState != EState.Idle)
		{
			return;
		}
		if ((Object)(object)_growContainer == (Object)null)
		{
			Disable_Networked(null);
		}
		else if (IsRequiredItemInInventory(_growContainer))
		{
			if (IsAtGrowContainer())
			{
				PerformAction();
			}
			else
			{
				WalkTo(_growContainer);
			}
		}
		else if (DoSuppliesContainRequiredItem(_growContainer))
		{
			if (IsAtSupplies())
			{
				GrabRequiredItemFromSupplies();
			}
			else
			{
				WalkTo(_botanist.GetSuppliesAsTransitEntity());
			}
		}
		else
		{
			Disable_Networked(null);
		}
	}

	protected virtual void OnStartPerformAction()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		_growContainer.SetNPCUser(((NetworkBehaviour)_botanist).NetworkObject);
		base.Npc.Movement.FacePoint(((Component)_growContainer).transform.position);
		string animationBool = GetAnimationBool();
		if (animationBool != string.Empty)
		{
			base.Npc.SetAnimationBool_Networked(null, animationBool, value: true);
		}
		AvatarEquippable actionEquippable = GetActionEquippable();
		if ((Object)(object)actionEquippable != (Object)null)
		{
			base.Npc.SetEquippable_Client(null, actionEquippable.AssetPath);
		}
	}

	protected virtual void OnStopPerformAction()
	{
		_growContainer.SetNPCUser(null);
		string animationBool = GetAnimationBool();
		if (animationBool != string.Empty)
		{
			base.Npc.SetAnimationBool_Networked(null, animationBool, value: false);
		}
		if ((Object)(object)GetActionEquippable() != (Object)null)
		{
			base.Npc.SetEquippable_Client(null, string.Empty);
		}
	}

	protected virtual Vector3 GetGrowContainerLookPoint()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return _growContainer.LinkOrigin.position;
	}

	protected virtual AvatarEquippable GetActionEquippable()
	{
		return null;
	}

	protected virtual TrashItem GetTrashPrefab(ItemInstance usedItem)
	{
		return null;
	}

	protected abstract void OnActionSuccess(ItemInstance usedItem);

	protected abstract string GetAnimationBool();

	protected abstract float GetActionDuration();

	private void WalkTo(ITransitEntity entity)
	{
		if (!_botanist.Movement.CanGetTo(entity))
		{
			Disable_Networked(null);
			return;
		}
		_currentState = EState.Walking;
		_walkRoutine = ((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			SetDestination(entity);
			yield return (object)new WaitForEndOfFrame();
			yield return (object)new WaitUntil((Func<bool>)(() => !base.Npc.Movement.IsMoving));
			_currentState = EState.Idle;
			_walkRoutine = null;
		}
	}

	private void GrabRequiredItemFromSupplies()
	{
		_currentState = EState.GrabbingSupplies;
		_grabRoutine = ((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			base.Npc.Movement.FacePoint(_botanist.GetSuppliesAsTransitEntity().LinkOrigin.position);
			base.Npc.Avatar.Animation.ResetTrigger("GrabItem");
			base.Npc.Avatar.Animation.SetTrigger("GrabItem");
			yield return (object)new WaitForSeconds(0.5f);
			ItemSlot suppliesSlotContainingRequiredItem = GetSuppliesSlotContainingRequiredItem(GetRequiredItemSuitableIDs(_growContainer));
			if (suppliesSlotContainingRequiredItem != null && suppliesSlotContainingRequiredItem.Quantity > 0)
			{
				base.Npc.Inventory.InsertItem(suppliesSlotContainingRequiredItem.ItemInstance.GetCopy(1));
				suppliesSlotContainingRequiredItem.ChangeQuantity(-1);
			}
			yield return (object)new WaitForSeconds(0.5f);
			_grabRoutine = null;
			_currentState = EState.Idle;
		}
	}

	private void PerformAction()
	{
		if (!AreTaskConditionsMetForContainer(_growContainer))
		{
			Disable_Networked(null);
			return;
		}
		_currentState = EState.PerformingAction;
		_performActionRoutine = ((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			OnStartPerformAction();
			float waitTime = GetActionDuration() / _botanist.CurrentWorkSpeed;
			for (float i = 0f; i < waitTime; i += Time.deltaTime)
			{
				base.Npc.Avatar.LookController.OverrideLookTarget(GetGrowContainerLookPoint(), 0);
				yield return (object)new WaitForEndOfFrame();
			}
			if (!AreTaskConditionsMetForContainer(_growContainer))
			{
				Disable_Networked(null);
			}
			else
			{
				OnStopPerformAction();
				ItemSlot itemSlot = null;
				if (DoesTaskRequireItem(_growContainer, out var suitableItemIDs))
				{
					itemSlot = GetItemSlotContainingRequiredItem(_botanist.Inventory, suitableItemIDs);
				}
				ItemInstance usedItem = itemSlot?.ItemInstance.GetCopy(1);
				if (CheckSuccess(usedItem))
				{
					OnActionSuccess(usedItem);
					if (itemSlot != null && itemSlot.Quantity > 0)
					{
						itemSlot.ChangeQuantity(-1);
					}
					TrashItem trashPrefab = GetTrashPrefab(usedItem);
					if ((Object)(object)trashPrefab != (Object)null)
					{
						NetworkSingleton<TrashManager>.Instance.CreateTrashItem(trashPrefab.ID, ((Component)base.Npc).transform.position + Vector3.up * 0.3f, Random.rotation);
					}
				}
				Disable_Networked(null);
			}
		}
	}

	protected virtual bool CheckSuccess(ItemInstance usedItem)
	{
		return true;
	}

	private void StopAllRoutines()
	{
		if (_walkRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_walkRoutine);
			_walkRoutine = null;
		}
		if (_grabRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_grabRoutine);
			_grabRoutine = null;
		}
		if (_performActionRoutine != null)
		{
			OnStopPerformAction();
			((MonoBehaviour)this).StopCoroutine(_performActionRoutine);
			_performActionRoutine = null;
		}
	}

	protected virtual string[] GetRequiredItemSuitableIDs(GrowContainer growContainer)
	{
		return null;
	}

	private bool DoesTaskRequireItem(GrowContainer growContainer, out string[] suitableItemIDs)
	{
		suitableItemIDs = GetRequiredItemSuitableIDs(growContainer);
		return suitableItemIDs != null;
	}

	private bool IsRequiredItemInInventory(GrowContainer growContainer)
	{
		if (!DoesTaskRequireItem(growContainer, out var suitableItemIDs))
		{
			return true;
		}
		for (int i = 0; i < suitableItemIDs.Length; i++)
		{
			if (((IItemSlotOwner)_botanist.Inventory).GetQuantityOfItem(suitableItemIDs[i]) > 0)
			{
				return true;
			}
		}
		return false;
	}

	private bool DoSuppliesContainRequiredItem(GrowContainer growContainer)
	{
		if (!DoesTaskRequireItem(growContainer, out var suitableItemIDs))
		{
			Debug.LogWarning((object)"DoSuppliesContainRequiredItem called but task does not require an item!");
			return true;
		}
		if ((Object)(object)_botanistConfiguration.Supplies.SelectedObject == (Object)null)
		{
			return false;
		}
		return GetItemSlotContainingRequiredItem((_botanistConfiguration.Supplies.SelectedObject as PlaceableStorageEntity).StorageEntity, suitableItemIDs) != null;
	}

	private ItemSlot GetSuppliesSlotContainingRequiredItem(string[] suitableItemIDs)
	{
		if ((Object)(object)_botanistConfiguration.Supplies.SelectedObject == (Object)null)
		{
			return null;
		}
		return GetItemSlotContainingRequiredItem((_botanistConfiguration.Supplies.SelectedObject as PlaceableStorageEntity).StorageEntity, suitableItemIDs);
	}

	protected ItemSlot GetItemSlotContainingRequiredItem(IItemSlotOwner itemSlotOwner, string[] suitableItemIDs)
	{
		for (int i = 0; i < suitableItemIDs.Length; i++)
		{
			ItemSlot firstSlotContaining = itemSlotOwner.GetFirstSlotContaining(suitableItemIDs[i]);
			if (firstSlotContaining != null)
			{
				return firstSlotContaining;
			}
		}
		return null;
	}

	private bool IsAtSupplies()
	{
		return NavMeshUtility.IsAtTransitEntity(_botanist.GetSuppliesAsTransitEntity(), base.Npc);
	}

	private bool IsAtGrowContainer()
	{
		if ((Object)(object)_growContainer == (Object)null)
		{
			return false;
		}
		return NavMeshUtility.IsAtTransitEntity(_growContainer, base.Npc);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EGrowContainerBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EGrowContainerBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		_botanist = base.Npc as Botanist;
	}
}
