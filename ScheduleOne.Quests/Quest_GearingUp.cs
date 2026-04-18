using ScheduleOne.Economy;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Quests;

public class Quest_GearingUp : Quest
{
	public QuestEntry WaitForDropEntry;

	public QuestEntry CollectDropEntry;

	public Supplier Supplier;

	private bool setCollectionPosition;

	protected override void Start()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.Start();
		Supplier.onDeaddropReady.AddListener(new UnityAction(DropReady));
	}

	protected override void OnUncappedMinPass()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		base.OnUncappedMinPass();
		if (CollectDropEntry.State == EQuestState.Active && !setCollectionPosition)
		{
			DeadDrop deadDrop = DeadDrop.DeadDrops.Find((DeadDrop x) => x.Storage.ItemCount > 0);
			if ((Object)(object)deadDrop != (Object)null)
			{
				setCollectionPosition = true;
				CollectDropEntry.SetPoILocation(((Component)deadDrop).transform.position);
			}
		}
		if (WaitForDropEntry.State == EQuestState.Active)
		{
			float num = Supplier.minsUntilDeaddropReady;
			if (num > 0f)
			{
				WaitForDropEntry.SetEntryTitle("Wait for the dead drop (" + num + " mins)");
			}
			else
			{
				WaitForDropEntry.SetEntryTitle("Wait for the dead drop");
			}
		}
	}

	private void DropReady()
	{
		if (WaitForDropEntry.State == EQuestState.Active)
		{
			WaitForDropEntry.Complete();
			OnMinPass();
		}
	}
}
