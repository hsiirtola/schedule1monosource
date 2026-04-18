using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Logging;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Animation;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.Combat;
using ScheduleOne.Core;
using ScheduleOne.Core.Audio;
using ScheduleOne.Core.Equipping.Framework;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Cutscenes;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects;
using ScheduleOne.Equipping.Framework;
using ScheduleOne.FX;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Law;
using ScheduleOne.Map;
using ScheduleOne.Money;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts.Health;
using ScheduleOne.Product;
using ScheduleOne.Property;
using ScheduleOne.Skating;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using ScheduleOne.UI.MainMenu;
using ScheduleOne.Variables;
using ScheduleOne.Vehicles;
using ScheduleOne.Vision;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ScheduleOne.PlayerScripts;

public class Player : NetworkBehaviour, ISaveable, ICombatTargetable, IDamageable, ISightable, INetworkedEquippableUser, IEquippableUser, IEquippablePlayerUser
{
	public delegate void VehicleEvent(LandVehicle vehicle);

	public delegate void VehicleTransformEvent(LandVehicle vehicle, Transform exitPoint);

	public const string OWNER_PLAYER_CODE = "Local";

	public const float CapColDefaultHeight = 2f;

	private const int LightningStrikeBoostDuration = 60;

	public List<NetworkObject> objectsTemporarilyOwnedByPlayer = new List<NetworkObject>();

	public static Action onLocalPlayerSpawned;

	public static Action<Player> onPlayerSpawned;

	public static Action<Player> onPlayerDespawned;

	public static Player Local;

	public static List<Player> PlayerList = new List<Player>();

	[Header("References")]
	public GameObject LocalGameObject;

	public Avatar Avatar;

	public AvatarAnimation Anim;

	public SmoothedVelocityCalculator VelocityCalculator;

	public PlayerVisibility VisualState;

	public EntityVisibility Visibility;

	public CapsuleCollider CapCol;

	public POI PoI;

	public PlayerHealth Health;

	public PlayerCrimeData CrimeData;

	public PlayerEnergy Energy;

	public Transform MimicCamera;

	public AvatarFootstepDetector FootstepDetector;

	public CharacterController CharacterController;

	public AudioSourceController PunchSound;

	public OptimizedLight ThirdPersonFlashlight;

	public WorldspaceDialogueRenderer NameLabel;

	public PlayerClothing Clothing;

	public WorldspaceDialogueRenderer WorldspaceDialogue;

	[Header("Settings")]
	public LayerMask GroundDetectionMask;

	public float AvatarOffset_Standing = -0.97f;

	public float AvatarOffset_Crouched = -0.45f;

	[ColorUsage(true, true)]
	[SerializeField]
	private Color _lightningColorTint = Color.white;

	[Header("Movement mapping")]
	public AnimationCurve WalkingMapCurve;

	public AnimationCurve CrouchWalkMapCurve;

	[CompilerGenerated]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public string _003CPlayerName_003Ek__BackingField;

	public NetworkConnection Connection;

	[CompilerGenerated]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public string _003CPlayerCode_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(OnChange = "CurrentVehicleChanged")]
	public NetworkObject _003CCurrentVehicle_003Ek__BackingField;

	public VehicleEvent onEnterVehicle;

	public VehicleTransformEvent onExitVehicle;

	[CompilerGenerated]
	[SyncVar]
	public NetworkObject _003CCurrentBed_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	public bool _003CIsReadyToSleep_003Ek__BackingField;

	[CompilerGenerated]
	private bool _003CIsSkating_003Ek__BackingField;

	public Action<Skateboard> onSkateboardMounted;

	public Action onSkateboardDismounted;

	public bool HasCompletedIntro;

	[CompilerGenerated]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public Vector3 _003CCameraPosition_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public Quaternion _003CCameraRotation_003Ek__BackingField;

	public ItemSlot[] Inventory;

	[Header("Appearance debugging")]
	public BasicAvatarSettings DebugAvatarSettings;

	private PlayerLoader loader;

	public UnityEvent onRagdoll;

	public UnityEvent onRagdollEnd;

	public UnityEvent onArrested;

	public UnityEvent onFreed;

	public UnityEvent onTased;

	public UnityEvent onTasedEnd;

	public UnityEvent onPassedOut;

	public UnityEvent onPassOutRecovery;

	public UnityEvent onStruckByLightning;

	public List<BaseVariable> PlayerVariables;

	public Dictionary<string, BaseVariable> VariableDict;

	private float standingScale;

	private float timeAirborne;

	private Coroutine taseCoroutine;

	private List<ConstantForce> ragdollForceComponents;

	private List<int> impactHistory;

	private List<Quaternion> seizureRotations;

	private List<int> equippableMessageIDHistory;

	private NetworkedEquipper _networkedEquipper;

	private Coroutine lerpScaleRoutine;

	public SyncVar<string> syncVar____003CPlayerName_003Ek__BackingField;

	public SyncVar<string> syncVar____003CPlayerCode_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CCurrentVehicle_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CCurrentBed_003Ek__BackingField;

	public SyncVar<bool> syncVar____003CIsReadyToSleep_003Ek__BackingField;

	public SyncVar<Vector3> syncVar____003CCameraPosition_003Ek__BackingField;

	public SyncVar<Quaternion> syncVar____003CCameraRotation_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EPlayerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EPlayerAssembly_002DCSharp_002Edll_Excuted;

	public bool IsLocalPlayer => ((NetworkBehaviour)this).IsOwner;

	public IThirdPersonReferencesProvider ThirdPersonReferences => (IThirdPersonReferencesProvider)(object)Avatar;

	public IFirstPersonReferencesProvider FirstPersonReferences => (IFirstPersonReferencesProvider)(object)PlayerSingleton<PlayerInventory>.Instance;

	public Transform CenterPointTransform => Avatar.CenterPointTransform;

	public Vector3 LookAtPoint => ((Component)Avatar.Eyes).transform.position;

	public bool IsCurrentlyTargetable
	{
		get
		{
			if (Health.IsAlive && !IsArrested && !IsUnconscious && !IsSleeping)
			{
				return !IsRagdolled;
			}
			return false;
		}
	}

	public float RangedHitChanceMultiplier => Mathf.Clamp01(Health.TimeSinceLastDamage / 4f);

	public Vector3 Velocity => VelocityCalculator.Velocity;

	public VisionEvent HighestProgressionEvent { get; set; }

	public EntityVisibility VisibilityComponent => Visibility;

	public Vector3 EyePosition { get; private set; } = Vector3.zero;

	public string PlayerName
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CPlayerName_003Ek__BackingField;
		}
		[CompilerGenerated]
		protected set
		{
			this.sync___set_value__003CPlayerName_003Ek__BackingField(value, true);
		}
	}

	public string PlayerCode
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CPlayerCode_003Ek__BackingField;
		}
		[CompilerGenerated]
		protected set
		{
			this.sync___set_value__003CPlayerCode_003Ek__BackingField(value, true);
		}
	}

	public NetworkObject CurrentVehicle
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CCurrentVehicle_003Ek__BackingField;
		}
		[CompilerGenerated]
		[ServerRpc(RunLocally = true)]
		set
		{
			RpcWriter___Server_set_CurrentVehicle_3323014238(value);
			RpcLogic___set_CurrentVehicle_3323014238(value);
		}
	}

	public NetworkBehaviour NetworkBehaviour => (NetworkBehaviour)(object)this;

	public bool ThirdPersonMeshesVisibleToLocalPlayer { get; private set; }

	public bool IsInVehicle => (Object)(object)SyncAccessor__003CCurrentVehicle_003Ek__BackingField != (Object)null;

	public VehicleSeat CurrentVehicleSeat { get; private set; }

	public LandVehicle LastDrivenVehicle { get; private set; }

	public float TimeSinceVehicleExit { get; protected set; }

	public bool Crouched { get; private set; }

	public NetworkObject CurrentBed
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CCurrentBed_003Ek__BackingField;
		}
		[CompilerGenerated]
		[ServerRpc]
		set
		{
			RpcWriter___Server_set_CurrentBed_3323014238(value);
		}
	}

	public bool IsReadyToSleep
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CIsReadyToSleep_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			this.sync___set_value__003CIsReadyToSleep_003Ek__BackingField(value, true);
		}
	}

	public bool IsSkating
	{
		[CompilerGenerated]
		get
		{
			return _003CIsSkating_003Ek__BackingField;
		}
		[CompilerGenerated]
		[ServerRpc]
		set
		{
			RpcWriter___Server_set_IsSkating_1140765316(value);
		}
	}

	public Skateboard ActiveSkateboard { get; private set; }

	public bool IsSleeping { get; protected set; }

	public bool IsRagdolled { get; protected set; }

	public bool IsArrested { get; protected set; }

	public bool IsTased { get; protected set; }

	public bool IsUnconscious { get; protected set; }

	public float Scale { get; private set; }

	public ScheduleOne.Property.Property CurrentProperty { get; protected set; }

	public ScheduleOne.Property.Property LastVisitedProperty { get; protected set; }

	public Business CurrentBusiness
	{
		get
		{
			if (!((Object)(object)CurrentProperty != (Object)null))
			{
				return null;
			}
			return CurrentProperty as Business;
		}
	}

	public EMapRegion CurrentRegion { get; protected set; }

	public Vector3 PlayerBasePosition => ((Component)this).transform.position - ((Component)this).transform.up * (CharacterController.height / 2f);

	public Vector3 CameraPosition
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return SyncAccessor__003CCameraPosition_003Ek__BackingField;
		}
		[CompilerGenerated]
		[ServerRpc]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			RpcWriter___Server_set_CameraPosition_4276783012(value);
		}
	}

	public Quaternion CameraRotation
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return SyncAccessor__003CCameraRotation_003Ek__BackingField;
		}
		[CompilerGenerated]
		[ServerRpc]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			RpcWriter___Server_set_CameraRotation_3429297120(value);
		}
	}

	public int EquippedItemSlotIndex { get; private set; }

	public BasicAvatarSettings CurrentAvatarSettings { get; protected set; }

	public ProductItemInstance ConsumedProduct { get; private set; }

	public int TimeSinceProductConsumed { get; private set; }

	public string SaveFolderName
	{
		get
		{
			if (InstanceFinder.IsServer && ((NetworkBehaviour)this).IsOwner)
			{
				return "Player_0";
			}
			return "Player_" + SyncAccessor__003CPlayerCode_003Ek__BackingField;
		}
	}

	public string SaveFileName => "Player";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => true;

	public List<string> LocalExtraFiles { get; set; }

	public List<string> LocalExtraFolders { get; set; }

	public bool HasChanged { get; set; }

	public bool avatarVisibleToLocalPlayer { get; private set; }

	public bool playerDataRetrieveReturned { get; private set; }

	public bool playerSaveRequestReturned { get; private set; }

	public bool Paranoid { get; set; }

	public bool Sneaky { get; set; }

	public bool Disoriented { get; set; }

	public bool Seizure { get; set; }

	public bool Slippery { get; set; }

	public bool Schizophrenic { get; set; }

	public bool StruckByLightning { get; set; }

	public string SyncAccessor__003CPlayerName_003Ek__BackingField
	{
		get
		{
			return PlayerName;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				PlayerName = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CPlayerName_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public string SyncAccessor__003CPlayerCode_003Ek__BackingField
	{
		get
		{
			return PlayerCode;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				PlayerCode = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CPlayerCode_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public NetworkObject SyncAccessor__003CCurrentVehicle_003Ek__BackingField
	{
		get
		{
			return CurrentVehicle;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				CurrentVehicle = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CCurrentVehicle_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public NetworkObject SyncAccessor__003CCurrentBed_003Ek__BackingField
	{
		get
		{
			return CurrentBed;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				CurrentBed = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CCurrentBed_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public bool SyncAccessor__003CIsReadyToSleep_003Ek__BackingField
	{
		get
		{
			return IsReadyToSleep;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				IsReadyToSleep = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CIsReadyToSleep_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public Vector3 SyncAccessor__003CCameraPosition_003Ek__BackingField
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return CameraPosition;
		}
		set
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				CameraPosition = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CCameraPosition_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public Quaternion SyncAccessor__003CCameraRotation_003Ek__BackingField
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return CameraRotation;
		}
		set
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				CameraRotation = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CCameraRotation_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public event Action<bool> OnThirdPersonMeshesVisibilityChanged;

	public void RecordLastKnownPosition(bool resetTimeSinceLastSeen)
	{
		CrimeData.RecordLastKnownPosition(resetTimeSinceLastSeen);
	}

	public float GetSearchTime()
	{
		return CrimeData.GetSearchTime();
	}

	public bool IsCurrentlySightable()
	{
		if (!Health.IsAlive)
		{
			return false;
		}
		if (IsArrested)
		{
			return false;
		}
		return true;
	}

	[Button]
	public void LoadDebugAvatarSettings()
	{
		SetAppearance(DebugAvatarSettings, refreshClothing: false);
	}

	public static Player GetPlayer(NetworkConnection conn)
	{
		for (int i = 0; i < PlayerList.Count; i++)
		{
			if (PlayerList[i].Connection == conn)
			{
				return PlayerList[i];
			}
		}
		return null;
	}

	public static Player GetRandomPlayer(bool excludeArrestedOrDead = true, bool excludeSleeping = true)
	{
		List<Player> list = new List<Player>();
		for (int i = 0; i < PlayerList.Count; i++)
		{
			if ((!excludeArrestedOrDead || (!PlayerList[i].IsArrested && PlayerList[i].Health.IsAlive)) && (!excludeSleeping || !PlayerList[i].IsSleeping))
			{
				list.Add(PlayerList[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		int index = Random.Range(0, list.Count);
		return list[index];
	}

	public static Player GetPlayer(string playerCode)
	{
		return PlayerList.Find((Player x) => x.SyncAccessor__003CPlayerCode_003Ek__BackingField == playerCode);
	}

	public static Player GetPlayerByName(string playerName)
	{
		return PlayerList.Find((Player x) => x.SyncAccessor__003CPlayerName_003Ek__BackingField.ToLower() == playerName.ToLower());
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EPlayerScripts_002EPlayer_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	protected virtual void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(OnUncappedMinutePass);
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onSleepStart = (Action)Delegate.Remove(instance.onSleepStart, new Action(SleepStart));
			TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
			instance2.onSleepEnd = (Action)Delegate.Remove(instance2.onSleepEnd, new Action(SleepEnd));
		}
		if (NetworkSingleton<MoneyManager>.InstanceExists)
		{
			MoneyManager instance3 = NetworkSingleton<MoneyManager>.Instance;
			instance3.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Remove(instance3.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
		}
	}

	public override void OnStartClient()
	{
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		((NetworkBehaviour)this).OnStartClient();
		Connection = ((NetworkBehaviour)this).Owner;
		if (((NetworkBehaviour)this).IsOwner)
		{
			if (Application.isEditor)
			{
				LoadDebugAvatarSettings();
			}
			LocalGameObject.gameObject.SetActive(true);
			Local = this;
			if (onLocalPlayerSpawned != null)
			{
				onLocalPlayerSpawned();
			}
			LayerUtility.SetLayerRecursively(((Component)Avatar).gameObject, LayerMask.NameToLayer("Invisible"));
			if (Singleton<Lobby>.Instance.IsInLobby && !Singleton<Lobby>.Instance.IsHost)
			{
				InstanceFinder.TransportManager.Transport.OnClientConnectionState += ClientConnectionStateChanged;
			}
			((Behaviour)FootstepDetector).enabled = false;
			PoI.SetMainText("You");
			if ((Object)(object)PoI.UI != (Object)null)
			{
				((Component)PoI.UI).GetComponentInChildren<Animation>().Play();
			}
			((Component)NameLabel).gameObject.SetActive(false);
			if (((NetworkBehaviour)this).IsHost)
			{
				if (Singleton<LoadManager>.Instance.IsGameLoaded)
				{
					PlayerLoaded();
				}
				else
				{
					Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(PlayerLoaded));
				}
			}
			ulong id = 0uL;
			if (SteamManager.Initialized)
			{
				id = SteamUser.GetSteamID().m_SteamID;
				PlayerName = SteamFriends.GetPersonaName();
			}
			SendPlayerNameData(SyncAccessor__003CPlayerName_003Ek__BackingField, id);
			SetCapsuleColliderHeight(1f);
			if (!InstanceFinder.IsServer)
			{
				RequestPlayerData(SyncAccessor__003CPlayerCode_003Ek__BackingField);
			}
		}
		else
		{
			((Collider)CapCol).isTrigger = true;
			((Object)((Component)this).gameObject).name = SyncAccessor__003CPlayerName_003Ek__BackingField + " (" + SyncAccessor__003CPlayerCode_003Ek__BackingField + ")";
			PoI.SetMainText(SyncAccessor__003CPlayerName_003Ek__BackingField);
		}
		if (((NetworkBehaviour)this).IsOwner || InstanceFinder.IsServer || (Singleton<Lobby>.Instance.IsInLobby && Singleton<Lobby>.Instance.IsHost))
		{
			CreatePlayerVariables();
		}
		if (onPlayerSpawned != null)
		{
			onPlayerSpawned(this);
		}
		Console.Log("Player spawned (" + SyncAccessor__003CPlayerName_003Ek__BackingField + ")");
		CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: false);
		if (!((NetworkBehaviour)this).IsOwner && (Object)(object)SyncAccessor__003CCurrentVehicle_003Ek__BackingField != (Object)null)
		{
			Console.Log("This player is in a vehicle!");
			LandVehicle component = ((Component)SyncAccessor__003CCurrentVehicle_003Ek__BackingField).GetComponent<LandVehicle>();
			EnterVehicle(component, CurrentVehicleSeat);
		}
		PlayerList.Add(this);
	}

	private void PlayerLoaded()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(PlayerLoaded));
		if (!((NetworkBehaviour)this).IsOwner)
		{
			return;
		}
		if ((Object)(object)PoI != (Object)null)
		{
			PoI.SetMainText("You");
			if ((Object)(object)PoI.UI != (Object)null)
			{
				((Component)PoI.UI).GetComponentInChildren<Animation>().Play();
			}
		}
		if (!HasCompletedIntro && !Singleton<LoadManager>.Instance.DebugMode)
		{
			Scene activeScene = SceneManager.GetActiveScene();
			if (((Scene)(ref activeScene)).name == "Main")
			{
				PlayerSingleton<PlayerMovement>.Instance.Teleport(NetworkSingleton<GameManager>.Instance.SpawnPoint.position);
				((Component)this).transform.forward = NetworkSingleton<GameManager>.Instance.SpawnPoint.forward;
				Console.Log("Player has not completed intro; playing intro");
				Singleton<IntroManager>.Instance.Play();
				Singleton<CharacterCreator>.Instance.onComplete.AddListener((UnityAction<BasicAvatarSettings>)MarkIntroCompleted);
			}
		}
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (((NetworkBehaviour)this).Owner != connection)
		{
			PlayerData data = new PlayerData(SyncAccessor__003CPlayerCode_003Ek__BackingField, ((Component)this).transform.position, ((Component)this).transform.eulerAngles.y, HasCompletedIntro);
			string empty = string.Empty;
			string appearanceString = (((Object)(object)CurrentAvatarSettings != (Object)null) ? CurrentAvatarSettings.GetJson() : string.Empty);
			string clothingString = GetClothingString();
			if (Crouched)
			{
				ReceiveCrouched(connection, crouched: true);
			}
			ReceivePlayerData(connection, data, empty, appearanceString, clothingString, null);
			ReceivePlayerNameData(connection, SyncAccessor__003CPlayerName_003Ek__BackingField, SyncAccessor__003CPlayerCode_003Ek__BackingField);
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void RequestSavePlayer()
	{
		RpcWriter___Server_RequestSavePlayer_2166136261();
		RpcLogic___RequestSavePlayer_2166136261();
	}

	[ObserversRpc]
	[TargetRpc]
	private void ReturnSaveRequest(NetworkConnection conn, bool successful)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ReturnSaveRequest_214505783(conn, successful);
		}
		else
		{
			RpcWriter___Target_ReturnSaveRequest_214505783(conn, successful);
		}
	}

	[ObserversRpc(RunLocally = true)]
	public void HostExitedGame()
	{
		RpcWriter___Observers_HostExitedGame_2166136261();
		RpcLogic___HostExitedGame_2166136261();
	}

	private unsafe void ClientConnectionStateChanged(ClientConnectionStateArgs args)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Console.Log("Client connection state changed: " + ((object)(*(LocalConnectionState*)(&args.ConnectionState))/*cast due to .constrained prefix*/).ToString());
		if ((int)args.ConnectionState == 3 || (int)args.ConnectionState == 0)
		{
			HostExitedGame();
		}
	}

	[ServerRpc(RunLocally = true)]
	public void SendPlayerNameData(string playerName, ulong id)
	{
		RpcWriter___Server_SendPlayerNameData_586648380(playerName, id);
		RpcLogic___SendPlayerNameData_586648380(playerName, id);
	}

	[ServerRpc(RequireOwnership = false)]
	public void RequestPlayerData(string playerCode)
	{
		RpcWriter___Server_RequestPlayerData_3615296227(playerCode);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void ReceivePlayerData(NetworkConnection conn, PlayerData data, string inventoryString, string appearanceString, string clothigString, VariableData[] vars)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ReceivePlayerData_3244732873(conn, data, inventoryString, appearanceString, clothigString, vars);
			RpcLogic___ReceivePlayerData_3244732873(conn, data, inventoryString, appearanceString, clothigString, vars);
		}
		else
		{
			RpcWriter___Target_ReceivePlayerData_3244732873(conn, data, inventoryString, appearanceString, clothigString, vars);
		}
	}

	public void SetGravityMultiplier(float multiplier)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (((NetworkBehaviour)this).IsOwner)
		{
			PlayerMovement.GravityMultiplier = multiplier;
		}
		foreach (ConstantForce ragdollForceComponent in ragdollForceComponents)
		{
			ragdollForceComponent.force = Physics.gravity * multiplier * ((Component)ragdollForceComponent).GetComponent<Rigidbody>().mass;
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void ReceivePlayerNameData(NetworkConnection conn, string playerName, string id)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ReceivePlayerNameData_3895153758(conn, playerName, id);
			RpcLogic___ReceivePlayerNameData_3895153758(conn, playerName, id);
		}
		else
		{
			RpcWriter___Target_ReceivePlayerNameData_3895153758(conn, playerName, id);
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetFlashlightOn_Server(bool on)
	{
		RpcWriter___Server_SetFlashlightOn_Server_1140765316(on);
		RpcLogic___SetFlashlightOn_Server_1140765316(on);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetFlashlightOn_Client(bool on)
	{
		RpcWriter___Observers_SetFlashlightOn_Client_1140765316(on);
		RpcLogic___SetFlashlightOn_Client_1140765316(on);
	}

	public override void OnStopClient()
	{
		((NetworkBehaviour)this).OnStopClient();
		PlayerList.Remove(this);
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		((NetworkBehaviour)this).ServerManager.Objects.OnPreDestroyClientObjects += PreDestroyClientObjects;
	}

	protected virtual void Update()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		HasChanged = true;
		((Component)CapCol).transform.position = ((Component)Avatar).transform.position;
		if ((Object)(object)SyncAccessor__003CCurrentVehicle_003Ek__BackingField != (Object)null)
		{
			TimeSinceVehicleExit = 0f;
		}
		else
		{
			TimeSinceVehicleExit += Time.deltaTime;
		}
		if (((NetworkBehaviour)this).IsOwner)
		{
			if (((Component)this).transform.position.y < -20f)
			{
				PlayerSingleton<PlayerMovement>.Instance.WarpToNavMesh(clearVelocity: true);
			}
			if ((Object)(object)ActiveSkateboard != (Object)null)
			{
				SetCapsuleColliderHeight(1f - ActiveSkateboard.Animation.CurrentCrouchShift * 0.25f);
			}
			if (NetworkSingleton<VariableDatabase>.InstanceExists)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Player_In_Vehicle", ((Object)(object)SyncAccessor__003CCurrentVehicle_003Ek__BackingField != (Object)null).ToString());
			}
		}
	}

	protected virtual void OnUncappedMinutePass()
	{
		if (ConsumedProduct != null)
		{
			TimeSinceProductConsumed++;
			if (TimeSinceProductConsumed >= (ConsumedProduct.Definition as ProductDefinition).PlayerEffectDuration)
			{
				ClearProduct();
			}
		}
	}

	protected virtual void LateUpdate()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (((NetworkBehaviour)this).IsOwner)
		{
			RpcWriter___Server_set_CameraPosition_4276783012(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position);
			RpcWriter___Server_set_CameraRotation_3429297120(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.rotation);
		}
		if (Seizure)
		{
			for (int i = 0; i < Avatar.RagdollRBs.Length; i++)
			{
				if (seizureRotations.Count <= Avatar.RagdollRBs.Length)
				{
					seizureRotations.Add(Quaternion.identity);
				}
				seizureRotations[i] = Quaternion.Lerp(seizureRotations[i], Quaternion.Euler(Random.insideUnitSphere * 30f), Time.deltaTime * 10f);
				Transform transform = ((Component)Avatar.RagdollRBs[i]).transform;
				transform.localRotation *= seizureRotations[i];
			}
		}
		((Component)MimicCamera).transform.position = SyncAccessor__003CCameraPosition_003Ek__BackingField;
		((Component)MimicCamera).transform.rotation = SyncAccessor__003CCameraRotation_003Ek__BackingField;
		EyePosition = ((Component)Avatar.Eyes).transform.position;
	}

	private void RecalculateCurrentProperty()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		ScheduleOne.Property.Property property = ScheduleOne.Property.Property.Properties.OrderBy((ScheduleOne.Property.Property x) => Vector3.Distance(x.BoundingBox.transform.position, Avatar.CenterPoint)).FirstOrDefault();
		Business.Businesses.OrderBy((Business x) => Vector3.Distance(x.BoundingBox.transform.position, Avatar.CenterPoint)).FirstOrDefault();
		if ((Object)(object)property == (Object)null)
		{
			CurrentProperty = null;
		}
		else if (property.DoBoundsContainPoint(Avatar.CenterPoint))
		{
			CurrentProperty = property;
			LastVisitedProperty = CurrentProperty;
		}
		else
		{
			CurrentProperty = null;
		}
	}

	private void RecalculateCurrentRegion()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Singleton<ScheduleOne.Map.Map>.Instance.Regions.Length; i++)
		{
			if (Singleton<ScheduleOne.Map.Map>.Instance.Regions[i].RegionBounds.IsPointInsidePolygon(Avatar.CenterPoint))
			{
				CurrentRegion = Singleton<ScheduleOne.Map.Map>.Instance.Regions[i].Region;
				break;
			}
		}
	}

	private void FixedUpdate()
	{
		ApplyMovementVisuals();
	}

	private void ApplyMovementVisuals()
	{
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		if (IsSkating)
		{
			Anim.SetTimeAirborne(0f);
			Anim.SetGrounded(grounded: true);
			Anim.SetDirection(0f);
			Anim.SetStrafe(0f);
			return;
		}
		bool isGrounded = GetIsGrounded();
		if (isGrounded)
		{
			timeAirborne = 0f;
		}
		else
		{
			timeAirborne += Time.deltaTime;
		}
		Anim.SetTimeAirborne(timeAirborne);
		if (Crouched)
		{
			standingScale = Mathf.MoveTowards(standingScale, 0f, Time.deltaTime / 0.2f);
		}
		else
		{
			standingScale = Mathf.MoveTowards(standingScale, 1f, Time.deltaTime / 0.2f);
		}
		float capsuleColliderHeight = (Crouched ? 0.75f : 1f);
		SetCapsuleColliderHeight(capsuleColliderHeight);
		Anim.SetGrounded(isGrounded);
		Anim.SetCrouched(Crouched);
		if ((Object)(object)SyncAccessor__003CCurrentVehicle_003Ek__BackingField == (Object)null)
		{
			((Component)Avatar).transform.localPosition = new Vector3(0f, Mathf.Lerp(AvatarOffset_Crouched, AvatarOffset_Standing, standingScale), 0f);
		}
		Vector3 val = ((Component)this).transform.InverseTransformVector(VelocityCalculator.Velocity) / 6.1749997f;
		if (Crouched)
		{
			Anim.SetDirection(CrouchWalkMapCurve.Evaluate(Mathf.Abs(val.z)) * Mathf.Sign(val.z));
			Anim.SetStrafe(CrouchWalkMapCurve.Evaluate(Mathf.Abs(val.x)) * Mathf.Sign(val.x));
		}
		else
		{
			Anim.SetDirection(WalkingMapCurve.Evaluate(Mathf.Abs(val.z)) * Mathf.Sign(val.z));
			Anim.SetStrafe(WalkingMapCurve.Evaluate(Mathf.Abs(val.x)) * Mathf.Sign(val.x));
		}
	}

	public void SetVisible(bool vis, bool network = false)
	{
		Avatar.SetVisible(vis);
		if (network)
		{
			SetVisible_Networked(vis);
		}
	}

	[ObserversRpc]
	public void PlayJumpAnimation()
	{
		RpcWriter___Observers_PlayJumpAnimation_2166136261();
	}

	public bool GetIsGrounded()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		float num = 1.85f * (Crouched ? 0.65f : 1f) / 2f + 0.1f;
		RaycastHit val = default(RaycastHit);
		return Physics.SphereCast(((Component)this).transform.position, 0.2625f, Vector3.down, ref val, num, LayerMask.op_Implicit(GroundDetectionMask), (QueryTriggerInteraction)1);
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SendCrouched(bool crouched)
	{
		RpcWriter___Server_SendCrouched_1140765316(crouched);
		RpcLogic___SendCrouched_1140765316(crouched);
	}

	public void SetCrouchedLocal(bool crouched)
	{
		Crouched = crouched;
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	private void ReceiveCrouched(NetworkConnection conn, bool crouched)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ReceiveCrouched_214505783(conn, crouched);
			RpcLogic___ReceiveCrouched_214505783(conn, crouched);
		}
		else
		{
			RpcWriter___Target_ReceiveCrouched_214505783(conn, crouched);
		}
	}

	[ServerRpc(RunLocally = true)]
	public void SendAvatarSettings(AvatarSettings settings)
	{
		RpcWriter___Server_SendAvatarSettings_4281687581(settings);
		RpcLogic___SendAvatarSettings_4281687581(settings);
	}

	[ObserversRpc(BufferLast = true, RunLocally = true)]
	public void SetAvatarSettings(AvatarSettings settings)
	{
		RpcWriter___Observers_SetAvatarSettings_4281687581(settings);
		RpcLogic___SetAvatarSettings_4281687581(settings);
	}

	[ObserversRpc]
	private void SetVisible_Networked(bool vis)
	{
		RpcWriter___Observers_SetVisible_Networked_1140765316(vis);
	}

	public void EnterVehicle(LandVehicle vehicle, VehicleSeat seat)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		CurrentVehicle = ((NetworkBehaviour)vehicle).NetworkObject;
		CurrentVehicleSeat = seat;
		LastDrivenVehicle = vehicle;
		Collider[] componentsInChildren = ((Component)vehicle).GetComponentsInChildren<Collider>();
		foreach (Collider val in componentsInChildren)
		{
			if (!val.isTrigger)
			{
				Physics.IgnoreCollision((Collider)(object)CapCol, val, true);
			}
		}
		((Component)Avatar).transform.SetParent(((Component)vehicle).transform);
		if ((Object)(object)seat != (Object)null)
		{
			((Component)Avatar).transform.position = ((Component)seat).transform.position + Vector3.down * 1f;
		}
		else
		{
			((Component)Avatar).transform.localPosition = Vector3.zero;
		}
		((Component)Avatar).transform.localRotation = Quaternion.identity;
		if (onEnterVehicle != null)
		{
			onEnterVehicle(vehicle);
		}
		SetVisible(vis: false, network: true);
	}

	public void ExitVehicle(Transform exitPoint)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)SyncAccessor__003CCurrentVehicle_003Ek__BackingField == (Object)null)
		{
			return;
		}
		((Component)Avatar).transform.SetParent(((Component)this).transform);
		((Component)Avatar).transform.localPosition = Vector3.zero;
		((Component)Avatar).transform.localRotation = Quaternion.identity;
		((Component)Local).transform.position = exitPoint.position;
		((Component)Local).transform.rotation = exitPoint.rotation;
		((Component)Local).transform.eulerAngles = new Vector3(0f, ((Component)this).transform.eulerAngles.y, 0f);
		if (onExitVehicle != null)
		{
			onExitVehicle(((Component)SyncAccessor__003CCurrentVehicle_003Ek__BackingField).GetComponent<LandVehicle>(), exitPoint);
		}
		Collider[] componentsInChildren = ((Component)SyncAccessor__003CCurrentVehicle_003Ek__BackingField).GetComponentsInChildren<Collider>();
		foreach (Collider val in componentsInChildren)
		{
			if (!val.isTrigger)
			{
				Physics.IgnoreCollision((Collider)(object)CapCol, val, false);
			}
		}
		SetVisible(vis: true);
		CurrentVehicle = null;
		CurrentVehicleSeat = null;
	}

	private void PreDestroyClientObjects(NetworkConnection conn)
	{
		if ((Object)(object)SyncAccessor__003CCurrentVehicle_003Ek__BackingField != (Object)null)
		{
			SyncAccessor__003CCurrentVehicle_003Ek__BackingField.RemoveOwnership();
			((Component)SyncAccessor__003CCurrentVehicle_003Ek__BackingField).GetComponent<LandVehicle>().ExitVehicle();
		}
		int count = objectsTemporarilyOwnedByPlayer.Count;
		for (int i = 0; i < count; i++)
		{
			Debug.Log((object)("Stripping object ownership back to server: " + ((Object)((Component)objectsTemporarilyOwnedByPlayer[i]).gameObject).name));
			objectsTemporarilyOwnedByPlayer[i].RemoveOwnership();
		}
	}

	private void CurrentVehicleChanged(NetworkObject oldVeh, NetworkObject newVeh, bool asServer)
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (((NetworkBehaviour)this).IsOwner || (Object)(object)oldVeh == (Object)(object)newVeh)
		{
			return;
		}
		if ((Object)(object)oldVeh != (Object)null)
		{
			Collider[] componentsInChildren = ((Component)oldVeh).GetComponentsInChildren<Collider>();
			foreach (Collider val in componentsInChildren)
			{
				if (!val.isTrigger)
				{
					Physics.IgnoreCollision((Collider)(object)CapCol, val, false);
				}
			}
		}
		if ((Object)(object)newVeh != (Object)null)
		{
			LastDrivenVehicle = ((Component)newVeh).GetComponent<LandVehicle>();
			((Component)Avatar).transform.SetParent(((Component)newVeh).transform);
			((Component)Avatar).transform.localPosition = Vector3.zero;
			((Component)Avatar).transform.localRotation = Quaternion.identity;
			Collider[] componentsInChildren = ((Component)newVeh).GetComponentsInChildren<Collider>();
			foreach (Collider val2 in componentsInChildren)
			{
				if (!val2.isTrigger)
				{
					Physics.IgnoreCollision((Collider)(object)CapCol, val2, true);
				}
			}
			SetVisible(vis: false);
		}
		else
		{
			((Component)Avatar).transform.SetParent(((Component)this).transform);
			((Component)Avatar).transform.localPosition = Vector3.zero;
			((Component)Avatar).transform.localRotation = Quaternion.identity;
			SetVisible(vis: true);
		}
	}

	public static bool AreAllPlayersReadyToSleep()
	{
		if (PlayerList.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < PlayerList.Count; i++)
		{
			if (!((Object)(object)PlayerList[i] == (Object)null) && !PlayerList[i].SyncAccessor__003CIsReadyToSleep_003Ek__BackingField)
			{
				return false;
			}
		}
		return true;
	}

	private void SleepStart()
	{
		IsSleeping = true;
		ResetHitByLightning();
		ClearProduct();
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetReadyToSleep(bool ready)
	{
		RpcWriter___Server_SetReadyToSleep_1140765316(ready);
		RpcLogic___SetReadyToSleep_1140765316(ready);
	}

	private void SleepEnd()
	{
		IsSleeping = false;
	}

	public static void Activate()
	{
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
	}

	public static void Deactivate(bool freeMouse)
	{
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.ResetRotation();
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		if (freeMouse)
		{
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		}
	}

	public void ExitAll()
	{
		if ((Object)(object)SyncAccessor__003CCurrentVehicle_003Ek__BackingField != (Object)null)
		{
			((Component)SyncAccessor__003CCurrentVehicle_003Ek__BackingField).GetComponent<LandVehicle>().ExitVehicle();
			SetVisible(vis: true);
		}
		Singleton<GameInput>.Instance.ExitAll();
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		((Behaviour)Singleton<HUD>.Instance.canvas).enabled = false;
	}

	public void SetVisibleToLocalPlayer(bool vis)
	{
		avatarVisibleToLocalPlayer = vis;
		if (vis)
		{
			LayerUtility.SetLayerRecursively(((Component)Avatar).gameObject, LayerMask.NameToLayer("Player"));
		}
		else
		{
			LayerUtility.SetLayerRecursively(((Component)Avatar).gameObject, LayerMask.NameToLayer("Invisible"));
		}
		SetThirdPersonMeshesVisibility(vis);
	}

	[ServerRpc]
	public void SendPunch()
	{
		RpcWriter___Server_SendPunch_2166136261();
	}

	[ObserversRpc]
	private void Punch()
	{
		RpcWriter___Observers_Punch_2166136261();
	}

	[ServerRpc(RunLocally = true)]
	private void MarkIntroCompleted(BasicAvatarSettings appearance)
	{
		RpcWriter___Server_MarkIntroCompleted_3281254764(appearance);
		RpcLogic___MarkIntroCompleted_3281254764(appearance);
	}

	public bool IsPointVisibleToPlayer(Vector3 point, float maxDistance_Visible = 30f, float minDistance_Invisible = 5f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Distance(point, ((Component)MimicCamera).transform.position);
		if (num > maxDistance_Visible)
		{
			return false;
		}
		if (num < minDistance_Invisible)
		{
			return true;
		}
		if (MimicCamera.InverseTransformPoint(point).z < -1f)
		{
			return false;
		}
		Vector3 position = ((Component)MimicCamera).transform.position;
		Vector3 val = point - ((Component)MimicCamera).transform.position;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(position, ((Vector3)(ref val)).normalized, ref val2, Mathf.Min(maxDistance_Visible, num - 0.5f), 1 << LayerMask.NameToLayer("Default")))
		{
			return false;
		}
		return true;
	}

	public static Player GetClosestPlayer(Vector3 point, out float distance, List<Player> exclude = null)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Player player = null;
		float num = float.MaxValue;
		distance = 0f;
		foreach (Player player2 in PlayerList)
		{
			if (exclude == null || !exclude.Contains(player2))
			{
				Vector3 val = point - player2.Avatar.CenterPoint;
				float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					player = player2;
				}
			}
		}
		if ((Object)(object)player != (Object)null)
		{
			distance = Mathf.Sqrt(num);
		}
		return player;
	}

	public void SetCapsuleColliderHeight(float normalizedHeight)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		CapCol.height = 2f * normalizedHeight;
		CapCol.center = new Vector3(0f, 1f - (2f - CapCol.height) / 2f, 0f);
	}

	public void SetScale(float scale)
	{
		Scale = scale;
		ApplyScale();
	}

	public void SetScale(float scale, float lerpTime)
	{
		if (lerpScaleRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(lerpScaleRoutine);
		}
		float startScale = Scale;
		lerpScaleRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(LerpScale());
		IEnumerator LerpScale()
		{
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				SetScale(Mathf.Lerp(startScale, scale, i / lerpTime));
				yield return (object)new WaitForEndOfFrame();
			}
			SetScale(scale);
		}
	}

	protected virtual void ApplyScale()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ActiveSkateboard != (Object)null)
		{
			ActiveSkateboard.ApplyPlayerScale();
			((Component)this).transform.localScale = Vector3.one;
		}
		else
		{
			((Component)this).transform.localScale = new Vector3(Scale, Scale, Scale);
		}
	}

	public virtual string GetSaveString()
	{
		return GetPlayerData().GetJson();
	}

	public PlayerData GetPlayerData()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return new PlayerData(SyncAccessor__003CPlayerCode_003Ek__BackingField, ((Component)this).transform.position, ((Component)this).transform.eulerAngles.y, HasCompletedIntro);
	}

	public virtual List<string> WriteData(string parentFolderPath)
	{
		List<string> result = new List<string>();
		((ISaveable)this).WriteSubfile(parentFolderPath, "Inventory", GetInventoryString());
		if ((Object)(object)CurrentAvatarSettings != (Object)null)
		{
			string appearanceString = GetAppearanceString();
			((ISaveable)this).WriteSubfile(parentFolderPath, "Appearance", appearanceString);
		}
		((ISaveable)this).WriteSubfile(parentFolderPath, "Clothing", GetClothingString());
		((ISaveable)this).WriteSubfile(parentFolderPath, "Variables", GetVariablesString());
		return result;
	}

	public string GetInventoryString()
	{
		return new ItemSet(Inventory.ToList()).GetJSON();
	}

	public string GetAppearanceString()
	{
		if ((Object)(object)CurrentAvatarSettings != (Object)null)
		{
			return CurrentAvatarSettings.GetJson();
		}
		return string.Empty;
	}

	public string GetClothingString()
	{
		return new ItemSet(Clothing.ItemSlots.ToList()).GetJSON();
	}

	public string GetVariablesString()
	{
		List<VariableData> list = new List<VariableData>();
		for (int i = 0; i < PlayerVariables.Count; i++)
		{
			if (PlayerVariables[i] != null && PlayerVariables[i].Persistent)
			{
				list.Add(new VariableData(PlayerVariables[i].Name, PlayerVariables[i].GetValue().ToString()));
			}
		}
		return new VariableCollectionData(list.ToArray()).GetJson();
	}

	public virtual void Load(PlayerData data, string containerPath)
	{
		Load(data);
		if (Loader.TryLoadFile(containerPath, "Inventory", out var contents))
		{
			LoadInventory(contents);
		}
		else
		{
			Console.LogWarning("Failed to load player inventory under " + containerPath);
		}
		if (Loader.TryLoadFile(containerPath, "Appearance", out var contents2))
		{
			LoadAppearance(contents2);
		}
		else
		{
			Console.LogWarning("Failed to load player appearance under " + containerPath);
		}
		if (Loader.TryLoadFile(containerPath, "Clothing", out var contents3))
		{
			LoadClothing(contents3);
		}
		else
		{
			Console.LogWarning("Failed to load player clothing under " + containerPath);
		}
		bool flag = false;
		if (Loader.TryLoadFile(containerPath, "Variables", out var contents4))
		{
			VariableCollectionData variableCollectionData = null;
			try
			{
				variableCollectionData = JsonUtility.FromJson<VariableCollectionData>(contents4);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Error loading player variable data: " + ex.Message));
			}
			if (data != null)
			{
				flag = true;
				VariableData[] variables = variableCollectionData.Variables;
				foreach (VariableData variableData in variables)
				{
					if (variableData != null)
					{
						LoadVariable(variableData);
					}
				}
			}
		}
		if (flag)
		{
			return;
		}
		string text = Path.Combine(containerPath, "Variables");
		if (Directory.Exists(text))
		{
			Console.Log("Loading legacy player variables from " + text);
			string[] files = Directory.GetFiles(text);
			VariablesLoader variablesLoader = new VariablesLoader();
			for (int j = 0; j < files.Length; j++)
			{
				if (variablesLoader.TryLoadFile(files[j], out var contents5, autoAddExtension: false))
				{
					VariableData data2 = null;
					try
					{
						data2 = JsonUtility.FromJson<VariableData>(contents5);
					}
					catch (Exception ex2)
					{
						Debug.LogError((object)("Error loading player variable data: " + ex2.Message));
					}
					if (data != null)
					{
						LoadVariable(data2);
					}
				}
			}
		}
		else
		{
			Console.LogWarning("Failed to load player variables under " + containerPath);
		}
	}

	public virtual void Load(PlayerData data)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		playerDataRetrieveReturned = true;
		if (((NetworkBehaviour)this).IsOwner)
		{
			PlayerSingleton<PlayerMovement>.Instance.Teleport(data.Position);
			if (!float.IsNaN(data.Rotation) && !float.IsInfinity(data.Rotation))
			{
				((Component)this).transform.eulerAngles = new Vector3(0f, data.Rotation, 0f);
			}
		}
		HasCompletedIntro = data.IntroCompleted;
	}

	public virtual void LoadInventory(string contentsString)
	{
		if (string.IsNullOrEmpty(contentsString))
		{
			Console.LogWarning("Empty inventory string");
		}
		else
		{
			if (!((NetworkBehaviour)this).IsOwner || !ItemSet.TryDeserialize(contentsString, out var itemSet))
			{
				return;
			}
			for (int i = 0; i < itemSet.Items.Length; i++)
			{
				if (itemSet.Items[i] is CashInstance)
				{
					PlayerSingleton<PlayerInventory>.Instance.cashInstance.SetBalance((itemSet.Items[i] as CashInstance).Balance);
				}
				else if (i < 8)
				{
					PlayerSingleton<PlayerInventory>.Instance.hotbarSlots[i].SetStoredItem(itemSet.Items[i]);
				}
				else
				{
					Console.LogWarning("Hotbar slot out of range");
				}
			}
		}
	}

	public virtual void LoadAppearance(string appearanceString)
	{
		if (string.IsNullOrEmpty(appearanceString))
		{
			Console.LogWarning("Empty appearance string");
			return;
		}
		BasicAvatarSettings basicAvatarSettings = ScriptableObject.CreateInstance<BasicAvatarSettings>();
		JsonUtility.FromJsonOverwrite(appearanceString, (object)basicAvatarSettings);
		SetAppearance(basicAvatarSettings, refreshClothing: false);
	}

	public virtual void LoadClothing(string contentsString)
	{
		DeserializedItemSet itemSet;
		if (string.IsNullOrEmpty(contentsString))
		{
			Console.LogWarning("Empty clothing string");
		}
		else if (((NetworkBehaviour)this).IsOwner && ItemSet.TryDeserialize(contentsString, out itemSet))
		{
			itemSet.LoadTo(Clothing.ItemSlots);
		}
	}

	public void SetRagdolled(bool ragdolled)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (ragdolled == IsRagdolled)
		{
			return;
		}
		IsRagdolled = ragdolled;
		Avatar.SetRagdollPhysicsEnabled(ragdolled, playStandUpAnim: false);
		((Component)Avatar).transform.localEulerAngles = Vector3.zero;
		if (((NetworkBehaviour)this).IsOwner)
		{
			if (IsRagdolled)
			{
				LayerUtility.SetLayerRecursively(((Component)Avatar).gameObject, LayerMask.NameToLayer("Player"));
			}
			else
			{
				LayerUtility.SetLayerRecursively(((Component)Avatar).gameObject, LayerMask.NameToLayer("Invisible"));
			}
		}
		if (IsRagdolled)
		{
			if (onRagdoll != null)
			{
				onRagdoll.Invoke();
			}
		}
		else if (onRagdollEnd != null)
		{
			onRagdollEnd.Invoke();
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public virtual void SendImpact(Impact impact)
	{
		RpcWriter___Server_SendImpact_427288424(impact);
		RpcLogic___SendImpact_427288424(impact);
	}

	[ObserversRpc(RunLocally = true)]
	public virtual void ReceiveImpact(Impact impact)
	{
		RpcWriter___Observers_ReceiveImpact_427288424(impact);
		RpcLogic___ReceiveImpact_427288424(impact);
	}

	public virtual void ProcessImpactForce(Vector3 forcePoint, Vector3 forceDirection, float force)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (force >= 50f)
		{
			Avatar.Animation.Flinch(forceDirection, AvatarAnimation.EFlinchType.Light);
		}
	}

	private void HitByLightning()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		StruckByLightning = true;
		Electrifying.ApplyToAvatar(Avatar);
		if (IsLocalPlayer)
		{
			PlayerSingleton<PlayerMovement>.Instance.MoveSpeedMultiplierStack.Add(new FloatStack.StackEntry("lightning", 1.5f, FloatStack.EStackMode.Multiplicative, 10));
			PlayerSingleton<PlayerMovement>.Instance.Jump();
			PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.AddOverride(10f, 10, "lightning");
			Singleton<PostProcessingManager>.Instance.ColorFilterController.AddOverride(_lightningColorTint, 10, "lightning");
		}
		if (onStruckByLightning != null)
		{
			onStruckByLightning.Invoke();
		}
		new TimedCallback(ResetHitByLightning, 60);
		((MonoBehaviour)this).StartCoroutine(Reset());
		IEnumerator Reset()
		{
			yield return (object)new WaitForSeconds(4f);
			if ((Object)(object)Avatar != (Object)null)
			{
				Electrifying.ClearFromAvatar(Avatar);
			}
		}
	}

	private void ResetHitByLightning()
	{
		StruckByLightning = false;
		if (IsLocalPlayer)
		{
			PlayerSingleton<PlayerMovement>.Instance.MoveSpeedMultiplierStack.Remove("lightning");
			PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.RemoveOverride("lightning");
			Singleton<PostProcessingManager>.Instance.ColorFilterController.RemoveOverride("lightning");
		}
	}

	public virtual void OnDied()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (((NetworkBehaviour)this).Owner.IsLocalClient)
		{
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			((Behaviour)Singleton<HUD>.Instance.canvas).enabled = false;
			ExitAll();
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.rotation, 0f);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement("Dead");
		}
		ClearProduct();
		((Component)NameLabel).gameObject.SetActive(false);
		((Collider)CapCol).enabled = false;
		SetRagdolled(ragdolled: true);
		Avatar.MiddleSpineRB.AddForce(((Component)this).transform.forward * 30f, (ForceMode)2);
		Avatar.MiddleSpineRB.AddRelativeTorque(new Vector3(0f, Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 10f, (ForceMode)2);
		Visibility.ClearStates();
		if (CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None)
		{
			IsArrested = true;
		}
		if (((NetworkBehaviour)this).Owner.IsLocalClient)
		{
			Singleton<DeathScreen>.Instance.Open();
		}
	}

	public virtual void OnRevived()
	{
		SetRagdolled(ragdolled: false);
		if (!((NetworkBehaviour)this).Owner.IsLocalClient)
		{
			((Component)NameLabel).gameObject.SetActive(true);
		}
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: false, returnToOriginalRotation: false);
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement("Dead");
		((Collider)CapCol).enabled = true;
	}

	[ServerRpc(RunLocally = true)]
	public void Arrest_Server()
	{
		RpcWriter___Server_Arrest_Server_2166136261();
		RpcLogic___Arrest_Server_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void Arrest_Client()
	{
		RpcWriter___Observers_Arrest_Client_2166136261();
		RpcLogic___Arrest_Client_2166136261();
	}

	[ServerRpc(RunLocally = true)]
	public void Free_Server()
	{
		RpcWriter___Server_Free_Server_2166136261();
		RpcLogic___Free_Server_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void Free_Client()
	{
		RpcWriter___Observers_Free_Client_2166136261();
		RpcLogic___Free_Client_2166136261();
	}

	[ServerRpc(RunLocally = true)]
	public void SendPassOut()
	{
		RpcWriter___Server_SendPassOut_2166136261();
		RpcLogic___SendPassOut_2166136261();
	}

	[ObserversRpc(RunLocally = true, ExcludeOwner = true)]
	public void PassOut()
	{
		RpcWriter___Observers_PassOut_2166136261();
		RpcLogic___PassOut_2166136261();
	}

	[ServerRpc(RunLocally = true)]
	public void SendPassOutRecovery()
	{
		RpcWriter___Server_SendPassOutRecovery_2166136261();
		RpcLogic___SendPassOutRecovery_2166136261();
	}

	[ObserversRpc(RunLocally = true, ExcludeOwner = true)]
	public void PassOutRecovery()
	{
		RpcWriter___Observers_PassOutRecovery_2166136261();
		RpcLogic___PassOutRecovery_2166136261();
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SendEquippable_Networked(string assetPath)
	{
		RpcWriter___Server_SendEquippable_Networked_3615296227(assetPath);
		RpcLogic___SendEquippable_Networked_3615296227(assetPath);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetEquippable_Networked(string assetPath)
	{
		RpcWriter___Observers_SetEquippable_Networked_3615296227(assetPath);
		RpcLogic___SetEquippable_Networked_3615296227(assetPath);
	}

	[ServerRpc(RunLocally = true)]
	public void SendEquippableMessage_Networked(string message, int receipt)
	{
		RpcWriter___Server_SendEquippableMessage_Networked_3643459082(message, receipt);
		RpcLogic___SendEquippableMessage_Networked_3643459082(message, receipt);
	}

	[ObserversRpc(RunLocally = true)]
	private void ReceiveEquippableMessage_Networked(string message, int receipt)
	{
		RpcWriter___Observers_ReceiveEquippableMessage_Networked_3643459082(message, receipt);
		RpcLogic___ReceiveEquippableMessage_Networked_3643459082(message, receipt);
	}

	[ServerRpc(RunLocally = true)]
	public void SendEquippableMessage_Networked_Vector(string message, int receipt, Vector3 data)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_SendEquippableMessage_Networked_Vector_3190074053(message, receipt, data);
		RpcLogic___SendEquippableMessage_Networked_Vector_3190074053(message, receipt, data);
	}

	[ObserversRpc(RunLocally = true)]
	private void ReceiveEquippableMessage_Networked_Vector(string message, int receipt, Vector3 data)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_ReceiveEquippableMessage_Networked_Vector_3190074053(message, receipt, data);
		RpcLogic___ReceiveEquippableMessage_Networked_Vector_3190074053(message, receipt, data);
	}

	public IEquippedItemHandler Equip(EquippableData equippable)
	{
		return _networkedEquipper.Equip(equippable);
	}

	public IEquippedItemHandler Equip(BaseItemInstance item)
	{
		return _networkedEquipper.Equip(item);
	}

	public IEquippedItemHandler EquipLocal(EquippableData equippable)
	{
		return _networkedEquipper.Equip(equippable, networked: false);
	}

	public IEquippedItemHandler EquipLocal(BaseItemInstance item)
	{
		return _networkedEquipper.Equip(item, networked: false);
	}

	public void Unequip(IEquippedItemHandler equippedItem)
	{
		_networkedEquipper.Unequip(equippedItem);
	}

	public void UnequipAll()
	{
		_networkedEquipper.UnequipAll();
	}

	public void SetThirdPersonMeshesVisibility(bool visible)
	{
		ThirdPersonMeshesVisibleToLocalPlayer = visible;
		if (this.OnThirdPersonMeshesVisibilityChanged != null)
		{
			this.OnThirdPersonMeshesVisibilityChanged(visible);
		}
	}

	[ServerRpc(RunLocally = true)]
	public void SendAnimationTrigger(string trigger)
	{
		RpcWriter___Server_SendAnimationTrigger_3615296227(trigger);
		RpcLogic___SendAnimationTrigger_3615296227(trigger);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetAnimationTrigger_Networked(NetworkConnection conn, string trigger)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetAnimationTrigger_Networked_2971853958(conn, trigger);
			RpcLogic___SetAnimationTrigger_Networked_2971853958(conn, trigger);
		}
		else
		{
			RpcWriter___Target_SetAnimationTrigger_Networked_2971853958(conn, trigger);
		}
	}

	public void SetAnimationTrigger(string trigger)
	{
		Avatar.Animation.SetTrigger(trigger);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void ResetAnimationTrigger_Networked(NetworkConnection conn, string trigger)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ResetAnimationTrigger_Networked_2971853958(conn, trigger);
			RpcLogic___ResetAnimationTrigger_Networked_2971853958(conn, trigger);
		}
		else
		{
			RpcWriter___Target_ResetAnimationTrigger_Networked_2971853958(conn, trigger);
		}
	}

	public void ResetAnimationTrigger(string trigger)
	{
		Avatar.Animation.ResetTrigger(trigger);
	}

	[ServerRpc(RunLocally = true)]
	public void SendAnimationBool(string name, bool val)
	{
		RpcWriter___Server_SendAnimationBool_310431262(name, val);
		RpcLogic___SendAnimationBool_310431262(name, val);
	}

	[ObserversRpc(RunLocally = true)]
	public void SetAnimationBool(string name, bool val)
	{
		RpcWriter___Observers_SetAnimationBool_310431262(name, val);
		RpcLogic___SetAnimationBool_310431262(name, val);
	}

	[ObserversRpc]
	public void Taze()
	{
		RpcWriter___Observers_Taze_2166136261();
	}

	[ServerRpc(RunLocally = true)]
	public void SetInventoryItem(int index, ItemInstance item)
	{
		RpcWriter___Server_SetInventoryItem_2317364410(index, item);
		RpcLogic___SetInventoryItem_2317364410(index, item);
	}

	[ServerRpc(RunLocally = true)]
	public void SetEquippedSlotIndex(int index)
	{
		RpcWriter___Server_SetEquippedSlotIndex_3316948804(index);
		RpcLogic___SetEquippedSlotIndex_3316948804(index);
	}

	public ItemInstance GetEquippedItem()
	{
		if (EquippedItemSlotIndex == -1)
		{
			return null;
		}
		return Inventory[EquippedItemSlotIndex].ItemInstance;
	}

	[ObserversRpc]
	public void RemoveEquippedItemFromInventory(string id, int amount)
	{
		RpcWriter___Observers_RemoveEquippedItemFromInventory_3643459082(id, amount);
	}

	private void GetNetworth(MoneyManager.FloatContainer container)
	{
		for (int i = 0; i < Inventory.Length; i++)
		{
			if (Inventory[i].ItemInstance != null)
			{
				container.ChangeValue(((BaseItemInstance)Inventory[i].ItemInstance).GetMonetaryValue());
			}
		}
	}

	[ServerRpc(RunLocally = true)]
	public void SendAppearance(BasicAvatarSettings settings)
	{
		RpcWriter___Server_SendAppearance_3281254764(settings);
		RpcLogic___SendAppearance_3281254764(settings);
	}

	[ObserversRpc(RunLocally = true)]
	public void SetAppearance(BasicAvatarSettings settings, bool refreshClothing)
	{
		RpcWriter___Observers_SetAppearance_2139595489(settings, refreshClothing);
		RpcLogic___SetAppearance_2139595489(settings, refreshClothing);
	}

	public void MountSkateboard(Skateboard board)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		Collider[] componentsInChildren = ((Component)this).GetComponentsInChildren<Collider>(true);
		foreach (Collider val in componentsInChildren)
		{
			Collider[] mainColliders = board.MainColliders;
			foreach (Collider val2 in mainColliders)
			{
				Physics.IgnoreCollision(val, val2, true);
			}
		}
		SendMountedSkateboard(((NetworkBehaviour)board).NetworkObject);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.rotation, 0f);
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.SetCameraMode(PlayerCamera.ECameraMode.Skateboard);
		((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position = ((Component)((Component)PlayerSingleton<PlayerCamera>.Instance).transform).transform.position - ((Component)this).transform.forward * 0.5f;
		SetVisibleToLocalPlayer(vis: true);
		((Collider)CapCol).enabled = true;
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		((Collider)PlayerSingleton<PlayerMovement>.Instance.Controller).enabled = false;
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		Singleton<InputPromptsCanvas>.Instance.LoadModule("skateboard");
	}

	[ServerRpc(RunLocally = true)]
	private void SendMountedSkateboard(NetworkObject skateboardObj)
	{
		RpcWriter___Server_SendMountedSkateboard_3323014238(skateboardObj);
		RpcLogic___SendMountedSkateboard_3323014238(skateboardObj);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetMountedSkateboard(NetworkObject skateboardObj)
	{
		RpcWriter___Observers_SetMountedSkateboard_3323014238(skateboardObj);
		RpcLogic___SetMountedSkateboard_3323014238(skateboardObj);
	}

	public void DismountSkateboard()
	{
		SendMountedSkateboard(null);
		SetVisibleToLocalPlayer(vis: false);
		((Collider)CapCol).enabled = true;
		SetCapsuleColliderHeight(1f);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		((Collider)PlayerSingleton<PlayerMovement>.Instance.Controller).enabled = true;
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: true, returnToOriginalRotation: false);
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
		PlayerSingleton<PlayerCamera>.Instance.SetCameraMode(PlayerCamera.ECameraMode.Default);
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
	}

	public void ConsumeProduct(ProductItemInstance product)
	{
		SendConsumeProduct(product);
		ConsumeProductInternal(product);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendConsumeProduct(ProductItemInstance product)
	{
		RpcWriter___Server_SendConsumeProduct_2622925554(product);
	}

	[ObserversRpc]
	private void ReceiveConsumeProduct(ProductItemInstance product)
	{
		RpcWriter___Observers_ReceiveConsumeProduct_2622925554(product);
	}

	private void ConsumeProductInternal(ProductItemInstance product)
	{
		if (ConsumedProduct != null)
		{
			ClearProduct();
		}
		ConsumedProduct = product;
		TimeSinceProductConsumed = 0;
		product.ApplyEffectsToPlayer(this);
	}

	public void ClearProduct()
	{
		if (ConsumedProduct != null)
		{
			ConsumedProduct.ClearEffectsFromPlayer(this);
			ConsumedProduct = null;
		}
	}

	private void CreatePlayerVariables()
	{
		if (VariableDict.Count <= 0)
		{
			Console.Log("Creating player variables for " + SyncAccessor__003CPlayerName_003Ek__BackingField + " (" + SyncAccessor__003CPlayerCode_003Ek__BackingField + ")");
			NetworkSingleton<VariableDatabase>.Instance.CreatePlayerVariables(this);
			if (InstanceFinder.IsServer)
			{
				SetVariableValue("IsServer", true.ToString());
			}
		}
	}

	public BaseVariable GetVariable(string variableName)
	{
		variableName = variableName.ToLower();
		if (VariableDict.ContainsKey(variableName))
		{
			return VariableDict[variableName];
		}
		Console.LogWarning("Failed to find variable with name: " + variableName);
		return null;
	}

	public T GetValue<T>(string variableName)
	{
		variableName = variableName.ToLower();
		if (VariableDict.ContainsKey(variableName))
		{
			return (T)VariableDict[variableName].GetValue();
		}
		Console.LogError("Variable with name " + variableName + " does not exist in the database.");
		return default(T);
	}

	public void SetVariableValue(string variableName, string value, bool network = true)
	{
		variableName = variableName.ToLower();
		if (VariableDict.ContainsKey(variableName))
		{
			VariableDict[variableName].SetValue(value, network);
		}
		else
		{
			Console.LogWarning("Failed to find variable with name: " + variableName);
		}
	}

	public void AddVariable(BaseVariable variable)
	{
		if (VariableDict.ContainsKey(variable.Name.ToLower()))
		{
			Console.LogError("Variable with name " + variable.Name + " already exists in the database.");
			return;
		}
		PlayerVariables.Add(variable);
		VariableDict.Add(variable.Name.ToLower(), variable);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendValue(string variableName, string value, bool sendToOwner)
	{
		RpcWriter___Server_SendValue_3589193952(variableName, value, sendToOwner);
		RpcLogic___SendValue_3589193952(variableName, value, sendToOwner);
	}

	[TargetRpc]
	private void ReceiveValue(NetworkConnection conn, string variableName, string value)
	{
		RpcWriter___Target_ReceiveValue_3895153758(conn, variableName, value);
	}

	private void ReceiveValue(string variableName, string value)
	{
		variableName = variableName.ToLower();
		if (VariableDict.ContainsKey(variableName))
		{
			VariableDict[variableName].SetValue(value, replicate: false);
		}
		else
		{
			Console.LogWarning("Failed to find player variable with name: " + variableName);
		}
	}

	public void LoadVariable(VariableData data)
	{
		BaseVariable variable = GetVariable(data.Name);
		if (variable == null)
		{
			Console.LogWarning("Failed to find variable with name: " + data.Name);
		}
		else
		{
			variable.SetValue(data.Value);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendWorldSpaceDialogue(string text, float duration)
	{
		RpcWriter___Server_SendWorldSpaceDialogue_606697822(text, duration);
	}

	[ObserversRpc(RunLocally = true)]
	private void ShowWorldSpaceDialogue(string text, float duration)
	{
		RpcWriter___Observers_ShowWorldSpaceDialogue_606697822(text, duration);
		RpcLogic___ShowWorldSpaceDialogue_606697822(text, duration);
	}

	public Player()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		PlayerName = "Player";
		PlayerCode = string.Empty;
		ThirdPersonMeshesVisibleToLocalPlayer = true;
		TimeSinceVehicleExit = 1000f;
		Scale = 1f;
		CameraPosition = Vector3.zero;
		CameraRotation = Quaternion.identity;
		Inventory = new ItemSlot[9];
		EquippedItemSlotIndex = -1;
		loader = new PlayerLoader();
		LocalExtraFiles = new List<string> { "Inventory", "Appearance", "Clothing", "Variables" };
		LocalExtraFolders = new List<string>();
		HasChanged = true;
		PlayerVariables = new List<BaseVariable>();
		VariableDict = new Dictionary<string, BaseVariable>();
		standingScale = 1f;
		ragdollForceComponents = new List<ConstantForce>();
		impactHistory = new List<int>();
		seizureRotations = new List<Quaternion>();
		equippableMessageIDHistory = new List<int>();
		((NetworkBehaviour)this)._002Ector();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Expected O, but got Unknown
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Expected O, but got Unknown
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected O, but got Unknown
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Expected O, but got Unknown
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Expected O, but got Unknown
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Expected O, but got Unknown
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Expected O, but got Unknown
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Expected O, but got Unknown
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Expected O, but got Unknown
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Expected O, but got Unknown
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Expected O, but got Unknown
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Expected O, but got Unknown
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Expected O, but got Unknown
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Expected O, but got Unknown
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Expected O, but got Unknown
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Expected O, but got Unknown
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Expected O, but got Unknown
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Expected O, but got Unknown
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Expected O, but got Unknown
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Expected O, but got Unknown
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Expected O, but got Unknown
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Expected O, but got Unknown
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Expected O, but got Unknown
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Expected O, but got Unknown
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Expected O, but got Unknown
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Expected O, but got Unknown
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Expected O, but got Unknown
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Expected O, but got Unknown
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Expected O, but got Unknown
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Expected O, but got Unknown
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Expected O, but got Unknown
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Expected O, but got Unknown
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Expected O, but got Unknown
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Expected O, but got Unknown
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Expected O, but got Unknown
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Expected O, but got Unknown
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Expected O, but got Unknown
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Expected O, but got Unknown
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d6: Expected O, but got Unknown
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Expected O, but got Unknown
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Expected O, but got Unknown
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Expected O, but got Unknown
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Expected O, but got Unknown
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Expected O, but got Unknown
		//IL_0556: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Expected O, but got Unknown
		//IL_056d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0577: Expected O, but got Unknown
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		//IL_058e: Expected O, but got Unknown
		//IL_059b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Expected O, but got Unknown
		//IL_05b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bc: Expected O, but got Unknown
		//IL_05c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d3: Expected O, but got Unknown
		//IL_05e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ea: Expected O, but got Unknown
		//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0601: Expected O, but got Unknown
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0618: Expected O, but got Unknown
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_062f: Expected O, but got Unknown
		//IL_063c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0646: Expected O, but got Unknown
		//IL_0653: Unknown result type (might be due to invalid IL or missing references)
		//IL_065d: Expected O, but got Unknown
		//IL_066a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0674: Expected O, but got Unknown
		//IL_0681: Unknown result type (might be due to invalid IL or missing references)
		//IL_068b: Expected O, but got Unknown
		//IL_0698: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a2: Expected O, but got Unknown
		//IL_06af: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b9: Expected O, but got Unknown
		//IL_06c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d0: Expected O, but got Unknown
		//IL_06dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e7: Expected O, but got Unknown
		//IL_06f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fe: Expected O, but got Unknown
		//IL_070b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0715: Expected O, but got Unknown
		//IL_0722: Unknown result type (might be due to invalid IL or missing references)
		//IL_072c: Expected O, but got Unknown
		//IL_0734: Unknown result type (might be due to invalid IL or missing references)
		//IL_073e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EPlayerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EPlayerScripts_002EPlayerAssembly_002DCSharp_002Edll_Excuted = true;
			syncVar____003CCameraRotation_003Ek__BackingField = new SyncVar<Quaternion>((NetworkBehaviour)(object)this, 6u, (WritePermission)0, (ReadPermission)0, 0.1f, (Channel)1, CameraRotation);
			syncVar____003CCameraPosition_003Ek__BackingField = new SyncVar<Vector3>((NetworkBehaviour)(object)this, 5u, (WritePermission)0, (ReadPermission)0, 0.1f, (Channel)1, CameraPosition);
			syncVar____003CIsReadyToSleep_003Ek__BackingField = new SyncVar<bool>((NetworkBehaviour)(object)this, 4u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, IsReadyToSleep);
			syncVar____003CCurrentBed_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 3u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, CurrentBed);
			syncVar____003CCurrentVehicle_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 2u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, CurrentVehicle);
			syncVar____003CCurrentVehicle_003Ek__BackingField.OnChange += CurrentVehicleChanged;
			syncVar____003CPlayerCode_003Ek__BackingField = new SyncVar<string>((NetworkBehaviour)(object)this, 1u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, PlayerCode);
			syncVar____003CPlayerName_003Ek__BackingField = new SyncVar<string>((NetworkBehaviour)(object)this, 0u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, PlayerName);
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_set_CurrentVehicle_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(1u, new ServerRpcDelegate(RpcReader___Server_set_CurrentBed_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_set_IsSkating_1140765316));
			((NetworkBehaviour)this).RegisterServerRpc(3u, new ServerRpcDelegate(RpcReader___Server_set_CameraPosition_4276783012));
			((NetworkBehaviour)this).RegisterServerRpc(4u, new ServerRpcDelegate(RpcReader___Server_set_CameraRotation_3429297120));
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_RequestSavePlayer_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_ReturnSaveRequest_214505783));
			((NetworkBehaviour)this).RegisterTargetRpc(7u, new ClientRpcDelegate(RpcReader___Target_ReturnSaveRequest_214505783));
			((NetworkBehaviour)this).RegisterObserversRpc(8u, new ClientRpcDelegate(RpcReader___Observers_HostExitedGame_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(9u, new ServerRpcDelegate(RpcReader___Server_SendPlayerNameData_586648380));
			((NetworkBehaviour)this).RegisterServerRpc(10u, new ServerRpcDelegate(RpcReader___Server_RequestPlayerData_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(11u, new ClientRpcDelegate(RpcReader___Observers_ReceivePlayerData_3244732873));
			((NetworkBehaviour)this).RegisterTargetRpc(12u, new ClientRpcDelegate(RpcReader___Target_ReceivePlayerData_3244732873));
			((NetworkBehaviour)this).RegisterObserversRpc(13u, new ClientRpcDelegate(RpcReader___Observers_ReceivePlayerNameData_3895153758));
			((NetworkBehaviour)this).RegisterTargetRpc(14u, new ClientRpcDelegate(RpcReader___Target_ReceivePlayerNameData_3895153758));
			((NetworkBehaviour)this).RegisterServerRpc(15u, new ServerRpcDelegate(RpcReader___Server_SetFlashlightOn_Server_1140765316));
			((NetworkBehaviour)this).RegisterObserversRpc(16u, new ClientRpcDelegate(RpcReader___Observers_SetFlashlightOn_Client_1140765316));
			((NetworkBehaviour)this).RegisterObserversRpc(17u, new ClientRpcDelegate(RpcReader___Observers_PlayJumpAnimation_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(18u, new ServerRpcDelegate(RpcReader___Server_SendCrouched_1140765316));
			((NetworkBehaviour)this).RegisterTargetRpc(19u, new ClientRpcDelegate(RpcReader___Target_ReceiveCrouched_214505783));
			((NetworkBehaviour)this).RegisterObserversRpc(20u, new ClientRpcDelegate(RpcReader___Observers_ReceiveCrouched_214505783));
			((NetworkBehaviour)this).RegisterServerRpc(21u, new ServerRpcDelegate(RpcReader___Server_SendAvatarSettings_4281687581));
			((NetworkBehaviour)this).RegisterObserversRpc(22u, new ClientRpcDelegate(RpcReader___Observers_SetAvatarSettings_4281687581));
			((NetworkBehaviour)this).RegisterObserversRpc(23u, new ClientRpcDelegate(RpcReader___Observers_SetVisible_Networked_1140765316));
			((NetworkBehaviour)this).RegisterServerRpc(24u, new ServerRpcDelegate(RpcReader___Server_SetReadyToSleep_1140765316));
			((NetworkBehaviour)this).RegisterServerRpc(25u, new ServerRpcDelegate(RpcReader___Server_SendPunch_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(26u, new ClientRpcDelegate(RpcReader___Observers_Punch_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(27u, new ServerRpcDelegate(RpcReader___Server_MarkIntroCompleted_3281254764));
			((NetworkBehaviour)this).RegisterServerRpc(28u, new ServerRpcDelegate(RpcReader___Server_SendImpact_427288424));
			((NetworkBehaviour)this).RegisterObserversRpc(29u, new ClientRpcDelegate(RpcReader___Observers_ReceiveImpact_427288424));
			((NetworkBehaviour)this).RegisterServerRpc(30u, new ServerRpcDelegate(RpcReader___Server_Arrest_Server_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(31u, new ClientRpcDelegate(RpcReader___Observers_Arrest_Client_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(32u, new ServerRpcDelegate(RpcReader___Server_Free_Server_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(33u, new ClientRpcDelegate(RpcReader___Observers_Free_Client_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(34u, new ServerRpcDelegate(RpcReader___Server_SendPassOut_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(35u, new ClientRpcDelegate(RpcReader___Observers_PassOut_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(36u, new ServerRpcDelegate(RpcReader___Server_SendPassOutRecovery_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(37u, new ClientRpcDelegate(RpcReader___Observers_PassOutRecovery_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(38u, new ServerRpcDelegate(RpcReader___Server_SendEquippable_Networked_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(39u, new ClientRpcDelegate(RpcReader___Observers_SetEquippable_Networked_3615296227));
			((NetworkBehaviour)this).RegisterServerRpc(40u, new ServerRpcDelegate(RpcReader___Server_SendEquippableMessage_Networked_3643459082));
			((NetworkBehaviour)this).RegisterObserversRpc(41u, new ClientRpcDelegate(RpcReader___Observers_ReceiveEquippableMessage_Networked_3643459082));
			((NetworkBehaviour)this).RegisterServerRpc(42u, new ServerRpcDelegate(RpcReader___Server_SendEquippableMessage_Networked_Vector_3190074053));
			((NetworkBehaviour)this).RegisterObserversRpc(43u, new ClientRpcDelegate(RpcReader___Observers_ReceiveEquippableMessage_Networked_Vector_3190074053));
			((NetworkBehaviour)this).RegisterServerRpc(44u, new ServerRpcDelegate(RpcReader___Server_SendAnimationTrigger_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(45u, new ClientRpcDelegate(RpcReader___Observers_SetAnimationTrigger_Networked_2971853958));
			((NetworkBehaviour)this).RegisterTargetRpc(46u, new ClientRpcDelegate(RpcReader___Target_SetAnimationTrigger_Networked_2971853958));
			((NetworkBehaviour)this).RegisterObserversRpc(47u, new ClientRpcDelegate(RpcReader___Observers_ResetAnimationTrigger_Networked_2971853958));
			((NetworkBehaviour)this).RegisterTargetRpc(48u, new ClientRpcDelegate(RpcReader___Target_ResetAnimationTrigger_Networked_2971853958));
			((NetworkBehaviour)this).RegisterServerRpc(49u, new ServerRpcDelegate(RpcReader___Server_SendAnimationBool_310431262));
			((NetworkBehaviour)this).RegisterObserversRpc(50u, new ClientRpcDelegate(RpcReader___Observers_SetAnimationBool_310431262));
			((NetworkBehaviour)this).RegisterObserversRpc(51u, new ClientRpcDelegate(RpcReader___Observers_Taze_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(52u, new ServerRpcDelegate(RpcReader___Server_SetInventoryItem_2317364410));
			((NetworkBehaviour)this).RegisterServerRpc(53u, new ServerRpcDelegate(RpcReader___Server_SetEquippedSlotIndex_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(54u, new ClientRpcDelegate(RpcReader___Observers_RemoveEquippedItemFromInventory_3643459082));
			((NetworkBehaviour)this).RegisterServerRpc(55u, new ServerRpcDelegate(RpcReader___Server_SendAppearance_3281254764));
			((NetworkBehaviour)this).RegisterObserversRpc(56u, new ClientRpcDelegate(RpcReader___Observers_SetAppearance_2139595489));
			((NetworkBehaviour)this).RegisterServerRpc(57u, new ServerRpcDelegate(RpcReader___Server_SendMountedSkateboard_3323014238));
			((NetworkBehaviour)this).RegisterObserversRpc(58u, new ClientRpcDelegate(RpcReader___Observers_SetMountedSkateboard_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(59u, new ServerRpcDelegate(RpcReader___Server_SendConsumeProduct_2622925554));
			((NetworkBehaviour)this).RegisterObserversRpc(60u, new ClientRpcDelegate(RpcReader___Observers_ReceiveConsumeProduct_2622925554));
			((NetworkBehaviour)this).RegisterServerRpc(61u, new ServerRpcDelegate(RpcReader___Server_SendValue_3589193952));
			((NetworkBehaviour)this).RegisterTargetRpc(62u, new ClientRpcDelegate(RpcReader___Target_ReceiveValue_3895153758));
			((NetworkBehaviour)this).RegisterServerRpc(63u, new ServerRpcDelegate(RpcReader___Server_SendWorldSpaceDialogue_606697822));
			((NetworkBehaviour)this).RegisterObserversRpc(64u, new ClientRpcDelegate(RpcReader___Observers_ShowWorldSpaceDialogue_606697822));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EPlayerScripts_002EPlayer));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EPlayerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EPlayerScripts_002EPlayerAssembly_002DCSharp_002Edll_Excuted = true;
			((SyncBase)syncVar____003CCameraRotation_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CCameraPosition_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CIsReadyToSleep_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CCurrentBed_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CCurrentVehicle_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CPlayerCode_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CPlayerName_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_set_CurrentVehicle_3323014238(NetworkObject value)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteNetworkObject(value);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	[SpecialName]
	public void RpcLogic___set_CurrentVehicle_3323014238(NetworkObject value)
	{
		this.sync___set_value__003CCurrentVehicle_003Ek__BackingField(value, true);
	}

	private void RpcReader___Server_set_CurrentVehicle_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject value = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___set_CurrentVehicle_3323014238(value);
		}
	}

	private void RpcWriter___Server_set_CurrentBed_3323014238(NetworkObject value)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteNetworkObject(value);
			((NetworkBehaviour)this).SendServerRpc(1u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	[SpecialName]
	public void RpcLogic___set_CurrentBed_3323014238(NetworkObject value)
	{
		this.sync___set_value__003CCurrentBed_003Ek__BackingField(value, true);
	}

	private void RpcReader___Server_set_CurrentBed_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject value = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn))
		{
			RpcLogic___set_CurrentBed_3323014238(value);
		}
	}

	private void RpcWriter___Server_set_IsSkating_1140765316(bool value)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteBoolean(value);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	[SpecialName]
	public void RpcLogic___set_IsSkating_1140765316(bool value)
	{
		IsSkating = value;
	}

	private void RpcReader___Server_set_IsSkating_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool value = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn))
		{
			RpcLogic___set_IsSkating_1140765316(value);
		}
	}

	private void RpcWriter___Server_set_CameraPosition_4276783012(Vector3 value)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteVector3(value);
			((NetworkBehaviour)this).SendServerRpc(3u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	[SpecialName]
	public void RpcLogic___set_CameraPosition_4276783012(Vector3 value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		this.sync___set_value__003CCameraPosition_003Ek__BackingField(value, true);
	}

	private void RpcReader___Server_set_CameraPosition_4276783012(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 value = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn))
		{
			RpcLogic___set_CameraPosition_4276783012(value);
		}
	}

	private void RpcWriter___Server_set_CameraRotation_3429297120(Quaternion value)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteQuaternion(value, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(4u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	[SpecialName]
	public void RpcLogic___set_CameraRotation_3429297120(Quaternion value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		this.sync___set_value__003CCameraRotation_003Ek__BackingField(value, true);
	}

	private void RpcReader___Server_set_CameraRotation_3429297120(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Quaternion value = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn))
		{
			RpcLogic___set_CameraRotation_3429297120(value);
		}
	}

	private void RpcWriter___Server_RequestSavePlayer_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___RequestSavePlayer_2166136261()
	{
		playerSaveRequestReturned = false;
		if (InstanceFinder.IsServer)
		{
			Console.Log("Save request received");
			Singleton<PlayerManager>.Instance.SavePlayer(this);
			ReturnSaveRequest(((NetworkBehaviour)this).Owner, successful: true);
		}
	}

	private void RpcReader___Server_RequestSavePlayer_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___RequestSavePlayer_2166136261();
		}
	}

	private void RpcWriter___Observers_ReturnSaveRequest_214505783(NetworkConnection conn, bool successful)
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
			((Writer)writer).WriteBoolean(successful);
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReturnSaveRequest_214505783(NetworkConnection conn, bool successful)
	{
		Console.Log("Save request returned. Successful: " + successful);
		playerSaveRequestReturned = true;
	}

	private void RpcReader___Observers_ReturnSaveRequest_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool successful = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReturnSaveRequest_214505783(null, successful);
		}
	}

	private void RpcWriter___Target_ReturnSaveRequest_214505783(NetworkConnection conn, bool successful)
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
			((Writer)writer).WriteBoolean(successful);
			((NetworkBehaviour)this).SendTargetRpc(7u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ReturnSaveRequest_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool successful = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReturnSaveRequest_214505783(((NetworkBehaviour)this).LocalConnection, successful);
		}
	}

	private void RpcWriter___Observers_HostExitedGame_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(8u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___HostExitedGame_2166136261()
	{
		if (!InstanceFinder.IsServer && (!Singleton<LoadManager>.InstanceExists || (!Singleton<LoadManager>.Instance.IsLoading && Singleton<LoadManager>.Instance.IsGameLoaded)))
		{
			Console.Log("Host exited game");
			Singleton<LoadManager>.Instance.ExitToMenu(null, new MainMenuPopup.Data("Exited Game", "Host left the game", isBad: false));
		}
	}

	private void RpcReader___Observers_HostExitedGame_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___HostExitedGame_2166136261();
		}
	}

	private void RpcWriter___Server_SendPlayerNameData_586648380(string playerName, ulong id)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteString(playerName);
			((Writer)writer).WriteUInt64(id, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(9u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendPlayerNameData_586648380(string playerName, ulong id)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		ReceivePlayerNameData(null, playerName, id.ToString());
		if (!Application.isEditor && !Debug.isDebugBuild && SteamManager.Initialized && (int)SteamFriends.GetFriendRelationship(new CSteamID(id)) != 3 && !((NetworkBehaviour)this).Owner.IsLocalClient)
		{
			Console.LogError("Player " + playerName + " is not friends with the host. Kicking from game.");
			((NetworkBehaviour)this).Owner.Kick((KickReason)0, (LoggingType)2, "Not friends with host");
		}
	}

	private void RpcReader___Server_SendPlayerNameData_586648380(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string playerName = ((Reader)PooledReader0).ReadString();
		ulong id = ((Reader)PooledReader0).ReadUInt64((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___SendPlayerNameData_586648380(playerName, id);
		}
	}

	private void RpcWriter___Server_RequestPlayerData_3615296227(string playerCode)
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
			((NetworkBehaviour)this).SendServerRpc(10u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___RequestPlayerData_3615296227(string playerCode)
	{
		Singleton<PlayerManager>.Instance.TryGetPlayerData(playerCode, out var data, out var inventoryString, out var appearanceString, out var clothingString, out var variables);
		Console.Log("Sending player data for " + playerCode + " (" + data?.ToString() + ")");
		ReceivePlayerData(null, data, inventoryString, appearanceString, clothingString, variables);
	}

	private void RpcReader___Server_RequestPlayerData_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string playerCode = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___RequestPlayerData_3615296227(playerCode);
		}
	}

	private void RpcWriter___Observers_ReceivePlayerData_3244732873(NetworkConnection conn, PlayerData data, string inventoryString, string appearanceString, string clothigString, VariableData[] vars)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002EDatas_002EPlayerDataFishNet_002ESerializing_002EGenerated((Writer)(object)writer, data);
			((Writer)writer).WriteString(inventoryString);
			((Writer)writer).WriteString(appearanceString);
			((Writer)writer).WriteString(clothigString);
			GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002EDatas_002EVariableData_005B_005DFishNet_002ESerializing_002EGenerated((Writer)(object)writer, vars);
			((NetworkBehaviour)this).SendObserversRpc(11u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___ReceivePlayerData_3244732873(NetworkConnection conn, PlayerData data, string inventoryString, string appearanceString, string clothigString, VariableData[] vars)
	{
		playerDataRetrieveReturned = true;
		if (data != null)
		{
			Load(data);
			if (!string.IsNullOrEmpty(inventoryString))
			{
				LoadInventory(inventoryString);
			}
			if (!string.IsNullOrEmpty(appearanceString))
			{
				LoadAppearance(appearanceString);
			}
			if (!string.IsNullOrEmpty(clothigString))
			{
				LoadClothing(clothigString);
			}
		}
		else if (((NetworkBehaviour)this).IsOwner)
		{
			Console.Log("No player data found for this player; first time joining");
		}
		if (!((NetworkBehaviour)this).IsOwner)
		{
			return;
		}
		if (vars != null)
		{
			foreach (VariableData data2 in vars)
			{
				LoadVariable(data2);
			}
		}
		PlayerLoaded();
	}

	private void RpcReader___Observers_ReceivePlayerData_3244732873(PooledReader PooledReader0, Channel channel)
	{
		PlayerData data = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002EDatas_002EPlayerDataFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		string inventoryString = ((Reader)PooledReader0).ReadString();
		string appearanceString = ((Reader)PooledReader0).ReadString();
		string clothigString = ((Reader)PooledReader0).ReadString();
		VariableData[] vars = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002EDatas_002EVariableData_005B_005DFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceivePlayerData_3244732873(null, data, inventoryString, appearanceString, clothigString, vars);
		}
	}

	private void RpcWriter___Target_ReceivePlayerData_3244732873(NetworkConnection conn, PlayerData data, string inventoryString, string appearanceString, string clothigString, VariableData[] vars)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002EDatas_002EPlayerDataFishNet_002ESerializing_002EGenerated((Writer)(object)writer, data);
			((Writer)writer).WriteString(inventoryString);
			((Writer)writer).WriteString(appearanceString);
			((Writer)writer).WriteString(clothigString);
			GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002EDatas_002EVariableData_005B_005DFishNet_002ESerializing_002EGenerated((Writer)(object)writer, vars);
			((NetworkBehaviour)this).SendTargetRpc(12u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ReceivePlayerData_3244732873(PooledReader PooledReader0, Channel channel)
	{
		PlayerData data = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002EDatas_002EPlayerDataFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		string inventoryString = ((Reader)PooledReader0).ReadString();
		string appearanceString = ((Reader)PooledReader0).ReadString();
		string clothigString = ((Reader)PooledReader0).ReadString();
		VariableData[] vars = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002EDatas_002EVariableData_005B_005DFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceivePlayerData_3244732873(((NetworkBehaviour)this).LocalConnection, data, inventoryString, appearanceString, clothigString, vars);
		}
	}

	private void RpcWriter___Observers_ReceivePlayerNameData_3895153758(NetworkConnection conn, string playerName, string id)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(playerName);
			((Writer)writer).WriteString(id);
			((NetworkBehaviour)this).SendObserversRpc(13u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceivePlayerNameData_3895153758(NetworkConnection conn, string playerName, string id)
	{
		PlayerName = playerName;
		PlayerCode = id;
		((Object)((Component)this).gameObject).name = SyncAccessor__003CPlayerName_003Ek__BackingField + " (" + id + ")";
		PoI.SetMainText(SyncAccessor__003CPlayerName_003Ek__BackingField);
		NameLabel.ShowText(SyncAccessor__003CPlayerName_003Ek__BackingField);
		Debug.Log((object)("Received player name data: " + playerName + " (" + id + ")"));
	}

	private void RpcReader___Observers_ReceivePlayerNameData_3895153758(PooledReader PooledReader0, Channel channel)
	{
		string playerName = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceivePlayerNameData_3895153758(null, playerName, id);
		}
	}

	private void RpcWriter___Target_ReceivePlayerNameData_3895153758(NetworkConnection conn, string playerName, string id)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(playerName);
			((Writer)writer).WriteString(id);
			((NetworkBehaviour)this).SendTargetRpc(14u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ReceivePlayerNameData_3895153758(PooledReader PooledReader0, Channel channel)
	{
		string playerName = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceivePlayerNameData_3895153758(((NetworkBehaviour)this).LocalConnection, playerName, id);
		}
	}

	private void RpcWriter___Server_SetFlashlightOn_Server_1140765316(bool on)
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
			((Writer)writer).WriteBoolean(on);
			((NetworkBehaviour)this).SendServerRpc(15u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetFlashlightOn_Server_1140765316(bool on)
	{
		SetFlashlightOn_Client(on);
	}

	private void RpcReader___Server_SetFlashlightOn_Server_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool flag = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetFlashlightOn_Server_1140765316(flag);
		}
	}

	private void RpcWriter___Observers_SetFlashlightOn_Client_1140765316(bool on)
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
			((Writer)writer).WriteBoolean(on);
			((NetworkBehaviour)this).SendObserversRpc(16u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetFlashlightOn_Client_1140765316(bool on)
	{
		((Component)ThirdPersonFlashlight).gameObject.SetActive(on && !((NetworkBehaviour)this).IsOwner);
	}

	private void RpcReader___Observers_SetFlashlightOn_Client_1140765316(PooledReader PooledReader0, Channel channel)
	{
		bool flag = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetFlashlightOn_Client_1140765316(flag);
		}
	}

	private void RpcWriter___Observers_PlayJumpAnimation_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(17u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___PlayJumpAnimation_2166136261()
	{
		Anim.Jump();
	}

	private void RpcReader___Observers_PlayJumpAnimation_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___PlayJumpAnimation_2166136261();
		}
	}

	private void RpcWriter___Server_SendCrouched_1140765316(bool crouched)
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
			((Writer)writer).WriteBoolean(crouched);
			((NetworkBehaviour)this).SendServerRpc(18u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendCrouched_1140765316(bool crouched)
	{
		ReceiveCrouched(null, crouched);
	}

	private void RpcReader___Server_SendCrouched_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool crouched = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendCrouched_1140765316(crouched);
		}
	}

	private void RpcWriter___Target_ReceiveCrouched_214505783(NetworkConnection conn, bool crouched)
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
			((Writer)writer).WriteBoolean(crouched);
			((NetworkBehaviour)this).SendTargetRpc(19u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveCrouched_214505783(NetworkConnection conn, bool crouched)
	{
		if (!((NetworkBehaviour)this).Owner.IsLocalClient)
		{
			Crouched = crouched;
		}
	}

	private void RpcReader___Target_ReceiveCrouched_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool crouched = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveCrouched_214505783(((NetworkBehaviour)this).LocalConnection, crouched);
		}
	}

	private void RpcWriter___Observers_ReceiveCrouched_214505783(NetworkConnection conn, bool crouched)
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
			((Writer)writer).WriteBoolean(crouched);
			((NetworkBehaviour)this).SendObserversRpc(20u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_ReceiveCrouched_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool crouched = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceiveCrouched_214505783(null, crouched);
		}
	}

	private void RpcWriter___Server_SendAvatarSettings_4281687581(AvatarSettings settings)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			GeneratedWriters___Internal.Write___ScheduleOne_002EAvatarFramework_002EAvatarSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, settings);
			((NetworkBehaviour)this).SendServerRpc(21u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendAvatarSettings_4281687581(AvatarSettings settings)
	{
		SetAvatarSettings(settings);
	}

	private void RpcReader___Server_SendAvatarSettings_4281687581(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		AvatarSettings settings = GeneratedReaders___Internal.Read___ScheduleOne_002EAvatarFramework_002EAvatarSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___SendAvatarSettings_4281687581(settings);
		}
	}

	private void RpcWriter___Observers_SetAvatarSettings_4281687581(AvatarSettings settings)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EAvatarFramework_002EAvatarSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, settings);
			((NetworkBehaviour)this).SendObserversRpc(22u, writer, val, (DataOrderType)0, true, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetAvatarSettings_4281687581(AvatarSettings settings)
	{
		Avatar.LoadAvatarSettings(settings);
		if (((NetworkBehaviour)this).IsOwner)
		{
			LayerUtility.SetLayerRecursively(((Component)Avatar).gameObject, LayerMask.NameToLayer("Invisible"));
		}
	}

	private void RpcReader___Observers_SetAvatarSettings_4281687581(PooledReader PooledReader0, Channel channel)
	{
		AvatarSettings settings = GeneratedReaders___Internal.Read___ScheduleOne_002EAvatarFramework_002EAvatarSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetAvatarSettings_4281687581(settings);
		}
	}

	private void RpcWriter___Observers_SetVisible_Networked_1140765316(bool vis)
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
			((Writer)writer).WriteBoolean(vis);
			((NetworkBehaviour)this).SendObserversRpc(23u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetVisible_Networked_1140765316(bool vis)
	{
		Avatar.SetVisible(vis);
	}

	private void RpcReader___Observers_SetVisible_Networked_1140765316(PooledReader PooledReader0, Channel channel)
	{
		bool vis = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetVisible_Networked_1140765316(vis);
		}
	}

	private void RpcWriter___Server_SetReadyToSleep_1140765316(bool ready)
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
			((Writer)writer).WriteBoolean(ready);
			((NetworkBehaviour)this).SendServerRpc(24u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetReadyToSleep_1140765316(bool ready)
	{
		IsReadyToSleep = ready;
	}

	private void RpcReader___Server_SetReadyToSleep_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool ready = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetReadyToSleep_1140765316(ready);
		}
	}

	private void RpcWriter___Server_SendPunch_2166136261()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendServerRpc(25u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendPunch_2166136261()
	{
		Punch();
	}

	private void RpcReader___Server_SendPunch_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn))
		{
			RpcLogic___SendPunch_2166136261();
		}
	}

	private void RpcWriter___Observers_Punch_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(26u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Punch_2166136261()
	{
		Avatar.Animation.SetTrigger("Punch");
		if (!((NetworkBehaviour)this).IsOwner)
		{
			PunchSound.Play();
		}
	}

	private void RpcReader___Observers_Punch_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Punch_2166136261();
		}
	}

	private void RpcWriter___Server_MarkIntroCompleted_3281254764(BasicAvatarSettings appearance)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			GeneratedWriters___Internal.Write___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((NetworkBehaviour)this).SendServerRpc(27u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___MarkIntroCompleted_3281254764(BasicAvatarSettings appearance)
	{
		HasCompletedIntro = true;
		Console.Log(SyncAccessor__003CPlayerName_003Ek__BackingField + " has completed intro");
		SetAppearance(appearance, refreshClothing: false);
	}

	private void RpcReader___Server_MarkIntroCompleted_3281254764(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		BasicAvatarSettings appearance = GeneratedReaders___Internal.Read___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___MarkIntroCompleted_3281254764(appearance);
		}
	}

	private void RpcWriter___Server_SendImpact_427288424(Impact impact)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ECombat_002EImpactFishNet_002ESerializing_002EGenerated((Writer)(object)writer, impact);
			((NetworkBehaviour)this).SendServerRpc(28u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public virtual void RpcLogic___SendImpact_427288424(Impact impact)
	{
		ReceiveImpact(impact);
	}

	private void RpcReader___Server_SendImpact_427288424(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Impact impact = GeneratedReaders___Internal.Read___ScheduleOne_002ECombat_002EImpactFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendImpact_427288424(impact);
		}
	}

	private void RpcWriter___Observers_ReceiveImpact_427288424(Impact impact)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ECombat_002EImpactFishNet_002ESerializing_002EGenerated((Writer)(object)writer, impact);
			((NetworkBehaviour)this).SendObserversRpc(29u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public virtual void RpcLogic___ReceiveImpact_427288424(Impact impact)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		if (impactHistory.Contains(impact.ImpactID))
		{
			return;
		}
		impactHistory.Add(impact.ImpactID);
		Debug.Log((object)$"{SyncAccessor__003CPlayerName_003Ek__BackingField} hit by impact! Type: {impact.ImpactType}, Damage: {impact.ImpactDamage}, Force: {impact.ImpactForce}");
		ProcessImpactForce(impact.HitPoint, impact.ImpactForceDirection, impact.ImpactForce);
		if (impact.ImpactType == EImpactType.Explosion && impact.ExplosionType == EExplosionType.Lightning)
		{
			HitByLightning();
			return;
		}
		Health.TakeDamage(impact.ImpactDamage, flinch: true, impact.ImpactDamage > 0f);
		if (Singleton<SFXManager>.InstanceExists)
		{
			if (impact.ImpactType == EImpactType.Punch)
			{
				Singleton<SFXManager>.Instance.PlayImpactSound((EImpactSound)8, impact.HitPoint, impact.ImpactForce);
			}
			else if (impact.ImpactType == EImpactType.BluntMetal)
			{
				Singleton<SFXManager>.Instance.PlayImpactSound((EImpactSound)9, impact.HitPoint, impact.ImpactForce);
			}
		}
	}

	private void RpcReader___Observers_ReceiveImpact_427288424(PooledReader PooledReader0, Channel channel)
	{
		Impact impact = GeneratedReaders___Internal.Read___ScheduleOne_002ECombat_002EImpactFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceiveImpact_427288424(impact);
		}
	}

	private void RpcWriter___Server_Arrest_Server_2166136261()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendServerRpc(30u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___Arrest_Server_2166136261()
	{
		Arrest_Client();
	}

	private void RpcReader___Server_Arrest_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___Arrest_Server_2166136261();
		}
	}

	private void RpcWriter___Observers_Arrest_Client_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(31u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Arrest_Client_2166136261()
	{
		if (!IsArrested)
		{
			if (onArrested != null)
			{
				onArrested.Invoke();
			}
			IsArrested = true;
			CrimeData.MinsSinceLastArrested = 0;
			if (Health.IsAlive && ((NetworkBehaviour)this).IsOwner)
			{
				ExitAll();
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement("Arrested");
				AchievementManager.UnlockAchievement(AchievementManager.EAchievement.LONG_ARM_OF_THE_LAW);
				Singleton<ArrestScreen>.Instance.Open();
			}
		}
	}

	private void RpcReader___Observers_Arrest_Client_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Arrest_Client_2166136261();
		}
	}

	private void RpcWriter___Server_Free_Server_2166136261()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendServerRpc(32u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___Free_Server_2166136261()
	{
		Free_Client();
	}

	private void RpcReader___Server_Free_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___Free_Server_2166136261();
		}
	}

	private void RpcWriter___Observers_Free_Client_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(33u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Free_Client_2166136261()
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (!IsArrested)
		{
			return;
		}
		if (((NetworkBehaviour)this).IsOwner)
		{
			Transform val = Singleton<ScheduleOne.Map.Map>.Instance.PoliceStation.SpawnPoint;
			if (NetworkSingleton<CurfewManager>.Instance.IsCurrentlyActive)
			{
				val = (((Object)(object)Local.LastVisitedProperty != (Object)null) ? Local.LastVisitedProperty.InteriorSpawnPoint : ((ScheduleOne.Property.Property.OwnedProperties.Count((ScheduleOne.Property.Property x) => x.CanRespawnInsideProperty()) <= 0) ? NetworkSingleton<GameManager>.Instance.NoHomeRespawnPoint : ScheduleOne.Property.Property.OwnedProperties.First((ScheduleOne.Property.Property x) => x.CanRespawnInsideProperty()).InteriorSpawnPoint));
			}
			PlayerSingleton<PlayerMovement>.Instance.Teleport(val.position + Vector3.up * 1f);
			((Component)this).transform.forward = val.forward;
			((Behaviour)Singleton<HUD>.Instance.canvas).enabled = true;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement("Arrested");
		}
		IsArrested = false;
		if (onFreed != null)
		{
			onFreed.Invoke();
		}
	}

	private void RpcReader___Observers_Free_Client_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Free_Client_2166136261();
		}
	}

	private void RpcWriter___Server_SendPassOut_2166136261()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendServerRpc(34u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendPassOut_2166136261()
	{
		PassOut();
	}

	private void RpcReader___Server_SendPassOut_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___SendPassOut_2166136261();
		}
	}

	private void RpcWriter___Observers_PassOut_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(35u, writer, val, (DataOrderType)0, false, false, true);
			writer.Store();
		}
	}

	public void RpcLogic___PassOut_2166136261()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		IsUnconscious = true;
		if (onPassedOut != null)
		{
			onPassedOut.Invoke();
		}
		((Collider)CapCol).enabled = false;
		SetRagdolled(ragdolled: true);
		Avatar.MiddleSpineRB.AddForce(((Component)this).transform.forward * 30f, (ForceMode)2);
		Avatar.MiddleSpineRB.AddRelativeTorque(new Vector3(0f, Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 10f, (ForceMode)2);
		if (Health.IsAlive)
		{
			if (CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None)
			{
				IsArrested = true;
			}
			if (((NetworkBehaviour)this).IsOwner)
			{
				ExitAll();
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement("Passed out");
				Singleton<PassOutScreen>.Instance.Open();
			}
		}
	}

	private void RpcReader___Observers_PassOut_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___PassOut_2166136261();
		}
	}

	private void RpcWriter___Server_SendPassOutRecovery_2166136261()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendServerRpc(36u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendPassOutRecovery_2166136261()
	{
		PassOutRecovery();
	}

	private void RpcReader___Server_SendPassOutRecovery_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___SendPassOutRecovery_2166136261();
		}
	}

	private void RpcWriter___Observers_PassOutRecovery_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(37u, writer, val, (DataOrderType)0, false, false, true);
			writer.Store();
		}
	}

	public void RpcLogic___PassOutRecovery_2166136261()
	{
		Debug.Log((object)"Player recovered from pass out");
		IsUnconscious = false;
		SetRagdolled(ragdolled: false);
		((Collider)CapCol).enabled = true;
		if (((NetworkBehaviour)this).IsOwner)
		{
			((Behaviour)Singleton<HUD>.Instance.canvas).enabled = true;
			Energy.RestoreEnergy();
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement("Passed out");
		}
		if (onPassOutRecovery != null)
		{
			onPassOutRecovery.Invoke();
		}
	}

	private void RpcReader___Observers_PassOutRecovery_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___PassOutRecovery_2166136261();
		}
	}

	private void RpcWriter___Server_SendEquippable_Networked_3615296227(string assetPath)
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
			((Writer)writer).WriteString(assetPath);
			((NetworkBehaviour)this).SendServerRpc(38u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendEquippable_Networked_3615296227(string assetPath)
	{
		SetEquippable_Networked(assetPath);
	}

	private void RpcReader___Server_SendEquippable_Networked_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string assetPath = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendEquippable_Networked_3615296227(assetPath);
		}
	}

	private void RpcWriter___Observers_SetEquippable_Networked_3615296227(string assetPath)
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
			((Writer)writer).WriteString(assetPath);
			((NetworkBehaviour)this).SendObserversRpc(39u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetEquippable_Networked_3615296227(string assetPath)
	{
		Avatar.SetEquippable(assetPath);
	}

	private void RpcReader___Observers_SetEquippable_Networked_3615296227(PooledReader PooledReader0, Channel channel)
	{
		string assetPath = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetEquippable_Networked_3615296227(assetPath);
		}
	}

	private void RpcWriter___Server_SendEquippableMessage_Networked_3643459082(string message, int receipt)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteString(message);
			((Writer)writer).WriteInt32(receipt, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(40u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendEquippableMessage_Networked_3643459082(string message, int receipt)
	{
		ReceiveEquippableMessage_Networked(message, receipt);
	}

	private void RpcReader___Server_SendEquippableMessage_Networked_3643459082(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string message = ((Reader)PooledReader0).ReadString();
		int receipt = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___SendEquippableMessage_Networked_3643459082(message, receipt);
		}
	}

	private void RpcWriter___Observers_ReceiveEquippableMessage_Networked_3643459082(string message, int receipt)
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
			((Writer)writer).WriteString(message);
			((Writer)writer).WriteInt32(receipt, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(41u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveEquippableMessage_Networked_3643459082(string message, int receipt)
	{
		if (!equippableMessageIDHistory.Contains(receipt))
		{
			equippableMessageIDHistory.Add(receipt);
			Avatar.ReceiveEquippableMessage(message, null);
		}
	}

	private void RpcReader___Observers_ReceiveEquippableMessage_Networked_3643459082(PooledReader PooledReader0, Channel channel)
	{
		string message = ((Reader)PooledReader0).ReadString();
		int receipt = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceiveEquippableMessage_Networked_3643459082(message, receipt);
		}
	}

	private void RpcWriter___Server_SendEquippableMessage_Networked_Vector_3190074053(string message, int receipt, Vector3 data)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteString(message);
			((Writer)writer).WriteInt32(receipt, (AutoPackType)1);
			((Writer)writer).WriteVector3(data);
			((NetworkBehaviour)this).SendServerRpc(42u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendEquippableMessage_Networked_Vector_3190074053(string message, int receipt, Vector3 data)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		ReceiveEquippableMessage_Networked_Vector(message, receipt, data);
	}

	private void RpcReader___Server_SendEquippableMessage_Networked_Vector_3190074053(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		string message = ((Reader)PooledReader0).ReadString();
		int receipt = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		Vector3 data = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___SendEquippableMessage_Networked_Vector_3190074053(message, receipt, data);
		}
	}

	private void RpcWriter___Observers_ReceiveEquippableMessage_Networked_Vector_3190074053(string message, int receipt, Vector3 data)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(message);
			((Writer)writer).WriteInt32(receipt, (AutoPackType)1);
			((Writer)writer).WriteVector3(data);
			((NetworkBehaviour)this).SendObserversRpc(43u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveEquippableMessage_Networked_Vector_3190074053(string message, int receipt, Vector3 data)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!equippableMessageIDHistory.Contains(receipt))
		{
			equippableMessageIDHistory.Add(receipt);
			Avatar.ReceiveEquippableMessage(message, data);
		}
	}

	private void RpcReader___Observers_ReceiveEquippableMessage_Networked_Vector_3190074053(PooledReader PooledReader0, Channel channel)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		string message = ((Reader)PooledReader0).ReadString();
		int receipt = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		Vector3 data = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceiveEquippableMessage_Networked_Vector_3190074053(message, receipt, data);
		}
	}

	private void RpcWriter___Server_SendAnimationTrigger_3615296227(string trigger)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteString(trigger);
			((NetworkBehaviour)this).SendServerRpc(44u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendAnimationTrigger_3615296227(string trigger)
	{
		SetAnimationTrigger_Networked(null, trigger);
	}

	private void RpcReader___Server_SendAnimationTrigger_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string trigger = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___SendAnimationTrigger_3615296227(trigger);
		}
	}

	private void RpcWriter___Observers_SetAnimationTrigger_Networked_2971853958(NetworkConnection conn, string trigger)
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
			((Writer)writer).WriteString(trigger);
			((NetworkBehaviour)this).SendObserversRpc(45u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetAnimationTrigger_Networked_2971853958(NetworkConnection conn, string trigger)
	{
		SetAnimationTrigger(trigger);
	}

	private void RpcReader___Observers_SetAnimationTrigger_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string trigger = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetAnimationTrigger_Networked_2971853958(null, trigger);
		}
	}

	private void RpcWriter___Target_SetAnimationTrigger_Networked_2971853958(NetworkConnection conn, string trigger)
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
			((Writer)writer).WriteString(trigger);
			((NetworkBehaviour)this).SendTargetRpc(46u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetAnimationTrigger_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string trigger = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetAnimationTrigger_Networked_2971853958(((NetworkBehaviour)this).LocalConnection, trigger);
		}
	}

	private void RpcWriter___Observers_ResetAnimationTrigger_Networked_2971853958(NetworkConnection conn, string trigger)
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
			((Writer)writer).WriteString(trigger);
			((NetworkBehaviour)this).SendObserversRpc(47u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___ResetAnimationTrigger_Networked_2971853958(NetworkConnection conn, string trigger)
	{
		ResetAnimationTrigger(trigger);
	}

	private void RpcReader___Observers_ResetAnimationTrigger_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string trigger = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ResetAnimationTrigger_Networked_2971853958(null, trigger);
		}
	}

	private void RpcWriter___Target_ResetAnimationTrigger_Networked_2971853958(NetworkConnection conn, string trigger)
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
			((Writer)writer).WriteString(trigger);
			((NetworkBehaviour)this).SendTargetRpc(48u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ResetAnimationTrigger_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string trigger = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ResetAnimationTrigger_Networked_2971853958(((NetworkBehaviour)this).LocalConnection, trigger);
		}
	}

	private void RpcWriter___Server_SendAnimationBool_310431262(string name, bool val)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val2 = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteBoolean(val);
			((NetworkBehaviour)this).SendServerRpc(49u, writer, val2, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendAnimationBool_310431262(string name, bool val)
	{
		SetAnimationBool(name, val);
	}

	private void RpcReader___Server_SendAnimationBool_310431262(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string name = ((Reader)PooledReader0).ReadString();
		bool val = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___SendAnimationBool_310431262(name, val);
		}
	}

	private void RpcWriter___Observers_SetAnimationBool_310431262(string name, bool val)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			Channel val2 = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteBoolean(val);
			((NetworkBehaviour)this).SendObserversRpc(50u, writer, val2, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetAnimationBool_310431262(string name, bool val)
	{
		Avatar.Animation.SetBool(name, val);
	}

	private void RpcReader___Observers_SetAnimationBool_310431262(PooledReader PooledReader0, Channel channel)
	{
		string name = ((Reader)PooledReader0).ReadString();
		bool val = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetAnimationBool_310431262(name, val);
		}
	}

	private void RpcWriter___Observers_Taze_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(51u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___Taze_2166136261()
	{
		IsTased = true;
		if (onTased != null)
		{
			onTased.Invoke();
		}
		if (taseCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(taseCoroutine);
		}
		Health.TakeDamage(1f, flinch: true, playBloodMist: false);
		taseCoroutine = ((MonoBehaviour)this).StartCoroutine(Tase());
		IEnumerator Tase()
		{
			Avatar.Effects.SetZapped(zapped: true);
			yield return (object)new WaitForSeconds(2f);
			Avatar.Effects.SetZapped(zapped: false);
			IsTased = false;
			if (onTasedEnd != null)
			{
				onTasedEnd.Invoke();
			}
		}
	}

	private void RpcReader___Observers_Taze_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Taze_2166136261();
		}
	}

	private void RpcWriter___Server_SetInventoryItem_2317364410(int index, ItemInstance item)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteInt32(index, (AutoPackType)1);
			((Writer)(object)writer).WriteItemInstance(item);
			((NetworkBehaviour)this).SendServerRpc(52u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetInventoryItem_2317364410(int index, ItemInstance item)
	{
		Inventory[index].SetStoredItem(item);
	}

	private void RpcReader___Server_SetInventoryItem_2317364410(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int index = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		ItemInstance item = ((Reader)(object)PooledReader0).ReadItemInstance();
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___SetInventoryItem_2317364410(index, item);
		}
	}

	private void RpcWriter___Server_SetEquippedSlotIndex_3316948804(int index)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteInt32(index, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(53u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetEquippedSlotIndex_3316948804(int index)
	{
		EquippedItemSlotIndex = index;
	}

	private void RpcReader___Server_SetEquippedSlotIndex_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int index = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___SetEquippedSlotIndex_3316948804(index);
		}
	}

	private void RpcWriter___Observers_RemoveEquippedItemFromInventory_3643459082(string id, int amount)
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
			((Writer)writer).WriteString(id);
			((Writer)writer).WriteInt32(amount, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(54u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___RemoveEquippedItemFromInventory_3643459082(string id, int amount)
	{
		if (IsLocalPlayer && PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped && ((BaseItemInstance)PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance).ID == id)
		{
			PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ChangeQuantity(-amount);
		}
	}

	private void RpcReader___Observers_RemoveEquippedItemFromInventory_3643459082(PooledReader PooledReader0, Channel channel)
	{
		string id = ((Reader)PooledReader0).ReadString();
		int amount = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___RemoveEquippedItemFromInventory_3643459082(id, amount);
		}
	}

	private void RpcWriter___Server_SendAppearance_3281254764(BasicAvatarSettings settings)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			GeneratedWriters___Internal.Write___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, settings);
			((NetworkBehaviour)this).SendServerRpc(55u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendAppearance_3281254764(BasicAvatarSettings settings)
	{
		SetAppearance(settings, refreshClothing: true);
	}

	private void RpcReader___Server_SendAppearance_3281254764(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		BasicAvatarSettings settings = GeneratedReaders___Internal.Read___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___SendAppearance_3281254764(settings);
		}
	}

	private void RpcWriter___Observers_SetAppearance_2139595489(BasicAvatarSettings settings, bool refreshClothing)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, settings);
			((Writer)writer).WriteBoolean(refreshClothing);
			((NetworkBehaviour)this).SendObserversRpc(56u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetAppearance_2139595489(BasicAvatarSettings settings, bool refreshClothing)
	{
		CurrentAvatarSettings = settings;
		AvatarSettings avatarSettings = CurrentAvatarSettings.GetAvatarSettings();
		Avatar.LoadAvatarSettings(avatarSettings);
		if (refreshClothing)
		{
			Clothing.RefreshAppearance();
		}
		SetVisibleToLocalPlayer(!((NetworkBehaviour)this).IsOwner);
	}

	private void RpcReader___Observers_SetAppearance_2139595489(PooledReader PooledReader0, Channel channel)
	{
		BasicAvatarSettings settings = GeneratedReaders___Internal.Read___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool refreshClothing = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetAppearance_2139595489(settings, refreshClothing);
		}
	}

	private void RpcWriter___Server_SendMountedSkateboard_3323014238(NetworkObject skateboardObj)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteNetworkObject(skateboardObj);
			((NetworkBehaviour)this).SendServerRpc(57u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendMountedSkateboard_3323014238(NetworkObject skateboardObj)
	{
		SetMountedSkateboard(skateboardObj);
	}

	private void RpcReader___Server_SendMountedSkateboard_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject skateboardObj = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___SendMountedSkateboard_3323014238(skateboardObj);
		}
	}

	private void RpcWriter___Observers_SetMountedSkateboard_3323014238(NetworkObject skateboardObj)
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
			((Writer)writer).WriteNetworkObject(skateboardObj);
			((NetworkBehaviour)this).SendObserversRpc(58u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetMountedSkateboard_3323014238(NetworkObject skateboardObj)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)skateboardObj != (Object)null)
		{
			if (!((Object)(object)ActiveSkateboard != (Object)null))
			{
				Skateboard component = ((Component)skateboardObj).GetComponent<Skateboard>();
				RpcWriter___Server_set_IsSkating_1140765316(value: true);
				ActiveSkateboard = component;
				((Component)this).transform.SetParent(component.PlayerContainer);
				((Component)this).transform.localPosition = Vector3.zero;
				((Component)this).transform.localRotation = Quaternion.identity;
				if (onSkateboardMounted != null)
				{
					onSkateboardMounted(component);
				}
			}
		}
		else if (!((Object)(object)ActiveSkateboard == (Object)null))
		{
			RpcWriter___Server_set_IsSkating_1140765316(value: false);
			ActiveSkateboard = null;
			((Component)this).transform.SetParent((Transform)null);
			((Component)this).transform.rotation = Quaternion.LookRotation(((Component)this).transform.forward, Vector3.up);
			((Component)this).transform.eulerAngles = new Vector3(0f, ((Component)this).transform.eulerAngles.y, 0f);
			if (onSkateboardDismounted != null)
			{
				onSkateboardDismounted();
			}
		}
	}

	private void RpcReader___Observers_SetMountedSkateboard_3323014238(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject skateboardObj = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetMountedSkateboard_3323014238(skateboardObj);
		}
	}

	private void RpcWriter___Server_SendConsumeProduct_2622925554(ProductItemInstance product)
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
			((Writer)(object)writer).WriteProductItemInstance(product);
			((NetworkBehaviour)this).SendServerRpc(59u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendConsumeProduct_2622925554(ProductItemInstance product)
	{
		ReceiveConsumeProduct(product);
	}

	private void RpcReader___Server_SendConsumeProduct_2622925554(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		ProductItemInstance product = ((Reader)(object)PooledReader0).ReadProductItemInstance();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendConsumeProduct_2622925554(product);
		}
	}

	private void RpcWriter___Observers_ReceiveConsumeProduct_2622925554(ProductItemInstance product)
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
			((Writer)(object)writer).WriteProductItemInstance(product);
			((NetworkBehaviour)this).SendObserversRpc(60u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveConsumeProduct_2622925554(ProductItemInstance product)
	{
		if (!((NetworkBehaviour)this).IsOwner)
		{
			ConsumeProductInternal(product);
		}
	}

	private void RpcReader___Observers_ReceiveConsumeProduct_2622925554(PooledReader PooledReader0, Channel channel)
	{
		ProductItemInstance product = ((Reader)(object)PooledReader0).ReadProductItemInstance();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveConsumeProduct_2622925554(product);
		}
	}

	private void RpcWriter___Server_SendValue_3589193952(string variableName, string value, bool sendToOwner)
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
			((Writer)writer).WriteString(variableName);
			((Writer)writer).WriteString(value);
			((Writer)writer).WriteBoolean(sendToOwner);
			((NetworkBehaviour)this).SendServerRpc(61u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendValue_3589193952(string variableName, string value, bool sendToOwner)
	{
		if (sendToOwner || !((NetworkBehaviour)this).IsOwner)
		{
			ReceiveValue(variableName, value);
		}
		if (sendToOwner)
		{
			ReceiveValue(((NetworkBehaviour)this).Owner, variableName, value);
		}
	}

	private void RpcReader___Server_SendValue_3589193952(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string variableName = ((Reader)PooledReader0).ReadString();
		string value = ((Reader)PooledReader0).ReadString();
		bool sendToOwner = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendValue_3589193952(variableName, value, sendToOwner);
		}
	}

	private void RpcWriter___Target_ReceiveValue_3895153758(NetworkConnection conn, string variableName, string value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(variableName);
			((Writer)writer).WriteString(value);
			((NetworkBehaviour)this).SendTargetRpc(62u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveValue_3895153758(NetworkConnection conn, string variableName, string value)
	{
		ReceiveValue(variableName, value);
	}

	private void RpcReader___Target_ReceiveValue_3895153758(PooledReader PooledReader0, Channel channel)
	{
		string variableName = ((Reader)PooledReader0).ReadString();
		string value = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveValue_3895153758(((NetworkBehaviour)this).LocalConnection, variableName, value);
		}
	}

	private void RpcWriter___Server_SendWorldSpaceDialogue_606697822(string text, float duration)
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
			((Writer)writer).WriteString(text);
			((Writer)writer).WriteSingle(duration, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(63u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendWorldSpaceDialogue_606697822(string text, float duration)
	{
		ShowWorldSpaceDialogue(text, duration);
	}

	private void RpcReader___Server_SendWorldSpaceDialogue_606697822(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string text = ((Reader)PooledReader0).ReadString();
		float duration = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendWorldSpaceDialogue_606697822(text, duration);
		}
	}

	private void RpcWriter___Observers_ShowWorldSpaceDialogue_606697822(string text, float duration)
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
			((Writer)writer).WriteString(text);
			((Writer)writer).WriteSingle(duration, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(64u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ShowWorldSpaceDialogue_606697822(string text, float duration)
	{
		WorldspaceDialogue.ShowText(text, duration);
	}

	private void RpcReader___Observers_ShowWorldSpaceDialogue_606697822(PooledReader PooledReader0, Channel channel)
	{
		string text = ((Reader)PooledReader0).ReadString();
		float duration = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ShowWorldSpaceDialogue_606697822(text, duration);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EPlayerScripts_002EPlayer(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		switch (UInt321)
		{
		case 6u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CCameraRotation_003Ek__BackingField(syncVar____003CCameraRotation_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			Quaternion value2 = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
			this.sync___set_value__003CCameraRotation_003Ek__BackingField(value2, Boolean2);
			return true;
		}
		case 5u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CCameraPosition_003Ek__BackingField(syncVar____003CCameraPosition_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			Vector3 value6 = ((Reader)PooledReader0).ReadVector3();
			this.sync___set_value__003CCameraPosition_003Ek__BackingField(value6, Boolean2);
			return true;
		}
		case 4u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CIsReadyToSleep_003Ek__BackingField(syncVar____003CIsReadyToSleep_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			bool value3 = ((Reader)PooledReader0).ReadBoolean();
			this.sync___set_value__003CIsReadyToSleep_003Ek__BackingField(value3, Boolean2);
			return true;
		}
		case 3u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CCurrentBed_003Ek__BackingField(syncVar____003CCurrentBed_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			NetworkObject value5 = ((Reader)PooledReader0).ReadNetworkObject();
			this.sync___set_value__003CCurrentBed_003Ek__BackingField(value5, Boolean2);
			return true;
		}
		case 2u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CCurrentVehicle_003Ek__BackingField(syncVar____003CCurrentVehicle_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			NetworkObject value7 = ((Reader)PooledReader0).ReadNetworkObject();
			this.sync___set_value__003CCurrentVehicle_003Ek__BackingField(value7, Boolean2);
			return true;
		}
		case 1u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CPlayerCode_003Ek__BackingField(syncVar____003CPlayerCode_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			string value4 = ((Reader)PooledReader0).ReadString();
			this.sync___set_value__003CPlayerCode_003Ek__BackingField(value4, Boolean2);
			return true;
		}
		case 0u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CPlayerName_003Ek__BackingField(syncVar____003CPlayerName_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			string value = ((Reader)PooledReader0).ReadString();
			this.sync___set_value__003CPlayerName_003Ek__BackingField(value, Boolean2);
			return true;
		}
		default:
			return false;
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EPlayerScripts_002EPlayer_Assembly_002DCSharp_002Edll()
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		_networkedEquipper = ((Component)this).GetComponent<NetworkedEquipper>();
		if ((Object)(object)InstanceFinder.NetworkManager == (Object)null)
		{
			Local = this;
		}
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepStart = (Action)Delegate.Combine(instance.onSleepStart, new Action(SleepStart));
		TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
		instance2.onSleepEnd = (Action)Delegate.Combine(instance2.onSleepEnd, new Action(SleepEnd));
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnUncappedMinutePass);
		MoneyManager instance3 = NetworkSingleton<MoneyManager>.Instance;
		instance3.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Combine(instance3.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
		Health.onDie.AddListener(new UnityAction(OnDied));
		Health.onRevive.AddListener(new UnityAction(OnRevived));
		Health.onRevive.AddListener(new UnityAction(ResetHitByLightning));
		Energy.onEnergyDepleted.AddListener(new UnityAction(SendPassOut));
		((MonoBehaviour)this).InvokeRepeating("RecalculateCurrentProperty", 0f, 0.5f);
		((MonoBehaviour)this).InvokeRepeating("RecalculateCurrentRegion", 0f, 0.5f);
		InitializeSaveable();
		Inventory = new ItemSlot[9];
		for (int i = 0; i < Inventory.Length; i++)
		{
			Inventory[i] = new ItemSlot();
		}
		Rigidbody[] ragdollRBs = Avatar.RagdollRBs;
		foreach (Rigidbody val in ragdollRBs)
		{
			Physics.IgnoreCollision(((Component)val).GetComponent<Collider>(), (Collider)(object)CapCol, true);
			ragdollForceComponents.Add(((Component)val).gameObject.AddComponent<ConstantForce>());
		}
		EyePosition = ((Component)Avatar.Eyes).transform.position;
		SetGravityMultiplier(1f);
	}
}
