using ScheduleOne.VoiceOver;

namespace ScheduleOne.NPCs.Behaviour;

public class CoweringBehaviour : Behaviour
{
	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECoweringBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECoweringBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public override void Activate()
	{
		base.Activate();
		SetCowering(cowering: true);
	}

	public override void Enable()
	{
		base.Enable();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		SetCowering(cowering: false);
	}

	public override void Resume()
	{
		base.Resume();
		SetCowering(cowering: true);
	}

	public override void Pause()
	{
		base.Pause();
		SetCowering(cowering: false);
	}

	public override void Disable()
	{
		base.Disable();
		Deactivate();
	}

	private void SetCowering(bool cowering)
	{
		base.Npc.Avatar.Animation.SetCrouched(cowering);
		base.Npc.Avatar.Animation.SetBool("HandsUp", cowering);
		if (cowering)
		{
			base.Npc.PlayVO(EVOLineType.Scared);
			base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("cowering", 80, 0f));
		}
		else
		{
			base.Npc.Movement.SpeedController.RemoveSpeedControl("cowering");
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECoweringBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECoweringBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECoweringBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECoweringBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
