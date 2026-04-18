using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.Interaction;
using ScheduleOne.Management;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.PlayerTasks.Tasks;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class Equippable_Pourable : Equippable_Viewmodel
{
	private const float InteractionRange = 2.5f;

	[SerializeField]
	public Pourable PourablePrefab;

	[field: SerializeField]
	public string InteractionLabel { get; set; } = "Pour";

	protected virtual void Awake()
	{
	}

	protected override void Update()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (Singleton<TaskManager>.Instance.currentTask != null || PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0 || !PlayerSingleton<PlayerCamera>.Instance.LookRaycast(2.5f, out var hit, Singleton<InteractionManager>.Instance.Interaction_SearchMask, includeTriggers: true, 0.075f))
		{
			return;
		}
		GrowContainer componentInParent = ((Component)((RaycastHit)(ref hit)).collider).GetComponentInParent<GrowContainer>();
		if ((Object)(object)componentInParent == (Object)null)
		{
			return;
		}
		string reason = string.Empty;
		if (CanPour(componentInParent, out reason))
		{
			if (((IUsable)componentInParent).IsInUse)
			{
				componentInParent.ConfigureInteraction("In use by " + ((IUsable)componentInParent).UserName, InteractableObject.EInteractableState.Invalid);
				return;
			}
			componentInParent.ConfigureInteraction(InteractionLabel, InteractableObject.EInteractableState.Default);
			if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact))
			{
				StartPourTask(componentInParent);
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

	protected virtual void StartPourTask(GrowContainer growContainer)
	{
		new GrowContainerPourTask(growContainer, itemInstance, PourablePrefab);
	}

	protected virtual bool CanPour(GrowContainer growContainer, out string reason)
	{
		reason = string.Empty;
		return true;
	}
}
