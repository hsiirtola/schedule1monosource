using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using UnityEngine;

namespace ScheduleOne.Dialogue;

public class DialogueController_Oscar : DialogueController
{
	public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (dialogueLabel == "ENTRY")
		{
			dialogueText = dialogueText.Replace("<REGION>", Singleton<ScheduleOne.Map.Map>.Instance.GetRegionFromPosition(((Component)NetworkSingleton<SewerManager>.Instance.RandomWorldSewerKeyPickup).transform.position).ToString().ToLower());
		}
		if (dialogueLabel == "NPC_HINT")
		{
			SewerManager.KeyPossessor sewerKeyPossessor = NetworkSingleton<SewerManager>.Instance.GetSewerKeyPossessor();
			dialogueText = dialogueText.Replace("<NPC_DESCRIPTION>", sewerKeyPossessor.NPCDescription);
		}
		return base.ModifyDialogueText(dialogueLabel, dialogueText);
	}
}
