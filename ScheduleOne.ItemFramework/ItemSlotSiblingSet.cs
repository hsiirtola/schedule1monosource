using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.ItemFramework;

public class ItemSlotSiblingSet
{
	public List<ItemSlot> Slots = new List<ItemSlot>();

	public ItemSlotSiblingSet(params ItemSlot[] slots)
	{
		foreach (ItemSlot slot in slots)
		{
			AddSlot(slot);
		}
	}

	public ItemSlotSiblingSet(List<ItemSlot> slots)
	{
		foreach (ItemSlot slot in slots)
		{
			AddSlot(slot);
		}
	}

	public void AddSlot(ItemSlot slot)
	{
		if (Slots.Contains(slot))
		{
			Debug.LogWarning((object)"Slot already exists in this sibling set");
			return;
		}
		Slots.Add(slot);
		slot.SetSiblingSet(this);
	}
}
