using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Component.Ownership;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using Pathfinding;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.Graffiti;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using ScheduleOne.Vehicles.AI;
using ScheduleOne.Vehicles.Modification;
using ScheduleOne.Weather;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ScheduleOne.Vehicles;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(PredictedOwner))]
[RequireComponent(typeof(PhysicsDamageable))]
public class LandVehicle : NetworkBehaviour, IGUIDRegisterable, ISaveable, IWeatherEntity
{
	public const float KINEMATIC_THRESHOLD_DISTANCE = 30f;

	public const float MAX_TURNOVER_SPEED = 5f;

	public const float TURNOVER_FORCE = 8f;

	public const bool USE_WHEEL = false;

	public const float SPEED_DISPLAY_MULTIPLIER = 1.4f;

	public const float MaxImpactDamage = 120f;

	public const float MaxImpactDamageSpeed = 100f;

	public bool DEBUG;

	[Header("Settings")]
	[SerializeField]
	protected string vehicleName = "Vehicle";

	[SerializeField]
	protected string vehicleCode = "vehicle_code";

	[SerializeField]
	protected float vehiclePrice = 1000f;

	public bool UseHumanoidCollider = true;

	public bool SpawnAsPlayerOwned;

	[Header("References")]
	[SerializeField]
	protected GameObject vehicleModel;

	[SerializeField]
	protected WheelCollider[] driveWheels;

	[SerializeField]
	protected WheelCollider[] steerWheels;

	[SerializeField]
	protected WheelCollider[] handbrakeWheels;

	[HideInInspector]
	public List<Wheel> wheels = new List<Wheel>();

	[SerializeField]
	protected InteractableObject intObj;

	[SerializeField]
	protected List<Transform> exitPoints = new List<Transform>();

	public Rigidbody Rb;

	public VehicleColor Color;

	public VehicleSeat[] Seats;

	public BoxCollider boundingBox;

	public VehicleAgent Agent;

	public SmoothedVelocityCalculator VelocityCalculator;

	public StorageDoorAnimation Trunk;

	public NavMeshObstacle NavMeshObstacle;

	public NavmeshCut NavmeshCut;

	public VehicleHumanoidCollider HumanoidColliderContainer;

	public POI POI;

	[SerializeField]
	private SpraySurface[] _spraySurfaces;

	private List<PlayerPusher> pushers = new List<PlayerPusher>();

	[SerializeField]
	protected Transform centerOfMass;

	[SerializeField]
	protected Transform cameraOrigin;

	[SerializeField]
	protected VehicleLights lights;

	[Header("Steer settings")]
	[SerializeField]
	protected float maxSteeringAngle = 25f;

	[SerializeField]
	protected float steerRate = 50f;

	[SerializeField]
	protected bool flipSteer;

	[Header("Drive settings")]
	[SerializeField]
	protected AnimationCurve motorTorque = new AnimationCurve((Keyframe[])(object)new Keyframe[3]
	{
		new Keyframe(0f, 200f),
		new Keyframe(50f, 300f),
		new Keyframe(200f, 0f)
	});

	public float TopSpeed = 60f;

	[Range(2f, 16f)]
	[SerializeField]
	protected float diffGearing = 4f;

	[SerializeField]
	protected float handBrakeForce = 300f;

	[SerializeField]
	protected AnimationCurve brakeForce = new AnimationCurve((Keyframe[])(object)new Keyframe[3]
	{
		new Keyframe(0f, 200f),
		new Keyframe(50f, 300f),
		new Keyframe(200f, 0f)
	});

	[SerializeField]
	[Range(0.1f, 3f)]
	protected float BrakeForceMultiplier = 1f;

	[Range(0.5f, 10f)]
	[SerializeField]
	protected float downforce = 1f;

	[Range(0f, 1f)]
	[SerializeField]
	protected float reverseMultiplier = 0.35f;

	[HideInInspector]
	public bool overrideControls;

	[HideInInspector]
	public float throttleOverride;

	[HideInInspector]
	public float steerOverride;

	[HideInInspector]
	public bool handbrakeOverride;

	[Header("Storage settings")]
	public StorageEntity Storage;

	private VehicleSeat localPlayerSeat;

	private bool _isOccupied;

	private RollingAverage<float> previousSpeeds = new RollingAverage<float>(20, (float a, float b) => a + b, (float a, float b) => a - b, (float a, float c) => a / c);

	private const int previousSpeedsSampleSize = 20;

	[CompilerGenerated]
	[HideInInspector]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public float _003CCurrentSteerAngle_003Ek__BackingField;

	private float lastFrameSteerAngle;

	private float lastReplicatedSteerAngle;

	private bool justExitedVehicle;

	[CompilerGenerated]
	[HideInInspector]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public bool _003CBrakesApplied_003Ek__BackingField;

	[CompilerGenerated]
	[HideInInspector]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public bool _003CIsReversing_003Ek__BackingField;

	private Vector3 lastFramePosition = Vector3.zero;

	private Transform closestExitPoint;

	private float timeOnSpawn;

	private float timeOnLastOccupied;

	[HideInInspector]
	public ParkData CurrentParkData;

	private VehicleLoader loader = new VehicleLoader();

	public Action onVehicleStart;

	public Action onVehicleStop;

	public Action onHandbrakeApplied;

	public Action<Collision> onCollision;

	public SyncVar<float> syncVar____003CCurrentSteerAngle_003Ek__BackingField;

	public SyncVar<bool> syncVar____003CBrakesApplied_003Ek__BackingField;

	public SyncVar<bool> syncVar____003CIsReversing_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EVehicles_002ELandVehicleAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EVehicles_002ELandVehicleAssembly_002DCSharp_002Edll_Excuted;

	public string VehicleName => vehicleName;

	public string VehicleCode => vehicleCode;

	public float VehiclePrice => vehiclePrice;

	public bool IsPlayerOwned { get; protected set; }

	public bool IsVisible { get; protected set; } = true;

	public Guid GUID { get; protected set; }

	public Vector3 BoundingBoxDimensions => new Vector3(boundingBox.size.x * ((Component)boundingBox).transform.localScale.x, boundingBox.size.y * ((Component)boundingBox).transform.localScale.y, boundingBox.size.z * ((Component)boundingBox).transform.localScale.z);

	public Transform driverEntryPoint => exitPoints[0];

	public float ActualMaxSteeringAngle
	{
		get
		{
			if (!MaxSteerAngleOverridden)
			{
				return maxSteeringAngle;
			}
			return OverriddenMaxSteerAngle;
		}
	}

	public bool MaxSteerAngleOverridden { get; private set; }

	public float OverriddenMaxSteerAngle { get; private set; }

	public int Capacity => Seats.Length;

	public int CurrentPlayerOccupancy => Seats.Count((VehicleSeat s) => s.isOccupied);

	public bool LocalPlayerIsDriver { get; protected set; }

	public bool LocalPlayerIsInVehicle { get; protected set; }

	public bool IsOccupied
	{
		get
		{
			return _isOccupied;
		}
		set
		{
			if (_isOccupied && !value)
			{
				timeOnLastOccupied = Time.timeSinceLevelLoad;
			}
			_isOccupied = value;
		}
	}

	public Player DriverPlayer
	{
		get
		{
			if ((Object)(object)Seats[0].Occupant != (Object)null)
			{
				return Seats[0].Occupant;
			}
			return null;
		}
	}

	public List<Player> OccupantPlayers => (from s in Seats
		where s.isOccupied
		select s.Occupant).ToList();

	public NPC[] OccupantNPCs { get; protected set; } = new NPC[0];

	public float Speed_Kmh { get; protected set; }

	public bool IsPhysicallySimulated { get; protected set; }

	public float currentThrottle { get; protected set; }

	public float CurrentSteerAngle
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CCurrentSteerAngle_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.sync___set_value__003CCurrentSteerAngle_003Ek__BackingField(value, true);
		}
	}

	public bool BrakesApplied
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CBrakesApplied_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.sync___set_value__003CBrakesApplied_003Ek__BackingField(value, true);
		}
	}

	public bool IsReversing
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CIsReversing_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.sync___set_value__003CIsReversing_003Ek__BackingField(value, true);
		}
	}

	public bool HandbrakeApplied { get; protected set; }

	public float boundingBaseOffset => ((Component)this).transform.InverseTransformPoint(((Component)boundingBox).transform.position).y + boundingBox.size.y * 0.5f;

	private float timeSinceSpawn => Time.timeSinceLevelLoad - timeOnSpawn;

	public float timeSinceLastOccupied => Time.timeSinceLevelLoad - timeOnLastOccupied;

	public EVehicleColor OwnedColor { get; private set; } = EVehicleColor.White;

	public bool isParked => (Object)(object)CurrentParkingLot != (Object)null;

	public ParkingLot CurrentParkingLot { get; protected set; }

	public ParkingSpot CurrentParkingSpot { get; protected set; }

	public string SaveFolderName => vehicleCode + "_" + GUID.ToString().Substring(0, 6);

	public string SaveFileName => "Vehicle";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; } = true;

	Transform IWeatherEntity.Transform => ((Component)this).transform;

	string IWeatherEntity.WeatherVolume { get; set; }

	public bool IsUnderCover { get; set; }

	public float SyncAccessor__003CCurrentSteerAngle_003Ek__BackingField
	{
		get
		{
			return CurrentSteerAngle;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				CurrentSteerAngle = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CCurrentSteerAngle_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public bool SyncAccessor__003CBrakesApplied_003Ek__BackingField
	{
		get
		{
			return BrakesApplied;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				BrakesApplied = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CBrakesApplied_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public bool SyncAccessor__003CIsReversing_003Ek__BackingField
	{
		get
		{
			return IsReversing;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				IsReversing = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CIsReversing_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EVehicles_002ELandVehicle_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		((NetworkBehaviour)this).NetworkObject.GiveOwnership(((NetworkBehaviour)this).LocalConnection);
		Rb.interpolation = (RigidbodyInterpolation)1;
		if (SpawnAsPlayerOwned)
		{
			IsPlayerOwned = true;
			SetIsPlayerOwned(null, playerOwned: true);
		}
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (connection.IsHost)
		{
			return;
		}
		SetOwnedColor(connection, OwnedColor);
		for (int i = 0; i < Seats.Length; i++)
		{
			if ((Object)(object)Seats[i].Occupant != (Object)null)
			{
				SetSeatOccupant(connection, i, Seats[i].Occupant.Connection);
			}
		}
		if (isParked)
		{
			Park_Networked(connection, CurrentParkData);
		}
		if (IsPlayerOwned)
		{
			SetIsPlayerOwned(connection, playerOwned: true);
		}
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		UpdatePhysicallySimulated(forceApply: true);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetIsPlayerOwned(NetworkConnection conn, bool playerOwned)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetIsPlayerOwned_214505783(conn, playerOwned);
			RpcLogic___SetIsPlayerOwned_214505783(conn, playerOwned);
		}
		else
		{
			RpcWriter___Target_SetIsPlayerOwned_214505783(conn, playerOwned);
		}
	}

	private void RefreshPoI()
	{
		if ((Object)(object)POI != (Object)null)
		{
			if (IsPlayerOwned)
			{
				POI.SetMainText("Owned Vehicle\n(" + Singleton<VehicleColors>.Instance.GetColorName(OwnedColor) + " " + VehicleName + ")");
				((Behaviour)POI).enabled = true;
			}
			else
			{
				((Behaviour)POI).enabled = false;
			}
		}
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	protected virtual void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		intObj.onHovered.AddListener(new UnityAction(Hovered));
		intObj.onInteractStart.AddListener(new UnityAction(Interacted));
		if ((Object)(object)centerOfMass != (Object)null)
		{
			Rb.centerOfMass = ((Component)this).transform.InverseTransformPoint(((Component)centerOfMass).transform.position);
		}
		if (GUID == Guid.Empty)
		{
			GUID = GUIDManager.GenerateUniqueGUID();
		}
		MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
		instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Combine(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
		GameInput.RegisterExitListener(Exit);
		if (UseHumanoidCollider)
		{
			HumanoidColliderContainer.Vehicle = this;
			((Component)HumanoidColliderContainer).transform.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
		}
		else
		{
			((Component)HumanoidColliderContainer).gameObject.SetActive(false);
		}
		if (!NetworkSingleton<VehicleManager>.Instance.AllVehicles.Contains(this))
		{
			NetworkSingleton<VehicleManager>.Instance.AllVehicles.Add(this);
		}
		EnvironmentHandler.RegisterWeatherEntity(this);
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && action.exitType == ExitType.Escape && LocalPlayerIsInVehicle)
		{
			action.Used = true;
			ExitVehicle();
		}
	}

	protected virtual void OnDestroy()
	{
		if (NetworkSingleton<MoneyManager>.InstanceExists)
		{
			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
			instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Remove(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
		}
		if ((Object)(object)HumanoidColliderContainer != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)HumanoidColliderContainer).gameObject);
		}
		if (NetworkSingleton<VehicleManager>.InstanceExists)
		{
			NetworkSingleton<VehicleManager>.Instance.AllVehicles.Remove(this);
		}
		EnvironmentHandler.UnregisterWeatherEntity(this);
	}

	private void GetNetworth(MoneyManager.FloatContainer container)
	{
		if (IsPlayerOwned)
		{
			container.ChangeValue(GetVehicleValue());
		}
	}

	protected virtual void Update()
	{
		if (PlayerSingleton<PlayerCamera>.InstanceExists && ((NetworkBehaviour)this).IsSpawned)
		{
			bool flag = LocalPlayerIsDriver || ((NetworkBehaviour)this).IsOwner || (((NetworkBehaviour)this).OwnerId == -1 && InstanceFinder.IsHost);
			Rb.interpolation = (RigidbodyInterpolation)(flag ? 1 : 0);
			if (LocalPlayerIsInVehicle && !Singleton<PauseMenu>.Instance.IsPaused && GameInput.GetButtonDown(GameInput.ButtonCode.Interact) && !GameInput.IsTyping)
			{
				ExitVehicle();
			}
			if (overrideControls)
			{
				currentThrottle = throttleOverride;
				CurrentSteerAngle = steerOverride * ActualMaxSteeringAngle;
			}
			else
			{
				UpdateThrottle();
				UpdateSteerAngle();
			}
			ApplySteerAngle();
		}
	}

	protected virtual void FixedUpdate()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsSpawned)
		{
			return;
		}
		UpdatePhysicallySimulated();
		UpdateSpeedCalculation();
		SetObstaclesActive(Speed_Kmh < 5f);
		if (IsPhysicallySimulated)
		{
			foreach (Wheel wheel in wheels)
			{
				wheel.FixedUpdateWheel();
			}
			ApplyThrottle();
			UpdateOutOfBounds();
			ApplyDownForce();
			if (LocalPlayerIsDriver)
			{
				UpdateTurnOver();
			}
		}
		else
		{
			if (!PlayerSingleton<PlayerCamera>.InstanceExists || !(Vector3.SqrMagnitude(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position - ((Component)this).transform.position) < 400f))
			{
				return;
			}
			foreach (Wheel wheel2 in wheels)
			{
				wheel2.FakeWheelRotation();
			}
		}
	}

	private void LateUpdate()
	{
		handbrakeOverride = false;
	}

	private void UpdateSpeedCalculation()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		float value = ((Component)this).transform.InverseTransformDirection(((Component)this).transform.position - lastFramePosition).z / Time.fixedDeltaTime * 3.6f;
		previousSpeeds.Add(value);
		if (!LocalPlayerIsDriver)
		{
			Speed_Kmh = previousSpeeds.Average;
		}
		else
		{
			Speed_Kmh = ((Component)this).transform.InverseTransformDirection(Rb.velocity).z * 3.6f;
		}
		lastFramePosition = ((Component)this).transform.position;
	}

	private void UpdateOutOfBounds()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		if ((((NetworkBehaviour)this).IsOwner || (((NetworkBehaviour)this).OwnerId == -1 && InstanceFinder.IsHost)) && ((Component)this).transform.position.y < -20f)
		{
			if ((Object)(object)Rb != (Object)null)
			{
				Rb.velocity = Vector3.zero;
				Rb.angularVelocity = Vector3.zero;
			}
			if (MapHeightSampler.TrySample(((Component)this).transform.position.x, ((Component)this).transform.position.z, out var hitPoint))
			{
				SetTransform(new Vector3(((Component)this).transform.position.x, hitPoint.y + 3f, ((Component)this).transform.position.z), Quaternion.identity);
			}
			else
			{
				SetTransform(Vector3.zero, Quaternion.identity);
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (onCollision != null)
		{
			onCollision(collision);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	protected virtual void SetOwner(NetworkConnection conn)
	{
		RpcWriter___Server_SetOwner_328543758(conn);
	}

	[ObserversRpc]
	protected virtual void OnOwnerChanged()
	{
		RpcWriter___Observers_OnOwnerChanged_2166136261();
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetTransform_Server(Vector3 pos, Quaternion rot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_SetTransform_Server_3848837105(pos, rot);
		RpcLogic___SetTransform_Server_3848837105(pos, rot);
	}

	[ObserversRpc(RunLocally = true)]
	public void SetTransform(Vector3 pos, Quaternion rot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_SetTransform_3848837105(pos, rot);
		RpcLogic___SetTransform_3848837105(pos, rot);
	}

	public void DestroyVehicle()
	{
		if (!InstanceFinder.IsServer)
		{
			Console.LogWarning("DestroyVehicle called on client!");
			return;
		}
		if (IsOccupied)
		{
			Console.LogError("Can't destroy vehicle while occupied.", (Object)(object)((Component)this).gameObject);
			return;
		}
		if (isParked)
		{
			ExitPark_Networked(null, moveToExitPoint: false);
		}
		if ((Object)(object)HumanoidColliderContainer != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)HumanoidColliderContainer).gameObject);
		}
		((NetworkBehaviour)this).Despawn((DespawnType?)null);
	}

	protected virtual void UpdateThrottle()
	{
		currentThrottle = 0f;
		if (LocalPlayerIsDriver)
		{
			currentThrottle = GameInput.VehicleDriveAxis;
			if (DriverPlayer.IsTased)
			{
				currentThrottle = 0f;
			}
		}
	}

	protected virtual void ApplyThrottle()
	{
		bool flag = false;
		bool isBraking = false;
		bool flag2 = false;
		if (LocalPlayerIsDriver || overrideControls)
		{
			foreach (Wheel wheel in wheels)
			{
				wheel.wheelCollider.motorTorque = 0.0001f;
				wheel.wheelCollider.brakeTorque = 0f;
			}
			if (LocalPlayerIsDriver)
			{
				flag = GameInput.GetButton(GameInput.ButtonCode.VehicleHandbrake);
			}
			if (overrideControls)
			{
				flag = handbrakeOverride;
			}
			if (flag && Mathf.Abs(Speed_Kmh) > 4f)
			{
				isBraking = true;
			}
			if (currentThrottle != 0f && (Mathf.Abs(Speed_Kmh) < 4f || Mathf.Sign(Speed_Kmh) == Mathf.Sign(currentThrottle)))
			{
				if (Speed_Kmh < -0.1f && currentThrottle < 0f)
				{
					flag2 = true;
				}
				float num = motorTorque.Evaluate(Mathf.Abs(Speed_Kmh));
				if (flag2)
				{
					num = motorTorque.Evaluate(Mathf.Abs(Speed_Kmh) / reverseMultiplier);
				}
				if (DEBUG)
				{
					Console.Log("Applying throttle: " + currentThrottle);
				}
				WheelCollider[] array = driveWheels;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].motorTorque = currentThrottle * num * diffGearing / 2f;
				}
			}
			else if (currentThrottle != 0f)
			{
				if (Mathf.Abs(currentThrottle) > 0.05f)
				{
					isBraking = true;
				}
				foreach (Wheel wheel2 in wheels)
				{
					float brakeTorque = Mathf.Abs(currentThrottle) * brakeForce.Evaluate(Mathf.Clamp01(Mathf.Abs(Speed_Kmh / TopSpeed))) * BrakeForceMultiplier * Rb.mass;
					wheel2.wheelCollider.brakeTorque = brakeTorque;
				}
			}
			SetIsBraking(isBraking);
			SetIsReversing(flag2);
		}
		else
		{
			foreach (Wheel wheel3 in wheels)
			{
				wheel3.wheelCollider.motorTorque = 0f;
			}
			if (!IsOccupied)
			{
				flag = true;
			}
		}
		if (!IsOccupied && InstanceFinder.IsServer)
		{
			SetIsBraking(braking: false);
			SetIsReversing(reversing: false);
		}
		if (flag)
		{
			if (!HandbrakeApplied && onHandbrakeApplied != null)
			{
				onHandbrakeApplied();
			}
			WheelCollider[] array = handbrakeWheels;
			foreach (WheelCollider obj in array)
			{
				obj.motorTorque = 0f;
				obj.brakeTorque = handBrakeForce;
			}
		}
		HandbrakeApplied = flag;
	}

	private void ApplyDownForce()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Rb.AddForce(-((Component)this).transform.up * Speed_Kmh * downforce);
	}

	private void UpdateTurnOver()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if (!LocalPlayerIsDriver || !(Mathf.Abs(Speed_Kmh) < 5f))
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < wheels.Count; i++)
		{
			if (!wheels[i].IsWheelGrounded())
			{
				num++;
			}
		}
		if (num >= 2)
		{
			Rb.AddRelativeTorque(Vector3.forward * 8f * (0f - Mathf.Clamp(SyncAccessor__003CCurrentSteerAngle_003Ek__BackingField / ActualMaxSteeringAngle, -1f, 1f)), (ForceMode)5);
		}
	}

	protected virtual void UpdateSteerAngle()
	{
		if (!LocalPlayerIsDriver)
		{
			return;
		}
		CurrentSteerAngle = lastFrameSteerAngle;
		if (DriverPlayer.IsTased || Player.Local.Seizure)
		{
			CurrentSteerAngle = Mathf.MoveTowards(SyncAccessor__003CCurrentSteerAngle_003Ek__BackingField, Random.Range(0f - ActualMaxSteeringAngle, ActualMaxSteeringAngle), steerRate * Time.deltaTime);
		}
		else
		{
			float num = 1f;
			if (Player.Local.Disoriented)
			{
				num = -1f;
			}
			if (Mathf.Abs(GameInput.MotionAxis.x) > 0.001f)
			{
				CurrentSteerAngle = Mathf.Clamp(SyncAccessor__003CCurrentSteerAngle_003Ek__BackingField + GameInput.MotionAxis.x * steerRate * Time.deltaTime * num, 0f - ActualMaxSteeringAngle, ActualMaxSteeringAngle);
			}
			else
			{
				CurrentSteerAngle = Mathf.MoveTowards(SyncAccessor__003CCurrentSteerAngle_003Ek__BackingField, 0f, steerRate * Time.deltaTime);
			}
		}
		if (Mathf.Abs(lastReplicatedSteerAngle - SyncAccessor__003CCurrentSteerAngle_003Ek__BackingField) > 3f)
		{
			lastReplicatedSteerAngle = SyncAccessor__003CCurrentSteerAngle_003Ek__BackingField;
			SetSteeringAngle(SyncAccessor__003CCurrentSteerAngle_003Ek__BackingField);
		}
		lastFrameSteerAngle = SyncAccessor__003CCurrentSteerAngle_003Ek__BackingField;
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetSteeringAngle(float sa)
	{
		RpcWriter___Server_SetSteeringAngle_431000436(sa);
	}

	private void SetIsBraking(bool braking)
	{
		if (SyncAccessor__003CBrakesApplied_003Ek__BackingField != braking)
		{
			SetIsBreaking_Server(braking);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void SetIsBreaking_Server(bool braking)
	{
		RpcWriter___Server_SetIsBreaking_Server_1140765316(braking);
		RpcLogic___SetIsBreaking_Server_1140765316(braking);
	}

	private void SetIsReversing(bool reversing)
	{
		if (SyncAccessor__003CIsReversing_003Ek__BackingField != reversing)
		{
			SetIsReversing_Server(reversing);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void SetIsReversing_Server(bool reversing)
	{
		RpcWriter___Server_SetIsReversing_Server_1140765316(reversing);
		RpcLogic___SetIsReversing_Server_1140765316(reversing);
	}

	protected virtual void ApplySteerAngle()
	{
		float num = SyncAccessor__003CCurrentSteerAngle_003Ek__BackingField;
		if (flipSteer)
		{
			num *= -1f;
		}
		WheelCollider[] array = steerWheels;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].steerAngle = num;
		}
	}

	public void AlignTo(Transform target, EParkingAlignment type, bool network = false)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (DEBUG)
		{
			Debug.Log((object)("Aligning vehicle to target: " + ((Object)target).name), (Object)(object)this);
		}
		Tuple<Vector3, Quaternion> alignmentTransform = GetAlignmentTransform(target, type);
		((Component)this).transform.rotation = alignmentTransform.Item2;
		((Component)this).transform.position = alignmentTransform.Item1;
		Rb.position = alignmentTransform.Item1;
		Rb.rotation = alignmentTransform.Item2;
		if (network)
		{
			SetTransform_Server(alignmentTransform.Item1, alignmentTransform.Item2);
		}
	}

	public Tuple<Vector3, Quaternion> GetAlignmentTransform(Transform target, EParkingAlignment type)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = target.rotation;
		if (type == EParkingAlignment.FrontToKerb)
		{
			val *= Quaternion.Euler(0f, 180f, 0f);
		}
		Vector3 val2 = target.position + target.up * (BoundingBoxDimensions.y / 2f - ((Component)boundingBox).transform.localPosition.y);
		val2 = ((type != EParkingAlignment.FrontToKerb) ? (val2 + target.forward * (BoundingBoxDimensions.z / 2f - ((Component)boundingBox).transform.localPosition.y)) : (val2 + target.forward * (BoundingBoxDimensions.z / 2f - ((Component)boundingBox).transform.localPosition.y)));
		return new Tuple<Vector3, Quaternion>(val2, val);
	}

	public float GetVehicleValue()
	{
		return VehiclePrice;
	}

	public void OverrideMaxSteerAngle(float maxAngle)
	{
		OverriddenMaxSteerAngle = maxAngle;
		MaxSteerAngleOverridden = true;
	}

	public void ResetMaxSteerAngle()
	{
		MaxSteerAngleOverridden = false;
	}

	public void SetObstaclesActive(bool active)
	{
		((Behaviour)NavMeshObstacle).enabled = true;
		if (NavMeshObstacle.carving != active)
		{
			NavMeshObstacle.carving = active;
		}
		((Behaviour)NavmeshCut).enabled = false;
	}

	private void UpdatePhysicallySimulated(bool forceApply = false)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		bool flag = !Rb.isKinematic;
		bool flag2 = ShouldBePhysicallySimulated();
		if (flag == flag2 && !forceApply)
		{
			return;
		}
		Rb.isKinematic = !flag2;
		Rb.interpolation = (RigidbodyInterpolation)(flag2 ? 1 : 0);
		foreach (Wheel wheel in wheels)
		{
			wheel.SetPhysicsEnabled(flag2);
		}
		if (!flag && flag2)
		{
			Rb.velocity = previousSpeeds.Average * ((Component)this).transform.forward / 3.6f;
		}
		IsPhysicallySimulated = flag2;
	}

	private bool ShouldBePhysicallySimulated()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (LocalPlayerIsDriver)
		{
			return true;
		}
		if (isParked)
		{
			return false;
		}
		if (((NetworkBehaviour)this).Owner != (NetworkConnection)null && ((NetworkBehaviour)this).Owner.ClientId != -1 && !((NetworkBehaviour)this).Owner.IsLocalClient)
		{
			return false;
		}
		if (!InstanceFinder.IsServer)
		{
			return false;
		}
		if (!IsVisible)
		{
			return false;
		}
		if (Time.timeSinceLevelLoad < 3f)
		{
			return false;
		}
		if (timeSinceSpawn < 5f)
		{
			return true;
		}
		if (timeSinceLastOccupied < 5f)
		{
			return true;
		}
		Player.GetClosestPlayer(((Component)this).transform.position, out var distance);
		if (distance < 30f * QualitySettings.lodBias)
		{
			return true;
		}
		return false;
	}

	public VehicleSeat GetFirstFreeSeat()
	{
		for (int i = 0; i < Seats.Length; i++)
		{
			if (!Seats[i].isOccupied)
			{
				return Seats[i];
			}
		}
		return null;
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetSeatOccupant(NetworkConnection conn, int seatIndex, NetworkConnection occupant)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetSeatOccupant_3428404692(conn, seatIndex, occupant);
			RpcLogic___SetSeatOccupant_3428404692(conn, seatIndex, occupant);
		}
		else
		{
			RpcWriter___Target_SetSeatOccupant_3428404692(conn, seatIndex, occupant);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void SetSeatOccupant_Server(int seatIndex, NetworkConnection conn)
	{
		RpcWriter___Server_SetSeatOccupant_Server_3266232555(seatIndex, conn);
		RpcLogic___SetSeatOccupant_Server_3266232555(seatIndex, conn);
	}

	private void Hovered()
	{
		if (!IsPlayerOwned)
		{
			intObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
		else if (CurrentPlayerOccupancy < Capacity)
		{
			intObj.SetMessage("Enter vehicle");
			intObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}
		else
		{
			intObj.SetMessage("Vehicle full");
			intObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
		}
	}

	private void Interacted()
	{
		if (!justExitedVehicle && IsPlayerOwned && CurrentPlayerOccupancy < Capacity)
		{
			EnterVehicle();
		}
	}

	private void StartVehicle()
	{
		if (onVehicleStart != null)
		{
			onVehicleStart();
		}
	}

	private void StopVehicle()
	{
		if (InstanceFinder.IsServer)
		{
			SetIsBraking(braking: false);
			SetIsReversing(reversing: false);
		}
		if (onVehicleStart != null)
		{
			onVehicleStop();
		}
	}

	private void EnterVehicle()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (!justExitedVehicle)
		{
			LocalPlayerIsInVehicle = true;
			IsOccupied = true;
			localPlayerSeat = GetFirstFreeSeat();
			LocalPlayerIsDriver = localPlayerSeat.isDriverSeat;
			SetSeatOccupant_Server(Array.IndexOf(Seats, localPlayerSeat), Player.Local.Connection);
			if (LocalPlayerIsDriver)
			{
				((NetworkBehaviour)this).NetworkObject.SetLocalOwnership(Player.Local.Connection);
				SetOwner(Player.Local.Connection);
			}
			closestExitPoint = GetClosestExitPoint(((Component)localPlayerSeat).transform.position);
			Player.Local.EnterVehicle(this, localPlayerSeat);
			PlayerSingleton<PlayerCamera>.Instance.SetCameraMode(PlayerCamera.ECameraMode.Vehicle);
			if (PlayerSingleton<PlayerInventory>.InstanceExists)
			{
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			}
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		}
	}

	public void ExitVehicle()
	{
		if (((NetworkBehaviour)this).IsOwner)
		{
			SetOwner(null);
			((Component)this).GetComponent<NetworkTransform>().ClearGoalData();
		}
		LocalPlayerIsInVehicle = false;
		LocalPlayerIsDriver = false;
		if ((Object)(object)localPlayerSeat != (Object)null)
		{
			SetSeatOccupant_Server(Array.IndexOf(Seats, localPlayerSeat), null);
			localPlayerSeat = null;
		}
		List<Transform> list = new List<Transform>();
		list.Add(closestExitPoint);
		list.AddRange(exitPoints);
		Transform validExitPoint = GetValidExitPoint(list);
		Player.Local.ExitVehicle(validExitPoint);
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f);
		PlayerSingleton<PlayerCamera>.Instance.ResetRotation();
		PlayerSingleton<PlayerCamera>.Instance.SetCameraMode(PlayerCamera.ECameraMode.Default);
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		justExitedVehicle = true;
		((MonoBehaviour)this).Invoke("EndJustExited", 0.05f);
	}

	private void EndJustExited()
	{
		justExitedVehicle = false;
	}

	public Transform GetExitPoint(int seatIndex = 0)
	{
		return exitPoints[Mathf.Clamp(seatIndex, 0, exitPoints.Count - 1)];
	}

	private Transform GetClosestExitPoint(Vector3 pos)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Transform val = null;
		for (int i = 0; i < exitPoints.Count; i++)
		{
			if ((Object)(object)val == (Object)null || Vector3.Distance(exitPoints[i].position, pos) < Vector3.Distance(((Component)val).transform.position, pos))
			{
				val = exitPoints[i];
			}
		}
		return val;
	}

	private Transform GetValidExitPoint(List<Transform> possibleExitPoints)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		LayerMask val = LayerMask.op_Implicit(LayerMask.op_Implicit(default(LayerMask)) | (1 << LayerMask.NameToLayer("Default")));
		val = LayerMask.op_Implicit(LayerMask.op_Implicit(val) | (1 << LayerMask.NameToLayer("Vehicle")));
		val = LayerMask.op_Implicit(LayerMask.op_Implicit(val) | (1 << LayerMask.NameToLayer("Terrain")));
		for (int i = 0; i < possibleExitPoints.Count; i++)
		{
			if (Physics.OverlapSphere(possibleExitPoints[i].position, 0.35f, LayerMask.op_Implicit(val)).Length == 0)
			{
				return possibleExitPoints[i];
			}
		}
		Console.LogWarning("Unable to find clear exit point for vehicle. Using first exit point.");
		return possibleExitPoints[0];
	}

	public void AddNPCOccupant(NPC npc)
	{
		if (OccupantNPCs.Contains(npc))
		{
			return;
		}
		int num = OccupantNPCs.Where((NPC x) => (Object)(object)x != (Object)null).Count();
		for (int num2 = 0; num2 < OccupantNPCs.Length; num2++)
		{
			if ((Object)(object)OccupantNPCs[num2] == (Object)null)
			{
				OccupantNPCs[num2] = npc;
				break;
			}
		}
		IsOccupied = true;
		if (num == 0)
		{
			StartVehicle();
		}
	}

	public void RemoveNPCOccupant(NPC npc)
	{
		for (int i = 0; i < OccupantNPCs.Length; i++)
		{
			if ((Object)(object)OccupantNPCs[i] == (Object)(object)npc)
			{
				OccupantNPCs[i] = null;
			}
		}
		if (OccupantNPCs.Where((NPC x) => (Object)(object)x != (Object)null).Count() == 0)
		{
			IsOccupied = false;
			StopVehicle();
		}
	}

	public virtual bool CanBeRecovered()
	{
		if (IsPlayerOwned)
		{
			return !IsOccupied;
		}
		return false;
	}

	public virtual void RecoverVehicle()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		VehicleRecoveryPoint closestRecoveryPoint = VehicleRecoveryPoint.GetClosestRecoveryPoint(((Component)this).transform.position);
		((Component)this).transform.position = ((Component)closestRecoveryPoint).transform.position + Vector3.up * 2f;
		((Component)this).transform.up = Vector3.up;
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendOwnedColor(EVehicleColor col)
	{
		RpcWriter___Server_SendOwnedColor_911055161(col);
		RpcLogic___SendOwnedColor_911055161(col);
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	protected virtual void SetOwnedColor(NetworkConnection conn, EVehicleColor col)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetOwnedColor_1679996372(conn, col);
			RpcLogic___SetOwnedColor_1679996372(conn, col);
		}
		else
		{
			RpcWriter___Target_SetOwnedColor_1679996372(conn, col);
		}
	}

	public void ApplyColor(EVehicleColor col)
	{
		Color.ApplyColor(col);
	}

	public void ApplyOwnedColor()
	{
		Color.ApplyColor(OwnedColor);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void Park_Networked(NetworkConnection conn, ParkData parkData)
	{
		if (conn == null)
		{
			RpcWriter___Observers_Park_Networked_2633993806(conn, parkData);
			RpcLogic___Park_Networked_2633993806(conn, parkData);
		}
		else
		{
			RpcWriter___Target_Park_Networked_2633993806(conn, parkData);
		}
	}

	public void Park(NetworkConnection conn, ParkData parkData, bool network)
	{
		if (isParked)
		{
			ExitPark();
		}
		if (network)
		{
			Park_Networked(conn, parkData);
			return;
		}
		CurrentParkingLot = GUIDManager.GetObject<ParkingLot>(parkData.lotGUID);
		if ((Object)(object)CurrentParkingLot == (Object)null)
		{
			Console.LogWarning("LandVehicle.Park: parking lot not found with the given GUID.");
			return;
		}
		CurrentParkData = parkData;
		UpdatePhysicallySimulated();
		if (parkData.spotIndex < 0 || parkData.spotIndex >= CurrentParkingLot.ParkingSpots.Count)
		{
			SetVisible(vis: false);
			return;
		}
		CurrentParkingSpot = CurrentParkingLot.ParkingSpots[parkData.spotIndex];
		CurrentParkingSpot.SetOccupant(this);
		AlignTo(CurrentParkingSpot.AlignmentPoint, parkData.alignment);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void ExitPark_Networked(NetworkConnection conn, bool moveToExitPoint = true)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ExitPark_Networked_214505783(conn, moveToExitPoint);
			RpcLogic___ExitPark_Networked_214505783(conn, moveToExitPoint);
		}
		else
		{
			RpcWriter___Target_ExitPark_Networked_214505783(conn, moveToExitPoint);
		}
	}

	public void ExitPark(bool moveToExitPoint = true)
	{
		if (!((Object)(object)CurrentParkingLot == (Object)null))
		{
			if ((Object)(object)CurrentParkingLot.ExitPoint != (Object)null && moveToExitPoint)
			{
				AlignTo(CurrentParkingLot.ExitPoint, CurrentParkingLot.ExitAlignment);
			}
			CurrentParkData = null;
			CurrentParkingLot = null;
			if ((Object)(object)CurrentParkingSpot != (Object)null)
			{
				CurrentParkingSpot.SetOccupant(null);
				CurrentParkingSpot = null;
			}
			SetVisible(vis: true);
		}
	}

	public void SetVisible(bool vis)
	{
		IsVisible = vis;
		vehicleModel.gameObject.SetActive(vis);
		((Component)HumanoidColliderContainer).gameObject.SetActive(vis);
		((Component)boundingBox).gameObject.SetActive(vis);
		UpdatePhysicallySimulated();
	}

	public void RegisterPusher(PlayerPusher pusher)
	{
		if (!pushers.Contains(pusher))
		{
			pushers.Add(pusher);
		}
	}

	public void DeregisterPusher(PlayerPusher pusher)
	{
		pushers.Remove(pusher);
	}

	public List<ItemInstance> GetContents()
	{
		List<ItemInstance> list = new List<ItemInstance>();
		if ((Object)(object)Storage != (Object)null)
		{
			list.AddRange(Storage.GetAllItems());
		}
		return list;
	}

	public virtual VehicleData GetVehicleData()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		return new VehicleData(GUID, vehicleCode, ((Component)this).transform.position, ((Component)this).transform.rotation, OwnedColor, GetContentsSet(), GetSpraySurfaceData());
	}

	protected List<SpraySurfaceData> GetSpraySurfaceData()
	{
		List<SpraySurfaceData> list = new List<SpraySurfaceData>();
		SpraySurface[] componentsInChildren = ((Component)this).GetComponentsInChildren<SpraySurface>();
		foreach (SpraySurface spraySurface in componentsInChildren)
		{
			list.Add(spraySurface.GetSaveData());
		}
		return list;
	}

	public string GetSaveString()
	{
		return GetVehicleData().GetJson();
	}

	private ItemSet GetContentsSet()
	{
		if ((Object)(object)Storage == (Object)null || Storage.ItemCount == 0)
		{
			return null;
		}
		return new ItemSet(Storage.ItemSlots);
	}

	public virtual void Load(VehicleData data, string containerPath)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		SetGUID(new Guid(data.GUID));
		SetTransform(data.Position, data.Rotation);
		SetOwnedColor(null, Enum.Parse<EVehicleColor>(data.Color));
		if ((Object)(object)Storage != (Object)null)
		{
			if (data.VehicleContents != null && data.VehicleContents.Items != null)
			{
				if (ItemSet.TryDeserialize(data.VehicleContents, out var itemSet))
				{
					itemSet.LoadTo(Storage.ItemSlots);
				}
			}
			else if (File.Exists(Path.Combine(containerPath, "Contents.json")))
			{
				Console.LogWarning("Loading legacy vehicle contents.");
				if (Loader.TryLoadFile(containerPath, "Contents", out var contents) && ItemSet.TryDeserialize(contents, out var itemSet2))
				{
					itemSet2.LoadTo(Storage.ItemSlots);
				}
			}
		}
		if (data.SpraySurfaces == null)
		{
			return;
		}
		for (int i = 0; i < data.SpraySurfaces.Count; i++)
		{
			if (_spraySurfaces.Length > i)
			{
				_spraySurfaces[i].Set(null, data.SpraySurfaces[i].Strokes.ToArray(), data.SpraySurfaces[i].ContainsCartelGraffiti);
			}
		}
	}

	public void OnWeatherChange(WeatherConditions newConditions)
	{
		foreach (Wheel wheel in wheels)
		{
			wheel.OnWeatherChange(newConditions);
		}
	}

	public void OnUpdateWeatherEntity()
	{
	}

	public override void NetworkInitialize___Early()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Expected O, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Expected O, but got Unknown
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Expected O, but got Unknown
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected O, but got Unknown
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Expected O, but got Unknown
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Expected O, but got Unknown
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Expected O, but got Unknown
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Expected O, but got Unknown
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Expected O, but got Unknown
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Expected O, but got Unknown
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Expected O, but got Unknown
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Expected O, but got Unknown
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EVehicles_002ELandVehicleAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EVehicles_002ELandVehicleAssembly_002DCSharp_002Edll_Excuted = true;
			syncVar____003CIsReversing_003Ek__BackingField = new SyncVar<bool>((NetworkBehaviour)(object)this, 2u, (WritePermission)1, (ReadPermission)0, 0.1f, (Channel)1, IsReversing);
			syncVar____003CBrakesApplied_003Ek__BackingField = new SyncVar<bool>((NetworkBehaviour)(object)this, 1u, (WritePermission)1, (ReadPermission)0, 0.1f, (Channel)1, BrakesApplied);
			syncVar____003CCurrentSteerAngle_003Ek__BackingField = new SyncVar<float>((NetworkBehaviour)(object)this, 0u, (WritePermission)1, (ReadPermission)0, 0.05f, (Channel)1, CurrentSteerAngle);
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_SetIsPlayerOwned_214505783));
			((NetworkBehaviour)this).RegisterTargetRpc(1u, new ClientRpcDelegate(RpcReader___Target_SetIsPlayerOwned_214505783));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_SetOwner_328543758));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_OnOwnerChanged_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(4u, new ServerRpcDelegate(RpcReader___Server_SetTransform_Server_3848837105));
			((NetworkBehaviour)this).RegisterObserversRpc(5u, new ClientRpcDelegate(RpcReader___Observers_SetTransform_3848837105));
			((NetworkBehaviour)this).RegisterServerRpc(6u, new ServerRpcDelegate(RpcReader___Server_SetSteeringAngle_431000436));
			((NetworkBehaviour)this).RegisterServerRpc(7u, new ServerRpcDelegate(RpcReader___Server_SetIsBreaking_Server_1140765316));
			((NetworkBehaviour)this).RegisterServerRpc(8u, new ServerRpcDelegate(RpcReader___Server_SetIsReversing_Server_1140765316));
			((NetworkBehaviour)this).RegisterObserversRpc(9u, new ClientRpcDelegate(RpcReader___Observers_SetSeatOccupant_3428404692));
			((NetworkBehaviour)this).RegisterTargetRpc(10u, new ClientRpcDelegate(RpcReader___Target_SetSeatOccupant_3428404692));
			((NetworkBehaviour)this).RegisterServerRpc(11u, new ServerRpcDelegate(RpcReader___Server_SetSeatOccupant_Server_3266232555));
			((NetworkBehaviour)this).RegisterServerRpc(12u, new ServerRpcDelegate(RpcReader___Server_SendOwnedColor_911055161));
			((NetworkBehaviour)this).RegisterTargetRpc(13u, new ClientRpcDelegate(RpcReader___Target_SetOwnedColor_1679996372));
			((NetworkBehaviour)this).RegisterObserversRpc(14u, new ClientRpcDelegate(RpcReader___Observers_SetOwnedColor_1679996372));
			((NetworkBehaviour)this).RegisterObserversRpc(15u, new ClientRpcDelegate(RpcReader___Observers_Park_Networked_2633993806));
			((NetworkBehaviour)this).RegisterTargetRpc(16u, new ClientRpcDelegate(RpcReader___Target_Park_Networked_2633993806));
			((NetworkBehaviour)this).RegisterObserversRpc(17u, new ClientRpcDelegate(RpcReader___Observers_ExitPark_Networked_214505783));
			((NetworkBehaviour)this).RegisterTargetRpc(18u, new ClientRpcDelegate(RpcReader___Target_ExitPark_Networked_214505783));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EVehicles_002ELandVehicle));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EVehicles_002ELandVehicleAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EVehicles_002ELandVehicleAssembly_002DCSharp_002Edll_Excuted = true;
			((SyncBase)syncVar____003CIsReversing_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CBrakesApplied_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CCurrentSteerAngle_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_SetIsPlayerOwned_214505783(NetworkConnection conn, bool playerOwned)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteBoolean(playerOwned);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetIsPlayerOwned_214505783(NetworkConnection conn, bool playerOwned)
	{
		IsPlayerOwned = playerOwned;
		StorageEntity storageEntity = default(StorageEntity);
		if (((Component)this).TryGetComponent<StorageEntity>(ref storageEntity))
		{
			storageEntity.AccessSettings = (playerOwned ? StorageEntity.EAccessSettings.Full : StorageEntity.EAccessSettings.Closed);
			foreach (ItemSlot itemSlot in storageEntity.ItemSlots)
			{
				itemSlot.SetFilterable(filterable: true);
			}
		}
		for (int i = 0; i < _spraySurfaces.Length; i++)
		{
			_spraySurfaces[i].Editable = true;
		}
		RefreshPoI();
	}

	private void RpcReader___Observers_SetIsPlayerOwned_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool playerOwned = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetIsPlayerOwned_214505783(null, playerOwned);
		}
	}

	private void RpcWriter___Target_SetIsPlayerOwned_214505783(NetworkConnection conn, bool playerOwned)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteBoolean(playerOwned);
			((NetworkBehaviour)this).SendTargetRpc(1u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetIsPlayerOwned_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool playerOwned = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetIsPlayerOwned_214505783(((NetworkBehaviour)this).LocalConnection, playerOwned);
		}
	}

	private void RpcWriter___Server_SetOwner_328543758(NetworkConnection conn)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	protected virtual void RpcLogic___SetOwner_328543758(NetworkConnection conn)
	{
		((NetworkBehaviour)this).NetworkObject.GiveOwnership(conn);
		OnOwnerChanged();
	}

	private void RpcReader___Server_SetOwner_328543758(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = ((Reader)PooledReader0).ReadNetworkConnection();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SetOwner_328543758(conn2);
		}
	}

	private void RpcWriter___Observers_OnOwnerChanged_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	protected virtual void RpcLogic___OnOwnerChanged_2166136261()
	{
		if (((NetworkBehaviour)this).NetworkObject.Owner == ((NetworkBehaviour)this).LocalConnection || (((NetworkBehaviour)this).NetworkObject.OwnerId == -1 && InstanceFinder.IsHost))
		{
			Console.Log("Local client owns vehicle");
		}
		else
		{
			Console.Log("Local client no longer owns vehicle");
		}
		UpdatePhysicallySimulated();
	}

	private void RpcReader___Observers_OnOwnerChanged_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___OnOwnerChanged_2166136261();
		}
	}

	private void RpcWriter___Server_SetTransform_Server_3848837105(Vector3 pos, Quaternion rot)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(pos);
			((Writer)writer).WriteQuaternion(rot, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(4u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetTransform_Server_3848837105(Vector3 pos, Quaternion rot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		SetTransform(pos, rot);
	}

	private void RpcReader___Server_SetTransform_Server_3848837105(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 pos = ((Reader)PooledReader0).ReadVector3();
		Quaternion rot = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetTransform_Server_3848837105(pos, rot);
		}
	}

	private void RpcWriter___Observers_SetTransform_3848837105(Vector3 pos, Quaternion rot)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(pos);
			((Writer)writer).WriteQuaternion(rot, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(5u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetTransform_3848837105(Vector3 pos, Quaternion rot)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = pos;
		((Component)this).transform.rotation = rot;
		Rb.position = pos;
		Rb.rotation = rot;
	}

	private void RpcReader___Observers_SetTransform_3848837105(PooledReader PooledReader0, Channel channel)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 pos = ((Reader)PooledReader0).ReadVector3();
		Quaternion rot = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetTransform_3848837105(pos, rot);
		}
	}

	private void RpcWriter___Server_SetSteeringAngle_431000436(float sa)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteSingle(sa, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(6u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SetSteeringAngle_431000436(float sa)
	{
		CurrentSteerAngle = sa;
	}

	private void RpcReader___Server_SetSteeringAngle_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		float sa = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SetSteeringAngle_431000436(sa);
		}
	}

	private void RpcWriter___Server_SetIsBreaking_Server_1140765316(bool braking)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteBoolean(braking);
			((NetworkBehaviour)this).SendServerRpc(7u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SetIsBreaking_Server_1140765316(bool braking)
	{
		BrakesApplied = braking;
	}

	private void RpcReader___Server_SetIsBreaking_Server_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool braking = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetIsBreaking_Server_1140765316(braking);
		}
	}

	private void RpcWriter___Server_SetIsReversing_Server_1140765316(bool reversing)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteBoolean(reversing);
			((NetworkBehaviour)this).SendServerRpc(8u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SetIsReversing_Server_1140765316(bool reversing)
	{
		IsReversing = reversing;
	}

	private void RpcReader___Server_SetIsReversing_Server_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool reversing = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetIsReversing_Server_1140765316(reversing);
		}
	}

	private void RpcWriter___Observers_SetSeatOccupant_3428404692(NetworkConnection conn, int seatIndex, NetworkConnection occupant)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(seatIndex, (AutoPackType)1);
			((Writer)writer).WriteNetworkConnection(occupant);
			((NetworkBehaviour)this).SendObserversRpc(9u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetSeatOccupant_3428404692(NetworkConnection conn, int seatIndex, NetworkConnection occupant)
	{
		Player occupant2 = Seats[seatIndex].Occupant;
		Player player = Player.GetPlayer(occupant);
		if ((Object)(object)occupant2 == (Object)(object)player)
		{
			return;
		}
		Seats[seatIndex].Occupant = player;
		IsOccupied = Seats.Count((VehicleSeat s) => s.isOccupied) > 0;
		if (seatIndex == 0)
		{
			if (occupant != (NetworkConnection)null)
			{
				StartVehicle();
			}
			else
			{
				StopVehicle();
			}
		}
	}

	private void RpcReader___Observers_SetSeatOccupant_3428404692(PooledReader PooledReader0, Channel channel)
	{
		int seatIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		NetworkConnection occupant = ((Reader)PooledReader0).ReadNetworkConnection();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetSeatOccupant_3428404692(null, seatIndex, occupant);
		}
	}

	private void RpcWriter___Target_SetSeatOccupant_3428404692(NetworkConnection conn, int seatIndex, NetworkConnection occupant)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(seatIndex, (AutoPackType)1);
			((Writer)writer).WriteNetworkConnection(occupant);
			((NetworkBehaviour)this).SendTargetRpc(10u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetSeatOccupant_3428404692(PooledReader PooledReader0, Channel channel)
	{
		int seatIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		NetworkConnection occupant = ((Reader)PooledReader0).ReadNetworkConnection();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetSeatOccupant_3428404692(((NetworkBehaviour)this).LocalConnection, seatIndex, occupant);
		}
	}

	private void RpcWriter___Server_SetSeatOccupant_Server_3266232555(int seatIndex, NetworkConnection conn)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(seatIndex, (AutoPackType)1);
			((Writer)writer).WriteNetworkConnection(conn);
			((NetworkBehaviour)this).SendServerRpc(11u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SetSeatOccupant_Server_3266232555(int seatIndex, NetworkConnection conn)
	{
		SetSeatOccupant(null, seatIndex, conn);
	}

	private void RpcReader___Server_SetSeatOccupant_Server_3266232555(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int seatIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		NetworkConnection conn2 = ((Reader)PooledReader0).ReadNetworkConnection();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetSeatOccupant_Server_3266232555(seatIndex, conn2);
		}
	}

	private void RpcWriter___Server_SendOwnedColor_911055161(EVehicleColor col)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EVehicles_002EModification_002EEVehicleColorFishNet_002ESerializing_002EGenerated((Writer)(object)writer, col);
			((NetworkBehaviour)this).SendServerRpc(12u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendOwnedColor_911055161(EVehicleColor col)
	{
		SetOwnedColor(null, col);
	}

	private void RpcReader___Server_SendOwnedColor_911055161(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		EVehicleColor col = GeneratedReaders___Internal.Read___ScheduleOne_002EVehicles_002EModification_002EEVehicleColorFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendOwnedColor_911055161(col);
		}
	}

	private void RpcWriter___Target_SetOwnedColor_1679996372(NetworkConnection conn, EVehicleColor col)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EVehicles_002EModification_002EEVehicleColorFishNet_002ESerializing_002EGenerated((Writer)(object)writer, col);
			((NetworkBehaviour)this).SendTargetRpc(13u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	protected virtual void RpcLogic___SetOwnedColor_1679996372(NetworkConnection conn, EVehicleColor col)
	{
		OwnedColor = col;
		ApplyOwnedColor();
		RefreshPoI();
	}

	private void RpcReader___Target_SetOwnedColor_1679996372(PooledReader PooledReader0, Channel channel)
	{
		EVehicleColor col = GeneratedReaders___Internal.Read___ScheduleOne_002EVehicles_002EModification_002EEVehicleColorFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetOwnedColor_1679996372(((NetworkBehaviour)this).LocalConnection, col);
		}
	}

	private void RpcWriter___Observers_SetOwnedColor_1679996372(NetworkConnection conn, EVehicleColor col)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EVehicles_002EModification_002EEVehicleColorFishNet_002ESerializing_002EGenerated((Writer)(object)writer, col);
			((NetworkBehaviour)this).SendObserversRpc(14u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_SetOwnedColor_1679996372(PooledReader PooledReader0, Channel channel)
	{
		EVehicleColor col = GeneratedReaders___Internal.Read___ScheduleOne_002EVehicles_002EModification_002EEVehicleColorFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetOwnedColor_1679996372(null, col);
		}
	}

	private void RpcWriter___Observers_Park_Networked_2633993806(NetworkConnection conn, ParkData parkData)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerated((Writer)(object)writer, parkData);
			((NetworkBehaviour)this).SendObserversRpc(15u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Park_Networked_2633993806(NetworkConnection conn, ParkData parkData)
	{
		Park(conn, parkData, network: false);
	}

	private void RpcReader___Observers_Park_Networked_2633993806(PooledReader PooledReader0, Channel channel)
	{
		ParkData parkData = GeneratedReaders___Internal.Read___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Park_Networked_2633993806(null, parkData);
		}
	}

	private void RpcWriter___Target_Park_Networked_2633993806(NetworkConnection conn, ParkData parkData)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerated((Writer)(object)writer, parkData);
			((NetworkBehaviour)this).SendTargetRpc(16u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_Park_Networked_2633993806(PooledReader PooledReader0, Channel channel)
	{
		ParkData parkData = GeneratedReaders___Internal.Read___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Park_Networked_2633993806(((NetworkBehaviour)this).LocalConnection, parkData);
		}
	}

	private void RpcWriter___Observers_ExitPark_Networked_214505783(NetworkConnection conn, bool moveToExitPoint = true)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteBoolean(moveToExitPoint);
			((NetworkBehaviour)this).SendObserversRpc(17u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___ExitPark_Networked_214505783(NetworkConnection conn, bool moveToExitPoint = true)
	{
		ExitPark(moveToExitPoint);
	}

	private void RpcReader___Observers_ExitPark_Networked_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool moveToExitPoint = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ExitPark_Networked_214505783(null, moveToExitPoint);
		}
	}

	private void RpcWriter___Target_ExitPark_Networked_214505783(NetworkConnection conn, bool moveToExitPoint = true)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteBoolean(moveToExitPoint);
			((NetworkBehaviour)this).SendTargetRpc(18u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ExitPark_Networked_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool moveToExitPoint = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ExitPark_Networked_214505783(((NetworkBehaviour)this).LocalConnection, moveToExitPoint);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EVehicles_002ELandVehicle(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		switch (UInt321)
		{
		case 2u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CIsReversing_003Ek__BackingField(syncVar____003CIsReversing_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			bool value3 = ((Reader)PooledReader0).ReadBoolean();
			this.sync___set_value__003CIsReversing_003Ek__BackingField(value3, Boolean2);
			return true;
		}
		case 1u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CBrakesApplied_003Ek__BackingField(syncVar____003CBrakesApplied_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			bool value2 = ((Reader)PooledReader0).ReadBoolean();
			this.sync___set_value__003CBrakesApplied_003Ek__BackingField(value2, Boolean2);
			return true;
		}
		case 0u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CCurrentSteerAngle_003Ek__BackingField(syncVar____003CCurrentSteerAngle_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			float value = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
			this.sync___set_value__003CCurrentSteerAngle_003Ek__BackingField(value, Boolean2);
			return true;
		}
		default:
			return false;
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EVehicles_002ELandVehicle_Assembly_002DCSharp_002Edll()
	{
		OccupantNPCs = new NPC[Seats.Length];
		((Component)boundingBox).gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		for (int i = 0; i < driveWheels.Length; i++)
		{
			wheels.Add(((Component)driveWheels[i]).GetComponent<Wheel>());
		}
		for (int j = 0; j < steerWheels.Length; j++)
		{
			if (!wheels.Contains(((Component)steerWheels[j]).GetComponent<Wheel>()))
			{
				wheels.Add(((Component)steerWheels[j]).GetComponent<Wheel>());
			}
			((Component)steerWheels[j]).GetComponent<Wheel>().IsSteerWheel = true;
		}
		for (int k = 0; k < _spraySurfaces.Length; k++)
		{
			_spraySurfaces[k].Editable = false;
		}
		Collider[] componentsInChildren = ((Component)this).GetComponentsInChildren<Collider>(true);
		Collider[] array = componentsInChildren;
		foreach (Collider val in array)
		{
			for (int m = 0; m < componentsInChildren.Length; m++)
			{
				Physics.IgnoreCollision(val, componentsInChildren[m], true);
			}
		}
		OwnedColor = Color.DefaultColor;
		InitializeSaveable();
		if ((Object)(object)((Component)this).GetComponent<StorageEntity>() != (Object)null)
		{
			((Component)this).GetComponent<StorageEntity>().AccessSettings = StorageEntity.EAccessSettings.Closed;
		}
		RefreshPoI();
		timeOnSpawn = Time.timeSinceLevelLoad;
	}
}
