using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using ScheduleOne.NPCs;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class NPCFieldUI : MonoBehaviour
{
	[Header("References")]
	public TextMeshProUGUI FieldLabel;

	public Image IconImg;

	public TextMeshProUGUI SelectionLabel;

	public GameObject NoneSelected;

	public GameObject MultipleSelected;

	public RectTransform ClearButton;

	public List<NPCField> Fields { get; protected set; } = new List<NPCField>();

	public void Bind(List<NPCField> field)
	{
		Fields = new List<NPCField>();
		Fields.AddRange(field);
		Fields[Fields.Count - 1].onNPCChanged.AddListener((UnityAction<NPC>)Refresh);
		Refresh(Fields[0].SelectedNPC);
	}

	private void Refresh(NPC newVal)
	{
		((Component)IconImg).gameObject.SetActive(false);
		NoneSelected.gameObject.SetActive(false);
		MultipleSelected.gameObject.SetActive(false);
		if (AreFieldsUniform())
		{
			if ((Object)(object)newVal != (Object)null)
			{
				IconImg.sprite = newVal.MugshotSprite;
				((TMP_Text)SelectionLabel).text = newVal.FirstName + "\n" + newVal.LastName;
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
		NPCField nPCField = Fields.FirstOrDefault((NPCField x) => (Object)(object)x.SelectedNPC != (Object)null);
		((Component)ClearButton).gameObject.SetActive(nPCField != null);
	}

	private bool AreFieldsUniform()
	{
		for (int i = 0; i < Fields.Count - 1; i++)
		{
			if ((Object)(object)Fields[i].SelectedNPC != (Object)(object)Fields[i + 1].SelectedNPC)
			{
				return false;
			}
		}
		return true;
	}

	public void Clicked()
	{
		AreFieldsUniform();
		Singleton<ManagementInterface>.Instance.NPCSelector.Open("Select " + ((TMP_Text)FieldLabel).text, Fields[0].TypeRequirement, NPCSelected);
	}

	public void NPCSelected(NPC npc)
	{
		if ((Object)(object)npc != (Object)null && ((object)npc).GetType() != Fields[0].TypeRequirement)
		{
			Console.LogError("Wrong NPC type selection");
			return;
		}
		foreach (NPCField field in Fields)
		{
			field.SetNPC(npc, network: true);
		}
	}

	public void ClearClicked()
	{
		NPCSelected(null);
	}
}
