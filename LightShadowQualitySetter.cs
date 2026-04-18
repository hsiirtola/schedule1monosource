using System.Collections.Generic;
using UnityEngine;

public class LightShadowQualitySetter : MonoBehaviour
{
	private Dictionary<Light, LightShadows> originalLightShadows = new Dictionary<Light, LightShadows>();

	private void Awake()
	{
		QualitySettings.activeQualityLevelChanged += OnQualityChange;
	}

	private void OnDestroy()
	{
		QualitySettings.activeQualityLevelChanged -= OnQualityChange;
	}

	private void Start()
	{
		ToggleAdditionalLightShadows(QualitySettings.GetQualityLevel() > 1);
	}

	private void OnQualityChange(int previousIdx, int newIdx)
	{
		if (previousIdx < 2 && newIdx > 1)
		{
			ToggleAdditionalLightShadows(on: true);
		}
		else if (previousIdx > 1 && newIdx < 2)
		{
			ToggleAdditionalLightShadows(on: false);
		}
	}

	private void ToggleAdditionalLightShadows(bool on)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Light[] array = Object.FindObjectsOfType<Light>();
		foreach (Light val in array)
		{
			string name = ((Object)((Component)((Component)val).transform).gameObject).name;
			if (!(name == "Sun") && !(name == "Moon"))
			{
				originalLightShadows.TryAdd(val, val.shadows);
				val.shadows = (LightShadows)(on ? ((int)originalLightShadows[val]) : 0);
			}
		}
		Debug.Log((object)$"AdditionalLightShadows have been set to {on}");
	}
}
