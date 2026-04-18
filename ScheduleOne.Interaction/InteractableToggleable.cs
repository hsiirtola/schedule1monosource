using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Interaction;

public class InteractableToggleable : MonoBehaviour
{
	public string ActivateMessage = "Activate";

	public string DeactivateMessage = "Deactivate";

	public float CoolDown;

	[Header("References")]
	public InteractableObject IntObj;

	public UnityEvent onToggle = new UnityEvent();

	public UnityEvent onActivate = new UnityEvent();

	public UnityEvent onDeactivate = new UnityEvent();

	private float lastActivated;

	public bool IsActivated { get; private set; }

	public void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		IntObj.onHovered.AddListener(new UnityAction(Hovered));
		IntObj.onInteractStart.AddListener(new UnityAction(Interacted));
	}

	public void Hovered()
	{
		if (Time.time - lastActivated < CoolDown)
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			return;
		}
		IntObj.SetMessage(IsActivated ? DeactivateMessage : ActivateMessage);
		IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
	}

	public void Interacted()
	{
		Toggle();
	}

	public void Toggle()
	{
		lastActivated = Time.time;
		IsActivated = !IsActivated;
		if (onToggle != null)
		{
			onToggle.Invoke();
		}
		if (IsActivated)
		{
			onActivate.Invoke();
		}
		else
		{
			onDeactivate.Invoke();
		}
	}

	public void SetState(bool activated)
	{
		if (IsActivated != activated)
		{
			lastActivated = Time.time;
			IsActivated = !IsActivated;
			if (IsActivated)
			{
				onActivate.Invoke();
			}
			else
			{
				onDeactivate.Invoke();
			}
		}
	}

	public void PoliceDetected()
	{
		if (!IsActivated)
		{
			Toggle();
		}
	}
}
