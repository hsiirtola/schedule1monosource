using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules;

public class NPCEvent : NPCAction
{
	public int Duration = 60;

	public int EndTime;

	private bool _forgotUmbrella;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEventAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEventAssembly_002DCSharp_002Edll_Excuted;

	public new string ActionName => "Event";

	[Button]
	public void ApplyDuration()
	{
		Debug.Log((object)"Applying duration");
		EndTime = TimeManager.AddMinutesTo24HourTime(StartTime, Duration);
		((Component)this).GetComponentInParent<NPCScheduleManager>().InitializeActions();
	}

	[Button]
	public void ApplyEndTime()
	{
		if (EndTime > StartTime)
		{
			Debug.Log((object)"Set duration");
			Duration = TimeManager.GetMinSumFrom24HourTime(EndTime) - TimeManager.GetMinSumFrom24HourTime(StartTime);
		}
		else
		{
			Debug.Log((object)"Set duration");
			Duration = 1440 - TimeManager.GetMinSumFrom24HourTime(StartTime) + TimeManager.GetMinSumFrom24HourTime(EndTime);
		}
		((Component)this).GetComponentInParent<NPCScheduleManager>().InitializeActions();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_forgotUmbrella = false;
	}

	public override void OnActiveMinPass()
	{
		base.OnActiveMinPass();
		if (NetworkSingleton<TimeManager>.Instance.CurrentTime == GetEndTime())
		{
			End();
		}
	}

	public override void OnActiveTick()
	{
		base.OnActiveTick();
		if (NetworkSingleton<TimeManager>.Instance.CurrentTime == GetEndTime())
		{
			End();
		}
	}

	public override void PendingMinPassed()
	{
		base.PendingMinPassed();
	}

	public override string GetName()
	{
		return ActionName;
	}

	public override string GetTimeDescription()
	{
		return TimeManager.Get12HourTime(StartTime) + " - " + TimeManager.Get12HourTime(GetEndTime());
	}

	public override int GetEndTime()
	{
		return TimeManager.AddMinutesTo24HourTime(StartTime, Duration);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEventAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEventAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEventAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEventAssembly_002DCSharp_002Edll_Excuted = true;
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
