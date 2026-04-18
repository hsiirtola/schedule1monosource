using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.Settings;

public class SettingsDropdown : MonoBehaviour
{
	public string[] DefaultOptions;

	[SerializeField]
	protected UIPopupSelector _popupSelector;

	private TMP_Dropdown _dropdown;

	protected virtual void Awake()
	{
		_dropdown = ((Component)this).GetComponent<TMP_Dropdown>();
		if (Object.op_Implicit((Object)(object)_dropdown))
		{
			((UnityEvent<int>)(object)_dropdown.onValueChanged).AddListener((UnityAction<int>)OnValueChanged);
		}
		string[] defaultOptions = DefaultOptions;
		foreach (string option in defaultOptions)
		{
			AddOption(option);
		}
	}

	protected void SetValueWithoutNotify(int value)
	{
		if (Object.op_Implicit((Object)(object)_dropdown))
		{
			_dropdown.SetValueWithoutNotify(Mathf.Clamp(value, 0, _dropdown.options.Count - 1));
		}
		if (Object.op_Implicit((Object)(object)_popupSelector))
		{
			_popupSelector.SetCurrentOptionWithoutNotify(Mathf.Clamp(value, 0, _popupSelector.GetOptionCount() - 1));
		}
	}

	protected virtual void Start()
	{
		if (Object.op_Implicit((Object)(object)_popupSelector))
		{
			_popupSelector.OnChanged.AddListener((UnityAction<UIPopupScreen_ContextMenu.ContextMenuOption>)delegate(UIPopupScreen_ContextMenu.ContextMenuOption v)
			{
				OnValueChanged(v.optionID);
			});
		}
	}

	protected virtual void OnValueChanged(int value)
	{
	}

	protected void AddOption(string option)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)_dropdown))
		{
			_dropdown.options.Add(new OptionData(option));
		}
		if (Object.op_Implicit((Object)(object)_popupSelector))
		{
			_popupSelector.AddOption(new UIPopupScreen_ContextMenu.ContextMenuOption(_popupSelector.GetOptionCount(), option, delegate
			{
			}));
		}
	}

	protected void AddOptions(List<string> options)
	{
		foreach (string option in options)
		{
			AddOption(option);
		}
	}

	protected void ClearOptions()
	{
		if (Object.op_Implicit((Object)(object)_dropdown))
		{
			_dropdown.ClearOptions();
		}
		if (Object.op_Implicit((Object)(object)_popupSelector))
		{
			_popupSelector.ClearOptions();
		}
	}
}
