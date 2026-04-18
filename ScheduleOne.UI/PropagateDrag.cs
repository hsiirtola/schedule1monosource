using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI;

[RequireComponent(typeof(EventTrigger))]
public class PropagateDrag : MonoBehaviour
{
	public ScrollRect ScrollView;

	private void Start()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ScrollView == (Object)null)
		{
			ScrollView = ((Component)this).GetComponentInParent<ScrollRect>();
		}
		if (!((Object)(object)ScrollView == (Object)null))
		{
			EventTrigger component = ((Component)this).GetComponent<EventTrigger>();
			Entry val = new Entry();
			Entry val2 = new Entry();
			Entry val3 = new Entry();
			Entry val4 = new Entry();
			Entry val5 = new Entry();
			val.eventID = (EventTriggerType)13;
			((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate(BaseEventData data)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Expected O, but got Unknown
				ScrollView.OnBeginDrag((PointerEventData)data);
			});
			component.triggers.Add(val);
			val2.eventID = (EventTriggerType)5;
			((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate(BaseEventData data)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Expected O, but got Unknown
				ScrollView.OnDrag((PointerEventData)data);
			});
			component.triggers.Add(val2);
			val3.eventID = (EventTriggerType)14;
			((UnityEvent<BaseEventData>)(object)val3.callback).AddListener((UnityAction<BaseEventData>)delegate(BaseEventData data)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Expected O, but got Unknown
				ScrollView.OnEndDrag((PointerEventData)data);
			});
			component.triggers.Add(val3);
			val4.eventID = (EventTriggerType)12;
			((UnityEvent<BaseEventData>)(object)val4.callback).AddListener((UnityAction<BaseEventData>)delegate(BaseEventData data)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Expected O, but got Unknown
				ScrollView.OnInitializePotentialDrag((PointerEventData)data);
			});
			component.triggers.Add(val4);
			val5.eventID = (EventTriggerType)7;
			((UnityEvent<BaseEventData>)(object)val5.callback).AddListener((UnityAction<BaseEventData>)delegate(BaseEventData data)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Expected O, but got Unknown
				ScrollView.OnScroll((PointerEventData)data);
			});
			component.triggers.Add(val5);
		}
	}
}
