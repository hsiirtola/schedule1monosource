using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Settings;

public class SettingsToggle : MonoBehaviour
{
	protected Toggle toggle;

	[SerializeField]
	protected UIToggle uiToggle;

	protected virtual void Awake()
	{
		toggle = ((Component)this).GetComponent<Toggle>();
		if (Object.op_Implicit((Object)(object)toggle))
		{
			((UnityEvent<bool>)(object)toggle.onValueChanged).AddListener((UnityAction<bool>)OnValueChanged);
		}
		if (Object.op_Implicit((Object)(object)uiToggle))
		{
			uiToggle.OnChanged.AddListener((UnityAction<bool>)OnValueChanged);
		}
	}

	protected void SetIsOnWithoutNotify(bool value)
	{
		Toggle obj = toggle;
		if (obj != null)
		{
			obj.SetIsOnWithoutNotify(value);
		}
		uiToggle?.SetStateWithoutNotify(value);
	}

	protected virtual void OnValueChanged(bool value)
	{
	}
}
