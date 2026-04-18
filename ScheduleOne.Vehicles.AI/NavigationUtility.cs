using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using ScheduleOne.DevUtilities;
using ScheduleOne.Math;
using UnityEngine;

namespace ScheduleOne.Vehicles.AI;

public class NavigationUtility
{
	public enum ENavigationCalculationResult
	{
		Success,
		Failed
	}

	public delegate void NavigationCalculationCallback(ENavigationCalculationResult result, PathSmoothingUtility.SmoothedPath path);

	public delegate void PathGroupEvent(PathGroup calculatedGroup);

	public const float ROAD_MULTIPLIER = 1f;

	public const float OFFROAD_MULTIPLIER = 3f;

	public static Coroutine CalculatePath(Vector3 startPosition, Vector3 destination, NavigationSettings navSettings, DriveFlags flags, Seeker generalSeeker, Seeker roadSeeker, NavigationCalculationCallback callback)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		PathGroup lastGeneratedPathGroup;
		bool pathGroupGenerated;
		Path lastCalculatedPath;
		return ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		void PathCompleted(Path p)
		{
			lastCalculatedPath = p;
		}
		void PathGroupCallback(PathGroup pg)
		{
			lastGeneratedPathGroup = pg;
			pathGroupGenerated = true;
		}
		IEnumerator Routine()
		{
			_ = Time.realtimeSinceStartup;
			PathGroup finalGroup = null;
			if (flags.UseRoads)
			{
				List<NodeLink> closestNodeLinks = NodeLink.GetClosestLinks(startPosition);
				List<NodeLink> nodeLinksClosestToLocation = NodeLink.GetClosestLinks(destination);
				FunnelZone funnelZone = FunnelZone.GetFunnelZone(destination);
				if ((Object)(object)funnelZone != (Object)null)
				{
					nodeLinksClosestToLocation = NodeLink.GetClosestLinks(funnelZone.entryPoint.position);
				}
				int entryPointChecks = 3;
				List<Vector3> checkedEntryPoints = new List<Vector3>();
				List<PathGroup> groups = new List<PathGroup>();
				for (int i = 0; i < entryPointChecks && i < closestNodeLinks.Count; i++)
				{
					Vector3 entryPoint = closestNodeLinks[i].GetClosestPoint(startPosition);
					if (DoesCloseDistanceExist(checkedEntryPoints, entryPoint, 1f))
					{
						entryPointChecks += 2;
					}
					else
					{
						checkedEntryPoints.Add(entryPoint);
						int exitPointChecks = 3;
						List<Vector3> checkedExitPoints = new List<Vector3>();
						for (int j = 0; j < exitPointChecks; j++)
						{
							NodeLink val = nodeLinksClosestToLocation[j];
							Vector3 closestPoint = val.GetClosestPoint(destination);
							if (DoesCloseDistanceExist(checkedExitPoints, closestPoint, 1f))
							{
								exitPointChecks += 2;
							}
							else
							{
								checkedExitPoints.Add(closestPoint);
								lastGeneratedPathGroup = null;
								pathGroupGenerated = false;
								((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(GenerateNavigationGroup(startPosition, entryPoint, val, closestPoint, destination, generalSeeker, roadSeeker, PathGroupCallback));
								yield return (object)new WaitUntil((Func<bool>)(() => pathGroupGenerated));
								if (lastGeneratedPathGroup != null)
								{
									groups.Add(lastGeneratedPathGroup);
								}
							}
						}
					}
				}
				if (groups.Count > 0)
				{
					groups = groups.OrderBy((PathGroup x) => ((x.startToEntryPath.vectorPath == null) ? 0f : (x.startToEntryPath.GetTotalLength() * 3f)) + ((x.entryToExitPath.vectorPath == null) ? 0f : (x.entryToExitPath.GetTotalLength() * 1f)) + ((x.exitToDestinationPath.vectorPath == null) ? 0f : (x.exitToDestinationPath.GetTotalLength() * 3f))).ToList();
					finalGroup = groups[0];
					if (navSettings.endAtRoad)
					{
						finalGroup.exitToDestinationPath = null;
					}
					lastCalculatedPath = null;
					generalSeeker.StartPath(startPosition, destination, new OnPathDelegate(PathCompleted));
					yield return (object)new WaitUntil((Func<bool>)(() => lastCalculatedPath != null));
					Path val2 = lastCalculatedPath;
					if (finalGroup.startToEntryPath.GetTotalLength() > val2.GetTotalLength())
					{
						finalGroup = new PathGroup
						{
							startToEntryPath = val2
						};
					}
				}
			}
			else
			{
				lastCalculatedPath = null;
				generalSeeker.StartPath(startPosition, destination, new OnPathDelegate(PathCompleted));
				yield return (object)new WaitUntil((Func<bool>)(() => lastCalculatedPath != null));
				if (!lastCalculatedPath.error)
				{
					finalGroup = new PathGroup
					{
						entryToExitPath = lastCalculatedPath
					};
				}
			}
			int num = 0;
			if (finalGroup != null)
			{
				if (finalGroup.startToEntryPath != null)
				{
					num += finalGroup.startToEntryPath.vectorPath.Count;
				}
				if (finalGroup.entryToExitPath != null)
				{
					num += finalGroup.entryToExitPath.vectorPath.Count;
				}
				if (finalGroup.exitToDestinationPath != null)
				{
					num += finalGroup.exitToDestinationPath.vectorPath.Count;
				}
			}
			if (finalGroup == null || num < 2)
			{
				if (callback != null)
				{
					callback(ENavigationCalculationResult.Failed, null);
				}
			}
			else
			{
				AdjustEntryPoint(finalGroup);
				if (finalGroup.entryToExitPath != null && finalGroup.exitToDestinationPath != null)
				{
					AdjustExitPoint(finalGroup);
				}
				DrawPath(finalGroup);
				PathSmoothingUtility.SmoothedPath smoothedPath = GetSmoothedPath(finalGroup);
				callback(ENavigationCalculationResult.Success, smoothedPath);
			}
		}
	}

	private static void AdjustExitPoint(PathGroup group)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		if (group.entryToExitPath.vectorPath.Count < 4 || group.exitToDestinationPath.vectorPath.Count < 2 || group.exitToDestinationPath.GetTotalLength() < 5f)
		{
			return;
		}
		for (int num = Mathf.Min(5, group.exitToDestinationPath.vectorPath.Count - 1); num >= 0; num--)
		{
			Vector3 val = group.exitToDestinationPath.vectorPath[num];
			Vector3 val2 = Vector3.zero;
			float num2 = float.MaxValue;
			int num3 = 0;
			for (int i = 0; i < 3; i++)
			{
				int num4 = group.entryToExitPath.vectorPath.Count - 1 - i;
				int index = num4 - 1;
				Vector3 line_end = group.entryToExitPath.vectorPath[num4];
				Vector3 line_start = group.entryToExitPath.vectorPath[index];
				Vector3 closestPointOnFiniteLine = GetClosestPointOnFiniteLine(val, line_start, line_end);
				if (Vector3.Distance(val, closestPointOnFiniteLine) < num2)
				{
					num2 = Vector3.Distance(val, closestPointOnFiniteLine);
					val2 = closestPointOnFiniteLine;
					num3 = num4;
				}
			}
			if (val2 == Vector3.zero)
			{
				Debug.LogWarning((object)"Failed to find closest entry-to-exit path point");
				break;
			}
			float num5 = 0f;
			for (int j = 0; j < num; j++)
			{
				num5 += Vector3.Distance(group.exitToDestinationPath.vectorPath[j], group.exitToDestinationPath.vectorPath[j + 1]);
			}
			num5 += Vector3.Distance(val2, group.entryToExitPath.vectorPath[num3]);
			for (int k = num3; k < group.entryToExitPath.vectorPath.Count - 1; k++)
			{
				num5 += Vector3.Distance(group.entryToExitPath.vectorPath[k], group.entryToExitPath.vectorPath[k + 1]);
			}
			if (num2 < num5 * 0.5f)
			{
				for (int l = num3; l < group.entryToExitPath.vectorPath.Count; l++)
				{
					group.entryToExitPath.vectorPath.RemoveAt(num3);
				}
				group.entryToExitPath.vectorPath.Insert(num3, val2);
				for (int m = 0; m < num; m++)
				{
					group.exitToDestinationPath.vectorPath.RemoveAt(0);
				}
				Debug.DrawLine(val, val2, Color.green, 1f);
				break;
			}
		}
	}

	private static void AdjustEntryPoint(PathGroup group)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		if (group.startToEntryPath != null && group.startToEntryPath.vectorPath.Count >= 2 && !(group.startToEntryPath.GetTotalLength() < 5f) && group.entryToExitPath != null && group.entryToExitPath.vectorPath.Count >= 2 && !(group.entryToExitPath.GetTotalLength() < 5f))
		{
			float num = 2f;
			Vector3 val = group.startToEntryPath.vectorPath[group.startToEntryPath.vectorPath.Count - 1];
			Vector3 val2 = group.startToEntryPath.vectorPath[group.startToEntryPath.vectorPath.Count - 2];
			Vector3 val3 = val - val2;
			Vector3 normalized = ((Vector3)(ref val3)).normalized;
			Vector3 value = val - normalized * num;
			group.startToEntryPath.vectorPath[group.startToEntryPath.vectorPath.Count - 1] = value;
			Vector3 val4 = group.entryToExitPath.vectorPath[0];
			val3 = group.entryToExitPath.vectorPath[1] - val4;
			normalized = ((Vector3)(ref val3)).normalized;
			Vector3 value2 = val4 + normalized * num;
			group.entryToExitPath.vectorPath[0] = value2;
		}
	}

	private static bool DoesCloseDistanceExist(List<Vector3> vectorList, Vector3 point, float thresholdDistance)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		foreach (Vector3 vector in vectorList)
		{
			if (Vector3.Distance(vector, point) <= thresholdDistance)
			{
				return true;
			}
		}
		return false;
	}

	private static IEnumerator GenerateNavigationGroup(Vector3 startPoint, Vector3 entryPoint, NodeLink exitLink, Vector3 exitPoint, Vector3 destination, Seeker generalSeeker, Seeker roadSeeker, PathGroupEvent callback)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Vector3 closestPointOnGraph = AstarUtility.GetClosestPointOnGraph(startPoint, "General Vehicle Graph");
		Vector3 destinationOnGraph = AstarUtility.GetClosestPointOnGraph(destination, "General Vehicle Graph");
		Path lastCalculatedPath = null;
		generalSeeker.StartPath(closestPointOnGraph, entryPoint, new OnPathDelegate(PathCompleted));
		yield return (object)new WaitUntil((Func<bool>)(() => lastCalculatedPath != null));
		if (lastCalculatedPath.error)
		{
			callback(null);
			yield break;
		}
		Path path_StartToEntry = lastCalculatedPath;
		lastCalculatedPath = null;
		Vector3 position = NodeLink.GetClosestLinks(entryPoint)[0].Start.position;
		roadSeeker.StartPath(position, exitLink.Start.position, new OnPathDelegate(PathCompleted));
		yield return (object)new WaitUntil((Func<bool>)(() => lastCalculatedPath != null));
		if (lastCalculatedPath.error)
		{
			callback(null);
			yield break;
		}
		lastCalculatedPath.vectorPath[0] = entryPoint;
		lastCalculatedPath.vectorPath.Add(exitPoint);
		Path path_EntryToExit = lastCalculatedPath;
		lastCalculatedPath = null;
		generalSeeker.StartPath(exitPoint, destinationOnGraph, new OnPathDelegate(PathCompleted));
		yield return (object)new WaitUntil((Func<bool>)(() => lastCalculatedPath != null));
		if (lastCalculatedPath.error)
		{
			callback(null);
			yield break;
		}
		Path exitToDestinationPath = lastCalculatedPath;
		PathGroup pathGroup = new PathGroup();
		pathGroup.entryPoint = entryPoint;
		pathGroup.startToEntryPath = path_StartToEntry;
		pathGroup.entryToExitPath = path_EntryToExit;
		pathGroup.exitToDestinationPath = exitToDestinationPath;
		callback(pathGroup);
		void PathCompleted(Path p)
		{
			lastCalculatedPath = p;
		}
	}

	public static void DrawPath(PathGroup group, float duration = 10f)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (group.startToEntryPath != null)
		{
			for (int i = 1; i < group.startToEntryPath.vectorPath.Count; i++)
			{
				Debug.DrawLine(group.startToEntryPath.vectorPath[i], group.startToEntryPath.vectorPath[i - 1], Color.red, duration);
			}
		}
		if (group.entryToExitPath != null)
		{
			for (int j = 1; j < group.entryToExitPath.vectorPath.Count; j++)
			{
				if (j % 2 == 0)
				{
					Debug.DrawLine(group.entryToExitPath.vectorPath[j], group.entryToExitPath.vectorPath[j - 1], Color.blue, duration);
				}
				else
				{
					Debug.DrawLine(group.entryToExitPath.vectorPath[j], group.entryToExitPath.vectorPath[j - 1], Color.white, duration);
				}
			}
		}
		if (group.exitToDestinationPath != null)
		{
			for (int k = 1; k < group.exitToDestinationPath.vectorPath.Count; k++)
			{
				Debug.DrawLine(group.exitToDestinationPath.vectorPath[k], group.exitToDestinationPath.vectorPath[k - 1], Color.yellow, duration);
			}
		}
	}

	private static PathSmoothingUtility.SmoothedPath GetSmoothedPath(PathGroup group)
	{
		List<Vector3> list = new List<Vector3>();
		if (group.startToEntryPath != null)
		{
			list.AddRange(group.startToEntryPath.vectorPath);
		}
		if (group.entryToExitPath != null)
		{
			list.AddRange(group.entryToExitPath.vectorPath);
		}
		if (group.exitToDestinationPath != null)
		{
			list.AddRange(group.exitToDestinationPath.vectorPath);
		}
		return PathSmoothingUtility.CalculateSmoothedPath(list);
	}

	public static Vector3 SampleVehicleGraph(Vector3 destination)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		NNConstraint val = new NNConstraint();
		val.graphMask = GraphMask.FromGraphName("General Vehicle Graph");
		return AstarPath.active.GetNearest(destination, val).position;
	}

	public static Vector3 GetClosestPointOnFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = line_end - line_start;
		float magnitude = ((Vector3)(ref val)).magnitude;
		((Vector3)(ref val)).Normalize();
		float num = Mathf.Clamp(Vector3.Dot(point - line_start, val), 0f, magnitude);
		return line_start + val * num;
	}
}
