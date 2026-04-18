using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Dialogue;

public class DialogueController_Dan : DialogueController
{
	public ItemDefinition ItemToGive;

	protected override void Start()
	{
		base.Start();
		if ((Object)(object)ItemToGive == (Object)null)
		{
			Debug.LogWarning((object)"ItemToGive is not set in the inspector.");
		}
	}

	public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
	{
		if (dialogueLabel == "GIVE_ITEM" && (Object)(object)ItemToGive != (Object)null)
		{
			PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(ItemToGive.GetDefaultInstance());
		}
		return base.ModifyDialogueText(dialogueLabel, dialogueText);
	}
}
