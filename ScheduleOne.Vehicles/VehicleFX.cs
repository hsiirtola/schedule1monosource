using System;
using UnityEngine;

namespace ScheduleOne.Vehicles;

public class VehicleFX : MonoBehaviour
{
	public ParticleSystem[] exhaustFX;

	protected virtual void Awake()
	{
		LandVehicle componentInParent = ((Component)this).GetComponentInParent<LandVehicle>();
		if (!((Object)(object)componentInParent == (Object)null))
		{
			componentInParent.onVehicleStart = (Action)Delegate.Combine(componentInParent.onVehicleStart, new Action(OnVehicleStart));
			componentInParent.onVehicleStop = (Action)Delegate.Combine(componentInParent.onVehicleStop, new Action(OnVehicleStop));
		}
	}

	public virtual void OnVehicleStart()
	{
		ParticleSystem[] array = exhaustFX;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}

	public virtual void OnVehicleStop()
	{
		ParticleSystem[] array = exhaustFX;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
	}
}
