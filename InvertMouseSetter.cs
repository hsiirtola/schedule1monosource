using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InvertMouseSetter : MonoBehaviour
{
	private void Awake()
	{
		((UnityEvent<bool>)(object)((Component)this).GetComponent<Toggle>().onValueChanged).AddListener((UnityAction<bool>)delegate(bool x)
		{
			Singleton<Settings>.Instance.InvertMouse = x;
		});
	}
}
