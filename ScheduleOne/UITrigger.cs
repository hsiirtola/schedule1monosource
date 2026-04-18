using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ScheduleOne;

[RequireComponent(typeof(Selectable))]
public class UITrigger : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerClickHandler, IPointerExitHandler
{
	public enum TriggerType
	{
		Press,
		Hold
	}

	[SerializeField]
	private TriggerType triggerType;

	[SerializeField]
	[Tooltip("Set to true if you want Mouse to be always Press")]
	private bool mouseAlwaysPress;

	[SerializeField]
	[Tooltip("Duration in seconds to hold for Hold trigger")]
	private float holdDuration = 1f;

	[SerializeField]
	[Tooltip("Optional UI image to show hold progress (should be Image Type: Filled)")]
	private Image holdImage;

	[SerializeField]
	[Tooltip("Optional UGUI Selectable. If assigned, the uiTrigger interactable will also check for the UGUI Selectable interactable property.")]
	private Selectable uGUISelectable;

	[Tooltip("Event triggered when the action is performed")]
	public UnityEvent OnTrigger;

	private bool isHolding;

	private float holdTime;

	private bool isHoldStarted;

	private bool interactable = true;

	public bool Interactable
	{
		get
		{
			return interactable;
		}
		set
		{
			interactable = value;
			((Component)this).GetComponent<Selectable>().interactable = value;
		}
	}

	public Image HoldImage
	{
		get
		{
			return holdImage;
		}
		set
		{
			holdImage = value;
		}
	}

	internal TriggerType GetTriggerType()
	{
		return triggerType;
	}

	protected virtual void Awake()
	{
		if (Object.op_Implicit((Object)(object)holdImage))
		{
			holdImage.fillAmount = 0f;
		}
	}

	private bool IsInteractable()
	{
		if (!interactable)
		{
			return false;
		}
		if (!((Component)this).gameObject.activeInHierarchy)
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)uGUISelectable) && !uGUISelectable.interactable)
		{
			return false;
		}
		return true;
	}

	private void Update()
	{
		if (triggerType != TriggerType.Hold || !isHoldStarted || !IsInteractable() || (mouseAlwaysPress && GameInput.GetCurrentInputDeviceIsKeyboardMouse()) || !isHolding)
		{
			return;
		}
		holdTime += Time.deltaTime;
		if (holdTime >= holdDuration)
		{
			isHolding = false;
			isHoldStarted = false;
			holdTime = 0f;
			UnityEvent onTrigger = OnTrigger;
			if (onTrigger != null)
			{
				onTrigger.Invoke();
			}
		}
		UpdateHoldImage(holdTime / holdDuration);
	}

	internal virtual void OnReset()
	{
		isHolding = false;
		isHoldStarted = false;
		holdTime = 0f;
		UpdateHoldImage(0f);
	}

	internal virtual void DetectTriggerInput(InputActionReference inputAction)
	{
		if (!GameInput.GetCurrentInputDeviceIsGamepad())
		{
			return;
		}
		if (triggerType == TriggerType.Press)
		{
			if (inputAction.action.WasPressedThisFrame())
			{
				OnInputDown();
			}
		}
		else
		{
			if (triggerType != TriggerType.Hold)
			{
				return;
			}
			if (inputAction.action.WasPressedThisFrame())
			{
				isHoldStarted = true;
			}
			else if (inputAction.action.ReadValue<float>() > 0.5f)
			{
				if (isHoldStarted)
				{
					OnInputDown();
				}
			}
			else
			{
				OnInputUp();
			}
		}
	}

	internal void OnInputDown()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		if (!IsInteractable())
		{
			return;
		}
		if (triggerType == TriggerType.Press)
		{
			ExecuteEvents.Execute<IPointerClickHandler>(((Component)this).gameObject, (BaseEventData)new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
			UnityEvent onTrigger = OnTrigger;
			if (onTrigger != null)
			{
				onTrigger.Invoke();
			}
		}
		else
		{
			HandleHoldStart();
		}
	}

	internal void OnInputUp()
	{
		HandleHoldEnd();
	}

	public virtual void OnPointerDown(PointerEventData eventData)
	{
		HandleHoldStart();
	}

	public virtual void OnPointerUp(PointerEventData eventData)
	{
		HandleHoldEnd();
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		HandleHoldEnd();
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (!IsInteractable() || !GameInput.GetCurrentInputDeviceIsKeyboardMouse())
		{
			return;
		}
		if (triggerType == TriggerType.Press)
		{
			UnityEvent onTrigger = OnTrigger;
			if (onTrigger != null)
			{
				onTrigger.Invoke();
			}
		}
		else if (mouseAlwaysPress)
		{
			UnityEvent onTrigger2 = OnTrigger;
			if (onTrigger2 != null)
			{
				onTrigger2.Invoke();
			}
		}
	}

	private void HandleHoldStart()
	{
		if (triggerType == TriggerType.Hold && !isHolding)
		{
			isHolding = true;
			holdTime = 0f;
			UpdateHoldImage(0f);
			if (GameInput.GetCurrentInputDeviceIsKeyboardMouse())
			{
				isHoldStarted = true;
			}
		}
	}

	private void HandleHoldEnd()
	{
		if (triggerType == TriggerType.Hold)
		{
			isHolding = false;
			isHoldStarted = false;
			holdTime = 0f;
			UpdateHoldImage(0f);
		}
	}

	private void UpdateHoldImage(float amount)
	{
		if (Object.op_Implicit((Object)(object)holdImage))
		{
			holdImage.fillAmount = amount;
		}
	}
}
