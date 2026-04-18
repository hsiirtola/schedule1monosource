using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class EventTriggerContact : MonoBehaviour
{
	public GameObject selectedImage;

	private void Awake()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		EventTrigger val = ((Component)this).GetComponent<EventTrigger>();
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)this).gameObject.AddComponent<EventTrigger>();
		}
		Entry val2 = new Entry
		{
			eventID = (EventTriggerType)4
		};
		((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			TestFunc();
		});
		val.triggers.Add(val2);
		Entry val3 = new Entry
		{
			eventID = (EventTriggerType)9
		};
		((UnityEvent<BaseEventData>)(object)val3.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			TestFunc();
		});
		val.triggers.Add(val3);
	}

	public void TestFunc()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		selectedImage.transform.position = ((Component)this).transform.position;
		selectedImage.SetActive(true);
	}
}
