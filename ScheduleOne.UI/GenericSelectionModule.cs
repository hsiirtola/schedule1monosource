using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class GenericSelectionModule : Singleton<GenericSelectionModule>
{
	[Header("References")]
	public Canvas canvas;

	public TextMeshProUGUI TitleText;

	public RectTransform OptionContainer;

	public Button CloseButton;

	[Header("Prefabs")]
	public GameObject ListOptionPrefab;

	[HideInInspector]
	public bool OptionChosen;

	public bool isOpen { get; protected set; }

	[HideInInspector]
	public int ChosenOptionIndex { get; protected set; } = -1;

	protected override void Awake()
	{
		base.Awake();
		Close();
	}

	protected override void Start()
	{
		base.Start();
		GameInput.RegisterExitListener(Exit, 50);
	}

	private void Exit(ExitAction action)
	{
		if (isOpen && !action.Used && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			Cancel();
		}
	}

	public void Open(string title, List<string> options)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		isOpen = true;
		OptionChosen = false;
		ChosenOptionIndex = -1;
		ClearOptions();
		((TMP_Text)TitleText).text = title;
		for (int i = 0; i < options.Count; i++)
		{
			RectTransform component = Object.Instantiate<GameObject>(ListOptionPrefab, (Transform)(object)OptionContainer).GetComponent<RectTransform>();
			((TMP_Text)((Component)((Transform)component).Find("Label")).GetComponent<TextMeshProUGUI>()).text = options[i];
			component.anchoredPosition = new Vector2(0f, (0f - ((float)i + 0.5f)) * component.sizeDelta.y);
			int index = i;
			((UnityEvent)((Component)component).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				ListOptionClicked(index);
			});
		}
		((Behaviour)canvas).enabled = true;
	}

	public void Close()
	{
		isOpen = false;
		((Behaviour)canvas).enabled = false;
		ClearOptions();
	}

	public void Cancel()
	{
		ChosenOptionIndex = -1;
		OptionChosen = true;
		Close();
	}

	private void ClearOptions()
	{
		int childCount = ((Transform)OptionContainer).childCount;
		for (int i = 0; i < childCount; i++)
		{
			Object.Destroy((Object)(object)((Component)((Transform)OptionContainer).GetChild(0)).gameObject);
		}
	}

	private void ListOptionClicked(int index)
	{
		ChosenOptionIndex = index;
		OptionChosen = true;
		Close();
	}
}
