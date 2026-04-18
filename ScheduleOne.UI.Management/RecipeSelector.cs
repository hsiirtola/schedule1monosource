using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Stations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class RecipeSelector : ClipboardScreen
{
	[Header("References")]
	public RectTransform OptionContainer;

	public TextMeshProUGUI TitleLabel;

	public GameObject OptionPrefab;

	[Header("Settings")]
	public Sprite EmptyOptionSprite;

	private Coroutine lerpRoutine;

	private List<StationRecipe> options = new List<StationRecipe>();

	private StationRecipe selectedOption;

	private List<RectTransform> optionButtons = new List<RectTransform>();

	private Action<StationRecipe> optionCallback;

	private UIContentPanel panel;

	public void Initialize(string selectionTitle, List<StationRecipe> _options, StationRecipe _selectedOption = null, Action<StationRecipe> _optionCallback = null)
	{
		((TMP_Text)TitleLabel).text = selectionTitle;
		options = new List<StationRecipe>();
		options.AddRange(_options);
		selectedOption = _selectedOption;
		optionCallback = _optionCallback;
		panel = ((Component)this).GetComponent<UIContentPanel>();
		if (!Object.op_Implicit((Object)(object)panel))
		{
			panel = ((Component)this).gameObject.AddComponent<UIContentPanel>();
		}
		DeleteOptions();
		CreateOptions(options);
	}

	public override void Open()
	{
		base.Open();
		Debug.Log((object)(((Object)((Component)Container).gameObject).name + " is active: " + ((Component)Container).gameObject.activeSelf));
		Singleton<ManagementInterface>.Instance.MainScreen.Close();
		Singleton<ManagementInterface>.Instance.UIScreen.AddPanel(panel);
		Singleton<ManagementInterface>.Instance.UIScreen.SetCurrentSelectedPanel(panel);
	}

	public override void Close()
	{
		base.Close();
		Debug.Log((object)"Closed");
		Singleton<ManagementInterface>.Instance.MainScreen.Open();
		Singleton<ManagementInterface>.Instance.UIScreen.RemovePanel(panel);
	}

	private void ButtonClicked(StationRecipe option)
	{
		if (optionCallback != null)
		{
			optionCallback(option);
		}
		Close();
	}

	private void CreateOptions(List<StationRecipe> options)
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		options.Sort((StationRecipe a, StationRecipe b) => a.RecipeTitle.CompareTo(b.RecipeTitle));
		for (int num = 0; num < options.Count; num++)
		{
			StationRecipeEntry component = Object.Instantiate<GameObject>(OptionPrefab, (Transform)(object)OptionContainer).GetComponent<StationRecipeEntry>();
			component.AssignRecipe(options[num]);
			UISelectable uISelectable = ((Component)component).gameObject.GetComponent<UISelectable>();
			if (!Object.op_Implicit((Object)(object)uISelectable))
			{
				uISelectable = ((Component)component).gameObject.AddComponent<UISelectable>();
			}
			panel.AddSelectable(uISelectable);
			if ((Object)(object)options[num] == (Object)(object)selectedOption)
			{
				((Graphic)((Component)((Component)component).transform.Find("Selected")).gameObject.GetComponent<Image>()).color = Color32.op_Implicit(new Color32((byte)90, (byte)90, (byte)90, byte.MaxValue));
			}
			StationRecipe opt = options[num];
			((UnityEvent)component.Button.onClick).AddListener((UnityAction)delegate
			{
				ButtonClicked(opt);
			});
			uISelectable.OnTrigger.AddListener((UnityAction)delegate
			{
				ButtonClicked(opt);
			});
			optionButtons.Add(((Component)component).GetComponent<RectTransform>());
		}
	}

	private void DeleteOptions()
	{
		for (int i = 0; i < optionButtons.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)optionButtons[i]).gameObject);
		}
		optionButtons.Clear();
		panel.ClearAllSelectables();
	}
}
