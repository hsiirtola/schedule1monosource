using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Growing;

public class PotInteraction : GrowContainerInteraction
{
	[SerializeField]
	private Pot _pot;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override bool TryGetFallbackInteractionMessage(out string message, out InteractableObject.EInteractableState state)
	{
		state = InteractableObject.EInteractableState.Label;
		if (Singleton<ManagementClipboard>.InstanceExists && Singleton<ManagementClipboard>.Instance.IsEquipped)
		{
			message = string.Empty;
			return false;
		}
		if ((Object)(object)_pot.Plant != (Object)null)
		{
			if (_pot.Plant.IsFullyGrown)
			{
				message = "Use trimmers to harvest";
				return true;
			}
			message = Mathf.RoundToInt(_pot.Plant.NormalizedGrowthProgress * 100f) + "% grown";
			return true;
		}
		return base.TryGetFallbackInteractionMessage(out message, out state);
	}
}
