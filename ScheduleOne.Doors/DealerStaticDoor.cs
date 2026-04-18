using ScheduleOne.Economy;
using UnityEngine;

namespace ScheduleOne.Doors;

public class DealerStaticDoor : StaticDoor
{
	public Dealer Dealer;

	protected override bool IsKnockValid(out string message)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (Building.OccupantCount == 0 && Vector3.Distance(((Component)this).transform.position, ((Component)Dealer).transform.position) > 2f)
		{
			message = Dealer.FirstName + " is out dealing";
			return false;
		}
		return base.IsKnockValid(out message);
	}
}
