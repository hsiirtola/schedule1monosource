using System.Runtime.CompilerServices;
using System.Text;
using ScheduleOne;
using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class VirtualMouseDebugger : MonoBehaviour
{
	private TMP_Text msg;

	private VirtualMouseInput vmi;

	private Mouse systemMouse;

	private Mouse virtualMouse;

	private StringBuilder sb = new StringBuilder();

	private void OnEnable()
	{
		Singleton<OnScreenMouse>.Instance.Activate();
		msg = ((Component)this).GetComponent<TMP_Text>();
		vmi = Object.FindObjectOfType<VirtualMouseInput>();
		systemMouse = CustomUIUtils.GetSystemMouse();
		virtualMouse = CustomUIUtils.GetVirtualMouse();
	}

	private unsafe void Update()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown((KeyCode)104))
		{
			Singleton<OnScreenMouse>.Instance.Activate();
		}
		else if (Input.GetKeyDown((KeyCode)106))
		{
			Singleton<OnScreenMouse>.Instance.Deactivate();
		}
		sb.AppendLine($"primary click {GameInput.GetButton(GameInput.ButtonCode.PrimaryClick)}");
		sb.AppendLine($"mouse position: {Input.mousePosition}");
		sb.AppendLine($"mouse isPressed: {Input.GetMouseButton(0)}");
		sb.AppendLine($"system mouse position: {(object)System.Runtime.CompilerServices.Unsafe.Read<Vector2>((void*)((InputControl<Vector2>)(object)((Pointer)systemMouse).position).value)}");
		sb.AppendLine($"system mouse isPressed: {systemMouse.leftButton.isPressed}");
		sb.AppendLine($"virtual mouse position: {(object)System.Runtime.CompilerServices.Unsafe.Read<Vector2>((void*)((InputControl<Vector2>)(object)((Pointer)virtualMouse).position).value)}");
		StringBuilder stringBuilder = sb;
		InputActionProperty leftButtonAction = vmi.leftButtonAction;
		stringBuilder.AppendLine($"left button action: {((InputActionProperty)(ref leftButtonAction)).action.IsPressed()}");
		msg.text = sb.ToString();
		sb.Clear();
	}
}
