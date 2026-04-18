using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Equipping;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerTasks.Tasks;
using UnityEngine;

namespace ScheduleOne.ObjectScripts.Soil;

public class Equippable_Soil : Equippable_Pourable
{
	protected override bool CanPour(GrowContainer growContainer, out string reason)
	{
		if (growContainer.IsFullyFilledWithSoil)
		{
			reason = "Already full";
			return false;
		}
		if (!growContainer.IsSoilAllowed(itemInstance.Definition as SoilDefinition))
		{
			reason = "Invalid soil type";
			return false;
		}
		if ((Object)(object)growContainer.CurrentSoil != (Object)null && ((BaseItemDefinition)growContainer.CurrentSoil).ID != ((BaseItemInstance)itemInstance).ID)
		{
			reason = "Soil type mismatch";
			return false;
		}
		return base.CanPour(growContainer, out reason);
	}

	protected override void StartPourTask(GrowContainer growContainer)
	{
		new PourSoilTask(growContainer, itemInstance, PourablePrefab);
	}
}
