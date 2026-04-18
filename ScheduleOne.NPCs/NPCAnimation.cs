using System;
using FishNet.Object;
using UnityEngine;

namespace ScheduleOne.NPCs;

public class NPCAnimation : NetworkBehaviour
{
	public bool DEBUG;

	protected NPC npc;

	[Header("Settings")]
	public AnimationCurve WalkMapCurve;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCAnimationAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCAnimationAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCAnimation_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected virtual void LateUpdate()
	{
		if (((Behaviour)npc.Avatar.Animation).enabled && ((Component)npc.Avatar.Animation).gameObject.activeSelf && !npc.Avatar.Animation.IsAvatarCulled && npc.isVisible)
		{
			UpdateMovementAnimation();
		}
	}

	protected virtual void UpdateMovementAnimation()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)npc.Avatar).transform.InverseTransformVector(npc.Velocity) / 8f;
		if (DEBUG)
		{
			Debug.Log((object)("Animation velocity:" + ((Vector3)(ref val)).ToString("F3")));
		}
		npc.Avatar.Animation.SetDirection(WalkMapCurve.Evaluate(Mathf.Abs(val.z)) * Mathf.Sign(val.z));
		npc.Avatar.Animation.SetStrafe(WalkMapCurve.Evaluate(Mathf.Abs(val.x)) * Mathf.Sign(val.x));
		npc.Avatar.Animation.SetBool("IsClimbing", Mathf.Abs(npc.Movement.CurrentLadderSpeed) > 0.01f);
		npc.Avatar.Animation.animator.SetFloat("ClimbSpeed", npc.Movement.CurrentLadderSpeed);
	}

	public virtual void SetRagdollActive(bool active)
	{
		npc.Avatar.SetRagdollPhysicsEnabled(active);
	}

	public void StandupStart()
	{
		npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("ragdollstandup", 100, 0f));
	}

	public void StandupDone()
	{
		npc.Movement.SpeedController.RemoveSpeedControl("ragdollstandup");
	}

	private void OnNPCVisibilityChanged(bool visible)
	{
		if (!visible)
		{
			npc.Avatar.Animation.ResetAnimatorState();
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCAnimationAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCAnimationAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCAnimationAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCAnimationAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCAnimation_Assembly_002DCSharp_002Edll()
	{
		npc = ((Component)this).GetComponent<NPC>();
		NPC nPC = npc;
		nPC.onVisibilityChanged = (Action<bool>)Delegate.Combine(nPC.onVisibilityChanged, new Action<bool>(OnNPCVisibilityChanged));
	}
}
