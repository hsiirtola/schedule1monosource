using System;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;

namespace ScheduleOne;

public class OnScreenMouse : Singleton<OnScreenMouse>
{
	private static readonly Vector2 CURSOR_COORDINATE_REFERENCE = new Vector2(32f, 32f);

	[Tooltip("Unity new input system virtual mouse")]
	public VirtualMouseInput ptrComponent;

	private Mouse systemMouse;

	private new void Awake()
	{
		if (Singleton<OnScreenMouse>.InstanceExists)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		Singleton<OnScreenMouse>.Instance = this;
	}

	private void OnInputDeviceChanged(GameInput.InputDeviceType type)
	{
		SetVirtualMouseEnabled(GameInput.GetCurrentInputDeviceIsGamepad());
	}

	private void OnEnable()
	{
		GameInput.OnInputDeviceChanged = (Action<GameInput.InputDeviceType>)Delegate.Combine(GameInput.OnInputDeviceChanged, new Action<GameInput.InputDeviceType>(OnInputDeviceChanged));
		SetVirtualMouseEnabled(GameInput.GetCurrentInputDeviceIsGamepad());
	}

	private void OnDisable()
	{
		GameInput.OnInputDeviceChanged = (Action<GameInput.InputDeviceType>)Delegate.Remove(GameInput.OnInputDeviceChanged, new Action<GameInput.InputDeviceType>(OnInputDeviceChanged));
		SetVirtualMouseEnabled(isOn: false);
	}

	private void Update()
	{
		UpdateSystemMouseValues();
	}

	public void Activate()
	{
		SetVirtualMouseEnabled(GameInput.GetCurrentInputDeviceIsGamepad());
		((Behaviour)this).enabled = true;
	}

	public void Deactivate()
	{
		SetVirtualMouseEnabled(isOn: false);
		((Behaviour)this).enabled = false;
	}

	public void SetTexture(Texture tex, Vector2 hotSpot)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		ptrComponent.cursorGraphic.canvasRenderer.SetTexture(tex);
		ptrComponent.cursorTransform.pivot = new Vector2(hotSpot.x / CURSOR_COORDINATE_REFERENCE.x, 1f - hotSpot.y / CURSOR_COORDINATE_REFERENCE.y);
	}

	private void SetVirtualMouseEnabled(bool isOn)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (systemMouse == null)
		{
			systemMouse = CustomUIUtils.GetSystemMouse();
		}
		((Transform)ptrComponent.cursorTransform).position = GameInput.MousePosition;
		systemMouse.WarpCursorPosition(Vector2.op_Implicit(GameInput.MousePosition));
		((Behaviour)ptrComponent).enabled = isOn;
		((Behaviour)ptrComponent.cursorGraphic).enabled = isOn;
	}

	private void UpdateSystemMouseValues()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (GameInput.GetCurrentInputDeviceIsGamepad() && systemMouse != null && ((Behaviour)ptrComponent).enabled)
		{
			MouseState val = default(MouseState);
			InputControlExtensions.CopyState<MouseState>((InputDevice)(object)systemMouse, ref val);
			InputActionProperty leftButtonAction = ptrComponent.leftButtonAction;
			((MouseState)(ref val)).WithButton((MouseButton)0, ((InputActionProperty)(ref leftButtonAction)).action.IsPressed());
			val.position = Vector2.op_Implicit(((Transform)ptrComponent.cursorTransform).position);
			InputState.Change<MouseState>((InputControl)(object)systemMouse, val, (InputUpdateType)0, default(InputEventPtr));
		}
	}
}
