using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.AvatarFramework.Animation;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.Dragging;
using ScheduleOne.Management;
using ScheduleOne.Map;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Skating;
using ScheduleOne.Tools;
using ScheduleOne.Vehicles;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ScheduleOne.NPCs;

public class NPCMovement : NetworkBehaviour
{
	public enum EAgentType
	{
		Humanoid,
		BigHumanoid,
		IgnoreCosts
	}

	public enum EStance
	{
		None,
		Stanced
	}

	public enum WalkResult
	{
		Failed,
		Interrupted,
		Stopped,
		Partial,
		Success
	}

	private const float VehicleRunoverSpeed = 10f;

	private const float VehicleRunoverRelativeVelocityThreshold_Sqr = 7.71605f;

	private const float VehicleImpactCooldown = 0.25f;

	private const float VehicleImpactForceMultiplier = 5f;

	private const float SkateboardRunoverSpeed = 10f;

	private const float SkateboardImpactForceMultiplier = 4f;

	public const float LIGHT_FLINCH_THRESHOLD = 50f;

	public const float HEAVY_FLINCH_THRESHOLD = 100f;

	public const float RAGDOLL_THRESHOLD = 150f;

	public const float MOMENTUM_ANNOYED_THRESHOLD = 10f;

	public const float MOMENTUM_LIGHT_FLINCH_THRESHOLD = 20f;

	public const float MOMENTUM_HEAVY_FLINCH_THRESHOLD = 40f;

	public const float MOMENTUM_RAGDOLL_THRESHOLD = 60f;

	public const bool USE_PATH_CACHE = true;

	public const float STUMBLE_DURATION = 0.66f;

	public const float STUMBLE_FORCE = 7f;

	public const float OBSTACLE_AVOIDANCE_RANGE = 25f;

	public const float PLAYER_DIST_IMPACT_THRESHOLD = 30f;

	public static Dictionary<Vector3, Vector3> cachedClosestReachablePoints = new Dictionary<Vector3, Vector3>();

	public static List<Vector3> cachedClosestPointKeys = new List<Vector3>();

	public const float CLOSEST_REACHABLE_POINT_CACHE_MAX_SQR_OFFSET = 1f;

	public bool DEBUG;

	[Header("Settings")]
	public float WalkSpeed = 1.8f;

	public float RunSpeed = 7f;

	public float MoveSpeedMultiplier = 1f;

	[Header("Obstacle Avoidance")]
	public bool ObstacleAvoidanceEnabled = true;

	public ObstacleAvoidanceType DefaultObstacleAvoidanceType = (ObstacleAvoidanceType)4;

	[Header("Slippery Mode")]
	public bool SlipperyMode;

	public float SlipperyModeMultiplier = 1f;

	[Header("References")]
	public NavMeshAgent Agent;

	public NPCSpeedController SpeedController;

	public CapsuleCollider CapsuleCollider;

	public NPCAnimation Animation;

	public SmoothedVelocityCalculator VelocityCalculator;

	public Draggable RagdollDraggable;

	public Collider RagdollDraggableCollider;

	protected NPC npc;

	public float MovementSpeedScale;

	private float ragdollStaticTime;

	public UnityEvent<LandVehicle> onHitByCar;

	public UnityEvent onRagdollStart;

	public UnityEvent onRagdollEnd;

	private bool cacheNextPath;

	private Vector3 currentDestination_Reachable = Vector3.zero;

	private Action<WalkResult> walkResultCallback;

	private float currentMaxDistanceForSuccess = 0.5f;

	private bool forceIsMoving;

	private Coroutine faceDirectionRoutine;

	private List<ConstantForce> ragdollForceComponents = new List<ConstantForce>();

	private float timeUntilNextStumble;

	private float timeSinceStumble = 1000f;

	private Vector3 stumbleDirection = Vector3.zero;

	private CircularQueue<Vector3> desiredVelocityHistory;

	private int desiredVelocityHistoryLength = 40;

	private float velocityHistorySpacing = 0.05f;

	private float timeSinceLastVelocityHistoryRecord;

	private NavMeshPath agentCurrentPath;

	private float agentCurrentSpeed;

	private Vector3[] agentCurrentPathCorners;

	private Coroutine ladderClimbRoutine;

	private float _defaultAngularSpeed;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCMovementAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCMovementAssembly_002DCSharp_002Edll_Excuted;

	public bool HasDestination { get; protected set; }

	public bool IsMoving
	{
		get
		{
			if ((!Agent.hasPath && !Agent.pathPending) || !(Agent.remainingDistance > 0.25f))
			{
				return forceIsMoving;
			}
			return true;
		}
	}

	public bool IsPaused { get; protected set; }

	public Vector3 FootPosition => ((Component)this).transform.position;

	public float GravityMultiplier { get; protected set; } = 1f;

	public EStance Stance { get; protected set; }

	public float TimeSinceHitByCar { get; protected set; }

	public bool FaceDirectionInProgress => faceDirectionRoutine != null;

	public bool IsOnLadder => (Object)(object)CurrentLadder != (Object)null;

	public float CurrentLadderSpeed { get; protected set; }

	public bool IsClimbingUpwards => CurrentLadderSpeed > 0.1f;

	public Ladder CurrentLadder { get; protected set; }

	public Vector3 CurrentDestination { get; protected set; } = Vector3.zero;

	public NPCPathCache PathCache { get; private set; } = new NPCPathCache();

	public bool Disoriented { get; set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCMovement_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void Start()
	{
		string bakedGUID = npc.BakedGUID;
		if (bakedGUID != string.Empty)
		{
			bakedGUID = ((bakedGUID[bakedGUID.Length - 1] == '1') ? (bakedGUID.Substring(0, bakedGUID.Length - 1) + "2") : (bakedGUID.Substring(0, bakedGUID.Length - 1) + "1"));
			RagdollDraggable.SetGUID(new Guid(bakedGUID));
		}
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		if (!InstanceFinder.IsServer)
		{
			SetAgentEnabled(enabled: false);
			Agent.obstacleAvoidanceType = (ObstacleAvoidanceType)0;
		}
	}

	protected virtual void Update()
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		if (DEBUG)
		{
			Debug.Log((object)(npc.fullName + " movement debug: "));
			Debug.Log((object)("HasPath: " + Agent.hasPath));
			Debug.Log((object)("PathPending: " + Agent.pathPending));
			Debug.Log((object)("IsMoving: " + IsMoving));
			Debug.Log((object)("IsPaused: " + IsPaused));
			Debug.Log((object)("IsRagdolled: " + npc.Avatar.Ragdolled));
			Debug.Log((object)("IsInVehicle: " + npc.IsInVehicle));
			Debug.Log((object)("HasDestination: " + HasDestination));
			Debug.Log((object)("CurrentDestination: " + ((object)CurrentDestination/*cast due to .constrained prefix*/).ToString()));
			Debug.Log((object)("Movement Speed Scale: " + MovementSpeedScale));
		}
		if (!IsOnLadder && Agent.isOnOffMeshLink)
		{
			OffMeshLinkData currentOffMeshLinkData = Agent.currentOffMeshLinkData;
			TraverseLadder(((Component)((OffMeshLinkData)(ref currentOffMeshLinkData)).offMeshLink).GetComponent<Ladder>());
		}
	}

	public void SetAgentEnabled(bool enabled)
	{
		if (DEBUG)
		{
			Console.Log("Setting agent enabled: " + enabled + " for " + npc.fullName);
		}
		((Behaviour)Agent).enabled = enabled;
	}

	private void UpdateRagdoll()
	{
		if (npc.IsConscious && ragdollStaticTime > 1f && npc.Avatar.Ragdolled)
		{
			DeactivateRagdoll();
		}
	}

	private void Stumble()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		timeUntilNextStumble = Random.Range(5f, 15f);
		if (Random.Range(1f, 0f) < 0.1f)
		{
			ActivateRagdoll_Server();
			return;
		}
		timeSinceStumble = 0f;
		stumbleDirection = Random.onUnitSphere;
		stumbleDirection.y = 0f;
		((Vector3)(ref stumbleDirection)).Normalize();
	}

	private void UpdateDestination()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if (!HasDestination)
		{
			return;
		}
		if (npc.IsInVehicle)
		{
			EndSetDestination(WalkResult.Interrupted);
		}
		else
		{
			if (IsMoving || Agent.pathPending || !CanMove() || IsOnLadder)
			{
				return;
			}
			if (IsAsCloseAsPossible(CurrentDestination))
			{
				if (Agent.hasPath)
				{
					Agent.ResetPath();
					agentCurrentPath = null;
					agentCurrentPathCorners = null;
				}
				if (Vector3.Distance(CurrentDestination, FootPosition) < currentMaxDistanceForSuccess || Vector3.Distance(CurrentDestination, ((Component)this).transform.position) < currentMaxDistanceForSuccess)
				{
					EndSetDestination(WalkResult.Success);
				}
				else
				{
					EndSetDestination(WalkResult.Partial);
				}
			}
			else
			{
				SetDestination(CurrentDestination, walkResultCallback, interruptExistingCallback: false, currentMaxDistanceForSuccess);
			}
		}
	}

	protected virtual void FixedUpdate()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (IsPaused)
		{
			Agent.isStopped = true;
		}
		TimeSinceHitByCar += Time.fixedDeltaTime;
		UpdateSpeed();
		UpdateStumble();
		UpdateRagdoll();
		UpdateDestination();
		RecordVelocity();
		UpdateSlippery();
		UpdateCache();
		if (npc.Avatar.Ragdolled && CanRecoverFromRagdoll())
		{
			Vector3 velocity = npc.Avatar.MiddleSpineRB.velocity;
			if (((Vector3)(ref velocity)).magnitude < 0.15f)
			{
				ragdollStaticTime += Time.fixedDeltaTime;
			}
			else
			{
				ragdollStaticTime = 0f;
			}
		}
		else
		{
			ragdollStaticTime = 0f;
		}
	}

	private void UpdateStumble()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (IsOnLadder)
		{
			return;
		}
		if (Disoriented && IsMoving)
		{
			timeUntilNextStumble -= Time.fixedDeltaTime;
			if (timeUntilNextStumble <= 0f)
			{
				Stumble();
			}
		}
		timeSinceStumble += Time.fixedDeltaTime;
		if (timeSinceStumble < 0.66f)
		{
			Agent.Move(stumbleDirection * (0.66f - timeSinceStumble) * Time.fixedDeltaTime * 7f);
		}
	}

	private void UpdateSpeed()
	{
		float num = 0f;
		if ((double)MovementSpeedScale >= 0.0)
		{
			num = Mathf.Lerp(WalkSpeed, RunSpeed, MovementSpeedScale) * MoveSpeedMultiplier;
		}
		if (!Mathf.Approximately(num, agentCurrentSpeed))
		{
			Agent.speed = num;
			agentCurrentSpeed = num;
		}
	}

	private void RecordVelocity()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (timeSinceLastVelocityHistoryRecord > velocityHistorySpacing)
		{
			timeSinceLastVelocityHistoryRecord = 0f;
			desiredVelocityHistory.Enqueue(Agent.velocity);
		}
		else
		{
			timeSinceLastVelocityHistoryRecord += Time.fixedDeltaTime;
		}
	}

	private void UpdateSlippery()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (SlipperyMode && ((Behaviour)Agent).enabled && Agent.isOnNavMesh)
		{
			BurstFunctions.Average(ref desiredVelocityHistory.q, out var result);
			float num = Vector3.Angle(result, ((Component)this).transform.forward);
			Agent.Move(result * (SlipperyModeMultiplier * Time.fixedDeltaTime * Mathf.Clamp01(num / 90f)));
		}
	}

	private void UpdateCache()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (cacheNextPath && agentCurrentPath != null && agentCurrentPathCorners.Length > 1)
		{
			cacheNextPath = false;
			NPCPathCache pathCache = PathCache;
			Vector3 start = agentCurrentPathCorners[0];
			Vector3[] array = agentCurrentPathCorners;
			pathCache.AddPath(start, array[array.Length - 1], agentCurrentPath);
		}
	}

	public bool CanRecoverFromRagdoll()
	{
		if (npc.Behaviour.RagdollBehaviour.Seizure)
		{
			return false;
		}
		return true;
	}

	private void UpdateAvoidance()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (InstanceFinder.IsServer)
		{
			Player.GetClosestPlayer(((Component)this).transform.position, out var distance);
			if (distance > 25f || !ObstacleAvoidanceEnabled)
			{
				Agent.obstacleAvoidanceType = (ObstacleAvoidanceType)0;
			}
			else
			{
				Agent.obstacleAvoidanceType = DefaultObstacleAvoidanceType;
			}
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		CheckHit(other, (Collider)(object)CapsuleCollider, isCollision: false, ((Component)other).transform.position);
	}

	public void OnCollisionEnter(Collision collision)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		CheckHit(collision.collider, ((ContactPoint)(ref collision.contacts[0])).thisCollider, isCollision: true, ((ContactPoint)(ref collision.contacts[0])).point, collision);
	}

	private void CheckHit(Collider other, Collider thisCollider, bool isCollision, Vector3 hitPoint, Collision collision = null)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		if (npc.IgnoreImpacts)
		{
			return;
		}
		float distance;
		Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, out distance);
		if (distance > 30f)
		{
			return;
		}
		if (DEBUG)
		{
			Debug.Log((object)("NPCMovement.CheckHit: " + ((Object)((Component)other).gameObject).name + " hit by " + ((Object)((Component)thisCollider).gameObject).name));
		}
		LandVehicle landVehicle = null;
		if (((Component)other).gameObject.layer == LayerMask.NameToLayer("Vehicle") || ((Component)other).gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
		{
			landVehicle = ((Component)other).GetComponentInParent<LandVehicle>();
			if ((Object)(object)landVehicle == (Object)null)
			{
				VehicleHumanoidCollider componentInParent = ((Component)other).GetComponentInParent<VehicleHumanoidCollider>();
				if ((Object)(object)componentInParent != (Object)null)
				{
					landVehicle = componentInParent.Vehicle;
				}
			}
		}
		Vector3 velocity;
		if ((Object)(object)landVehicle != (Object)null)
		{
			if (!((Object)(object)landVehicle != (Object)null) || !((Object)(object)npc.CurrentVehicle != (Object)(object)landVehicle) || !(Mathf.Abs(landVehicle.Speed_Kmh) > 10f))
			{
				return;
			}
			if (!npc.Avatar.Ragdolled)
			{
				ActivateRagdoll_Server();
			}
			Vector3 val = npc.Avatar.MiddleSpineRB.velocity - landVehicle.VelocityCalculator.Velocity;
			if (!(((Vector3)(ref val)).sqrMagnitude > 7.71605f) || !(TimeSinceHitByCar > 0.25f))
			{
				return;
			}
			if (onHitByCar != null)
			{
				onHitByCar.Invoke(landVehicle);
			}
			if (DEBUG)
			{
				Debug.Log((object)("Hit with vehicle at relative velocity: " + ((Vector3)(ref val)).magnitude));
			}
			if (landVehicle.LocalPlayerIsDriver || ((Object)(object)landVehicle.DriverPlayer == (Object)null && InstanceFinder.IsServer))
			{
				NetworkObject impactSource = null;
				if ((Object)(object)landVehicle.DriverPlayer != (Object)null)
				{
					impactSource = ((NetworkBehaviour)landVehicle.DriverPlayer).NetworkObject;
				}
				float impactDamage = 120f * (Mathf.Abs(((Vector3)(ref val)).magnitude * 3.6f) / 100f);
				velocity = landVehicle.VelocityCalculator.Velocity;
				Vector3 impactForceDirection = ((Vector3)(ref velocity)).normalized + Vector3.up * 1f;
				Impact impact = new Impact(hitPoint, impactForceDirection, Mathf.Abs(landVehicle.Speed_Kmh) * 5f, impactDamage, EImpactType.BluntMetal, impactSource);
				npc.SendImpact(impact);
			}
			TimeSinceHitByCar = 0f;
		}
		else if ((Object)(object)((Component)other).GetComponentInParent<Skateboard>() != (Object)null)
		{
			if (!npc.Avatar.Ragdolled)
			{
				Skateboard componentInParent2 = ((Component)other).GetComponentInParent<Skateboard>();
				velocity = componentInParent2.VelocityCalculator.Velocity;
				if (((Vector3)(ref velocity)).magnitude > 2.777778f)
				{
					velocity = componentInParent2.VelocityCalculator.Velocity;
					ActivateRagdoll_Server(hitPoint, ((Vector3)(ref velocity)).normalized, componentInParent2.CurrentSpeed_Kmh * 4f);
					npc.PlayVO(EVOLineType.Hurt);
				}
			}
		}
		else
		{
			if (!InstanceFinder.IsServer || other.isTrigger || !((Object)(object)((Component)other).GetComponentInParent<PhysicsDamageable>() != (Object)null))
			{
				return;
			}
			PhysicsDamageable componentInParent3 = ((Component)other).GetComponentInParent<PhysicsDamageable>();
			float num = Mathf.Sqrt(componentInParent3.Rb.mass);
			velocity = componentInParent3.Rb.velocity;
			float num2 = num * ((Vector3)(ref velocity)).magnitude;
			velocity = componentInParent3.Rb.velocity;
			float magnitude = ((Vector3)(ref velocity)).magnitude;
			if (magnitude > 40f)
			{
				return;
			}
			if (magnitude > 1f)
			{
				magnitude = Mathf.Pow(magnitude, 1.5f);
			}
			else
			{
				magnitude = Mathf.Sqrt(magnitude);
			}
			if (num2 > 10f)
			{
				float num3 = 1f;
				switch (Stance)
				{
				case EStance.None:
					num3 = 1f;
					break;
				case EStance.Stanced:
					num3 = 0.67f;
					break;
				}
				float num4 = num2 * 2.5f;
				float num5 = num2 * 0.3f;
				if (num2 > 20f)
				{
					npc.Health.TakeDamage(num5, isLethal: false);
					NPC nPC = npc;
					velocity = componentInParent3.Rb.velocity;
					nPC.ProcessImpactForce(hitPoint, ((Vector3)(ref velocity)).normalized, num4 * num3);
				}
				velocity = componentInParent3.Rb.velocity;
				Impact impact2 = new Impact(hitPoint, ((Vector3)(ref velocity)).normalized, num4, num5, EImpactType.PhysicsProp, (distance < 15f) ? ((NetworkBehaviour)closestPlayer).NetworkObject : null, Random.Range(int.MinValue, int.MaxValue));
				npc.Responses.ImpactReceived(impact2);
			}
		}
	}

	public void Warp(Transform target)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Warp(target.position);
	}

	public unsafe void Warp(Vector3 position)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (!IsNPCPositionValid(position))
		{
			Vector3 val = position;
			Console.LogWarning("NPCMovement.Warp called with invalid position: " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString());
			return;
		}
		if (DEBUG)
		{
			string fullName = npc.fullName;
			Vector3 val = position;
			Console.Log("Warping " + fullName + " to position: " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString());
		}
		if (IsOnLadder)
		{
			CancelTraverseLadder();
		}
		if (((Behaviour)Agent).enabled)
		{
			Agent.Warp(position);
		}
		else
		{
			((Component)this).transform.position = position;
		}
		ReceiveWarp(position);
	}

	[ObserversRpc(ExcludeServer = true)]
	private void ReceiveWarp(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_ReceiveWarp_4276783012(position);
	}

	public void VisibilityChange(bool visible)
	{
		((Component)CapsuleCollider).gameObject.SetActive(visible);
	}

	public bool CanMove()
	{
		if (!npc.Avatar.Ragdolled && !npc.isInBuilding)
		{
			return !npc.IsInVehicle;
		}
		return false;
	}

	public void SetAgentType(EAgentType type)
	{
		string name = type.ToString();
		if (type == EAgentType.BigHumanoid)
		{
			name = "Big Humanoid";
		}
		if (type == EAgentType.IgnoreCosts)
		{
			name = "Ignore Costs";
		}
		Agent.agentTypeID = NavMeshUtility.GetNavMeshAgentID(name);
	}

	public void SetSeat(AvatarSeat seat)
	{
		npc.Avatar.Animation.SetSeat(seat);
		SetAgentEnabled((Object)(object)seat == (Object)null && InstanceFinder.IsServer);
	}

	public void SetStance(EStance stance)
	{
		Stance = stance;
	}

	public void SetGravityMultiplier(float multiplier)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		GravityMultiplier = multiplier;
		foreach (ConstantForce ragdollForceComponent in ragdollForceComponents)
		{
			ragdollForceComponent.force = Physics.gravity * GravityMultiplier * ((Component)ragdollForceComponent).GetComponent<Rigidbody>().mass;
		}
	}

	public void SetAngularSpeedMultiplier(float multiplier)
	{
		Agent.angularSpeed = _defaultAngularSpeed * multiplier;
	}

	public void SetRagdollDraggable(bool draggable)
	{
		if ((Object)(object)RagdollDraggable != (Object)null)
		{
			((Behaviour)RagdollDraggable).enabled = draggable;
		}
		if ((Object)(object)RagdollDraggableCollider != (Object)null)
		{
			RagdollDraggableCollider.enabled = draggable;
		}
	}

	public void ActivateRagdoll_Server()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		ActivateRagdoll_Server(Vector3.zero, Vector3.zero, 0f);
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void ActivateRagdoll_Server(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_ActivateRagdoll_Server_2690242654(forcePoint, forceDir, forceMagnitude);
		RpcLogic___ActivateRagdoll_Server_2690242654(forcePoint, forceDir, forceMagnitude);
	}

	[ObserversRpc(RunLocally = true)]
	public void ActivateRagdoll(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_ActivateRagdoll_2690242654(forcePoint, forceDir, forceMagnitude);
		RpcLogic___ActivateRagdoll_2690242654(forcePoint, forceDir, forceMagnitude);
	}

	[ObserversRpc(RunLocally = true)]
	public void ApplyRagdollForce(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_ApplyRagdollForce_2690242654(forcePoint, forceDir, forceMagnitude);
		RpcLogic___ApplyRagdollForce_2690242654(forcePoint, forceDir, forceMagnitude);
	}

	[ObserversRpc(RunLocally = true)]
	public void DeactivateRagdoll()
	{
		RpcWriter___Observers_DeactivateRagdoll_2166136261();
		RpcLogic___DeactivateRagdoll_2166136261();
	}

	private bool SmartSampleNavMesh(Vector3 position, out NavMeshHit hit, float minRadius = 1f, float maxRadius = 10f, int steps = 3)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		hit = default(NavMeshHit);
		NavMeshQueryFilter val = default(NavMeshQueryFilter);
		((NavMeshQueryFilter)(ref val)).agentTypeID = NavMeshUtility.GetNavMeshAgentID("Humanoid");
		((NavMeshQueryFilter)(ref val)).areaMask = -1;
		for (int i = 0; i < steps; i++)
		{
			float num = Mathf.Lerp(minRadius, maxRadius, (float)(i / steps));
			if (NavMesh.SamplePosition(((Component)this).transform.position, ref hit, num, val))
			{
				return true;
			}
		}
		return false;
	}

	public void SetDestination(Transform target)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		SetDestination(target.position);
	}

	public void SetDestination(Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		SetDestination(pos, null, 1f, 1f);
	}

	public void SetDestination(ITransitEntity entity)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		SetDestination(NavMeshUtility.GetReachableAccessPoint(entity, npc).position);
	}

	public void SetDestination(Vector3 pos, Action<WalkResult> callback = null, float maximumDistanceForSuccess = 1f, float cacheMaxDistSqr = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		SetDestination(pos, callback, interruptExistingCallback: true, maximumDistanceForSuccess, cacheMaxDistSqr);
	}

	private unsafe void SetDestination(Vector3 pos, Action<WalkResult> callback = null, bool interruptExistingCallback = true, float successThreshold = 1f, float cacheMaxDistSqr = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		if (!IsNPCPositionValid(pos))
		{
			Vector3 val = pos;
			Console.LogWarning("NPCMovement.SetDestination called with invalid position: " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString());
			return;
		}
		if (npc.Avatar.Animation.IsSeated)
		{
			npc.Movement.SetSeat(null);
		}
		if (!InstanceFinder.IsServer)
		{
			Console.LogWarning("NPCMovement.SetDestination called on client");
			return;
		}
		if (npc.isInBuilding)
		{
			npc.ExitBuilding();
		}
		if (DEBUG)
		{
			string fullName = npc.fullName;
			Vector3 val = pos;
			Console.Log(fullName + " SetDestination called: " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString());
			Debug.DrawLine(FootPosition, pos, Color.green, 1f);
		}
		if (!CanMove())
		{
			Console.LogWarning("NPCMovement.SetDestination called but CanWalk == false (" + npc.fullName + ")");
			return;
		}
		if (!Agent.isOnNavMesh && !IsOnLadder)
		{
			Console.LogWarning("NPC is not on navmesh; warping to navmesh");
			if (!SmartSampleNavMesh(((Component)this).transform.position, out var hit))
			{
				Console.LogWarning("NavMesh sample failed at " + ((object)((Component)this).transform.position/*cast due to .constrained prefix*/).ToString());
				return;
			}
			Warp(((NavMeshHit)(ref hit)).position);
			SetAgentEnabled(enabled: false);
			SetAgentEnabled(enabled: true);
		}
		if (walkResultCallback != null && interruptExistingCallback)
		{
			EndSetDestination(WalkResult.Interrupted);
		}
		walkResultCallback = callback;
		currentMaxDistanceForSuccess = successThreshold;
		if (npc.IsInVehicle)
		{
			Console.LogWarning("SetDestination called but NPC is in a vehicle; returning WalkResult.Failed");
			EndSetDestination(WalkResult.Failed);
			return;
		}
		if (!GetClosestReachablePoint(pos, out var closestPoint))
		{
			if (DEBUG)
			{
				string fullName2 = npc.fullName;
				Vector3 val = pos;
				Console.LogWarning(fullName2 + " failed to find closest reachable point for destination: " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString());
				Debug.DrawLine(FootPosition, pos, Color.red, 1f);
			}
			EndSetDestination(WalkResult.Failed);
			return;
		}
		if (!IsNPCPositionValid(closestPoint))
		{
			string fullName3 = npc.fullName;
			Vector3 val = pos;
			Console.LogWarning(fullName3 + " failed to find valid reachable point for destination: " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString());
			EndSetDestination(WalkResult.Failed);
			return;
		}
		HasDestination = true;
		CurrentDestination = pos;
		currentDestination_Reachable = closestPoint;
		if (IsOnLadder)
		{
			return;
		}
		NavMeshPath path = PathCache.GetPath(((Component)Agent).transform.position, closestPoint, cacheMaxDistSqr);
		bool flag = false;
		if (path != null)
		{
			try
			{
				flag = path == agentCurrentPath || Agent.SetPath(path);
			}
			catch (Exception ex)
			{
				Console.LogWarning("Agent.SetDestination error: " + ex.Message);
				flag = false;
			}
		}
		if (!flag)
		{
			if (DEBUG)
			{
				Console.Log("No cached path for " + npc.fullName + "; calculating new path");
			}
			try
			{
				if (!Agent.SetDestination(closestPoint))
				{
					Console.LogError("Agent.SetDestination returned false for " + npc.fullName);
				}
				cacheNextPath = true;
			}
			catch (Exception ex2)
			{
				Console.LogWarning("Agent.SetDestination error: " + ex2.Message);
			}
		}
		agentCurrentPath = Agent.path;
		agentCurrentPathCorners = agentCurrentPath.corners;
		if (IsPaused)
		{
			Agent.isStopped = true;
		}
	}

	private bool IsNPCPositionValid(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
		{
			return false;
		}
		if (float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z))
		{
			return false;
		}
		if (((Vector3)(ref position)).magnitude > 10000f)
		{
			return false;
		}
		return true;
	}

	private void EndSetDestination(WalkResult result)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (DEBUG)
		{
			Console.Log(npc.fullName + " EndSetDestination called: " + result);
		}
		if (walkResultCallback != null)
		{
			walkResultCallback(result);
			walkResultCallback = null;
		}
		HasDestination = false;
		CurrentDestination = Vector3.zero;
		currentDestination_Reachable = Vector3.zero;
	}

	public void Stop()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (Agent.isOnNavMesh)
		{
			Agent.ResetPath();
			Agent.velocity = Vector3.zero;
			Agent.isStopped = true;
			Agent.isStopped = false;
			agentCurrentPath = null;
			agentCurrentPathCorners = null;
		}
		if (InstanceFinder.IsServer)
		{
			EndSetDestination(WalkResult.Stopped);
		}
	}

	public void WarpToNavMesh()
	{
	}

	public unsafe void FacePoint(Vector3 point, float lerpTime = 0.5f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = new Vector3(point.x, ((Component)this).transform.position.y, point.z) - ((Component)this).transform.position;
		if (faceDirectionRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(faceDirectionRoutine);
		}
		if (DEBUG)
		{
			Vector3 val = point;
			Debug.Log((object)("Facing point: " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString()));
		}
		faceDirectionRoutine = ((MonoBehaviour)this).StartCoroutine(FaceDirection_Process(forward, lerpTime));
	}

	public unsafe void FaceDirection(Vector3 forward, float lerpTime = 0.5f)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (faceDirectionRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(faceDirectionRoutine);
		}
		if (DEBUG)
		{
			Vector3 val = forward;
			Debug.Log((object)("Facing dir: " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString()));
		}
		faceDirectionRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(FaceDirection_Process(forward, lerpTime));
	}

	protected IEnumerator FaceDirection_Process(Vector3 forward, float lerpTime)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (lerpTime > 0f)
		{
			Quaternion startRot = ((Component)this).transform.rotation;
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				if (!IsOnLadder)
				{
					((Component)this).transform.rotation = Quaternion.Lerp(startRot, Quaternion.LookRotation(forward, Vector3.up), i / lerpTime);
				}
				yield return (object)new WaitForEndOfFrame();
			}
		}
		((Component)this).transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
		faceDirectionRoutine = null;
	}

	public void PauseMovement()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		IsPaused = true;
		Agent.isStopped = true;
		Agent.velocity = Vector3.zero;
	}

	public void ResumeMovement()
	{
		IsPaused = false;
		if (Agent.isOnNavMesh)
		{
			Agent.isStopped = false;
		}
	}

	public bool IsAsCloseAsPossible(Vector3 location, float distanceThreshold = 0.5f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Distance(FootPosition, location) < distanceThreshold)
		{
			return true;
		}
		Vector3 closestPoint = Vector3.zero;
		if (!GetClosestReachablePoint(location, out closestPoint))
		{
			return false;
		}
		return Vector3.Distance(FootPosition, closestPoint) < distanceThreshold;
	}

	public bool GetClosestReachablePoint(Vector3 targetPosition, out Vector3 closestPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		closestPoint = Vector3.zero;
		bool flag = false;
		Vector3 val = Vector3.zero;
		for (int i = 0; i < cachedClosestPointKeys.Count; i++)
		{
			if (Vector3.SqrMagnitude(cachedClosestPointKeys[i] - targetPosition) < 1f)
			{
				val = cachedClosestReachablePoints[cachedClosestPointKeys[i]];
				flag = true;
				break;
			}
		}
		if (flag)
		{
			closestPoint = val;
			return true;
		}
		if (!Agent.isOnNavMesh && !IsOnLadder)
		{
			return false;
		}
		NavMeshQueryFilter val2 = default(NavMeshQueryFilter);
		((NavMeshQueryFilter)(ref val2)).agentTypeID = Agent.agentTypeID;
		((NavMeshQueryFilter)(ref val2)).areaMask = Agent.areaMask;
		NavMeshPath val3 = new NavMeshPath();
		float num = 3f;
		for (int j = 0; j < 3; j++)
		{
			if (NavMeshUtility.SamplePosition(targetPosition, out var hit, num * (float)(j + 1), -1))
			{
				if (DEBUG)
				{
					Console.Log("Hit!");
				}
				Vector3 val4 = ((Component)Agent).transform.position;
				if (IsOnLadder)
				{
					val4 = ((!IsClimbingUpwards) ? CurrentLadder.OffMeshLink.startTransform.position : CurrentLadder.OffMeshLink.endTransform.position);
				}
				NavMesh.CalculatePath(val4, ((NavMeshHit)(ref hit)).position, val2, val3);
				if (val3 != null && val3.corners.Length != 0)
				{
					Vector3 val5 = val3.corners[val3.corners.Length - 1];
					closestPoint = val5;
					return true;
				}
			}
		}
		return false;
	}

	public bool CanGetTo(Vector3 position, float proximityReq = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		NavMeshPath path;
		return CanGetTo(position, proximityReq, out path);
	}

	public bool CanGetTo(ITransitEntity entity, float proximityReq = 1f)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (entity == null)
		{
			return false;
		}
		Transform[] accessPoints = entity.AccessPoints;
		foreach (Transform val in accessPoints)
		{
			if (!((Object)(object)val == (Object)null) && CanGetTo(val.position, proximityReq))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanGetTo(Vector3 position, float proximityReq, out NavMeshPath path)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		path = null;
		if (Vector3.Distance(position, ((Component)this).transform.position) <= proximityReq)
		{
			return true;
		}
		if (!Agent.isOnNavMesh)
		{
			return false;
		}
		if (!NavMeshUtility.SamplePosition(position, out var hit, 2f, -1))
		{
			return false;
		}
		path = GetPathTo(((NavMeshHit)(ref hit)).position, proximityReq);
		if (path == null)
		{
			return false;
		}
		if (path.corners.Length < 2)
		{
			return false;
		}
		float num = Vector3.Distance(path.corners[path.corners.Length - 1], ((NavMeshHit)(ref hit)).position);
		float num2 = Vector3.Distance(((NavMeshHit)(ref hit)).position, position);
		if (num <= proximityReq)
		{
			return num2 <= proximityReq;
		}
		return false;
	}

	private NavMeshPath GetPathTo(Vector3 position, float proximityReq = 1f)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (!Agent.isOnNavMesh)
		{
			Console.LogWarning("Agent not on nav mesh!");
			return null;
		}
		NavMeshPath val = new NavMeshPath();
		NavMeshUtility.SamplePosition(position, out var hit, 2f, -1);
		if (!Agent.CalculatePath(((NavMeshHit)(ref hit)).position, val))
		{
			return null;
		}
		float num = Vector3.Distance(val.corners[val.corners.Length - 1], ((NavMeshHit)(ref hit)).position);
		float num2 = Vector3.Distance(((NavMeshHit)(ref hit)).position, position);
		if (num <= proximityReq && num2 <= proximityReq)
		{
			return val;
		}
		return null;
	}

	public void TraverseLadder(Ladder ladder)
	{
		CurrentLadder = ladder;
		SetAgentEnabled(enabled: false);
		if (ladderClimbRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(ladderClimbRoutine);
			ladderClimbRoutine = null;
		}
		ladderClimbRoutine = ((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator OverrideLookDirection()
		{
			while ((Object)(object)CurrentLadder != (Object)null)
			{
				npc.Avatar.LookController.BlockLookTargetOverrides();
				yield return (object)new WaitForEndOfFrame();
			}
		}
		IEnumerator Routine()
		{
			bool startFromTop = false;
			if (Vector3.Distance(FootPosition, ladder.TopCenter) < Vector3.Distance(FootPosition, ladder.BottomCenter))
			{
				startFromTop = true;
			}
			Vector3 startLadderPos = (startFromTop ? (ladder.TopCenter - ladder.LadderTransform.up * 0.25f) : ladder.BottomCenter) - ladder.LadderTransform.forward * 0.42f;
			Vector3 endLadderPos = (startFromTop ? ladder.BottomCenter : (ladder.TopCenter - ladder.LadderTransform.up * 1f)) - ladder.LadderTransform.forward * 0.42f;
			Quaternion ladderRot = ladder.LadderTransform.rotation;
			Vector3 startNPCPos = ((Component)this).transform.position;
			Quaternion startNPCRot = ((Component)this).transform.rotation;
			Vector3 endNPCPos = (startFromTop ? CurrentLadder.OffMeshLink.startTransform.position : CurrentLadder.OffMeshLink.endTransform.position);
			Quaternion endPlayerRot = (startFromTop ? CurrentLadder.OffMeshLink.startTransform.rotation : CurrentLadder.OffMeshLink.endTransform.rotation);
			float mountLerpTime = Mathf.Max(Vector3.Distance(startNPCPos, startLadderPos) * 0.4f, startFromTop ? 0.7f : 0.3f);
			float climbLerpTime = Vector3.Distance(startLadderPos, endLadderPos) * 0.75f;
			float dismountLerpTime = Mathf.Max(Vector3.Distance(endLadderPos, endNPCPos) * 0.4f, startFromTop ? 0.4f : 0.3f);
			if (!startFromTop)
			{
				CurrentLadderSpeed = 1f;
			}
			((MonoBehaviour)this).StartCoroutine(OverrideLookDirection());
			if (startFromTop && (Object)(object)ladder.LinkedManholeCover != (Object)null && InstanceFinder.IsServer && !ladder.LinkedManholeCover.IsOpen)
			{
				ladder.LinkedManholeCover.SetIsOpen_Server(open: true, EDoorSide.Exterior, openedForPlayer: false);
			}
			for (float t = 0f; t < mountLerpTime; t += Time.deltaTime)
			{
				Vector3 position = Vector3.Lerp(startNPCPos, startLadderPos, t / mountLerpTime);
				Quaternion rotation = Quaternion.Slerp(startNPCRot, ladderRot, t / mountLerpTime);
				((Component)this).transform.position = position;
				((Component)this).transform.rotation = rotation;
				yield return (object)new WaitForEndOfFrame();
			}
			if (startFromTop)
			{
				CurrentLadderSpeed = -1f;
			}
			float timeUntilClimbSoundPlays = 0.3f;
			for (float t = 0f; t < climbLerpTime; t += Time.deltaTime)
			{
				Vector3 position2 = Vector3.Lerp(startLadderPos, endLadderPos, t / climbLerpTime);
				((Component)this).transform.position = position2;
				timeUntilClimbSoundPlays -= Time.deltaTime;
				if (timeUntilClimbSoundPlays <= 0f)
				{
					ladder.PlayClimbSound(npc.CenterPoint);
					timeUntilClimbSoundPlays = 0.3f;
				}
				if (!startFromTop && (Object)(object)ladder.LinkedManholeCover != (Object)null && InstanceFinder.IsServer)
				{
					float num = Vector3.Distance(npc.CenterPoint, ladder.TopCenter);
					if (!ladder.LinkedManholeCover.IsOpen && num < 1.1f)
					{
						ladder.LinkedManholeCover.SetIsOpen_Server(open: true, EDoorSide.Interior, openedForPlayer: false);
					}
				}
				yield return (object)new WaitForEndOfFrame();
			}
			CurrentLadderSpeed = 0f;
			for (float t = 0f; t < dismountLerpTime; t += Time.deltaTime)
			{
				Vector3 position3 = Vector3.Lerp(endLadderPos, endNPCPos, t / dismountLerpTime);
				Quaternion rotation2 = Quaternion.Slerp(ladderRot, endPlayerRot, t / dismountLerpTime);
				((Component)this).transform.position = position3;
				((Component)this).transform.rotation = rotation2;
				yield return (object)new WaitForEndOfFrame();
			}
			SetAgentEnabled(enabled: true);
			CurrentLadder = null;
			Agent.CompleteOffMeshLink();
		}
	}

	private void CancelTraverseLadder()
	{
		if (ladderClimbRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(ladderClimbRoutine);
			ladderClimbRoutine = null;
		}
		CurrentLadderSpeed = 0f;
		CurrentLadder = null;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCMovementAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCMovementAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_ReceiveWarp_4276783012));
			((NetworkBehaviour)this).RegisterServerRpc(1u, new ServerRpcDelegate(RpcReader___Server_ActivateRagdoll_Server_2690242654));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_ActivateRagdoll_2690242654));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_ApplyRagdollForce_2690242654));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_DeactivateRagdoll_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCMovementAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCMovementAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_ReceiveWarp_4276783012(Vector3 position)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteVector3(position);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, true, false);
			writer.Store();
		}
	}

	private unsafe void RpcLogic___ReceiveWarp_4276783012(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (!IsNPCPositionValid(position))
		{
			Vector3 val = position;
			Console.LogWarning("NPCMovement.Warp called with invalid position: " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString());
			return;
		}
		if (DEBUG)
		{
			string fullName = npc.fullName;
			Vector3 val = position;
			Console.Log("Warping " + fullName + " to position: " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString());
		}
		if (((Behaviour)Agent).enabled)
		{
			Agent.Warp(position);
		}
		else
		{
			((Component)this).transform.position = position;
		}
	}

	private void RpcReader___Observers_ReceiveWarp_4276783012(PooledReader PooledReader0, Channel channel)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveWarp_4276783012(position);
		}
	}

	private void RpcWriter___Server_ActivateRagdoll_Server_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(forcePoint);
			((Writer)writer).WriteVector3(forceDir);
			((Writer)writer).WriteSingle(forceMagnitude, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(1u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___ActivateRagdoll_Server_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		ActivateRagdoll(forcePoint, forceDir, forceMagnitude);
	}

	private void RpcReader___Server_ActivateRagdoll_Server_2690242654(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forcePoint = ((Reader)PooledReader0).ReadVector3();
		Vector3 forceDir = ((Reader)PooledReader0).ReadVector3();
		float forceMagnitude = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___ActivateRagdoll_Server_2690242654(forcePoint, forceDir, forceMagnitude);
		}
	}

	private void RpcWriter___Observers_ActivateRagdoll_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteVector3(forcePoint);
			((Writer)writer).WriteVector3(forceDir);
			((Writer)writer).WriteSingle(forceMagnitude, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public unsafe void RpcLogic___ActivateRagdoll_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if (DEBUG)
		{
			string[] obj = new string[8] { "Activating ragdoll for ", npc.fullName, " with forcePoint: ", null, null, null, null, null };
			Vector3 val = forcePoint;
			obj[3] = ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString();
			obj[4] = ", forceDir: ";
			val = forceDir;
			obj[5] = ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString();
			obj[6] = ", forceMagnitude: ";
			obj[7] = forceMagnitude.ToString();
			Console.Log(string.Concat(obj));
		}
		if (IsOnLadder)
		{
			CancelTraverseLadder();
		}
		Animation.SetRagdollActive(active: true);
		if (onRagdollStart != null)
		{
			onRagdollStart.Invoke();
		}
		if (InstanceFinder.IsServer)
		{
			EndSetDestination(WalkResult.Interrupted);
			SetAgentEnabled(enabled: false);
		}
		((Component)CapsuleCollider).gameObject.SetActive(false);
		if (forceMagnitude > 0f)
		{
			ApplyRagdollForce(forcePoint, forceDir, forceMagnitude);
		}
	}

	private void RpcReader___Observers_ActivateRagdoll_2690242654(PooledReader PooledReader0, Channel channel)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forcePoint = ((Reader)PooledReader0).ReadVector3();
		Vector3 forceDir = ((Reader)PooledReader0).ReadVector3();
		float forceMagnitude = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ActivateRagdoll_2690242654(forcePoint, forceDir, forceMagnitude);
		}
	}

	private void RpcWriter___Observers_ApplyRagdollForce_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteVector3(forcePoint);
			((Writer)writer).WriteVector3(forceDir);
			((Writer)writer).WriteSingle(forceMagnitude, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___ApplyRagdollForce_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		npc.Avatar.ImpactForceRBs.OrderBy((Rigidbody rb) => Vector3.Distance(rb.worldCenterOfMass, forcePoint)).FirstOrDefault().AddForceAtPosition(((Vector3)(ref forceDir)).normalized * forceMagnitude, forcePoint, (ForceMode)1);
	}

	private void RpcReader___Observers_ApplyRagdollForce_2690242654(PooledReader PooledReader0, Channel channel)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forcePoint = ((Reader)PooledReader0).ReadVector3();
		Vector3 forceDir = ((Reader)PooledReader0).ReadVector3();
		float forceMagnitude = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ApplyRagdollForce_2690242654(forcePoint, forceDir, forceMagnitude);
		}
	}

	private void RpcWriter___Observers_DeactivateRagdoll_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___DeactivateRagdoll_2166136261()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		((Component)CapsuleCollider).gameObject.SetActive(npc.isVisible);
		Animation.SetRagdollActive(active: false);
		((Component)this).transform.position = ((Component)npc.Avatar).transform.position;
		((Component)this).transform.rotation = ((Component)npc.Avatar).transform.rotation;
		((Component)npc.Avatar).transform.localPosition = Vector3.zero;
		((Component)npc.Avatar).transform.localRotation = Quaternion.identity;
		VelocityCalculator.FlushBuffer();
		if (InstanceFinder.IsServer)
		{
			SetAgentEnabled(enabled: false);
			if (!Agent.isOnNavMesh)
			{
				NavMeshQueryFilter val = default(NavMeshQueryFilter);
				((NavMeshQueryFilter)(ref val)).agentTypeID = NavMeshUtility.GetNavMeshAgentID("Humanoid");
				((NavMeshQueryFilter)(ref val)).areaMask = -1;
				if (SmartSampleNavMesh(((Component)this).transform.position, out var hit))
				{
					Warp(((NavMeshHit)(ref hit)).position);
				}
				SetAgentEnabled(enabled: false);
				SetAgentEnabled(enabled: true);
			}
		}
		if (onRagdollEnd != null)
		{
			onRagdollEnd.Invoke();
		}
	}

	private void RpcReader___Observers_DeactivateRagdoll_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___DeactivateRagdoll_2166136261();
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCMovement_Assembly_002DCSharp_002Edll()
	{
		npc = ((Component)this).GetComponent<NPC>();
		NPC nPC = npc;
		nPC.onVisibilityChanged = (Action<bool>)Delegate.Combine(nPC.onVisibilityChanged, new Action<bool>(VisibilityChange));
		VisibilityChange(npc.isVisible);
		((MonoBehaviour)this).InvokeRepeating("UpdateAvoidance", 0f, 0.5f);
		for (int i = 0; i < npc.Avatar.RagdollRBs.Length; i++)
		{
			ragdollForceComponents.Add(((Component)npc.Avatar.RagdollRBs[i]).gameObject.AddComponent<ConstantForce>());
		}
		desiredVelocityHistory = new CircularQueue<Vector3>(desiredVelocityHistoryLength);
		_defaultAngularSpeed = Agent.angularSpeed;
		SetRagdollDraggable(draggable: false);
		SetGravityMultiplier(1f);
	}
}
