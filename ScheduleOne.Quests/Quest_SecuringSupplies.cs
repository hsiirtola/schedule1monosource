using FishNet;
using ScheduleOne.Economy;

namespace ScheduleOne.Quests;

public class Quest_SecuringSupplies : Quest
{
	public Supplier Supplier;

	protected override void OnUncappedMinPass()
	{
		base.OnUncappedMinPass();
		if (InstanceFinder.IsServer)
		{
			_ = base.State;
		}
	}
}
