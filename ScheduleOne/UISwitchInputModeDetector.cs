using System;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne;

public class UISwitchInputModeDetector : MonoBehaviour
{
	public UnityEvent OnInputModeChanged;

	public UnityEvent OnInputModeChangedToController;

	public UnityEvent OnInputModeChangedToMouse;

	private void Start()
	{
		GameInput.OnInputDeviceChanged = (Action<GameInput.InputDeviceType>)Delegate.Combine(GameInput.OnInputDeviceChanged, new Action<GameInput.InputDeviceType>(OnControlsChanged));
		SwitchMode(GameInput.CurrentInputDevice);
	}

	private void OnControlsChanged(GameInput.InputDeviceType type)
	{
		UnityEvent onInputModeChanged = OnInputModeChanged;
		if (onInputModeChanged != null)
		{
			onInputModeChanged.Invoke();
		}
		SwitchMode(type);
	}

	private void SwitchMode(GameInput.InputDeviceType type)
	{
		switch (type)
		{
		case GameInput.InputDeviceType.Gamepad:
		{
			UnityEvent onInputModeChangedToController = OnInputModeChangedToController;
			if (onInputModeChangedToController != null)
			{
				onInputModeChangedToController.Invoke();
			}
			break;
		}
		case GameInput.InputDeviceType.KeyboardMouse:
		{
			UnityEvent onInputModeChangedToMouse = OnInputModeChangedToMouse;
			if (onInputModeChangedToMouse != null)
			{
				onInputModeChangedToMouse.Invoke();
			}
			break;
		}
		}
	}
}
