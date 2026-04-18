using ScheduleOne.AvatarFramework;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Cutscenes;

public class EndCutscene : Cutscene
{
	public UnityEvent onStandUp;

	public UnityEvent onRunStart;

	public UnityEvent onEngineStart;

	public UnityEvent onLightsOn;

	public Avatar Avatar;

	public override void Play()
	{
		base.Play();
		Avatar.LoadAvatarSettings(Player.Local.Avatar.CurrentSettings);
	}

	public void StandUp()
	{
		if (onStandUp != null)
		{
			onStandUp.Invoke();
		}
	}

	public void RunStart()
	{
		if (onRunStart != null)
		{
			onRunStart.Invoke();
		}
	}

	public void EngineStart()
	{
		if (onEngineStart != null)
		{
			onEngineStart.Invoke();
		}
	}

	public void LightsOn()
	{
		if (onLightsOn != null)
		{
			onLightsOn.Invoke();
		}
	}

	public void On3rdPerson()
	{
		((Component)Avatar).gameObject.SetActive(true);
		Avatar.Animation.SetBool("Sitting", value: true);
	}
}
