using System;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Cartel;

public class CartelCustomerDeal : CartelActivity
{
	public const int TIMEOUT_MINUTES = 720;

	private CartelDealer dealer;

	public override bool IsRegionValidForActivity(EMapRegion region)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		dealer = NetworkSingleton<Cartel>.Instance.Activities.GetRegionalActivities(region).CartelDealer;
		if ((Object)(object)dealer == (Object)null || !dealer.IsConscious)
		{
			return false;
		}
		dealer.Health.onDieOrKnockedOut.AddListener(new UnityAction(DealerUnconscious));
		return base.IsRegionValidForActivity(region);
	}

	public override void Activate(EMapRegion region)
	{
		base.Activate(region);
		if (InstanceFinder.IsServer)
		{
			dealer = NetworkSingleton<Cartel>.Instance.Activities.GetRegionalActivities(region).CartelDealer;
			dealer.SetIsAcceptingDeals(accepting: true);
			dealer.RandomizeAppearance();
			dealer.RandomizeInventory();
			CartelDealer cartelDealer = dealer;
			cartelDealer.onContractAccepted = (Action)Delegate.Combine(cartelDealer.onContractAccepted, new Action(Deactivate));
		}
	}

	protected override void MinPassed()
	{
		base.MinPassed();
		if (base.MinsSinceActivation >= 720)
		{
			Deactivate();
		}
	}

	protected override void Deactivate()
	{
		base.Deactivate();
		if ((Object)(object)dealer != (Object)null)
		{
			dealer.SetIsAcceptingDeals(accepting: false);
			CartelDealer cartelDealer = dealer;
			cartelDealer.onContractAccepted = (Action)Delegate.Remove(cartelDealer.onContractAccepted, new Action(Deactivate));
		}
	}

	private void DealerUnconscious()
	{
	}
}
