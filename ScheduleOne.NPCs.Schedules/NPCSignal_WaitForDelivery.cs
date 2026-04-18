using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.Quests;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules;

public class NPCSignal_WaitForDelivery : NPCSignal
{
	public const float DestinationThreshold = 1.5f;

	public const float WalkSpeedMultiplier = 1.25f;

	private Contract contract;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDeliveryAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDeliveryAssembly_002DCSharp_002Edll_Excuted;

	public new string ActionName => "Wait for delivery";

	private DeliveryLocation Location => contract.DeliveryLocation;

	public void SetContract(Contract contract)
	{
		this.contract = contract;
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDelivery_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		priority = 100;
	}

	public override string GetName()
	{
		return ActionName;
	}

	public override void Started()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		base.Started();
		if ((Object)(object)contract == (Object)null)
		{
			Debug.LogError((object)"NPCSignal_WaitForDelivery: Contract is not set before starting the signal.");
			return;
		}
		((Component)npc).GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: true);
		npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("NPCSignal_WaitForDelivery", 5, npc.Movement.SpeedController.DefaultWalkSpeed * 1.25f));
		SetDestination(Location.CustomerStandPoint.position);
		EnsureNPCHasEnoughCash();
	}

	public override void LateStarted()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		base.LateStarted();
		if ((Object)(object)contract == (Object)null)
		{
			Debug.LogError((object)"NPCSignal_WaitForDelivery: Contract is not set before starting the signal.");
			return;
		}
		((Component)npc).GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: true);
		npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("NPCSignal_WaitForDelivery", 5, npc.Movement.SpeedController.DefaultWalkSpeed * 1.25f));
		if (InstanceFinder.IsServer)
		{
			SetDestination(Location.CustomerStandPoint.position);
			EnsureNPCHasEnoughCash();
		}
	}

	public override void JumpTo()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		base.JumpTo();
		if ((Object)(object)contract == (Object)null)
		{
			Debug.LogError((object)"NPCSignal_WaitForDelivery: Contract is not set before starting the signal.");
			return;
		}
		((Component)npc).GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: true);
		npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("NPCSignal_WaitForDelivery", 5, npc.Movement.SpeedController.DefaultWalkSpeed * 1.25f));
		if (InstanceFinder.IsServer)
		{
			npc.Movement.Warp(Location.CustomerStandPoint.position);
			npc.Movement.FaceDirection(Location.CustomerStandPoint.forward);
			EnsureNPCHasEnoughCash();
		}
	}

	private void EnsureNPCHasEnoughCash()
	{
		if (InstanceFinder.IsServer && npc.Inventory.GetCashInInventory() < contract.Payment)
		{
			npc.Inventory.AddCash(contract.Payment);
		}
	}

	public override void OnActiveTick()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		CheckWarp();
		if (!npc.Movement.IsMoving)
		{
			if (!IsAtDestination())
			{
				SetDestination(Location.CustomerStandPoint.position);
			}
			else
			{
				((Component)npc).GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: true);
			}
		}
		else if (Vector3.Distance(npc.Movement.CurrentDestination, Location.CustomerStandPoint.position) > 1.5f)
		{
			SetDestination(Location.CustomerStandPoint.position);
		}
	}

	private void CheckWarp()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (InstanceFinder.IsServer && !IsAtDestination())
		{
			Customer.GetContractTimings(contract.DeliveryWindow, out var _, out var hardStartTime, out var endTime);
			if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(hardStartTime, endTime) && Vector3.Distance(npc.Avatar.CenterPoint, Location.CustomerStandPoint.position) > Vector3.Distance(Location.TeleportPoint.position, Location.CustomerStandPoint.position) * 2f)
			{
				npc.Movement.Warp(Location.TeleportPoint.position);
				Console.Log(npc.fullName + " was warped to delivery location");
			}
		}
	}

	public override void Interrupt()
	{
		base.Interrupt();
		((Component)npc).GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: false);
		npc.Movement.SpeedController.RemoveSpeedControl("NPCSignal_WaitForDelivery");
		npc.Movement.Stop();
	}

	public override void Resume()
	{
		base.Resume();
		npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("NPCSignal_WaitForDelivery", 5, npc.Movement.SpeedController.DefaultWalkSpeed * 1.25f));
		((Component)npc).GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: true);
	}

	public override void End()
	{
		((Component)npc).GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: false);
		npc.Movement.SpeedController.RemoveSpeedControl("NPCSignal_WaitForDelivery");
		base.StartedThisCycle = false;
		base.End();
	}

	public override void Skipped()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		base.Skipped();
		if (InstanceFinder.IsServer)
		{
			npc.Movement.Warp(Location.CustomerStandPoint.position);
		}
	}

	private bool IsAtDestination()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(npc.Movement.FootPosition, Location.CustomerStandPoint.position) < 1.5f;
	}

	protected override void WalkCallback(NPCMovement.WalkResult result)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		base.WalkCallback(result);
		if (base.IsActive && result == NPCMovement.WalkResult.Success)
		{
			npc.Movement.FaceDirection(Location.CustomerStandPoint.forward);
			((Component)npc).GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: true);
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDeliveryAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDeliveryAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDeliveryAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDeliveryAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDelivery_Assembly_002DCSharp_002Edll()
	{
		((NPCAction)this).Awake();
		priority = 1000;
		MaxDuration = 720;
	}
}
