using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Economy;

public class SupplierLocation : MonoBehaviour
{
	public static List<SupplierLocation> AllLocations = new List<SupplierLocation>();

	[Header("Settings")]
	public string LocationName;

	public string LocationDescription;

	[Header("References")]
	public Transform GenericContainer;

	public Transform SupplierStandPoint;

	public WorldStorageEntity[] DeliveryBays;

	public POI PoI;

	private SupplierLocationConfiguration[] configs;

	public bool IsOccupied => (Object)(object)ActiveSupplier != (Object)null;

	public Supplier ActiveSupplier { get; private set; }

	public void Awake()
	{
		AllLocations.Add(this);
		((Component)GenericContainer).gameObject.SetActive(false);
		WorldStorageEntity[] deliveryBays = DeliveryBays;
		for (int i = 0; i < deliveryBays.Length; i++)
		{
			((Component)((Component)deliveryBays[i]).transform.Find("Container")).gameObject.SetActive(false);
		}
		configs = ((Component)this).GetComponentsInChildren<SupplierLocationConfiguration>(true);
		SupplierLocationConfiguration[] array = configs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Deactivate();
		}
	}

	private void Start()
	{
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepStart = (Action)Delegate.Combine(instance.onSleepStart, new Action(OnSleep));
	}

	private void OnSleep()
	{
		SetDeliveryBaysVisible(visible: false);
	}

	private void OnDestroy()
	{
		AllLocations.Remove(this);
	}

	public void SetActiveSupplier(Supplier supplier)
	{
		ActiveSupplier = supplier;
		((Component)GenericContainer).gameObject.SetActive((Object)(object)ActiveSupplier != (Object)null);
		if ((Object)(object)supplier != (Object)null)
		{
			SetDeliveryBaysVisible(visible: true);
		}
		if ((Object)(object)supplier != (Object)null)
		{
			PoI.SetMainText("Supplier Meeting\n(" + supplier.fullName + ")");
		}
		SupplierLocationConfiguration[] array = configs;
		foreach (SupplierLocationConfiguration supplierLocationConfiguration in array)
		{
			if ((Object)(object)ActiveSupplier != (Object)null && supplierLocationConfiguration.SupplierID == ActiveSupplier.ID)
			{
				supplierLocationConfiguration.Activate();
			}
			else
			{
				supplierLocationConfiguration.Deactivate();
			}
		}
	}

	private void SetDeliveryBaysVisible(bool visible)
	{
		WorldStorageEntity[] deliveryBays = DeliveryBays;
		for (int i = 0; i < deliveryBays.Length; i++)
		{
			((Component)((Component)deliveryBays[i]).transform.Find("Container")).gameObject.SetActive(visible);
		}
	}
}
