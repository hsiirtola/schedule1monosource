using ScheduleOne.DevUtilities;
using ScheduleOne.TV;
using UnityEngine;

namespace ScheduleOne.UI;

public class TVPauseScreen : MonoBehaviour
{
	public TVApp App;

	public bool IsPaused { get; private set; }

	private void Awake()
	{
		GameInput.RegisterExitListener(Exit, 4);
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && IsPaused && App.IsOpen)
		{
			action.Used = true;
			Back();
		}
	}

	public void Pause()
	{
		IsPaused = true;
		((Component)this).gameObject.SetActive(true);
	}

	public void Resume()
	{
		IsPaused = false;
		((Component)this).gameObject.SetActive(false);
		App.Resume();
	}

	public void Back()
	{
		App.Close();
	}
}
