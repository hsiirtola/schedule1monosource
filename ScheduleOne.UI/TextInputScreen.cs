using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI;

public class TextInputScreen : Singleton<TextInputScreen>
{
	public delegate void OnSubmit(string text);

	public Canvas Canvas;

	public TextMeshProUGUI HeaderLabel;

	public TMP_InputField InputField;

	private OnSubmit onSubmit;

	public bool IsOpen => ((Behaviour)Canvas).enabled;

	protected override void Awake()
	{
		base.Awake();
		GameInput.RegisterExitListener(Exit, 2);
	}

	public void Submit()
	{
		Close(submit: true);
	}

	public void Cancel()
	{
		Close(submit: false);
	}

	private void Update()
	{
		if (IsOpen && Input.GetKeyDown((KeyCode)13))
		{
			Submit();
		}
	}

	public void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			Close(submit: false);
		}
	}

	public void Open(string header, string text, OnSubmit _onSubmit, int maxChars = 10000)
	{
		((TMP_Text)HeaderLabel).text = header;
		InputField.SetTextWithoutNotify(text);
		((Behaviour)Canvas).enabled = true;
		InputField.characterLimit = maxChars;
		InputField.ActivateInputField();
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		onSubmit = _onSubmit;
	}

	private void Close(bool submit)
	{
		((Behaviour)Canvas).enabled = false;
		InputField.DeactivateInputField(false);
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		if (submit)
		{
			string text = InputField.text;
			if (onSubmit != null)
			{
				onSubmit(text);
			}
		}
	}
}
