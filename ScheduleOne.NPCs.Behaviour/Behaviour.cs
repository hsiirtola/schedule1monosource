using FishNet;
using FishNet.Connection;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Behaviour;

public class Behaviour : NetworkBehaviour
{
	public const int MAX_CONSECUTIVE_PATHING_FAILURES = 5;

	public bool EnabledOnAwake;

	[Header("Settings")]
	public string Name = "Behaviour";

	[Tooltip("Behaviour priority; higher = takes priority over lower number behaviour")]
	public int Priority;

	[Header("Umbrella")]
	[SerializeField]
	private bool _canUseUmbrellaDuringBehaviour;

	[HideInInspector]
	public int BehaviourIndex = -1;

	public UnityEvent onEnable = new UnityEvent();

	public UnityEvent onDisable = new UnityEvent();

	public UnityEvent onBegin;

	public UnityEvent onEnd;

	protected int consecutivePathingFailures;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public bool Enabled { get; protected set; }

	public bool Started { get; private set; }

	public bool Active { get; private set; }

	public NPCBehaviour beh { get; private set; }

	public NPC Npc => beh.Npc;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void OnValidate()
	{
		((NetworkBehaviour)this).OnValidate();
		UpdateGameObjectName();
	}

	public virtual void Enable()
	{
		if (Npc.Behaviour.DEBUG_MODE)
		{
			Debug.Log((object)(Name + " enabled"));
		}
		Enabled = true;
		if (onEnable != null)
		{
			onEnable.Invoke();
		}
	}

	public void Enable_Server()
	{
		beh.EnableBehaviour_Server(BehaviourIndex);
	}

	public void Enable_Networked()
	{
		if (InstanceFinder.IsServer)
		{
			Enable_Server();
		}
		else
		{
			Enable();
		}
	}

	public virtual void Disable()
	{
		if (Npc.Behaviour.DEBUG_MODE)
		{
			Debug.Log((object)(Name + " disabled"));
		}
		Enabled = false;
		Started = false;
		if (Active)
		{
			Deactivate();
		}
		if (onDisable != null)
		{
			onDisable.Invoke();
		}
	}

	public void Disable_Server()
	{
		beh.DisableBehaviour_Server(BehaviourIndex);
	}

	public void Disable_Networked(NetworkConnection conn)
	{
		if (InstanceFinder.IsServer)
		{
			Disable_Server();
		}
		else
		{
			Disable();
		}
	}

	public void Activate_Server(NetworkConnection conn)
	{
		beh.ActivateBehaviour_Server(BehaviourIndex);
	}

	public virtual void Activate()
	{
		if (beh.DEBUG_MODE)
		{
			Console.Log("Behaviour (" + Name + ") started");
		}
		Started = true;
		Active = true;
		beh.activeBehaviour = this;
		Npc.Actions.SetCanUseUmbrella(_canUseUmbrellaDuringBehaviour);
		UpdateGameObjectName();
		if (onBegin != null)
		{
			onBegin.Invoke();
		}
	}

	public void Deactivate_Server()
	{
		beh.DeactivateBehaviour_Server(BehaviourIndex);
	}

	public void Deactivate_Networked(NetworkConnection conn)
	{
		if (InstanceFinder.IsServer)
		{
			Deactivate_Server();
		}
		else
		{
			Deactivate();
		}
	}

	public virtual void Deactivate()
	{
		if (beh.DEBUG_MODE)
		{
			Console.Log("Behaviour (" + Name + ") ended");
		}
		Active = false;
		if ((Object)(object)beh.activeBehaviour == (Object)(object)this)
		{
			beh.activeBehaviour = null;
		}
		UpdateGameObjectName();
		if (onEnd != null)
		{
			onEnd.Invoke();
		}
	}

	public void Pause_Server()
	{
		beh.PauseBehaviour_Server(BehaviourIndex);
	}

	public virtual void Pause()
	{
		if (beh.DEBUG_MODE)
		{
			Console.Log("Behaviour (" + Name + ") paused");
		}
		Active = false;
		if ((Object)(object)beh.activeBehaviour == (Object)(object)this)
		{
			beh.activeBehaviour = null;
		}
		UpdateGameObjectName();
	}

	public void Resume_Server()
	{
		beh.ResumeBehaviour_Server(BehaviourIndex);
	}

	public virtual void Resume()
	{
		if (beh.DEBUG_MODE)
		{
			Console.Log("Behaviour (" + Name + ") resumed");
		}
		Active = true;
		beh.activeBehaviour = this;
		Npc.Actions.SetCanUseUmbrella(_canUseUmbrellaDuringBehaviour);
		UpdateGameObjectName();
	}

	public virtual void BehaviourUpdate()
	{
	}

	public virtual void BehaviourLateUpdate()
	{
	}

	public virtual void OnActiveTick()
	{
	}

	public virtual void OnActiveUncappedMinutePass()
	{
	}

	protected void SetDestination(ITransitEntity transitEntity, bool teleportIfFail = true)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		SetDestination(NavMeshUtility.GetReachableAccessPoint(transitEntity, Npc).position, teleportIfFail);
	}

	protected unsafe virtual void SetDestination(Vector3 position, bool teleportIfFail = true, float successThreshold = 1f)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (InstanceFinder.IsServer)
		{
			if (teleportIfFail && consecutivePathingFailures >= 5 && !Npc.Movement.CanGetTo(position))
			{
				Console.LogWarning(Npc.fullName + " too many pathing failures. Warping to " + ((object)(*(Vector3*)(&position))/*cast due to .constrained prefix*/).ToString());
				NavMeshUtility.SamplePosition(position, out var hit, 5f, -1);
				position = ((NavMeshHit)(ref hit)).position;
				Npc.Movement.Warp(position);
				WalkCallback(NPCMovement.WalkResult.Success);
			}
			Npc.Movement.SetDestination(position, WalkCallback, successThreshold, 0.1f);
		}
	}

	protected virtual void WalkCallback(NPCMovement.WalkResult result)
	{
		if (Active)
		{
			if (result == NPCMovement.WalkResult.Failed)
			{
				consecutivePathingFailures++;
			}
			else
			{
				consecutivePathingFailures = 0;
			}
			if (beh.DEBUG_MODE)
			{
				Console.Log("Walk callback result: " + result);
			}
		}
	}

	private void UpdateGameObjectName()
	{
	}

	public void SetCanUseUmbrellaDuringBehaviour(bool canUse)
	{
		_canUseUmbrellaDuringBehaviour = canUse;
		if (Active)
		{
			Npc.Actions.SetCanUseUmbrella(_canUseUmbrellaDuringBehaviour);
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EBehaviour_Assembly_002DCSharp_002Edll()
	{
		beh = ((Component)this).GetComponentInParent<NPCBehaviour>();
		Enabled = EnabledOnAwake;
	}
}
