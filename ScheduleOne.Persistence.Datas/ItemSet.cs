using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

[Serializable]
public class ItemSet
{
	public string[] Items;

	public SlotFilter[] SlotFilters;

	public ItemSet(List<ItemData> items)
	{
		Items = new string[items.Count];
		for (int i = 0; i < items.Count; i++)
		{
			Items[i] = items[i].GetJson(prettyPrint: false);
		}
	}

	public string GetJSON()
	{
		return JsonUtility.ToJson((object)this, true);
	}

	public ItemSet(List<ItemSlot> itemSlots)
	{
		Items = new string[itemSlots.Count];
		SlotFilters = new SlotFilter[itemSlots.Count];
		for (int i = 0; i < itemSlots.Count; i++)
		{
			if (itemSlots[i].ItemInstance != null)
			{
				Items[i] = itemSlots[i].ItemInstance.GetItemData().GetJson(prettyPrint: false);
			}
			else
			{
				Items[i] = new ItemData(string.Empty, 0).GetJson(prettyPrint: false);
			}
			if (!itemSlots[i].PlayerFilter.IsDefault())
			{
				SlotFilters[i] = itemSlots[i].PlayerFilter;
			}
			else
			{
				SlotFilters[i] = null;
			}
		}
	}

	public ItemSet(ItemSlot[] itemSlots)
	{
		Items = new string[itemSlots.Length];
		SlotFilters = new SlotFilter[itemSlots.Length];
		for (int i = 0; i < itemSlots.Length; i++)
		{
			if (itemSlots[i].ItemInstance != null)
			{
				Items[i] = itemSlots[i].ItemInstance.GetItemData().GetJson(prettyPrint: false);
			}
			else
			{
				Items[i] = new ItemData(string.Empty, 0).GetJson(prettyPrint: false);
			}
			if (!itemSlots[i].PlayerFilter.IsDefault())
			{
				SlotFilters[i] = itemSlots[i].PlayerFilter;
			}
			else
			{
				SlotFilters[i] = null;
			}
		}
	}

	public void LoadTo(List<ItemSlot> slots)
	{
		for (int i = 0; i < slots.Count; i++)
		{
			if (Items != null && Items.Length > i)
			{
				slots[i].SetStoredItem(ItemDeserializer.LoadItem(Items[i]));
			}
			if (SlotFilters != null && SlotFilters.Length > i)
			{
				slots[i].SetPlayerFilter(SlotFilters[i]);
			}
		}
	}

	public void LoadTo(ItemSlot[] slots)
	{
		LoadTo(slots.ToList());
	}

	public void LoadTo(ItemSlot slot, int index = 0)
	{
		if (Items != null && Items.Length > index)
		{
			slot.SetStoredItem(ItemDeserializer.LoadItem(Items[index]));
		}
		if (SlotFilters != null && SlotFilters.Length > index)
		{
			slot.SetPlayerFilter(SlotFilters[index]);
		}
	}

	public static bool TryDeserialize(string json, out DeserializedItemSet itemSet)
	{
		itemSet = new DeserializedItemSet();
		ItemSet itemSet2 = null;
		try
		{
			itemSet2 = JsonUtility.FromJson<ItemSet>(json);
		}
		catch (Exception ex)
		{
			Console.LogError("Failed to deserialize ItemSet from JSON: " + json + "\nException: " + ex);
			return false;
		}
		return TryDeserialize(itemSet2, out itemSet);
	}

	public static bool TryDeserialize(ItemSet set, out DeserializedItemSet itemSet)
	{
		itemSet = new DeserializedItemSet();
		if (set == null)
		{
			Console.LogError("ItemSet is null");
			return false;
		}
		if (set.Items != null)
		{
			itemSet.Items = new ItemInstance[set.Items.Length];
			itemSet.SlotFilters = new SlotFilter[set.Items.Length];
			for (int i = 0; i < set.Items.Length; i++)
			{
				itemSet.Items[i] = ItemDeserializer.LoadItem(set.Items[i]);
				if (set.SlotFilters != null && set.SlotFilters.Length > i)
				{
					itemSet.SlotFilters[i] = set.SlotFilters[i];
				}
				else
				{
					itemSet.SlotFilters[i] = null;
				}
			}
		}
		return true;
	}
}
