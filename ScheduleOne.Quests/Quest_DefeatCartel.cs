using System;
using FishNet;
using ScheduleOne.Audio;
using ScheduleOne.Cartel;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.Property;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Quests;

public class Quest_DefeatCartel : Quest
{
	public const float DIG_TUNNEL_COST = 10000f;

	[Header("References")]
	public Sam Sam;

	public Manor Manor;

	public QuestEntry DigTunnelEntry;

	public QuestEntry WaitForTunnelEntry;

	public QuestEntry EnquireAboutRDXEntry;

	public QuestEntry ObtainRDXEntry;

	public QuestEntry EnquireAboutBombEntry;

	public QuestEntry KillBanditEntry;

	public NPC Bandit;

	public GameObject BanditScheduleContainer;

	protected override void Start()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		base.Start();
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepEnd = (Action)Delegate.Combine(instance.onSleepEnd, new Action(OnSleepEnd));
		onInitialComplete.AddListener(new UnityAction(Defeat));
	}

	private void OnSleepEnd()
	{
		if (InstanceFinder.IsServer)
		{
			if (base.State == EQuestState.Inactive && Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(EMapRegion.Uptown).IsUnlocked && NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Status == ECartelStatus.Hostile)
			{
				Begin();
			}
			if (WaitForTunnelEntry.State == EQuestState.Active)
			{
				WaitForTunnelEntry.Complete();
				Sam.SendTunnelDugMessage();
			}
		}
	}

	protected override void OnUncappedMinPass()
	{
		base.OnUncappedMinPass();
		BanditScheduleContainer.gameObject.SetActive(KillBanditEntry.State == EQuestState.Active);
		if (KillBanditEntry.State == EQuestState.Active && Bandit.Health.IsDead)
		{
			KillBanditEntry.Complete();
		}
	}

	public override void SetQuestEntryState(int entryIndex, EQuestState state, bool network = true)
	{
		base.SetQuestEntryState(entryIndex, state, network);
		if ((Object)(object)Entries[entryIndex] == (Object)(object)WaitForTunnelEntry && state == EQuestState.Completed)
		{
			Manor.SetTunnelDug(null, dug: true);
		}
	}

	public void PlayCountdownMusic()
	{
		Singleton<MusicManager>.Instance.SetTrackEnabled("Explosion countdown", enabled: true);
	}

	private void Defeat()
	{
		NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.SetStatus(null, ECartelStatus.Defeated, resetStatusChangeTimer: true);
	}

	public override void SetQuestState(EQuestState state, bool network = true)
	{
		base.SetQuestState(state, network);
		if (state == EQuestState.Completed)
		{
			NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.SetStatus(null, ECartelStatus.Defeated, resetStatusChangeTimer: true);
			AchievementManager.UnlockAchievement(AchievementManager.EAchievement.FINISHING_THE_JOB);
		}
	}
}
