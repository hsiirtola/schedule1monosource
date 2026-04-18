using System.Collections.Generic;
using System.Linq;
using FishNet;
using GameKit.Utilities;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using UnityEngine;

namespace ScheduleOne.Cartel;

public class StealDeadDrop : CartelActivity
{
	public const int MIN_TIME_SINCE_CONTENTS_CHANGED = 360;

	public ItemDefinition[] ItemsToLeave;

	public override bool IsRegionValidForActivity(EMapRegion region)
	{
		if ((Object)(object)GetRandomDropToStealFrom(region) == (Object)null)
		{
			return false;
		}
		return base.IsRegionValidForActivity(region);
	}

	public override void Activate(EMapRegion region)
	{
		base.Activate(region);
		if (InstanceFinder.IsServer)
		{
			DeadDrop randomDropToStealFrom = GetRandomDropToStealFrom(region);
			if ((Object)(object)randomDropToStealFrom != (Object)null)
			{
				Console.Log("Stealing from dead drop: " + randomDropToStealFrom.DeadDropName);
				randomDropToStealFrom.Storage.ClearContents();
				ItemInstance defaultInstance = ItemsToLeave[Random.Range(0, ItemsToLeave.Length)].GetDefaultInstance();
				randomDropToStealFrom.Storage.InsertItem(defaultInstance);
			}
			Deactivate();
		}
	}

	private static DeadDrop GetRandomDropToStealFrom(EMapRegion region)
	{
		new List<DeadDrop>();
		List<DeadDrop> list = DeadDrop.DeadDrops.FindAll((DeadDrop dd) => dd.Region == region);
		Arrays.Shuffle<DeadDrop>(list);
		float num = NetworkSingleton<TimeManager>.Instance.GetDateTime().GetMinSum();
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			if (list[num2].Storage.ItemCount >= 2 && !(num - (float)list[num2].Storage.LastContentChangeTime.GetMinSum() < 360f) && !(list[num2].Storage.GetAllItems().Sum((ItemInstance item) => ((BaseItemInstance)item).GetMonetaryValue()) < 200f))
			{
				return list[num2];
			}
		}
		return null;
	}
}
