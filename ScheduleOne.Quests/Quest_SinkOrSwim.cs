using System;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Persistence;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using ScheduleOne.Vehicles;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Quests;

public class Quest_SinkOrSwim : Quest
{
	public const int DAYS_TO_COMPLETE = 4;

	public string QuestName = "Make at least $1,000 to pay off the sharks";

	public int NelsonCallTime = 1215;

	public Transform LoanSharkVehiclePosition;

	public GameObject LoanSharkGraves;

	protected override void Awake()
	{
		base.Awake();
		LoanSharkGraves.gameObject.SetActive(false);
	}

	protected override void Start()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		base.Start();
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onHourPass = (Action)Delegate.Combine(instance.onHourPass, new Action(HourPass));
		Singleton<SleepCanvas>.Instance.onSleepEndFade.AddListener(new UnityAction(SleepStart));
		Singleton<SleepCanvas>.Instance.onSleepEndFade.AddListener(new UnityAction(CheckArrival));
		Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(UpdateName));
		Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(UpdateName));
	}

	private void HourPass()
	{
		if (Entries[0].State == EQuestState.Active)
		{
			UpdateName();
		}
	}

	private void SleepStart()
	{
		if (Entries[0].State == EQuestState.Active)
		{
			float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Days_Since_Tutorial_Completed");
			int num = 4 - (int)value;
			switch (num)
			{
			case -1:
				Singleton<SleepCanvas>.Instance.QueueSleepMessage("In the midst of night gunshots ring out nearby, but you are not the target. The rest of the night is quiet.", 5f);
				break;
			case 0:
				Singleton<SleepCanvas>.Instance.QueueSleepMessage("The loan sharks are arriving tonight.", 4f);
				break;
			case 1:
				Singleton<SleepCanvas>.Instance.QueueSleepMessage(num + " day until the loan sharks arrive");
				break;
			default:
				Singleton<SleepCanvas>.Instance.QueueSleepMessage(num + " days until the loan sharks arrive");
				break;
			}
		}
	}

	private void SpawnLoanSharkVehicle()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		NetworkSingleton<VehicleManager>.Instance.SpawnLoanSharkVehicle(LoanSharkVehiclePosition.position, LoanSharkVehiclePosition.rotation);
	}

	private void CheckArrival()
	{
		if (InstanceFinder.IsServer && !NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("Loan_Sharks_Arrived") && NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Days_Since_Tutorial_Completed") > 4f)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Loan_Sharks_Arrived", true.ToString());
			SpawnLoanSharkVehicle();
			LoanSharkGraves.gameObject.SetActive(true);
			Entries[Entries.Count - 1].SetState(EQuestState.Completed);
		}
	}

	public override void SetQuestState(EQuestState state, bool network = true)
	{
		base.SetQuestState(state, network);
		LoanSharkGraves.gameObject.SetActive(state == EQuestState.Completed);
	}

	private void UpdateName()
	{
		float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Days_Since_Tutorial_Completed");
		int num = 4 - (int)value;
		string text = num switch
		{
			-1 => string.Empty, 
			0 => "(arriving tonight)", 
			1 => "(" + num + " day remaining)", 
			_ => "(" + num + " days remaining)", 
		};
		Entries[0].SetEntryTitle(QuestName + " " + text);
	}
}
