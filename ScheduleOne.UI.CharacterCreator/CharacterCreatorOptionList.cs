using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Clothing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCreator;

public class CharacterCreatorOptionList : CharacterCreatorField<string>
{
	[Serializable]
	public class Option
	{
		public string Label;

		public string AssetPath;

		public ClothingDefinition ClothingItemEquivalent;
	}

	[Header("References")]
	public RectTransform OptionContainer;

	[Header("Settings")]
	public bool CanSelectNone = true;

	public List<Option> Options;

	public GameObject OptionPrefab;

	private List<Button> optionButtons = new List<Button>();

	private Button selectedButton;

	protected override void Awake()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		base.Awake();
		if (CanSelectNone)
		{
			Options.Insert(0, new Option
			{
				AssetPath = "",
				Label = "None"
			});
		}
		for (int i = 0; i < Options.Count; i++)
		{
			GameObject val = Object.Instantiate<GameObject>(OptionPrefab, (Transform)(object)OptionContainer);
			((TMP_Text)((Component)val.transform.Find("Text")).GetComponent<TextMeshProUGUI>()).text = Options[i].Label;
			string option = Options[i].AssetPath;
			((UnityEvent)val.GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				OptionClicked(option);
			});
			optionButtons.Add(val.GetComponent<Button>());
		}
	}

	public override void ApplyValue()
	{
		base.ApplyValue();
		Button val = null;
		for (int i = 0; i < Options.Count; i++)
		{
			if (base.value == Options[i].AssetPath)
			{
				selectedClothingDefinition = Options[i].ClothingItemEquivalent;
				if (optionButtons.Count > i)
				{
					val = optionButtons[i];
				}
				break;
			}
		}
		if ((Object)(object)selectedButton != (Object)null)
		{
			((Selectable)selectedButton).interactable = true;
		}
		selectedButton = val;
		if ((Object)(object)selectedButton != (Object)null)
		{
			((Selectable)selectedButton).interactable = false;
		}
	}

	public void OptionClicked(string option)
	{
		base.value = option;
		Option option2 = Options.FirstOrDefault((Option o) => o.AssetPath == option);
		if (option2 != null)
		{
			selectedClothingDefinition = option2.ClothingItemEquivalent;
		}
		else
		{
			selectedClothingDefinition = null;
		}
		WriteValue();
	}
}
