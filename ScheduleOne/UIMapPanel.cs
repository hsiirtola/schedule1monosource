using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne;

public class UIMapPanel : UIPanel, INonNavigablePanel
{
	[SerializeField]
	private PinchableScrollRect mapScrollRect;

	[SerializeField]
	private float scrollSensitivity = 0.05f;

	[SerializeField]
	private float minZoomScrollSpeedMult = 1f;

	[SerializeField]
	private float maxZoomScrollSpeedMult = 2f;

	[SerializeField]
	private float zoomSensitivity = 0.1f;

	[SerializeField]
	private RectTransform centerPoint;

	private const float initialHoldThreshold = 0.5f;

	private const float repeatInterval = 0.25f;

	private float zoomTimer;

	private bool wasZoomPressedLastFrame;

	private List<UIMapItem> mapItems = new List<UIMapItem>();

	private UIMapItem snappedItem;

	private bool lockMapInput;

	public bool LockMapInput
	{
		get
		{
			return lockMapInput;
		}
		set
		{
			lockMapInput = value;
			((Component)centerPoint).gameObject.SetActive(!lockMapInput && GameInput.GetCurrentInputDeviceIsGamepad());
		}
	}

	protected override void Start()
	{
		base.Start();
		((Component)centerPoint).gameObject.SetActive(GameInput.GetCurrentInputDeviceIsGamepad());
	}

	protected override void Update()
	{
		if (!base.IsLocked && !lockInputThisFrame && !LockMapInput && base.IsSelected && !GameInput.GetCurrentInputDeviceIsKeyboardMouse())
		{
			DetectScreenInputDescriptors();
			Navigate();
			Zoom();
			SnapToNearestMapItem();
		}
	}

	private void Navigate()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		Vector2 uIMapNavigationDirection = GameInput.UIMapNavigationDirection;
		RectTransform content = ((ScrollRect)mapScrollRect).content;
		RectTransform viewport = ((ScrollRect)mapScrollRect).viewport;
		float num = Mathf.Lerp(minZoomScrollSpeedMult, maxZoomScrollSpeedMult, (mapScrollRect.GetScaleFactor() - mapScrollRect.lowerScale.x) / (mapScrollRect.upperScale.x - mapScrollRect.lowerScale.x));
		float num2 = scrollSensitivity * Time.deltaTime * num;
		Vector2 val = centerPoint.anchoredPosition + uIMapNavigationDirection * num2;
		Rect rect = viewport.rect;
		Vector2 val2 = ((Rect)(ref rect)).size * 0.5f;
		Vector2 anchoredPosition = val;
		bool flag = false;
		bool flag2 = false;
		Vector2 val3 = ((Component)mapScrollRect).GetComponent<RectTransform>().sizeDelta / 2f;
		if (val.x > val2.x + val3.x)
		{
			anchoredPosition.x = val2.x + val3.x;
			flag = true;
		}
		if (val.x < 0f - val2.x + val3.x)
		{
			anchoredPosition.x = 0f - val2.x + val3.x;
			flag = true;
		}
		if (val.y > val2.y + val3.y)
		{
			anchoredPosition.y = val2.y + val3.y;
			flag2 = true;
		}
		if (val.y < 0f - val2.y + val3.y)
		{
			anchoredPosition.y = 0f - val2.y + val3.y;
			flag2 = true;
		}
		centerPoint.anchoredPosition = anchoredPosition;
		Vector2 anchoredPosition2 = content.anchoredPosition;
		if (flag)
		{
			anchoredPosition2.x -= uIMapNavigationDirection.x * num2;
		}
		if (flag2)
		{
			anchoredPosition2.y -= uIMapNavigationDirection.y * num2;
		}
		content.anchoredPosition = anchoredPosition2;
	}

	private void Zoom()
	{
		float uIMapZoomAxis = GameInput.UIMapZoomAxis;
		bool flag = uIMapZoomAxis != 0f;
		if (flag)
		{
			if (!wasZoomPressedLastFrame)
			{
				float num = uIMapZoomAxis * zoomSensitivity;
				mapScrollRect.ControlZoom(num);
				zoomTimer = 0.5f;
			}
			else
			{
				zoomTimer -= Time.unscaledDeltaTime;
				if (zoomTimer <= 0f)
				{
					float num2 = uIMapZoomAxis * zoomSensitivity;
					mapScrollRect.ControlZoom(num2);
					zoomTimer += 0.25f;
				}
			}
		}
		else
		{
			zoomTimer = 0f;
		}
		wasZoomPressedLastFrame = flag;
	}

	public void RegisterMapItem(UIMapItem item)
	{
		if (!mapItems.Contains(item))
		{
			mapItems.Add(item);
		}
	}

	public void DeregisterMapItem(UIMapItem item)
	{
		if (mapItems.Contains(item))
		{
			mapItems.Remove(item);
		}
	}

	public void SetSnappedItem(UIMapItem newItem)
	{
		if ((Object)(object)snappedItem != (Object)(object)newItem)
		{
			snappedItem = newItem;
			snappedItem.OnClick();
		}
	}

	public void ResetSnappedItem()
	{
		if ((Object)(object)snappedItem != (Object)null)
		{
			snappedItem.OnPointerExit();
			snappedItem = null;
		}
	}

	private void SnapToNearestMapItem()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)centerPoint == (Object)null || mapItems.Count == 0)
		{
			return;
		}
		Vector2 val = Vector2.op_Implicit(((Transform)centerPoint).position);
		Vector2 val2 = RectTransformUtility.WorldToScreenPoint((Camera)null, Vector2.op_Implicit(val));
		UIMapItem uIMapItem = null;
		float num = float.MaxValue;
		foreach (UIMapItem mapItem in mapItems)
		{
			if (!((Component)mapItem).gameObject.activeInHierarchy)
			{
				continue;
			}
			RectTransform rectTransform = mapItem.GetRectTransform();
			if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, val2, (Camera)null))
			{
				Vector2 val3 = RectTransformUtility.WorldToScreenPoint((Camera)null, ((Transform)rectTransform).position);
				float num2 = Vector2.Distance(val2, val3);
				if (num2 < num || (Mathf.Approximately(num2, num) && ((Transform)rectTransform).GetSiblingIndex() > ((Transform)uIMapItem.GetRectTransform()).GetSiblingIndex()))
				{
					num = num2;
					uIMapItem = mapItem;
				}
			}
			else
			{
				mapItem.OnPointerExit();
				if ((Object)(object)mapItem == (Object)(object)snappedItem)
				{
					snappedItem = null;
				}
			}
		}
		if ((Object)(object)uIMapItem != (Object)null && (Object)(object)uIMapItem != (Object)(object)snappedItem)
		{
			SnapMapToItem(uIMapItem);
			snappedItem = uIMapItem;
			snappedItem.OnClick();
			snappedItem.OnPointerEnter();
		}
	}

	private void SnapMapToItem(UIMapItem item)
	{
	}

	protected override void HandleInputDeviceChanged(GameInput.InputDeviceType type)
	{
		base.HandleInputDeviceChanged(type);
		((Component)centerPoint).gameObject.SetActive(type == GameInput.InputDeviceType.Gamepad);
	}
}
