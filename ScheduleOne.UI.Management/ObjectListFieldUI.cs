using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class ObjectListFieldUI : MonoBehaviour
{
	[Header("References")]
	public string FieldText = "Objects";

	public string InstructionText = "Select <ObjectType>";

	public string ExtendedInstructionText = string.Empty;

	public TextMeshProUGUI FieldLabel;

	public GameObject NoneSelected;

	public GameObject MultipleSelected;

	public RectTransform[] Entries;

	public Button Button;

	public GameObject EditIcon;

	public GameObject NoMultiEdit;

	public List<ObjectListField> Fields { get; protected set; } = new List<ObjectListField>();

	public void Bind(List<ObjectListField> field)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		Fields = new List<ObjectListField>();
		Fields.AddRange(field);
		Fields[Fields.Count - 1].onListChanged.AddListener((UnityAction<List<BuildableItem>>)Refresh);
		Refresh(Fields[0].SelectedObjects);
		for (int i = 0; i < Entries.Length; i++)
		{
			int index = i;
			((UnityEvent)((Component)((Transform)Entries[i]).Find("Remove")).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				RemoveEntryClicked(index);
			});
			if (i >= Fields[0].MaxItems)
			{
				((Component)Entries[i]).gameObject.SetActive(false);
			}
		}
		if (field.Count == 1)
		{
			EditIcon.gameObject.SetActive(true);
			NoMultiEdit.gameObject.SetActive(false);
			((Selectable)Button).interactable = true;
		}
		else
		{
			EditIcon.gameObject.SetActive(false);
			NoMultiEdit.gameObject.SetActive(true);
			((Selectable)Button).interactable = false;
		}
	}

	private void Refresh(List<BuildableItem> newVal)
	{
		NoneSelected.gameObject.SetActive(false);
		MultipleSelected.gameObject.SetActive(false);
		bool flag = AreFieldsUniform();
		if (flag)
		{
			if (Fields[0].SelectedObjects.Count == 0)
			{
				NoneSelected.SetActive(true);
			}
		}
		else
		{
			MultipleSelected.SetActive(true);
		}
		if (Fields.Count == 1)
		{
			((TMP_Text)FieldLabel).text = FieldText + " (" + newVal.Count + "/" + Fields[0].MaxItems + ")";
		}
		else
		{
			((TMP_Text)FieldLabel).text = FieldText;
		}
		for (int i = 0; i < Entries.Length; i++)
		{
			if (flag && Fields[0].SelectedObjects.Count > i)
			{
				((TMP_Text)((Component)((Transform)Entries[i]).Find("Title")).GetComponent<TextMeshProUGUI>()).text = Fields[0].SelectedObjects[i].GetManagementName();
				((Component)((Transform)Entries[i]).Find("Title")).gameObject.SetActive(true);
				((Component)((Transform)Entries[i]).Find("Icon")).GetComponent<Image>().sprite = ((BaseItemInstance)Fields[0].SelectedObjects[i].ItemInstance).Icon;
				((Component)((Transform)Entries[i]).Find("Icon")).gameObject.SetActive(true);
				((Component)((Transform)Entries[i]).Find("Remove")).gameObject.SetActive(true);
			}
			else
			{
				((Component)((Transform)Entries[i]).Find("Title")).gameObject.SetActive(false);
				((Component)((Transform)Entries[i]).Find("Icon")).gameObject.SetActive(false);
				((Component)((Transform)Entries[i]).Find("Remove")).gameObject.SetActive(false);
			}
		}
	}

	private void RemoveEntryClicked(int index)
	{
		if (AreFieldsUniform())
		{
			List<BuildableItem> list = new List<BuildableItem>(Fields[0].SelectedObjects);
			if (index < list.Count)
			{
				list.RemoveAt(index);
				ObjectsSelected(list);
			}
		}
	}

	private bool AreFieldsUniform()
	{
		for (int i = 0; i < Fields.Count - 1; i++)
		{
			if (!Fields[i].SelectedObjects.SequenceEqual(Fields[i + 1].SelectedObjects))
			{
				return false;
			}
		}
		return true;
	}

	public void Clicked()
	{
		List<BuildableItem> list = new List<BuildableItem>();
		if (AreFieldsUniform())
		{
			list.AddRange(Fields[0].SelectedObjects);
		}
		Singleton<ManagementInterface>.Instance.ObjectSelector.Open(InstructionText, ExtendedInstructionText, Fields[0].MaxItems, list, Fields[0].TypeRequirements, Fields[0].ParentConfig.Configurable.ParentProperty, ObjectValid, ObjectsSelected);
	}

	private bool ObjectValid(BuildableItem obj, out string reason)
	{
		string text = string.Empty;
		for (int i = 0; i < Fields.Count; i++)
		{
			if (Fields[i].objectFilter == null || Fields[i].objectFilter(obj, out reason))
			{
				reason = string.Empty;
				return true;
			}
			text = reason;
		}
		reason = text;
		return false;
	}

	public void ObjectsSelected(List<BuildableItem> objs)
	{
		foreach (ObjectListField field in Fields)
		{
			new List<BuildableItem>().AddRange(objs);
			field.SetList(objs, network: true);
		}
	}

	public void Clear()
	{
		foreach (ObjectListField field in Fields)
		{
			field.SetList(new List<BuildableItem>(), network: true);
		}
	}
}
