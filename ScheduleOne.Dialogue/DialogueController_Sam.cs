using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.Quests;
using UnityEngine;

namespace ScheduleOne.Dialogue;

public class DialogueController_Sam : DialogueController
{
	private Quest_DefeatCartel questDefeatCartel;

	protected override void Start()
	{
		base.Start();
		questDefeatCartel = NetworkSingleton<QuestManager>.Instance.DefaultQuests.FirstOrDefault((Quest q) => q is Quest_DefeatCartel) as Quest_DefeatCartel;
		if ((Object)(object)questDefeatCartel == (Object)null)
		{
			Debug.LogError((object)"Quest_DefeatCartel not found in DefaultQuests.");
		}
	}

	public override bool CheckChoice(string choiceLabel, out string invalidReason)
	{
		if (choiceLabel == "CONFIRM_DIG" && NetworkSingleton<MoneyManager>.Instance.cashBalance <= 10000f)
		{
			invalidReason = "Insufficient cash";
			return false;
		}
		return base.CheckChoice(choiceLabel, out invalidReason);
	}

	public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
	{
		if (dialogueLabel == "DIG_OFFER")
		{
			return dialogueText.Replace("<DIG_PRICE>", MoneyManager.ApplyMoneyTextColor(MoneyManager.FormatAmount(10000f)));
		}
		return base.ModifyDialogueText(dialogueLabel, dialogueText);
	}

	public override void ChoiceCallback(string choiceLabel)
	{
		if (choiceLabel == "CONFIRM_DIG")
		{
			questDefeatCartel.DigTunnelEntry.Complete();
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(-10000f, visualizeChange: true, playCashSound: true);
		}
		base.ChoiceCallback(choiceLabel);
	}
}
