using FishNet;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Quests;

public class Quest_CleanCash : Quest
{
	public QuestEntry BuyBusinessEntry;

	public QuestEntry GoToBusinessEntry;

	protected override void OnUncappedMinPass()
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		base.OnUncappedMinPass();
		if (base.State == EQuestState.Inactive && InstanceFinder.IsServer && ATM.WeeklyDepositSum >= 10000f)
		{
			Begin();
		}
		if (base.State == EQuestState.Completed)
		{
			return;
		}
		if (InstanceFinder.IsServer && BuyBusinessEntry.State == EQuestState.Active && Business.OwnedBusinesses.Count > 0)
		{
			BuyBusinessEntry.Complete();
		}
		if (GoToBusinessEntry.State == EQuestState.Active)
		{
			if (Business.OwnedBusinesses.Count > 0)
			{
				((Component)GoToBusinessEntry).transform.position = ((Component)Business.OwnedBusinesses[0].PoI).transform.position;
			}
			if ((Object)(object)Player.Local.CurrentBusiness != (Object)null)
			{
				GoToBusinessEntry.Complete();
			}
		}
	}
}
