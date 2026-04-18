using Funly.SkyStudio;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.FX;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class SkyProfileTransitionTrigger : MonoBehaviour
{
	public SkyProfile TransitionToOnEnter;

	public SkyProfile TransitionToOnExit;

	public float TransitionDuration = 2f;

	public void OnTriggerEnter(Collider other)
	{
		if ((Object)(object)other == (Object)(object)Player.Local.CapCol && (Object)(object)TransitionToOnEnter != (Object)null)
		{
			TimeOfDayController.instance.StartSkyProfileTransition(TransitionToOnEnter, TransitionDuration);
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if ((Object)(object)other == (Object)(object)Player.Local.CapCol && (Object)(object)TransitionToOnExit != (Object)null)
		{
			TimeOfDayController.instance.StartSkyProfileTransition(TransitionToOnExit, TransitionDuration);
		}
	}
}
