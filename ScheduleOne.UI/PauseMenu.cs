using System;
using System.Collections;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.MainMenu;
using UnityEngine;

namespace ScheduleOne.UI;

public class PauseMenu : Singleton<PauseMenu>
{
	public Canvas Canvas;

	public RectTransform Container;

	public MainMenuScreen Screen;

	public FeedbackForm FeedbackForm;

	[Header("Custom UI")]
	public UIScreen uiScreen;

	public UIPanel uiPanel;

	private bool justPaused;

	private bool justResumed;

	private bool couldLook;

	private bool lockedMouse;

	private bool crosshairVisible;

	private bool hudVisible;

	public Action onPause;

	public Action onResume;

	private bool _togglePausePressed;

	private bool _backWasTriggeredThisFrame;

	public bool IsPaused { get; protected set; }

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && action.exitType != ExitType.RightClick && !justResumed && !GameInput.IsTyping)
		{
			if (IsPaused)
			{
				Resume();
			}
			else
			{
				Pause();
			}
		}
	}

	private void Update()
	{
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			if (GameInput.GetButtonDown(GameInput.ButtonCode.TogglePauseMenu))
			{
				_togglePausePressed = true;
			}
			_backWasTriggeredThisFrame = UIScreenManager.IsBackTriggeredThisFrame;
		}
	}

	private void LateUpdate()
	{
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			CheckTogglePause();
			_togglePausePressed = false;
			justPaused = false;
			justResumed = false;
		}
	}

	private void CheckTogglePause()
	{
		if (_togglePausePressed && !GameInput.TogglePauseInputUsed && !justResumed && !GameInput.IsTyping && !UIScreenManager.IsBackTriggeredThisFrame && !_backWasTriggeredThisFrame)
		{
			if (IsPaused)
			{
				Resume();
			}
			else
			{
				Pause();
			}
		}
	}

	public void Pause()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Invalid comparison between Unknown and I4
		Console.Log("Game paused");
		IsPaused = true;
		justPaused = true;
		if ((Object)(object)FeedbackForm != (Object)null)
		{
			FeedbackForm.PrepScreenshot();
		}
		if (Singleton<ScheduleOne.DevUtilities.Settings>.InstanceExists && Singleton<ScheduleOne.DevUtilities.Settings>.Instance.PausingFreezesTime)
		{
			Time.timeScale = 0f;
		}
		((Behaviour)Canvas).enabled = true;
		((Component)Container).gameObject.SetActive(true);
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			couldLook = PlayerSingleton<PlayerCamera>.Instance.canLook;
			lockedMouse = (int)Cursor.lockState == 1;
			crosshairVisible = ((Component)Singleton<HUD>.Instance.crosshair).gameObject.activeSelf;
			hudVisible = ((Behaviour)Singleton<HUD>.Instance.canvas).enabled;
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0.075f);
			((Behaviour)Singleton<HUD>.Instance.canvas).enabled = false;
		}
		Screen.Open(closePrevious: false);
		Singleton<UIScreenManager>.Instance.AddScreen(uiScreen, Resume);
		((MonoBehaviour)this).StartCoroutine(DelayPanelSelect());
	}

	private IEnumerator DelayPanelSelect()
	{
		yield return null;
		uiScreen.SetCurrentSelectedPanel(uiPanel);
		uiPanel.SelectSelectable(returnFirstFound: true);
	}

	public void Resume()
	{
		if (justPaused)
		{
			return;
		}
		Console.Log("Game resumed");
		IsPaused = false;
		justResumed = true;
		if (Singleton<ScheduleOne.DevUtilities.Settings>.InstanceExists && Singleton<ScheduleOne.DevUtilities.Settings>.Instance.PausingFreezesTime)
		{
			Time.timeScale = 1f;
		}
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			if (couldLook)
			{
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			}
			if (lockedMouse && (!Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.InstanceExists || !Singleton<ScheduleOne.AvatarFramework.Customization.CharacterCreator>.Instance.IsOpen))
			{
				PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			}
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0.075f);
		}
		if (Singleton<HUD>.InstanceExists)
		{
			Singleton<HUD>.Instance.SetCrosshairVisible(crosshairVisible);
			((Behaviour)Singleton<HUD>.Instance.canvas).enabled = hudVisible;
		}
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		Screen.Close(openPrevious: false);
		Singleton<UIScreenManager>.Instance.RemoveScreen(uiScreen);
	}

	public void StuckButtonClicked()
	{
		Resume();
		PlayerSingleton<PlayerMovement>.Instance.WarpToNavMesh();
	}
}
