using UnityEngine;

namespace ScheduleOne.Dialogue;

public class DialogueChoiceEnabler : MonoBehaviour
{
	public DialogueController DialogueController;

	public int ChoiceIndex;

	private DialogueController.DialogueChoice choice;

	private void Awake()
	{
		if ((Object)(object)DialogueController == (Object)null)
		{
			Console.LogError("DialogueChoiceEnabler requires a DialogueController component in the parent hierarchy.");
		}
		else
		{
			choice = DialogueController.Choices[ChoiceIndex];
		}
	}

	private void OnValidate()
	{
		if ((Object)(object)DialogueController == (Object)null)
		{
			DialogueController = ((Component)this).GetComponentInParent<DialogueController>();
		}
		if ((Object)(object)DialogueController == (Object)null)
		{
			Debug.LogError((object)"DialogueChoiceEnabler requires a DialogueHandler component in the parent hierarchy.");
			return;
		}
		DialogueController.DialogueChoice dialogueChoice = DialogueController.Choices[ChoiceIndex];
		((Object)((Component)this).gameObject).name = "(" + ChoiceIndex + ") " + dialogueChoice.ChoiceText;
	}

	public void EnableChoice()
	{
		choice.Enabled = true;
	}

	public void DisableChoice()
	{
		choice.Enabled = false;
	}
}
