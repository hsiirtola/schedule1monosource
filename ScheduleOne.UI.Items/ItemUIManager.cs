using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items;

public class ItemUIManager : Singleton<ItemUIManager>
{
	private static readonly float[] CASH_DRAG_AMOUNTS = new float[3] { 50f, 10f, 1f };

	private static readonly float[] CASH_DRAG_THRESHOLDS = new float[3] { 100f, 10f, 1f };

	[Header("References")]
	public Canvas Canvas;

	public RectTransform CashDragAmountContainer;

	public RectTransform InputsContainer;

	public ItemInfoPanel InfoPanel;

	public RectTransform ItemQuantityPrompt;

	public FilterConfigPanel FilterConfigPanel;

	[Header("Prefabs")]
	public ItemSlotUI ItemSlotUIPrefab;

	public ItemUI DefaultItemUIPrefab;

	public ItemSlotUI HotbarSlotUIPrefab;

	private ItemSlotUI draggedSlot;

	private Vector2 mouseOffset = Vector2.zero;

	private int draggedAmount;

	private RectTransform tempIcon;

	private List<GraphicRaycaster> _raycasters = new List<GraphicRaycaster>();

	private bool isDraggingCash;

	private float draggedCashAmount;

	private List<ItemSlot> PrimarySlots = new List<ItemSlot>();

	private List<ItemSlot> SecondarySlots = new List<ItemSlot>();

	private bool customDragAmount;

	private Coroutine quantityChangePopRoutine;

	public UnityEvent onDragStart;

	public UnityEvent onItemMoved;

	private bool canControllerQuickMove;

	private bool isInfoPanelToggledOn;

	public bool DraggingEnabled { get; protected set; }

	public ItemSlotUI HoveredSlot { get; protected set; }

	public bool QuickMoveEnabled { get; protected set; }

	protected override void Awake()
	{
		base.Awake();
		((Component)InputsContainer).gameObject.SetActive(false);
		((Component)ItemQuantityPrompt).gameObject.SetActive(false);
		GameInput.OnInputDeviceChanged = (Action<GameInput.InputDeviceType>)Delegate.Combine(GameInput.OnInputDeviceChanged, new Action<GameInput.InputDeviceType>(OnInputDeviceChanged));
	}

	private void OnInputDeviceChanged(GameInput.InputDeviceType type)
	{
		TryCloseInfoPanel();
		EndDrag();
		canControllerQuickMove = false;
		isInfoPanelToggledOn = false;
		HoveredSlot = null;
	}

	public void ControllerHighlightSlot(ItemSlotUI itemSlot)
	{
		HoveredSlot = itemSlot;
		if (isInfoPanelToggledOn)
		{
			TryOpenInfoPanel(HoveredSlot);
		}
	}

	public void ControllerToggleTooltip()
	{
		if (!((Object)(object)HoveredSlot == (Object)null) && !((Object)(object)draggedSlot != (Object)null) && HoveredSlot.assignedSlot != null && HoveredSlot.assignedSlot.Quantity != 0)
		{
			isInfoPanelToggledOn = !isInfoPanelToggledOn;
			if (isInfoPanelToggledOn)
			{
				TryOpenInfoPanel(HoveredSlot);
			}
			else
			{
				TryCloseInfoPanel();
			}
		}
	}

	public void ControllerGrabAllSlot()
	{
		if (!((Object)(object)HoveredSlot == (Object)null))
		{
			if ((Object)(object)draggedSlot == (Object)null)
			{
				SlotClicked(HoveredSlot);
			}
			else
			{
				EndDrag();
			}
		}
	}

	public void ControllerQuickMoveSlot()
	{
		if (!((Object)(object)HoveredSlot == (Object)null))
		{
			if ((Object)(object)draggedSlot != (Object)null)
			{
				EndDrag();
				return;
			}
			canControllerQuickMove = true;
			SlotClicked(HoveredSlot);
			TryCloseInfoPanel();
			canControllerQuickMove = false;
		}
	}

	public void ControllerDragAddQuantity()
	{
		if (!((Object)(object)draggedSlot != (Object)null) || !customDragAmount || !((Object)(object)draggedSlot != (Object)null) || !customDragAmount)
		{
			return;
		}
		if (isDraggingCash)
		{
			CashInstance cashInstance = draggedSlot.assignedSlot.ItemInstance as CashInstance;
			AddCashAmount(cashInstance, wrapAround: true);
			return;
		}
		int quantity = draggedSlot.assignedSlot.Quantity;
		if (draggedAmount + 1 > quantity)
		{
			draggedAmount = 0;
		}
		SetDraggedAmount(Mathf.Clamp(draggedAmount + 1, 1, quantity));
	}

	public void ControllerDragSubtractQuantity()
	{
		if (!((Object)(object)draggedSlot != (Object)null) || !customDragAmount || !((Object)(object)draggedSlot != (Object)null) || !customDragAmount)
		{
			return;
		}
		if (isDraggingCash)
		{
			CashInstance cashInstance = draggedSlot.assignedSlot.ItemInstance as CashInstance;
			SubtractCashAmount(cashInstance, wrapAround: true);
			return;
		}
		int quantity = draggedSlot.assignedSlot.Quantity;
		if (draggedAmount - 1 <= 0)
		{
			draggedAmount = quantity + 1;
		}
		SetDraggedAmount(Mathf.Clamp(draggedAmount - 1, 1, quantity));
	}

	public void ControllerDiscardSlot()
	{
		if (!((Object)(object)HoveredSlot == (Object)null) && !((Object)(object)draggedSlot == (Object)null))
		{
			HoveredSlot = Singleton<HUD>.Instance.discardSlot;
			EndDrag();
		}
	}

	private void TryOpenInfoPanel(ItemSlotUI itemSlot)
	{
		if (itemSlot.assignedSlot != null && itemSlot.assignedSlot.Quantity > 0)
		{
			InfoPanel.Open(itemSlot.assignedSlot.ItemInstance.Definition, itemSlot.Rect);
		}
		else
		{
			TryCloseInfoPanel();
		}
	}

	private void TryCloseInfoPanel()
	{
		if (InfoPanel.IsOpen)
		{
			InfoPanel.Close();
		}
	}

	private void UpdateControllerTooltip()
	{
		if (isInfoPanelToggledOn && (Object)(object)draggedSlot != (Object)null)
		{
			TryCloseInfoPanel();
			isInfoPanelToggledOn = false;
		}
	}

	protected virtual void Update()
	{
		if (GameInput.GetCurrentInputDeviceIsGamepad())
		{
			UpdateControllerTooltip();
			return;
		}
		HoveredSlot = null;
		if (DraggingEnabled)
		{
			CursorManager.ECursorType cursorAppearance = CursorManager.ECursorType.Default;
			HoveredSlot = GetHoveredItemSlot();
			if ((Object)(object)HoveredSlot != (Object)null && CanDragFromSlot(HoveredSlot))
			{
				cursorAppearance = CursorManager.ECursorType.OpenHand;
			}
			if ((Object)(object)HoveredSlot != (Object)null && (Object)(object)draggedSlot == (Object)null && HoveredSlot.assignedSlot != null && HoveredSlot.assignedSlot.Quantity > 0)
			{
				if (InfoPanel.CurrentItem != HoveredSlot.assignedSlot.ItemInstance)
				{
					InfoPanel.Open(HoveredSlot.assignedSlot.ItemInstance, HoveredSlot.Rect);
				}
			}
			else
			{
				ItemDefinitionInfoHoverable hoveredItemInfo = GetHoveredItemInfo();
				if ((Object)(object)hoveredItemInfo != (Object)null)
				{
					ItemInfoPanel infoPanel = InfoPanel;
					ItemDefinition assignedItem = hoveredItemInfo.AssignedItem;
					Transform transform = ((Component)hoveredItemInfo).transform;
					infoPanel.Open(assignedItem, (RectTransform)(object)((transform is RectTransform) ? transform : null));
				}
				else if (InfoPanel.IsOpen)
				{
					InfoPanel.Close();
				}
			}
			if ((Object)(object)draggedSlot != (Object)null)
			{
				cursorAppearance = CursorManager.ECursorType.Grab;
				if (!GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && !GameInput.GetButton(GameInput.ButtonCode.SecondaryClick) && !GameInput.GetButton(GameInput.ButtonCode.TertiaryClick))
				{
					EndDrag();
				}
			}
			else if ((GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) || GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick) || GameInput.GetButtonDown(GameInput.ButtonCode.TertiaryClick)) && (Object)(object)HoveredSlot != (Object)null)
			{
				SlotClicked(HoveredSlot);
			}
			Singleton<CursorManager>.Instance.SetCursorAppearance(cursorAppearance);
		}
		if ((Object)(object)draggedSlot != (Object)null && customDragAmount)
		{
			if (isDraggingCash)
			{
				CashInstance cashInstance = draggedSlot.assignedSlot.ItemInstance as CashInstance;
				UpdateCashDragAmount(cashInstance);
			}
			else if (GameInput.MouseScrollDelta > 0f)
			{
				SetDraggedAmount(Mathf.Clamp(draggedAmount + 1, 1, draggedSlot.assignedSlot.Quantity));
			}
			else if (GameInput.MouseScrollDelta < 0f)
			{
				SetDraggedAmount(Mathf.Clamp(draggedAmount - 1, 1, draggedSlot.assignedSlot.Quantity));
			}
		}
	}

	public void AddRaycaster(GraphicRaycaster raycaster)
	{
		if (!((Object)(object)raycaster == (Object)null) && !_raycasters.Contains(raycaster))
		{
			_raycasters.Add(raycaster);
		}
	}

	protected virtual void LateUpdate()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		if (DraggingEnabled && (Object)(object)draggedSlot != (Object)null)
		{
			Vector2 zero = Vector2.zero;
			Rect rect;
			if (GameInput.GetCurrentInputDeviceIsGamepad())
			{
				Vector3 position = ((Transform)HoveredSlot.Rect).position;
				rect = tempIcon.rect;
				zero = Vector2.op_Implicit(position + new Vector3(0f, ((Rect)(ref rect)).height, 0f));
			}
			else
			{
				zero = new Vector2(GameInput.MousePosition.x, GameInput.MousePosition.y) - mouseOffset;
			}
			((Transform)tempIcon).position = Vector2.op_Implicit(zero);
			if (customDragAmount)
			{
				RectTransform itemQuantityPrompt = ItemQuantityPrompt;
				Vector3 position2 = ((Transform)tempIcon).position;
				rect = tempIcon.rect;
				((Transform)itemQuantityPrompt).position = position2 + new Vector3(0f, ((Rect)(ref rect)).height * 0.5f + 25f, 0f);
			}
		}
		UpdateCashDragSelectorUI();
	}

	private void UpdateCashDragSelectorUI()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)draggedSlot != (Object)null && draggedSlot.assignedSlot != null && draggedSlot.assignedSlot.ItemInstance != null && draggedSlot.assignedSlot.ItemInstance is CashInstance && customDragAmount)
		{
			_ = draggedSlot.assignedSlot.ItemInstance;
			((TMP_Text)((Component)((Transform)tempIcon).Find("Balance")).GetComponent<TextMeshProUGUI>()).text = MoneyManager.FormatAmount(draggedCashAmount);
			RectTransform cashDragAmountContainer = CashDragAmountContainer;
			Vector3 position = ((Transform)tempIcon).position;
			Rect rect = tempIcon.rect;
			((Transform)cashDragAmountContainer).position = position + new Vector3(0f, ((Rect)(ref rect)).height * 0.5f + 15f, 0f);
			((Component)CashDragAmountContainer).gameObject.SetActive(true);
		}
		else
		{
			((Component)CashDragAmountContainer).gameObject.SetActive(false);
		}
	}

	private void UpdateCashDragAmount(CashInstance instance)
	{
		if (GameInput.MouseScrollDelta > 0f)
		{
			AddCashAmount(instance);
		}
		else if (GameInput.MouseScrollDelta < 0f)
		{
			SubtractCashAmount(instance);
		}
	}

	private void AddCashAmount(CashInstance instance, bool wrapAround = false)
	{
		float num = 0f;
		for (int i = 0; i < CASH_DRAG_AMOUNTS.Length; i++)
		{
			if (draggedCashAmount >= CASH_DRAG_THRESHOLDS[i])
			{
				num = CASH_DRAG_AMOUNTS[i];
				break;
			}
		}
		if (num != 0f)
		{
			float num2 = draggedCashAmount + num;
			float num3 = Mathf.Min(instance.Balance, 1000f);
			if (wrapAround && num2 > num3)
			{
				num2 = 1f;
			}
			draggedCashAmount = Mathf.Clamp(num2, 1f, num3);
		}
	}

	private void SubtractCashAmount(CashInstance instance, bool wrapAround = false)
	{
		float num = -1f;
		for (int i = 0; i < CASH_DRAG_AMOUNTS.Length; i++)
		{
			if (draggedCashAmount > CASH_DRAG_THRESHOLDS[i])
			{
				num = 0f - CASH_DRAG_AMOUNTS[i];
				break;
			}
		}
		float num2 = draggedCashAmount + num;
		float num3 = Mathf.Min(instance.Balance, 1000f);
		if (wrapAround && num2 <= 0f)
		{
			num2 = num3;
		}
		draggedCashAmount = Mathf.Clamp(num2, 1f, num3);
	}

	public void SetDraggingEnabled(bool enabled, bool modifierPromptsVisible = true)
	{
		DraggingEnabled = enabled;
		if (!DraggingEnabled && (Object)(object)draggedSlot != (Object)null)
		{
			EndDrag();
		}
		if (InfoPanel.IsOpen)
		{
			InfoPanel.Close();
		}
		if (!enabled)
		{
			DisableQuickMove();
			FilterConfigPanel.Close();
		}
		((Component)InputsContainer).gameObject.SetActive(DraggingEnabled && modifierPromptsVisible);
		((Component)Singleton<HUD>.Instance.discardSlot).gameObject.SetActive(DraggingEnabled);
	}

	public void EnableQuickMove(List<ItemSlot> primarySlots, List<ItemSlot> secondarySlots)
	{
		QuickMoveEnabled = true;
		PrimarySlots = new List<ItemSlot>();
		PrimarySlots.AddRange(primarySlots);
		SecondarySlots = new List<ItemSlot>();
		SecondarySlots.AddRange(secondarySlots);
		((Component)InputsContainer).gameObject.SetActive(QuickMoveEnabled);
	}

	private List<ItemSlot> GetQuickMoveSlots(ItemSlot sourceSlot)
	{
		if (sourceSlot == null || sourceSlot.ItemInstance == null)
		{
			return new List<ItemSlot>();
		}
		List<ItemSlot> obj = (PrimarySlots.Contains(sourceSlot) ? SecondarySlots : PrimarySlots);
		List<ItemSlot> list = new List<ItemSlot>();
		foreach (ItemSlot item in obj)
		{
			if (!item.IsLocked && !item.IsAddLocked && !item.IsRemovalLocked && item.DoesItemMatchHardFilters(sourceSlot.ItemInstance) && (item.GetCapacityForItem(sourceSlot.ItemInstance) > 0 || sourceSlot.ItemInstance is CashInstance))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public void DisableQuickMove()
	{
		QuickMoveEnabled = false;
	}

	private ItemSlotUI GetHoveredItemSlot()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		PointerEventData val = new PointerEventData(EventSystem.current);
		val.position = Vector2.op_Implicit(GameInput.MousePosition);
		foreach (GraphicRaycaster raycaster in _raycasters)
		{
			if ((Object)(object)raycaster == (Object)null)
			{
				continue;
			}
			List<RaycastResult> list = new List<RaycastResult>();
			((BaseRaycaster)raycaster).Raycast(val, list);
			for (int i = 0; i < list.Count; i++)
			{
				RaycastResult val2 = list[i];
				ItemSlotUI componentInParent = ((RaycastResult)(ref val2)).gameObject.GetComponentInParent<ItemSlotUI>();
				if (!((Object)(object)componentInParent != (Object)null))
				{
					continue;
				}
				if ((Object)(object)componentInParent.FilterButton != (Object)null)
				{
					val2 = list[i];
					if ((Object)(object)((RaycastResult)(ref val2)).gameObject == (Object)(object)((Component)componentInParent.FilterButton).gameObject)
					{
						return null;
					}
				}
				return componentInParent;
			}
		}
		return null;
	}

	private ItemDefinitionInfoHoverable GetHoveredItemInfo()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		PointerEventData val = new PointerEventData(EventSystem.current);
		val.position = Vector2.op_Implicit(GameInput.MousePosition);
		foreach (GraphicRaycaster raycaster in _raycasters)
		{
			if ((Object)(object)raycaster == (Object)null)
			{
				continue;
			}
			List<RaycastResult> list = new List<RaycastResult>();
			((BaseRaycaster)raycaster).Raycast(val, list);
			for (int i = 0; i < list.Count; i++)
			{
				RaycastResult val2 = list[i];
				ItemDefinitionInfoHoverable componentInParent = ((RaycastResult)(ref val2)).gameObject.GetComponentInParent<ItemDefinitionInfoHoverable>();
				if ((Object)(object)componentInParent != (Object)null && ((Behaviour)componentInParent).enabled)
				{
					return componentInParent;
				}
			}
		}
		return null;
	}

	private void SlotClicked(ItemSlotUI ui)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		if (!CanDragFromSlot(ui) || !DraggingEnabled || (Object)(object)draggedSlot != (Object)null || ui.assignedSlot.ItemInstance == null || ui.assignedSlot.IsLocked || ui.assignedSlot.IsRemovalLocked)
		{
			return;
		}
		mouseOffset = new Vector2(GameInput.MousePosition.x, GameInput.MousePosition.y) - new Vector2(((Transform)ui.ItemUI.Rect).position.x, ((Transform)ui.ItemUI.Rect).position.y);
		draggedSlot = ui;
		isDraggingCash = draggedSlot.assignedSlot.ItemInstance is CashInstance;
		if (isDraggingCash)
		{
			StartDragCash();
			return;
		}
		customDragAmount = GameInput.GetCurrentInputDeviceIsGamepad();
		draggedAmount = draggedSlot.assignedSlot.Quantity;
		if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
		{
			draggedAmount = 1;
			customDragAmount = true;
			mouseOffset += new Vector2(-10f, -15f);
		}
		if ((GameInput.GetButton(GameInput.ButtonCode.QuickMove) || canControllerQuickMove) && QuickMoveEnabled)
		{
			List<ItemSlot> quickMoveSlots = GetQuickMoveSlots(draggedSlot.assignedSlot);
			if (quickMoveSlots.Count > 0)
			{
				int num = 0;
				for (int i = 0; i < quickMoveSlots.Count; i++)
				{
					if (num >= draggedAmount)
					{
						break;
					}
					if (quickMoveSlots[i].ItemInstance != null && quickMoveSlots[i].ItemInstance.CanStackWith(draggedSlot.assignedSlot.ItemInstance, checkQuantities: false))
					{
						int num2 = Mathf.Min(quickMoveSlots[i].GetCapacityForItem(draggedSlot.assignedSlot.ItemInstance), draggedAmount - num);
						quickMoveSlots[i].AddItem(draggedSlot.assignedSlot.ItemInstance.GetCopy(num2));
						num += num2;
					}
				}
				for (int j = 0; j < quickMoveSlots.Count; j++)
				{
					if (num >= draggedAmount)
					{
						break;
					}
					int num3 = Mathf.Min(quickMoveSlots[j].GetCapacityForItem(draggedSlot.assignedSlot.ItemInstance), draggedAmount - num);
					quickMoveSlots[j].AddItem(draggedSlot.assignedSlot.ItemInstance.GetCopy(num3));
					num += num3;
				}
				draggedSlot.assignedSlot.ChangeQuantity(-num);
			}
			draggedSlot = null;
			if (onItemMoved != null)
			{
				onItemMoved.Invoke();
			}
		}
		else
		{
			if (onDragStart != null)
			{
				onDragStart.Invoke();
			}
			((Component)ItemQuantityPrompt).gameObject.SetActive(customDragAmount);
			tempIcon = draggedSlot.DuplicateIcon(((Component)Singleton<HUD>.Instance).transform, draggedAmount);
			draggedSlot.IsBeingDragged = true;
			if (draggedAmount == draggedSlot.assignedSlot.Quantity)
			{
				draggedSlot.SetVisible(shown: false);
			}
			else
			{
				draggedSlot.OverrideDisplayedQuantity(draggedSlot.assignedSlot.Quantity - draggedAmount);
			}
		}
	}

	private void StartDragCash()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		CashInstance cashInstance = draggedSlot.assignedSlot.ItemInstance as CashInstance;
		draggedCashAmount = Mathf.Min(cashInstance.Balance, 1000f);
		draggedAmount = 1;
		if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
		{
			draggedAmount = 1;
			draggedCashAmount = Mathf.Min(cashInstance.Balance, 100f);
			mouseOffset += new Vector2(-10f, -15f);
			customDragAmount = true;
		}
		if (draggedCashAmount <= 0f)
		{
			draggedSlot = null;
		}
		else if ((GameInput.GetButton(GameInput.ButtonCode.QuickMove) || canControllerQuickMove) && QuickMoveEnabled)
		{
			List<ItemSlot> quickMoveSlots = GetQuickMoveSlots(draggedSlot.assignedSlot);
			if (quickMoveSlots.Count > 0)
			{
				Debug.Log((object)("Quick-moving " + draggedAmount + " items..."));
				float num = draggedCashAmount;
				float num2 = 0f;
				for (int i = 0; i < quickMoveSlots.Count; i++)
				{
					if (num2 >= (float)draggedAmount)
					{
						break;
					}
					ItemSlot itemSlot = quickMoveSlots[i];
					if (itemSlot.ItemInstance != null)
					{
						if (itemSlot.ItemInstance is CashInstance cashInstance2)
						{
							float num3 = 0f;
							num3 = ((!(itemSlot is CashSlot)) ? Mathf.Min(num, 1000f - cashInstance2.Balance) : Mathf.Min(num, float.MaxValue - cashInstance2.Balance));
							cashInstance2.ChangeBalance(num3);
							itemSlot.ReplicateStoredInstance();
							num2 += num3;
						}
					}
					else
					{
						CashInstance cashInstance3 = Registry.GetItem("cash").GetDefaultInstance() as CashInstance;
						cashInstance3.SetBalance(draggedCashAmount);
						itemSlot.SetStoredItem(cashInstance3);
						num2 += draggedCashAmount;
					}
				}
				if (num2 >= cashInstance.Balance)
				{
					draggedSlot.assignedSlot.ClearStoredInstance();
				}
				else
				{
					cashInstance.ChangeBalance(0f - num2);
					draggedSlot.assignedSlot.ReplicateStoredInstance();
				}
			}
			if (onItemMoved != null)
			{
				onItemMoved.Invoke();
			}
			draggedSlot = null;
		}
		else
		{
			if (onDragStart != null)
			{
				onDragStart.Invoke();
			}
			if (draggedSlot.assignedSlot != PlayerSingleton<PlayerInventory>.Instance.cashSlot)
			{
				Singleton<HUD>.Instance.CashSlotHintAnim.Play();
			}
			tempIcon = draggedSlot.DuplicateIcon(((Component)Singleton<HUD>.Instance).transform, draggedAmount);
			((TMP_Text)((Component)((Transform)tempIcon).Find("Balance")).GetComponent<TextMeshProUGUI>()).text = MoneyManager.FormatAmount(draggedCashAmount);
			draggedSlot.IsBeingDragged = true;
			if (draggedCashAmount >= cashInstance.Balance)
			{
				draggedSlot.SetVisible(shown: false);
			}
			else
			{
				(draggedSlot.ItemUI as ItemUI_Cash).SetDisplayedBalance(cashInstance.Balance - draggedCashAmount);
			}
		}
	}

	private void EndDrag()
	{
		if (isDraggingCash)
		{
			EndCashDrag();
			return;
		}
		if (CanDragFromSlot(draggedSlot) && (Object)(object)HoveredSlot != (Object)null && (Object)(object)HoveredSlot != (Object)(object)draggedSlot && HoveredSlot.assignedSlot != null && !HoveredSlot.assignedSlot.IsLocked && !HoveredSlot.assignedSlot.IsAddLocked && HoveredSlot.assignedSlot.DoesItemMatchHardFilters(draggedSlot.assignedSlot.ItemInstance))
		{
			if (HoveredSlot.assignedSlot.ItemInstance == null)
			{
				HoveredSlot.assignedSlot.SetStoredItem(draggedSlot.assignedSlot.ItemInstance.GetCopy(draggedAmount));
				draggedSlot.assignedSlot.ChangeQuantity(-draggedAmount);
			}
			else if (HoveredSlot.assignedSlot.ItemInstance.CanStackWith(draggedSlot.assignedSlot.ItemInstance, checkQuantities: false))
			{
				while (HoveredSlot.assignedSlot.Quantity < ((BaseItemInstance)HoveredSlot.assignedSlot.ItemInstance).StackLimit && draggedAmount > 0)
				{
					HoveredSlot.assignedSlot.ChangeQuantity(1);
					draggedSlot.assignedSlot.ChangeQuantity(-1);
					draggedAmount--;
				}
			}
			else if (draggedSlot.assignedSlot.DoesItemMatchHardFilters(HoveredSlot.assignedSlot.ItemInstance))
			{
				if (draggedAmount == draggedSlot.assignedSlot.Quantity)
				{
					ItemInstance itemInstance = draggedSlot.assignedSlot.ItemInstance;
					ItemInstance itemInstance2 = HoveredSlot.assignedSlot.ItemInstance;
					draggedSlot.assignedSlot.SetStoredItem(itemInstance2);
					HoveredSlot.assignedSlot.SetStoredItem(itemInstance);
				}
				else if (HoveredSlot.assignedSlot.ItemInstance == null)
				{
					HoveredSlot.assignedSlot.SetStoredItem(draggedSlot.assignedSlot.ItemInstance);
					draggedSlot.assignedSlot.ClearStoredInstance();
				}
			}
			if (onItemMoved != null)
			{
				onItemMoved.Invoke();
			}
		}
		if ((Object)(object)draggedSlot != (Object)null)
		{
			draggedSlot.SetVisible(shown: true);
			draggedSlot.UpdateUI();
			draggedSlot.IsBeingDragged = false;
			draggedSlot = null;
		}
		if ((Object)(object)tempIcon != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)tempIcon).gameObject);
			tempIcon = null;
		}
		((Component)ItemQuantityPrompt).gameObject.SetActive(false);
		Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
	}

	private void SetDraggedAmount(int amount)
	{
		draggedAmount = amount;
		TextMeshProUGUI quantityText = ((Component)((Transform)tempIcon).Find("Quantity")).GetComponent<TextMeshProUGUI>();
		if ((Object)(object)quantityText != (Object)null && ((Object)((Component)quantityText).gameObject).name == "Quantity")
		{
			((TMP_Text)quantityText).text = draggedAmount + "x";
			((Behaviour)quantityText).enabled = draggedAmount > 1;
		}
		if (draggedAmount == draggedSlot.assignedSlot.Quantity)
		{
			draggedSlot.SetVisible(shown: false);
		}
		else
		{
			draggedSlot.OverrideDisplayedQuantity(draggedSlot.assignedSlot.Quantity - draggedAmount);
			draggedSlot.SetVisible(shown: true);
		}
		if ((Object)(object)quantityText != (Object)null)
		{
			if (quantityChangePopRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(quantityChangePopRoutine);
			}
			quantityChangePopRoutine = ((MonoBehaviour)this).StartCoroutine(LerpQuantityTextSize());
		}
		IEnumerator LerpQuantityTextSize()
		{
			RectTransform quantityTransform = ((TMP_Text)quantityText).rectTransform;
			while ((Object)(object)quantityTransform != (Object)null && ((Transform)quantityTransform).localScale.x < 1.35f)
			{
				float num = Mathf.MoveTowards(((Transform)quantityTransform).localScale.x, 1.35f, Time.deltaTime * 10f);
				((Transform)quantityTransform).localScale = Vector3.one * num;
				yield return (object)new WaitForEndOfFrame();
			}
			yield return (object)new WaitForSeconds(0.1f);
			while ((Object)(object)quantityTransform != (Object)null && ((Transform)quantityTransform).localScale.x > 1f)
			{
				float num2 = Mathf.MoveTowards(((Transform)quantityTransform).localScale.x, 1f, Time.deltaTime * 5f);
				((Transform)quantityTransform).localScale = Vector3.one * num2;
				yield return (object)new WaitForEndOfFrame();
			}
			quantityChangePopRoutine = null;
		}
	}

	private void EndCashDrag()
	{
		CashInstance cashInstance = null;
		if ((Object)(object)draggedSlot != (Object)null && draggedSlot.assignedSlot != null)
		{
			cashInstance = draggedSlot.assignedSlot.ItemInstance as CashInstance;
		}
		Singleton<HUD>.Instance.CashSlotHintAnim.Stop();
		Singleton<HUD>.Instance.CashSlotHintAnimCanvasGroup.alpha = 0f;
		if (CanDragFromSlot(draggedSlot) && (Object)(object)HoveredSlot != (Object)null && CanCashBeDraggedIntoSlot(HoveredSlot) && !HoveredSlot.assignedSlot.IsLocked && !HoveredSlot.assignedSlot.IsAddLocked && HoveredSlot.assignedSlot.DoesItemMatchHardFilters(draggedSlot.assignedSlot.ItemInstance))
		{
			if (HoveredSlot.assignedSlot is HotbarSlot && !(HoveredSlot.assignedSlot is CashSlot))
			{
				HoveredSlot = ((Component)Singleton<HUD>.Instance.cashSlotUI).GetComponent<CashSlotUI>();
			}
			float num = Mathf.Min(draggedCashAmount, cashInstance.Balance);
			if (num > 0f)
			{
				float num2 = num;
				if (HoveredSlot.assignedSlot.ItemInstance != null)
				{
					CashInstance cashInstance2 = HoveredSlot.assignedSlot.ItemInstance as CashInstance;
					num2 = ((!(HoveredSlot.assignedSlot is CashSlot)) ? Mathf.Min(num, 1000f - cashInstance2.Balance) : Mathf.Min(num, float.MaxValue - cashInstance2.Balance));
					cashInstance2.ChangeBalance(num2);
					HoveredSlot.assignedSlot.ReplicateStoredInstance();
				}
				else
				{
					CashInstance cashInstance3 = Registry.GetItem("cash").GetDefaultInstance() as CashInstance;
					cashInstance3.SetBalance(num2);
					HoveredSlot.assignedSlot.SetStoredItem(cashInstance3);
				}
				if (num2 >= cashInstance.Balance)
				{
					draggedSlot.assignedSlot.ClearStoredInstance();
				}
				else
				{
					cashInstance.ChangeBalance(0f - num2);
					draggedSlot.assignedSlot.ReplicateStoredInstance();
				}
			}
		}
		if ((Object)(object)draggedSlot != (Object)null)
		{
			draggedSlot.SetVisible(shown: true);
			draggedSlot.UpdateUI();
			draggedSlot.IsBeingDragged = false;
			draggedSlot = null;
		}
		Object.Destroy((Object)(object)((Component)tempIcon).gameObject);
		Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
	}

	public bool CanDragFromSlot(ItemSlotUI slotUI)
	{
		if ((Object)(object)slotUI == (Object)null)
		{
			return false;
		}
		if (slotUI.assignedSlot == null)
		{
			return false;
		}
		if (slotUI.assignedSlot.ItemInstance == null)
		{
			return false;
		}
		if (slotUI.assignedSlot.IsLocked || slotUI.assignedSlot.IsRemovalLocked)
		{
			return false;
		}
		return true;
	}

	public bool CanCashBeDraggedIntoSlot(ItemSlotUI ui)
	{
		if ((Object)(object)ui == (Object)null)
		{
			return false;
		}
		if (ui.assignedSlot == null)
		{
			return false;
		}
		if (ui.assignedSlot.ItemInstance != null && !(ui.assignedSlot.ItemInstance is CashInstance))
		{
			return false;
		}
		return true;
	}
}
