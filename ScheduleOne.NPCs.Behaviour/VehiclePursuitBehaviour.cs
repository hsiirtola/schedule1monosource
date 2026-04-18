using System;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Lighting;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.AI;
using ScheduleOne.Vision;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class VehiclePursuitBehaviour : Behaviour
{
	public const float RECENT_VISIBILITY_THRESHOLD = 5f;

	public const float EXIT_VEHICLE_MAX_SPEED = 4f;

	public const float CLOSE_ENOUGH_THRESHOLD = 10f;

	public const float UPDATE_FREQUENCY = 0.2f;

	public const float STATIONARY_THRESHOLD = 1f;

	public const float TIME_STATIONARY_TO_EXIT = 3f;

	[Header("Settings")]
	public AnimationCurve RepathDistanceThresholdMap;

	public LandVehicle vehicle;

	private bool initialContactMade;

	private bool aggressiveDrivingEnabled;

	private float timeSinceLastSighting = 10000f;

	private bool visionEventReceived;

	private int consecutiveVehiclePathingFailures;

	private float timeStationary;

	private Vector3 currentDriveTarget = Vector3.zero;

	private int targetChanges;

	private float timeSincePursuitStart;

	private bool beginAsSighted;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public Player Target { get; protected set; }

	public bool IsTargetRecentlyVisible { get; private set; }

	public bool IsTargetImmediatelyVisible { get; private set; }

	private bool isDriving => (Object)(object)vehicle.OccupantNPCs[0] == (Object)(object)base.Npc;

	private VehicleAgent Agent => vehicle.Agent;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void OnDestroy()
	{
		PoliceOfficer.OnPoliceVisionEvent = (Action<VisionEventReceipt>)Delegate.Remove(PoliceOfficer.OnPoliceVisionEvent, new Action<VisionEventReceipt>(ProcessThirdPartyVisionEvent));
	}

	public void BeginAsSighted()
	{
		beginAsSighted = true;
	}

	public override void Activate()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		base.Activate();
		base.Npc.Awareness.VisionCone.RangeMultiplier = 1.8f;
		if (beginAsSighted)
		{
			IsTargetRecentlyVisible = true;
			initialContactMade = true;
			IsTargetImmediatelyVisible = true;
			SetAggressiveDriving(initialContactMade);
			if (InstanceFinder.IsServer)
			{
				DriveTo(GetPlayerChasePoint());
			}
		}
		StartPursuit();
	}

	public override void Resume()
	{
		base.Resume();
		StartPursuit();
	}

	public override void Pause()
	{
		base.Pause();
		initialContactMade = false;
		if (InstanceFinder.IsServer)
		{
			base.Npc.ExitVehicle();
			Agent.StopNavigating();
		}
		base.Npc.Awareness.VisionCone.RangeMultiplier = 1f;
		base.Npc.Awareness.SetAwarenessActive(active: true);
	}

	public override void Deactivate()
	{
		base.Deactivate();
		Disable();
		initialContactMade = false;
		if ((Object)(object)vehicle != (Object)null)
		{
			PoliceLight componentInChildren = ((Component)vehicle).GetComponentInChildren<PoliceLight>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				componentInChildren.IsOn = false;
			}
		}
		if (InstanceFinder.IsServer)
		{
			base.Npc.ExitVehicle();
			Agent.StopNavigating();
			if ((Object)(object)Target != (Object)null)
			{
				(base.Npc as PoliceOfficer).PursuitBehaviour.SetTargetAndEnable_Server(((NetworkBehaviour)Target).NetworkObject);
				(base.Npc as PoliceOfficer).PursuitBehaviour.MarkPlayerVisible();
			}
		}
		base.Npc.Awareness.VisionCone.RangeMultiplier = 1f;
		base.Npc.Awareness.SetAwarenessActive(active: true);
	}

	public virtual void AssignTarget(Player target)
	{
		Target = target;
		visionEventReceived = true;
	}

	private void StartPursuit()
	{
		if ((Object)(object)vehicle == (Object)null)
		{
			Console.LogError("VehiclePursuitBehaviour: Vehicle is unassigned");
			Deactivate();
			return;
		}
		if ((Object)(object)Target == (Object)null)
		{
			Console.LogError("VehiclePursuitBehaviour: TargetPlayer is unassigned");
			Deactivate();
			return;
		}
		if (InstanceFinder.IsServer && (Object)(object)base.Npc.CurrentVehicle != (Object)(object)vehicle)
		{
			if ((Object)(object)base.Npc.CurrentVehicle != (Object)null)
			{
				base.Npc.ExitVehicle();
			}
			base.Npc.EnterVehicle(null, vehicle);
		}
		PoliceLight componentInChildren = ((Component)vehicle).GetComponentInChildren<PoliceLight>();
		if ((Object)(object)componentInChildren != (Object)null)
		{
			componentInChildren.IsOn = true;
		}
		if (!isDriving)
		{
			base.Npc.Awareness.SetAwarenessActive(active: false);
		}
		UpdateDestination();
	}

	public override void BehaviourUpdate()
	{
		base.BehaviourUpdate();
		if (InstanceFinder.IsServer)
		{
			timeSincePursuitStart += Time.deltaTime;
		}
	}

	public override void OnActiveTick()
	{
		base.OnActiveTick();
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (!IsTargetValid())
		{
			Deactivate_Networked(null);
			return;
		}
		if ((Object)(object)Target != (Object)null && (base.Npc as PoliceOfficer).IgnorePlayers)
		{
			if (InstanceFinder.IsServer)
			{
				Disable_Networked(null);
			}
			return;
		}
		CheckExitVehicle();
		if (isDriving)
		{
			SetAggressiveDriving(initialContactMade);
		}
	}

	protected virtual void FixedUpdate()
	{
		if (base.Active)
		{
			CheckTargetVisibility();
		}
	}

	private void UpdateDestination()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Active || !InstanceFinder.IsServer || Agent.NavigationCalculationInProgress || !isDriving)
		{
			return;
		}
		if (Agent.GetIsStuck() && vehicle.Speed_Kmh < 4f)
		{
			Deactivate_Networked(null);
			return;
		}
		Vector3 velocity = vehicle.VelocityCalculator.Velocity;
		if (((Vector3)(ref velocity)).magnitude < 1f)
		{
			timeStationary += 0.2f;
			if (timeStationary > 3f && timeSincePursuitStart > 10f)
			{
				Deactivate_Networked(null);
				return;
			}
		}
		else
		{
			timeStationary = 0f;
		}
		if (IsTargetRecentlyVisible)
		{
			if (IsAsCloseAsPossible(GetPlayerChasePoint(), out var closestPosition) || IsAsCloseAsPossible(Target.Avatar.CenterPoint, out closestPosition) || Vector3.Distance(((Component)vehicle).transform.position, closestPosition) < 10f)
			{
				vehicle.handbrakeOverride = true;
				Agent.StopNavigating();
				if (vehicle.Speed_Kmh < 4f)
				{
					Deactivate_Networked(null);
					return;
				}
			}
			else if (!Agent.AutoDriving || Vector3.Distance(vehicle.Agent.TargetLocation, GetPlayerChasePoint()) > 10f)
			{
				DriveTo(GetPlayerChasePoint());
			}
			float num = Vector3.Distance(currentDriveTarget, Target.CrimeData.LastKnownPosition);
			float num2 = Vector3.Distance(((Component)this).transform.position, Target.CrimeData.LastKnownPosition);
			if (num > RepathDistanceThresholdMap.Evaluate(Mathf.Clamp(num2, 0f, 100f)))
			{
				DriveTo(GetPlayerChasePoint());
			}
			return;
		}
		if (!Agent.AutoDriving)
		{
			if (IsAsCloseAsPossible(Target.CrimeData.LastKnownPosition, out var closestPosition2) || Vector3.Distance(closestPosition2, ((Component)vehicle).transform.position) < 10f)
			{
				if (vehicle.Speed_Kmh < 4f)
				{
					Deactivate_Networked(null);
					return;
				}
			}
			else
			{
				DriveTo(Target.CrimeData.LastKnownPosition);
			}
		}
		float num3 = Vector3.Distance(currentDriveTarget, Target.CrimeData.LastKnownPosition);
		float num4 = Vector3.Distance(((Component)this).transform.position, Target.CrimeData.LastKnownPosition);
		if (num3 > RepathDistanceThresholdMap.Evaluate(Mathf.Clamp(num4, 0f, 100f)))
		{
			DriveTo(Target.CrimeData.LastKnownPosition);
		}
	}

	private bool IsTargetValid()
	{
		if ((Object)(object)Target == (Object)null)
		{
			return false;
		}
		if (Target.IsArrested)
		{
			return false;
		}
		if (Target.IsUnconscious)
		{
			return false;
		}
		if (Target.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
		{
			return false;
		}
		return true;
	}

	private void CheckExitVehicle()
	{
		if (InstanceFinder.IsServer && !isDriving && (Object)(object)vehicle.OccupantNPCs[0] == (Object)null)
		{
			Deactivate_Networked(null);
		}
	}

	private Vector3 GetPlayerChasePoint()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Mathf.Min(5f, Vector3.Distance(Target.Avatar.CenterPoint, ((Component)this).transform.position));
		Vector3 velocity = Target.VelocityCalculator.Velocity;
		Mathf.Clamp01(((Vector3)(ref velocity)).magnitude / 8f);
		return Target.Avatar.CenterPoint;
	}

	private void SetAggressiveDriving(bool aggressive)
	{
		bool flag = aggressiveDrivingEnabled;
		aggressiveDrivingEnabled = aggressive;
		if (aggressive)
		{
			vehicle.Agent.Flags.OverriddenSpeed = 80f;
			vehicle.Agent.Flags.OverriddenReverseSpeed = 20f;
			vehicle.Agent.Flags.OverrideSpeed = true;
			vehicle.Agent.Flags.AutoBrakeAtDestination = false;
			vehicle.Agent.Flags.IgnoreTrafficLights = true;
			vehicle.Agent.Flags.UseRoads = false;
			vehicle.Agent.Flags.ObstacleMode = DriveFlags.EObstacleMode.IgnoreOnlySquishy;
		}
		else
		{
			vehicle.Agent.Flags.OverrideSpeed = false;
			vehicle.Agent.Flags.SpeedLimitMultiplier = 1.5f;
			vehicle.Agent.Flags.AutoBrakeAtDestination = true;
			vehicle.Agent.Flags.IgnoreTrafficLights = true;
			vehicle.Agent.Flags.UseRoads = true;
			vehicle.Agent.Flags.ObstacleMode = DriveFlags.EObstacleMode.Default;
		}
		if (aggressive != flag && vehicle.Agent.AutoDriving)
		{
			vehicle.Agent.RecalculateNavigation();
		}
	}

	private void DriveTo(Vector3 location)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (InstanceFinder.IsServer)
		{
			if (!Agent.IsOnVehicleGraph())
			{
				Deactivate();
				return;
			}
			targetChanges++;
			currentDriveTarget = location;
			Agent.Navigate(location, null, NavigationCallback);
		}
	}

	private void NavigationCallback(VehicleAgent.ENavigationResult status)
	{
		if (status == VehicleAgent.ENavigationResult.Failed)
		{
			consecutiveVehiclePathingFailures++;
		}
		else
		{
			consecutiveVehiclePathingFailures = 0;
		}
		if (consecutiveVehiclePathingFailures > 5 && InstanceFinder.IsServer)
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

	protected void CheckTargetVisibility()
	{
		if ((Object)(object)Target == (Object)null)
		{
			return;
		}
		base.Npc.Awareness.VisionCone.SetSightableStateEnabled(Target, EVisualState.Visible, !IsTargetRecentlyVisible);
		if (IsTargetVisible() && visionEventReceived)
		{
			IsTargetImmediatelyVisible = true;
			IsTargetRecentlyVisible = true;
		}
		else
		{
			timeSinceLastSighting += Time.fixedDeltaTime;
			IsTargetImmediatelyVisible = false;
			if (timeSinceLastSighting < 5f)
			{
				Target.RecordLastKnownPosition(resetTimeSinceLastSeen: false);
				IsTargetRecentlyVisible = true;
			}
			else
			{
				visionEventReceived = false;
				IsTargetRecentlyVisible = false;
			}
		}
		if (IsTargetRecentlyVisible)
		{
			MarkPlayerVisible();
		}
	}

	public void MarkPlayerVisible()
	{
		if (IsTargetVisible())
		{
			Target.RecordLastKnownPosition(resetTimeSinceLastSeen: true);
			timeSinceLastSighting = 0f;
		}
		else
		{
			Target.RecordLastKnownPosition(resetTimeSinceLastSeen: false);
		}
	}

	protected bool IsTargetVisible()
	{
		return base.Npc.Awareness.VisionCone.IsTargetVisible(Target);
	}

	private void ProcessVisionEvent(VisionEventReceipt visionEventReceipt)
	{
		if (base.Active && (Object)(object)visionEventReceipt.Target == (Object)(object)((NetworkBehaviour)Target).NetworkObject)
		{
			TargetSpotted();
			initialContactMade = true;
			if (((NetworkBehaviour)Target).IsOwner && Target.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.Investigating)
			{
				Target.CrimeData.Escalate();
			}
		}
	}

	private void ProcessThirdPartyVisionEvent(VisionEventReceipt visionEventReceipt)
	{
		if (base.Active && (Object)(object)visionEventReceipt.Target == (Object)(object)((NetworkBehaviour)Target).NetworkObject)
		{
			TargetSpotted();
		}
	}

	protected virtual void TargetSpotted()
	{
		IsTargetRecentlyVisible = true;
		IsTargetImmediatelyVisible = true;
		visionEventReceived = true;
		timeSinceLastSighting = 0f;
		NotifyServerTargetSeen();
	}

	[ServerRpc(RequireOwnership = false)]
	public void NotifyServerTargetSeen()
	{
		RpcWriter___Server_NotifyServerTargetSeen_2166136261();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_NotifyServerTargetSeen_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_NotifyServerTargetSeen_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsClientInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___NotifyServerTargetSeen_2166136261()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		visionEventReceived = true;
		IsTargetRecentlyVisible = true;
		IsTargetImmediatelyVisible = true;
		DriveTo(GetPlayerChasePoint());
	}

	private void RpcReader___Server_NotifyServerTargetSeen_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___NotifyServerTargetSeen_2166136261();
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		if (InstanceFinder.IsOffline || InstanceFinder.IsServer)
		{
			VisionCone visionCone = base.Npc.Awareness.VisionCone;
			visionCone.onVisionEventFull = (VisionCone.EventStateChange)Delegate.Combine(visionCone.onVisionEventFull, new VisionCone.EventStateChange(ProcessVisionEvent));
			((MonoBehaviour)this).InvokeRepeating("UpdateDestination", 0.5f, 0.2f);
		}
		PoliceOfficer.OnPoliceVisionEvent = (Action<VisionEventReceipt>)Delegate.Combine(PoliceOfficer.OnPoliceVisionEvent, new Action<VisionEventReceipt>(ProcessThirdPartyVisionEvent));
	}
}
