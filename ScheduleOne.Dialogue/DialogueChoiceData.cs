using System;

namespace ScheduleOne.Dialogue;

[Serializable]
public class DialogueChoiceData
{
	public string Guid;

	public string ChoiceText;

	public string ChoiceLabel;

	public bool ShowWorldspaceDialogue = true;

	public DialogueChoiceData GetCopy()
	{
		return new DialogueChoiceData
		{
			Guid = Guid,
			ChoiceText = ChoiceText,
			ChoiceLabel = ChoiceLabel,
			ShowWorldspaceDialogue = ShowWorldspaceDialogue
		};
	}
}
