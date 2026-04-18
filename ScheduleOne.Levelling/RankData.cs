using System.Collections.Generic;
using ScheduleOne.Map;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Levelling;

public class RankData : SaveData
{
	public int Rank;

	public int Tier;

	public int XP;

	public int TotalXP;

	public List<EMapRegion> UnlockedRegions = new List<EMapRegion>();

	public RankData(int rank, int tier, int xp, int totalXP, List<EMapRegion> unlockedRegions)
	{
		DataVersion = 1;
		Rank = rank;
		Tier = tier;
		XP = xp;
		TotalXP = totalXP;
		UnlockedRegions = unlockedRegions;
	}
}
