using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SensitivitySetter : MonoBehaviour
{
	private void Awake()
	{
		((UnityEvent<float>)(object)((Component)this).GetComponent<Slider>().onValueChanged).AddListener((UnityAction<float>)delegate(float x)
		{
			Singleton<Settings>.Instance.LookSensitivity = x;
		});
	}
}
