using ScheduleOne.Interaction;
using UnityEngine;

namespace ScheduleOne.Growing;

public class GrowContainerInteraction : MonoBehaviour
{
	[SerializeField]
	private InteractableObject _interactableObject;

	private bool _interactableActivatedThisFrame;

	private Vector3 displayLocationPointDefaultLocalPosition = Vector3.zero;

	protected virtual void Awake()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		displayLocationPointDefaultLocalPosition = ((Component)_interactableObject.displayLocationPoint).transform.localPosition;
	}

	private void LateUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!_interactableActivatedThisFrame)
		{
			if (TryGetFallbackInteractionMessage(out var message, out var state))
			{
				ConfigureInteraction(message, state);
			}
			else
			{
				_interactableObject.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
		}
		_interactableActivatedThisFrame = false;
	}

	public void ConfigureInteraction(string labelText, InteractableObject.EInteractableState interactionState, bool setLabelPosition = false, Vector3 labelPosition = default(Vector3))
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		_interactableActivatedThisFrame = true;
		_interactableObject.SetMessage(labelText);
		_interactableObject.SetInteractableState(interactionState);
		if (setLabelPosition)
		{
			((Component)_interactableObject.displayLocationPoint).transform.position = labelPosition;
		}
		else
		{
			((Component)_interactableObject.displayLocationPoint).transform.localPosition = displayLocationPointDefaultLocalPosition;
		}
	}

	protected virtual bool TryGetFallbackInteractionMessage(out string message, out InteractableObject.EInteractableState state)
	{
		message = string.Empty;
		state = InteractableObject.EInteractableState.Disabled;
		return false;
	}
}
