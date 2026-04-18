using ScheduleOne.Cartel;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules;

public class NPCEvent_CartelGoonExit : NPCEvent_StayInBuilding
{
	public CartelGoon Goon;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_CartelGoonExitAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_CartelGoonExitAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_CartelGoonExit_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void Started()
	{
		FindExitBuilding();
		base.Started();
	}

	public override void LateStarted()
	{
		FindExitBuilding();
		base.LateStarted();
	}

	public override void JumpTo()
	{
		FindExitBuilding();
		base.JumpTo();
	}

	public override void Resume()
	{
		FindExitBuilding();
		base.Resume();
	}

	private void FindExitBuilding()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Goon.GoonPool == (Object)null))
		{
			Building = Goon.GoonPool.GetNearestExitBuilding(Goon.Avatar.CenterPoint);
		}
	}

	protected override void EnterBuilding(int doorIndex)
	{
		Goon.Despawn();
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_CartelGoonExitAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_CartelGoonExitAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_CartelGoonExitAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_CartelGoonExitAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_CartelGoonExit_Assembly_002DCSharp_002Edll()
	{
	}
}
