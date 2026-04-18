using System;
using System.Collections;
using ScheduleOne.Cartel;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs.CharacterClasses;
using UnityEngine;

namespace ScheduleOne.Dialogue;

public class DialogueController_ThomasBenzies : DialogueController
{
	public override void ChoiceCallback(string choiceLabel)
	{
		base.ChoiceCallback(choiceLabel);
		if (choiceLabel == "ACCEPT_DEAL")
		{
			NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.SetStatus_Server(ECartelStatus.Truced, resetStatusChangedTimer: true);
			NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.DealManager.SetHoursUntilDealRequest(12);
			WaitForConversationEnd();
		}
		else if (choiceLabel == "REFUSE_DEAL")
		{
			NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.SetStatus_Server(ECartelStatus.Hostile, resetStatusChangedTimer: true);
			WaitForConversationEnd();
		}
	}

	private void WaitForConversationEnd()
	{
		((MonoBehaviour)this).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => !handler.IsDialogueInProgress));
			((Component)this).GetComponentInParent<Thomas>().MeetingEnded_Server();
		}
	}
}
