using System.Collections.Generic;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.Management;

public class StringFieldUI : MonoBehaviour
{
	[Header("References")]
	public TextMeshProUGUI FieldLabel;

	public TMP_InputField InputField;

	public List<StringField> Fields { get; protected set; } = new List<StringField>();

	public void Bind(List<StringField> field)
	{
		Fields = new List<StringField>();
		Fields.AddRange(field);
		Fields[Fields.Count - 1].onItemChanged.AddListener((UnityAction<string>)Refresh);
		((UnityEvent<string>)(object)InputField.onValueChanged).AddListener((UnityAction<string>)ValueChanged);
		Refresh(Fields[0].Value);
	}

	private void Refresh(string newVal)
	{
		if (AreFieldsUniform())
		{
			InputField.SetTextWithoutNotify(newVal);
		}
		else
		{
			InputField.SetTextWithoutNotify("Mixed");
		}
	}

	private bool AreFieldsUniform()
	{
		for (int i = 0; i < Fields.Count - 1; i++)
		{
			if (Fields[i].Value != Fields[i + 1].Value)
			{
				return false;
			}
		}
		return true;
	}

	public void ValueChanged(string value)
	{
		for (int i = 0; i < Fields.Count; i++)
		{
			Fields[i].SetValue(value, network: true);
		}
	}
}
