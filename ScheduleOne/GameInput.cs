using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ScheduleOne;

public class GameInput : PersistentSingleton<GameInput>
{
	public enum ButtonCode
	{
		PrimaryClick,
		SecondaryClick,
		TertiaryClick,
		Forward,
		Backward,
		Left,
		Right,
		Jump,
		Crouch,
		Sprint,
		Escape,
		Back,
		Interact,
		Submit,
		TogglePhone,
		VehicleToggleLights,
		VehicleHandbrake,
		RotateLeft,
		RotateRight,
		ManagementMode,
		OpenMap,
		OpenJournal,
		OpenTexts,
		QuickMove,
		ToggleFlashlight,
		ViewAvatar,
		Reload,
		InventoryLeft,
		InventoryRight,
		Holster,
		VehicleResetCamera,
		SkateboardDismount,
		SkateboardMount,
		TogglePauseMenu
	}

	public enum InputDeviceType
	{
		KeyboardMouse,
		Gamepad
	}

	public class ExitListener
	{
		public ExitDelegate listenerFunction;

		public int priority;
	}

	public delegate void ExitDelegate(ExitAction exitAction);

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__73_0;

		internal void _003CStart_003Eb__73_0()
		{
			exitListeners.Clear();
		}
	}

	public static Action<InputDeviceType> OnInputDeviceChanged;

	public static List<ExitListener> exitListeners = new List<ExitListener>();

	public PlayerInput PlayerInput;

	public static bool IsTyping = false;

	public static Vector2 MotionAxis = Vector2.zero;

	public static Vector2 CameraAxis = Vector2.zero;

	public static bool TogglePauseInputUsed = false;

	private static Mouse systemMouse;

	public static float MouseWheelAxis;

	public static bool ControllerComboActive;

	private float vehicleDriveAxis;

	private List<ButtonCode> buttonsDownThisFrame = new List<ButtonCode>();

	private List<ButtonCode> buttonsDown = new List<ButtonCode>();

	private List<ButtonCode> buttonsUpThisFrame = new List<ButtonCode>();

	public static InputDeviceType CurrentInputDevice { get; private set; }

	public static Vector2 MouseDelta => CameraAxis;

	public unsafe static Vector3 MousePosition
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			if (GetCurrentInputDeviceIsKeyboardMouse())
			{
				return Input.mousePosition;
			}
			if (systemMouse == null)
			{
				systemMouse = CustomUIUtils.GetSystemMouse();
			}
			Mouse obj = systemMouse;
			return Vector2.op_Implicit((Vector2)((obj != null) ? System.Runtime.CompilerServices.Unsafe.Read<Vector2>((void*)((InputControl<Vector2>)(object)((Pointer)obj).position).value) : Vector2.op_Implicit(Input.mousePosition)));
		}
	}

	public static float MouseScrollDelta => MouseWheelAxis;

	public static float VehicleDriveAxis
	{
		get
		{
			return MotionAxis.y;
		}
		private set
		{
			Singleton<GameInput>.Instance.vehicleDriveAxis = value;
		}
	}

	public static Vector2 UINavigationDirection { get; private set; }

	public static Vector2 UICyclePanelDirection { get; private set; }

	public static float UITabNavigationPrimaryAxis { get; private set; }

	public static float UITabNavigationSecondaryAxis { get; private set; }

	public static float UIScrollbarAxis { get; private set; }

	public static Vector2 UIMapNavigationDirection { get; private set; }

	public static float UIMapZoomAxis { get; private set; }

	public static float UIModifyAmountIncrementTierOneAxis { get; private set; }

	public static float UIModifyAmountIncrementTierTwoAxis { get; private set; }

	public static float UIModifyAmountIncrementTierThreeAxis { get; private set; }

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PlayerInput.onControlsChanged -= OnControlsChanged;
	}

	protected override void Start()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		base.Start();
		if ((Object)(object)Singleton<GameInput>.Instance == (Object)null || (Object)(object)Singleton<GameInput>.Instance != (Object)(object)this)
		{
			return;
		}
		LoadManager loadManager = Singleton<LoadManager>.Instance;
		if (loadManager != null)
		{
			UnityEvent onPreSceneChange = loadManager.onPreSceneChange;
			object obj = _003C_003Ec._003C_003E9__73_0;
			if (obj == null)
			{
				UnityAction val = delegate
				{
					exitListeners.Clear();
				};
				_003C_003Ec._003C_003E9__73_0 = val;
				obj = (object)val;
			}
			onPreSceneChange.AddListener((UnityAction)obj);
		}
		((Behaviour)PlayerInput).enabled = true;
		PlayerInput.onControlsChanged += OnControlsChanged;
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			return;
		}
		foreach (ButtonCode item in buttonsDown)
		{
			buttonsUpThisFrame.Add(item);
		}
		buttonsDown.Clear();
	}

	public static bool GetButton(ButtonCode buttonCode)
	{
		return Singleton<GameInput>.Instance.buttonsDown.Contains(buttonCode);
	}

	public static bool GetButtonDown(ButtonCode buttonCode)
	{
		return Singleton<GameInput>.Instance.buttonsDownThisFrame.Contains(buttonCode);
	}

	public static bool GetButtonUp(ButtonCode buttonCode)
	{
		return Singleton<GameInput>.Instance.buttonsUpThisFrame.Contains(buttonCode);
	}

	public static bool GetCurrentInputDeviceIsKeyboardMouse()
	{
		return CurrentInputDevice == InputDeviceType.KeyboardMouse;
	}

	public static bool GetCurrentInputDeviceIsGamepad()
	{
		return CurrentInputDevice == InputDeviceType.Gamepad;
	}

	protected virtual void Update()
	{
		if (!Singleton<GameInput>.InstanceExists)
		{
			return;
		}
		TogglePauseInputUsed = false;
		bool buttonDown = GetButtonDown(ButtonCode.Escape);
		bool buttonDown2 = GetButtonDown(ButtonCode.Back);
		bool buttonDown3 = GetButtonDown(ButtonCode.SecondaryClick);
		if (buttonDown || buttonDown2 || buttonDown3)
		{
			bool flag = true;
			UIScreenManager uIScreenManager = Singleton<UIScreenManager>.Instance;
			if (Object.op_Implicit((Object)(object)uIScreenManager) && uIScreenManager.IsActiveScreenRegisteredForBack())
			{
				flag = false;
			}
			if (flag)
			{
				ExitType type = ((!buttonDown3) ? ExitType.Escape : ExitType.RightClick);
				Exit(type);
			}
		}
	}

	private void Exit(ExitType type)
	{
		ExitAction exitAction = new ExitAction();
		exitAction.exitType = type;
		for (int i = 0; i < exitListeners.Count; i++)
		{
			bool used = exitAction.Used;
			exitListeners[exitListeners.Count - (1 + i)].listenerFunction(exitAction);
			if (exitAction.Used && !used && type == ExitType.Escape)
			{
				TogglePauseInputUsed = true;
			}
		}
	}

	private void LateUpdate()
	{
		buttonsDownThisFrame.Clear();
		buttonsUpThisFrame.Clear();
	}

	public void ExitAll()
	{
		int num = 20;
		while (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
		{
			num--;
			if (num <= 0)
			{
				Console.LogError("Failed to exit from all active UI elements.");
				for (int i = 0; i < PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount; i++)
				{
					Debug.LogError((object)PlayerSingleton<PlayerCamera>.Instance.activeUIElements[i]);
				}
				break;
			}
			Exit(ExitType.Escape);
		}
	}

	private void OnControlsChanged(PlayerInput input)
	{
		if (input.currentControlScheme == "Gamepad")
		{
			CurrentInputDevice = InputDeviceType.Gamepad;
		}
		else if (input.currentControlScheme == "Keyboard and Mouse")
		{
			CurrentInputDevice = InputDeviceType.KeyboardMouse;
		}
		buttonsDown.Clear();
		buttonsDownThisFrame.Clear();
		buttonsUpThisFrame.Clear();
		if (OnInputDeviceChanged != null)
		{
			OnInputDeviceChanged(CurrentInputDevice);
		}
	}

	private void OnMotion(InputValue value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		MotionAxis = value.Get<Vector2>();
		if (MotionAxis.x > 0f)
		{
			if (!buttonsDown.Contains(ButtonCode.Right))
			{
				buttonsDownThisFrame.Add(ButtonCode.Right);
				buttonsDown.Add(ButtonCode.Right);
			}
		}
		else if (buttonsDown.Contains(ButtonCode.Right))
		{
			buttonsUpThisFrame.Add(ButtonCode.Right);
			buttonsDown.Remove(ButtonCode.Right);
		}
		if (MotionAxis.x < 0f)
		{
			if (!buttonsDown.Contains(ButtonCode.Left))
			{
				buttonsDownThisFrame.Add(ButtonCode.Left);
				buttonsDown.Add(ButtonCode.Left);
			}
		}
		else if (buttonsDown.Contains(ButtonCode.Left))
		{
			buttonsUpThisFrame.Add(ButtonCode.Left);
			buttonsDown.Remove(ButtonCode.Left);
		}
		if (MotionAxis.y > 0f)
		{
			if (!buttonsDown.Contains(ButtonCode.Forward))
			{
				buttonsDownThisFrame.Add(ButtonCode.Forward);
				buttonsDown.Add(ButtonCode.Forward);
			}
		}
		else if (buttonsDown.Contains(ButtonCode.Forward))
		{
			buttonsUpThisFrame.Add(ButtonCode.Forward);
			buttonsDown.Remove(ButtonCode.Forward);
		}
		if (MotionAxis.y < 0f)
		{
			if (!buttonsDown.Contains(ButtonCode.Backward))
			{
				buttonsDownThisFrame.Add(ButtonCode.Backward);
				buttonsDown.Add(ButtonCode.Backward);
			}
		}
		else if (buttonsDown.Contains(ButtonCode.Backward))
		{
			buttonsUpThisFrame.Add(ButtonCode.Backward);
			buttonsDown.Remove(ButtonCode.Backward);
		}
	}

	private void OnPrimaryClick()
	{
		if (buttonsDown.Contains(ButtonCode.PrimaryClick))
		{
			buttonsDown.Remove(ButtonCode.PrimaryClick);
			buttonsUpThisFrame.Add(ButtonCode.PrimaryClick);
		}
		else
		{
			buttonsDown.Add(ButtonCode.PrimaryClick);
			buttonsDownThisFrame.Add(ButtonCode.PrimaryClick);
		}
	}

	private void OnSecondaryClick()
	{
		if (buttonsDown.Contains(ButtonCode.SecondaryClick))
		{
			buttonsDown.Remove(ButtonCode.SecondaryClick);
			buttonsUpThisFrame.Add(ButtonCode.SecondaryClick);
		}
		else
		{
			buttonsDown.Add(ButtonCode.SecondaryClick);
			buttonsDownThisFrame.Add(ButtonCode.SecondaryClick);
		}
	}

	private void OnTertiaryClick()
	{
		if (buttonsDown.Contains(ButtonCode.TertiaryClick))
		{
			buttonsDown.Remove(ButtonCode.TertiaryClick);
			buttonsUpThisFrame.Add(ButtonCode.TertiaryClick);
		}
		else
		{
			buttonsDown.Add(ButtonCode.TertiaryClick);
			buttonsDownThisFrame.Add(ButtonCode.TertiaryClick);
		}
	}

	private void OnJump()
	{
		if (!ControllerComboActive)
		{
			if (buttonsDown.Contains(ButtonCode.Jump))
			{
				buttonsDown.Remove(ButtonCode.Jump);
				buttonsUpThisFrame.Add(ButtonCode.Jump);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Jump);
				buttonsDownThisFrame.Add(ButtonCode.Jump);
			}
		}
	}

	private void OnCrouch()
	{
		if (!ControllerComboActive)
		{
			if (buttonsDown.Contains(ButtonCode.Crouch))
			{
				buttonsDown.Remove(ButtonCode.Crouch);
				buttonsUpThisFrame.Add(ButtonCode.Crouch);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Crouch);
				buttonsDownThisFrame.Add(ButtonCode.Crouch);
			}
		}
	}

	private void OnSprint()
	{
		if (buttonsDown.Contains(ButtonCode.Sprint))
		{
			buttonsDown.Remove(ButtonCode.Sprint);
			buttonsUpThisFrame.Add(ButtonCode.Sprint);
		}
		else
		{
			buttonsDown.Add(ButtonCode.Sprint);
			buttonsDownThisFrame.Add(ButtonCode.Sprint);
		}
	}

	private void OnEscape()
	{
		if (buttonsDown.Contains(ButtonCode.Escape))
		{
			buttonsDown.Remove(ButtonCode.Escape);
			buttonsUpThisFrame.Add(ButtonCode.Escape);
		}
		else
		{
			buttonsDown.Add(ButtonCode.Escape);
			buttonsDownThisFrame.Add(ButtonCode.Escape);
		}
	}

	private void OnBack()
	{
		if (!ControllerComboActive)
		{
			if (buttonsDown.Contains(ButtonCode.Back))
			{
				buttonsDown.Remove(ButtonCode.Back);
				buttonsUpThisFrame.Add(ButtonCode.Back);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Back);
				buttonsDownThisFrame.Add(ButtonCode.Back);
			}
		}
	}

	private void OnInteract()
	{
		if (!ControllerComboActive)
		{
			if (buttonsDown.Contains(ButtonCode.Interact))
			{
				buttonsDown.Remove(ButtonCode.Interact);
				buttonsUpThisFrame.Add(ButtonCode.Interact);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Interact);
				buttonsDownThisFrame.Add(ButtonCode.Interact);
			}
		}
	}

	private void OnSubmit()
	{
		if (buttonsDown.Contains(ButtonCode.Submit))
		{
			buttonsDown.Remove(ButtonCode.Submit);
			buttonsUpThisFrame.Add(ButtonCode.Submit);
		}
		else
		{
			buttonsDown.Add(ButtonCode.Submit);
			buttonsDownThisFrame.Add(ButtonCode.Submit);
		}
	}

	private void OnTogglePhone()
	{
		if (buttonsDown.Contains(ButtonCode.TogglePhone))
		{
			buttonsDown.Remove(ButtonCode.TogglePhone);
			buttonsUpThisFrame.Add(ButtonCode.TogglePhone);
		}
		else
		{
			buttonsDown.Add(ButtonCode.TogglePhone);
			buttonsDownThisFrame.Add(ButtonCode.TogglePhone);
		}
	}

	private void OnVehicleToggleLights()
	{
		if (buttonsDown.Contains(ButtonCode.VehicleToggleLights))
		{
			buttonsDown.Remove(ButtonCode.VehicleToggleLights);
			buttonsUpThisFrame.Add(ButtonCode.VehicleToggleLights);
		}
		else
		{
			buttonsDown.Add(ButtonCode.VehicleToggleLights);
			buttonsDownThisFrame.Add(ButtonCode.VehicleToggleLights);
		}
	}

	private void OnVehicleHandbrake()
	{
		if (buttonsDown.Contains(ButtonCode.VehicleHandbrake))
		{
			buttonsDown.Remove(ButtonCode.VehicleHandbrake);
			buttonsUpThisFrame.Add(ButtonCode.VehicleHandbrake);
		}
		else
		{
			buttonsDown.Add(ButtonCode.VehicleHandbrake);
			buttonsDownThisFrame.Add(ButtonCode.VehicleHandbrake);
		}
	}

	private void OnRotateLeft()
	{
		if (buttonsDown.Contains(ButtonCode.RotateLeft))
		{
			buttonsDown.Remove(ButtonCode.RotateLeft);
			buttonsUpThisFrame.Add(ButtonCode.RotateLeft);
		}
		else
		{
			buttonsDown.Add(ButtonCode.RotateLeft);
			buttonsDownThisFrame.Add(ButtonCode.RotateLeft);
		}
	}

	private void OnRotateRight()
	{
		if (buttonsDown.Contains(ButtonCode.RotateRight))
		{
			buttonsDown.Remove(ButtonCode.RotateRight);
			buttonsUpThisFrame.Add(ButtonCode.RotateRight);
		}
		else
		{
			buttonsDown.Add(ButtonCode.RotateRight);
			buttonsDownThisFrame.Add(ButtonCode.RotateRight);
		}
	}

	private void OnManagementMode()
	{
		if (buttonsDown.Contains(ButtonCode.ManagementMode))
		{
			buttonsDown.Remove(ButtonCode.ManagementMode);
			buttonsUpThisFrame.Add(ButtonCode.ManagementMode);
		}
		else
		{
			buttonsDown.Add(ButtonCode.ManagementMode);
			buttonsDownThisFrame.Add(ButtonCode.ManagementMode);
		}
	}

	private void OnOpenMap()
	{
		if (buttonsDown.Contains(ButtonCode.OpenMap))
		{
			buttonsDown.Remove(ButtonCode.OpenMap);
			buttonsUpThisFrame.Add(ButtonCode.OpenMap);
		}
		else
		{
			buttonsDown.Add(ButtonCode.OpenMap);
			buttonsDownThisFrame.Add(ButtonCode.OpenMap);
		}
	}

	private void OnOpenJournal()
	{
		if (buttonsDown.Contains(ButtonCode.OpenJournal))
		{
			buttonsDown.Remove(ButtonCode.OpenJournal);
			buttonsUpThisFrame.Add(ButtonCode.OpenJournal);
		}
		else
		{
			buttonsDown.Add(ButtonCode.OpenJournal);
			buttonsDownThisFrame.Add(ButtonCode.OpenJournal);
		}
	}

	private void OnOpenTexts()
	{
		if (buttonsDown.Contains(ButtonCode.OpenTexts))
		{
			buttonsDown.Remove(ButtonCode.OpenTexts);
			buttonsUpThisFrame.Add(ButtonCode.OpenTexts);
		}
		else
		{
			buttonsDown.Add(ButtonCode.OpenTexts);
			buttonsDownThisFrame.Add(ButtonCode.OpenTexts);
		}
	}

	private void OnQuickMove()
	{
		if (buttonsDown.Contains(ButtonCode.QuickMove))
		{
			buttonsDown.Remove(ButtonCode.QuickMove);
			buttonsUpThisFrame.Add(ButtonCode.QuickMove);
		}
		else
		{
			buttonsDown.Add(ButtonCode.QuickMove);
			buttonsDownThisFrame.Add(ButtonCode.QuickMove);
		}
	}

	private void OnToggleFlashlight()
	{
		if (buttonsDown.Contains(ButtonCode.ToggleFlashlight))
		{
			buttonsDown.Remove(ButtonCode.ToggleFlashlight);
			buttonsUpThisFrame.Add(ButtonCode.ToggleFlashlight);
		}
		else
		{
			buttonsDown.Add(ButtonCode.ToggleFlashlight);
			buttonsDownThisFrame.Add(ButtonCode.ToggleFlashlight);
		}
	}

	private void OnViewAvatar()
	{
		if (buttonsDown.Contains(ButtonCode.ViewAvatar))
		{
			buttonsDown.Remove(ButtonCode.ViewAvatar);
			buttonsUpThisFrame.Add(ButtonCode.ViewAvatar);
		}
		else
		{
			buttonsDown.Add(ButtonCode.ViewAvatar);
			buttonsDownThisFrame.Add(ButtonCode.ViewAvatar);
		}
	}

	private void OnReload()
	{
		if (!ControllerComboActive)
		{
			if (buttonsDown.Contains(ButtonCode.Reload))
			{
				buttonsDown.Remove(ButtonCode.Reload);
				buttonsUpThisFrame.Add(ButtonCode.Reload);
			}
			else
			{
				buttonsDown.Add(ButtonCode.Reload);
				buttonsDownThisFrame.Add(ButtonCode.Reload);
			}
		}
	}

	private void OnCamera(InputValue value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = value.Get<Vector2>();
		if (CurrentInputDevice == InputDeviceType.Gamepad)
		{
			val *= Time.deltaTime;
		}
		CameraAxis = val;
	}

	private void OnScrollWheel(InputValue value)
	{
		MouseWheelAxis = value.Get<float>();
	}

	private void OnInventoryLeft()
	{
		if (buttonsDown.Contains(ButtonCode.InventoryLeft))
		{
			buttonsDown.Remove(ButtonCode.InventoryLeft);
			buttonsUpThisFrame.Add(ButtonCode.InventoryLeft);
		}
		else
		{
			buttonsDown.Add(ButtonCode.InventoryLeft);
			buttonsDownThisFrame.Add(ButtonCode.InventoryLeft);
		}
	}

	private void OnInventoryRight()
	{
		if (buttonsDown.Contains(ButtonCode.InventoryRight))
		{
			buttonsDown.Remove(ButtonCode.InventoryRight);
			buttonsUpThisFrame.Add(ButtonCode.InventoryRight);
		}
		else
		{
			buttonsDown.Add(ButtonCode.InventoryRight);
			buttonsDownThisFrame.Add(ButtonCode.InventoryRight);
		}
	}

	private void OnHolster()
	{
		if (buttonsDown.Contains(ButtonCode.Holster))
		{
			buttonsDown.Remove(ButtonCode.Holster);
			buttonsUpThisFrame.Add(ButtonCode.Holster);
		}
		else
		{
			buttonsDown.Add(ButtonCode.Holster);
			buttonsDownThisFrame.Add(ButtonCode.Holster);
		}
	}

	private void OnControllerCombo(InputValue value)
	{
		ControllerComboActive = value.Get<float>() == 1f;
	}

	private void OnVehicleResetCamera()
	{
		if (buttonsDown.Contains(ButtonCode.VehicleResetCamera))
		{
			buttonsDown.Remove(ButtonCode.VehicleResetCamera);
			buttonsUpThisFrame.Add(ButtonCode.VehicleResetCamera);
		}
		else
		{
			buttonsDown.Add(ButtonCode.VehicleResetCamera);
			buttonsDownThisFrame.Add(ButtonCode.VehicleResetCamera);
		}
	}

	private void OnVehicleDrive(InputValue value)
	{
		VehicleDriveAxis = value.Get<float>();
	}

	private void OnSkateboardDismount()
	{
		if (buttonsDown.Contains(ButtonCode.SkateboardDismount))
		{
			buttonsDown.Remove(ButtonCode.SkateboardDismount);
			buttonsUpThisFrame.Add(ButtonCode.SkateboardDismount);
		}
		else
		{
			buttonsDown.Add(ButtonCode.SkateboardDismount);
			buttonsDownThisFrame.Add(ButtonCode.SkateboardDismount);
		}
	}

	private void OnSkateboardMount()
	{
		if (buttonsDown.Contains(ButtonCode.SkateboardMount))
		{
			buttonsDown.Remove(ButtonCode.SkateboardMount);
			buttonsUpThisFrame.Add(ButtonCode.SkateboardMount);
		}
		else
		{
			buttonsDown.Add(ButtonCode.SkateboardMount);
			buttonsDownThisFrame.Add(ButtonCode.SkateboardMount);
		}
	}

	private void OnTogglePauseMenu()
	{
		if (buttonsDown.Contains(ButtonCode.TogglePauseMenu))
		{
			buttonsDown.Remove(ButtonCode.TogglePauseMenu);
			buttonsUpThisFrame.Add(ButtonCode.TogglePauseMenu);
		}
		else
		{
			buttonsDown.Add(ButtonCode.TogglePauseMenu);
			buttonsDownThisFrame.Add(ButtonCode.TogglePauseMenu);
		}
	}

	private void OnUINavigationDirection(InputValue value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		UINavigationDirection = value.Get<Vector2>();
	}

	private void OnUICyclePanelDirection(InputValue value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		UICyclePanelDirection = value.Get<Vector2>();
	}

	private void OnUITabNavigationPrimary(InputValue value)
	{
		UITabNavigationPrimaryAxis = value.Get<float>();
	}

	private void OnUITabNavigationSecondary(InputValue value)
	{
		UITabNavigationSecondaryAxis = value.Get<float>();
	}

	private void OnUIScrollbar(InputValue value)
	{
		UIScrollbarAxis = value.Get<float>();
	}

	private void OnUIMapNavigationDirection(InputValue value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		UIMapNavigationDirection = value.Get<Vector2>();
	}

	private void OnUIMapZoom(InputValue value)
	{
		UIMapZoomAxis = value.Get<float>();
	}

	private void OnUIModifyAmountIncrementTierOne(InputValue value)
	{
		UIModifyAmountIncrementTierOneAxis = value.Get<float>();
	}

	private void OnUIModifyAmountIncrementTierTwo(InputValue value)
	{
		UIModifyAmountIncrementTierTwoAxis = value.Get<float>();
	}

	private void OnUIModifyAmountIncrementTierThree(InputValue value)
	{
		UIModifyAmountIncrementTierThreeAxis = value.Get<float>();
	}

	public static void RegisterExitListener(ExitDelegate listener, int priority = 0)
	{
		ExitListener exitListener = new ExitListener();
		exitListener.listenerFunction = listener;
		exitListener.priority = priority;
		for (int i = 0; i < exitListeners.Count; i++)
		{
			if (priority <= exitListeners[i].priority)
			{
				exitListeners.Insert(i, exitListener);
				return;
			}
		}
		exitListeners.Add(exitListener);
	}

	public static void DeregisterExitListener(ExitDelegate listener)
	{
		for (int i = 0; i < exitListeners.Count; i++)
		{
			if (exitListeners[i].listenerFunction == listener)
			{
				exitListeners.RemoveAt(i);
				i--;
			}
		}
	}

	public InputAction GetAction(ButtonCode code)
	{
		return PlayerInput.currentActionMap.FindAction(code.ToString(), false);
	}
}
