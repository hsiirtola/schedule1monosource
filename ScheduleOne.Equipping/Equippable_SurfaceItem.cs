using ScheduleOne.Building;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class Equippable_SurfaceItem : Equippable
{
	protected bool isBuilding;

	protected override void Update()
	{
		base.Update();
		if (!isBuilding)
		{
			isBuilding = true;
			NetworkSingleton<BuildManager>.Instance.StartBuilding(itemInstance);
			_ = (Object)(object)NetworkSingleton<BuildManager>.Instance.currentBuildHandler.GetComponent<BuildUpdate_Surface>() != (Object)null;
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
