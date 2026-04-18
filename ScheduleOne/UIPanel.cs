using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne;

[RequireComponent(typeof(RectTransform))]
public abstract class UIPanel : MonoBehaviour
{
	public enum UINavigationType
	{
		ImmediateDirection,
		NearestDirectionAndDistance
	}

	[SerializeField]
	[Tooltip("Manually assign the UIPanel attached to this screen in editor. Alternatively, you can use AddSelectable and RemoveSelectable to add/remove UISelectable.")]
	protected List<UISelectable> selectables = new List<UISelectable>();

	[SerializeField]
	[Tooltip("Default selectable to focus when the panel is selected.")]
	protected UISelectable defaultSelectable;

	[SerializeField]
	[Tooltip("ScrollRect for scrolling Layout Group.")]
	protected ScrollRect scrollRect;

	[SerializeField]
	[Tooltip("Priority value to control which panel will be selected by default by the Screen.")]
	private int priority;

	[SerializeField]
	[Tooltip("When selected, the input action in the inputDescriptor list will be active")]
	private List<InputDescriptor> inputDescriptors = new List<InputDescriptor>();

	[SerializeField]
	[Tooltip("Select this panel on Start")]
	private bool selectPanelOnStart;

	[SerializeField]
	[Tooltip("Select this panel on OnEnable")]
	private bool selectPanelOnEnable;

	[SerializeField]
	[Tooltip("Deselect this panel on OnDisable")]
	private bool deselectPanelOnDisable;

	[SerializeField]
	[Tooltip("Set to true if this panel is supporting UIOptions to prevent left/right navigation of UISelectable and UIPanel")]
	protected bool preventSideNavigation;

	[SerializeField]
	private UnityEvent OnPanelSelected;

	[SerializeField]
	private UnityEvent OnPanelDeselected;

	private UISelectable currentSelectedSelectable;

	protected int currentIndex = -1;

	protected float navTimer;

	protected bool wasNavPressedLastFrame;

	protected float scrollSpeed;

	private Coroutine scrollCoroutine;

	private bool isDisabled;

	private bool isQuitting;

	private Vector2 scrollMargin;

	protected bool lockInputThisFrame;

	public int Priority => priority;

	public RectTransform RectTransform { get; private set; }

	public bool IsSelected { get; private set; }

	public bool IsLocked { get; set; }

	public UIScreen ParentScreen { get; private set; }

	public UISelectable CurrentSelectedSelectable
	{
		get
		{
			return currentSelectedSelectable;
		}
		set
		{
			ResetCurrentSelectedSelectable();
			currentSelectedSelectable = value;
			ResetCurrentSelectedSelectable();
		}
	}

	public IReadOnlyList<UISelectable> Selectables => selectables.AsReadOnly();

	public bool IsNavigablePanel => !(this is INonNavigablePanel);

	protected virtual void Awake()
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		RectTransform = ((Component)this).GetComponent<RectTransform>();
		foreach (UISelectable selectable in selectables)
		{
			selectable.SetParentPanel(this);
		}
		foreach (InputDescriptor inputDescriptor in inputDescriptors)
		{
			if ((Object)(object)inputDescriptor == (Object)null)
			{
				Debug.LogError((object)("UIPanel: InputDescriptor in panel " + ((Object)((Component)this).gameObject).name + " is null. Please check the inspector."), (Object)(object)((Component)this).gameObject);
			}
		}
		if ((Object)(object)scrollRect != (Object)null)
		{
			LayoutGroup componentInChildren = ((Component)scrollRect).GetComponentInChildren<LayoutGroup>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				scrollMargin = new Vector2((float)componentInChildren.padding.top, (float)componentInChildren.padding.bottom);
			}
		}
	}

	protected virtual void Start()
	{
		if ((Object)(object)defaultSelectable != (Object)null)
		{
			SelectSelectable(defaultSelectable);
		}
		if (selectPanelOnStart)
		{
			Select();
		}
		if ((Object)(object)scrollRect != (Object)null)
		{
			UISelectable aValidCurrentSelectedSelectable = GetAValidCurrentSelectedSelectable();
			if ((Object)(object)aValidCurrentSelectedSelectable != (Object)null)
			{
				ScrollToChild(aValidCurrentSelectedSelectable.RectTransform, 0f);
			}
		}
		GameInput.OnInputDeviceChanged = (Action<GameInput.InputDeviceType>)Delegate.Combine(GameInput.OnInputDeviceChanged, new Action<GameInput.InputDeviceType>(HandleInputDeviceChanged));
	}

	protected virtual void OnDestroy()
	{
		ParentScreen?.RemovePanel(this);
		GameInput.OnInputDeviceChanged = (Action<GameInput.InputDeviceType>)Delegate.Remove(GameInput.OnInputDeviceChanged, new Action<GameInput.InputDeviceType>(HandleInputDeviceChanged));
	}

	protected virtual void OnEnable()
	{
		if (selectPanelOnEnable)
		{
			Select();
		}
		else if (isDisabled)
		{
			isDisabled = false;
			if (IsSelected)
			{
				Select();
			}
			else
			{
				Deselect();
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (isQuitting)
		{
			return;
		}
		isDisabled = true;
		if (deselectPanelOnDisable && IsSelected)
		{
			Deselect();
			if ((Object)(object)ParentScreen != (Object)null)
			{
				ParentScreen.SetCurrentSelectedPanel(null, null, true);
			}
		}
	}

	protected virtual void Update()
	{
		if (!IsLocked && !lockInputThisFrame && selectables.Count != 0)
		{
			EarlyUpdate();
			if (IsSelected && !GameInput.GetCurrentInputDeviceIsKeyboardMouse())
			{
				DetectInput();
				DetectScreenInputDescriptors();
				DetectSelectableInput();
			}
		}
	}

	private void LateUpdate()
	{
		if (lockInputThisFrame)
		{
			lockInputThisFrame = false;
		}
	}

	protected virtual void EarlyUpdate()
	{
	}

	protected virtual void HandleInputDeviceChanged(GameInput.InputDeviceType type)
	{
	}

	protected virtual void DetectInput()
	{
	}

	protected void DetectScreenInputDescriptors()
	{
		foreach (InputDescriptor inputDescriptor in inputDescriptors)
		{
			inputDescriptor.DetectTriggerInput();
		}
	}

	private void DetectSelectableInput()
	{
		if ((Object)(object)currentSelectedSelectable == (Object)null || !currentSelectedSelectable.IsSelected())
		{
			return;
		}
		if (currentSelectedSelectable.GetInputDescriptors().Count > 0)
		{
			foreach (InputDescriptor inputDescriptor in currentSelectedSelectable.GetInputDescriptors())
			{
				inputDescriptor.DetectTriggerInput();
			}
			if (currentSelectedSelectable.AllowTriggerSubmitWithInputDescriptors)
			{
				currentSelectedSelectable.DetectTriggerInput(Singleton<UIScreenManager>.Instance.SubmitInputAction);
			}
		}
		else
		{
			currentSelectedSelectable.DetectTriggerInput(Singleton<UIScreenManager>.Instance.SubmitInputAction);
		}
	}

	protected void SendClickEventToCurrentSelectedSelectable()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		ExecuteEvents.Execute<IPointerClickHandler>(((Component)currentSelectedSelectable).gameObject, (BaseEventData)new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
	}

	public void SetParentScreen(UIScreen screen)
	{
		ParentScreen = screen;
	}

	internal bool IsPanelVisible()
	{
		CanvasGroup component = ((Component)this).GetComponent<CanvasGroup>();
		if (IsNavigablePanel && ((Component)this).gameObject.activeInHierarchy && ((Object)(object)component == (Object)null || component.alpha > 0f) && selectables.Count > 0)
		{
			return IsAnySelectablesActive();
		}
		return false;
	}

	internal bool IsAnySelectablesActive()
	{
		if (selectables.Exists((UISelectable s) => (Object)(object)s == (Object)null))
		{
			Debug.LogError((object)("UIPanel: Panel " + ((Object)((Component)this).gameObject).name + " has null selectables in its list. Please check the inspector."), (Object)(object)((Component)this).gameObject);
		}
		return selectables.Exists((UISelectable s) => s.CanBeSelected);
	}

	public UISelectable GetAValidCurrentSelectedSelectable(bool returnFirstFound = false)
	{
		if ((Object)(object)currentSelectedSelectable == (Object)null || !currentSelectedSelectable.CanBeSelected)
		{
			return GetFallbackSelectable(returnFirstFound);
		}
		return currentSelectedSelectable;
	}

	public void SelectSelectable(UISelectable selectable, bool scrollToSelectable = false)
	{
		if (!((Object)(object)selectable == (Object)null) && selectables.Contains(selectable))
		{
			CurrentSelectedSelectable = selectable;
			currentIndex = selectables.IndexOf(selectable);
			if (scrollToSelectable && (Object)(object)scrollRect != (Object)null)
			{
				ScrollToChild(currentSelectedSelectable.RectTransform);
			}
			if (IsSelected)
			{
				UIScreenManager.LastSelectedObject = ((Component)selectable).gameObject;
			}
		}
	}

	public void SelectSelectable(int index, bool scrollToSelectable = false)
	{
		if (index < selectables.Count)
		{
			CurrentSelectedSelectable = selectables[index];
			currentIndex = index;
			if (scrollToSelectable && (Object)(object)scrollRect != (Object)null)
			{
				ScrollToChild(currentSelectedSelectable.RectTransform);
			}
			if (IsSelected)
			{
				UIScreenManager.LastSelectedObject = ((Component)currentSelectedSelectable).gameObject;
			}
		}
	}

	public void SelectSelectable(bool returnFirstFound, bool scrollToSelectable = false)
	{
		CurrentSelectedSelectable = GetFallbackSelectable(returnFirstFound);
		currentIndex = selectables.IndexOf(currentSelectedSelectable);
		if (scrollToSelectable && (Object)(object)scrollRect != (Object)null)
		{
			ScrollToChild(currentSelectedSelectable.RectTransform);
		}
		if (IsSelected)
		{
			UISelectable uISelectable = currentSelectedSelectable;
			UIScreenManager.LastSelectedObject = ((uISelectable != null) ? ((Component)uISelectable).gameObject : null);
		}
	}

	public bool AddSelectable(UISelectable selectable)
	{
		if ((Object)(object)selectable != (Object)null)
		{
			if (!selectables.Contains(selectable))
			{
				selectables.Add(selectable);
			}
			selectable.SetParentPanel(this);
			return true;
		}
		return false;
	}

	public void RemoveSelectable(UISelectable selectable, bool autoFallback = true)
	{
		if ((Object)(object)selectable != (Object)null && selectables.Contains(selectable))
		{
			selectables.Remove(selectable);
			if (selectables.Count == 0)
			{
				ParentScreen.SetCurrentSelectedPanel();
				CurrentSelectedSelectable = null;
				currentIndex = -1;
			}
			else if ((Object)(object)currentSelectedSelectable == (Object)(object)selectable && autoFallback)
			{
				SelectSelectable(GetFallbackSelectable());
			}
		}
	}

	public void DeselectSelectable()
	{
		CurrentSelectedSelectable = null;
		currentIndex = -1;
	}

	public void ClearAllSelectables()
	{
		selectables.Clear();
		CurrentSelectedSelectable = null;
		currentIndex = -1;
	}

	private UISelectable GetFallbackSelectable(bool returnFirstFound = false)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		currentIndex = Mathf.Clamp(currentIndex, 0, selectables.Count - 1);
		if (returnFirstFound)
		{
			for (int i = 0; i < selectables.Count; i++)
			{
				if ((Object)(object)selectables[i] != (Object)null && selectables[i].CanBeSelected)
				{
					return selectables[i];
				}
			}
		}
		int count = selectables.Count;
		if (count == 0)
		{
			return null;
		}
		if (Object.op_Implicit((Object)(object)currentSelectedSelectable))
		{
			UISelectable uISelectable = null;
			float num = float.MaxValue;
			Vector3 position = ((Transform)currentSelectedSelectable.RectTransform).position;
			for (int j = 0; j < count; j++)
			{
				UISelectable uISelectable2 = selectables[j];
				if ((Object)(object)uISelectable2 != (Object)null && uISelectable2.CanBeSelected)
				{
					float num2 = Vector3.Distance(position, ((Transform)uISelectable2.RectTransform).position);
					if (num2 < num)
					{
						num = num2;
						uISelectable = uISelectable2;
					}
				}
			}
			if ((Object)(object)uISelectable != (Object)null)
			{
				return uISelectable;
			}
		}
		if (currentIndex == 0)
		{
			for (int k = 0; k < count; k++)
			{
				if ((Object)(object)selectables[k] != (Object)null && selectables[k].CanBeSelected)
				{
					return selectables[k];
				}
			}
		}
		else
		{
			for (int l = 0; l < count; l++)
			{
				int index = (currentIndex - l + count) % count;
				if ((Object)(object)selectables[index] != (Object)null && selectables[index].CanBeSelected)
				{
					return selectables[index];
				}
			}
		}
		return null;
	}

	internal UISelectable Select(UISelectable overrideSelectable = null, bool scrollToChild = true)
	{
		lockInputThisFrame = true;
		if (IsSelected)
		{
			if ((Object)(object)overrideSelectable != (Object)null)
			{
				SelectSelectable(overrideSelectable);
			}
			SelectSelectable(currentSelectedSelectable, scrollToChild);
			return currentSelectedSelectable;
		}
		IsSelected = true;
		UISelectable uISelectable = (((Object)(object)overrideSelectable != (Object)null) ? overrideSelectable : GetAValidCurrentSelectedSelectable());
		SelectSelectable(uISelectable);
		if (scrollToChild && (Object)(object)scrollRect != (Object)null)
		{
			ScrollToChild(currentSelectedSelectable?.RectTransform, 0f);
		}
		UnityEvent onPanelSelected = OnPanelSelected;
		if (onPanelSelected != null)
		{
			onPanelSelected.Invoke();
		}
		Debug.Log((object)$"UIPanel: Selecting panel {((Object)((Component)this).gameObject).name}. Selected Index {currentIndex}");
		return uISelectable;
	}

	internal void Deselect()
	{
		IsSelected = false;
		lockInputThisFrame = false;
		OnReset();
		UnityEvent onPanelDeselected = OnPanelDeselected;
		if (onPanelDeselected != null)
		{
			onPanelDeselected.Invoke();
		}
	}

	internal void OnReset()
	{
		foreach (InputDescriptor inputDescriptor in inputDescriptors)
		{
			inputDescriptor.OnReset();
		}
		foreach (UISelectable selectable in selectables)
		{
			if (!((Object)(object)selectable == (Object)null))
			{
				selectable.OnReset();
			}
		}
	}

	private void ResetCurrentSelectedSelectable()
	{
		if ((Object)(object)currentSelectedSelectable != (Object)null)
		{
			currentSelectedSelectable.OnReset();
		}
	}

	public void ScrollToCurrentSelectedSelectable()
	{
		if (!((Object)(object)scrollRect == (Object)null) && !((Object)(object)currentSelectedSelectable == (Object)null))
		{
			ScrollToChild(currentSelectedSelectable.RectTransform);
		}
	}

	protected void ScrollToChild(RectTransform child, float duration = 0.25f)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)child == (Object)null)
		{
			return;
		}
		Canvas.ForceUpdateCanvases();
		Vector2 val = scrollMargin;
		Rect rect = scrollRect.viewport.rect;
		Vector2 min = ((Rect)(ref rect)).min;
		rect = scrollRect.viewport.rect;
		Vector2 max = ((Rect)(ref rect)).max;
		RectTransform viewport = scrollRect.viewport;
		rect = child.rect;
		Vector2 val2 = Vector2.op_Implicit(((Transform)viewport).InverseTransformPoint(((Transform)child).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).min))));
		RectTransform viewport2 = scrollRect.viewport;
		rect = child.rect;
		Vector2 val3 = Vector2.op_Implicit(((Transform)viewport2).InverseTransformPoint(((Transform)child).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).max))));
		val2 -= val;
		val3 += val;
		Vector2 zero = Vector2.zero;
		if (scrollRect.vertical && val3.y > max.y)
		{
			zero.y = val3.y - max.y;
		}
		if (scrollRect.horizontal && val2.x < min.x)
		{
			zero.x = val2.x - min.x;
		}
		if (scrollRect.horizontal && val3.x > max.x)
		{
			zero.x = val3.x - max.x;
		}
		if (scrollRect.vertical && val2.y < min.y)
		{
			zero.y = val2.y - min.y;
		}
		Vector3 val4 = ((Transform)scrollRect.viewport).TransformDirection(Vector2.op_Implicit(zero));
		if (duration <= 0f)
		{
			RectTransform content = scrollRect.content;
			((Transform)content).localPosition = ((Transform)content).localPosition - ((Transform)scrollRect.content).InverseTransformDirection(val4);
			return;
		}
		Vector3 targetLocalPosition = ((Transform)scrollRect.content).localPosition - ((Transform)scrollRect.content).InverseTransformDirection(val4);
		if (scrollCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(scrollCoroutine);
		}
		scrollCoroutine = ((MonoBehaviour)this).StartCoroutine(SmoothScrollContent(targetLocalPosition, duration));
	}

	private IEnumerator SmoothScrollContent(Vector3 targetLocalPosition, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		RectTransform content = scrollRect.content;
		Vector3 startPos = ((Transform)content).localPosition;
		float time = 0f;
		while (time < duration)
		{
			time += Time.unscaledDeltaTime;
			float num = Mathf.Clamp01(time / duration);
			((Transform)content).localPosition = Vector3.Lerp(startPos, targetLocalPosition, num);
			yield return null;
		}
		((Transform)content).localPosition = targetLocalPosition;
		scrollCoroutine = null;
	}

	public void EnableSideNavigation(bool enabled)
	{
		preventSideNavigation = !enabled;
	}

	protected virtual bool Navigate(Vector2 navDir)
	{
		return false;
	}

	private void ResetNavigationData()
	{
		navTimer = 0f;
		wasNavPressedLastFrame = false;
		if (scrollCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(scrollCoroutine);
			scrollCoroutine = null;
		}
	}

	internal void LockNavigationTemporarily()
	{
		navTimer = 0.5f;
		wasNavPressedLastFrame = true;
	}

	protected virtual bool NavigateUsingCyclePanel(Vector2 dir)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (!IsNavigablePanel || preventSideNavigation)
		{
			return false;
		}
		bool flag = false;
		if (Object.op_Implicit((Object)(object)ParentScreen))
		{
			RectTransform rectTransform = currentSelectedSelectable.RectTransform;
			Rect rect = rectTransform.rect;
			Vector2 fromPos = Vector2.op_Implicit(((Transform)rectTransform).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center)));
			flag = ParentScreen.ForceNavigate(dir, fromPos);
		}
		if (flag)
		{
			ResetNavigationData();
		}
		return flag;
	}
}
