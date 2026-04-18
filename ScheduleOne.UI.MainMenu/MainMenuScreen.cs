using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.UI.MainMenu;

public class MainMenuScreen : MonoBehaviour
{
	public const float LERP_TIME = 0.075f;

	public const float LERP_SCALE = 1.25f;

	[Header("Settings")]
	public int ExitInputPriority;

	public bool OpenOnStart;

	[Header("References")]
	public MainMenuScreen PreviousScreen;

	public CanvasGroup Group;

	[Header("Custom UI")]
	public UIScreen uiScreen;

	public UIPanel uiPanel;

	private RectTransform Rect;

	private Coroutine lerpRoutine;

	public bool IsOpen { get; protected set; }

	protected virtual void Awake()
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Rect = ((Component)this).GetComponent<RectTransform>();
		GameInput.RegisterExitListener(Exit, ExitInputPriority);
		if (OpenOnStart)
		{
			Group.alpha = 1f;
			((Transform)Rect).localScale = new Vector3(1f, 1f, 1f);
			((Component)this).gameObject.SetActive(true);
			IsOpen = true;
		}
		else
		{
			Group.alpha = 0f;
			((Transform)Rect).localScale = new Vector3(1.25f, 1.25f, 1.25f);
			((Component)this).gameObject.SetActive(false);
			IsOpen = false;
		}
		if (OpenOnStart)
		{
			Singleton<MusicManager>.Instance.SetTrackEnabled("Main Menu", enabled: true);
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)Singleton<MusicManager>.Instance != (Object)null)
		{
			Singleton<MusicManager>.Instance.SetTrackEnabled("Main Menu", enabled: false);
		}
	}

	protected virtual void Exit(ExitAction action)
	{
		if (!action.Used && action.exitType != ExitType.RightClick && !((Object)(object)PreviousScreen == (Object)null) && IsOpen)
		{
			Close(openPrevious: true);
			action.Used = true;
		}
	}

	public virtual void Open(bool closePrevious)
	{
		IsOpen = true;
		Lerp(open: true);
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)((Component)this).gameObject).name);
		}
		if (closePrevious && (Object)(object)PreviousScreen != (Object)null)
		{
			PreviousScreen.Close(openPrevious: false);
		}
		if ((Object)(object)uiScreen != (Object)null)
		{
			Singleton<UIScreenManager>.Instance.AddScreen(uiScreen, Close);
			if ((Object)(object)uiPanel != (Object)null)
			{
				uiScreen.SetCurrentSelectedPanel(uiPanel);
			}
		}
	}

	private void Close()
	{
		Close(openPrevious: true);
	}

	public virtual void Close(bool openPrevious)
	{
		IsOpen = false;
		Lerp(open: false);
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)((Component)this).gameObject).name);
		}
		if (openPrevious && (Object)(object)PreviousScreen != (Object)null)
		{
			PreviousScreen.Open(closePrevious: false);
		}
		if ((Object)(object)uiScreen != (Object)null)
		{
			Singleton<UIScreenManager>.Instance.RemoveScreen(uiScreen);
		}
	}

	private void Lerp(bool open)
	{
		if (lerpRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(lerpRoutine);
		}
		if (open)
		{
			((Component)this).gameObject.SetActive(true);
		}
		if ((Object)(object)Rect == (Object)null)
		{
			Rect = ((Component)this).GetComponent<RectTransform>();
		}
		lerpRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			float startAlpha = Group.alpha;
			float startScale = ((Transform)Rect).localScale.x;
			float endAlpha = (open ? 1f : 0f);
			float endScale = (open ? 1f : 1.25f);
			float lerpTime = Mathf.Abs(startScale - endScale) / Mathf.Abs(-0.25f) * 0.075f;
			for (float i = 0f; i < lerpTime; i += Time.unscaledDeltaTime)
			{
				float num = Mathf.Lerp(startScale, endScale, i / lerpTime);
				Group.alpha = Mathf.Lerp(startAlpha, endAlpha, i / lerpTime);
				((Transform)Rect).localScale = new Vector3(num, num, num);
				yield return (object)new WaitForEndOfFrame();
			}
			Group.alpha = endAlpha;
			((Transform)Rect).localScale = new Vector3(endScale, endScale, endScale);
			lerpRoutine = null;
			if (!open)
			{
				((Component)this).gameObject.SetActive(false);
			}
		}
	}
}
