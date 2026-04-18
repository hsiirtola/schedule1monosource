using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Map;

public class Map : Singleton<Map>
{
	public const EMapRegion FINAL_REGION = EMapRegion.Uptown;

	public bool UNLOCK_ALL_REGIONS;

	public MapRegionData[] Regions;

	[Header("References")]
	public PoliceStation PoliceStation;

	public MedicalCentre MedicalCentre;

	public Transform TreeBounds;

	protected override void Awake()
	{
		base.Awake();
		if (!GameManager.IS_TUTORIAL)
		{
			foreach (EMapRegion region in Enum.GetValues(typeof(EMapRegion)))
			{
				if (Regions == null || Array.Find(Regions, (MapRegionData x) => x.Region == region) == null)
				{
					Console.LogError($"No region data found for {region}");
				}
			}
		}
		MapRegionData[] regions = Regions;
		foreach (MapRegionData mapRegionData in regions)
		{
			if (mapRegionData.UnlockedByDefault)
			{
				mapRegionData.SetUnlocked();
			}
		}
		if ((Object)(object)TreeBounds != (Object)null)
		{
			((Component)TreeBounds).gameObject.SetActive(false);
		}
	}

	protected override void Start()
	{
		base.Start();
		if (Application.isEditor && UNLOCK_ALL_REGIONS)
		{
			MapRegionData[] regions = Regions;
			for (int i = 0; i < regions.Length; i++)
			{
				regions[i].SetUnlocked();
			}
		}
		LevelManager levelManager = NetworkSingleton<LevelManager>.Instance;
		levelManager.onRankUp = (Action<FullRank, FullRank>)Delegate.Combine(levelManager.onRankUp, new Action<FullRank, FullRank>(OnRankUp));
	}

	private void OnRankUp(FullRank old, FullRank newRank)
	{
		MapRegionData regionData = GetRegionData(EMapRegion.Westville);
		if (!regionData.IsUnlocked && newRank >= regionData.RankRequirement)
		{
			regionData.SetUnlocked();
			Singleton<RegionUnlockedCanvas>.Instance.QueueUnlocked(EMapRegion.Westville);
		}
	}

	public MapRegionData GetRegionData(EMapRegion region)
	{
		return Array.Find(Regions, (MapRegionData x) => x.Region == region);
	}

	public List<EMapRegion> GetUnlockedRegions()
	{
		List<EMapRegion> list = new List<EMapRegion>();
		MapRegionData[] regions = Regions;
		foreach (MapRegionData mapRegionData in regions)
		{
			if (mapRegionData.IsUnlocked)
			{
				list.Add(mapRegionData.Region);
			}
		}
		return list;
	}

	public EMapRegion GetRegionFromPosition(Vector3 position)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		MapRegionData[] regions = Regions;
		foreach (MapRegionData mapRegionData in regions)
		{
			if (mapRegionData.RegionBounds.IsPointInsidePolygon(position))
			{
				return mapRegionData.Region;
			}
		}
		Console.LogError($"No region found for position {position}. Returning default region.");
		return EMapRegion.Northtown;
	}
}
