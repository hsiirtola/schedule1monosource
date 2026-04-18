using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerTasks;

namespace ScheduleOne.Equipping;

public class Equippable_Additive : Equippable_Pourable
{
	private AdditiveDefinition additiveDef;

	public override void Equip(ItemInstance item)
	{
		base.Equip(item);
		additiveDef = itemInstance.Definition as AdditiveDefinition;
		base.InteractionLabel = "Apply " + ((BaseItemDefinition)additiveDef).Name;
	}

	protected override void StartPourTask(GrowContainer growContainer)
	{
		new ApplyAdditiveToPot(growContainer, itemInstance, PourablePrefab);
	}

	protected override bool CanPour(GrowContainer pot, out string reason)
	{
		if (!pot.CanApplyAdditive(additiveDef, out reason))
		{
			return false;
		}
		return base.CanPour(pot, out reason);
	}
}
