using System.Collections.Generic;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence;
using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.DevUtilities;

public static class NavMeshUtility
{
	public const float SAMPLE_MAX_DISTANCE = 2f;

	public const float SAMPLE_CACHE_MAX_DIST = 0.5f;

	public const float SAMPLE_CACHE_MAX_SQR_DIST = 0.25f;

	public const float MAX_CACHE_SIZE = 10000f;

	public static Dictionary<Vector3, Vector3> SampleCache = new Dictionary<Vector3, Vector3>();

	public static List<Vector3> sampleCacheKeys = new List<Vector3>();

	public static float GetPathLength(NavMeshPath path)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (path == null)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 1; i < path.corners.Length; i++)
		{
			num += Vector3.Distance(path.corners[i - 1], path.corners[i]);
		}
		return num;
	}

	public static Transform GetReachableAccessPoint(ITransitEntity entity, NPC npc)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (entity == null)
		{
			return null;
		}
		if (entity.AccessPoints == null || entity.AccessPoints.Length == 0)
		{
			Console.LogWarning("Entity has no access points!");
			return null;
		}
		float num = float.MaxValue;
		Transform result = null;
		BuildableItem buildableItem = entity as BuildableItem;
		for (int i = 0; i < entity.AccessPoints.Length; i++)
		{
			NavMeshPath path;
			if ((Object)(object)entity.AccessPoints[i] == (Object)null)
			{
				Console.LogWarning("Access point is null!");
			}
			else if ((!((Object)(object)buildableItem != (Object)null) || buildableItem.ParentProperty.DoBoundsContainPoint(entity.AccessPoints[i].position)) && npc.Movement.CanGetTo(entity.AccessPoints[i].position, 1f, out path))
			{
				float num2 = ((path != null) ? GetPathLength(path) : Vector3.Distance(((Component)npc).transform.position, entity.AccessPoints[i].position));
				if (num2 < num)
				{
					num = num2;
					result = entity.AccessPoints[i];
				}
			}
		}
		return result;
	}

	public static bool IsAtTransitEntity(ITransitEntity entity, NPC npc, float distanceThreshold = 0.4f)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (entity == null)
		{
			Console.LogWarning("IsAtTransitEntity: Entity is null!");
		}
		for (int i = 0; i < entity.AccessPoints.Length; i++)
		{
			if (Vector3.Distance(((Component)npc).transform.position, entity.AccessPoints[i].position) < distanceThreshold)
			{
				return true;
			}
			if (npc.Movement.IsAsCloseAsPossible(((Component)entity.AccessPoints[i]).transform.position, distanceThreshold))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetNavMeshAgentID(string name)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
		{
			NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(i);
			if (name == NavMesh.GetSettingsNameFromID(((NavMeshBuildSettings)(ref settingsByIndex)).agentTypeID))
			{
				return ((NavMeshBuildSettings)(ref settingsByIndex)).agentTypeID;
			}
		}
		return -1;
	}

	public static bool SamplePosition(Vector3 sourcePosition, out NavMeshHit hit, float maxDistance, int areaMask, bool useCache = true)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (useCache)
		{
			for (int i = 0; i < sampleCacheKeys.Count; i++)
			{
				if (Vector3.SqrMagnitude(sourcePosition - sampleCacheKeys[i]) < 0.25f)
				{
					hit = default(NavMeshHit);
					((NavMeshHit)(ref hit)).position = SampleCache[sampleCacheKeys[i]];
					return true;
				}
			}
		}
		bool num = NavMesh.SamplePosition(sourcePosition, ref hit, maxDistance, areaMask);
		if (num)
		{
			CacheSampleResult(sourcePosition, ((NavMeshHit)(ref hit)).position);
		}
		return num;
	}

	private static void CacheSampleResult(Vector3 sourcePosition, Vector3 hitPosition)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!Singleton<LoadManager>.InstanceExists || !Singleton<LoadManager>.Instance.IsLoading)
		{
			if ((float)sampleCacheKeys.Count >= 10000f)
			{
				Console.LogWarning("Sample cache is full! Clearing cache...");
				ClearCache();
			}
			Vector3 val = Quantize(sourcePosition);
			if (!sampleCacheKeys.Contains(val))
			{
				sampleCacheKeys.Add(val);
				SampleCache.Add(val, hitPosition);
			}
		}
	}

	private static Vector3 Quantize(Vector3 position, float precision = 0.1f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Mathf.Round(position.x / precision) * precision, Mathf.Round(position.y / precision) * precision, Mathf.Round(position.z / precision) * precision);
	}

	public static void ClearCache()
	{
		SampleCache.Clear();
		sampleCacheKeys.Clear();
	}
}
