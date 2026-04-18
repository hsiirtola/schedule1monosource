using System;
using FishNet;
using ScheduleOne.Economy;
using ScheduleOne.NPCs.Relation;

namespace ScheduleOne.Quests;

public class Quest_GrowShrooms : Quest
{
	public Supplier ShroomSupplier;

	protected override void Start()
	{
		base.Start();
		NPCRelationData relationData = ShroomSupplier.RelationData;
		relationData.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(SupplierUnlocked));
	}

	private void SupplierUnlocked(NPCRelationData.EUnlockType unlockType, bool notify)
	{
		if (InstanceFinder.IsServer && base.State == EQuestState.Inactive)
		{
			Begin();
		}
	}
}
