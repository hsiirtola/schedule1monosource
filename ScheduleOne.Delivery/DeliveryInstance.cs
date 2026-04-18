using System;
using FishNet.Serializing.Helping;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Property;
using ScheduleOne.UI.Phone.Delivery;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Delivery;

[Serializable]
public class DeliveryInstance
{
	public string DeliveryID;

	public string StoreName;

	public string DestinationCode;

	public int LoadingDockIndex;

	public StringIntPair[] Items;

	public EDeliveryStatus Status;

	public int TimeUntilArrival;

	[NonSerialized]
	[CodegenExclude]
	public UnityEvent onDeliveryCompleted;

	[CodegenExclude]
	public DeliveryVehicle ActiveVehicle { get; private set; }

	[CodegenExclude]
	public ScheduleOne.Property.Property Destination => Singleton<PropertyManager>.Instance.GetProperty(DestinationCode);

	[CodegenExclude]
	public LoadingDock LoadingDock => Destination.LoadingDocks[LoadingDockIndex];

	public DeliveryInstance(string deliveryID, string storeName, string destinationCode, int loadingDockIndex, StringIntPair[] items, EDeliveryStatus status, int timeUntilArrival)
	{
		DeliveryID = deliveryID;
		StoreName = storeName;
		DestinationCode = destinationCode;
		LoadingDockIndex = loadingDockIndex;
		Items = items;
		Status = status;
		TimeUntilArrival = timeUntilArrival;
	}

	public DeliveryInstance()
	{
	}

	public int GetTimeStatus()
	{
		if (Status == EDeliveryStatus.Arrived)
		{
			return -2;
		}
		if (Status == EDeliveryStatus.Waiting)
		{
			return -1;
		}
		return TimeUntilArrival;
	}

	public void SetStatus(EDeliveryStatus status)
	{
		Console.Log("Setting delivery status to " + status.ToString() + " for delivery " + DeliveryID);
		Status = status;
		if (Status == EDeliveryStatus.Arrived)
		{
			ActiveVehicle = NetworkSingleton<DeliveryManager>.Instance.GetShopInterface(StoreName).DeliveryVehicle;
			ActiveVehicle.Activate(this);
		}
		if (Status == EDeliveryStatus.Completed)
		{
			if ((Object)(object)ActiveVehicle != (Object)null)
			{
				ActiveVehicle.Deactivate();
			}
			if (onDeliveryCompleted != null)
			{
				onDeliveryCompleted.Invoke();
			}
		}
	}

	public void AddItemsToDeliveryVehicle()
	{
		DeliveryVehicle deliveryVehicle = PlayerSingleton<DeliveryApp>.Instance.GetShop(StoreName).MatchingShop.DeliveryVehicle;
		StringIntPair[] items = Items;
		foreach (StringIntPair stringIntPair in items)
		{
			ItemDefinition item = Registry.GetItem(stringIntPair.String);
			int num = stringIntPair.Int;
			while (num > 0)
			{
				int num2 = Mathf.Min(num, ((BaseItemDefinition)item).StackLimit);
				num -= num2;
				ItemInstance defaultInstance = Registry.GetItem(stringIntPair.String).GetDefaultInstance(num2);
				deliveryVehicle.Vehicle.Storage.InsertItem(defaultInstance);
			}
		}
	}

	public DeliveryReceipt GetReceipt()
	{
		return new DeliveryReceipt(DeliveryID, StoreName, DestinationCode, LoadingDockIndex, Items);
	}

	public void OnTimePass(int minutes)
	{
		TimeUntilArrival = Mathf.Max(0, TimeUntilArrival - minutes);
	}
}
