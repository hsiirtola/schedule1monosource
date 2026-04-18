using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.Interaction;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class Equippable_Seed : Equippable_Viewmodel
{
	public SeedDefinition Seed;

	protected override void Update()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (Singleton<TaskManager>.Instance.currentTask != null || PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0 || !PlayerSingleton<PlayerCamera>.Instance.LookRaycast(5f, out var hit, Singleton<InteractionManager>.Instance.Interaction_SearchMask, includeTriggers: true, 0.075f))
		{
			return;
		}
		Pot componentInParent = ((Component)((RaycastHit)(ref hit)).collider).GetComponentInParent<Pot>();
		if (!((Object)(object)componentInParent != (Object)null))
		{
			return;
		}
		if (componentInParent.CanAcceptSeed(out var reason))
		{
			if ((Object)(object)componentInParent.PlayerUserObject != (Object)null)
			{
				componentInParent.ConfigureInteraction("In use by other player", InteractableObject.EInteractableState.Invalid);
				return;
			}
			if ((Object)(object)componentInParent.NPCUserObject != (Object)null)
			{
				componentInParent.ConfigureInteraction("In use by workers", InteractableObject.EInteractableState.Invalid);
				return;
			}
			componentInParent.ConfigureInteraction("Sow seed", InteractableObject.EInteractableState.Default);
			if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact))
			{
				StartSowSeedTask(componentInParent);
			}
		}
		else
		{
			componentInParent.ConfigureInteraction(reason, InteractableObject.EInteractableState.Invalid);
		}
	}

	protected virtual void StartSowSeedTask(Pot pot)
	{
		new SowSeedTask(pot, Seed);
	}
}
