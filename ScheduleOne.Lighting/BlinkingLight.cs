using System.Collections;
using ScheduleOne.Misc;
using UnityEngine;

namespace ScheduleOne.Lighting;

[RequireComponent(typeof(ToggleableLight))]
public class BlinkingLight : MonoBehaviour
{
	public bool IsOn;

	public float OnTime = 0.5f;

	public float OffTime = 0.5f;

	private ToggleableLight light;

	private Coroutine blinkRoutine;

	private void Awake()
	{
		light = ((Component)this).GetComponent<ToggleableLight>();
	}

	private void Update()
	{
		if (IsOn && blinkRoutine == null)
		{
			blinkRoutine = ((MonoBehaviour)this).StartCoroutine(Blink());
		}
	}

	private IEnumerator Blink()
	{
		while (IsOn)
		{
			light.isOn = true;
			yield return (object)new WaitForSeconds(OnTime);
			light.isOn = false;
			yield return (object)new WaitForSeconds(OffTime);
		}
		blinkRoutine = null;
	}
}
