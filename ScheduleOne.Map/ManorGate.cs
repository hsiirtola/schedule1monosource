using FishNet;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Map;

public class ManorGate : Gate
{
	[Header("References")]
	public InteractableObject IntercomInt;

	public Light IntercomLight;

	public VehicleDetector ExteriorVehicleDetector;

	public PlayerDetector ExteriorPlayerDetector;

	public VehicleDetector InteriorVehicleDetector;

	public PlayerDetector InteriorPlayerDetector;

	private bool intercomActive;

	private bool NetworkInitialize___EarlyScheduleOne_002EMap_002EManorGateAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EMap_002EManorGateAssembly_002DCSharp_002Edll_Excuted;

	protected virtual void Start()
	{
		SetIntercomActive(active: false);
		SetEnterable(enterable: false);
		((MonoBehaviour)this).InvokeRepeating("UpdateDetection", 0f, 0.25f);
	}

	private void UpdateDetection()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		bool flag = false;
		if (ExteriorVehicleDetector.AreAnyVehiclesOccupied())
		{
			flag = true;
		}
		if (ExteriorPlayerDetector.DetectedPlayers.Count > 0)
		{
			flag = true;
		}
		if (InteriorVehicleDetector.AreAnyVehiclesOccupied())
		{
			flag = true;
		}
		if (InteriorPlayerDetector.DetectedPlayers.Count > 0)
		{
			flag = true;
		}
		if (flag != base.IsOpen)
		{
			if (flag)
			{
				Open();
			}
			else
			{
				Close();
			}
		}
	}

	public void IntercomBuzzed()
	{
		SetIntercomActive(active: false);
	}

	public void SetEnterable(bool enterable)
	{
		ExteriorPlayerDetector.SetIgnoreNewCollisions(!enterable);
		ExteriorVehicleDetector.SetIgnoreNewCollisions(!enterable);
		ExteriorVehicleDetector.vehicles.Clear();
	}

	[Button]
	public void ActivateIntercom()
	{
		SetIntercomActive(active: true);
	}

	public void SetIntercomActive(bool active)
	{
		intercomActive = active;
		UpdateIntercom();
	}

	private void UpdateIntercom()
	{
		IntercomInt.SetInteractableState((!intercomActive) ? InteractableObject.EInteractableState.Disabled : InteractableObject.EInteractableState.Default);
		((Behaviour)IntercomLight).enabled = intercomActive;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EMap_002EManorGateAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EMap_002EManorGateAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EMap_002EManorGateAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EMap_002EManorGateAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
