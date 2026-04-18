using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using UnityEngine;

namespace ScheduleOne.Management;

public interface ITransitEntity
{
	public enum ESlotType
	{
		Input,
		Output,
		Both
	}

	string Name { get; }

	List<ItemSlot> InputSlots { get; set; }

	List<ItemSlot> OutputSlots { get; set; }

	Transform LinkOrigin { get; }

	Transform[] AccessPoints { get; }

	bool Selectable { get; }

	bool IsAcceptingItems { get; }

	bool IsDestroyed { get; }

	Guid GUID { get; }

	void ShowOutline(Color color);

	void HideOutline();

	void InsertItemIntoInput(ItemInstance item, NPC inserter = null)
	{
		if (GetInputCapacityForItem(item, inserter) < ((BaseItemInstance)item).Quantity)
		{
			Console.LogWarning("ITransitEntity InsertItem() called but item won't fit!");
			return;
		}
		int num = ((BaseItemInstance)item).Quantity;
		for (int i = 0; i < InputSlots.Count; i++)
		{
			if (!InputSlots[i].IsLocked && !InputSlots[i].IsAddLocked)
			{
				int capacityForItem = InputSlots[i].GetCapacityForItem(item, checkPlayerFilters: true);
				if (capacityForItem > 0)
				{
					int num2 = Mathf.Min(capacityForItem, num);
					InputSlots[i].InsertItem(item.GetCopy(num2));
					num -= num2;
				}
				if (num <= 0)
				{
					break;
				}
			}
		}
	}

	void InsertItemIntoOutput(ItemInstance item, NPC inserter = null)
	{
		if (item == null)
		{
			Console.LogWarning("ITransitEntity InsertItemIntoOutput() called with null item!");
			return;
		}
		if (GetOutputCapacityForItem(item, inserter) < ((BaseItemInstance)item).Quantity)
		{
			Console.LogWarning("ITransitEntity InsertItem() called but item won't fit!");
			return;
		}
		int num = ((BaseItemInstance)item).Quantity;
		for (int i = 0; i < OutputSlots.Count; i++)
		{
			if (!OutputSlots[i].IsLocked && !OutputSlots[i].IsAddLocked)
			{
				int capacityForItem = OutputSlots[i].GetCapacityForItem(item);
				if (capacityForItem > 0)
				{
					int num2 = Mathf.Min(capacityForItem, num);
					OutputSlots[i].InsertItem(item.GetCopy(num2));
					num -= num2;
				}
				if (num <= 0)
				{
					break;
				}
			}
		}
	}

	int GetInputCapacityForItem(ItemInstance item, NPC asker = null, bool checkPlayerFilters = true)
	{
		int num = 0;
		NetworkObject val = (((Object)(object)asker != (Object)null) ? ((NetworkBehaviour)asker).NetworkObject : null);
		for (int i = 0; i < InputSlots.Count; i++)
		{
			if (InputSlots[i].IsLocked || InputSlots[i].IsAddLocked)
			{
				bool flag = false;
				if ((Object)(object)val != (Object)null && InputSlots[i].ActiveLock != null && (Object)(object)InputSlots[i].ActiveLock.LockOwner == (Object)(object)val)
				{
					flag = true;
				}
				if (!flag)
				{
					continue;
				}
			}
			num += InputSlots[i].GetCapacityForItem(item, checkPlayerFilters);
		}
		return num;
	}

	int GetOutputCapacityForItem(ItemInstance item, NPC asker = null)
	{
		int num = 0;
		NetworkObject val = (((Object)(object)asker != (Object)null) ? ((NetworkBehaviour)asker).NetworkObject : null);
		for (int i = 0; i < OutputSlots.Count; i++)
		{
			if (OutputSlots[i].IsLocked || OutputSlots[i].IsAddLocked)
			{
				bool flag = false;
				if ((Object)(object)val != (Object)null && OutputSlots[i].ActiveLock != null && (Object)(object)OutputSlots[i].ActiveLock.LockOwner == (Object)(object)val)
				{
					flag = true;
				}
				if (!flag)
				{
					continue;
				}
			}
			num += OutputSlots[i].GetCapacityForItem(item);
		}
		return num;
	}

	ItemSlot GetOutputItemContainer(ItemInstance item)
	{
		return OutputSlots.FirstOrDefault((ItemSlot x) => x.ItemInstance == item);
	}

	List<ItemSlot> ReserveInputSlotsForItem(ItemInstance item, NetworkObject locker)
	{
		List<ItemSlot> list = new List<ItemSlot>();
		int num = ((BaseItemInstance)item).Quantity;
		for (int i = 0; i < InputSlots.Count; i++)
		{
			int capacityForItem = InputSlots[i].GetCapacityForItem(item);
			if (capacityForItem != 0)
			{
				int num2 = Mathf.Min(capacityForItem, num);
				num -= num2;
				InputSlots[i].ApplyLock(locker, "Employee is about to place an item here");
				list.Add(InputSlots[i]);
				if (num <= 0)
				{
					break;
				}
			}
		}
		return list;
	}

	void RemoveSlotLocks(NetworkObject locker)
	{
		for (int i = 0; i < InputSlots.Count; i++)
		{
			if (InputSlots[i].ActiveLock != null && (Object)(object)InputSlots[i].ActiveLock.LockOwner == (Object)(object)locker)
			{
				InputSlots[i].RemoveLock();
			}
		}
	}

	ItemSlot GetFirstSlotContainingItem(string id, ESlotType searchType)
	{
		if (searchType == ESlotType.Output || searchType == ESlotType.Both)
		{
			for (int i = 0; i < OutputSlots.Count; i++)
			{
				if (OutputSlots[i].ItemInstance != null && ((BaseItemInstance)OutputSlots[i].ItemInstance).ID == id)
				{
					return OutputSlots[i];
				}
			}
		}
		if (searchType == ESlotType.Input || searchType == ESlotType.Both)
		{
			for (int j = 0; j < InputSlots.Count; j++)
			{
				if (InputSlots[j].ItemInstance != null && ((BaseItemInstance)InputSlots[j].ItemInstance).ID == id)
				{
					return InputSlots[j];
				}
			}
		}
		return null;
	}

	ItemSlot GetFirstSlotContainingTemplateItem(ItemInstance templateItem, ESlotType searchType)
	{
		if (searchType == ESlotType.Output || searchType == ESlotType.Both)
		{
			for (int i = 0; i < OutputSlots.Count; i++)
			{
				if (OutputSlots[i].ItemInstance != null && OutputSlots[i].ItemInstance.CanStackWith(templateItem, checkQuantities: false))
				{
					return OutputSlots[i];
				}
			}
		}
		if (searchType == ESlotType.Input || searchType == ESlotType.Both)
		{
			for (int j = 0; j < InputSlots.Count; j++)
			{
				if (InputSlots[j].ItemInstance != null && InputSlots[j].ItemInstance.CanStackWith(templateItem, checkQuantities: false))
				{
					return InputSlots[j];
				}
			}
		}
		return null;
	}
}
