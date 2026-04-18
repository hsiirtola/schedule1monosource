using System;
using ScheduleOne.DevUtilities;

namespace ScheduleOne.Delivery;

[Serializable]
public class DeliveryReceipt
{
	public string DeliveryID;

	public string StoreName;

	public string DestinationCode;

	public int LoadingDockIndex;

	public StringIntPair[] Items;

	public DeliveryReceipt(string deliveryID, string storeName, string destinationCode, int loadingDockIndex, StringIntPair[] items)
	{
		DeliveryID = deliveryID;
		StoreName = storeName;
		DestinationCode = destinationCode;
		LoadingDockIndex = loadingDockIndex;
		Items = items;
	}

	public DeliveryReceipt()
	{
	}
}
