using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne;

public class UIMapItem : MonoBehaviour
{
	[SerializeField]
	private UIMapPanel mapPanel;

	[SerializeField]
	private Button button;

	private RectTransform rectTransform;

	public RectTransform GetRectTransform()
	{
		return rectTransform;
	}

	private void Awake()
	{
		if ((Object)(object)mapPanel != (Object)null)
		{
			mapPanel.RegisterMapItem(this);
		}
		rectTransform = ((Component)this).GetComponent<RectTransform>();
	}

	public void SetMapPanel(UIMapPanel panel)
	{
		mapPanel = panel;
	}

	public Vector2 GetMapPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = rectTransform.rect;
		return ((Rect)(ref rect)).center;
	}

	public void OnClick()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		if ((Object)(object)button != (Object)null)
		{
			ExecuteEvents.Execute<IPointerClickHandler>(((Component)button).gameObject, (BaseEventData)new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
		}
	}

	public void OnPointerEnter()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		if ((Object)(object)button != (Object)null)
		{
			ExecuteEvents.Execute<IPointerEnterHandler>(((Component)button).gameObject, (BaseEventData)new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
		}
	}

	public void OnPointerExit()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		if ((Object)(object)button != (Object)null)
		{
			ExecuteEvents.Execute<IPointerExitHandler>(((Component)button).gameObject, (BaseEventData)new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
		}
	}
}
