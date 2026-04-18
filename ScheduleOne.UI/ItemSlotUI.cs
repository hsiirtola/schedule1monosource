using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Items;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class ItemSlotUI : MonoBehaviour
{
	public Color32 normalColor = new Color32((byte)140, (byte)140, (byte)140, (byte)40);

	public Color32 highlightColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)60);

	[HideInInspector]
	public bool IsBeingDragged;

	[Header("Settings")]
	[SerializeField]
	private bool _playBopAnimation = true;

	[Header("References")]
	public RectTransform Rect;

	public Image Background;

	public GameObject LockContainer;

	public RectTransform ItemContainer;

	public ItemSlotFilterButton FilterButton;

	public Animation BopAnimation;

	[Header("Controller Support")]
	public UITrigger CmdQuickMove;

	public UITrigger CmdGrabAll;

	public UITrigger CmdQtyAdd;

	public UITrigger CmdQtySubtract;

	public UITrigger CmdToggleTooltip;

	public UITrigger CmdDiscardItem;

	private int _lastQuantity;

	private bool _slotBopQueued;

	public ItemSlot assignedSlot { get; protected set; }

	public ItemUI ItemUI { get; protected set; }

	private void Awake()
	{
		AssignControllerCommands();
	}

	public virtual void AssignSlot(ItemSlot s)
	{
		if (s == null)
		{
			Console.LogWarning("AssignSlot passed null slot. Use ClearSlot() instead");
		}
		assignedSlot = s;
		ItemSlot itemSlot = assignedSlot;
		itemSlot.onItemDataChanged = (Action)Delegate.Combine(itemSlot.onItemDataChanged, new Action(OnItemSlotDataChanged));
		ItemSlot itemSlot2 = assignedSlot;
		itemSlot2.onLocked = (Action)Delegate.Combine(itemSlot2.onLocked, new Action(Lock));
		ItemSlot itemSlot3 = assignedSlot;
		itemSlot3.onUnlocked = (Action)Delegate.Combine(itemSlot3.onUnlocked, new Action(Unlock));
		SetHighlighted(h: false);
		if (assignedSlot is HotbarSlot)
		{
			HotbarSlot obj = assignedSlot as HotbarSlot;
			obj.onEquipChanged = (HotbarSlot.EquipEvent)Delegate.Combine(obj.onEquipChanged, new HotbarSlot.EquipEvent(SetHighlighted));
		}
		if (s.IsLocked)
		{
			SetLockVisible(vis: true);
		}
		if ((Object)(object)FilterButton != (Object)null && s.CanPlayerSetFilter)
		{
			FilterButton.AssignSlot(s);
			((Selectable)FilterButton.Button).interactable = !s.IsLocked;
		}
		UpdateUI();
	}

	public virtual void ClearSlot()
	{
		if (assignedSlot != null)
		{
			ItemSlot itemSlot = assignedSlot;
			itemSlot.onItemDataChanged = (Action)Delegate.Remove(itemSlot.onItemDataChanged, new Action(OnItemSlotDataChanged));
			ItemSlot itemSlot2 = assignedSlot;
			itemSlot2.onLocked = (Action)Delegate.Remove(itemSlot2.onLocked, new Action(Lock));
			ItemSlot itemSlot3 = assignedSlot;
			itemSlot3.onUnlocked = (Action)Delegate.Remove(itemSlot3.onUnlocked, new Action(Unlock));
			if (assignedSlot is HotbarSlot)
			{
				HotbarSlot obj = assignedSlot as HotbarSlot;
				obj.onEquipChanged = (HotbarSlot.EquipEvent)Delegate.Remove(obj.onEquipChanged, new HotbarSlot.EquipEvent(SetHighlighted));
			}
			assignedSlot = null;
			SetLockVisible(vis: false);
			if ((Object)(object)FilterButton != (Object)null)
			{
				FilterButton.UnassignSlot();
			}
			UpdateUI();
		}
	}

	protected virtual void LateUpdate()
	{
		if (_slotBopQueued)
		{
			CheckSlotBop();
		}
	}

	public void OnDestroy()
	{
		if (assignedSlot != null)
		{
			ItemSlot itemSlot = assignedSlot;
			itemSlot.onItemDataChanged = (Action)Delegate.Remove(itemSlot.onItemDataChanged, new Action(OnItemSlotDataChanged));
			UnassignControllerCommands();
		}
	}

	public virtual void UpdateUI()
	{
		if ((Object)(object)ItemUI != (Object)null)
		{
			ItemUI.Destroy();
			ItemUI = null;
		}
		if (assignedSlot != null && assignedSlot.ItemInstance != null)
		{
			ItemUI itemUI = Singleton<ItemUIManager>.Instance.DefaultItemUIPrefab;
			if ((Object)(object)assignedSlot.ItemInstance.Definition.CustomItemUI != (Object)null)
			{
				itemUI = assignedSlot.ItemInstance.Definition.CustomItemUI;
			}
			ItemUI = ((Component)Object.Instantiate<ItemUI>(itemUI, (Transform)(object)ItemContainer)).GetComponent<ItemUI>();
			((Component)ItemUI).transform.SetAsLastSibling();
			ItemUI.Setup(assignedSlot.ItemInstance);
		}
	}

	public void SetHighlighted(bool h)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (h)
		{
			((Graphic)Background).color = Color32.op_Implicit(highlightColor);
		}
		else
		{
			((Graphic)Background).color = Color32.op_Implicit(normalColor);
		}
	}

	public void SetNormalColor(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		normalColor = Color32.op_Implicit(color);
		SetHighlighted(h: false);
	}

	public void SetHighlightColor(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		highlightColor = Color32.op_Implicit(color);
		SetHighlighted(h: false);
	}

	private void Lock()
	{
		SetLockVisible(vis: true);
		if ((Object)(object)FilterButton != (Object)null)
		{
			((Selectable)FilterButton.Button).interactable = false;
		}
	}

	private void Unlock()
	{
		SetLockVisible(vis: false);
		if ((Object)(object)FilterButton != (Object)null)
		{
			((Selectable)FilterButton.Button).interactable = true;
		}
	}

	public void SetLockVisible(bool vis)
	{
		LockContainer.gameObject.SetActive(vis);
	}

	public RectTransform DuplicateIcon(Transform parent, int overriddenQuantity = -1)
	{
		if ((Object)(object)ItemUI == (Object)null)
		{
			return null;
		}
		RectTransform val = ItemUI.DuplicateIcon(parent, overriddenQuantity);
		if (Object.op_Implicit((Object)(object)val))
		{
			CmdDiscardItem.HoldImage = ((Component)((Transform)val).Find("Discard")).GetComponent<Image>();
		}
		return val;
	}

	public void SetVisible(bool shown)
	{
		if ((Object)(object)ItemUI != (Object)null)
		{
			ItemUI.SetVisible(shown);
		}
	}

	public void OverrideDisplayedQuantity(int quantity)
	{
		if (!((Object)(object)ItemUI == (Object)null))
		{
			ItemUI.SetDisplayedQuantity(quantity);
		}
	}

	private void AssignControllerCommands()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		if ((Object)(object)CmdQuickMove != (Object)null)
		{
			CmdQuickMove.OnTrigger.AddListener(new UnityAction(WrapCmdQuickMove));
		}
		if ((Object)(object)CmdGrabAll != (Object)null)
		{
			CmdGrabAll.OnTrigger.AddListener(new UnityAction(WrapCmdGrabAll));
		}
		if ((Object)(object)CmdQtyAdd != (Object)null)
		{
			CmdQtyAdd.OnTrigger.AddListener(new UnityAction(WrapCmdQtyAdd));
		}
		if ((Object)(object)CmdQtySubtract != (Object)null)
		{
			CmdQtySubtract.OnTrigger.AddListener(new UnityAction(WrapCmdQtySubtract));
		}
		if ((Object)(object)CmdToggleTooltip != (Object)null)
		{
			CmdToggleTooltip.OnTrigger.AddListener(new UnityAction(WrapCmdToggleTooltip));
		}
		if ((Object)(object)CmdDiscardItem != (Object)null)
		{
			CmdDiscardItem.OnTrigger.AddListener(new UnityAction(WrapCmdDiscardItem));
		}
	}

	private void UnassignControllerCommands()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		if ((Object)(object)CmdQuickMove != (Object)null)
		{
			CmdQuickMove.OnTrigger.RemoveListener(new UnityAction(WrapCmdQuickMove));
		}
		if ((Object)(object)CmdGrabAll != (Object)null)
		{
			CmdGrabAll.OnTrigger.RemoveListener(new UnityAction(WrapCmdGrabAll));
		}
		if ((Object)(object)CmdQtyAdd != (Object)null)
		{
			CmdQtyAdd.OnTrigger.RemoveListener(new UnityAction(WrapCmdQtyAdd));
		}
		if ((Object)(object)CmdQtySubtract != (Object)null)
		{
			CmdQtySubtract.OnTrigger.RemoveListener(new UnityAction(WrapCmdQtySubtract));
		}
		if ((Object)(object)CmdToggleTooltip != (Object)null)
		{
			CmdToggleTooltip.OnTrigger.RemoveListener(new UnityAction(WrapCmdToggleTooltip));
		}
		if ((Object)(object)CmdDiscardItem != (Object)null)
		{
			CmdDiscardItem.OnTrigger.RemoveListener(new UnityAction(WrapCmdDiscardItem));
		}
	}

	private void WrapCmdQuickMove()
	{
		Singleton<ItemUIManager>.Instance.ControllerQuickMoveSlot();
	}

	private void WrapCmdGrabAll()
	{
		Singleton<ItemUIManager>.Instance.ControllerGrabAllSlot();
	}

	private void WrapCmdQtyAdd()
	{
		Singleton<ItemUIManager>.Instance.ControllerDragAddQuantity();
	}

	private void WrapCmdQtySubtract()
	{
		Singleton<ItemUIManager>.Instance.ControllerDragSubtractQuantity();
	}

	private void WrapCmdToggleTooltip()
	{
		Singleton<ItemUIManager>.Instance.ControllerToggleTooltip();
	}

	private void WrapCmdDiscardItem()
	{
		Singleton<ItemUIManager>.Instance.ControllerDiscardSlot();
	}

	public void ControllerSelect(bool isSelected)
	{
		SetHighlighted(isSelected);
		if (isSelected)
		{
			Singleton<ItemUIManager>.Instance.ControllerHighlightSlot(this);
		}
	}

	private void OnItemSlotDataChanged()
	{
		UpdateUI();
		_slotBopQueued = true;
	}

	private void CheckSlotBop()
	{
		_slotBopQueued = false;
		if (assignedSlot != null)
		{
			if (assignedSlot.Quantity > _lastQuantity && _playBopAnimation && (Object)(object)BopAnimation != (Object)null)
			{
				if (BopAnimation.isPlaying)
				{
					BopAnimation.Stop();
				}
				BopAnimation.Play();
			}
			_lastQuantity = assignedSlot.Quantity;
		}
		else
		{
			_lastQuantity = -1;
		}
	}
}
