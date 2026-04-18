using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Quests;
using UnityEngine;

namespace ScheduleOne.Dialogue;

public class DialogueController_Billy : DialogueController
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

	public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
	{
		if (dialogueLabel == "REQUEST_PRODUCT")
		{
			questDefeatCartel.EnquireAboutRDXEntry.Complete();
		}
		return base.ModifyDialogueText(dialogueLabel, dialogueText);
	}
}
