using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.PlayerTasks.Tasks;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class Equippable_SprayBottle : Equippable_Viewmodel
{
	private const float InteractionRange = 2.5f;

	[SerializeField]
	private GameObject _sprayablePrefab;

	private WaterContainerInstance _waterContainerInstance;

	[SerializeField]
	private string InteractionLabel { get; set; } = "Spray";

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
		if (CanSpray(componentInParent, out reason))
		{
			if (((IUsable)componentInParent).IsInUse)
			{
				componentInParent.ConfigureInteraction("In use by " + ((IUsable)componentInParent).UserName, InteractableObject.EInteractableState.Invalid);
				return;
			}
			componentInParent.ConfigureInteraction(InteractionLabel, InteractableObject.EInteractableState.Default);
			if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact))
			{
				StartSprayTask(componentInParent);
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

	protected virtual bool CanSpray(GrowContainer growContainer, out string reason)
	{
		reason = string.Empty;
		if (!growContainer.IsFullyFilledWithSoil)
		{
			reason = "Must be filled with soil";
			return false;
		}
		if (growContainer.NormalizedMoistureAmount >= 0.975f)
		{
			return false;
		}
		if ((itemInstance as WaterContainerInstance).CurrentFillAmount <= 0f)
		{
			reason = "Spray bottle empty";
			return false;
		}
		return true;
	}

	protected void StartSprayTask(MushroomBed growContainer)
	{
		new MistMushroomBedTask(growContainer, itemInstance, _sprayablePrefab);
	}
}
