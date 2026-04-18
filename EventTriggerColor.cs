using ScheduleOne;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UISelectable))]
public class EventTriggerColor : MonoBehaviour
{
	public Image image;

	public Color SelectedColor = Color.white;

	public Color DeselectedColor = Color.gray;

	private UISelectable selectable;

	private void Awake()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		selectable = ((Component)this).GetComponent<UISelectable>();
		selectable.OnSelected.AddListener(new UnityAction(OnSelected));
		selectable.OnDeselected.AddListener(new UnityAction(OnDeselected));
		OnDeselected();
	}

	public void OnSelected()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (GameInput.GetCurrentInputDeviceIsGamepad() && Object.op_Implicit((Object)(object)image))
		{
			((Graphic)image).color = SelectedColor;
		}
	}

	public void OnDeselected()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)image))
		{
			((Graphic)image).color = DeselectedColor;
		}
	}
}
