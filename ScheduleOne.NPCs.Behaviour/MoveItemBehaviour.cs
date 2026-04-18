using System;
using System.Collections;
using FishNet;
using FishNet.Object;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class MoveItemBehaviour : Behaviour
{
	public enum EState
	{
		Idle,
		WalkingToSource,
		Grabbing,
		WalkingToDestination,
		Placing
	}

	private TransitRoute assignedRoute;

	private ItemInstance itemToRetrieveTemplate;

	private int grabbedAmount;

	private int maxMoveAmount = -1;

	private EState currentState;

	private Coroutine walkToSourceRoutine;

	private Coroutine grabRoutine;

	private Coroutine walkToDestinationRoutine;

	private Coroutine placingRoutine;

	private bool skipPickup;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EMoveItemBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EMoveItemBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public bool Initialized { get; protected set; }

	public void Initialize(TransitRoute route, ItemInstance _itemToRetrieveTemplate, int _maxMoveAmount = -1, bool _skipPickup = false)
	{
		if (!IsTransitRouteValid(route, _itemToRetrieveTemplate, out var invalidReason))
		{
			Debug.LogError((object)("Invalid transit route for move item behaviour! Reason: " + invalidReason), (Object)(object)((Component)this).gameObject);
			return;
		}
		assignedRoute = route;
		itemToRetrieveTemplate = _itemToRetrieveTemplate;
		maxMoveAmount = _maxMoveAmount;
		if (base.Npc.Behaviour.DEBUG_MODE)
		{
			Console.Log("MoveItemBehaviour initialized with route: " + route.Source.Name + " -> " + route.Destination.Name + " for item: " + ((BaseItemInstance)_itemToRetrieveTemplate).ID);
		}
		skipPickup = _skipPickup;
	}

	public void Resume(TransitRoute route, ItemInstance _itemToRetrieveTemplate, int _maxMoveAmount = -1)
	{
		assignedRoute = route;
		itemToRetrieveTemplate = _itemToRetrieveTemplate;
		maxMoveAmount = _maxMoveAmount;
	}

	public override void Activate()
	{
		base.Activate();
		StartTransit();
	}

	public override void Pause()
	{
		base.Pause();
		StopCurrentActivity();
	}

	public override void Resume()
	{
		base.Resume();
		StartTransit();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		skipPickup = false;
		EndTransit();
	}

	public override void Disable()
	{
		base.Disable();
		if (base.Active)
		{
			Deactivate();
		}
	}

	private void StartTransit()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (base.Npc.Inventory.GetIdenticalItemAmount(itemToRetrieveTemplate) == 0)
		{
			if (!IsTransitRouteValid(assignedRoute, itemToRetrieveTemplate, out var _))
			{
				Console.LogWarning("Invalid transit route for move item behaviour!");
				Disable_Networked(null);
				return;
			}
		}
		else
		{
			ItemInstance firstIdenticalItem = base.Npc.Inventory.GetFirstIdenticalItem(itemToRetrieveTemplate, IsNpcInventoryItemValid);
			if (base.Npc.Behaviour.DEBUG_MODE)
			{
				Console.Log("Moving item: " + (object)firstIdenticalItem);
			}
			if (!IsDestinationValid(assignedRoute, firstIdenticalItem))
			{
				Console.LogWarning("Invalid transit route for move item behaviour!");
				Disable_Networked(null);
				return;
			}
		}
		currentState = EState.Idle;
	}

	private bool IsNpcInventoryItemValid(ItemInstance item)
	{
		if (assignedRoute.Destination.GetInputCapacityForItem(item, base.Npc) == 0)
		{
			return false;
		}
		return true;
	}

	private void EndTransit()
	{
		StopCurrentActivity();
		if (assignedRoute != null && (Object)(object)base.Npc != (Object)null && assignedRoute.Destination != null)
		{
			assignedRoute.Destination.RemoveSlotLocks(((NetworkBehaviour)base.Npc).NetworkObject);
		}
		Initialized = false;
		assignedRoute = null;
		itemToRetrieveTemplate = null;
		grabbedAmount = 0;
	}

	public override void OnActiveTick()
	{
		base.OnActiveTick();
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (!assignedRoute.AreEntitiesNonNull())
		{
			Console.LogWarning("Transit route entities are null!");
			Disable_Networked(null);
			return;
		}
		if (base.beh.DEBUG_MODE)
		{
			Console.Log("State: " + currentState);
			Console.Log("Moving: " + base.Npc.Movement.IsMoving);
		}
		if (currentState != EState.Idle)
		{
			return;
		}
		if (base.Npc.Inventory.GetIdenticalItemAmount(itemToRetrieveTemplate) > 0 && grabbedAmount > 0)
		{
			if (IsAtDestination())
			{
				PlaceItem();
			}
			else
			{
				WalkToDestination();
			}
		}
		else if (skipPickup)
		{
			TakeItem();
			skipPickup = false;
		}
		else if (IsAtSource())
		{
			GrabItem();
		}
		else
		{
			WalkToSource();
		}
	}

	public void WalkToSource()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (base.beh.DEBUG_MODE)
		{
			Console.Log("MoveItemBehaviour.WalkToSource");
		}
		if (!base.Npc.Movement.CanGetTo(GetSourceAccessPoint(assignedRoute).position))
		{
			Console.LogWarning("MoveItemBehaviour.WalkToSource: Can't get to source");
			Disable_Networked(null);
		}
		else
		{
			currentState = EState.WalkingToSource;
			walkToSourceRoutine = ((MonoBehaviour)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			SetDestination(GetSourceAccessPoint(assignedRoute).position);
			yield return (object)new WaitForSecondsRealtime(0.5f);
			yield return (object)new WaitUntil((Func<bool>)(() => !base.Npc.Movement.IsMoving));
			currentState = EState.Idle;
			walkToSourceRoutine = null;
		}
	}

	public void GrabItem()
	{
		if (base.beh.DEBUG_MODE)
		{
			Console.Log("MoveItemBehaviour.GrabItem");
		}
		currentState = EState.Grabbing;
		grabRoutine = ((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			Transform sourceAccessPoint = GetSourceAccessPoint(assignedRoute);
			if ((Object)(object)sourceAccessPoint == (Object)null)
			{
				Console.LogWarning("Could not find source access point!");
				grabRoutine = null;
				Disable_Networked(null);
			}
			else
			{
				base.Npc.Movement.FaceDirection(sourceAccessPoint.forward);
				base.Npc.SetAnimationTrigger_Networked(null, "GrabItem");
				float num = 0.5f / (base.Npc as Employee).CurrentWorkSpeed;
				yield return (object)new WaitForSeconds(num);
				if (!IsTransitRouteValid(assignedRoute, itemToRetrieveTemplate, out var invalidReason))
				{
					Console.LogWarning(base.Npc.fullName + " transit route no longer valid! Reason: " + invalidReason);
					grabRoutine = null;
					Disable_Networked(null);
				}
				else
				{
					TakeItem();
					yield return (object)new WaitForSeconds(0.5f);
					grabRoutine = null;
					currentState = EState.Idle;
				}
			}
		}
	}

	private void TakeItem()
	{
		if (base.beh.DEBUG_MODE)
		{
			Console.Log("MoveItemBehaviour.TakeItem");
		}
		int amountToGrab = GetAmountToGrab();
		if (amountToGrab == 0)
		{
			Console.LogWarning("Amount to grab is 0!");
			return;
		}
		ItemSlot firstSlotContainingTemplateItem = assignedRoute.Source.GetFirstSlotContainingTemplateItem(itemToRetrieveTemplate, ITransitEntity.ESlotType.Output);
		ItemInstance copy = (firstSlotContainingTemplateItem?.ItemInstance).GetCopy(amountToGrab);
		grabbedAmount = amountToGrab;
		firstSlotContainingTemplateItem.ChangeQuantity(-amountToGrab);
		base.Npc.Inventory.InsertItem(copy);
		assignedRoute.Destination.ReserveInputSlotsForItem(copy, ((NetworkBehaviour)base.Npc).NetworkObject);
	}

	public void WalkToDestination()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (base.beh.DEBUG_MODE)
		{
			Console.Log("MoveItemBehaviour.WalkToDestination");
		}
		if (assignedRoute == null)
		{
			Debug.LogWarning((object)"Assigned route is null!", (Object)(object)((Component)this).gameObject);
			Disable_Networked(null);
		}
		else if (!base.Npc.Movement.CanGetTo(GetDestinationAccessPoint(assignedRoute).position))
		{
			Console.LogWarning("MoveItemBehaviour.WalkToDestination: Can't get to destination");
			Disable_Networked(null);
		}
		else
		{
			currentState = EState.WalkingToDestination;
			walkToDestinationRoutine = ((MonoBehaviour)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			SetDestination(GetDestinationAccessPoint(assignedRoute).position);
			yield return (object)new WaitForSecondsRealtime(0.5f);
			yield return (object)new WaitUntil((Func<bool>)(() => !base.Npc.Movement.IsMoving));
			currentState = EState.Idle;
			walkToDestinationRoutine = null;
		}
	}

	public void PlaceItem()
	{
		if (base.beh.DEBUG_MODE)
		{
			Console.Log("MoveItemBehaviour.PlaceItem");
		}
		currentState = EState.Placing;
		placingRoutine = ((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if ((Object)(object)GetDestinationAccessPoint(assignedRoute) != (Object)null)
			{
				base.Npc.Movement.FaceDirection(GetDestinationAccessPoint(assignedRoute).forward);
			}
			base.Npc.SetAnimationTrigger_Networked(null, "GrabItem");
			float num = 0.5f / (base.Npc as Employee).CurrentWorkSpeed;
			yield return (object)new WaitForSeconds(num);
			assignedRoute.Destination.RemoveSlotLocks(((NetworkBehaviour)base.Npc).NetworkObject);
			ItemInstance firstIdenticalItem = base.Npc.Inventory.GetFirstIdenticalItem(itemToRetrieveTemplate);
			if (firstIdenticalItem != null && grabbedAmount > 0)
			{
				ItemInstance copy = firstIdenticalItem.GetCopy(grabbedAmount);
				if (assignedRoute.Destination.GetInputCapacityForItem(copy, base.Npc) >= grabbedAmount)
				{
					assignedRoute.Destination.InsertItemIntoInput(copy, base.Npc);
				}
				else
				{
					Console.LogWarning("Destination does not have enough capacity for item! Attempting to return item to source.");
					if (assignedRoute.Source.GetOutputCapacityForItem(copy, base.Npc) >= grabbedAmount)
					{
						assignedRoute.Source.InsertItemIntoOutput(copy, base.Npc);
					}
					else
					{
						Console.LogWarning("Source does not have enough capacity for item! Item will be lost.");
					}
				}
				((BaseItemInstance)firstIdenticalItem).ChangeQuantity(-grabbedAmount);
			}
			else
			{
				Console.LogWarning("Could not find carried item to place!");
			}
			yield return (object)new WaitForSeconds(0.5f);
			placingRoutine = null;
			currentState = EState.Idle;
			Disable_Networked(null);
		}
	}

	private int GetAmountToGrab()
	{
		ItemInstance itemInstance = assignedRoute.Source.GetFirstSlotContainingTemplateItem(itemToRetrieveTemplate, ITransitEntity.ESlotType.Output)?.ItemInstance;
		if (itemInstance == null)
		{
			return 0;
		}
		int num = ((BaseItemInstance)itemInstance).Quantity;
		if (maxMoveAmount > 0)
		{
			num = Mathf.Min(maxMoveAmount, num);
		}
		int inputCapacityForItem = assignedRoute.Destination.GetInputCapacityForItem(itemInstance, base.Npc);
		return Mathf.Min(num, inputCapacityForItem);
	}

	private void StopCurrentActivity()
	{
		switch (currentState)
		{
		case EState.WalkingToSource:
			if (walkToSourceRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(walkToSourceRoutine);
			}
			break;
		case EState.Grabbing:
			if (grabRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(grabRoutine);
			}
			break;
		case EState.WalkingToDestination:
			if (walkToDestinationRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(walkToDestinationRoutine);
			}
			break;
		case EState.Placing:
			if (placingRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(placingRoutine);
			}
			break;
		}
		currentState = EState.Idle;
	}

	public bool IsTransitRouteValid(TransitRoute route, string itemID, out string invalidReason)
	{
		invalidReason = string.Empty;
		if (route == null)
		{
			invalidReason = "Route is null!";
			return false;
		}
		if (!route.AreEntitiesNonNull())
		{
			invalidReason = "Entities are null!";
			return false;
		}
		ItemInstance itemInstance = route.Source.GetFirstSlotContainingItem(itemID, ITransitEntity.ESlotType.Output)?.ItemInstance;
		if (itemInstance == null || ((BaseItemInstance)itemInstance).Quantity <= 0)
		{
			invalidReason = "Item is null or quantity is 0!";
			return false;
		}
		if (!IsDestinationValid(route, itemInstance, out invalidReason))
		{
			return false;
		}
		if (base.Npc.Inventory.GetCapacityForItem(itemInstance) == 0)
		{
			invalidReason = "Npc inventory doesn't have capacity!";
			return false;
		}
		return true;
	}

	public bool IsTransitRouteValid(TransitRoute route, ItemInstance templateItem, out string invalidReason)
	{
		invalidReason = string.Empty;
		if (route == null)
		{
			invalidReason = "Route is null!";
			return false;
		}
		if (!route.AreEntitiesNonNull())
		{
			invalidReason = "Entities are null!";
			return false;
		}
		ItemInstance itemInstance = route.Source.GetFirstSlotContainingTemplateItem(templateItem, ITransitEntity.ESlotType.Output)?.ItemInstance;
		if (itemInstance == null || ((BaseItemInstance)itemInstance).Quantity <= 0)
		{
			invalidReason = "Item is null or quantity is 0!";
			return false;
		}
		if (!IsDestinationValid(route, itemInstance, out invalidReason))
		{
			return false;
		}
		return true;
	}

	public bool IsTransitRouteValid(TransitRoute route, string itemID)
	{
		string invalidReason;
		return IsTransitRouteValid(route, itemID, out invalidReason);
	}

	public bool IsDestinationValid(TransitRoute route, ItemInstance item)
	{
		string invalidReason;
		return IsDestinationValid(route, item, out invalidReason);
	}

	public bool IsDestinationValid(TransitRoute route, ItemInstance item, out string invalidReason)
	{
		invalidReason = string.Empty;
		if (route.Destination.GetInputCapacityForItem(item, base.Npc) == 0)
		{
			invalidReason = "Destination has no capacity for item!";
			return false;
		}
		if (!CanGetToDestination(route))
		{
			invalidReason = "Cannot get to destination!";
			return false;
		}
		if (!CanGetToSource(route))
		{
			invalidReason = "Cannot get to source!";
			return false;
		}
		return true;
	}

	public bool CanGetToSource(TransitRoute route)
	{
		return (Object)(object)GetSourceAccessPoint(route) != (Object)null;
	}

	private Transform GetSourceAccessPoint(TransitRoute route)
	{
		if (route == null)
		{
			Debug.LogError((object)"GetSourceAccessPoint: Route is null!");
			return null;
		}
		return NavMeshUtility.GetReachableAccessPoint(route.Source, base.Npc);
	}

	private bool IsAtSource()
	{
		return NavMeshUtility.IsAtTransitEntity(assignedRoute.Source, base.Npc);
	}

	public bool CanGetToDestination(TransitRoute route)
	{
		return (Object)(object)GetDestinationAccessPoint(route) != (Object)null;
	}

	private Transform GetDestinationAccessPoint(TransitRoute route)
	{
		if (route == null)
		{
			Debug.LogError((object)"GetDestinationAccessPoint: Route is null!");
			return null;
		}
		if (route.Destination == null)
		{
			Console.LogWarning("Destination is null!");
			return null;
		}
		return NavMeshUtility.GetReachableAccessPoint(route.Destination, base.Npc);
	}

	private bool IsAtDestination()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (base.beh.DEBUG_MODE)
		{
			Transform[] accessPoints = assignedRoute.Destination.AccessPoints;
			foreach (Transform val in accessPoints)
			{
				Debug.DrawLine(((Component)base.Npc).transform.position, val.position, Color.red, 0.1f);
			}
		}
		return NavMeshUtility.IsAtTransitEntity(assignedRoute.Destination, base.Npc);
	}

	public MoveItemData GetSaveData()
	{
		if (!base.Active || grabbedAmount == 0)
		{
			return null;
		}
		string templateItemJson = string.Empty;
		if (itemToRetrieveTemplate != null)
		{
			templateItemJson = itemToRetrieveTemplate.GetItemData().GetJson(prettyPrint: false);
		}
		return new MoveItemData(templateItemJson, grabbedAmount, (assignedRoute.Source as IGUIDRegisterable).GUID, (assignedRoute.Destination as IGUIDRegisterable).GUID);
	}

	public void Load(MoveItemData moveItemData)
	{
		if (moveItemData == null || moveItemData.GrabbedItemQuantity == 0 || string.IsNullOrEmpty(moveItemData.TemplateItemJSON))
		{
			return;
		}
		ITransitEntity transitEntity = GUIDManager.GetObject<ITransitEntity>(new Guid(moveItemData.SourceGUID));
		ITransitEntity transitEntity2 = GUIDManager.GetObject<ITransitEntity>(new Guid(moveItemData.DestinationGUID));
		if (transitEntity == null)
		{
			Console.LogWarning("Failed to load source transit entity");
			return;
		}
		if (transitEntity2 == null)
		{
			Console.LogWarning("Failed to load destination transit entity");
			return;
		}
		TransitRoute route = new TransitRoute(transitEntity, transitEntity2);
		grabbedAmount = moveItemData.GrabbedItemQuantity;
		Debug.Log((object)"Resuming move item behaviour");
		ItemInstance itemInstance = ItemDeserializer.LoadItem(moveItemData.TemplateItemJSON);
		if (itemInstance != null)
		{
			Resume(route, itemInstance);
			Enable_Networked();
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EMoveItemBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EMoveItemBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EMoveItemBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EMoveItemBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
