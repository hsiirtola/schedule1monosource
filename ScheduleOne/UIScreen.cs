using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne;

public class UIScreen : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Manually assign the UIPanel attached to this screen in editor.")]
	private List<UIPanel> panels = new List<UIPanel>();

	[SerializeField]
	[Tooltip("When selected, the input action in the inputDescriptor list will be active")]
	private List<InputDescriptor> inputDescriptors = new List<InputDescriptor>();

	[SerializeField]
	[Tooltip("Each screen support 1 active scroll rect to scroll. You can use uiScreen.ChangeActiveScrollRect(newScrollRect) to change the active scroll rect via script at runtime.")]
	private ScrollRect activeScrollRect;

	[SerializeField]
	[Tooltip("Add this screen to UIScreenManger on Start")]
	private bool addScreenOnStart;

	[SerializeField]
	[Tooltip("Add this screen to UIScreenManger on OnEnable")]
	private bool addScreenOnEnable;

	[SerializeField]
	[Tooltip("Remove this screen from UIScreenManger on OnDisable")]
	private bool removeScreenOnDisable;

	private UIPanel currentSelectedPanel;

	private bool isSelected;

	private bool wasNavPressedLastFrame;

	public bool IsSelected
	{
		get
		{
			return isSelected;
		}
		set
		{
			isSelected = value;
			foreach (UIPanel panel in panels)
			{
				panel.IsLocked = !value;
				panel.OnReset();
			}
			foreach (InputDescriptor inputDescriptor in inputDescriptors)
			{
				inputDescriptor.OnReset();
			}
			if (isSelected)
			{
				SetCurrentSelectedPanel();
			}
		}
	}

	public UIPanel CurrentSelectedPanel => currentSelectedPanel;

	public IReadOnlyList<UIPanel> Panels => panels.AsReadOnly();

	private void Awake()
	{
		foreach (UIPanel panel in panels)
		{
			panel.SetParentScreen(this);
		}
		OnAwake();
	}

	protected virtual void OnAwake()
	{
	}

	private void Start()
	{
		if (addScreenOnStart)
		{
			InitScreen();
		}
		OnStarted();
	}

	protected virtual void OnStarted()
	{
	}

	private void OnEnable()
	{
		if (addScreenOnEnable)
		{
			InitScreen();
		}
	}

	private void OnDisable()
	{
		if (removeScreenOnDisable)
		{
			Singleton<UIScreenManager>.Instance?.RemoveScreen(this);
		}
	}

	private void OnDestroy()
	{
		Singleton<UIScreenManager>.Instance?.RemoveScreen(this);
		OnDestroyed();
	}

	protected virtual void OnDestroyed()
	{
	}

	protected virtual void Update()
	{
		if (IsSelected && panels.Count != 0 && !GameInput.GetCurrentInputDeviceIsKeyboardMouse() && !((Object)(object)currentSelectedPanel == (Object)null))
		{
			DetectInput();
			DetectScreenInputDescriptors();
			UpdateScrollbar();
		}
	}

	private void InitScreen()
	{
		if (panels.Count <= 0)
		{
			return;
		}
		Singleton<UIScreenManager>.Instance?.AddScreen(this);
		foreach (UIPanel panel in panels)
		{
			panel.Deselect();
		}
		SetCurrentSelectedPanel();
	}

	public void AddPanel(UIPanel panel)
	{
		if (!((Object)(object)panel == (Object)null) && !panels.Contains(panel))
		{
			panel.IsLocked = !IsSelected;
			panels.Add(panel);
			panel.SetParentScreen(this);
		}
	}

	public void RemovePanel(UIPanel panel)
	{
		panels.Remove(panel);
		if ((Object)(object)currentSelectedPanel == (Object)(object)panel)
		{
			currentSelectedPanel.Deselect();
			currentSelectedPanel.SetParentScreen(null);
			currentSelectedPanel = null;
			SetCurrentSelectedPanel();
		}
	}

	public void ClearPanels()
	{
		for (int num = panels.Count - 1; num >= 0; num--)
		{
			RemovePanel(panels[num]);
		}
	}

	public void SetCurrentSelectedPanel(UISelectable overrideSelectable = null, bool scrollToChild = true)
	{
		if (panels.Count == 0)
		{
			Debug.LogWarning((object)("No panels available to select in screen " + ((Object)((Component)this).gameObject).name));
			return;
		}
		UIPanel uIPanel = null;
		if ((Object)(object)currentSelectedPanel != (Object)null && currentSelectedPanel.Selectables.Count > 0 && currentSelectedPanel.IsPanelVisible())
		{
			uIPanel = currentSelectedPanel;
		}
		else
		{
			foreach (UIPanel panel in panels)
			{
				if (panel.IsPanelVisible() && ((Object)(object)uIPanel == (Object)null || panel.Priority > uIPanel.Priority))
				{
					uIPanel = panel;
				}
			}
		}
		SetCurrentSelectedPanel(uIPanel, overrideSelectable, scrollToChild);
	}

	public void SetCurrentSelectedPanel(UIPanel panel, UISelectable overrideSelectable = null, bool scrollToChild = true)
	{
		if (!((Object)(object)panel == (Object)null))
		{
			if (!panel.IsSelected)
			{
				currentSelectedPanel?.Deselect();
			}
			currentSelectedPanel = panel;
			currentSelectedPanel.Select(overrideSelectable, scrollToChild);
		}
	}

	private void UpdateScrollbar()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)activeScrollRect == (Object)null) && panels.Count <= 1)
		{
			float uIScrollbarAxis = GameInput.UIScrollbarAxis;
			Rect rect = activeScrollRect.content.rect;
			float height = ((Rect)(ref rect)).height;
			rect = activeScrollRect.viewport.rect;
			float num = height - ((Rect)(ref rect)).height;
			float num2 = uIScrollbarAxis * Time.unscaledDeltaTime * activeScrollRect.scrollSensitivity * 25f / num;
			ScrollRect obj = activeScrollRect;
			obj.verticalNormalizedPosition += num2;
		}
	}

	private void DetectInput()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		Vector2 uICyclePanelDirection = GameInput.UICyclePanelDirection;
		bool flag = uICyclePanelDirection != Vector2.zero;
		if (flag && !wasNavPressedLastFrame)
		{
			RectTransform rectTransform = currentSelectedPanel.RectTransform;
			Rect rect = rectTransform.rect;
			Vector2 fromPos = Vector2.op_Implicit(((Transform)rectTransform).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center)));
			Navigate(uICyclePanelDirection, fromPos);
		}
		wasNavPressedLastFrame = flag;
	}

	private void DetectScreenInputDescriptors()
	{
		foreach (InputDescriptor inputDescriptor in inputDescriptors)
		{
			inputDescriptor.DetectTriggerInput();
		}
	}

	internal bool ForceNavigate(Vector2 navDir, Vector2 fromPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		bool num = Navigate(navDir, fromPos);
		if (num)
		{
			currentSelectedPanel.LockNavigationTemporarily();
		}
		return num;
	}

	private bool Navigate(Vector2 navDir, Vector2 fromPos)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MinValue;
		UIPanel uIPanel = null;
		foreach (UIPanel panel in panels)
		{
			if ((Object)(object)panel == (Object)(object)currentSelectedPanel || !panel.IsPanelVisible() || panel.IsLocked)
			{
				continue;
			}
			RectTransform rectTransform = panel.RectTransform;
			Rect rect = rectTransform.rect;
			Vector2 val = Vector2.op_Implicit(((Transform)rectTransform).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center)));
			Vector2 val2 = val - fromPos;
			Vector2 normalized = ((Vector2)(ref val2)).normalized;
			float num2 = Vector2.Dot(((Vector2)(ref navDir)).normalized, normalized);
			if (num2 > 0f)
			{
				float num3 = Vector2.Distance(val, fromPos);
				float num4 = num2 / (num3 + 0.01f);
				if (num4 > num)
				{
					num = num4;
					uIPanel = panel;
				}
			}
		}
		if ((Object)(object)uIPanel != (Object)null)
		{
			SetCurrentSelectedPanel(uIPanel);
			return true;
		}
		return false;
	}

	public void ChangeActiveScrollRect(ScrollRect newScrollRect)
	{
		activeScrollRect = newScrollRect;
	}
}
