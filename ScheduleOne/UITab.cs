using TMPro;
using UnityEngine;

namespace ScheduleOne;

public class UITab : UIPanel, INonNavigablePanel
{
	public enum CycleInputActionType
	{
		Primary,
		Secondary
	}

	public enum CycleDirection
	{
		Horizontal,
		Vertical
	}

	[SerializeField]
	[Tooltip("Set to true to looping of cycling behavior between the first and last selectables index.")]
	private bool allowLooping = true;

	[SerializeField]
	[Tooltip("The InputActions for cycling behavior.")]
	private CycleInputActionType cycleInputActionType;

	[SerializeField]
	[Tooltip("The InputActions for cycling behavior.")]
	private CycleDirection cycleDirection;

	[SerializeField]
	[Tooltip("UI display for cycle left")]
	private TextMeshProUGUI cycleLeftVisual;

	[SerializeField]
	[Tooltip("UI display for cycle right")]
	private TextMeshProUGUI cycleRightVisual;

	private float cycleTabTimer;

	private bool wasCycleTabPressedLastFrame;

	protected override void EarlyUpdate()
	{
		base.EarlyUpdate();
		float cycleTabInputValue = GetCycleTabInputValue();
		bool flag = cycleTabInputValue != 0f;
		if (flag)
		{
			if (!wasCycleTabPressedLastFrame)
			{
				CycleTab(cycleTabInputValue, 0.5f, 0.15f);
			}
			else
			{
				cycleTabTimer -= Time.unscaledDeltaTime;
				if (cycleTabTimer <= 0f)
				{
					CycleTab(cycleTabInputValue, 0.125f, 0.125f);
				}
			}
		}
		else
		{
			cycleTabTimer = 0f;
		}
		wasCycleTabPressedLastFrame = flag;
	}

	private float GetCycleTabInputValue()
	{
		float result = 0f;
		if (cycleInputActionType == CycleInputActionType.Primary)
		{
			result = GameInput.UITabNavigationPrimaryAxis;
		}
		else if (cycleInputActionType == CycleInputActionType.Secondary)
		{
			result = GameInput.UITabNavigationSecondaryAxis * -1f;
		}
		return result;
	}

	private void CycleTab(float navDir, float delay, float speed)
	{
		if (Navigate(navDir))
		{
			SendClickEventToCurrentSelectedSelectable();
			cycleTabTimer = delay;
			scrollSpeed = speed;
		}
	}

	private bool Navigate(float navDir)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)base.CurrentSelectedSelectable == (Object)null)
		{
			return false;
		}
		RectTransform rectTransform = base.CurrentSelectedSelectable.RectTransform;
		RectTransform component = ((Component)((Component)rectTransform).GetComponentInParent<Canvas>()).GetComponent<RectTransform>();
		Rect rect = rectTransform.rect;
		Vector2 val = Vector2.op_Implicit(((Transform)component).InverseTransformPoint(((Transform)rectTransform).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center))));
		Vector2 val2 = ((cycleDirection == CycleDirection.Horizontal) ? new Vector2(navDir, 0f) : new Vector2(0f, navDir));
		((Vector2)(ref val2)).Normalize();
		float num = float.MinValue;
		UISelectable uISelectable = null;
		float num2 = float.MinValue;
		UISelectable uISelectable2 = null;
		float num3 = float.MinValue;
		foreach (UISelectable selectable in selectables)
		{
			if ((Object)(object)selectable == (Object)(object)base.CurrentSelectedSelectable || !selectable.CanBeSelected)
			{
				continue;
			}
			RectTransform rectTransform2 = selectable.RectTransform;
			rect = rectTransform2.rect;
			Vector2 val3 = Vector2.op_Implicit(((Transform)component).InverseTransformPoint(((Transform)rectTransform2).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center)))) - val;
			Vector2 normalized = ((Vector2)(ref val3)).normalized;
			float num4 = Vector2.Dot(val2, normalized);
			if (num4 > 0f)
			{
				float magnitude = ((Vector2)(ref val3)).magnitude;
				float num5 = num4 / (magnitude + 0.01f);
				if (num5 > num)
				{
					num = num5;
					uISelectable = selectable;
				}
				if (magnitude > num2)
				{
					num2 = magnitude;
				}
			}
			else if (num4 < 0f)
			{
				float magnitude2 = ((Vector2)(ref val3)).magnitude;
				if (magnitude2 > num3)
				{
					num3 = magnitude2;
					uISelectable2 = selectable;
				}
			}
		}
		if ((Object)(object)uISelectable != (Object)null)
		{
			base.CurrentSelectedSelectable = uISelectable;
			SelectSelectable(base.CurrentSelectedSelectable);
			return true;
		}
		if (!allowLooping)
		{
			return false;
		}
		if ((Object)(object)uISelectable2 != (Object)null)
		{
			base.CurrentSelectedSelectable = uISelectable2;
			SelectSelectable(base.CurrentSelectedSelectable);
			return true;
		}
		return false;
	}

	private bool Navigate2(float navDir)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)base.CurrentSelectedSelectable == (Object)null)
		{
			return false;
		}
		RectTransform rectTransform = base.CurrentSelectedSelectable.RectTransform;
		Rect rect = rectTransform.rect;
		Vector2 val = Vector2.op_Implicit(((Transform)rectTransform).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center)));
		float num = float.MinValue;
		UISelectable uISelectable = null;
		float num2 = float.MinValue;
		UISelectable uISelectable2 = null;
		float num3 = float.MinValue;
		for (int i = 0; i < selectables.Count; i++)
		{
			UISelectable uISelectable3 = selectables[i];
			if ((Object)(object)uISelectable3 == (Object)(object)base.CurrentSelectedSelectable || !uISelectable3.CanBeSelected)
			{
				continue;
			}
			RectTransform rectTransform2 = uISelectable3.RectTransform;
			rect = rectTransform2.rect;
			Vector2 val2 = Vector2.op_Implicit(((Transform)rectTransform2).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center)));
			float num4 = float.MinValue;
			if (cycleDirection == CycleDirection.Horizontal)
			{
				float num5 = val2.x - val.x;
				if (Mathf.Sign(num5) == Mathf.Sign(navDir))
				{
					num4 = 0f - Mathf.Abs(num5);
					if (Mathf.Abs(num5) > num2)
					{
						num2 = Mathf.Abs(num5);
					}
				}
				else if (Mathf.Sign(num5) == 0f - Mathf.Sign(navDir) && Mathf.Abs(num5) > num3)
				{
					num3 = Mathf.Abs(num5);
					uISelectable2 = uISelectable3;
				}
			}
			else
			{
				float num6 = val2.y - val.y;
				if (Mathf.Sign(num6) == Mathf.Sign(navDir))
				{
					num4 = 0f - Mathf.Abs(num6);
					if (Mathf.Abs(num6) > num2)
					{
						num2 = Mathf.Abs(num6);
					}
				}
				else if (Mathf.Sign(num6) == 0f - Mathf.Sign(navDir) && Mathf.Abs(num6) > num3)
				{
					num3 = Mathf.Abs(num6);
					uISelectable2 = uISelectable3;
				}
			}
			if (num4 > num)
			{
				num = num4;
				uISelectable = uISelectable3;
			}
		}
		if ((Object)(object)uISelectable != (Object)null)
		{
			base.CurrentSelectedSelectable = uISelectable;
			SelectSelectable(base.CurrentSelectedSelectable);
			return true;
		}
		if (!allowLooping)
		{
			return false;
		}
		UISelectable uISelectable4 = uISelectable2;
		if ((Object)(object)uISelectable4 != (Object)null)
		{
			base.CurrentSelectedSelectable = uISelectable4;
			SelectSelectable(base.CurrentSelectedSelectable);
			return true;
		}
		return false;
	}

	protected override void HandleInputDeviceChanged(GameInput.InputDeviceType type)
	{
		if ((Object)(object)cycleLeftVisual != (Object)null)
		{
			((Component)cycleLeftVisual).gameObject.SetActive(type == GameInput.InputDeviceType.Gamepad && selectables.Count > 1);
		}
		if ((Object)(object)cycleRightVisual != (Object)null)
		{
			((Component)cycleRightVisual).gameObject.SetActive(type == GameInput.InputDeviceType.Gamepad && selectables.Count > 1);
		}
	}
}
