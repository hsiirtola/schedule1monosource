using ScheduleOne.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.TV;

public class TVInteractable : MonoBehaviour
{
	public InteractableObject IntObj;

	public TVInterface Interface;

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		IntObj.onHovered.AddListener(new UnityAction(Hovered));
		IntObj.onInteractStart.AddListener(new UnityAction(Interacted));
	}

	private void Hovered()
	{
		if (Interface.CanOpen())
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			IntObj.SetMessage("Use TV");
		}
		else
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	private void Interacted()
	{
		if (Interface.CanOpen())
		{
			Interface.Open();
		}
	}
}
