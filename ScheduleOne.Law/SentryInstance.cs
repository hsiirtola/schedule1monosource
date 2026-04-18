using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.Police;
using UnityEngine;

namespace ScheduleOne.Law;

[Serializable]
public class SentryInstance
{
	public SentryLocation[] _potentialLocations;

	public int Members = 2;

	[Header("Timing")]
	public int StartTime = 2000;

	public int EndTime = 100;

	[Range(1f, 10f)]
	public int IntensityRequirement = 5;

	public bool OnlyIfCurfewEnabled;

	private List<PoliceOfficer> _activeOfficers = new List<PoliceOfficer>();

	private SentryLocation _activeLocation;

	public void Evaluate()
	{
		if (_activeOfficers.Count <= 0 && !((Object)(object)GetRandomUnoccupiedLocation() == (Object)null) && Singleton<LawController>.Instance.LE_Intensity >= IntensityRequirement && NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, EndTime) && (!OnlyIfCurfewEnabled || NetworkSingleton<CurfewManager>.Instance.IsEnabled))
		{
			StartEntry();
		}
	}

	public void StartEntry()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		_activeLocation = GetRandomUnoccupiedLocation();
		if ((Object)(object)_activeLocation == (Object)null)
		{
			return;
		}
		PoliceStation closestPoliceStation = PoliceStation.GetClosestPoliceStation(((Component)_activeLocation).transform.position);
		if (closestPoliceStation.OfficerPool.Count == 0)
		{
			return;
		}
		for (int i = 0; i < Members; i++)
		{
			PoliceOfficer policeOfficer = closestPoliceStation.PullOfficer();
			if ((Object)(object)policeOfficer == (Object)null)
			{
				Console.LogWarning("Failed to pull officer from station");
				break;
			}
			policeOfficer.AssignToSentryLocation(_activeLocation);
			_activeOfficers.Add(policeOfficer);
		}
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
	}

	private void MinPass()
	{
		if (!NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, EndTime))
		{
			EndSentry();
		}
	}

	public void EndSentry()
	{
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
		for (int i = 0; i < _activeOfficers.Count; i++)
		{
			_activeOfficers[i].UnassignFromSentryLocation();
		}
		_activeLocation = null;
		_activeOfficers.Clear();
	}

	private SentryLocation GetRandomUnoccupiedLocation()
	{
		List<SentryLocation> list = new List<SentryLocation>();
		for (int i = 0; i < _potentialLocations.Length; i++)
		{
			if (_potentialLocations[i].AssignedOfficers.Count == 0)
			{
				list.Add(_potentialLocations[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		int index = Random.Range(0, list.Count);
		return list[index];
	}
}
