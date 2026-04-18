using UnityEngine;

namespace ScheduleOne;

public class InputDescriptor : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Assign a InputDescriptorData scriptableObject. The scriptableObject should be placed in Assets/CustomUI/InputDescriptor")]
	private InputDescriptorData data;

	[SerializeField]
	[Tooltip("Assign the UITrigger component that suppose to detect and receive input when the input action from the InputDescriptorData is fired")]
	private UITrigger uiTrigger;

	public void DetectTriggerInput()
	{
		uiTrigger.DetectTriggerInput(data.InputActionReference);
	}

	public void OnReset()
	{
		uiTrigger.OnReset();
	}

	public bool GetInputTriggered()
	{
		return data.InputActionReference.action.WasPressedThisFrame();
	}

	public T GetInputValue<T>() where T : struct
	{
		return data.InputActionReference.action.ReadValue<T>();
	}
}
