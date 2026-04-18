using System.Linq;
using FishNet;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using UnityEngine;

namespace ScheduleOne.Cartel;

public class RobDealer : CartelActivity
{
	public override bool IsRegionValidForActivity(EMapRegion region)
	{
		if ((Object)(object)GetDealerToRob(region) == (Object)null)
		{
			return false;
		}
		return base.IsRegionValidForActivity(region);
	}

	private Dealer GetDealerToRob(EMapRegion region)
	{
		Dealer dealer = Dealer.AllPlayerDealers.FirstOrDefault((Dealer x) => x.Region == region);
		if ((Object)(object)dealer == (Object)null)
		{
			return null;
		}
		if (!dealer.IsRecruited)
		{
			return null;
		}
		if (!dealer.IsConscious)
		{
			return null;
		}
		if (dealer.Cash < 100f || ((IItemSlotOwner)dealer.Inventory).GetQuantitySum() < 4)
		{
			return null;
		}
		if (dealer.AssignedCustomers.Count == 0)
		{
			return null;
		}
		return dealer;
	}

	public override void Activate(EMapRegion region)
	{
		base.Activate(region);
		if (InstanceFinder.IsServer)
		{
			Dealer dealerToRob = GetDealerToRob(region);
			if ((Object)(object)dealerToRob != (Object)null)
			{
				dealerToRob.TryRobDealer();
			}
			Deactivate();
		}
	}
}
