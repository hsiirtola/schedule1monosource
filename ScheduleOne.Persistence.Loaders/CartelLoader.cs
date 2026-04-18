using System;
using System.Collections.Generic;
using ScheduleOne.Cartel;
using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using ScheduleOne.Map;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class CartelLoader : Loader
{
	public override void Load(string mainPath)
	{
		Console.Log("Loading cartel");
		if (TryLoadFile(mainPath, out var contents))
		{
			CartelData cartelData = null;
			try
			{
				cartelData = JsonUtility.FromJson<CartelData>(contents);
			}
			catch (Exception ex)
			{
				Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
			}
			if (cartelData != null)
			{
				NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Load(cartelData);
			}
			return;
		}
		Console.Log("No cartel data found, constructing cartel data using existing region data.");
		CartelInfluence.RegionInfluenceData[] array = new CartelInfluence.RegionInfluenceData[Enum.GetValues(typeof(EMapRegion)).Length];
		Dictionary<EMapRegion, int> dictionary = new Dictionary<EMapRegion, int>();
		EMapRegion eMapRegion = EMapRegion.Northtown;
		EMapRegion[] array2 = (EMapRegion[])Enum.GetValues(typeof(EMapRegion));
		foreach (EMapRegion eMapRegion2 in array2)
		{
			dictionary.Add(eMapRegion2, 9999);
			MapRegionData regionData = Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(eMapRegion2);
			float influence = 1f;
			if (regionData.IsUnlocked)
			{
				eMapRegion = eMapRegion2;
				influence = 0.3f;
			}
			array[(int)eMapRegion2] = new CartelInfluence.RegionInfluenceData(eMapRegion2, influence);
		}
		if (eMapRegion != EMapRegion.Uptown)
		{
			FullRank rankRequirement = Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(eMapRegion).RankRequirement;
			EMapRegion region = eMapRegion + 1;
			FullRank rankRequirement2 = Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(region).RankRequirement;
			float num = rankRequirement.ToFloat();
			float num2 = rankRequirement2.ToFloat();
			float num3 = NetworkSingleton<LevelManager>.Instance.GetFullRank().ToFloat();
			float num4 = Mathf.InverseLerp(num, num2, num3);
			float influence2 = Mathf.Lerp(1f, 0.3f, num4);
			Console.Log("Cartel influence for " + eMapRegion.ToString() + " set to " + influence2 + " based on player rank float" + num3);
			array[(int)eMapRegion].Influence = influence2;
		}
		ECartelStatus status = ECartelStatus.Unknown;
		if (eMapRegion > EMapRegion.Westville)
		{
			status = ECartelStatus.Hostile;
		}
		array[0].Influence = 0f;
		Console.Log("Summary of region influence:");
		CartelInfluence.RegionInfluenceData[] array3 = array;
		foreach (CartelInfluence.RegionInfluenceData regionInfluenceData in array3)
		{
			Console.Log($"Region: {regionInfluenceData.Region}, Influence: {regionInfluenceData.Influence}");
		}
		List<CartelRegionalActivityData> list = new List<CartelRegionalActivityData>();
		CartelRegionActivities[] regionalActivities = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Activities.RegionalActivities;
		foreach (CartelRegionActivities cartelRegionActivities in regionalActivities)
		{
			CartelRegionalActivityData item = new CartelRegionalActivityData(cartelRegionActivities.Region, -1, CartelRegionActivities.GetNewCooldown(cartelRegionActivities.Region));
			list.Add(item);
		}
		CartelData data = new CartelData(status, 9999, array, CartelActivities.GetNewCooldown(), list.ToArray(), null, 24);
		NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Load(data);
	}
}
