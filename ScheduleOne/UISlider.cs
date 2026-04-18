using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne;

public class UISlider : UIOption
{
	[SerializeField]
	private bool canUpdateValueText = true;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private float stepSize = 1f;

	[SerializeField]
	private TextMeshProUGUI valueText;

	public UnityEvent<float> OnChanged;

	protected override void Awake()
	{
		base.Awake();
		((UnityEvent<float>)(object)slider.onValueChanged).AddListener((UnityAction<float>)delegate
		{
			UpdateSliderChanged();
		});
		UpdateText();
	}

	protected override void OnUpdate()
	{
		DetectInput();
	}

	protected override void MoveLeft()
	{
		base.MoveLeft();
		slider.value = Mathf.Max(slider.minValue, slider.value - stepSize);
		UpdateSliderChanged();
	}

	protected override void MoveRight()
	{
		base.MoveRight();
		slider.value = Mathf.Min(slider.maxValue, slider.value + stepSize);
		UpdateSliderChanged();
	}

	private void UpdateSliderChanged()
	{
		UpdateText();
		OnChanged?.Invoke(slider.value);
	}

	private void UpdateText()
	{
		if (canUpdateValueText)
		{
			((TMP_Text)valueText).text = slider.value.ToString("0.##");
		}
	}
}
