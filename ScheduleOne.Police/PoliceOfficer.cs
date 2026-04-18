using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.FX;
using ScheduleOne.Law;
using ScheduleOne.Map;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using ScheduleOne.Vision;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Police;

public class PoliceOfficer : NPC
{
	public const float DEACTIVATION_TIME = 1f;

	public const float INVESTIGATION_COOLDOWN = 60f;

	public const float INVESTIGATION_MAX_DISTANCE = 8f;

	public const float INVESTIGATION_MIN_VISIBILITY = 0.2f;

	public const float INVESTIGATION_CHECK_INTERVAL = 1f;

	public const float BODY_SEARCH_CHANCE_DEFAULT = 0.1f;

	public const float MIN_CHATTER_INTERVAL = 15f;

	public const float MAX_CHATTER_INTERVAL = 45f;

	public static Action<VisionEventReceipt> OnPoliceVisionEvent;

	public static List<PoliceOfficer> Officers = new List<PoliceOfficer>();

	[CompilerGenerated]
	[SyncVar]
	[HideInInspector]
	public bool _003CIgnorePlayers_003Ek__BackingField;

	[Header("References")]
	public PursuitBehaviour PursuitBehaviour;

	public VehiclePursuitBehaviour VehiclePursuitBehaviour;

	public BodySearchBehaviour BodySearchBehaviour;

	public CheckpointBehaviour CheckpointBehaviour;

	public FootPatrolBehaviour FootPatrolBehaviour;

	public ProximityCircle ProxCircle;

	public VehiclePatrolBehaviour VehiclePatrolBehaviour;

	public SentryBehaviour SentryBehaviour;

	public PoliceChatterVO ChatterVO;

	public Behaviour[] DeactivationBlockingBehaviours;

	[Header("Dialogue")]
	public DialogueContainer CheckpointDialogue;

	[Header("Tools")]
	public AvatarEquippable BatonPrefab;

	public AvatarEquippable TaserPrefab;

	public AvatarEquippable GunPrefab;

	[Header("Settings")]
	public bool AutoDeactivate = true;

	public bool ChatterEnabled = true;

	[Header("Behaviour Settings")]
	[Range(0f, 1f)]
	public float Suspicion = 0.5f;

	[Range(0f, 1f)]
	public float Leniency = 0.5f;

	[Header("Body Search Settings")]
	[Range(0f, 1f)]
	public float BodySearchChance = 0.1f;

	[Range(1f, 10f)]
	public float BodySearchDuration = 5f;

	[HideInInspector]
	public PoliceBelt belt;

	private float timeSinceReadyToPool;

	private float timeSinceOutOfSight;

	private float chatterCountDown;

	private Investigation currentBodySearchInvestigation;

	public SyncVar<bool> syncVar____003CIgnorePlayers_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EPolice_002EPoliceOfficerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EPolice_002EPoliceOfficerAssembly_002DCSharp_002Edll_Excuted;

	public bool IgnorePlayers
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CIgnorePlayers_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			this.sync___set_value__003CIgnorePlayers_003Ek__BackingField(value, true);
		}
	}

	public NetworkObject PursuitTarget => PursuitBehaviour.Target?.NetworkObject;

	public LandVehicle AssignedVehicle { get; set; }

	public bool SyncAccessor__003CIgnorePlayers_003Ek__BackingField
	{
		get
		{
			return IgnorePlayers;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				IgnorePlayers = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CIgnorePlayers_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EPolice_002EPoliceOfficer_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		base.Start();
		belt = ((Component)Avatar).GetComponentInChildren<PoliceBelt>();
		if (InstanceFinder.IsServer)
		{
			SetRandomAvoidancePriority();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Officers.Contains(this))
		{
			Officers.Remove(this);
		}
	}

	protected void Update()
	{
		if (InstanceFinder.IsServer)
		{
			UpdateBodySearch();
		}
		UpdateVision();
		UpdateChatter();
	}

	protected override void OnTick()
	{
		base.OnTick();
		if ((Object)(object)base.CurrentBuilding == (Object)null && InstanceFinder.IsServer && AutoDeactivate)
		{
			CheckDeactivation();
		}
	}

	private void UpdateVision()
	{
		for (int i = 0; i < Player.PlayerList.Count; i++)
		{
			if (SyncAccessor__003CIgnorePlayers_003Ek__BackingField)
			{
				Awareness.VisionCone.SetNoticePlayerCrimes(Player.PlayerList[i], active: false);
				Awareness.VisionCone.SetSightableStateEnabled(Player.PlayerList[i], EVisualState.Suspicious, enabled: false);
				Awareness.VisionCone.SetSightableStateEnabled(Player.PlayerList[i], EVisualState.Wanted, enabled: false);
				continue;
			}
			Awareness.VisionCone.SetNoticePlayerCrimes(Player.PlayerList[i], ShouldNoticeGeneralCrime(Player.PlayerList[i]));
			Awareness.VisionCone.SetSightableStateEnabled(Player.PlayerList[i], EVisualState.Suspicious, !Player.PlayerList[i].CrimeData.BodySearchPending && Player.PlayerList[i].CrimeData.TimeSinceLastBodySearch > 30f);
			bool enabled = true;
			if (PursuitBehaviour.Enabled && PursuitBehaviour.IsTargetRecentlyVisible)
			{
				enabled = false;
			}
			if (VehiclePursuitBehaviour.Enabled && VehiclePursuitBehaviour.IsTargetRecentlyVisible)
			{
				enabled = false;
			}
			Awareness.VisionCone.SetSightableStateEnabled(Player.PlayerList[i], EVisualState.Wanted, enabled);
		}
	}

	private void CheckDeactivation()
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (PursuitBehaviour.Active)
		{
			timeSinceReadyToPool = 0f;
			timeSinceOutOfSight = 0f;
			return;
		}
		if ((Object)(object)Behaviour.ScheduleManager.ActiveAction != (Object)null)
		{
			timeSinceReadyToPool = 0f;
			timeSinceOutOfSight = 0f;
			return;
		}
		Behaviour[] deactivationBlockingBehaviours = DeactivationBlockingBehaviours;
		for (int i = 0; i < deactivationBlockingBehaviours.Length; i++)
		{
			if (deactivationBlockingBehaviours[i].Active)
			{
				timeSinceReadyToPool = 0f;
				timeSinceOutOfSight = 0f;
				return;
			}
		}
		if (!base.IsConscious)
		{
			timeSinceReadyToPool = 0f;
			timeSinceOutOfSight = 0f;
			return;
		}
		timeSinceReadyToPool += 0.5f;
		if (timeSinceReadyToPool < 1f)
		{
			return;
		}
		if (!Movement.IsMoving && Movement.CanMove() && Singleton<ScheduleOne.Map.Map>.InstanceExists)
		{
			if (Movement.IsAsCloseAsPossible(((Component)Singleton<ScheduleOne.Map.Map>.Instance.PoliceStation.Doors[0]).transform.position, 1f))
			{
				Deactivate();
				return;
			}
			if (Movement.CanGetTo(((Component)Singleton<ScheduleOne.Map.Map>.Instance.PoliceStation.Doors[0]).transform.position))
			{
				Movement.SetDestination(((Component)Singleton<ScheduleOne.Map.Map>.Instance.PoliceStation.Doors[0]).transform.position);
			}
			else
			{
				Deactivate();
			}
		}
		bool flag = false;
		foreach (Player player in Player.PlayerList)
		{
			if (player.IsPointVisibleToPlayer(Avatar.CenterPoint))
			{
				flag = true;
				break;
			}
			if ((Object)(object)AssignedVehicle != (Object)null && player.IsPointVisibleToPlayer(((Component)AssignedVehicle).transform.position))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			timeSinceReadyToPool += 0.5f;
			timeSinceOutOfSight += 0.5f;
			if (timeSinceOutOfSight > 1f)
			{
				Deactivate();
			}
		}
		else
		{
			timeSinceOutOfSight = 0f;
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public virtual void BeginFootPursuit_Networked(string playerCode, bool includeColleagues = true)
	{
		RpcWriter___Server_BeginFootPursuit_Networked_310431262(playerCode, includeColleagues);
		RpcLogic___BeginFootPursuit_Networked_310431262(playerCode, includeColleagues);
	}

	[ObserversRpc(RunLocally = true)]
	private void BeginFootPursuit(string playerCode)
	{
		RpcWriter___Observers_BeginFootPursuit_3615296227(playerCode);
		RpcLogic___BeginFootPursuit_3615296227(playerCode);
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public virtual void BeginVehiclePursuit_Networked(string playerCode, NetworkObject vehicle, bool beginAsSighted)
	{
		RpcWriter___Server_BeginVehiclePursuit_Networked_1834136777(playerCode, vehicle, beginAsSighted);
		RpcLogic___BeginVehiclePursuit_Networked_1834136777(playerCode, vehicle, beginAsSighted);
	}

	[ObserversRpc(RunLocally = true)]
	private void BeginVehiclePursuit(string playerCode, NetworkObject vehicle, bool beginAsSighted)
	{
		RpcWriter___Observers_BeginVehiclePursuit_1834136777(playerCode, vehicle, beginAsSighted);
		RpcLogic___BeginVehiclePursuit_1834136777(playerCode, vehicle, beginAsSighted);
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public virtual void BeginBodySearch_Networked(string playerCode)
	{
		RpcWriter___Server_BeginBodySearch_Networked_3615296227(playerCode);
		RpcLogic___BeginBodySearch_Networked_3615296227(playerCode);
	}

	[ObserversRpc(RunLocally = true)]
	private void BeginBodySearch(string playerCode)
	{
		RpcWriter___Observers_BeginBodySearch_3615296227(playerCode);
		RpcLogic___BeginBodySearch_3615296227(playerCode);
	}

	[ObserversRpc(RunLocally = true)]
	public virtual void AssignToCheckpoint(CheckpointManager.ECheckpointLocation location)
	{
		RpcWriter___Observers_AssignToCheckpoint_4087078542(location);
		RpcLogic___AssignToCheckpoint_4087078542(location);
	}

	public void UnassignFromCheckpoint()
	{
		CheckpointBehaviour.Disable_Networked(null);
		((Component)DialogueHandler).GetComponent<DialogueController>().OverrideContainer = null;
	}

	public void StartFootPatrol(PatrolGroup group, bool warpToStartPoint)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		FootPatrolBehaviour.SetGroup(group);
		FootPatrolBehaviour.Enable_Networked();
		if (warpToStartPoint)
		{
			Movement.Warp(group.GetDestination(this));
		}
	}

	public void StartVehiclePatrol(VehiclePatrolRoute route, LandVehicle vehicle)
	{
		VehiclePatrolBehaviour.Vehicle = vehicle;
		VehiclePatrolBehaviour.SetRoute(route);
		VehiclePatrolBehaviour.Enable_Networked();
	}

	public virtual void AssignToSentryLocation(SentryLocation location)
	{
		SentryBehaviour.AssignLocation(location);
		SentryBehaviour.Enable();
	}

	public void UnassignFromSentryLocation()
	{
		SentryBehaviour.UnassignLocation();
		SentryBehaviour.Disable();
	}

	public void Activate()
	{
		timeSinceReadyToPool = 0f;
		timeSinceOutOfSight = 0f;
		ExitBuilding();
	}

	public void Deactivate()
	{
		if (!InstanceFinder.IsServer)
		{
			Console.LogError("Attempted to deactivate an officer on the client");
			return;
		}
		if ((Object)(object)AssignedVehicle != (Object)null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Wait());
		}
		EnterBuilding(null, Singleton<ScheduleOne.Map.Map>.Instance.PoliceStation.GUID.ToString(), 0);
		IEnumerator Wait()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => !AssignedVehicle.IsOccupied));
			AssignedVehicle.DestroyVehicle();
		}
	}

	protected bool ShouldNoticeGeneralCrime(Player player)
	{
		if (player.CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None)
		{
			return false;
		}
		if (!player.Health.IsAlive || player.IsArrested || player.IsUnconscious)
		{
			return false;
		}
		if (PursuitBehaviour.Enabled && PursuitBehaviour.Target != null)
		{
			return false;
		}
		if (VehiclePursuitBehaviour.Enabled && (Object)(object)VehiclePursuitBehaviour.Target != (Object)null)
		{
			return false;
		}
		return true;
	}

	public override bool ShouldSave()
	{
		return false;
	}

	public override string GetNameAddress()
	{
		return "Officer " + LastName;
	}

	private void UpdateChatter()
	{
		chatterCountDown -= Time.deltaTime;
		if (chatterCountDown <= 0f)
		{
			chatterCountDown = Random.Range(15f, 45f);
			if (ChatterEnabled && ((Component)ChatterVO).gameObject.activeInHierarchy)
			{
				ChatterVO.Play(EVOLineType.PoliceChatter);
			}
		}
	}

	private void ProcessVisionEvent(VisionEventReceipt visionEventReceipt)
	{
		if (OnPoliceVisionEvent != null)
		{
			OnPoliceVisionEvent(visionEventReceipt);
		}
	}

	public static PoliceOfficer GetNearestOfficer(Vector3 position, out float distanceToTarget, bool onlyConscious = true)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		PoliceOfficer result = null;
		float num = float.MaxValue;
		for (int i = 0; i < Officers.Count; i++)
		{
			if (!onlyConscious || Officers[i].IsConscious)
			{
				float num2 = Vector3.Distance(position, Officers[i].Avatar.CenterPoint);
				if (num2 < num)
				{
					num = num2;
					result = Officers[i];
				}
			}
		}
		distanceToTarget = num;
		return result;
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetIgnorePlayers(bool ignore)
	{
		RpcWriter___Server_SetIgnorePlayers_1140765316(ignore);
	}

	public void SetRandomAvoidancePriority()
	{
		Movement.Agent.avoidancePriority = Random.Range(0, 50);
	}

	public void SetAvoidancePriority(int priority)
	{
		Movement.Agent.avoidancePriority = priority;
	}

	public virtual void UpdateBodySearch()
	{
		if (InstanceFinder.IsServer && CanInvestigate() && currentBodySearchInvestigation != null)
		{
			UpdateExistingInvestigation();
		}
	}

	private bool CanInvestigate()
	{
		if (VehiclePursuitBehaviour.Active || PursuitBehaviour.Active || BodySearchBehaviour.Active)
		{
			return false;
		}
		if ((Object)(object)base.CurrentBuilding != (Object)null)
		{
			return false;
		}
		if (SyncAccessor__003CIgnorePlayers_003Ek__BackingField)
		{
			return false;
		}
		return true;
	}

	private void UpdateExistingInvestigation()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		if (!CanInvestigatePlayer(currentBodySearchInvestigation.Target))
		{
			StopBodySearchInvestigation();
			return;
		}
		Player target = currentBodySearchInvestigation.Target;
		float playerVisibility = Awareness.VisionCone.GetPlayerVisibility(target);
		float suspiciousness = target.VisualState.Suspiciousness;
		float num = Mathf.Lerp(0.2f, 2f, suspiciousness);
		float num2 = Mathf.Lerp(0.4f, 1f, playerVisibility);
		float num3 = Mathf.Lerp(1f, 0.05f, Vector3.Distance(Avatar.CenterPoint, target.Avatar.CenterPoint) / 12f);
		float num4 = num2 * num * num3;
		if (Application.isEditor && Input.GetKey((KeyCode)98))
		{
			num4 = 0.5f;
		}
		if (num4 < 0.08f)
		{
			num4 = -0.08f;
		}
		else if (num4 < 0.12f)
		{
			num4 = 0f;
		}
		currentBodySearchInvestigation.ChangeProgress(num4 * Time.deltaTime);
		if (currentBodySearchInvestigation.CurrentProgress >= 1f)
		{
			ConductBodySearch(currentBodySearchInvestigation.Target);
			StopBodySearchInvestigation();
		}
		else if (currentBodySearchInvestigation.CurrentProgress <= -0.1f)
		{
			StopBodySearchInvestigation();
		}
		else if (currentBodySearchInvestigation.CurrentProgress >= 0f)
		{
			float speed = Mathf.Lerp(0.05f, 0f, currentBodySearchInvestigation.CurrentProgress);
			Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("consideringbodysearch", 5, speed));
			Avatar.LookController.OverrideLookTarget(target.EyePosition, 10, currentBodySearchInvestigation.CurrentProgress >= 0.2f);
		}
	}

	private void CheckNewInvestigation()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (currentBodySearchInvestigation != null || !CanInvestigate() || BodySearchChance <= 0f)
		{
			return;
		}
		foreach (Player player in Player.PlayerList)
		{
			if (!CanInvestigatePlayer(player) || Vector3.Distance(Avatar.CenterPoint, player.Avatar.CenterPoint) > 8f)
			{
				continue;
			}
			float playerVisibility = Awareness.VisionCone.GetPlayerVisibility(player);
			if (!(playerVisibility < 0.2f))
			{
				float suspiciousness = player.VisualState.Suspiciousness;
				float num = Mathf.Lerp(0.2f, 2f, suspiciousness);
				float num2 = Mathf.Lerp(0.4f, 1f, playerVisibility);
				float num3 = Mathf.Lerp(0.5f, 1f, Suspicion);
				float num4 = Mathf.Clamp01(BodySearchChance * num * num2 * num3 * 1f);
				if (Random.Range(0f, 1f) < num4)
				{
					VoiceOverEmitter.Play(EVOLineType.Think);
					currentBodySearchInvestigation = new Investigation(player);
					break;
				}
			}
		}
	}

	private void StopBodySearchInvestigation()
	{
		currentBodySearchInvestigation = null;
		Movement.SpeedController.RemoveSpeedControl("consideringbodysearch");
	}

	public void BodySearchLocalPlayer()
	{
		ConductBodySearch(Player.Local);
	}

	public void ConductBodySearch(Player player)
	{
		Console.Log("Conducting body search on " + player.PlayerName);
		BodySearchBehaviour.AssignTarget(null, ((NetworkBehaviour)player).NetworkObject);
		BodySearchBehaviour.Enable_Networked();
	}

	private bool CanInvestigatePlayer(Player player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!player.Health.IsAlive)
		{
			return false;
		}
		if (player.CrimeData.BodySearchPending)
		{
			return false;
		}
		if (player.CrimeData.CurrentPursuitLevel > PlayerCrimeData.EPursuitLevel.None)
		{
			return false;
		}
		if (player.CrimeData.TimeSinceLastBodySearch < 60f)
		{
			return false;
		}
		if (player.IsArrested)
		{
			return false;
		}
		return true;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EPolice_002EPoliceOfficerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EPolice_002EPoliceOfficerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CIgnorePlayers_003Ek__BackingField = new SyncVar<bool>((NetworkBehaviour)(object)this, 1u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, IgnorePlayers);
			((NetworkBehaviour)this).RegisterServerRpc(39u, new ServerRpcDelegate(RpcReader___Server_BeginFootPursuit_Networked_310431262));
			((NetworkBehaviour)this).RegisterObserversRpc(40u, new ClientRpcDelegate(RpcReader___Observers_BeginFootPursuit_3615296227));
			((NetworkBehaviour)this).RegisterServerRpc(41u, new ServerRpcDelegate(RpcReader___Server_BeginVehiclePursuit_Networked_1834136777));
			((NetworkBehaviour)this).RegisterObserversRpc(42u, new ClientRpcDelegate(RpcReader___Observers_BeginVehiclePursuit_1834136777));
			((NetworkBehaviour)this).RegisterServerRpc(43u, new ServerRpcDelegate(RpcReader___Server_BeginBodySearch_Networked_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(44u, new ClientRpcDelegate(RpcReader___Observers_BeginBodySearch_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(45u, new ClientRpcDelegate(RpcReader___Observers_AssignToCheckpoint_4087078542));
			((NetworkBehaviour)this).RegisterServerRpc(46u, new ServerRpcDelegate(RpcReader___Server_SetIgnorePlayers_1140765316));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EPolice_002EPoliceOfficer));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EPolice_002EPoliceOfficerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EPolice_002EPoliceOfficerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)syncVar____003CIgnorePlayers_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_BeginFootPursuit_Networked_310431262(string playerCode, bool includeColleagues = true)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(playerCode);
			((Writer)writer).WriteBoolean(includeColleagues);
			((NetworkBehaviour)this).SendServerRpc(39u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public virtual void RpcLogic___BeginFootPursuit_Networked_310431262(string playerCode, bool includeColleagues = true)
	{
		BeginFootPursuit(playerCode);
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		BodySearchBehaviour.Disable_Networked(null);
		if (!includeColleagues)
		{
			return;
		}
		if (FootPatrolBehaviour.Enabled && FootPatrolBehaviour.Group != null)
		{
			for (int i = 0; i < FootPatrolBehaviour.Group.Members.Count; i++)
			{
				if (!((Object)(object)FootPatrolBehaviour.Group.Members[i] == (Object)(object)this))
				{
					(FootPatrolBehaviour.Group.Members[i] as PoliceOfficer).BeginFootPursuit(playerCode);
				}
			}
		}
		if (CheckpointBehaviour.Enabled && (Object)(object)CheckpointBehaviour.Checkpoint != (Object)null)
		{
			for (int j = 0; j < CheckpointBehaviour.Checkpoint.AssignedNPCs.Count; j++)
			{
				if (!((Object)(object)CheckpointBehaviour.Checkpoint.AssignedNPCs[j] == (Object)(object)this))
				{
					(CheckpointBehaviour.Checkpoint.AssignedNPCs[j] as PoliceOfficer).BeginFootPursuit(playerCode);
				}
			}
		}
		if (!SentryBehaviour.Enabled || !((Object)(object)SentryBehaviour.AssignedLocation != (Object)null))
		{
			return;
		}
		for (int k = 0; k < SentryBehaviour.AssignedLocation.AssignedOfficers.Count; k++)
		{
			if (!((Object)(object)SentryBehaviour.AssignedLocation.AssignedOfficers[k] == (Object)(object)this))
			{
				SentryBehaviour.AssignedLocation.AssignedOfficers[k].BeginFootPursuit(playerCode);
			}
		}
	}

	private void RpcReader___Server_BeginFootPursuit_Networked_310431262(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string playerCode = ((Reader)PooledReader0).ReadString();
		bool includeColleagues = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___BeginFootPursuit_Networked_310431262(playerCode, includeColleagues);
		}
	}

	private void RpcWriter___Observers_BeginFootPursuit_3615296227(string playerCode)
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
			((Writer)writer).WriteString(playerCode);
			((NetworkBehaviour)this).SendObserversRpc(40u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___BeginFootPursuit_3615296227(string playerCode)
	{
		Player player = Player.GetPlayer(playerCode);
		if ((Object)(object)player == (Object)null)
		{
			Console.LogError("Attempted to begin foot pursuit with null target");
		}
		else if (InstanceFinder.IsServer)
		{
			PursuitBehaviour.SetTargetAndEnable_Server(((NetworkBehaviour)player).NetworkObject);
		}
	}

	private void RpcReader___Observers_BeginFootPursuit_3615296227(PooledReader PooledReader0, Channel channel)
	{
		string playerCode = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___BeginFootPursuit_3615296227(playerCode);
		}
	}

	private void RpcWriter___Server_BeginVehiclePursuit_Networked_1834136777(string playerCode, NetworkObject vehicle, bool beginAsSighted)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(playerCode);
			((Writer)writer).WriteNetworkObject(vehicle);
			((Writer)writer).WriteBoolean(beginAsSighted);
			((NetworkBehaviour)this).SendServerRpc(41u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public virtual void RpcLogic___BeginVehiclePursuit_Networked_1834136777(string playerCode, NetworkObject vehicle, bool beginAsSighted)
	{
		BeginVehiclePursuit(playerCode, vehicle, beginAsSighted);
	}

	private void RpcReader___Server_BeginVehiclePursuit_Networked_1834136777(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string playerCode = ((Reader)PooledReader0).ReadString();
		NetworkObject vehicle = ((Reader)PooledReader0).ReadNetworkObject();
		bool beginAsSighted = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___BeginVehiclePursuit_Networked_1834136777(playerCode, vehicle, beginAsSighted);
		}
	}

	private void RpcWriter___Observers_BeginVehiclePursuit_1834136777(string playerCode, NetworkObject vehicle, bool beginAsSighted)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(playerCode);
			((Writer)writer).WriteNetworkObject(vehicle);
			((Writer)writer).WriteBoolean(beginAsSighted);
			((NetworkBehaviour)this).SendObserversRpc(42u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___BeginVehiclePursuit_1834136777(string playerCode, NetworkObject vehicle, bool beginAsSighted)
	{
		Player player = Player.GetPlayer(playerCode);
		if ((Object)(object)player == (Object)null)
		{
			Console.LogError("Attempted to begin foot pursuit with null target");
			return;
		}
		VehiclePursuitBehaviour.vehicle = ((Component)vehicle).GetComponent<LandVehicle>();
		VehiclePursuitBehaviour.AssignTarget(player);
		if (beginAsSighted)
		{
			VehiclePursuitBehaviour.BeginAsSighted();
		}
		VehiclePursuitBehaviour.Enable();
	}

	private void RpcReader___Observers_BeginVehiclePursuit_1834136777(PooledReader PooledReader0, Channel channel)
	{
		string playerCode = ((Reader)PooledReader0).ReadString();
		NetworkObject vehicle = ((Reader)PooledReader0).ReadNetworkObject();
		bool beginAsSighted = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___BeginVehiclePursuit_1834136777(playerCode, vehicle, beginAsSighted);
		}
	}

	private void RpcWriter___Server_BeginBodySearch_Networked_3615296227(string playerCode)
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
			((Writer)writer).WriteString(playerCode);
			((NetworkBehaviour)this).SendServerRpc(43u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public virtual void RpcLogic___BeginBodySearch_Networked_3615296227(string playerCode)
	{
		BeginBodySearch(playerCode);
	}

	private void RpcReader___Server_BeginBodySearch_Networked_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string playerCode = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___BeginBodySearch_Networked_3615296227(playerCode);
		}
	}

	private void RpcWriter___Observers_BeginBodySearch_3615296227(string playerCode)
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
			((Writer)writer).WriteString(playerCode);
			((NetworkBehaviour)this).SendObserversRpc(44u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___BeginBodySearch_3615296227(string playerCode)
	{
		Player player = Player.GetPlayer(playerCode);
		if ((Object)(object)player == (Object)null)
		{
			Console.LogError("Attempted to begin foot pursuit with null target");
			return;
		}
		BodySearchBehaviour.AssignTarget(null, ((NetworkBehaviour)player).NetworkObject);
		BodySearchBehaviour.Enable();
	}

	private void RpcReader___Observers_BeginBodySearch_3615296227(PooledReader PooledReader0, Channel channel)
	{
		string playerCode = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___BeginBodySearch_3615296227(playerCode);
		}
	}

	private void RpcWriter___Observers_AssignToCheckpoint_4087078542(CheckpointManager.ECheckpointLocation location)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerated((Writer)(object)writer, location);
			((NetworkBehaviour)this).SendObserversRpc(45u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public virtual void RpcLogic___AssignToCheckpoint_4087078542(CheckpointManager.ECheckpointLocation location)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Movement.Warp(((Component)NetworkSingleton<CheckpointManager>.Instance.GetCheckpoint(location)).transform.position);
		CheckpointBehaviour.SetCheckpoint(location);
		CheckpointBehaviour.Enable();
		((Component)DialogueHandler).GetComponent<DialogueController>().OverrideContainer = CheckpointDialogue;
	}

	private void RpcReader___Observers_AssignToCheckpoint_4087078542(PooledReader PooledReader0, Channel channel)
	{
		CheckpointManager.ECheckpointLocation location = GeneratedReaders___Internal.Read___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___AssignToCheckpoint_4087078542(location);
		}
	}

	private void RpcWriter___Server_SetIgnorePlayers_1140765316(bool ignore)
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
			((Writer)writer).WriteBoolean(ignore);
			((NetworkBehaviour)this).SendServerRpc(46u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetIgnorePlayers_1140765316(bool ignore)
	{
		if (!InstanceFinder.IsServer)
		{
			Console.LogError("Attempted to set IgnorePlayers on client");
		}
		Console.Log(base.fullName + " IgnorePlayers set to " + ignore);
		IgnorePlayers = ignore;
	}

	private void RpcReader___Server_SetIgnorePlayers_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool ignore = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SetIgnorePlayers_1140765316(ignore);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EPolice_002EPoliceOfficer(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		if (UInt321 == 1)
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CIgnorePlayers_003Ek__BackingField(syncVar____003CIgnorePlayers_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			bool value = ((Reader)PooledReader0).ReadBoolean();
			this.sync___set_value__003CIgnorePlayers_003Ek__BackingField(value, Boolean2);
			return true;
		}
		return false;
	}

	protected override void Awake_UserLogic_ScheduleOne_002EPolice_002EPoliceOfficer_Assembly_002DCSharp_002Edll()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		base.Awake();
		if (!Officers.Contains(this))
		{
			Officers.Add(this);
		}
		((MonoBehaviour)this).InvokeRepeating("CheckNewInvestigation", 1f, 1f);
		chatterCountDown = Random.Range(15f, 45f);
		VisionCone visionCone = Awareness.VisionCone;
		visionCone.onVisionEventFull = (VisionCone.EventStateChange)Delegate.Combine(visionCone.onVisionEventFull, new VisionCone.EventStateChange(ProcessVisionEvent));
		Health.onDie.AddListener(new UnityAction(OnDie));
	}
}
