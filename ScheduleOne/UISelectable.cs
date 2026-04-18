using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne;

[RequireComponent(typeof(RectTransform))]
public class UISelectable : UITrigger, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[SerializeField]
	[Tooltip("When selected, the input action in the inputDescriptor list will be active")]
	private List<InputDescriptor> inputDescriptors = new List<InputDescriptor>();

	[SerializeField]
	[Tooltip("Support default A to fire the button click event even if there are inputDescriptors")]
	private bool allowTriggerSubmitWithInputDescriptors;

	[SerializeField]
	[Tooltip("A gameobject that will show when selected. Only shown when in Controller mode")]
	private GameObject selectedImage;

	[SerializeField]
	[Tooltip("Search and Add selectable to a parent Panel on Awake")]
	private bool addToPanelOnAwake;

	[SerializeField]
	[Tooltip("On Disable, tell the parent Panel to search for another valid selectable to select")]
	private bool findAnotherSelectableInPanelOnDisable;

	[SerializeField]
	[Tooltip("Set to true if you want this to be not selectable when UGUI interactable is set to false")]
	private bool blockSelectionOnInteractableFalse;

	[Header("Components")]
	[SerializeField]
	private Text _label;

	public UnityEvent OnSelected;

	public UnityEvent OnDeselected;

	public RectTransform RectTransform { get; private set; }

	public UIPanel ParentPanel { get; private set; }

	public Text Label => _label;

	public bool AllowTriggerSubmitWithInputDescriptors => allowTriggerSubmitWithInputDescriptors;

	public bool CanBeSelected
	{
		get
		{
			bool flag = true;
			Selectable val = default(Selectable);
			if (blockSelectionOnInteractableFalse && !((Component)this).TryGetComponent<Selectable>(ref val))
			{
				flag = !blockSelectionOnInteractableFalse || val.interactable;
			}
			else
			{
				Debug.LogWarning((object)(((Object)((Component)this).gameObject).name + " has blockSelectionOnInteractableFalse enabled but has no Selectable component."), (Object)(object)((Component)this).gameObject);
			}
			return ((Component)this).gameObject.activeInHierarchy && flag;
		}
	}

	internal IReadOnlyList<InputDescriptor> GetInputDescriptors()
	{
		return inputDescriptors.AsReadOnly();
	}

	protected override void Awake()
	{
		base.Awake();
		RectTransform = ((Component)this).GetComponent<RectTransform>();
		SetSelectedImageVisible(visible: false);
		if (addToPanelOnAwake)
		{
			ParentPanel = ((Component)this).GetComponentInParent<UIPanel>();
			if ((Object)(object)ParentPanel != (Object)null)
			{
				ParentPanel.AddSelectable(this);
			}
		}
		if ((Object)(object)((Component)this).GetComponent<Selectable>() == (Object)null)
		{
			Debug.LogError((object)(((Object)((Component)this).gameObject).name + " has no Selectable component. Consider adding one if you want to block selection when interactable is false."), (Object)(object)((Component)this).gameObject);
		}
	}

	protected virtual void OnDisable()
	{
		SetSelectedImageVisible(visible: false);
		if (findAnotherSelectableInPanelOnDisable && IsSelected())
		{
			ParentPanel.SelectSelectable(returnFirstFound: false);
		}
	}

	protected virtual void OnEnable()
	{
		if (IsSelected() && ParentPanel.IsSelected && GameInput.GetCurrentInputDeviceIsGamepad())
		{
			SetSelectedImageVisible(visible: true);
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (!GameInput.GetCurrentInputDeviceIsKeyboardMouse())
		{
			return;
		}
		if ((Object)(object)ParentPanel == (Object)null)
		{
			Debug.LogWarning((object)(((Object)((Component)this).gameObject).name + " has no parent panel."), (Object)(object)((Component)this).gameObject);
		}
		else if (ParentPanel.IsNavigablePanel)
		{
			if ((Object)(object)ParentPanel.ParentScreen == (Object)null)
			{
				Debug.LogWarning((object)(((Object)((Component)this).gameObject).name + " parent panel has no parent screen."), (Object)(object)((Component)this).gameObject);
			}
			else
			{
				ParentPanel.ParentScreen.SetCurrentSelectedPanel(ParentPanel, this, scrollToChild: false);
			}
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		EventSystem current = EventSystem.current;
		if ((Object)(object)((current != null) ? current.currentSelectedGameObject : null) == (Object)(object)((Component)this).gameObject && DeselectOnPointerExit())
		{
			EventSystem current2 = EventSystem.current;
			if (current2 != null)
			{
				current2.SetSelectedGameObject((GameObject)null);
			}
			UnityEvent onDeselected = OnDeselected;
			if (onDeselected != null)
			{
				onDeselected.Invoke();
			}
			SetSelectedImageVisible(visible: false);
		}
	}

	protected virtual bool DeselectOnPointerExit()
	{
		return GameInput.GetCurrentInputDeviceIsKeyboardMouse();
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerClick(eventData);
		if (GameInput.GetCurrentInputDeviceIsKeyboardMouse())
		{
			if ((Object)(object)ParentPanel == (Object)null)
			{
				Debug.LogWarning((object)(((Object)((Component)this).gameObject).name + " has no parent panel."), (Object)(object)((Component)this).gameObject);
			}
			else if (!ParentPanel.IsNavigablePanel)
			{
				ParentPanel.SelectSelectable(this);
			}
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		UnityEvent onSelected = OnSelected;
		if (onSelected != null)
		{
			onSelected.Invoke();
		}
		if (GameInput.GetCurrentInputDeviceIsGamepad())
		{
			SetSelectedImageVisible(visible: true);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		UnityEvent onDeselected = OnDeselected;
		if (onDeselected != null)
		{
			onDeselected.Invoke();
		}
		SetSelectedImageVisible(visible: false);
	}

	internal override void OnReset()
	{
		base.OnReset();
		foreach (InputDescriptor inputDescriptor in inputDescriptors)
		{
			inputDescriptor.OnReset();
		}
	}

	internal void SetParentPanel(UIPanel panel)
	{
		ParentPanel = panel;
	}

	internal bool IsSelected()
	{
		EventSystem current = EventSystem.current;
		return (Object)(object)((current != null) ? current.currentSelectedGameObject : null) == (Object)(object)((Component)this).gameObject;
	}

	private void SetSelectedImageVisible(bool visible)
	{
		if ((Object)(object)selectedImage != (Object)null)
		{
			selectedImage.SetActive(visible);
		}
	}
}
