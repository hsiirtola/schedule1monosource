using System;
using System.Collections.Generic;
using FishNet.Object;
using ScheduleOne.Core.Items.Framework;
using UnityEngine;

namespace ScheduleOne.ItemFramework;

[Serializable]
public class ItemSlot
{
	public Action onItemDataChanged;

	public Action onItemInstanceChanged;

	public Action onLocked;

	public Action onUnlocked;

	public Action onFilterChange;

	public ItemInstance ItemInstance { get; protected set; }

	public IItemSlotOwner SlotOwner { get; protected set; }

	private int SlotIndex => SlotOwner.ItemSlots.IndexOf(this);

	public int Quantity
	{
		get
		{
			if (ItemInstance == null)
			{
				return 0;
			}
			return ((BaseItemInstance)ItemInstance).Quantity;
		}
	}

	public bool IsAtCapacity
	{
		get
		{
			if (ItemInstance != null)
			{
				return Quantity >= ((BaseItemInstance)ItemInstance).StackLimit;
			}
			return false;
		}
	}

	public bool IsLocked => ActiveLock != null;

	public ItemSlotLock ActiveLock { get; protected set; }

	public bool IsRemovalLocked { get; protected set; }

	public bool IsAddLocked { get; protected set; }

	protected List<ItemFilter> HardFilters { get; set; } = new List<ItemFilter>();

	public bool CanPlayerSetFilter { get; set; }

	public SlotFilter PlayerFilter { get; set; } = new SlotFilter();

	public ItemSlotSiblingSet SiblingSet { get; set; }

	public void SetSlotOwner(IItemSlotOwner owner)
	{
		SlotOwner = owner;
		SlotOwner.ItemSlots.Add(this);
	}

	public void SetSiblingSet(ItemSlotSiblingSet set)
	{
		if (SiblingSet != null)
		{
			Console.LogError("SetSiblingSet called on ItemSlot that already has a sibling set! Refusing.");
		}
		else
		{
			SiblingSet = set;
		}
	}

	public ItemSlot()
	{
		CanPlayerSetFilter = false;
		HardFilters = new List<ItemFilter>();
		PlayerFilter = new SlotFilter();
	}

	public ItemSlot(bool canPlayerSetFilter = false)
	{
		CanPlayerSetFilter = canPlayerSetFilter;
		HardFilters = new List<ItemFilter>();
		PlayerFilter = new SlotFilter();
	}

	public void ReplicateStoredInstance()
	{
		if (SlotOwner != null)
		{
			SlotOwner.SetStoredInstance(null, SlotIndex, ItemInstance);
		}
	}

	public virtual void SetStoredItem(ItemInstance instance, bool _internal = false)
	{
		if (IsLocked && !_internal)
		{
			Console.LogError("SetStoredInstance called on ItemSlot that is locked! Refusing.");
			return;
		}
		if (IsRemovalLocked)
		{
			Console.LogWarning("SetStoredItem called on ItemSlot that isRemovalLocked. You probably shouldn't do this.");
		}
		if (_internal || SlotOwner == null)
		{
			if (ItemInstance != null)
			{
				ClearStoredInstance(_internal: true);
			}
			ItemInstance = instance;
			if (ItemInstance != null)
			{
				((BaseItemInstance)ItemInstance).onDataChanged += ItemDataChanged;
				((BaseItemInstance)ItemInstance).requestClearSlot += ClearItemInstanceRequested;
			}
			if (onItemDataChanged != null)
			{
				onItemDataChanged();
			}
			if (onItemInstanceChanged != null)
			{
				onItemInstanceChanged();
			}
			ItemDataChanged();
		}
		else
		{
			SlotOwner.SetStoredInstance(null, SlotIndex, instance);
		}
	}

	public virtual void InsertItem(ItemInstance item)
	{
		AddItem(item);
	}

	public virtual void AddItem(ItemInstance item, bool _internal = false)
	{
		if (ItemInstance == null)
		{
			SetStoredItem(item, _internal);
		}
		else if (!ItemInstance.CanStackWith(item))
		{
			Console.LogWarning("AddItem called with item that cannot stack with current item. Refusing.");
		}
		else
		{
			ChangeQuantity(((BaseItemInstance)item).Quantity, _internal);
		}
	}

	public virtual void ClearStoredInstance(bool _internal = false)
	{
		if (IsRemovalLocked)
		{
			Console.LogError("ClearStoredInstance called on ItemSlot that is removal locked! Refusing.");
		}
		else
		{
			if (ItemInstance == null)
			{
				return;
			}
			if (_internal || SlotOwner == null)
			{
				((BaseItemInstance)ItemInstance).onDataChanged -= ItemDataChanged;
				((BaseItemInstance)ItemInstance).requestClearSlot -= ClearItemInstanceRequested;
				ItemInstance = null;
				if (onItemDataChanged != null)
				{
					onItemDataChanged();
				}
				if (onItemInstanceChanged != null)
				{
					onItemInstanceChanged();
				}
			}
			else
			{
				SlotOwner.SetStoredInstance(null, SlotIndex, null);
			}
		}
	}

	public void SetQuantity(int amount, bool _internal = false)
	{
		if (IsLocked && amount > Quantity)
		{
			Console.LogError("SetQuantity called on ItemSlot that is locked! Refusing.");
		}
		else if (ItemInstance == null)
		{
			Console.LogWarning("ChangeQuantity called but ItemInstance is null");
		}
		else if (amount < ((BaseItemInstance)ItemInstance).Quantity && IsRemovalLocked)
		{
			Console.LogError("SetQuantity called on ItemSlot and passed lower quantity that current, and isRemovalLocked = true. Refusing.");
		}
		else if (amount <= 0)
		{
			ClearStoredInstance(_internal);
		}
		else if (_internal || SlotOwner == null)
		{
			((BaseItemInstance)ItemInstance).SetQuantity(amount);
		}
		else
		{
			SlotOwner.SetItemSlotQuantity(SlotIndex, amount);
		}
	}

	public void ChangeQuantity(int change, bool _internal = false)
	{
		if (change != 0)
		{
			if (IsLocked && change > 0)
			{
				Console.LogWarning("ChangeQuantity called with change > 0 but isLocked = true! Refusing");
			}
			else if (ItemInstance == null)
			{
				Console.LogWarning("ChangeQuantity called but ItemInstance is null");
			}
			else if (IsRemovalLocked && change < 0)
			{
				Console.Log("Removal locked!");
			}
			else if (Quantity + change <= 0)
			{
				ClearStoredInstance(_internal);
			}
			else if (_internal || SlotOwner == null)
			{
				((BaseItemInstance)ItemInstance).ChangeQuantity(change);
			}
			else
			{
				SlotOwner.SetItemSlotQuantity(SlotIndex, Quantity + change);
			}
		}
	}

	protected virtual void ItemDataChanged()
	{
		if (ItemInstance != null && ((BaseItemInstance)ItemInstance).Quantity <= 0)
		{
			ClearStoredInstance();
		}
		else if (onItemDataChanged != null)
		{
			onItemDataChanged();
		}
	}

	protected virtual void ClearItemInstanceRequested()
	{
		ClearStoredInstance();
	}

	public void AddFilter(ItemFilter filter)
	{
		if (HardFilters == null)
		{
			HardFilters = new List<ItemFilter>();
		}
		HardFilters.Add(filter);
	}

	public void ApplyLock(NetworkObject lockOwner, string lockReason, bool _internal = false)
	{
		if (_internal || SlotOwner == null)
		{
			ActiveLock = new ItemSlotLock(this, lockOwner, lockReason);
			if (onLocked != null)
			{
				onLocked();
			}
		}
		else
		{
			SlotOwner.SetSlotLocked(null, SlotIndex, locked: true, lockOwner, lockReason);
		}
	}

	public void RemoveLock(bool _internal = false)
	{
		if (_internal || SlotOwner == null)
		{
			ActiveLock = null;
			if (onUnlocked != null)
			{
				onUnlocked();
			}
		}
		else
		{
			SlotOwner.SetSlotLocked(null, SlotIndex, locked: false, null, string.Empty);
		}
	}

	public void SetIsRemovalLocked(bool locked)
	{
		IsRemovalLocked = locked;
	}

	public void SetIsAddLocked(bool locked)
	{
		IsAddLocked = locked;
	}

	public virtual bool DoesItemMatchHardFilters(ItemInstance item)
	{
		foreach (ItemFilter hardFilter in HardFilters)
		{
			if (!hardFilter.DoesItemMatchFilter(item))
			{
				return false;
			}
		}
		if (item is CashInstance)
		{
			return CanSlotAcceptCash();
		}
		return true;
	}

	public virtual bool DoesItemMatchPlayerFilters(ItemInstance item)
	{
		if (!PlayerFilter.DoesItemMatchFilter(item))
		{
			return false;
		}
		return true;
	}

	public void SetFilterable(bool filterable)
	{
		CanPlayerSetFilter = filterable;
		if (!filterable)
		{
			PlayerFilter = new SlotFilter();
		}
	}

	public void SetPlayerFilter(SlotFilter filter, bool _internal = false)
	{
		if (filter == null)
		{
			return;
		}
		if (_internal || SlotOwner == null)
		{
			PlayerFilter = filter;
			if (onFilterChange != null)
			{
				onFilterChange();
			}
		}
		else
		{
			SlotOwner.SetSlotFilter(null, SlotIndex, filter);
		}
	}

	public virtual int GetCapacityForItem(ItemInstance item, bool checkPlayerFilters = false)
	{
		if (!DoesItemMatchHardFilters(item))
		{
			return 0;
		}
		if (checkPlayerFilters && !DoesItemMatchPlayerFilters(item))
		{
			return 0;
		}
		if (ItemInstance == null || ItemInstance.CanStackWith(item, checkQuantities: false))
		{
			return ((BaseItemInstance)item).StackLimit - Quantity;
		}
		return 0;
	}

	public virtual bool CanSlotAcceptCash()
	{
		return true;
	}

	public static bool TryInsertItemIntoSet(List<ItemSlot> ItemSlots, ItemInstance item)
	{
		int num = ((BaseItemInstance)item).Quantity;
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			if (num <= 0)
			{
				break;
			}
			if (!ItemSlots[i].IsLocked && !ItemSlots[i].IsAddLocked && ItemSlots[i].ItemInstance != null && ItemSlots[i].ItemInstance.CanStackWith(item))
			{
				int num2 = Mathf.Min(((BaseItemInstance)item).StackLimit - ((BaseItemInstance)ItemSlots[i].ItemInstance).Quantity, num);
				num -= num2;
				ItemSlots[i].ChangeQuantity(num2);
			}
		}
		for (int j = 0; j < ItemSlots.Count; j++)
		{
			if (num <= 0)
			{
				break;
			}
			if (!ItemSlots[j].IsLocked && !ItemSlots[j].IsAddLocked && ItemSlots[j].ItemInstance == null)
			{
				num -= ((BaseItemInstance)item).StackLimit;
				ItemSlots[j].SetStoredItem(item);
				break;
			}
		}
		return num <= 0;
	}
}
