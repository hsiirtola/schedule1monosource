using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class RagdollBehaviour : Behaviour
{
	public bool Seizure;

	public float SeizureForce = 1f;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ERagdollBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ERagdollBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private void Start()
	{
		((MonoBehaviour)this).InvokeRepeating("InfrequentUpdate", 0f, 0.1f);
	}

	private void InfrequentUpdate()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (Seizure)
		{
			Rigidbody[] ragdollRBs = base.Npc.Avatar.RagdollRBs;
			for (int i = 0; i < ragdollRBs.Length; i++)
			{
				ragdollRBs[i].AddForce(Random.insideUnitSphere * SeizureForce, (ForceMode)5);
			}
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ERagdollBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ERagdollBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ERagdollBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ERagdollBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
