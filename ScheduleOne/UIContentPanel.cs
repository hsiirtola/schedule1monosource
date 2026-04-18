using UnityEngine;

namespace ScheduleOne;

public class UIContentPanel : UIPanel
{
	[SerializeField]
	[Tooltip("Default is ImmediateDirection. ImmediatelyDirection is suitable if selectables are placed in grid format. NearestDirectionAndDistance is suitable for non-grid format")]
	private UINavigationType uiPanelNavigationType;

	protected override void DetectInput()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Vector2 uINavigationDirection = GameInput.UINavigationDirection;
		if (preventSideNavigation)
		{
			if (Mathf.Abs(uINavigationDirection.x) > Mathf.Abs(uINavigationDirection.y))
			{
				uINavigationDirection.y = 0f;
			}
			uINavigationDirection.x = 0f;
		}
		bool flag = uINavigationDirection != Vector2.zero;
		if (flag)
		{
			if (!wasNavPressedLastFrame)
			{
				if (Navigate(uINavigationDirection))
				{
					navTimer = 0.5f;
					scrollSpeed = 0.15f;
				}
			}
			else
			{
				navTimer -= Time.unscaledDeltaTime;
				if (navTimer <= 0f && Navigate(uINavigationDirection))
				{
					navTimer = 0.125f;
					scrollSpeed = 0.125f;
				}
			}
		}
		else
		{
			navTimer = 0f;
		}
		wasNavPressedLastFrame = flag;
	}

	protected override bool Navigate(Vector2 navDir)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Invalid comparison between Unknown and I4
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Invalid comparison between Unknown and I4
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Invalid comparison between Unknown and I4
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)base.CurrentSelectedSelectable == (Object)null)
		{
			return false;
		}
		Canvas componentInParent = ((Component)base.CurrentSelectedSelectable.RectTransform).GetComponentInParent<Canvas>();
		Camera val = componentInParent.worldCamera;
		if ((Object)(object)val == (Object)null)
		{
			val = Camera.main;
		}
		Vector2 normalized = ((Vector2)(ref navDir)).normalized;
		float num = float.MinValue;
		UISelectable uISelectable = null;
		bool flag = Mathf.Abs(normalized.x) > Mathf.Abs(normalized.y);
		Rect rect;
		Vector2 val3;
		if ((int)componentInParent.renderMode == 2)
		{
			RectTransform rectTransform = base.CurrentSelectedSelectable.RectTransform;
			rect = base.CurrentSelectedSelectable.RectTransform.rect;
			Vector3 val2 = ((Transform)rectTransform).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center));
			val3 = Vector2.op_Implicit(val.WorldToScreenPoint(val2));
		}
		else
		{
			RectTransform rectTransform2 = base.CurrentSelectedSelectable.RectTransform;
			RectTransform component = ((Component)componentInParent).GetComponent<RectTransform>();
			rect = rectTransform2.rect;
			val3 = Vector2.op_Implicit(((Transform)component).InverseTransformPoint(((Transform)rectTransform2).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center))));
		}
		foreach (UISelectable selectable in selectables)
		{
			if ((Object)(object)selectable == (Object)(object)base.CurrentSelectedSelectable || !selectable.CanBeSelected)
			{
				continue;
			}
			Vector2 val5;
			if ((int)componentInParent.renderMode == 2)
			{
				RectTransform rectTransform3 = selectable.RectTransform;
				rect = selectable.RectTransform.rect;
				Vector3 val4 = ((Transform)rectTransform3).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center));
				val5 = Vector2.op_Implicit(val.WorldToScreenPoint(val4));
			}
			else
			{
				RectTransform rectTransform4 = selectable.RectTransform;
				RectTransform component2 = ((Component)componentInParent).GetComponent<RectTransform>();
				rect = rectTransform4.rect;
				val5 = Vector2.op_Implicit(((Transform)component2).InverseTransformPoint(((Transform)rectTransform4).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center))));
			}
			Vector2 val6 = val5 - val3;
			Vector2 normalized2 = ((Vector2)(ref val6)).normalized;
			float num2 = Vector2.Dot(normalized, normalized2);
			if (num2 <= 0f)
			{
				continue;
			}
			float num3 = float.MinValue;
			if (uiPanelNavigationType == UINavigationType.NearestDirectionAndDistance || (int)componentInParent.renderMode == 2)
			{
				float num4 = Vector2.Distance(val5, val3);
				num3 = num2 / (num4 + 0.01f);
			}
			else if (uiPanelNavigationType == UINavigationType.ImmediateDirection)
			{
				if (flag)
				{
					if ((normalized.x > 0f && val6.x <= 0f) || (normalized.x < 0f && val6.x >= 0f))
					{
						continue;
					}
					float num5 = Mathf.Abs(val6.x);
					float num6 = Mathf.Abs(val6.y) * 0.25f;
					num3 = 1f / (num5 + num6 + 0.01f);
				}
				else
				{
					if ((normalized.y > 0f && val6.y <= 0f) || (normalized.y < 0f && val6.y >= 0f))
					{
						continue;
					}
					float num7 = Mathf.Abs(val6.y);
					float num8 = Mathf.Abs(val6.x) * 0.25f;
					num3 = 1f / (num7 + num8 + 0.01f);
				}
			}
			if (num3 > num)
			{
				num = num3;
				uISelectable = selectable;
			}
		}
		if ((Object)(object)uISelectable != (Object)null)
		{
			base.CurrentSelectedSelectable = uISelectable;
			SelectSelectable(base.CurrentSelectedSelectable);
			if ((Object)(object)scrollRect != (Object)null)
			{
				ScrollToChild(base.CurrentSelectedSelectable.RectTransform, scrollSpeed);
			}
			return true;
		}
		return NavigateUsingCyclePanel(navDir);
	}
}
