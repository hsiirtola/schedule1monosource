using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Map;

public class ParkingLot : MonoBehaviour, IGUIDRegisterable
{
	[SerializeField]
	protected string BakedGUID = string.Empty;

	[Header("READONLY")]
	public List<ParkingSpot> ParkingSpots = new List<ParkingSpot>();

	[Header("Entry")]
	public Transform EntryPoint;

	public Transform HiddenVehicleAccessPoint;

	[Header("Exit")]
	public bool UseExitPoint;

	public EParkingAlignment ExitAlignment = EParkingAlignment.RearToKerb;

	public Transform ExitPoint;

	public VehicleDetector ExitPointVehicleDetector;

	public Guid GUID { get; protected set; }

	private void Awake()
	{
		if ((Object)(object)ExitPoint != (Object)null && (Object)(object)ExitPointVehicleDetector == (Object)null)
		{
			Console.LogWarning("ExitPoint specified but no ExitPointVehicleDetector!");
		}
		if (!GUIDManager.IsGUIDValid(BakedGUID))
		{
			Console.LogError(((Object)((Component)this).gameObject).name + "'s baked GUID is not valid!");
		}
		if (GUIDManager.IsGUIDAlreadyRegistered(new Guid(BakedGUID)))
		{
			Console.LogError("ParkingLot " + ((Object)((Component)this).gameObject).name + " has a GUID that is already registered!", (Object)(object)this);
		}
		GUID = new Guid(BakedGUID);
		GUIDManager.RegisterObject(this);
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	public ParkingSpot GetRandomFreeSpot()
	{
		List<ParkingSpot> freeParkingSpots = GetFreeParkingSpots();
		if (freeParkingSpots.Count == 0)
		{
			Console.Log("No free parking spots in " + ((Object)((Component)this).gameObject).name + "!");
			return null;
		}
		return freeParkingSpots[Random.Range(0, freeParkingSpots.Count)];
	}

	public int GetRandomFreeSpotIndex()
	{
		List<ParkingSpot> freeParkingSpots = GetFreeParkingSpots();
		if (freeParkingSpots.Count == 0)
		{
			return -1;
		}
		return ParkingSpots.IndexOf(freeParkingSpots[Random.Range(0, freeParkingSpots.Count)]);
	}

	public List<ParkingSpot> GetFreeParkingSpots()
	{
		if (ParkingSpots == null || ParkingSpots.Count == 0)
		{
			return new List<ParkingSpot>();
		}
		return ParkingSpots.Where((ParkingSpot x) => (Object)(object)x != (Object)null && (Object)(object)x.OccupantVehicle == (Object)null).ToList();
	}
}
