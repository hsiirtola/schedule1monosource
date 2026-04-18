using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class NotificationsManager : Singleton<NotificationsManager>
{
	public const int MAX_NOTIFICATIONS = 6;

	[Header("References")]
	public RectTransform EntryContainer;

	public AudioSourceController Sound;

	[Header("Prefab")]
	public GameObject NotificationPrefab;

	private Dictionary<RectTransform, Coroutine> coroutines = new Dictionary<RectTransform, Coroutine>();

	private List<RectTransform> entries = new List<RectTransform>();

	public void SendNotification(string title, string subtitle, Sprite icon, float duration = 5f, bool playSound = true)
	{
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)("New notification: " + title + " - " + subtitle + " - " + ((object)icon)?.ToString() + " - " + duration));
		RectTransform newEntry = Object.Instantiate<GameObject>(NotificationPrefab, (Transform)(object)EntryContainer).GetComponent<RectTransform>();
		((Transform)newEntry).SetAsLastSibling();
		RectTransform container = ((Component)((Transform)newEntry).Find("Container")).GetComponent<RectTransform>();
		((TMP_Text)((Component)((Transform)container).Find("Title")).GetComponent<TextMeshProUGUI>()).text = title;
		((TMP_Text)((Component)((Transform)container).Find("Subtitle")).GetComponent<TextMeshProUGUI>()).text = subtitle;
		((Component)((Transform)container).Find("AppIcon/Mask/Image")).GetComponent<Image>().sprite = icon;
		float startX = -200f;
		float endX = 0f;
		float lerpTime = 0.15f;
		container.anchoredPosition = new Vector2(startX, container.anchoredPosition.y);
		if (playSound && !Sound.IsPlaying)
		{
			Sound.Play();
		}
		if (entries.Count >= 6)
		{
			RectTransform val = entries[0];
			if ((Object)(object)val != (Object)null)
			{
				((MonoBehaviour)this).StopCoroutine(coroutines[val]);
				coroutines.Remove(val);
				Object.Destroy((Object)(object)((Component)val).gameObject);
			}
			entries.RemoveAt(0);
		}
		coroutines.Add(newEntry, ((MonoBehaviour)this).StartCoroutine(Routine()));
		entries.Add(newEntry);
		IEnumerator Routine()
		{
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				container.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, i / lerpTime), container.anchoredPosition.y);
				yield return (object)new WaitForEndOfFrame();
			}
			container.anchoredPosition = new Vector2(endX, container.anchoredPosition.y);
			yield return (object)new WaitForSeconds(duration);
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				container.anchoredPosition = new Vector2(Mathf.Lerp(endX, startX, i / lerpTime), container.anchoredPosition.y);
				yield return (object)new WaitForEndOfFrame();
			}
			if ((Object)(object)container != (Object)null && coroutines.ContainsKey(container))
			{
				coroutines.Remove(container);
			}
			Object.Destroy((Object)(object)((Component)newEntry).gameObject);
			coroutines.Remove(container);
			entries.Remove(newEntry);
		}
	}
}
