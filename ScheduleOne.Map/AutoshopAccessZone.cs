using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Map;

public class AutoshopAccessZone : NPCPresenceAccessZone
{
	public Animation RollerDoorAnim;

	public VehicleDetector VehicleDetection;

	private bool rollerDoorOpen = true;

	public override void SetIsOpen(bool open)
	{
		base.SetIsOpen(open);
		if (rollerDoorOpen != open)
		{
			rollerDoorOpen = open;
			RollerDoorAnim.Play(rollerDoorOpen ? "Roller door open" : "Roller door close");
		}
	}

	protected override void MinPass()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)TargetNPC == (Object)null))
		{
			Bounds bounds = DetectionZone.bounds;
			SetIsOpen(((Bounds)(ref bounds)).Contains(TargetNPC.Avatar.CenterPoint) || (Object)(object)VehicleDetection.closestVehicle != (Object)null);
		}
	}
}
