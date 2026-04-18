using System;
using FishNet;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.NPCs.Behaviour;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules;

[Serializable]
public abstract class NPCAction : NetworkBehaviour
{
	public const int MAX_CONSECUTIVE_PATHING_FAILURES = 5;

	[SerializeField]
	protected int priority;

	[Header("Timing Settings")]
	public int StartTime;

	[Header("Umbrella Use")]
	[SerializeField]
	private bool _canUseUmbrella = true;

	protected NPC npc;

	protected NPCScheduleManager schedule;

	public Action onEnded;

	protected int consecutivePathingFailures;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCActionAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCActionAssembly_002DCSharp_002Edll_Excuted;

	protected string ActionName => "ActionName";

	public bool IsEvent => this is NPCEvent;

	public bool IsSignal => this is NPCSignal;

	public bool IsActive
	{
		get
		{
			if ((Object)(object)schedule != (Object)null)
			{
				return (Object)(object)schedule.ActiveAction == (Object)(object)this;
			}
			return false;
		}
	}

	public bool HasStarted { get; protected set; }

	public virtual int Priority => priority;

	protected NPCMovement movement => npc.Movement;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCAction_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void OnValidate()
	{
		((NetworkBehaviour)this).OnValidate();
		GetReferences();
	}

	private void GetReferences()
	{
		if ((Object)(object)npc == (Object)null)
		{
			npc = ((Component)this).GetComponentInParent<NPC>();
		}
		if ((Object)(object)schedule == (Object)null)
		{
			schedule = ((Component)this).GetComponentInParent<NPCScheduleManager>();
		}
	}

	protected virtual void Start()
	{
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPassed);
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPassed);
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPassed);
		}
	}

	public virtual void Started()
	{
		GetReferences();
		if (schedule.DEBUG_MODE)
		{
			Debug.Log((object)(GetName() + " started"));
		}
		schedule.ActiveAction = this;
		HasStarted = true;
		OnStart();
	}

	public virtual void LateStarted()
	{
		GetReferences();
		if (schedule.DEBUG_MODE)
		{
			Debug.Log((object)(GetName() + " late started"));
		}
		schedule.ActiveAction = this;
		HasStarted = true;
		OnStart();
	}

	public virtual void JumpTo()
	{
		GetReferences();
		if (schedule.DEBUG_MODE)
		{
			Debug.Log((object)(GetName() + " jumped to"));
		}
		schedule.ActiveAction = this;
		HasStarted = true;
		OnStart();
	}

	public virtual void End()
	{
		GetReferences();
		if (schedule.DEBUG_MODE)
		{
			Debug.Log((object)(GetName() + " ended"));
		}
		schedule.ActiveAction = null;
		HasStarted = false;
		if (onEnded != null)
		{
			onEnded();
		}
	}

	public virtual void Interrupt()
	{
		GetReferences();
		if (schedule.DEBUG_MODE)
		{
			Debug.Log((object)(GetName() + " interrupted"));
		}
		schedule.ActiveAction = null;
		if (!schedule.PendingActions.Contains(this))
		{
			schedule.PendingActions.Add(this);
		}
	}

	public virtual void Resume()
	{
		GetReferences();
		if (schedule.DEBUG_MODE)
		{
			Debug.Log((object)(GetName() + " resumed"));
		}
		schedule.ActiveAction = this;
		npc.Behaviour.GetBehaviour<ScheduleBehaviour>().SetCanUseUmbrellaDuringBehaviour(_canUseUmbrella);
		if (schedule.PendingActions.Contains(this))
		{
			schedule.PendingActions.Remove(this);
		}
	}

	public virtual void ResumeFailed()
	{
		GetReferences();
		if (schedule.DEBUG_MODE)
		{
			Debug.Log((object)(GetName() + " resume failed"));
		}
		HasStarted = false;
		if (schedule.PendingActions.Contains(this))
		{
			schedule.PendingActions.Remove(this);
		}
	}

	public virtual void Skipped()
	{
		GetReferences();
		if (schedule.DEBUG_MODE)
		{
			Debug.Log((object)(((Object)((Component)this).gameObject).name + " skipped"));
		}
		HasStarted = false;
	}

	public virtual void ActiveUpdate()
	{
	}

	public virtual void OnActiveTick()
	{
	}

	public virtual void OnActiveMinPass()
	{
	}

	public virtual void PendingMinPassed()
	{
		if (HasStarted && !IsActive && !NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, GetEndTime()))
		{
			ResumeFailed();
		}
	}

	public virtual void MinPassed()
	{
	}

	public virtual bool ShouldStart()
	{
		if (!((Component)this).gameObject.activeInHierarchy)
		{
			return false;
		}
		return true;
	}

	public abstract string GetName();

	public abstract string GetTimeDescription();

	public abstract int GetEndTime();

	protected unsafe void SetDestination(Vector3 position, bool teleportIfFail = true)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (InstanceFinder.IsServer)
		{
			if (teleportIfFail && consecutivePathingFailures >= 5 && !movement.CanGetTo(position))
			{
				Console.LogWarning(npc.fullName + " too many pathing failures. Warping to " + ((object)(*(Vector3*)(&position))/*cast due to .constrained prefix*/).ToString());
				movement.Warp(position);
				WalkCallback(NPCMovement.WalkResult.Success);
			}
			else
			{
				movement.SetDestination(position, WalkCallback);
			}
		}
	}

	protected virtual void WalkCallback(NPCMovement.WalkResult result)
	{
		if (IsActive)
		{
			if (result == NPCMovement.WalkResult.Failed)
			{
				consecutivePathingFailures++;
			}
			else
			{
				consecutivePathingFailures = 0;
			}
			if (schedule.DEBUG_MODE)
			{
				Console.Log("Walk callback result: " + result);
			}
		}
	}

	public virtual void SetStartTime(int startTime)
	{
		StartTime = startTime;
	}

	protected void SetCanUseUmbrella(bool canUse)
	{
		_canUseUmbrella = canUse;
		if (IsActive)
		{
			npc.Behaviour.GetBehaviour<ScheduleBehaviour>().SetCanUseUmbrellaDuringBehaviour(_canUseUmbrella);
		}
	}

	protected virtual void OnStart()
	{
		npc.Behaviour.GetBehaviour<ScheduleBehaviour>().SetCanUseUmbrellaDuringBehaviour(_canUseUmbrella);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCActionAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCActionAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCActionAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCActionAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCAction_Assembly_002DCSharp_002Edll()
	{
		GetReferences();
	}
}
