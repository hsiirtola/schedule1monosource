using FishNet;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class IdleBehaviour : Behaviour
{
	public Transform IdlePoint;

	private bool facingDir;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EIdleBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EIdleBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public override void Activate()
	{
		base.Activate();
	}

	public override void Resume()
	{
		base.Resume();
	}

	public override void OnActiveTick()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (!InstanceFinder.IsServer || (Object)(object)IdlePoint == (Object)null)
		{
			return;
		}
		if (!base.Npc.Movement.IsMoving)
		{
			if (IsAtIdleLocation())
			{
				if (!facingDir)
				{
					facingDir = true;
					base.Npc.Movement.FaceDirection(IdlePoint.forward);
				}
			}
			else
			{
				facingDir = false;
				SetDestination(IdlePoint.position);
			}
		}
		else
		{
			facingDir = false;
		}
	}

	public override void Pause()
	{
		base.Pause();
		facingDir = false;
		if (InstanceFinder.IsServer)
		{
			base.Npc.Movement.Stop();
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		facingDir = false;
		if (InstanceFinder.IsServer)
		{
			base.Npc.Movement.Stop();
		}
	}

	public bool IsAtIdleLocation()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)IdlePoint == (Object)null)
		{
			return false;
		}
		return Vector3.Distance(base.Npc.Movement.FootPosition, IdlePoint.position) < 1f;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EIdleBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EIdleBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EIdleBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EIdleBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
