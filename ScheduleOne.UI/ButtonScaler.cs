using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(EventTrigger))]
public class ButtonScaler : MonoBehaviour
{
	public RectTransform ScaleTarget;

	public float HoverScale = 1.1f;

	public float ScaleTime = 0.1f;

	private Coroutine scaleCoroutine;

	private Button button;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		button = ((Component)this).GetComponent<Button>();
		EventTrigger component = ((Component)this).GetComponent<EventTrigger>();
		Entry val = new Entry();
		val.eventID = (EventTriggerType)0;
		((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			Hovered();
		});
		component.triggers.Add(val);
		Entry val2 = new Entry();
		val2.eventID = (EventTriggerType)1;
		((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			HoverEnd();
		});
		component.triggers.Add(val2);
	}

	private void Hovered()
	{
		if (((Selectable)button).interactable)
		{
			SetScale(HoverScale);
		}
	}

	private void HoverEnd()
	{
		if (((Selectable)button).interactable)
		{
			SetScale(1f);
		}
	}

	private void SetScale(float endScale)
	{
		if (scaleCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(scaleCoroutine);
		}
		scaleCoroutine = ((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			float startScale = ((Transform)ScaleTarget).localScale.x;
			float lerpTime = Mathf.Abs(startScale - endScale) / Mathf.Abs(1f - HoverScale) * ScaleTime;
			for (float i = 0f; i < lerpTime; i += Time.unscaledDeltaTime)
			{
				float num = Mathf.Lerp(startScale, endScale, i / lerpTime);
				((Transform)ScaleTarget).localScale = new Vector3(num, num, num);
				yield return (object)new WaitForEndOfFrame();
			}
			((Transform)ScaleTarget).localScale = new Vector3(endScale, endScale, endScale);
			scaleCoroutine = null;
		}
	}
}
