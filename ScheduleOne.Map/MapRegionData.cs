using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Audio;
using ScheduleOne.Economy;
using ScheduleOne.Levelling;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Relation;
using UnityEngine;

namespace ScheduleOne.Map;

[Serializable]
public class MapRegionData
{
	[Serializable]
	public class RegionContainer
	{
		public EMapRegion Region;
	}

	public EMapRegion Region;

	public string Name;

	public bool UnlockedByDefault;

	public FullRank RankRequirement;

	public NPC[] StartingNPCs;

	public Sprite RegionSprite;

	public DeliveryLocation[] RegionDeliveryLocations;

	public RegionContainer[] AdjacentRegions;

	public PolygonalZone RegionBounds;

	public bool IsUnlocked { get; private set; }

	public DeliveryLocation GetRandomUnscheduledDeliveryLocation()
	{
		List<DeliveryLocation> list = RegionDeliveryLocations.Where((DeliveryLocation x) => x.ScheduledContracts.Count == 0).ToList();
		if (list.Count == 0)
		{
			Console.LogWarning("No unscheduled delivery locations found for " + Region);
			return null;
		}
		return list[Random.Range(0, list.Count)];
	}

	public void SetUnlocked()
	{
		Console.Log($"Region unlocked: {Region}");
		IsUnlocked = true;
		NPC[] startingNPCs = StartingNPCs;
		foreach (NPC nPC in startingNPCs)
		{
			if (!nPC.RelationData.Unlocked)
			{
				nPC.RelationData.Unlock(NPCRelationData.EUnlockType.DirectApproach, notify: false);
			}
		}
	}

	public List<EMapRegion> GetAdjacentRegions()
	{
		List<EMapRegion> list = new List<EMapRegion>();
		RegionContainer[] adjacentRegions = AdjacentRegions;
		foreach (RegionContainer regionContainer in adjacentRegions)
		{
			list.Add(regionContainer.Region);
		}
		return list;
	}
}
