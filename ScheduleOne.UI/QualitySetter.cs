using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI;

public class QualitySetter : MonoBehaviour
{
	private void Awake()
	{
		((UnityEvent<int>)(object)((Component)this).GetComponent<TMP_Dropdown>().onValueChanged).AddListener((UnityAction<int>)delegate(int x)
		{
			SetQuality(x);
		});
	}

	private void SetQuality(int quality)
	{
		Console.Log("Setting quality to " + quality);
		QualitySettings.SetQualityLevel(quality);
	}
}
