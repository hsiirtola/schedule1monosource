using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIScaleSetter : MonoBehaviour
{
	private void Awake()
	{
		((UnityEvent<float>)(object)((Component)this).GetComponent<Slider>().onValueChanged).AddListener((UnityAction<float>)delegate(float x)
		{
			CanvasScaler.SetScaleFactor(x);
		});
	}
}
