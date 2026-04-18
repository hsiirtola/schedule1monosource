using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.TV;

public class TVApp : MonoBehaviour
{
	public const float SCALE_MIN = 0.67f;

	public const float SCALE_MAX = 1.5f;

	public const float LERP_TIME = 0.12f;

	[Header("Settings")]
	public bool CanClose = true;

	public string AppName;

	public Sprite Icon;

	public bool Pauseable = true;

	[Header("References")]
	public Canvas Canvas;

	[HideInInspector]
	public TVApp PreviousScreen;

	public CanvasGroup CanvasGroup;

	public TVPauseScreen PauseScreen;

	private Coroutine lerpCoroutine;

	public bool IsOpen { get; private set; }

	public bool IsPaused
	{
		get
		{
			if ((Object)(object)PauseScreen != (Object)null)
			{
				return PauseScreen.IsPaused;
			}
			return false;
		}
	}

	protected virtual void Awake()
	{
		GameInput.RegisterExitListener(Exit, 3);
		CanvasGroup.alpha = 0f;
	}

	private void OnDestroy()
	{
		GameInput.DeregisterExitListener(Exit);
	}

	public virtual void Open()
	{
		IsOpen = true;
		((Component)Canvas).gameObject.SetActive(true);
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(ActiveMinPass);
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(ActiveMinPass);
		Lerp(1f, 1f);
	}

	public virtual void Close()
	{
		IsOpen = false;
		((Component)Canvas).gameObject.SetActive(false);
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(ActiveMinPass);
		if ((Object)(object)PreviousScreen != (Object)null)
		{
			Lerp(0.67f, 0f);
		}
		else
		{
			Lerp(1.5f, 0f);
		}
		if ((Object)(object)PreviousScreen != (Object)null)
		{
			PreviousScreen.Open();
		}
	}

	public virtual void Resume()
	{
	}

	private void Lerp(float endScale, float endAlpha)
	{
		if (lerpCoroutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(lerpCoroutine);
		}
		lerpCoroutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Lerp(endScale, endAlpha));
		IEnumerator Lerp(float num, float num2)
		{
			if (!((Object)(object)Canvas == (Object)null))
			{
				((Component)Canvas).gameObject.SetActive(true);
				float startScale = ((Component)Canvas).transform.localScale.x;
				float startAlpha = CanvasGroup.alpha;
				float lerpTime = Mathf.Abs(num - startScale) / 0.5f * 0.12f;
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					if ((Object)(object)Canvas == (Object)null)
					{
						yield break;
					}
					((Component)Canvas).transform.localScale = Vector3.one * Mathf.Lerp(startScale, num, i / lerpTime);
					CanvasGroup.alpha = Mathf.Lerp(startAlpha, num2, i / lerpTime);
					yield return (object)new WaitForEndOfFrame();
				}
				if ((Object)(object)Canvas != (Object)null)
				{
					((Component)Canvas).transform.localScale = Vector3.one * num;
					CanvasGroup.alpha = num2;
					if (num2 == 0f)
					{
						((Component)Canvas).gameObject.SetActive(false);
					}
				}
				lerpCoroutine = null;
			}
		}
	}

	protected virtual void ActiveMinPass()
	{
	}

	private void Exit(ExitAction action)
	{
		if (action.Used || !IsOpen)
		{
			return;
		}
		if (CanClose || Pauseable)
		{
			action.Used = true;
			if (Pauseable && (Object)(object)PauseScreen != (Object)null)
			{
				TryPause();
			}
			else
			{
				Close();
			}
		}
		else
		{
			PreviousScreen.Open();
		}
	}

	protected virtual void TryPause()
	{
		PauseScreen.Pause();
	}
}
