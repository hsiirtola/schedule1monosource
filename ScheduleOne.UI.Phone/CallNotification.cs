using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone;

public class CallNotification : Singleton<CallNotification>
{
	public const float TIME_PER_CHAR = 0.015f;

	[Header("References")]
	public RectTransform Container;

	public Image ProfilePicture;

	public CanvasGroup Group;

	private Coroutine slideRoutine;

	public PhoneCallData ActiveCallData { get; private set; }

	public bool IsOpen { get; protected set; }

	protected override void Awake()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		Group.alpha = 0f;
		Container.anchoredPosition = new Vector2(-600f, 0f);
		((Component)Container).gameObject.SetActive(false);
	}

	public void SetIsOpen(bool visible, CallerID caller)
	{
		IsOpen = visible;
		if (slideRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(slideRoutine);
		}
		slideRoutine = ((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (visible)
			{
				((Component)Container).gameObject.SetActive(true);
			}
			if ((Object)(object)caller != (Object)null)
			{
				ProfilePicture.sprite = caller.ProfilePicture;
			}
			float startX = Container.anchoredPosition.x;
			float endX = (visible ? 0f : (-600f));
			float startAlpha = Group.alpha;
			float endAlpha = (visible ? 1f : 0f);
			float lerpTime = 0.25f;
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				Container.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, i / lerpTime), 0f);
				Group.alpha = Mathf.Lerp(startAlpha, endAlpha, i / lerpTime);
				yield return (object)new WaitForEndOfFrame();
			}
			Container.anchoredPosition = new Vector2(endX, 0f);
			Group.alpha = endAlpha;
			if (!visible)
			{
				((Component)Container).gameObject.SetActive(false);
			}
			slideRoutine = null;
		}
	}
}
