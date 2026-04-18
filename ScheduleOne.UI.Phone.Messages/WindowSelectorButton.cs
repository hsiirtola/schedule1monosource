using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Messages;

public class WindowSelectorButton : MonoBehaviour
{
	public const float SELECTION_INDICATOR_SCALE = 1.1f;

	public const float INDICATOR_LERP_TIME = 0.075f;

	public UnityEvent OnSelected;

	public EDealWindow WindowType;

	[Header("References")]
	public Button Button;

	public GameObject InactiveOverlay;

	public RectTransform HoverIndicator;

	[Header("Custom UI")]
	public UISelectable uiSelectable;

	public EventTrigger trigger;

	private Coroutine hoverRoutine;

	private void Awake()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		((Component)HoverIndicator).gameObject.SetActive(true);
		((Transform)HoverIndicator).localScale = Vector3.one;
		((UnityEvent)Button.onClick).AddListener(new UnityAction(Clicked));
		Entry val = new Entry();
		val.eventID = (EventTriggerType)9;
		((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			HoverStart();
		});
		Entry val2 = new Entry();
		val2.eventID = (EventTriggerType)10;
		((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			HoverEnd();
		});
		trigger.triggers.Add(val);
		trigger.triggers.Add(val2);
	}

	public void SetInteractable(bool interactable)
	{
		((Selectable)Button).interactable = interactable;
		InactiveOverlay.SetActive(!interactable);
		if (!interactable)
		{
			SetHoverIndicator(shown: false);
		}
	}

	public void HoverStart()
	{
		if (((Selectable)Button).interactable)
		{
			SetHoverIndicator(shown: true);
		}
	}

	public void HoverEnd()
	{
		if (((Selectable)Button).interactable)
		{
			SetHoverIndicator(shown: false);
		}
	}

	public void Clicked()
	{
		if (OnSelected != null)
		{
			OnSelected.Invoke();
		}
	}

	public void SetHoverIndicator(bool shown)
	{
		if (hoverRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(hoverRoutine);
		}
		hoverRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			float startScale = ((Transform)HoverIndicator).localScale.x;
			float targetScale = (shown ? 1.1f : 1f);
			if (shown)
			{
				((Component)HoverIndicator).gameObject.SetActive(true);
			}
			for (float i = 0f; i < 0.075f; i += Time.deltaTime)
			{
				((Transform)HoverIndicator).localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, i / 0.075f);
				yield return (object)new WaitForEndOfFrame();
			}
			((Transform)HoverIndicator).localScale = Vector3.one * targetScale;
			if (!shown)
			{
				((Component)HoverIndicator).gameObject.SetActive(false);
			}
			hoverRoutine = null;
		}
	}
}
