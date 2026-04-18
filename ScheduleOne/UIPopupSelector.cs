using System;
using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne;

public class UIPopupSelector : UIOption
{
	[SerializeField]
	private TextMeshProUGUI currentOptionNameText;

	public UnityEvent<UIPopupScreen_ContextMenu.ContextMenuOption> OnChanged;

	private UIPopupScreen_ContextMenu.ContextMenuOption[] options = new UIPopupScreen_ContextMenu.ContextMenuOption[0];

	private int currentIndex = -1;

	public int GetOptionCount()
	{
		return options.Length;
	}

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.Awake();
		selectable.OnTrigger.AddListener((UnityAction)delegate
		{
			OpenPopup();
		});
	}

	private void OpenPopup()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (options.Length != 0)
		{
			Singleton<UIScreenManager>.Instance.OpenPopupScreen("ContextMenu", options, (object)new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f), UIPopupScreen_ContextMenu.AnchorType.Center, currentIndex, true);
		}
	}

	private void ClosePopup(int selectedIndex)
	{
		currentIndex = selectedIndex;
		ClampCurrentIndex();
		OnChanged?.Invoke(options[currentIndex]);
		UpdateCurrentOptionText();
	}

	public void SetCurrentOptionWithoutNotify(int index)
	{
		currentIndex = index;
		UpdateCurrentOptionText();
	}

	private void UpdateCurrentOptionText()
	{
		if (currentIndex < 0 || currentIndex >= options.Length)
		{
			((TMP_Text)currentOptionNameText).text = string.Empty;
		}
		else
		{
			((TMP_Text)currentOptionNameText).text = options[currentIndex].optionName;
		}
	}

	public void AddOption(UIPopupScreen_ContextMenu.ContextMenuOption option)
	{
		Array.Resize(ref options, options.Length + 1);
		options[options.Length - 1] = option;
		UIPopupScreen_ContextMenu.ContextMenuOption[] array = options;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].optionAction = null;
		}
		SetOptions(options);
	}

	public void AddOptions(UIPopupScreen_ContextMenu.ContextMenuOption[] newOptions)
	{
		int destinationIndex = options.Length;
		Array.Resize(ref options, options.Length + newOptions.Length);
		Array.Copy(newOptions, 0, options, destinationIndex, newOptions.Length);
		UIPopupScreen_ContextMenu.ContextMenuOption[] array = options;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].optionAction = null;
		}
		SetOptions(options);
	}

	public void ClearOptions()
	{
		SetOptions(new UIPopupScreen_ContextMenu.ContextMenuOption[0]);
	}

	private void ClampCurrentIndex()
	{
		currentIndex = Mathf.Clamp(currentIndex, 0, options.Length - 1);
	}

	public void SetOptions(UIPopupScreen_ContextMenu.ContextMenuOption[] newOptions, int defaultIndex = 0)
	{
		options = newOptions;
		ClampCurrentIndex();
		UIPopupScreen_ContextMenu.ContextMenuOption[] array = options;
		foreach (UIPopupScreen_ContextMenu.ContextMenuOption option in array)
		{
			UIPopupScreen_ContextMenu.ContextMenuOption contextMenuOption = option;
			contextMenuOption.optionAction = (Action)Delegate.Combine(contextMenuOption.optionAction, (Action)delegate
			{
				ClosePopup(option.optionID);
			});
		}
		UpdateCurrentOptionText();
	}
}
