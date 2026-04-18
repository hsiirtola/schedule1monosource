using System.Collections.Generic;
using ScheduleOne.ItemFramework;

namespace ScheduleOne.Persistence.Datas;

public class DeserializedItemSet
{
	public ItemInstance[] Items;

	public SlotFilter[] SlotFilters;

	public ItemInstance GetItemAt(int index)
	{
		if (Items == null || index < 0 || index >= Items.Length)
		{
			return null;
		}
		return Items[index];
	}

	public SlotFilter GetSlotFilterAt(int index)
	{
		if (SlotFilters == null || index < 0 || index >= SlotFilters.Length)
		{
			return null;
		}
		return SlotFilters[index];
	}

	public void LoadTo(List<ItemSlot> slots)
	{
		for (int i = 0; i < slots.Count; i++)
		{
			if (Items != null && Items.Length > i)
			{
				slots[i].SetStoredItem(Items[i]);
			}
			if (SlotFilters != null && SlotFilters.Length > i)
			{
				slots[i].SetPlayerFilter(SlotFilters[i]);
			}
		}
	}
}
