using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class SleepCanvas : Singleton<SleepCanvas>
{
	public const int MaxSleepTime = 12;

	public const int MinSleepTime = 4;

	private float QueuedMessageDisplayTime;

	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public UIScreen UIScreen;

	public RectTransform MenuContainer;

	public TextMeshProUGUI CurrentTimeLabel;

	public Button IncreaseButton;

	public Button DecreaseButton;

	public TextMeshProUGUI EndTimeLabel;

	public Button SleepButton;

	public TextMeshProUGUI SleepButtonLabel;

	public Image BlackOverlay;

	public TextMeshProUGUI SleepMessageLabel;

	public CanvasGroup SleepMessageGroup;

	public TextMeshProUGUI TimeLabel;

	public TextMeshProUGUI WakeLabel;

	public TextMeshProUGUI WaitingForHostLabel;

	public UnityEvent onSleepFullyFaded;

	public UnityEvent onSleepEndFade;

	private List<IPostSleepEvent> queuedPostSleepEvents = new List<IPostSleepEvent>();

	public bool IsMenuOpen { get; protected set; }

	public string QueuedSleepMessage { get; protected set; } = string.Empty;

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		base.Awake();
		((UnityEvent)IncreaseButton.onClick).AddListener((UnityAction)delegate
		{
			ChangeSleepAmount(1);
		});
		((UnityEvent)DecreaseButton.onClick).AddListener((UnityAction)delegate
		{
			ChangeSleepAmount(-1);
		});
		((UnityEvent)SleepButton.onClick).AddListener(new UnityAction(SleepButtonPressed));
		GameInput.RegisterExitListener(Exit, 1);
		TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
		timeManager.onSleepStart = (Action)Delegate.Combine(timeManager.onSleepStart, new Action(SleepStart));
		((Behaviour)TimeLabel).enabled = false;
		((Behaviour)WakeLabel).enabled = false;
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && IsMenuOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			SetIsOpen(open: false);
		}
	}

	public void SetIsOpen(bool open)
	{
		IsMenuOpen = open;
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		if (open)
		{
			Update();
			UpdateTimeLabels();
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			((Behaviour)Canvas).enabled = true;
			((Component)Container).gameObject.SetActive(true);
			Singleton<UIScreenManager>.Instance.AddScreen(UIScreen);
		}
		else
		{
			Player.Local.CurrentBed = null;
			Player.Local.SetReadyToSleep(ready: false);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: true, returnToOriginalRotation: false);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		}
		((Component)MenuContainer).gameObject.SetActive(open);
	}

	public void Update()
	{
		if (IsMenuOpen)
		{
			UpdateHourSetting();
			UpdateTimeLabels();
			UpdateSleepButton();
		}
		if (((Behaviour)Canvas).enabled)
		{
			((TMP_Text)CurrentTimeLabel).text = TimeManager.Get12HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime);
		}
	}

	public void AddPostSleepEvent(IPostSleepEvent postSleepEvent)
	{
		Console.Log("Adding post sleep event: " + postSleepEvent.GetType().Name);
		queuedPostSleepEvents.Add(postSleepEvent);
	}

	private void UpdateHourSetting()
	{
		((Selectable)IncreaseButton).interactable = true;
		((Selectable)DecreaseButton).interactable = true;
	}

	private void UpdateTimeLabels()
	{
		((TMP_Text)EndTimeLabel).text = TimeManager.Get12HourTime(700f);
	}

	private void UpdateSleepButton()
	{
		if (Player.Local.IsReadyToSleep)
		{
			((TMP_Text)SleepButtonLabel).text = "Waiting for others...";
		}
		else
		{
			((TMP_Text)SleepButtonLabel).text = "Sleep";
		}
	}

	private void ChangeSleepAmount(int change)
	{
		int time = TimeManager.AddMinutesTo24HourTime(700, change * 60);
		time = ClampWakeTime(time);
		UpdateHourSetting();
		UpdateTimeLabels();
	}

	private int ClampWakeTime(int time)
	{
		int currentTime = NetworkSingleton<TimeManager>.Instance.CurrentTime;
		int time2 = TimeManager.AddMinutesTo24HourTime(currentTime, 60 - currentTime % 100);
		int startTime = TimeManager.AddMinutesTo24HourTime(time2, 240);
		int endTime = TimeManager.AddMinutesTo24HourTime(time2, 720);
		return ClampTime(time, startTime, endTime);
	}

	private int ClampTime(int time, int startTime, int endTime)
	{
		if (endTime > startTime)
		{
			if (time < startTime)
			{
				return startTime;
			}
			if (time > endTime)
			{
				return endTime;
			}
		}
		else if (time < startTime && time > endTime)
		{
			int max = TimeManager.AddMinutesTo24HourTime(endTime, 720);
			if (TimeManager.IsGivenTimeWithinRange(time, endTime, max))
			{
				return endTime;
			}
			return startTime;
		}
		return time;
	}

	private void SleepButtonPressed()
	{
		Player.Local.SetReadyToSleep(!Player.Local.IsReadyToSleep);
	}

	private void SleepStart()
	{
		Player.Local.SetReadyToSleep(ready: false);
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		((Component)MenuContainer).gameObject.SetActive(false);
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		IsMenuOpen = false;
		int num = 700;
		((TMP_Text)WakeLabel).text = "Waking up at " + TimeManager.Get12HourTime(num);
		((MonoBehaviour)this).StartCoroutine(Sleep());
		IEnumerator Sleep()
		{
			((Behaviour)BlackOverlay).enabled = true;
			((TMP_Text)SleepMessageLabel).text = string.Empty;
			((Behaviour)Singleton<HUD>.Instance.canvas).enabled = false;
			LerpBlackOverlay(1f, 0.5f);
			yield return (object)new WaitForSecondsRealtime(0.5f);
			if (onSleepFullyFaded != null)
			{
				onSleepFullyFaded.Invoke();
			}
			yield return (object)new WaitForSecondsRealtime(0.5f);
			NetworkSingleton<DailySummary>.Instance.Open();
			yield return (object)new WaitUntil((Func<bool>)(() => !NetworkSingleton<DailySummary>.Instance.IsOpen));
			queuedPostSleepEvents = queuedPostSleepEvents.OrderBy((IPostSleepEvent x) => x.Order).ToList();
			foreach (IPostSleepEvent pse in queuedPostSleepEvents)
			{
				yield return (object)new WaitForSecondsRealtime(0.5f);
				Console.Log("Running post sleep event: " + pse.GetType().Name);
				pse.StartEvent();
				yield return (object)new WaitUntil((Func<bool>)(() => !pse.IsRunning));
			}
			queuedPostSleepEvents.Clear();
			if (InstanceFinder.IsServer)
			{
				NetworkSingleton<TimeManager>.Instance.SetHostSleepDone(done: true);
			}
			else
			{
				((Behaviour)WaitingForHostLabel).enabled = true;
				yield return (object)new WaitUntil((Func<bool>)(() => NetworkSingleton<TimeManager>.Instance.HostSleepDone));
				((Behaviour)WaitingForHostLabel).enabled = false;
			}
			((Behaviour)TimeLabel).enabled = true;
			yield return (object)new WaitForSecondsRealtime(1f);
			((Behaviour)TimeLabel).enabled = false;
			if (onSleepEndFade != null)
			{
				onSleepEndFade.Invoke();
			}
			if (!string.IsNullOrEmpty(QueuedSleepMessage))
			{
				yield return (object)new WaitForSecondsRealtime(0.5f);
				((TMP_Text)SleepMessageLabel).text = QueuedSleepMessage;
				QueuedSleepMessage = string.Empty;
				SleepMessageGroup.alpha = 0f;
				float lerpTime = 0.5f;
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					SleepMessageGroup.alpha = i / lerpTime;
					yield return (object)new WaitForEndOfFrame();
				}
				SleepMessageGroup.alpha = 1f;
				yield return (object)new WaitForSecondsRealtime(QueuedMessageDisplayTime);
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					SleepMessageGroup.alpha = 1f - i / lerpTime;
					yield return (object)new WaitForEndOfFrame();
				}
				SleepMessageGroup.alpha = 0f;
				yield return (object)new WaitForSecondsRealtime(0.5f);
			}
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: false);
			((Behaviour)TimeLabel).enabled = false;
			((Behaviour)WakeLabel).enabled = false;
			if (!NetworkSingleton<GameManager>.Instance.IsTutorial)
			{
				((Behaviour)Singleton<HUD>.Instance.canvas).enabled = true;
			}
			yield return (object)new WaitForSecondsRealtime(0.1f);
			if (!NetworkSingleton<GameManager>.Instance.IsTutorial)
			{
				SetIsOpen(open: false);
			}
			LerpBlackOverlay(0f, 0.5f);
		}
	}

	private void LerpBlackOverlay(float transparency, float lerpTime)
	{
		if (transparency > 0f)
		{
			((Behaviour)BlackOverlay).enabled = true;
		}
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			Color startColor = ((Graphic)BlackOverlay).color;
			Color endColor = new Color(0f, 0f, 0f, transparency);
			for (float i = 0f; i < lerpTime; i += Time.unscaledDeltaTime)
			{
				((Graphic)BlackOverlay).color = Color.Lerp(startColor, endColor, i / lerpTime);
				yield return (object)new WaitForEndOfFrame();
			}
			((Graphic)BlackOverlay).color = endColor;
			if (transparency == 0f)
			{
				((Behaviour)BlackOverlay).enabled = false;
				((Behaviour)Canvas).enabled = false;
				((Component)Container).gameObject.SetActive(false);
			}
		}
	}

	public void QueueSleepMessage(string message, float displayTime = 3f)
	{
		Console.Log("Queueing sleep message: " + message + " for " + displayTime + " seconds");
		QueuedSleepMessage = message;
		QueuedMessageDisplayTime = displayTime;
	}
}
