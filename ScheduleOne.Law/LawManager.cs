using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Vehicles;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Law;

public class LawManager : Singleton<LawManager>
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__2_0;

		internal void _003CStart_003Eb__2_0()
		{
			PoliceOfficer.Officers.Clear();
		}
	}

	public const int DISPATCH_OFFICER_COUNT = 2;

	public static float DISPATCH_VEHICLE_USE_THRESHOLD = 25f;

	protected override void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		base.Start();
		UnityEvent onPreSceneChange = Singleton<LoadManager>.Instance.onPreSceneChange;
		object obj = _003C_003Ec._003C_003E9__2_0;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				PoliceOfficer.Officers.Clear();
			};
			_003C_003Ec._003C_003E9__2_0 = val;
			obj = (object)val;
		}
		onPreSceneChange.AddListener((UnityAction)obj);
	}

	public void PoliceCalled(Player target, Crime crime)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkSingleton<GameManager>.Instance.IsTutorial)
		{
			Console.Log("Police called on " + target.PlayerName);
			PoliceStation closestPoliceStation = PoliceStation.GetClosestPoliceStation(target.CrimeData.LastKnownPosition);
			target.CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: false);
			closestPoliceStation.Dispatch(2, target);
		}
	}

	public PatrolGroup StartFootpatrol(FootPatrolRoute route, int requestedMembers)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		PoliceStation closestPoliceStation = PoliceStation.GetClosestPoliceStation(route.Waypoints[route.StartWaypointIndex].position);
		if (closestPoliceStation.OfficerPool.Count == 0)
		{
			Console.LogWarning(((Object)closestPoliceStation).name + " has no officers in its pool!");
			return null;
		}
		PatrolGroup patrolGroup = new PatrolGroup(route);
		List<PoliceOfficer> list = new List<PoliceOfficer>();
		for (int i = 0; i < requestedMembers; i++)
		{
			if (closestPoliceStation.OfficerPool.Count == 0)
			{
				break;
			}
			list.Add(closestPoliceStation.PullOfficer());
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].StartFootPatrol(patrolGroup, warpToStartPoint: false);
		}
		return patrolGroup;
	}

	public PoliceOfficer StartVehiclePatrol(VehiclePatrolRoute route)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		PoliceStation closestPoliceStation = PoliceStation.GetClosestPoliceStation(route.Waypoints[route.StartWaypointIndex].position);
		if (closestPoliceStation.OfficerPool.Count == 0)
		{
			Console.LogWarning(((Object)closestPoliceStation).name + " has no officers in its pool!");
			return null;
		}
		LandVehicle landVehicle = closestPoliceStation.CreateVehicle();
		PoliceOfficer policeOfficer = closestPoliceStation.PullOfficer();
		policeOfficer.AssignedVehicle = landVehicle;
		policeOfficer.EnterVehicle(null, landVehicle);
		policeOfficer.StartVehiclePatrol(route, landVehicle);
		return policeOfficer;
	}
}
