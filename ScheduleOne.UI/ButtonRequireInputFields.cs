using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class ButtonRequireInputFields : MonoBehaviour
{
	[Serializable]
	public class Input
	{
		public TMP_InputField InputField;

		public RectTransform ErrorMessage;
	}

	public List<Input> Inputs;

	public TMP_Dropdown Dropdown;

	public Button Button;

	public void Update()
	{
		((Selectable)Button).interactable = true;
		if ((Object)(object)Dropdown != (Object)null && Dropdown.value == 0)
		{
			((Selectable)Button).interactable = false;
		}
		foreach (Input input in Inputs)
		{
			if (string.IsNullOrEmpty(input.InputField.text))
			{
				((Component)input.ErrorMessage).gameObject.SetActive(true);
				((Selectable)Button).interactable = false;
			}
			else
			{
				((Component)input.ErrorMessage).gameObject.SetActive(false);
			}
		}
	}
}
