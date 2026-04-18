using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class DropdownUI : Dropdown
{
	public event Action OnOpen;

	protected override void Start()
	{
		((Dropdown)this).Start();
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		((Selectable)this).OnPointerUp(eventData);
		this.OnOpen?.Invoke();
	}
}
