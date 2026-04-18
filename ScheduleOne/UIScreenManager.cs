using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace ScheduleOne;

public class UIScreenManager : PersistentSingleton<UIScreenManager>
{
	public struct UIScreenInfo
	{
		public UIScreen screen;

		public Action onCloseCallback;
	}

	public const float NavigationRepeatDelay = 0.5f;

	public const float NavigationRepeatRate = 0.125f;

	public const float DefaultScrollSpeed = 0.15f;

	public const float ScrollbarScrollSpeed = 25f;

	[SerializeField]
	private UIPopupScreen[] popupScreenPrefabs;

	[SerializeField]
	[Tooltip("Default 'A' button on controller for basic selectable interaction. Used in UITrigger")]
	private InputActionReference submitInputAction;

	[SerializeField]
	[Tooltip("Default 'B' button on controller, RightMouseButton for back interaction. Used in UIScreenManager")]
	private InputActionReference backInputAction;

	[SerializeField]
	[Tooltip("Default 'Start' button on controller, Escape key for back interaction. Used in UIScreenManager")]
	private InputActionReference escapeInputAction;

	private List<UIPopupScreen> popupScreenInstances = new List<UIPopupScreen>();

	private Stack<UIScreenInfo> screenStack = new Stack<UIScreenInfo>();

	private static GameObject lastSelectedObject;

	private static bool isBackTriggeredThisFrame;

	public InputActionReference SubmitInputAction => submitInputAction;

	public static GameObject LastSelectedObject
	{
		get
		{
			return lastSelectedObject;
		}
		set
		{
			lastSelectedObject = value;
		}
	}

	public static bool IsBackTriggeredThisFrame => isBackTriggeredThisFrame;

	public UIScreen TopScreen => screenStack?.Peek().screen;

	protected override void Start()
	{
		base.Start();
		GameInput.OnInputDeviceChanged = (Action<GameInput.InputDeviceType>)Delegate.Combine(GameInput.OnInputDeviceChanged, new Action<GameInput.InputDeviceType>(HandleInputDeviceChanged));
		SceneManager.sceneLoaded += OnSceneLoaded;
		CheckInputDeviceMode();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GameInput.OnInputDeviceChanged = (Action<GameInput.InputDeviceType>)Delegate.Remove(GameInput.OnInputDeviceChanged, new Action<GameInput.InputDeviceType>(HandleInputDeviceChanged));
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void Update()
	{
		BackToCloseCurrentScreen();
	}

	private void LateUpdate()
	{
		isBackTriggeredThisFrame = false;
	}

	private void BackToCloseCurrentScreen()
	{
		if ((!backInputAction.action.WasPressedThisFrame() && !escapeInputAction.action.WasPressedThisFrame()) || screenStack.Count == 0 || screenStack.Peek().onCloseCallback == null)
		{
			return;
		}
		UIScreenInfo uIScreenInfo = screenStack.Pop();
		uIScreenInfo.screen.IsSelected = false;
		uIScreenInfo.onCloseCallback?.Invoke();
		if (screenStack.Count > 0)
		{
			uIScreenInfo = screenStack.Peek();
			uIScreenInfo.screen.IsSelected = true;
			if ((Object)(object)uIScreenInfo.screen.CurrentSelectedPanel != (Object)null && (Object)(object)uIScreenInfo.screen.CurrentSelectedPanel.CurrentSelectedSelectable != (Object)null)
			{
				LastSelectedObject = ((Component)uIScreenInfo.screen.CurrentSelectedPanel.CurrentSelectedSelectable).gameObject;
			}
			else
			{
				LastSelectedObject = null;
			}
		}
		isBackTriggeredThisFrame = true;
	}

	public bool IsActiveScreenRegisteredForBack()
	{
		if (screenStack.Count == 0)
		{
			return false;
		}
		UIScreenInfo uIScreenInfo = screenStack.Peek();
		if ((Object)(object)uIScreenInfo.screen != (Object)null)
		{
			return uIScreenInfo.onCloseCallback != null;
		}
		return false;
	}

	private void HandleInputDeviceChanged(GameInput.InputDeviceType type)
	{
		EventSystem current = EventSystem.current;
		if ((Object)(object)current == (Object)null)
		{
			return;
		}
		InputSystemUIInputModule component = ((Component)current).GetComponent<InputSystemUIInputModule>();
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogError((object)"InputSystemUIInputModule not found on EventSystem. Please ensure it is added.");
			return;
		}
		if (type == GameInput.InputDeviceType.Gamepad)
		{
			Cursor.visible = false;
			((Behaviour)component).enabled = false;
			Cursor.lockState = (CursorLockMode)1;
			current.SetSelectedGameObject(lastSelectedObject);
			return;
		}
		lastSelectedObject = current.currentSelectedGameObject;
		current.SetSelectedGameObject((GameObject)null);
		if (PlayerCamera.IsCursorShowing)
		{
			Cursor.visible = true;
			Cursor.lockState = (CursorLockMode)0;
		}
		else
		{
			Cursor.visible = false;
			Cursor.lockState = (CursorLockMode)1;
		}
		((Behaviour)component).enabled = true;
	}

	private void CheckInputDeviceMode()
	{
		if (GameInput.CurrentInputDevice == GameInput.InputDeviceType.Gamepad)
		{
			HandleInputDeviceChanged(GameInput.InputDeviceType.Gamepad);
		}
		else
		{
			HandleInputDeviceChanged(GameInput.InputDeviceType.KeyboardMouse);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		HandleInputDeviceChanged(GameInput.CurrentInputDevice);
	}

	public void AddScreen(UIScreen screen, Action onCloseCallback = null)
	{
		if (!((Object)(object)screen == (Object)null) && !IsScreenInStack(screen))
		{
			if (screenStack.Count > 0)
			{
				screenStack.Peek().screen.IsSelected = false;
			}
			UIScreenInfo item = new UIScreenInfo
			{
				screen = screen,
				onCloseCallback = onCloseCallback
			};
			screenStack.Push(item);
			screen.IsSelected = true;
			if ((Object)(object)screen.CurrentSelectedPanel != (Object)null && (Object)(object)screen.CurrentSelectedPanel.CurrentSelectedSelectable != (Object)null)
			{
				LastSelectedObject = ((Component)screen.CurrentSelectedPanel.CurrentSelectedSelectable).gameObject;
			}
			else
			{
				LastSelectedObject = null;
			}
			Debug.Log((object)$"UIScreenManager: Added screen {((Object)((Component)screen).gameObject).name}. Total screens in stack: {screenStack.Count}");
		}
	}

	public void RemoveScreen(UIScreen screen)
	{
		if ((Object)(object)screen == (Object)null || screenStack.Count == 0 || !IsScreenInStack(screen))
		{
			return;
		}
		if ((Object)(object)screenStack.Peek().screen == (Object)(object)screen)
		{
			screenStack.Pop();
			screen.IsSelected = false;
			Debug.Log((object)$"UIScreenManager: Removed screen {((Object)((Component)screen).gameObject).name}. Total screens in stack: {screenStack.Count}");
			if (screenStack.Count > 0)
			{
				screenStack.Peek().screen.IsSelected = true;
				if ((Object)(object)screen.CurrentSelectedPanel != (Object)null && (Object)(object)screen.CurrentSelectedPanel.CurrentSelectedSelectable != (Object)null)
				{
					LastSelectedObject = ((Component)screen.CurrentSelectedPanel.CurrentSelectedSelectable).gameObject;
				}
				else
				{
					LastSelectedObject = null;
				}
			}
			return;
		}
		Stack<UIScreenInfo> stack = new Stack<UIScreenInfo>();
		while (screenStack.Count > 0)
		{
			UIScreenInfo item = screenStack.Pop();
			if ((Object)(object)item.screen == (Object)(object)screen)
			{
				item.screen.IsSelected = false;
				break;
			}
			stack.Push(item);
		}
		while (stack.Count > 0)
		{
			screenStack.Push(stack.Pop());
		}
		Debug.Log((object)$"UIScreenManager: Attempted to remove screen {((Object)((Component)screen).gameObject).name} that is not on top of the stack. Total screens in stack: {screenStack.Count}");
	}

	private bool IsScreenInStack(UIScreen screen)
	{
		foreach (UIScreenInfo item in screenStack)
		{
			if ((Object)(object)item.screen == (Object)(object)screen)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyScreenActive()
	{
		return screenStack.Count > 0;
	}

	public bool IsAnyPopupScreenActive()
	{
		return popupScreenInstances.Exists((UIPopupScreen p) => p.IsSelected);
	}

	public void OpenPopupScreen(string popupID)
	{
		UIPopupScreen uIPopupScreen = FindPopupScreen(popupID);
		if (Object.op_Implicit((Object)(object)uIPopupScreen))
		{
			uIPopupScreen.Open();
		}
	}

	public void OpenPopupScreen(string popupID, params object[] args)
	{
		UIPopupScreen uIPopupScreen = FindPopupScreen(popupID);
		if (Object.op_Implicit((Object)(object)uIPopupScreen))
		{
			uIPopupScreen.Open(args);
		}
	}

	public void ClosePopupScreen(string popupID)
	{
		UIPopupScreen uIPopupScreen = popupScreenInstances.Find((UIPopupScreen p) => p.PopupID == popupID);
		if (Object.op_Implicit((Object)(object)uIPopupScreen))
		{
			uIPopupScreen.Close();
		}
	}

	private UIPopupScreen FindPopupScreen(string popupID)
	{
		UIPopupScreen uIPopupScreen = popupScreenInstances.Find((UIPopupScreen p) => p.PopupID == popupID);
		if (IsScreenInStack(uIPopupScreen))
		{
			return null;
		}
		if ((Object)(object)uIPopupScreen == (Object)null)
		{
			uIPopupScreen = Array.Find(popupScreenPrefabs, (UIPopupScreen p) => p.PopupID == popupID);
			if ((Object)(object)uIPopupScreen != (Object)null)
			{
				uIPopupScreen = Object.Instantiate<UIPopupScreen>(uIPopupScreen, ((Component)this).transform);
				popupScreenInstances.Add(uIPopupScreen);
				return uIPopupScreen;
			}
			Debug.LogError((object)("UIScreenManager: Popup screen with ID '" + popupID + "' not found in prefabs."));
			return null;
		}
		return uIPopupScreen;
	}
}
