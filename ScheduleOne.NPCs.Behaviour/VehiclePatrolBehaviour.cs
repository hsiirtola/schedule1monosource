using FishNet;
using ScheduleOne.Police;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.AI;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class VehiclePatrolBehaviour : Behaviour
{
	public new const float MAX_CONSECUTIVE_PATHING_FAILURES = 5f;

	public const float PROGRESSION_THRESHOLD = 10f;

	public int CurrentWaypoint;

	[Header("Settings")]
	public VehiclePatrolRoute Route;

	public LandVehicle Vehicle;

	private bool aggressiveDrivingEnabled = true;

	private new int consecutivePathingFailures;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool isDriving => (Object)(object)Vehicle.OccupantNPCs[0] == (Object)(object)base.Npc;

	private VehicleAgent Agent => Vehicle.Agent;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void Activate()
	{
		base.Activate();
		StartPatrol();
	}

	public override void Resume()
	{
		base.Resume();
		StartPatrol();
	}

	public override void Pause()
	{
		base.Pause();
		if (InstanceFinder.IsServer)
		{
			base.Npc.ExitVehicle();
			Agent.StopNavigating();
		}
		base.Npc.Awareness.VisionCone.RangeMultiplier = 1f;
		(base.Npc as PoliceOfficer).BodySearchChance = 0.1f;
		base.Npc.Awareness.SetAwarenessActive(active: true);
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (InstanceFinder.IsServer)
		{
			base.Npc.ExitVehicle();
			Agent.StopNavigating();
		}
		base.Npc.Awareness.VisionCone.RangeMultiplier = 1f;
		(base.Npc as PoliceOfficer).BodySearchChance = 0.1f;
		base.Npc.Awareness.SetAwarenessActive(active: true);
	}

	public void SetRoute(VehiclePatrolRoute route)
	{
		Route = route;
	}

	private void StartPatrol()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if ((Object)(object)Vehicle == (Object)null)
		{
			Console.LogError("VehiclePursuitBehaviour: Vehicle is unassigned");
			Disable_Networked(null);
			Deactivate_Networked(null);
		}
		else if (InstanceFinder.IsServer && (Object)(object)base.Npc.CurrentVehicle != (Object)(object)Vehicle)
		{
			if ((Object)(object)base.Npc.CurrentVehicle != (Object)null)
			{
				base.Npc.ExitVehicle();
			}
			base.Npc.EnterVehicle(null, Vehicle);
		}
	}

	public override void OnActiveTick()
	{
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (!InstanceFinder.IsServer || !isDriving)
		{
			return;
		}
		if (Agent.AutoDriving)
		{
			if (!Agent.NavigationCalculationInProgress && Vector3.Distance(((Component)Vehicle).transform.position, Route.Waypoints[CurrentWaypoint].position) < 10f)
			{
				CurrentWaypoint++;
				if (CurrentWaypoint >= Route.Waypoints.Length)
				{
					Disable_Networked(null);
				}
				else
				{
					DriveTo(Route.Waypoints[CurrentWaypoint].position);
				}
			}
		}
		else if (CurrentWaypoint >= Route.Waypoints.Length)
		{
			Disable_Networked(null);
		}
		else
		{
			DriveTo(Route.Waypoints[CurrentWaypoint].position);
		}
	}

	private void DriveTo(Vector3 location)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!Agent.IsOnVehicleGraph())
		{
			Deactivate();
		}
		else
		{
			Agent.Navigate(location, null, NavigationCallback);
		}
	}

	private void NavigationCallback(VehicleAgent.ENavigationResult status)
	{
		if (status == VehicleAgent.ENavigationResult.Failed)
		{
			consecutivePathingFailures++;
		}
		else
		{
			consecutivePathingFailures = 0;
		}
		if ((float)consecutivePathingFailures > 5f && InstanceFinder.IsServer)
		{
			Deactivate_Networked(null);
		}
	}

	private bool IsAsCloseAsPossible(Vector3 pos, out Vector3 closestPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		closestPosition = NavigationUtility.SampleVehicleGraph(pos);
		return Vector3.Distance(closestPosition, ((Component)this).transform.position) < 10f;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
	}
}
