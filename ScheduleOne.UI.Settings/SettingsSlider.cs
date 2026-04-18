using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Settings;

public class SettingsSlider : MonoBehaviour
{
	public float ValueDisplayTime = 2f;

	public bool DisplayValue = true;

	protected Slider slider;

	[SerializeField]
	protected TextMeshProUGUI valueLabel;

	protected float timeOnValueChange = -100f;

	protected virtual void Awake()
	{
		slider = ((Component)this).GetComponent<Slider>();
		((UnityEvent<float>)(object)slider.onValueChanged).AddListener((UnityAction<float>)OnValueChanged);
		if (!Object.op_Implicit((Object)(object)valueLabel))
		{
			valueLabel = ((Component)((Transform)slider.handleRect).Find("Value")).GetComponent<TextMeshProUGUI>();
		}
	}

	protected virtual void Update()
	{
		if (!(ValueDisplayTime <= 0f) && DisplayValue && Time.time - timeOnValueChange > ValueDisplayTime)
		{
			((Behaviour)valueLabel).enabled = false;
		}
	}

	protected virtual void OnValueChanged(float value)
	{
		timeOnValueChange = Time.time;
		SetDisplayValue(value);
	}

	protected void SetDisplayValue(float value)
	{
		if (DisplayValue)
		{
			((TMP_Text)valueLabel).text = GetDisplayValue(value);
			((Behaviour)valueLabel).enabled = true;
		}
	}

	protected void SetValueWithoutNotify(float value)
	{
		slider.SetValueWithoutNotify(value);
		SetDisplayValue(value);
	}

	protected virtual string GetDisplayValue(float value)
	{
		return value.ToString();
	}
}
