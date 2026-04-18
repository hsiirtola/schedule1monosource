using UnityEngine;

namespace ScheduleOne.Building;

public class BuildStop_AirConditioner : BuildStop_Base
{
	public override void Stop_Building()
	{
		((Component)this).GetComponent<BuildUpdate_AirConditioner>().RemoveFromPropery();
		base.Stop_Building();
	}
}
