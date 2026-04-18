using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Dialogue;

public class DialogueController_Employee : DialogueController
{
	private ScheduleOne.Property.Property selectedProperty;

	private void Awake()
	{
	}

	public override void ChoiceCallback(string choiceLabel)
	{
		ScheduleOne.Property.Property propertyByCode = GetPropertyByCode(choiceLabel);
		if ((Object)(object)propertyByCode != (Object)null)
		{
			selectedProperty = propertyByCode;
			handler.ShowNode(DialogueHandler.activeDialogue.GetDialogueNodeByLabel("FINALIZE"));
		}
		if (choiceLabel == "CONFIRM" && (Object)(object)selectedProperty != (Object)null)
		{
			(npc as Employee).SendTransfer(selectedProperty.PropertyCode);
			npc.DialogueHandler.ShowWorldspaceDialogue("Ok boss, I'll head over there shortly.", 5f);
		}
		base.ChoiceCallback(choiceLabel);
	}

	public override void ModifyChoiceList(string dialogueLabel, ref List<DialogueChoiceData> existingChoices)
	{
		if (dialogueLabel == "ENTRY" && ((Object)DialogueHandler.activeDialogue).name == "Employee_Transfer")
		{
			existingChoices.AddRange(GetChoices());
		}
		base.ModifyChoiceList(dialogueLabel, ref existingChoices);
	}

	private List<DialogueChoiceData> GetChoices()
	{
		List<DialogueChoiceData> list = new List<DialogueChoiceData>();
		foreach (ScheduleOne.Property.Property ownedProperty in ScheduleOne.Property.Property.OwnedProperties)
		{
			if (ownedProperty.EmployeeCapacity > 0 && !((Object)(object)ownedProperty == (Object)(object)(npc as Employee).AssignedProperty))
			{
				DialogueChoiceData dialogueChoiceData = new DialogueChoiceData();
				dialogueChoiceData.ChoiceText = ownedProperty.PropertyName + " (" + ownedProperty.Employees.Count + "/" + ownedProperty.EmployeeCapacity + ")";
				dialogueChoiceData.ChoiceLabel = ownedProperty.PropertyCode;
				list.Add(dialogueChoiceData);
			}
		}
		list.Sort(delegate(DialogueChoiceData x, DialogueChoiceData y)
		{
			ScheduleOne.Property.Property propertyByCode = GetPropertyByCode(x.ChoiceLabel);
			ScheduleOne.Property.Property propertyByCode2 = GetPropertyByCode(y.ChoiceLabel);
			if ((Object)(object)propertyByCode == (Object)null || (Object)(object)propertyByCode2 == (Object)null)
			{
				return 0;
			}
			int value = propertyByCode.EmployeeCapacity - propertyByCode.Employees.Count;
			return (propertyByCode2.EmployeeCapacity - propertyByCode2.Employees.Count).CompareTo(value);
		});
		list.Add(new DialogueChoiceData
		{
			ChoiceText = "Nevermind",
			ChoiceLabel = string.Empty
		});
		return list;
	}

	private ScheduleOne.Property.Property GetPropertyByCode(string code)
	{
		return Singleton<PropertyManager>.Instance.GetProperty(code);
	}

	public override bool CheckChoice(string choiceLabel, out string invalidReason)
	{
		ScheduleOne.Property.Property propertyByCode = GetPropertyByCode(choiceLabel);
		if ((Object)(object)propertyByCode != (Object)null && propertyByCode.Employees.Count >= propertyByCode.EmployeeCapacity)
		{
			invalidReason = "ALREADY AT MAX CAPACITY";
			return false;
		}
		return base.CheckChoice(choiceLabel, out invalidReason);
	}

	public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
	{
		if (dialogueLabel == "FINALIZE" && (Object)(object)selectedProperty != (Object)null)
		{
			dialogueText = dialogueText.Replace("<LOCATION>", selectedProperty.PropertyName);
		}
		return base.ModifyDialogueText(dialogueLabel, dialogueText);
	}
}
