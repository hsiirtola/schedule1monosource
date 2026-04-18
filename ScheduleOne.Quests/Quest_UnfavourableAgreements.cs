using System;
using System.Collections;
using FishNet;
using ScheduleOne.Cartel;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.AI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Quests;

public class Quest_UnfavourableAgreements : Quest
{
	public LandVehicle MeetingVehicle;

	public ParkingLot MeetingParkingLot;

	public ParkingLot ManorParkingLot;

	public Thomas Thomas;

	public QuestEntry ReadMessageQuestEntry;

	public QuestEntry MeetingQuestEntry;

	public Quest PrereqQuest;

	public UnityEvent onMeetingConcluded;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		base.Start();
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepEnd = (Action)Delegate.Combine(instance.onSleepEnd, new Action(CheckQuestStart));
		Thomas.onMeetingEnded.AddListener(new UnityAction(MeetingEnded));
	}

	private void CheckQuestStart()
	{
		if (!InstanceFinder.IsServer || base.State != EQuestState.Inactive || NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Status != ECartelStatus.Unknown)
		{
			return;
		}
		int num = 0;
		foreach (Customer lockedCustomer in Customer.LockedCustomers)
		{
			if (lockedCustomer.NPC.Region == EMapRegion.Westville)
			{
				num++;
			}
		}
		int num2 = 0;
		foreach (Customer unlockedCustomer in Customer.UnlockedCustomers)
		{
			if (unlockedCustomer.NPC.Region == EMapRegion.Westville)
			{
				num2++;
			}
		}
		if ((num2 >= 5 && PrereqQuest.State == EQuestState.Completed) || num == 0)
		{
			Begin();
		}
	}

	public override void Begin(bool network = true)
	{
		base.Begin(network);
		Thomas.SendIntroMessage();
	}

	protected override void OnUncappedMinPass()
	{
		base.OnUncappedMinPass();
		if (ReadMessageQuestEntry.State == EQuestState.Active && Thomas.MSGConversation.Read)
		{
			ReadMessageQuestEntry.Complete();
		}
		if (base.State == EQuestState.Active && NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Status != ECartelStatus.Unknown)
		{
			MeetingQuestEntry.Complete();
		}
	}

	public override void SetQuestState(EQuestState state, bool network = true)
	{
		base.SetQuestState(state, network);
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (state == EQuestState.Active)
		{
			if (MeetingVehicle.isParked)
			{
				MeetingVehicle.ExitPark_Networked(null);
			}
			MeetingVehicle.Park(null, new ParkData(MeetingParkingLot.GUID, 0, MeetingParkingLot.ParkingSpots[0].Alignment), network: true);
		}
		if (state == EQuestState.Completed)
		{
			Thomas.MSGConversation.SetIsKnown(known: true);
		}
	}

	private void MeetingEnded()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		Thomas.onMeetingEnded.RemoveListener(new UnityAction(MeetingEnded));
		if (onMeetingConcluded != null)
		{
			onMeetingConcluded.Invoke();
		}
		if (InstanceFinder.IsServer)
		{
			((MonoBehaviour)this).StartCoroutine(Wait());
		}
		IEnumerator Wait()
		{
			yield return (object)new WaitForSeconds(2f);
			Thomas.EnterVehicle(null, MeetingVehicle);
			MeetingVehicle.ExitPark_Networked(null, moveToExitPoint: false);
			MeetingVehicle.Agent.Navigate(ManorParkingLot.EntryPoint.position, null, DriveCallback);
		}
	}

	private void DriveCallback(VehicleAgent.ENavigationResult result)
	{
		if (InstanceFinder.IsServer)
		{
			Park();
		}
	}

	private void Park()
	{
		if (InstanceFinder.IsServer)
		{
			MeetingVehicle.Park(null, new ParkData
			{
				alignment = EParkingAlignment.RearToKerb,
				lotGUID = ManorParkingLot.GUID,
				spotIndex = 0
			}, network: true);
		}
	}
}
