using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomSelectable : Selectable
{
	public UnityEvent OnSelectionEnter;

	public UnityEvent OnSelectionExit;

	public override void OnSelect(BaseEventData eventData)
	{
		((Selectable)this).OnSelect(eventData);
		UnityEvent onSelectionEnter = OnSelectionEnter;
		if (onSelectionEnter != null)
		{
			onSelectionEnter.Invoke();
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		((Selectable)this).OnDeselect(eventData);
		UnityEvent onSelectionExit = OnSelectionExit;
		if (onSelectionExit != null)
		{
			onSelectionExit.Invoke();
		}
	}
}
