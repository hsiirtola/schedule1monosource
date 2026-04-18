using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Tools;

public class PlayerSmoothedVelocityCalculator : SmoothedVelocityCalculator
{
	public Player Player;

	public override Vector3 Velocity
	{
		get
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)Player.CurrentVehicle != (Object)null)
			{
				return ((Component)Player.CurrentVehicle).GetComponent<LandVehicle>().VelocityCalculator.Velocity;
			}
			return base.Velocity;
		}
	}
}
