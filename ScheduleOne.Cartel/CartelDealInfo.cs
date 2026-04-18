using System;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Cartel;

[Serializable]
public class CartelDealInfo
{
	public enum EStatus
	{
		Pending,
		Overdue
	}

	public string RequestedProductID;

	public int RequestedProductQuantity;

	public int PaymentAmount;

	public GameDateTime DueTime;

	public EStatus Status;

	public CartelDealInfo(string requestedProductID, int requestedProductQuantity, int payment, GameDateTime dueTime, EStatus status)
	{
		RequestedProductID = requestedProductID;
		RequestedProductQuantity = requestedProductQuantity;
		PaymentAmount = payment;
		DueTime = dueTime;
		Status = EStatus.Pending;
	}

	public CartelDealInfo()
	{
		RequestedProductID = string.Empty;
		RequestedProductQuantity = 0;
		DueTime = default(GameDateTime);
		Status = EStatus.Pending;
	}

	public bool IsValid()
	{
		if ((Object)(object)Registry.GetItem(RequestedProductID) == (Object)null)
		{
			return false;
		}
		if (RequestedProductQuantity <= 0)
		{
			return false;
		}
		return true;
	}
}
