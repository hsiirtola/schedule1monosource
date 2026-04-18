using ScheduleOne.Building;
using ScheduleOne.DevUtilities;

namespace ScheduleOne.Equipping;

public class Equippable_BuildableItem : Equippable
{
	protected bool isBuilding;

	protected override void Update()
	{
		base.Update();
		if (!isBuilding)
		{
			isBuilding = true;
			NetworkSingleton<BuildManager>.Instance.StartBuilding(itemInstance);
		}
	}

	public override void Unequip()
	{
		if (isBuilding)
		{
			NetworkSingleton<BuildManager>.Instance.StopBuilding();
		}
		base.Unequip();
	}
}
