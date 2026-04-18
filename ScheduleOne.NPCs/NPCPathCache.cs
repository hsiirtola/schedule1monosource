using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.NPCs;

public class NPCPathCache
{
	[Serializable]
	public class PathCache
	{
		public Vector3 Start;

		public Vector3 End;

		public NavMeshPath Path;

		public PathCache(Vector3 start, Vector3 end, NavMeshPath path)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			Start = start;
			End = end;
			Path = path;
		}
	}

	public List<PathCache> Paths { get; private set; } = new List<PathCache>();

	public NavMeshPath GetPath(Vector3 start, Vector3 end, float sqrMaxDistance)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		foreach (PathCache path in Paths)
		{
			Vector3 val = path.Start - start;
			if (((Vector3)(ref val)).sqrMagnitude < sqrMaxDistance)
			{
				val = path.End - end;
				if (((Vector3)(ref val)).sqrMagnitude < sqrMaxDistance)
				{
					return path.Path;
				}
			}
		}
		return null;
	}

	public void AddPath(Vector3 start, Vector3 end, NavMeshPath path)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Paths.Add(new PathCache(start, end, path));
	}
}
