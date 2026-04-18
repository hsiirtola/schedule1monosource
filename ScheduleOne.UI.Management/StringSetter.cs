using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class StringSetter : ClipboardScreen
{
	[Header("References")]
	public TextMeshProUGUI TitleLabel;

	public TMP_InputField InputField;

	public Button DoneButton;

	private string _existingValue = "";

	private bool _allowEmpty;

	private Action<string> _callback;

	private void Awake()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		((UnityEvent<string>)(object)InputField.onSubmit).AddListener((UnityAction<string>)OnSubmit);
		((UnityEvent)DoneButton.onClick).AddListener(new UnityAction(DoneButtonPressed));
	}

	public void Initialize(string selectionTitle, string existingValue, int characterLimit, bool allowEmpty, Action<string> callback = null)
	{
		((TMP_Text)TitleLabel).text = selectionTitle;
		InputField.SetTextWithoutNotify(existingValue);
		InputField.characterLimit = characterLimit;
		_allowEmpty = allowEmpty;
		_existingValue = existingValue;
		_callback = callback;
	}

	public override void Open()
	{
		base.Open();
		Singleton<ManagementInterface>.Instance.MainScreen.Close();
		((Selectable)InputField).Select();
	}

	public override void Close()
	{
		base.Close();
		Singleton<ManagementInterface>.Instance.MainScreen.Open();
	}

	private void DoneButtonPressed()
	{
		OnSubmit(InputField.text);
	}

	private void OnSubmit(string value)
	{
		if (string.IsNullOrWhiteSpace(value) && !_allowEmpty)
		{
			value = _existingValue;
		}
		_callback?.Invoke(value);
		Close();
	}
}
