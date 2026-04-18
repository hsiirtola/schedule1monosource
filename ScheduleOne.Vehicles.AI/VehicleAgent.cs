using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using Pathfinding;
using ScheduleOne.DevUtilities;
using ScheduleOne.Math;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vehicles.AI;

[RequireComponent(typeof(LandVehicle))]
public class VehicleAgent : MonoBehaviour
{
	public enum ENavigationResult
	{
		Failed,
		Complete,
		Stopped
	}

	public enum EAgentStatus
	{
		Inactive,
		MovingToRoad,
		OnRoad
	}

	public enum EPathGroupStatus
	{
		Inactive,
		Calculating
	}

	public enum ESweepType
	{
		FL,
		FR,
		RL,
		RR
	}

	public delegate void NavigationCallback(ENavigationResult status);

	public const string VehicleGraphName = "General Vehicle Graph";

	public const string RoadGraphName = "Road Nodes";

	public const float MaxDistanceFromPath = 6f;

	public const float MaxDistanceFromPathWhenReversing = 8f;

	public static Vector3 MainGraphSamplePoint = new Vector3(31.5f, 0f, 51f);

	public static float MinRenavigationRate = 2f;

	public const float Steer_P = 40f;

	public const float Steer_I = 5f;

	public const float Steer_D = 10f;

	public const float Throttle_P = 0.08f;

	public const float Throttle_I = 0f;

	public const float Throttle_D = 0f;

	public const float Steer_Rate = 135f;

	public const float MaxAxlePositionShift = 3f;

	public const float OBSTACLE_MIN_RANGE = 1.5f;

	public const float OBSTACLE_MAX_RANGE = 15f;

	public const float MAX_STEER_ANGLE_OVERRIDE = 35f;

	public const float INFREQUENT_UPDATE_RATE = 0.033f;

	public bool DEBUG_MODE;

	public DriveFlags Flags;

	[Header("Seekers")]
	[SerializeField]
	protected Seeker roadSeeker;

	[SerializeField]
	protected Seeker generalSeeker;

	[Header("References")]
	[SerializeField]
	protected Transform CTE_Origin;

	[SerializeField]
	protected Transform FrontAxlePosition;

	[SerializeField]
	protected Transform RearAxlePosition;

	[Header("Sensors")]
	[SerializeField]
	protected Sensor sensor_FL;

	[SerializeField]
	protected Sensor sensor_FM;

	[SerializeField]
	protected Sensor sensor_FR;

	[SerializeField]
	protected Sensor sensor_RR;

	[SerializeField]
	protected Sensor sensor_RL;

	private Sensor[] sensors;

	[Header("Sweeping")]
	[SerializeField]
	protected LayerMask sweepMask;

	[SerializeField]
	protected Transform sweepOrigin_FL;

	[SerializeField]
	protected Transform sweepOrigin_FR;

	[SerializeField]
	protected Transform sweepOrigin_RL;

	[SerializeField]
	protected Transform sweepOrigin_RR;

	[SerializeField]
	protected Wheel leftWheel;

	[SerializeField]
	protected Wheel rightWheel;

	protected const float sweepSegment = 15f;

	[Header("Path following")]
	protected float sampleStepSizeMin = 2f;

	protected float sampleStepSizeMax = 6f;

	protected int aheadPointSamples = 4;

	protected const float DestinationDistanceSlowThreshold = 8f;

	protected const float DestinationArrivalThreshold = 3f;

	[Header("Steer settings")]
	[SerializeField]
	protected float steerTargetFollowRate = 2f;

	private SteerPID steerPID;

	[Header("Turning speed reduction")]
	protected float turnSpeedReductionMinRange = 2f;

	protected float turnSpeedReductionMaxRange = 10f;

	protected float turnSpeedReductionDivisor = 90f;

	private float minTurnSpeedReductionAngleThreshold = 15f;

	private float minTurningSpeed = 10f;

	[Header("Throttle")]
	[SerializeField]
	protected float throttleMin = -1f;

	[SerializeField]
	protected float throttleMax = 1f;

	private PID throttlePID;

	public static float UnmarkedSpeed = 25f;

	public static float ReverseSpeed = 5f;

	private ValueTracker speedReductionTracker;

	[Header("Pursuit Mode")]
	public bool PursuitModeEnabled;

	public Transform PursuitTarget;

	public float PursuitDistanceUpdateThreshold = 5f;

	private Vector3 PursuitTargetLastPosition = Vector3.zero;

	[Header("Stuck Detection")]
	public VehicleTeleporter Teleporter;

	public PositionHistoryTracker PositionHistoryTracker;

	public float StuckTimeThreshold = 10f;

	public int StuckSamples = 4;

	public float StuckDistanceThreshold = 1f;

	protected NavigationCallback storedNavigationCallback;

	protected SpeedZone currentSpeedZone;

	protected LandVehicle vehicle;

	protected float wheelbase;

	protected float wheeltrack;

	protected float vehicleLength;

	protected float vehicleWidth;

	protected float turnRadius;

	protected float sweepTrack;

	private float wheelBottomOffset;

	[Header("Control info - READONLY")]
	[SerializeField]
	protected float targetSpeed;

	[SerializeField]
	protected float targetSteerAngle_Normalized;

	protected float lateralOffset;

	protected PathSmoothingUtility.SmoothedPath path;

	private float timeOnLastNavigationCall;

	private float sweepTestFailedTime;

	private NavigationSettings currentNavigationSettings;

	private Coroutine navigationCalculationRoutine;

	private Coroutine reverseCoroutine;

	public bool AutoDriving { get; protected set; }

	public bool KinematicMode => vehicle.Rb.isKinematic;

	public bool IsReversing => reverseCoroutine != null;

	public Vector3 TargetLocation { get; protected set; } = Vector3.zero;

	protected float sampleStepSize => Mathf.Lerp(sampleStepSizeMin, sampleStepSizeMax, Mathf.Clamp01(vehicle.Speed_Kmh / vehicle.TopSpeed));

	protected float turnSpeedReductionRange => Mathf.Lerp(turnSpeedReductionMinRange, turnSpeedReductionMaxRange, Mathf.Clamp(vehicle.Speed_Kmh / vehicle.TopSpeed, 0f, 1f));

	protected float maxSteerAngle => vehicle.ActualMaxSteeringAngle;

	private Vector3 frontOfVehiclePosition => ((Component)this).transform.position + ((Component)this).transform.forward * vehicleLength / 2f;

	public bool NavigationCalculationInProgress => navigationCalculationRoutine != null;

	private float timeSinceLastNavigationCall => Time.timeSinceLevelLoad - timeOnLastNavigationCall;

	private void Awake()
	{
		vehicle = ((Component)this).GetComponent<LandVehicle>();
		throttlePID = new PID(0.08f, 0f, 0f);
		steerPID = new SteerPID();
		speedReductionTracker = new ValueTracker(10f);
		PositionHistoryTracker.historyDuration = StuckTimeThreshold;
		sensors = new Sensor[5] { sensor_FL, sensor_FM, sensor_FR, sensor_RR, sensor_RL };
	}

	protected virtual void Start()
	{
		((MonoBehaviour)this).InvokeRepeating("RefreshSpeedZone", 0f, 0.25f);
		((MonoBehaviour)this).InvokeRepeating("UpdateStuckDetection", 1f, 1f);
		((MonoBehaviour)this).InvokeRepeating("InfrequentUpdate", 0f, 0.033f);
		InitializeVehicleData();
	}

	private void InitializeVehicleData()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		vehicleLength = ((Component)vehicle.boundingBox).transform.localScale.z;
		vehicleWidth = ((Component)vehicle.boundingBox).transform.localScale.x;
		Transform val = null;
		Transform val2 = null;
		Transform val3 = null;
		Transform val4 = null;
		foreach (Wheel wheel in vehicle.wheels)
		{
			if ((Object)(object)val == (Object)null || ((Component)vehicle).transform.InverseTransformPoint(((Component)wheel).transform.position).z > ((Component)vehicle).transform.InverseTransformPoint(val.position).z)
			{
				val = ((Component)wheel).transform;
			}
			if ((Object)(object)val2 == (Object)null || ((Component)vehicle).transform.InverseTransformPoint(((Component)wheel).transform.position).z < ((Component)vehicle).transform.InverseTransformPoint(val2.position).z)
			{
				val2 = ((Component)wheel).transform;
			}
			if ((Object)(object)val4 == (Object)null || ((Component)vehicle).transform.InverseTransformPoint(((Component)wheel).transform.position).x > ((Component)vehicle).transform.InverseTransformPoint(val4.position).x)
			{
				val4 = ((Component)wheel).transform;
			}
			if ((Object)(object)val3 == (Object)null || ((Component)vehicle).transform.InverseTransformPoint(((Component)wheel).transform.position).x < ((Component)vehicle).transform.InverseTransformPoint(val3.position).x)
			{
				val3 = ((Component)wheel).transform;
			}
		}
		wheelbase = ((Component)vehicle).transform.InverseTransformPoint(val.position).z - ((Component)vehicle).transform.InverseTransformPoint(val2.position).z;
		wheeltrack = ((Component)vehicle).transform.InverseTransformPoint(val4.position).x - ((Component)vehicle).transform.InverseTransformPoint(val3.position).x;
		sweepTrack = sweepOrigin_FR.localPosition.x - sweepOrigin_FL.localPosition.x;
		wheelBottomOffset = 0f - ((Component)this).transform.InverseTransformPoint(((Component)leftWheel).transform.position).y + leftWheel.wheelCollider.radius;
		turnRadius = wheelbase / Mathf.Sin(maxSteerAngle * ((float)System.Math.PI / 180f)) + 1.35f;
	}

	protected virtual void FixedUpdate()
	{
		if (Time.timeScale != 0f)
		{
			_ = AutoDriving;
		}
	}

	protected void InfrequentUpdate()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		UpdatePursuitMode();
		if (AutoDriving)
		{
			CheckDistanceFromPath();
			UpdateOvertaking();
			if (reverseCoroutine == null)
			{
				UpdateSpeed();
				UpdateSteering();
				UpdateSweep();
				UpdateSpeedReduction();
			}
			if (KinematicMode)
			{
				UpdateKinematic(0.033f);
			}
		}
	}

	protected void LateUpdate()
	{
		if (AutoDriving && Time.timeScale != 0f)
		{
			if (DEBUG_MODE)
			{
				Debug.Log((object)("Target speed: " + targetSpeed));
			}
			throttlePID.pFactor = 0.08f;
			throttlePID.iFactor = 0f;
			throttlePID.dFactor = 0f;
			float num = throttlePID.Update(targetSpeed, vehicle.Speed_Kmh, Time.deltaTime);
			float num2 = 0.01f;
			if (Mathf.Abs(num) < num2)
			{
				num = 0f;
			}
			vehicle.throttleOverride = Mathf.Clamp(num, throttleMin, throttleMax);
			vehicle.steerOverride = Mathf.Lerp(vehicle.steerOverride, targetSteerAngle_Normalized, Time.deltaTime * steerTargetFollowRate);
		}
	}

	protected void UpdateKinematic(float deltaTime)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		if (!AutoDriving || path == null)
		{
			return;
		}
		float distance = targetSpeed * 0.2f * deltaTime;
		Vector3 referencePoint = ((Component)vehicle.boundingBox).transform.position - ((Component)vehicle.boundingBox).transform.up * vehicle.BoundingBoxDimensions.y * 0.5f;
		Vector3 aheadPoint = PathUtility.GetAheadPoint(path, referencePoint, distance);
		if (DEBUG_MODE)
		{
			Debug.DrawLine(((Component)this).transform.position, aheadPoint, Color.red, 0.5f);
		}
		if (aheadPoint == Vector3.zero)
		{
			return;
		}
		((Component)this).transform.position = aheadPoint;
		int startPointIndex;
		int endPointIndex;
		float pointLerp;
		Vector3 val = PathUtility.GetClosestPointOnPath(path, ((Component)this).transform.position, out startPointIndex, out endPointIndex, out pointLerp);
		Vector3 val2 = val + Vector3.up * 2f;
		LayerMask val3 = LayerMask.op_Implicit(LayerMask.GetMask(new string[1] { "Default" }));
		val3 = LayerMask.op_Implicit(LayerMask.op_Implicit(val3) | LayerMask.GetMask(new string[1] { "Terrain" }));
		RaycastHit[] source = Physics.RaycastAll(val2, Vector3.down, 3f, LayerMask.op_Implicit(val3), (QueryTriggerInteraction)1);
		source = source.OrderBy((RaycastHit h) => ((RaycastHit)(ref h)).distance).ToArray();
		bool flag = false;
		RaycastHit val4 = default(RaycastHit);
		for (int num = 0; num < source.Length; num++)
		{
			if (!((Component)((RaycastHit)(ref source[num])).collider).transform.IsChildOf(((Component)this).transform))
			{
				val4 = source[num];
				flag = true;
				break;
			}
		}
		if (flag)
		{
			val = ((RaycastHit)(ref val4)).point;
		}
		((Component)this).transform.position = val + ((Component)this).transform.up * wheelBottomOffset;
		Vector3 val5 = Vector3.zero;
		int num2 = 3;
		for (int num3 = 0; num3 < num2; num3++)
		{
			val5 += PathUtility.GetAheadPoint(path, ((Component)this).transform.position, vehicleLength / 2f + 1f * (float)(num3 + 1), startPointIndex, endPointIndex);
		}
		val5 /= (float)num2;
		Vector3 val6 = val5 - val;
		Vector3 normalized = ((Vector3)(ref val6)).normalized;
		((Component)this).transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
		Vector3 axleGroundHit = GetAxleGroundHit(front: true);
		Vector3 axleGroundHit2 = GetAxleGroundHit(front: false);
		val6 = axleGroundHit - axleGroundHit2;
		normalized = ((Vector3)(ref val6)).normalized;
		((Component)this).transform.forward = normalized;
	}

	private Vector3 GetAxleGroundHit(bool front)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = FrontAxlePosition.position + Vector3.up * 1f;
		if (!front)
		{
			val = RearAxlePosition.position + Vector3.up * 1f;
		}
		LayerMask val2 = LayerMask.op_Implicit(LayerMask.GetMask(new string[1] { "Default" }));
		val2 = LayerMask.op_Implicit(LayerMask.op_Implicit(val2) | LayerMask.GetMask(new string[1] { "Terrain" }));
		RaycastHit[] source = Physics.RaycastAll(val, Vector3.down, 2f, LayerMask.op_Implicit(val2), (QueryTriggerInteraction)1);
		source = source.OrderBy((RaycastHit h) => ((RaycastHit)(ref h)).distance).ToArray();
		for (int num = 0; num < source.Length; num++)
		{
			if (!((Component)((RaycastHit)(ref source[num])).collider).transform.IsChildOf(((Component)this).transform))
			{
				return ((RaycastHit)(ref source[num])).point;
			}
		}
		if (front)
		{
			return FrontAxlePosition.position - ((Component)this).transform.up * wheelBottomOffset;
		}
		return RearAxlePosition.position - ((Component)this).transform.up * wheelBottomOffset;
	}

	private void UpdateSweep()
	{
		if (KinematicMode)
		{
			return;
		}
		if (Mathf.Abs(vehicle.Speed_Kmh) > 5f)
		{
			sweepTestFailedTime = 0f;
		}
		else if (Mathf.Abs(targetSteerAngle_Normalized) * maxSteerAngle > 5f)
		{
			float num = 1.5f;
			float hitDistance;
			Vector3 hitPoint;
			bool num2 = SweepTurn(ESweepType.FR, Mathf.Sign(targetSteerAngle_Normalized) * 30f, reverse: false, out hitDistance, out hitPoint, targetSteerAngle_Normalized * maxSteerAngle);
			float hitDistance2;
			Vector3 hitPoint2;
			bool flag = SweepTurn(ESweepType.FL, Mathf.Sign(targetSteerAngle_Normalized) * 30f, reverse: false, out hitDistance2, out hitPoint2, targetSteerAngle_Normalized * maxSteerAngle);
			if ((num2 && hitDistance < num) || (flag && hitDistance2 < num))
			{
				sweepTestFailedTime += Time.deltaTime;
				if ((double)sweepTestFailedTime > 0.25)
				{
					StartReverse();
					sweepTestFailedTime = 0f;
				}
			}
			else
			{
				sweepTestFailedTime = 0f;
			}
		}
		else
		{
			sweepTestFailedTime = 0f;
		}
	}

	private void UpdateSpeedReduction()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		if (path == null)
		{
			return;
		}
		if (path != null && Vector3.Distance(((Component)this).transform.position, path.vectorPath[path.vectorPath.Count - 1]) < 3f)
		{
			if (storedNavigationCallback != null)
			{
				storedNavigationCallback(ENavigationResult.Complete);
				storedNavigationCallback = null;
			}
			EndDriving();
		}
		else
		{
			if (KinematicMode)
			{
				return;
			}
			PathUtility.GetClosestPointOnPath(path, ((Component)this).transform.position, out var startPointIndex, out var _, out var pointLerp);
			float num = 1f;
			float num2 = 1f;
			float num3 = targetSpeed;
			if (Flags.TurnBasedSpeedReduction)
			{
				float num4 = Mathf.Max(PathUtility.CalculateAngleChangeOverPath(path, startPointIndex, pointLerp, turnSpeedReductionRange), targetSteerAngle_Normalized * maxSteerAngle);
				if (num4 > minTurnSpeedReductionAngleThreshold)
				{
					num3 = Mathf.Lerp(num3, minTurningSpeed, Mathf.Clamp(num4 / turnSpeedReductionDivisor, 0f, 1f));
				}
			}
			if (Flags.ObstacleMode != DriveFlags.EObstacleMode.IgnoreAll)
			{
				BetterSweepTurn(ESweepType.FL, vehicle.CurrentSteerAngle, reverse: false, sensor_FM.checkMask, out var hitDistance, out var _);
				BetterSweepTurn(ESweepType.FR, vehicle.CurrentSteerAngle, reverse: false, sensor_FM.checkMask, out var hitDistance2, out var _);
				float num5 = Mathf.Min(hitDistance, hitDistance2);
				float num6 = Mathf.Lerp(1.5f, 15f, Mathf.Clamp01(vehicle.Speed_Kmh / vehicle.TopSpeed));
				if (num5 < num6)
				{
					if (DEBUG_MODE)
					{
						Console.Log("Obstacle detected at " + num5 + "m: " + ((hitDistance < hitDistance2) ? "Left" : "Right"));
					}
					num = Mathf.Clamp((num5 - 1.5f) / (num6 - 1.5f), 0.002f, 1f);
				}
			}
			if (Flags.AutoBrakeAtDestination && path != null)
			{
				float num7 = Vector3.Distance(((Component)this).transform.position, path.vectorPath[path.vectorPath.Count - 1]);
				if (num7 < 8f)
				{
					num2 = Mathf.Clamp(num7 / 8f, 0f, 1f);
					if (num7 < 3f)
					{
						num2 = 0f;
					}
					if (num2 < 0.2f)
					{
						vehicle.handbrakeOverride = true;
					}
				}
			}
			if (DEBUG_MODE)
			{
				Debug.Log((object)("Obstacle speed multiplier: " + num));
				Debug.Log((object)("Destination speed multiplier: " + num2));
				Debug.Log((object)("Turn target speed: " + num3));
			}
			float num8 = num * num2;
			speedReductionTracker.SubmitValue(num8);
			targetSpeed *= num8;
			targetSpeed = Mathf.Min(targetSpeed, num3);
		}
	}

	private void UpdatePursuitMode()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (PursuitModeEnabled && !((Object)(object)PursuitTarget == (Object)null) && Vector3.Distance(PursuitTarget.position, PursuitTargetLastPosition) > PursuitDistanceUpdateThreshold)
		{
			PursuitTargetLastPosition = PursuitTarget.position;
			Navigate(PursuitTarget.position);
		}
	}

	private void UpdateStuckDetection()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (!AutoDriving)
		{
			PositionHistoryTracker.ClearHistory();
		}
		else
		{
			if (!Flags.StuckDetection || speedReductionTracker.RecordedHistoryLength() < StuckTimeThreshold || speedReductionTracker.GetLowestValue() < 0.1f || !(PositionHistoryTracker.RecordedTime >= StuckTimeThreshold))
			{
				return;
			}
			Vector3 val = Vector3.zero;
			for (int i = 0; i < StuckSamples; i++)
			{
				val += PositionHistoryTracker.GetPositionXSecondsAgo(StuckTimeThreshold / (float)StuckSamples * (float)(i + 1));
			}
			val /= (float)StuckSamples;
			if (Vector3.Distance(((Component)this).transform.position, val) < StuckDistanceThreshold)
			{
				if (DEBUG_MODE)
				{
					Console.LogWarning("Vehicle stuck");
				}
				if (IsOnVehicleGraph())
				{
					Teleporter.MoveToRoadNetwork();
				}
				else
				{
					Teleporter.MoveToGraph();
				}
				PositionHistoryTracker.ClearHistory();
			}
		}
	}

	private void CheckDistanceFromPath()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (timeSinceLastNavigationCall < MinRenavigationRate || KinematicMode || path == null)
		{
			return;
		}
		Vector3 closestPointOnPath = PathUtility.GetClosestPointOnPath(path, ((Component)this).transform.position, out var _, out var _, out var _);
		closestPointOnPath += GetPathLateralDirection() * lateralOffset;
		if (Vector3.Distance(((Component)this).transform.position, closestPointOnPath) > (IsReversing ? 8f : 6f))
		{
			if (DEBUG_MODE)
			{
				Console.Log("Too far from path! Re-navigating.");
				Debug.DrawLine(((Component)this).transform.position, closestPointOnPath, Color.red, 2f);
			}
			Navigate(TargetLocation, currentNavigationSettings, storedNavigationCallback);
		}
	}

	private void UpdateOvertaking()
	{
		lateralOffset = 0f;
		if ((Object)(object)sensor_FM.obstruction != (Object)null && (Object)(object)((Component)sensor_FM.obstruction).GetComponentInParent<LandVehicle>() != (Object)null && sensor_FM.obstructionDistance < 8f)
		{
			_ = sensor_FM.obstructionDistance / 8f;
		}
	}

	protected virtual void RefreshSpeedZone()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		IEnumerable<SpeedZone> speedZones = SpeedZone.GetSpeedZones(((Component)this).transform.position);
		currentSpeedZone = speedZones.FirstOrDefault();
	}

	protected virtual void UpdateSpeed()
	{
		if (path == null)
		{
			targetSpeed = 0f;
			return;
		}
		if ((Object)(object)currentSpeedZone != (Object)null)
		{
			targetSpeed = currentSpeedZone.speed * Flags.SpeedLimitMultiplier;
		}
		else
		{
			targetSpeed = UnmarkedSpeed * Flags.SpeedLimitMultiplier;
		}
		if (Flags.OverrideSpeed)
		{
			targetSpeed = Flags.OverriddenSpeed;
		}
	}

	protected void UpdateSteering()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		if (path == null || path.vectorPath.Count < 2 || KinematicMode)
		{
			targetSteerAngle_Normalized = 0f;
			return;
		}
		PathUtility.GetClosestPointOnPath(path, ((Component)this).transform.position, out var _, out var _, out var _);
		Vector3 aheadPoint = PathUtility.GetAheadPoint(path, ((Component)this).transform.position, vehicleLength / 2f + sampleStepSize);
		Vector3 averageAheadPoint = PathUtility.GetAverageAheadPoint(path, ((Component)this).transform.position, aheadPointSamples, sampleStepSize);
		Vector3 val = averageAheadPoint - aheadPoint;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		if (DEBUG_MODE)
		{
			Debug.DrawLine(((Component)this).transform.position, aheadPoint, Color.yellow, 0.5f);
			Debug.DrawLine(((Component)this).transform.position, averageAheadPoint, Color.magenta, 0.5f);
		}
		float error = PathUtility.CalculateCTE(CTE_Origin.position + ((Component)this).transform.forward * Mathf.Clamp01(vehicle.Speed_Kmh / vehicle.TopSpeed) * (vehicle.TopSpeed * 0.2778f * 0.3f), ((Component)this).transform, aheadPoint, averageAheadPoint, path);
		float num = Mathf.Clamp(steerPID.GetNewValue(error, new PID_Parameters(40f, 5f, 10f)) / maxSteerAngle, -1f, 1f);
		float num2 = Vector3.SignedAngle(((Component)this).transform.forward, normalized, Vector3.up);
		float num3 = 45f;
		if (Mathf.Abs(num2) > 45f)
		{
			num += Mathf.Clamp01(Mathf.Abs(num2 - num3) / (180f - num3)) * Mathf.Sign(num2);
		}
		targetSteerAngle_Normalized = Mathf.Clamp(num, -1f, 1f);
	}

	public void Navigate(Vector3 location, NavigationSettings settings = null, NavigationCallback callback = null)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		if (navigationCalculationRoutine != null)
		{
			Console.LogWarning("Navigate called before previous navigation calculation was complete!");
			((MonoBehaviour)this).StopCoroutine(navigationCalculationRoutine);
		}
		if (GetIsStuck())
		{
			Console.LogWarning("Navigate called but vehilc is stuck! Navigation will still be attemped");
		}
		if (reverseCoroutine != null)
		{
			StopReversing();
		}
		if (!InstanceFinder.IsHost)
		{
			return;
		}
		path = null;
		timeOnLastNavigationCall = Time.timeSinceLevelLoad;
		if (settings == null)
		{
			settings = new NavigationSettings();
		}
		if (GetDistanceFromVehicleGraph() > 6f)
		{
			if (settings.ensureProximityToGraph)
			{
				Teleporter.MoveToGraph();
			}
			else if (callback != null)
			{
				callback(ENavigationResult.Failed);
				return;
			}
		}
		if (DEBUG_MODE)
		{
			Console.Log("Navigate called...");
		}
		TargetLocation = location;
		AutoDriving = true;
		storedNavigationCallback = callback;
		Sensor[] array = sensors;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Enabled = true;
		}
		vehicle.OverrideMaxSteerAngle(35f);
		vehicle.overrideControls = true;
		currentNavigationSettings = settings;
		navigationCalculationRoutine = NavigationUtility.CalculatePath(frontOfVehiclePosition, TargetLocation, currentNavigationSettings, Flags, generalSeeker, roadSeeker, NavigationCalculationCallback);
	}

	private void NavigationCalculationCallback(NavigationUtility.ENavigationCalculationResult result, PathSmoothingUtility.SmoothedPath _path)
	{
		navigationCalculationRoutine = null;
		if (result == NavigationUtility.ENavigationCalculationResult.Failed)
		{
			if (storedNavigationCallback != null)
			{
				storedNavigationCallback(ENavigationResult.Failed);
			}
			EndDriving();
		}
		else
		{
			path = _path;
			path.InitializePath();
		}
	}

	private void EndDriving()
	{
		if (DEBUG_MODE)
		{
			Console.Log("End driving");
		}
		AutoDriving = false;
		vehicle.ResetMaxSteerAngle();
		Sensor[] array = sensors;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Enabled = false;
		}
		path = null;
		storedNavigationCallback = null;
		vehicle.overrideControls = false;
		vehicle.steerOverride = 0f;
		vehicle.throttleOverride = 0f;
		currentNavigationSettings = null;
	}

	public void StopNavigating()
	{
		if (navigationCalculationRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(navigationCalculationRoutine);
		}
		if (storedNavigationCallback != null)
		{
			storedNavigationCallback(ENavigationResult.Stopped);
		}
		EndDriving();
	}

	public void RecalculateNavigation()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (AutoDriving)
		{
			Navigate(TargetLocation, currentNavigationSettings, storedNavigationCallback);
		}
	}

	public bool SweepTurn(ESweepType sweep, float sweepAngle, bool reverse, out float hitDistance, out Vector3 hitPoint, float steerAngle = 0f)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		hitDistance = float.MaxValue;
		hitPoint = Vector3.zero;
		if (steerAngle == 0f)
		{
			steerAngle = maxSteerAngle;
		}
		steerAngle = Mathf.Abs(steerAngle);
		float num = Mathf.Sign(sweepAngle);
		FrontAxlePosition.localEulerAngles = new Vector3(0f, num * steerAngle, 0f);
		float num2 = turnRadius;
		Vector3 zero = Vector3.zero;
		Vector3 castStart = Vector3.zero;
		zero = ((!(sweepAngle > 0f)) ? (sweepOrigin_FR.position - FrontAxlePosition.right * turnRadius) : (sweepOrigin_FL.position + FrontAxlePosition.right * turnRadius));
		switch (sweep)
		{
		case ESweepType.FL:
			castStart = sweepOrigin_FL.position;
			break;
		case ESweepType.FR:
			castStart = sweepOrigin_FR.position;
			break;
		case ESweepType.RL:
			castStart = sweepOrigin_RL.position;
			break;
		case ESweepType.RR:
			castStart = sweepOrigin_RR.position;
			break;
		}
		Vector3 val = castStart - zero;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = Quaternion.AngleAxis(90f * num, ((Component)this).transform.up) * normalized;
		num2 = Vector3.Distance(zero, castStart);
		float num3 = 0f;
		float num4 = 0f;
		do
		{
			float num5 = num3;
			float num6 = Mathf.Clamp(num5 + Mathf.Abs(15f), 0f, Mathf.Abs(sweepAngle));
			num3 += num6 - num5;
			float num7 = num2 * Mathf.Cos(num6 * ((float)System.Math.PI / 180f));
			float num8 = num2 * Mathf.Sin(num6 * ((float)System.Math.PI / 180f));
			Vector3 val3 = zero;
			val3 += val2 * num8 * (reverse ? (-1f) : 1f);
			val3 += normalized * num7;
			Vector3 val4 = castStart;
			val = val3 - castStart;
			RaycastHit[] array = Physics.SphereCastAll(val4, 0.1f, ((Vector3)(ref val)).normalized, Vector3.Distance(castStart, val3), LayerMask.op_Implicit(sweepMask), (QueryTriggerInteraction)1);
			if (array.Length != 0)
			{
				array = array.OrderBy((RaycastHit x) => Vector3.Distance(castStart, ((RaycastHit)(ref x)).point)).ToArray();
			}
			RaycastHit val5 = default(RaycastHit);
			bool flag = false;
			for (int num9 = 0; num9 < array.Length; num9++)
			{
				if (!((Component)((RaycastHit)(ref array[num9])).collider).transform.IsChildOf(((Component)this).transform))
				{
					val5 = array[num9];
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (((RaycastHit)(ref val5)).point == Vector3.zero)
				{
					((RaycastHit)(ref val5)).point = castStart;
				}
				num4 += Vector3.Distance(castStart, ((RaycastHit)(ref val5)).point);
				hitDistance = num4;
				hitPoint = ((RaycastHit)(ref val5)).point;
				return true;
			}
			num4 += Vector3.Distance(castStart, val3);
			castStart = val3;
		}
		while (!(num3 >= Mathf.Abs(sweepAngle)));
		return false;
	}

	public void BetterSweepTurn(ESweepType sweep, float steerAngle, bool reverse, LayerMask mask, out float hitDistance, out RaycastHit hit)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		hitDistance = float.MaxValue;
		hit = default(RaycastHit);
		float num = Mathf.Sign(steerAngle);
		FrontAxlePosition.localEulerAngles = new Vector3(0f, steerAngle, 0f);
		Vector3 zero = Vector3.zero;
		Vector3 castStart = Vector3.zero;
		float num2 = Mathf.Clamp(wheelbase / Mathf.Sin(steerAngle * ((float)System.Math.PI / 180f)), -100f, 100f);
		zero = sweepOrigin_FL.position + FrontAxlePosition.right * num2;
		switch (sweep)
		{
		case ESweepType.FL:
			castStart = sweepOrigin_FL.position;
			break;
		case ESweepType.FR:
			castStart = sweepOrigin_FR.position;
			break;
		case ESweepType.RL:
			castStart = sweepOrigin_RL.position;
			break;
		case ESweepType.RR:
			castStart = sweepOrigin_RR.position;
			break;
		default:
			Console.LogWarning("Invalid sweep type: " + sweep);
			break;
		}
		Debug.DrawLine(castStart, zero, Color.white);
		Vector3 val = castStart - zero;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = Quaternion.AngleAxis(90f * num, ((Component)this).transform.up) * normalized;
		num2 = Vector3.Distance(zero, castStart);
		float num3 = 0f;
		int num4 = 6;
		float num5 = Mathf.Clamp(Mathf.Abs(steerAngle), 5f, 30f);
		for (float num6 = 0f; num6 < (float)num4; num6 += 1f)
		{
			float num7 = num5 * (num6 + 1f);
			float num8 = num2 * Mathf.Cos(num7 * ((float)System.Math.PI / 180f));
			float num9 = num2 * Mathf.Sin(num7 * ((float)System.Math.PI / 180f));
			Vector3 val3 = zero;
			val3 += val2 * num9 * (reverse ? (-1f) : 1f);
			val3 += normalized * num8;
			Vector3 val4 = castStart;
			float checkRadius = sensor_FM.checkRadius;
			val = val3 - castStart;
			RaycastHit[] array = Physics.SphereCastAll(val4, checkRadius, ((Vector3)(ref val)).normalized, Vector3.Distance(castStart, val3), LayerMask.op_Implicit(mask));
			if (array.Length != 0)
			{
				array = array.OrderBy((RaycastHit x) => Vector3.Distance(castStart, ((RaycastHit)(ref x)).point)).ToArray();
			}
			RaycastHit val5 = default(RaycastHit);
			bool flag = false;
			for (int num10 = 0; num10 < array.Length; num10++)
			{
				if (((Component)((RaycastHit)(ref array[num10])).collider).transform.IsChildOf(((Component)this).transform) || ((Component)((RaycastHit)(ref array[num10])).collider).transform.IsChildOf(((Component)((Component)vehicle.HumanoidColliderContainer).transform).transform))
				{
					continue;
				}
				if (Flags.IgnoreTrafficLights)
				{
					VehicleObstacle componentInParent = ((Component)((RaycastHit)(ref array[num10])).transform).GetComponentInParent<VehicleObstacle>();
					if (componentInParent != null && componentInParent.type == VehicleObstacle.EObstacleType.TrafficLight)
					{
						continue;
					}
				}
				VehicleObstacle componentInParent2 = ((Component)((Component)((RaycastHit)(ref array[num10])).collider).transform).GetComponentInParent<VehicleObstacle>();
				if ((Object)(object)componentInParent2 != (Object)null)
				{
					if (!componentInParent2.twoSided && Vector3.Angle(-((Component)componentInParent2).transform.forward, ((Component)this).transform.forward) > 90f)
					{
						continue;
					}
				}
				else if (((RaycastHit)(ref array[num10])).collider.isTrigger)
				{
					continue;
				}
				if (Flags.ObstacleMode != DriveFlags.EObstacleMode.IgnoreOnlySquishy || (!((Object)(object)((Component)((RaycastHit)(ref array[num10])).transform).GetComponentInParent<LandVehicle>() != (Object)null) && !((Object)(object)((Component)((RaycastHit)(ref array[num10])).transform).GetComponentInParent<Player>() != (Object)null) && !((Object)(object)((Component)((RaycastHit)(ref array[num10])).transform).GetComponentInParent<NPC>() != (Object)null)))
				{
					val5 = array[num10];
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (((RaycastHit)(ref val5)).point == Vector3.zero)
				{
					((RaycastHit)(ref val5)).point = castStart;
				}
				num3 += Vector3.Distance(castStart, ((RaycastHit)(ref val5)).point);
				hitDistance = num3;
				hit = val5;
				Debug.DrawLine(castStart, ((RaycastHit)(ref val5)).point, Color.red);
				break;
			}
			num3 += Vector3.Distance(castStart, val3);
			Debug.DrawLine(castStart, val3, (num6 % 2f == 0f) ? Color.green : Color.cyan);
			castStart = val3;
		}
	}

	public void StartReverse()
	{
		if (reverseCoroutine != null)
		{
			StopReversing();
		}
		reverseCoroutine = ((MonoBehaviour)this).StartCoroutine(Reverse());
	}

	public IEnumerator Reverse()
	{
		if (DEBUG_MODE)
		{
			Console.Log("Starting reverse operation");
		}
		targetSpeed = 0f;
		targetSteerAngle_Normalized = 0f;
		PathUtility.GetClosestPointOnPath(path, ((Component)this).transform.position, out var startPointIndex, out var endPointIndex, out var pointLerp);
		float num = 3f;
		_ = Vector3.zero;
		_ = Vector3.zero;
		int num2 = 0;
		Vector3 futureTarget;
		float steerAngleNormal;
		while (true)
		{
			Vector3 val = Vector3.zero;
			Vector3 val2 = Vector3.zero;
			for (int i = 1; i <= aheadPointSamples; i++)
			{
				val += PathUtility.GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, num + (float)i * sampleStepSizeMin);
				if (i == aheadPointSamples)
				{
					val2 = PathUtility.GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, num + (float)i * sampleStepSizeMin + 1f);
				}
			}
			val /= (float)aheadPointSamples;
			if (Mathf.Abs(((Component)this).transform.InverseTransformPoint(val).x) > 1f)
			{
				futureTarget = val;
				_ = val2 - futureTarget;
				steerAngleNormal = 0f - Mathf.Sign(((Component)this).transform.InverseTransformPoint(futureTarget).x);
				yield return (object)new WaitForSeconds(1f);
				break;
			}
			if (num2 >= 25)
			{
				reverseCoroutine = null;
				Console.LogWarning("Can't calculate average ahead point!");
				yield break;
			}
			num2++;
			num += 1f;
		}
		ESweepType frontWheel = ESweepType.FL;
		if (steerAngleNormal < 0f)
		{
			frontWheel = ESweepType.FR;
		}
		float num3 = 10f;
		float num4 = 90f;
		Vector3 val3 = futureTarget - ((Component)this).transform.position;
		val3.y = 0f;
		((Vector3)(ref val3)).Normalize();
		Vector3 forward = ((Component)this).transform.forward;
		forward.y = 0f;
		float sweepAngle = num3 + (num4 - num3) * Mathf.Clamp(Vector3.Angle(forward, val3) / 90f, 0f, 1f);
		if (DEBUG_MODE)
		{
			Console.Log("Beginning straight reverse...");
		}
		float reverseSweepDistanceMin = 1.25f;
		targetSpeed = (Flags.OverrideSpeed ? (0f - Flags.OverriddenReverseSpeed) : (0f - ReverseSpeed));
		bool canBeginSwing = false;
		while (!canBeginSwing)
		{
			yield return (object)new WaitForEndOfFrame();
			float hitDistance = 0f;
			float hitDistance2 = 0f;
			float hitDistance3 = 0f;
			Vector3 hitPoint = Vector3.zero;
			Vector3 hitPoint2 = Vector3.zero;
			if (SweepTurn(frontWheel, sweepAngle * steerAngleNormal, reverse: true, out hitDistance, out hitPoint) || SweepTurn(ESweepType.RL, sweepAngle * steerAngleNormal, reverse: true, out hitDistance2, out hitPoint2) || SweepTurn(ESweepType.RR, sweepAngle * steerAngleNormal, reverse: true, out hitDistance3, out hitPoint2))
			{
				float num5 = 2f;
				if (sensor_RR.obstructionDistance < num5 || sensor_RL.obstructionDistance < num5)
				{
					if (DEBUG_MODE)
					{
						Console.Log("Continued straight reversing will result in collision; starting turn");
					}
					canBeginSwing = true;
				}
			}
			else if (((Component)this).transform.InverseTransformPoint(futureTarget).z > 0f - vehicleLength)
			{
				canBeginSwing = true;
			}
		}
		if (DEBUG_MODE)
		{
			Console.Log("Beginning swing...");
		}
		targetSteerAngle_Normalized = steerAngleNormal;
		Vector3 faceTarget = PathUtility.GetClosestPointOnPath(path, ((Component)this).transform.position, out startPointIndex, out endPointIndex, out pointLerp);
		Vector3 val4 = PathUtility.GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, 0.5f) - faceTarget;
		Vector3 normalized = ((Vector3)(ref val4)).normalized;
		faceTarget += normalized * vehicleLength / 2f;
		bool continueReversing = true;
		while (continueReversing)
		{
			yield return (object)new WaitForEndOfFrame();
			if (path == null)
			{
				continueReversing = false;
				continue;
			}
			val3 = faceTarget - ((Component)this).transform.position;
			val3.y = 0f;
			((Vector3)(ref val3)).Normalize();
			forward = ((Component)this).transform.forward;
			forward.y = 0f;
			Debug.DrawLine(((Component)this).transform.position, faceTarget, Color.magenta);
			Debug.DrawLine(((Component)this).transform.position, ((Component)this).transform.position + forward * 5f, Color.cyan);
			if (Vector3.Angle(val3, forward) < 20f)
			{
				continueReversing = false;
			}
			float hitDistance4 = float.MaxValue;
			float hitDistance5 = float.MaxValue;
			float hitDistance6 = float.MaxValue;
			if ((SweepTurn(frontWheel, 30f * steerAngleNormal, reverse: true, out hitDistance4, out var hitPoint3) || SweepTurn(ESweepType.RL, 30f * steerAngleNormal, reverse: true, out hitDistance5, out hitPoint3) || SweepTurn(ESweepType.RR, 30f * steerAngleNormal, reverse: true, out hitDistance6, out hitPoint3)) && (hitDistance4 < reverseSweepDistanceMin || hitDistance5 < reverseSweepDistanceMin || hitDistance6 < reverseSweepDistanceMin))
			{
				continueReversing = false;
				if (DEBUG_MODE)
				{
					Console.Log("Reverse sweep obstructed");
				}
			}
		}
		targetSpeed = 0f;
		yield return (object)new WaitUntil((Func<bool>)(() => vehicle.Speed_Kmh >= -1f));
		if (DEBUG_MODE)
		{
			Console.Log("Reverse finished");
		}
		reverseCoroutine = null;
	}

	private void StopReversing()
	{
		if (DEBUG_MODE)
		{
			Console.Log("Reverse stop");
		}
		if (reverseCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(reverseCoroutine);
			reverseCoroutine = null;
			targetSpeed = 0f;
		}
	}

	private Collider GetClosestForwardObstruction(out float obstructionDist)
	{
		Collider result = null;
		obstructionDist = float.MaxValue;
		foreach (Sensor item in new List<Sensor> { sensor_FL, sensor_FM, sensor_FR })
		{
			if (!((Object)(object)item.obstruction != (Object)null))
			{
				continue;
			}
			if (Flags.IgnoreTrafficLights)
			{
				VehicleObstacle componentInParent = ((Component)item.obstruction).GetComponentInParent<VehicleObstacle>();
				if (componentInParent != null && componentInParent.type == VehicleObstacle.EObstacleType.TrafficLight)
				{
					continue;
				}
			}
			if ((Flags.ObstacleMode != DriveFlags.EObstacleMode.IgnoreOnlySquishy || (!((Object)(object)((Component)item.obstruction).GetComponentInParent<LandVehicle>() != (Object)null) && !((Object)(object)((Component)item.obstruction).GetComponentInParent<Player>() != (Object)null) && !((Object)(object)((Component)item.obstruction).GetComponentInParent<NPC>() != (Object)null))) && item.obstructionDistance < obstructionDist)
			{
				result = item.obstruction;
				obstructionDist = item.obstructionDistance;
			}
		}
		return result;
	}

	public bool IsOnVehicleGraph()
	{
		return GetDistanceFromVehicleGraph() < 2.5f;
	}

	private float GetDistanceFromVehicleGraph()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		NNConstraint val = new NNConstraint();
		val.graphMask = GraphMask.FromGraphName("General Vehicle Graph");
		return Vector3.Distance(AstarPath.active.GetNearest(((Component)this).transform.position, val).position, ((Component)this).transform.position - ((Component)this).transform.up * vehicle.BoundingBoxDimensions.y / 2f);
	}

	private Vector3 GetPathLateralDirection()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (path == null)
		{
			Console.LogWarning("Path is null!");
			return Vector3.zero;
		}
		int startPointIndex;
		int endPointIndex;
		float pointLerp;
		Vector3 closestPointOnPath = PathUtility.GetClosestPointOnPath(path, ((Component)this).transform.position, out startPointIndex, out endPointIndex, out pointLerp);
		Vector3 pointAheadOfPathPoint = PathUtility.GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, 0.01f);
		Quaternion val = Quaternion.AngleAxis(90f, ((Component)this).transform.up);
		Vector3 val2 = pointAheadOfPathPoint - closestPointOnPath;
		return val * ((Vector3)(ref val2)).normalized;
	}

	public bool GetIsStuck()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (speedReductionTracker.RecordedHistoryLength() < StuckTimeThreshold)
		{
			return false;
		}
		if (speedReductionTracker.GetLowestValue() < 0.1f)
		{
			return false;
		}
		if (PositionHistoryTracker.RecordedTime >= StuckTimeThreshold)
		{
			Vector3 val = Vector3.zero;
			for (int i = 0; i < StuckSamples; i++)
			{
				val += PositionHistoryTracker.GetPositionXSecondsAgo(StuckTimeThreshold / (float)StuckSamples * (float)(i + 1));
			}
			val /= (float)StuckSamples;
			if (Vector3.Distance(((Component)this).transform.position, val) < StuckDistanceThreshold)
			{
				if (DEBUG_MODE)
				{
					Console.LogWarning("Vehicle stuck");
				}
				return true;
			}
		}
		return false;
	}
}
