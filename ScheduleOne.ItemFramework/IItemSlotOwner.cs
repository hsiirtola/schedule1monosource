using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.ItemFramework;

public interface IItemSlotOwner
{
	List<ItemSlot> ItemSlots { get; set; }

	void SetStoredInstance(NetworkConnection conn, int itemSlotIndex, ItemInstance instance);

	void SetItemSlotQuantity(int itemSlotIndex, int quantity);

	void SetSlotLocked(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason);

	void SetSlotFilter(NetworkConnection conn, int itemSlotIndex, SlotFilter filter);

	void SendItemSlotDataToClient(NetworkConnection conn)
	{
		if (NetworkSingleton<ProductManager>.InstanceExists && !NetworkSingleton<ProductManager>.Instance.HasSentProductDataToConnection(conn))
		{
			Debug.Log((object)"Deferring sending item slot data until product data has been sent!");
			ProductManager instance = NetworkSingleton<ProductManager>.Instance;
			instance.onProductDataSentToConnection = (Action<NetworkConnection>)Delegate.Combine(instance.onProductDataSentToConnection, new Action<NetworkConnection>(Send));
		}
		else
		{
			Send(conn);
		}
		private void Send(NetworkConnection conn2)
		{
			for (int i = 0; i < ItemSlots.Count; i++)
			{
				if (ItemSlots[i].ItemInstance != null)
				{
					SetStoredInstance(conn2, i, ItemSlots[i].ItemInstance);
				}
				if (ItemSlots[i].IsLocked)
				{
					SetSlotLocked(conn2, i, locked: true, ItemSlots[i].ActiveLock.LockOwner, ItemSlots[i].ActiveLock.LockReason);
				}
				if (!ItemSlots[i].PlayerFilter.IsDefault())
				{
					SetSlotFilter(conn2, i, ItemSlots[i].PlayerFilter);
				}
			}
			if (NetworkSingleton<ProductManager>.InstanceExists)
			{
				ProductManager instance2 = NetworkSingleton<ProductManager>.Instance;
				instance2.onProductDataSentToConnection = (Action<NetworkConnection>)Delegate.Remove(instance2.onProductDataSentToConnection, new Action<NetworkConnection>(Send));
			}
		}
	}

	int GetQuantitySum()
	{
		return ItemSlots.Sum((ItemSlot x) => x.Quantity);
	}

	int GetQuantityOfItem(string id)
	{
		int num = 0;
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			if (ItemSlots[i].ItemInstance != null && ((BaseItemInstance)ItemSlots[i].ItemInstance).ID == id)
			{
				num += ItemSlots[i].Quantity;
			}
		}
		return num;
	}

	int GetNonEmptySlotCount()
	{
		int num = 0;
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			if (ItemSlots[i].ItemInstance != null)
			{
				num++;
			}
		}
		return num;
	}

	ItemSlot GetFirstSlotContaining(string id)
	{
		for (int i = 0; i < ItemSlots.Count; i++)
		{
			if (ItemSlots[i].ItemInstance != null && ((BaseItemInstance)ItemSlots[i].ItemInstance).ID == id)
			{
				return ItemSlots[i];
			}
		}
		return null;
	}
}
