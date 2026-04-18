using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using UnityEngine;

namespace ScheduleOne.Law;

[Serializable]
public class CheckpointInstance
{
	public const float MIN_ACTIVATION_DISTANCE = 50f;

	public CheckpointManager.ECheckpointLocation Location;

	public int Members = 2;

	public int StartTime = 800;

	public int EndTime = 2000;

	[Range(1f, 10f)]
	public int IntensityRequirement = 5;

	public bool OnlyIfCurfewEnabled;

	private RoadCheckpoint checkPoint;

	public RoadCheckpoint activeCheckpoint { get; protected set; }

	public void Evaluate()
	{
		if ((Object)(object)checkPoint == (Object)null)
		{
			checkPoint = NetworkSingleton<CheckpointManager>.Instance.GetCheckpoint(Location);
		}
		if (!((Object)(object)activeCheckpoint != (Object)null) && checkPoint.ActivationState != RoadCheckpoint.ECheckpointState.Enabled && Singleton<LawController>.Instance.LE_Intensity >= IntensityRequirement && NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, EndTime) && (!OnlyIfCurfewEnabled || NetworkSingleton<CurfewManager>.Instance.IsEnabled) && DistanceRequirementsMet())
		{
			EnableCheckpoint();
		}
	}

	public void EnableCheckpoint()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)activeCheckpoint != (Object)null)
		{
			Console.LogWarning("StartPatrol called but patrol is already active.");
		}
		else if (PoliceStation.GetClosestPoliceStation(Vector3.zero).OfficerPool.Count != 0)
		{
			activeCheckpoint = NetworkSingleton<CheckpointManager>.Instance.GetCheckpoint(Location);
			NetworkSingleton<CheckpointManager>.Instance.SetCheckpointEnabled(Location, enabled: true, Members);
			NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
		}
	}

	private bool DistanceRequirementsMet()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		float distance;
		Player closestPlayer = Player.GetClosestPlayer(((Component)NetworkSingleton<CheckpointManager>.Instance.GetCheckpoint(Location)).transform.position, out distance);
		if (NetworkSingleton<TimeManager>.Instance.IsSleepInProgress || (Object)(object)closestPlayer == (Object)null || distance >= 50f)
		{
			return true;
		}
		return false;
	}

	private void MinPass()
	{
		if (!NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, EndTime) && DistanceRequirementsMet())
		{
			DisableCheckpoint();
		}
	}

	public void DisableCheckpoint()
	{
		if (!((Object)(object)activeCheckpoint == (Object)null))
		{
			NetworkSingleton<CheckpointManager>.Instance.SetCheckpointEnabled(Location, enabled: false, Members);
			activeCheckpoint = null;
			NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
		}
	}
}
