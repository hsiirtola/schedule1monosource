using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace ScheduleOne;

public static class CustomUIUtils
{
	public static Mouse GetSystemMouse()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlyArray<InputDevice> devices = InputSystem.devices;
		for (int i = 0; i < devices.Count; i++)
		{
			InputDevice val = devices[i];
			if (val.native)
			{
				Mouse val2 = (Mouse)(object)((val is Mouse) ? val : null);
				if (val2 != null)
				{
					return val2;
				}
			}
		}
		return null;
	}

	public static Mouse GetVirtualMouse()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlyArray<InputDevice> devices = InputSystem.devices;
		for (int i = 0; i < devices.Count; i++)
		{
			InputDevice val = devices[i];
			if (!val.native)
			{
				Mouse val2 = (Mouse)(object)((val is Mouse) ? val : null);
				if (val2 != null)
				{
					return val2;
				}
			}
		}
		return null;
	}
}
