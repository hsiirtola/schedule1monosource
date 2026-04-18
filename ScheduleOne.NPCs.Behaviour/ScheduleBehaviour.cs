using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class ScheduleBehaviour : Behaviour
{
	[Header("References")]
	public NPCScheduleManager schedule;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EScheduleBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EScheduleBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EScheduleBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void Activate()
	{
		base.Activate();
		schedule.EnableSchedule();
	}

	public override void Resume()
	{
		base.Resume();
		schedule.EnableSchedule();
	}

	public override void Pause()
	{
		base.Pause();
		schedule.DisableSchedule();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		schedule.DisableSchedule();
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EScheduleBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EScheduleBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EScheduleBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EScheduleBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EScheduleBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
	}
}
