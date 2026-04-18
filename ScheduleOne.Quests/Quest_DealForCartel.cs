using ScheduleOne.Cartel;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.Quests;

public class Quest_DealForCartel : Quest
{
	public QuestEntry MainEntry;

	public QuestEntry EndTruceEntry;

	private CartelDealInfo dealInfo;

	public void Initialize(CartelDealInfo dealInfo)
	{
		if (dealInfo == null)
		{
			Debug.LogError((object)"Quest_DealForCartel: Cannot initialize with null dealInfo.");
			return;
		}
		ItemDefinition item = Registry.GetItem(dealInfo.RequestedProductID);
		if ((Object)(object)item == (Object)null)
		{
			Debug.LogError((object)("Quest_DealForCartel: Requested product not found in registry: " + dealInfo.RequestedProductID));
			return;
		}
		this.dealInfo = dealInfo;
		MainEntry.SetEntryTitle("Deliver " + dealInfo.RequestedProductQuantity + "x " + ((BaseItemDefinition)item).Name + " to Hyland Manor");
		MainEntry.SetState(EQuestState.Active);
		GameDateTime copy = dealInfo.DueTime.GetCopy();
		copy.elapsedDays++;
		ConfigureExpiry(expires: true, copy);
		Begin();
		SetIsTracked(tracked: true);
	}

	public override void Begin(bool network = true)
	{
		base.Begin(network);
		if ((Object)(object)journalEntry == (Object)null)
		{
			SetupJournalEntry();
		}
		if ((Object)(object)base.hudUI == (Object)null)
		{
			SetupHUDUI();
		}
	}

	protected override void OnUncappedMinPass()
	{
		base.OnUncappedMinPass();
		if (base.State == EQuestState.Active)
		{
			UpdateTimingLabel();
		}
	}

	private void UpdateTimingLabel()
	{
		int num = dealInfo.DueTime.GetMinSum() - NetworkSingleton<TimeManager>.Instance.GetDateTime().GetMinSum();
		bool flag = num < 0;
		string text = "\n";
		text = (flag ? (text + "<color=#FF5555>") : ((num >= 1440) ? (text + "<color=green>") : (text + "<color=#F7B119>")));
		text += "(";
		if (flag)
		{
			num += 1440;
			text += "Overdue, ";
		}
		int num2 = num / 1440;
		int num3 = num % 1440 / 60;
		if (num2 > 0)
		{
			text = text + num2 + " day" + ((num2 > 1) ? "s" : "") + ", ";
		}
		if (num2 == 0)
		{
			num3 = Mathf.Max(num3, 1);
		}
		text = text + num3 + " hour" + ((num3 > 1) ? "s" : "") + " remaining)";
		if (flag)
		{
			text += "</color>";
		}
		SetSubtitle(text);
	}

	public void NotifyDealCompleted()
	{
		MainEntry.Complete();
		Complete();
	}

	public void NotifyTruceEnded()
	{
		EndTruceEntry.Complete();
		Complete();
	}
}
