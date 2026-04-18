using System.Collections;
using FishNet;
using ScheduleOne.Map;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.AI;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules;

public class NPCSignal_DriveToCarPark : NPCSignal
{
	public ParkingLot ParkingLot;

	public LandVehicle Vehicle;

	[Header("Parking Settings")]
	public bool OverrideParkingType;

	public EParkingAlignment ParkingType;

	private bool isAtDestination;

	private float timeInVehicle;

	private float timeAtDestination;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_DriveToCarParkAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_DriveToCarParkAssembly_002DCSharp_002Edll_Excuted;

	public new string ActionName => "Drive to car park";

	public override string GetName()
	{
		if ((Object)(object)ParkingLot == (Object)null)
		{
			return ActionName + " (No Parking Lot)";
		}
		return ActionName + " (" + ((Object)((Component)ParkingLot).gameObject).name + ")";
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		priority = 12;
	}

	public override void Started()
	{
		base.Started();
		isAtDestination = false;
		CheckValidForStart();
	}

	public override void End()
	{
		base.End();
		if ((Object)(object)npc.CurrentVehicle != (Object)null)
		{
			npc.ExitVehicle();
		}
	}

	public override void LateStarted()
	{
		base.LateStarted();
		isAtDestination = false;
		CheckValidForStart();
	}

	private void CheckValidForStart()
	{
		if ((Object)(object)Vehicle.CurrentParkingLot == (Object)(object)ParkingLot)
		{
			End();
		}
	}

	public override void Interrupt()
	{
		base.Interrupt();
		Park();
		if (InstanceFinder.IsServer)
		{
			if (npc.IsInVehicle)
			{
				Vehicle.Agent.StopNavigating();
				npc.ExitVehicle();
			}
			else
			{
				npc.Movement.Stop();
			}
		}
	}

	public override void Resume()
	{
		base.Resume();
		isAtDestination = false;
		CheckValidForStart();
	}

	public override void Skipped()
	{
		base.Skipped();
		Park();
	}

	public override void ResumeFailed()
	{
		base.ResumeFailed();
		Park();
	}

	public override void JumpTo()
	{
		base.JumpTo();
		isAtDestination = false;
	}

	public override void OnActiveTick()
	{
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (npc.IsInVehicle)
		{
			timeInVehicle += 0.5f;
		}
		else
		{
			timeInVehicle = 0f;
		}
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (npc.IsInVehicle && (Object)(object)npc.CurrentVehicle.CurrentParkingLot == (Object)(object)ParkingLot)
		{
			timeAtDestination += 0.5f;
			if (timeAtDestination > 1f)
			{
				End();
			}
		}
		else
		{
			timeAtDestination = 0f;
		}
		if (isAtDestination)
		{
			return;
		}
		if (npc.IsInVehicle)
		{
			if (Vehicle.isParked)
			{
				if (timeInVehicle > 1f)
				{
					Vehicle.ExitPark_Networked(null, Vehicle.CurrentParkingLot.UseExitPoint);
				}
			}
			else if (!Vehicle.Agent.AutoDriving)
			{
				Vehicle.Agent.Navigate(ParkingLot.EntryPoint.position, null, DriveCallback);
			}
		}
		else if ((!npc.Movement.IsMoving || Vector3.Distance(npc.Movement.CurrentDestination, GetWalkDestination()) > 1f) && npc.Movement.CanMove())
		{
			if (npc.Movement.CanGetTo(GetWalkDestination(), 2f))
			{
				SetDestination(GetWalkDestination());
				return;
			}
			npc.EnterVehicle(null, Vehicle);
			Console.LogWarning("NPC " + ((Object)npc).name + " was unable to reach vehicle " + ((Object)Vehicle).name + " and was teleported to it.");
			Debug.DrawLine(((Component)npc).transform.position, GetWalkDestination(), Color.red, 10f);
		}
	}

	protected override void WalkCallback(NPCMovement.WalkResult result)
	{
		base.WalkCallback(result);
		if (base.IsActive && InstanceFinder.IsServer && (result == NPCMovement.WalkResult.Success || result == NPCMovement.WalkResult.Partial))
		{
			npc.EnterVehicle(null, Vehicle);
		}
	}

	private Vector3 GetWalkDestination()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (!Vehicle.IsVisible && (Object)(object)Vehicle.CurrentParkingLot != (Object)null && (Object)(object)Vehicle.CurrentParkingLot.HiddenVehicleAccessPoint != (Object)null)
		{
			return Vehicle.CurrentParkingLot.HiddenVehicleAccessPoint.position;
		}
		return Vehicle.driverEntryPoint.position;
	}

	private void DriveCallback(VehicleAgent.ENavigationResult result)
	{
		if (base.IsActive)
		{
			isAtDestination = true;
			if (InstanceFinder.IsServer)
			{
				Park();
				((MonoBehaviour)this).StartCoroutine(Wait());
			}
		}
		IEnumerator Wait()
		{
			yield return (object)new WaitForSeconds(1f);
			End();
		}
	}

	private void Park()
	{
		if (InstanceFinder.IsServer)
		{
			int randomFreeSpotIndex = ParkingLot.GetRandomFreeSpotIndex();
			EParkingAlignment alignment = EParkingAlignment.FrontToKerb;
			if (randomFreeSpotIndex != -1)
			{
				alignment = (OverrideParkingType ? ParkingType : ParkingLot.ParkingSpots[randomFreeSpotIndex].Alignment);
			}
			Vehicle.Park(null, new ParkData
			{
				lotGUID = ParkingLot.GUID,
				alignment = alignment,
				spotIndex = randomFreeSpotIndex
			}, network: true);
		}
	}

	private EParkingAlignment GetParkingType()
	{
		if (OverrideParkingType)
		{
			return ParkingType;
		}
		return ParkingLot.GetRandomFreeSpot().Alignment;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_DriveToCarParkAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_DriveToCarParkAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_DriveToCarParkAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_DriveToCarParkAssembly_002DCSharp_002Edll_Excuted = true;
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
