using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class ProgressSlider : Singleton<ProgressSlider>
{
	[Header("References")]
	public GameObject Container;

	public TextMeshProUGUI Label;

	public Slider Slider;

	public Image SliderFill;

	private bool progressSetThisFrame;

	private void LateUpdate()
	{
		if (progressSetThisFrame)
		{
			Container.SetActive(true);
			progressSetThisFrame = false;
		}
		else
		{
			Container.SetActive(false);
		}
	}

	public void ShowProgress(float progress)
	{
		progressSetThisFrame = true;
		Slider.value = progress;
	}

	public void Configure(string label, Color sliderFillColor)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)Label).text = label;
		((Graphic)Label).color = sliderFillColor;
		((Graphic)SliderFill).color = sliderFillColor;
	}
}
