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

public class ObjectFieldUI : MonoBehaviour
{
	[Header("References")]
	public string InstructionText = "Select <ObjectType>";

	public string ExtendedInstructionText = string.Empty;

	public TextMeshProUGUI FieldLabel;

	public Image IconImg;

	public TextMeshProUGUI SelectionLabel;

	public GameObject NoneSelected;

	public GameObject MultipleSelected;

	public RectTransform ClearButton;

	public List<ObjectField> Fields { get; protected set; } = new List<ObjectField>();

	public void Bind(List<ObjectField> field)
	{
		Fields = new List<ObjectField>();
		Fields.AddRange(field);
		Fields[Fields.Count - 1].onObjectChanged.AddListener((UnityAction<BuildableItem>)Refresh);
		Refresh(Fields[0].SelectedObject);
	}

	private void Refresh(BuildableItem newVal)
	{
		((Component)IconImg).gameObject.SetActive(false);
		NoneSelected.gameObject.SetActive(false);
		MultipleSelected.gameObject.SetActive(false);
		if (AreFieldsUniform())
		{
			if ((Object)(object)newVal != (Object)null)
			{
				IconImg.sprite = ((BaseItemInstance)newVal.ItemInstance).Icon;
				((TMP_Text)SelectionLabel).text = newVal.GetManagementName();
				((Component)IconImg).gameObject.SetActive(true);
			}
			else
			{
				NoneSelected.SetActive(true);
				((TMP_Text)SelectionLabel).text = "None";
			}
		}
		else
		{
			MultipleSelected.SetActive(true);
			((TMP_Text)SelectionLabel).text = "Mixed";
		}
		ObjectField objectField = Fields.FirstOrDefault((ObjectField x) => (Object)(object)x.SelectedObject != (Object)null);
		((Component)ClearButton).gameObject.SetActive(objectField != null);
	}

	private bool AreFieldsUniform()
	{
		for (int i = 0; i < Fields.Count - 1; i++)
		{
			if ((Object)(object)Fields[i].SelectedObject != (Object)(object)Fields[i + 1].SelectedObject)
			{
				return false;
			}
		}
		return true;
	}

	public void Clicked()
	{
		BuildableItem buildableItem = null;
		if (AreFieldsUniform())
		{
			buildableItem = Fields[0].SelectedObject;
		}
		List<BuildableItem> list = new List<BuildableItem>();
		if ((Object)(object)buildableItem != (Object)null)
		{
			list.Add(buildableItem);
		}
		List<Transform> list2 = new List<Transform>();
		for (int i = 0; i < Fields.Count; i++)
		{
			if (Fields[i].DrawTransitLine)
			{
				list2.Add(Fields[i].ParentConfig.Configurable.UIPoint);
			}
		}
		Singleton<ManagementInterface>.Instance.ObjectSelector.Open(InstructionText, ExtendedInstructionText, 1, list, Fields[0].TypeRequirements, Fields[0].ParentConfig.Configurable.ParentProperty, ObjectValid, ObjectsSelected, list2);
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
		ObjectSelected((objs.Count > 0) ? objs[objs.Count - 1] : null);
	}

	private void ObjectSelected(BuildableItem obj)
	{
		if ((Object)(object)obj != (Object)null && Fields[0].TypeRequirements.Count > 0 && !Fields[0].TypeRequirements.Contains(((object)obj).GetType()))
		{
			Console.LogError("Wrong Object type selection");
			return;
		}
		foreach (ObjectField field in Fields)
		{
			field.SetObject(obj, network: true);
		}
	}

	public void ClearClicked()
	{
		ObjectSelected(null);
	}
}
