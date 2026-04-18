using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class Equippable_Trimmers : Equippable_Viewmodel
{
	public bool CanClickAndDrag;

	public AudioSourceController SoundLoopPrefab;

	protected override void Update()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (Singleton<TaskManager>.Instance.currentTask != null || PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0 || !PlayerSingleton<PlayerCamera>.Instance.LookRaycast(5f, out var hit, Singleton<InteractionManager>.Instance.Interaction_SearchMask, includeTriggers: true, 0.075f))
		{
			return;
		}
		Pot componentInParent = ((Component)((RaycastHit)(ref hit)).collider).GetComponentInParent<Pot>();
		if ((Object)(object)componentInParent != (Object)null)
		{
			if (componentInParent.IsReadyForHarvest(out var reason))
			{
				componentInParent.ConfigureInteraction("Harvest", InteractableObject.EInteractableState.Default, componentInParent.Plant.HarvestLabelPositionTransform.position);
				if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact))
				{
					new HarvestPlant(componentInParent, CanClickAndDrag, SoundLoopPrefab);
				}
			}
			else
			{
				componentInParent.ConfigureInteraction(reason, InteractableObject.EInteractableState.Invalid);
			}
			return;
		}
		MushroomBed componentInParent2 = ((Component)((RaycastHit)(ref hit)).collider).GetComponentInParent<MushroomBed>();
		if (!((Object)(object)componentInParent2 != (Object)null))
		{
			return;
		}
		if (componentInParent2.IsReadyForHarvest(out var reason2))
		{
			componentInParent2.ConfigureInteraction("Harvest", InteractableObject.EInteractableState.Default, ((Component)componentInParent2.CurrentColony).transform.position);
			if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact))
			{
				new HarvestMushroomBedTask(componentInParent2, CanClickAndDrag, SoundLoopPrefab);
			}
		}
		else
		{
			componentInParent2.ConfigureInteraction(reason2, InteractableObject.EInteractableState.Invalid);
		}
	}
}
