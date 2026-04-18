using ScheduleOne.Interaction;
using UnityEngine;

namespace ScheduleOne.Storage;

public class StorageEntityInteractable : InteractableObject
{
	private StorageEntity StorageEntity;

	private void Awake()
	{
		StorageEntity = ((Component)this).GetComponentInParent<StorageEntity>();
		MaxInteractionRange = StorageEntity.MaxAccessDistance;
	}

	public override void Hovered()
	{
		SetInteractableState((!StorageEntity.CanBeOpened()) ? EInteractableState.Disabled : EInteractableState.Default);
		base.Hovered();
	}

	public override void StartInteract()
	{
		base.StartInteract();
		StorageEntity.Open();
	}
}
