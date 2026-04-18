using System;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.Dialogue;

[Serializable]
public class DialogueNodeData
{
	public string Guid;

	public string DialogueText;

	public string DialogueNodeLabel;

	public Vector2 Position;

	public DialogueChoiceData[] choices;

	public EVOLineType VoiceLine;

	public DialogueNodeData GetCopy()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		DialogueNodeData dialogueNodeData = new DialogueNodeData();
		dialogueNodeData.Guid = Guid;
		dialogueNodeData.DialogueText = DialogueText;
		dialogueNodeData.DialogueNodeLabel = DialogueNodeLabel;
		dialogueNodeData.Position = Position;
		for (int i = 0; i < choices.Length; i++)
		{
			choices.CopyTo(dialogueNodeData.choices, 0);
		}
		dialogueNodeData.VoiceLine = VoiceLine;
		return dialogueNodeData;
	}
}
