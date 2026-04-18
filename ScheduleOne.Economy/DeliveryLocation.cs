using System;
using System.Collections.Generic;
using ScheduleOne.Quests;
using UnityEngine;

namespace ScheduleOne.Economy;

public class DeliveryLocation : MonoBehaviour, IGUIDRegisterable
{
	public string LocationName = string.Empty;

	public string LocationDescription = string.Empty;

	public Transform CustomerStandPoint;

	public Transform TeleportPoint;

	public string StaticGUID = string.Empty;

	public List<Contract> ScheduledContracts = new List<Contract>();

	public Guid GUID { get; protected set; }

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	private void Awake()
	{
		if (!GUIDManager.IsGUIDValid(StaticGUID) || GUIDManager.IsGUIDAlreadyRegistered(new Guid(StaticGUID)))
		{
			Console.LogError("Delivery location Static GUID is not valid.");
		}
		else
		{
			((IGUIDRegisterable)this).SetGUID(StaticGUID);
		}
	}

	private void OnValidate()
	{
		((Object)((Component)this).gameObject).name = LocationName;
	}

	public virtual string GetDescription()
	{
		return LocationDescription;
	}
}
