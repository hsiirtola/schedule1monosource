using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class MushroomSpawnEquipped : Equippable_Viewmodel
{
	private const float InteractionRange = 2.5f;

	[SerializeField]
	private GameObject _taskPrefab;

	[SerializeField]
	private string InteractionLabel { get; set; } = "Add mushroom spawn";

	protected override void Update()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (Singleton<TaskManager>.Instance.currentTask != null || PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0 || !PlayerSingleton<PlayerCamera>.Instance.LookRaycast(2.5f, out var hit, Singleton<InteractionManager>.Instance.Interaction_SearchMask, includeTriggers: true, 0.075f))
		{
			return;
		}
		MushroomBed componentInParent = ((Component)((RaycastHit)(ref hit)).collider).GetComponentInParent<MushroomBed>();
		if ((Object)(object)componentInParent == (Object)null)
		{
			return;
		}
		string reason = string.Empty;
		if (CanApplyToMushroomBed(componentInParent, out reason))
		{
			componentInParent.ConfigureInteraction(InteractionLabel, InteractableObject.EInteractableState.Default);
			if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact))
			{
				StartTask(componentInParent);
			}
		}
		else if (reason != string.Empty)
		{
			componentInParent.ConfigureInteraction(reason, InteractableObject.EInteractableState.Invalid);
		}
		else
		{
			componentInParent.ConfigureInteraction(string.Empty, InteractableObject.EInteractableState.Disabled);
		}
	}

	protected virtual bool CanApplyToMushroomBed(MushroomBed bed, out string reason)
	{
		reason = string.Empty;
		if (!bed.IsFullyFilledWithSoil)
		{
			reason = "Must be filled with substrate";
			return false;
		}
		if ((Object)(object)bed.CurrentColony != (Object)null)
		{
			reason = "Already contains shrooms";
			return false;
		}
		if (((IUsable)bed).IsInUse)
		{
			reason = "In use by " + ((IUsable)bed).UserName;
			return false;
		}
		return true;
	}

	protected void StartTask(MushroomBed growContainer)
	{
		new ApplyShroomSpawnTask(growContainer, itemInstance.Definition as ShroomSpawnDefinition);
	}
}
