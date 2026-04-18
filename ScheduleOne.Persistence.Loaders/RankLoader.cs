using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using ScheduleOne.Map;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class RankLoader : Loader
{
	public override void Load(string mainPath)
	{
		if (!TryLoadFile(mainPath, out var contents))
		{
			return;
		}
		RankData rankData = null;
		try
		{
			rankData = JsonUtility.FromJson<RankData>(contents);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Failed to load rank data: " + ex.Message));
		}
		if (rankData.DataVersion < 1)
		{
			Console.LogWarning("Old rank data detected, recalculating unlocked regions based on player rank.");
			List<EMapRegion> list = new List<EMapRegion>();
			FullRank fullRank = new FullRank((ERank)rankData.Rank, rankData.Tier);
			for (int i = 0; i < Singleton<ScheduleOne.Map.Map>.Instance.Regions.Length; i++)
			{
				if (fullRank >= Singleton<ScheduleOne.Map.Map>.Instance.Regions[i].RankRequirement)
				{
					list.Add(Singleton<ScheduleOne.Map.Map>.Instance.Regions[i].Region);
				}
			}
			rankData.UnlockedRegions = list;
		}
		if (rankData != null)
		{
			NetworkSingleton<LevelManager>.Instance.SetData(null, (ERank)rankData.Rank, rankData.Tier, rankData.XP, rankData.TotalXP);
			NetworkSingleton<LevelManager>.Instance.SetUnlockedRegions(null, rankData.UnlockedRegions);
		}
	}
}
