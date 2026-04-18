using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks.Tasks;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class PourableWaterContainerEquipped : Equippable_Pourable
{
	[SerializeField]
	private WaterContainerVisualizer _visuals;

	[SerializeField]
	private WaterContainerPourable _pourablePrefab;

	private WaterContainerInstance _waterContainerInstance;

	public override void Equip(ItemInstance item)
	{
		base.Equip(item);
		_waterContainerInstance = item as WaterContainerInstance;
		if ((Object)(object)_visuals != (Object)null)
		{
			_visuals.AssignWaterContainer(_waterContainerInstance);
		}
	}

	public override void Unequip()
	{
		if ((Object)(object)_visuals != (Object)null)
		{
			_visuals.UnassignWaterContainer();
		}
		base.Unequip();
	}

	protected override bool CanPour(GrowContainer growContainer, out string reason)
	{
		if (growContainer is MushroomBed)
		{
			reason = "Use spray bottle for mushroom bed";
			return false;
		}
		if (!growContainer.IsFullyFilledWithSoil)
		{
			reason = "Must be filled with soil";
			return false;
		}
		if (growContainer.NormalizedMoistureAmount >= 0.975f)
		{
			reason = string.Empty;
			return false;
		}
		if ((itemInstance as WaterContainerInstance).CurrentFillAmount <= 0f)
		{
			reason = "Watering can empty";
			return false;
		}
		return base.CanPour(growContainer, out reason);
	}

	protected override void StartPourTask(GrowContainer growContainer)
	{
		new PourWaterTask(growContainer, itemInstance, _pourablePrefab);
	}
}
