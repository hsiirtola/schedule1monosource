using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.GameTime;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

[RequireComponent(typeof(Rigidbody))]
public class VehicleDetector : MonoBehaviour
{
	public const float ACTIVATION_DISTANCE_SQ = 400f;

	public List<LandVehicle> vehicles = new List<LandVehicle>();

	public LandVehicle closestVehicle;

	private bool ignoreExit;

	private Collider[] detectionColliders;

	private bool collidersEnabled = true;

	public bool IgnoreNewDetections { get; protected set; }

	private void Awake()
	{
		Rigidbody val = ((Component)this).GetComponent<Rigidbody>();
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)this).gameObject.AddComponent<Rigidbody>();
		}
		detectionColliders = ((Component)this).GetComponentsInChildren<Collider>();
		val.isKinematic = true;
	}

	private void Start()
	{
		NetworkSingleton<TimeManager>.Instance.onTick += new Action(OnTick);
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onTick -= new Action(OnTick);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!IgnoreNewDetections)
		{
			LandVehicle componentInParent = ((Component)other).GetComponentInParent<LandVehicle>();
			if ((Object)(object)componentInParent != (Object)null && (Object)(object)other == (Object)(object)componentInParent.boundingBox && !vehicles.Contains(componentInParent))
			{
				vehicles.Add(componentInParent);
				SortVehicles();
			}
		}
	}

	private void OnTick()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		for (int i = 0; i < NetworkSingleton<VehicleManager>.Instance.AllVehicles.Count; i++)
		{
			if (Vector3.SqrMagnitude(((Component)NetworkSingleton<VehicleManager>.Instance.AllVehicles[i]).transform.position - ((Component)this).transform.position) < 400f)
			{
				flag = true;
				break;
			}
		}
		if (flag != collidersEnabled)
		{
			collidersEnabled = flag;
			for (int j = 0; j < detectionColliders.Length; j++)
			{
				detectionColliders[j].enabled = collidersEnabled;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!ignoreExit)
		{
			LandVehicle componentInParent = ((Component)other).GetComponentInParent<LandVehicle>();
			if ((Object)(object)componentInParent != (Object)null && (Object)(object)other == (Object)(object)componentInParent.boundingBox && vehicles.Contains(componentInParent))
			{
				vehicles.Remove(componentInParent);
				SortVehicles();
			}
		}
	}

	private void SortVehicles()
	{
		if (vehicles.Count > 1)
		{
			vehicles.OrderBy((LandVehicle x) => Vector3.Distance(((Component)this).transform.position, ((Component)x).transform.position));
		}
		if (vehicles.Count > 0)
		{
			closestVehicle = vehicles[0];
		}
		else
		{
			closestVehicle = null;
		}
	}

	public void SetIgnoreNewCollisions(bool ignore)
	{
		IgnoreNewDetections = ignore;
		if (ignore)
		{
			return;
		}
		ignoreExit = true;
		Collider[] componentsInChildren = ((Component)this).GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].isTrigger)
			{
				componentsInChildren[i].enabled = false;
				componentsInChildren[i].enabled = true;
			}
		}
		ignoreExit = false;
	}

	public bool AreAnyVehiclesOccupied()
	{
		for (int i = 0; i < vehicles.Count; i++)
		{
			if (vehicles[i].IsOccupied)
			{
				return true;
			}
		}
		return false;
	}

	public void Clear()
	{
		vehicles.Clear();
		SortVehicles();
	}
}
