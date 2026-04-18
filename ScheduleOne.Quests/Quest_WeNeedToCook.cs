using FishNet;
using ScheduleOne.Economy;

namespace ScheduleOne.Quests;

public class Quest_WeNeedToCook : Quest
{
	public Quest[] PrerequisiteQuests;

	public Supplier MethSupplier;

	protected override void OnUncappedMinPass()
	{
		base.OnUncappedMinPass();
		if (!InstanceFinder.IsServer || base.State != EQuestState.Inactive || !MethSupplier.RelationData.Unlocked)
		{
			return;
		}
		Quest[] prerequisiteQuests = PrerequisiteQuests;
		for (int i = 0; i < prerequisiteQuests.Length; i++)
		{
			if (prerequisiteQuests[i].State != EQuestState.Completed)
			{
				return;
			}
		}
		Begin();
	}
}
