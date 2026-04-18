using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.Tools;

public class InputFieldAttachment : MonoBehaviour
{
	private void Awake()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		InputField inputField = ((Component)this).GetComponent<InputField>();
		if ((Object)(object)inputField != (Object)null)
		{
			EventTrigger obj = ((Component)inputField).gameObject.AddComponent<EventTrigger>();
			Entry val = new Entry();
			val.eventID = (EventTriggerType)9;
			((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
			{
				EditStart(inputField.text);
			});
			obj.triggers.Add(val);
			((UnityEvent<string>)(object)inputField.onEndEdit).AddListener((UnityAction<string>)EndEdit);
		}
		TMP_InputField component = ((Component)this).GetComponent<TMP_InputField>();
		if ((Object)(object)component != (Object)null)
		{
			((UnityEvent<string>)(object)component.onSelect).AddListener((UnityAction<string>)EditStart);
			((UnityEvent<string>)(object)component.onEndEdit).AddListener((UnityAction<string>)EndEdit);
		}
	}

	private void EditStart(string newVal)
	{
		GameInput.IsTyping = true;
	}

	private void EndEdit(string newVal)
	{
		GameInput.IsTyping = false;
	}
}
