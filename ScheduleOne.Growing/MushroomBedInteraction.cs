using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Temperature;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Growing;

public class MushroomBedInteraction : GrowContainerInteraction
{
	[SerializeField]
	private MushroomBed _bed;

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
		if ((Object)(object)_bed.CurrentColony != (Object)null)
		{
			if (_bed.CurrentColony.IsFullyGrown)
			{
				message = "Use trimmers to harvest";
				return true;
			}
			if (_bed.CurrentColony.IsTooHotToGrow)
			{
				message = "Temperature must be below " + TemperatureUtility.FormatTemperatureWithAppropriateUnit(15f);
				state = InteractableObject.EInteractableState.Invalid;
				return true;
			}
			message = Mathf.RoundToInt(_bed.CurrentColony.GrowthProgress * 100f) + "% grown";
			return true;
		}
		return base.TryGetFallbackInteractionMessage(out message, out state);
	}
}
