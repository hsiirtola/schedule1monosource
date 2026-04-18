using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EPOOutline;
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
using FluffyUnderware.DevTools.Extensions;
using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Animation;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.Combat;
using ScheduleOne.Core.Equipping.Framework;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Doors;
using ScheduleOne.Economy;
using ScheduleOne.Effects;
using ScheduleOne.Equipping.Framework;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using ScheduleOne.Messaging;
using ScheduleOne.NPCs.Actions;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.NPCs.Responses;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Variables;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.AI;
using ScheduleOne.Vision;
using ScheduleOne.VoiceOver;
using ScheduleOne.Weather;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ScheduleOne.NPCs;

[RequireComponent(typeof(NPCHealth))]
public class NPC : NetworkBehaviour, IGUIDRegisterable, ISaveable, ICombatTargetable, IDamageable, ISightable, INetworkedEquippableUser, IEquippableUser, IWeatherEntity
{
	private const int PanicDuration = 15;

	public const bool RequiresRegionUnlocked = true;

	[Header("Info Settings")]
	public string FirstName = string.Empty;

	public bool hasLastName = true;

	public string LastName = string.Empty;

	public string ID = string.Empty;

	public Sprite MugshotSprite;

	public EMapRegion Region = EMapRegion.Downtown;

	[Header("If true, NPC will respawn next day instead of waiting 3 days.")]
	public bool IsImportant;

	[Header("Personality")]
	[Range(0f, 1f)]
	public float Aggression;

	[Header("References")]
	[SerializeField]
	protected Transform modelContainer;

	[SerializeField]
	protected InteractableObject intObj;

	public NPCMovement Movement;

	public DialogueHandler DialogueHandler;

	public Avatar Avatar;

	public NPCAwareness Awareness;

	public NPCResponses Responses;

	public NPCActions Actions;

	public NPCBehaviour Behaviour;

	public NPCInventory Inventory;

	public VOEmitter VoiceOverEmitter;

	public NPCHealth Health;

	public EntityVisibility Visibility;

	public Action<LandVehicle> onEnterVehicle;

	public Action<LandVehicle> onExitVehicle;

	[Header("Summoning")]
	public bool CanBeSummoned = true;

	[Header("Relationship")]
	public NPCRelationData RelationData;

	public string NPCUnlockedVariable = string.Empty;

	public bool ShowRelationshipInfo = true;

	[Header("Messaging")]
	public List<EConversationCategory> ConversationCategories;

	public bool MessagingKnownByDefault = true;

	public bool ConversationCanBeHidden = true;

	public Action onConversationCreated;

	[Header("Other Settings")]
	public bool CanOpenDoors = true;

	public bool OverrideParent;

	public Transform OverriddenParent;

	public bool IgnoreImpacts;

	[Range(0f, 1f)]
	[SerializeField]
	private float _useUmbrellaChance = 0.75f;

	[Range(0f, 1f)]
	[SerializeField]
	private float _rainTolerance = 0.2f;

	[Range(1f, 10f)]
	[SerializeField]
	private float _walkInRainMaxSpeedMultiplier = 5f;

	[SerializeField]
	protected List<GameObject> OutlineRenderers = new List<GameObject>();

	protected Outlinable OutlineEffect;

	[Header("GUID")]
	public string BakedGUID = string.Empty;

	public Action<bool> onVisibilityChanged;

	private Coroutine resetUnsettledCoroutine;

	[CompilerGenerated]
	[SyncVar]
	[HideInInspector]
	public bool _003CHasUmbrella_003Ek__BackingField;

	private List<int> impactHistory = new List<int>();

	private int headlightStartTime = 1700;

	private int heaedLightsEndTime = 600;

	protected float defaultAggression;

	private WeatherConditions _weatherTolerence;

	protected WeatherConditions _currentWeatherConditionsForEntity;

	private float _wetness;

	private const float NPC_WET_RATE = 0.1f;

	private const float NPC_DRY_RATE = 0.05f;

	protected NetworkedEquipper _networkedEquipper;

	private Coroutine lerpScaleRoutine;

	public SyncVar<bool> syncVar____003CHasUmbrella_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCAssembly_002DCSharp_002Edll_Excuted;

	public bool IsLocalPlayer => false;

	public NetworkBehaviour NetworkBehaviour => (NetworkBehaviour)(object)this;

	public IThirdPersonReferencesProvider ThirdPersonReferences => (IThirdPersonReferencesProvider)(object)Avatar;

	public string fullName
	{
		get
		{
			if (hasLastName)
			{
				return FirstName + " " + LastName;
			}
			return FirstName;
		}
	}

	public float Scale { get; private set; } = 1f;

	public bool IsConscious
	{
		get
		{
			if (Health.Health > 0f)
			{
				if (!Behaviour.UnconsciousBehaviour.Active)
				{
					return !Behaviour.DeadBehaviour.Active;
				}
				return false;
			}
			return false;
		}
	}

	public LandVehicle CurrentVehicle { get; protected set; }

	public bool IsInVehicle => (Object)(object)CurrentVehicle != (Object)null;

	public bool isInBuilding => (Object)(object)CurrentBuilding != (Object)null;

	public NPCEnterableBuilding CurrentBuilding { get; protected set; }

	public StaticDoor LastEnteredDoor { get; set; }

	public MSGConversation MSGConversation { get; protected set; }

	public float WalkInRainMaxSpeedMultiplier => _walkInRainMaxSpeedMultiplier;

	public string SaveFolderName => fullName;

	public string SaveFileName => "NPC";

	public Loader Loader => null;

	public bool ShouldSaveUnderFolder => true;

	public List<string> LocalExtraFiles { get; set; } = new List<string> { "Relationship", "MessageConversation" };

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public Vector3 CenterPoint => CenterPointTransform.position;

	public Transform CenterPointTransform => Avatar.CenterPointTransform;

	public Vector3 LookAtPoint => ((Component)Avatar.Eyes).transform.position;

	public bool IsCurrentlyTargetable
	{
		get
		{
			if (!Health.IsDead && !Health.IsKnockedOut)
			{
				return isVisible;
			}
			return false;
		}
	}

	public float RangedHitChanceMultiplier => 1f;

	public Vector3 Velocity => Movement.VelocityCalculator.Velocity;

	public VisionEvent HighestProgressionEvent { get; set; }

	public EntityVisibility VisibilityComponent => Visibility;

	public Guid GUID { get; protected set; }

	public bool isVisible { get; protected set; } = true;

	public bool isUnsettled { get; protected set; }

	public bool IsPanicked { get; private set; }

	public float TimeSincePanicked { get; protected set; } = 1000f;

	public bool HasUmbrella
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CHasUmbrella_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			this.sync___set_value__003CHasUmbrella_003Ek__BackingField(value, true);
		}
	}

	Transform IWeatherEntity.Transform => CenterPointTransform;

	string IWeatherEntity.WeatherVolume { get; set; }

	public bool IsUnderCover { get; set; }

	public bool SyncAccessor__003CHasUmbrella_003Ek__BackingField
	{
		get
		{
			return HasUmbrella;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				HasUmbrella = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CHasUmbrella_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public void RecordLastKnownPosition(bool resetTimeSinceLastSeen)
	{
	}

	public float GetSearchTime()
	{
		return 30f;
	}

	public bool IsCurrentlySightable()
	{
		if (Health.IsDead)
		{
			return false;
		}
		if (Health.IsKnockedOut)
		{
			return false;
		}
		if (!isVisible)
		{
			return false;
		}
		return true;
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002ENPC_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected virtual void CheckAndGetReferences()
	{
		if ((Object)(object)Movement == (Object)null)
		{
			Movement = ((Component)this).GetComponentInChildren<NPCMovement>();
		}
		if ((Object)(object)DialogueHandler == (Object)null)
		{
			DialogueHandler = ((Component)this).GetComponentInChildren<DialogueHandler>();
		}
		if ((Object)(object)Avatar == (Object)null)
		{
			Avatar = ((Component)this).GetComponentInChildren<Avatar>();
		}
		if ((Object)(object)Awareness == (Object)null)
		{
			Awareness = ((Component)this).GetComponentInChildren<NPCAwareness>();
		}
		if ((Object)(object)Responses == (Object)null)
		{
			Responses = ((Component)this).GetComponentInChildren<NPCResponses>();
		}
		if ((Object)(object)Actions == (Object)null)
		{
			Actions = ((Component)this).GetComponentInChildren<NPCActions>();
		}
		if ((Object)(object)Behaviour == (Object)null)
		{
			Behaviour = ((Component)this).GetComponentInChildren<NPCBehaviour>();
		}
		if ((Object)(object)Inventory == (Object)null)
		{
			Inventory = ((Component)this).GetComponentInChildren<NPCInventory>();
		}
		if ((Object)(object)VoiceOverEmitter == (Object)null)
		{
			VoiceOverEmitter = ((Component)Avatar.HeadBone).GetComponentInChildren<VOEmitter>();
		}
		if ((Object)(object)Health == (Object)null)
		{
			Health = ((Component)this).GetComponentInChildren<NPCHealth>();
		}
		if ((Object)(object)Visibility == (Object)null)
		{
			Visibility = ((Component)this).GetComponentInChildren<EntityVisibility>();
		}
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	private void PlayerSpawned()
	{
		CreateMessageConversation();
	}

	protected virtual void CreateMessageConversation()
	{
		if (MSGConversation != null)
		{
			Console.LogWarning("Message conversation already exists for " + fullName);
			return;
		}
		MSGConversation = new MSGConversation(this, GetMessagingName());
		MSGConversation.SetCategories(ConversationCategories);
		MSGConversation.SetIsKnown(MessagingKnownByDefault);
		if (onConversationCreated != null)
		{
			onConversationCreated();
		}
	}

	protected virtual string GetMessagingName()
	{
		return fullName;
	}

	public virtual Sprite GetMessagingIcon()
	{
		return MugshotSprite;
	}

	public void SendTextMessage(string message)
	{
		MSGConversation.SendMessage(new Message(message, Message.ESenderType.Other, _endOfGroup: true, Random.Range(int.MinValue, int.MaxValue)));
	}

	protected virtual void Start()
	{
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(OnUncappedMinPass);
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnUncappedMinPass);
		NetworkSingleton<TimeManager>.Instance.onTick -= new Action(OnTick);
		NetworkSingleton<TimeManager>.Instance.onTick += new Action(OnTick);
		if (GUID == Guid.Empty)
		{
			if (!GUIDManager.IsGUIDValid(BakedGUID))
			{
				Console.LogWarning(((Object)((Component)this).gameObject).name + "'s baked GUID is not valid! Choosing random GUID");
				BakedGUID = GUIDManager.GenerateUniqueGUID().ToString();
			}
			GUID = new Guid(BakedGUID);
			GUIDManager.RegisterObject(this);
		}
		((Component)this).transform.SetParent(OverrideParent ? OverriddenParent : NetworkSingleton<NPCManager>.Instance.NPCContainer);
		EnvironmentHandler.RegisterWeatherEntity(this);
	}

	protected virtual void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
			NetworkSingleton<TimeManager>.Instance.onTick -= new Action(OnTick);
		}
		EnvironmentHandler.UnregisterWeatherEntity(this);
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsLocalClient)
		{
			if (RelationData.Unlocked)
			{
				ReceiveRelationshipData(connection, RelationData.RelationDelta, unlocked: true);
			}
			if (IsInVehicle)
			{
				EnterVehicle(connection, CurrentVehicle);
			}
			if (isInBuilding)
			{
				EnterBuilding(connection, CurrentBuilding.GUID.ToString(), ArrayExt.IndexOf<StaticDoor>(CurrentBuilding.Doors, LastEnteredDoor));
			}
			SetTransform(connection, ((Component)this).transform.position, ((Component)this).transform.rotation);
			if ((Object)(object)Avatar.CurrentEquippable != (Object)null)
			{
				SetEquippable_Client(connection, Avatar.CurrentEquippable.AssetPath);
			}
		}
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		RandomizeUseUmbrella();
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepEnd = (Action)Delegate.Combine(instance.onSleepEnd, new Action(RandomizeUseUmbrella));
	}

	[ObserversRpc]
	private void SetTransform(NetworkConnection conn, Vector3 position, Quaternion rotation)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_SetTransform_4260003484(conn, position, rotation);
	}

	protected virtual void MinPass()
	{
	}

	protected virtual void OnUncappedMinPass()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		TimeSincePanicked++;
		if (IsPanicked && TimeSincePanicked >= 15f)
		{
			RemovePanicked();
		}
		if ((Object)(object)CurrentVehicle != (Object)null)
		{
			VehicleLights component = ((Component)CurrentVehicle).GetComponent<VehicleLights>();
			if ((Object)(object)component != (Object)null)
			{
				component.HeadlightsOn = NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(headlightStartTime, heaedLightsEndTime);
			}
		}
	}

	protected virtual void OnTick()
	{
	}

	public virtual void SetVisible(bool visible, bool networked = false)
	{
		if (networked)
		{
			SetVisible_Networked(visible);
			return;
		}
		isVisible = visible;
		((Component)modelContainer).gameObject.SetActive(isVisible);
		if (InstanceFinder.IsServer || !isVisible)
		{
			Movement.SetAgentEnabled(isVisible);
		}
		if (onVisibilityChanged != null)
		{
			onVisibilityChanged(visible);
		}
	}

	[ObserversRpc(RunLocally = true)]
	private void SetVisible_Networked(bool visible)
	{
		RpcWriter___Observers_SetVisible_Networked_1140765316(visible);
		RpcLogic___SetVisible_Networked_1140765316(visible);
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
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localScale = new Vector3(Scale, Scale, Scale);
	}

	[ServerRpc(RequireOwnership = false)]
	public virtual void AimedAtByPlayer(NetworkObject player)
	{
		RpcWriter___Server_AimedAtByPlayer_3323014238(player);
	}

	public void OverrideAggression(float aggression)
	{
		Aggression = aggression;
	}

	public void ResetAggression()
	{
		Aggression = defaultAggression;
	}

	protected virtual void OnDie()
	{
		Visibility.ClearStates();
		_networkedEquipper.UnequipAll();
	}

	protected virtual void OnKnockedOut()
	{
		if ((Object)(object)Visibility != (Object)null)
		{
			Visibility.ClearStates();
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

	protected virtual void HitByLightning()
	{
		Electrifying.ApplyToAvatar(Avatar);
		((MonoBehaviour)this).StartCoroutine(Reset());
		IEnumerator Reset()
		{
			yield return (object)new WaitForSeconds(3f);
			if ((Object)(object)Avatar != (Object)null)
			{
				Electrifying.ClearFromAvatar(Avatar);
			}
		}
	}

	public virtual void ProcessImpactForce(Vector3 forcePoint, Vector3 forceDirection, float force)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (Avatar.Ragdolled)
		{
			Movement.ApplyRagdollForce(forcePoint, forceDirection, force);
		}
		else if (force >= 150f)
		{
			if (!Avatar.Ragdolled)
			{
				Movement.ActivateRagdoll(forcePoint, forceDirection, force);
			}
		}
		else if (force >= 100f)
		{
			if (!Movement.IsOnLadder && !Avatar.Animation.IsSeated)
			{
				Avatar.Animation.Flinch(forceDirection, AvatarAnimation.EFlinchType.Heavy);
			}
			else
			{
				Avatar.Animation.Flinch(forceDirection, AvatarAnimation.EFlinchType.Light);
			}
		}
		else if (force >= 50f)
		{
			Avatar.Animation.Flinch(forceDirection, AvatarAnimation.EFlinchType.Light);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public virtual void EnterVehicle(NetworkConnection connection, LandVehicle veh)
	{
		if (connection == null)
		{
			RpcWriter___Observers_EnterVehicle_3321926803(connection, veh);
			RpcLogic___EnterVehicle_3321926803(connection, veh);
		}
		else
		{
			RpcWriter___Target_EnterVehicle_3321926803(connection, veh);
		}
	}

	[ObserversRpc(RunLocally = true)]
	public virtual void ExitVehicle()
	{
		RpcWriter___Observers_ExitVehicle_2166136261();
		RpcLogic___ExitVehicle_2166136261();
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendWorldspaceDialogueReaction(string key, float duration)
	{
		RpcWriter___Server_SendWorldspaceDialogueReaction_606697822(key, duration);
		RpcLogic___SendWorldspaceDialogueReaction_606697822(key, duration);
	}

	[ObserversRpc(RunLocally = true)]
	private void PlayWorldspaceDialogueReaction(string key, float duration)
	{
		RpcWriter___Observers_PlayWorldspaceDialogueReaction_606697822(key, duration);
		RpcLogic___PlayWorldspaceDialogueReaction_606697822(key, duration);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendWorldSpaceDialogue(string text, float duration)
	{
		RpcWriter___Server_SendWorldSpaceDialogue_606697822(text, duration);
	}

	[ObserversRpc(RunLocally = true)]
	public void ShowWorldSpaceDialogue(string text, float duration)
	{
		RpcWriter___Observers_ShowWorldSpaceDialogue_606697822(text, duration);
		RpcLogic___ShowWorldSpaceDialogue_606697822(text, duration);
	}

	private void Hovered_Internal()
	{
		Hovered();
	}

	private void Interacted_Internal()
	{
		Interacted();
	}

	protected virtual void Hovered()
	{
	}

	protected virtual void Interacted()
	{
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void EnterBuilding(NetworkConnection connection, string buildingGUID, int doorIndex)
	{
		if (connection == null)
		{
			RpcWriter___Observers_EnterBuilding_3905681115(connection, buildingGUID, doorIndex);
			RpcLogic___EnterBuilding_3905681115(connection, buildingGUID, doorIndex);
		}
		else
		{
			RpcWriter___Target_EnterBuilding_3905681115(connection, buildingGUID, doorIndex);
		}
	}

	protected virtual void EnterBuilding(string buildingGUID, int doorIndex)
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		NPCEnterableBuilding nPCEnterableBuilding = GUIDManager.GetObject<NPCEnterableBuilding>(new Guid(buildingGUID));
		if ((Object)(object)nPCEnterableBuilding == (Object)null)
		{
			Console.LogWarning(fullName + ".EnterBuilding: building not found with given GUID");
			return;
		}
		if ((Object)(object)nPCEnterableBuilding == (Object)(object)CurrentBuilding)
		{
			if (InstanceFinder.IsServer)
			{
				Movement.Stop();
				Movement.Warp(nPCEnterableBuilding.Doors[doorIndex].AccessPoint);
				Movement.FaceDirection(-nPCEnterableBuilding.Doors[doorIndex].AccessPoint.forward, 0f);
			}
			SetVisible(visible: false);
			return;
		}
		if ((Object)(object)CurrentBuilding != (Object)null)
		{
			Console.LogWarning("NPC.EnterBuilding called but NPC is already in a building. New building will still be entered.");
			ExitBuilding();
		}
		CurrentBuilding = nPCEnterableBuilding;
		LastEnteredDoor = nPCEnterableBuilding.Doors[doorIndex];
		Awareness.SetAwarenessActive(active: false);
		nPCEnterableBuilding.NPCEnteredBuilding(this, LastEnteredDoor);
		SetVisible(visible: false);
		Movement.Stop();
		Movement.Warp(nPCEnterableBuilding.Doors[doorIndex].AccessPoint);
		Movement.FaceDirection(-nPCEnterableBuilding.Doors[doorIndex].AccessPoint.forward, 0f);
	}

	[ObserversRpc(RunLocally = true)]
	public void ExitBuilding(string buildingID = "")
	{
		RpcWriter___Observers_ExitBuilding_3615296227(buildingID);
		RpcLogic___ExitBuilding_3615296227(buildingID);
	}

	protected virtual void ExitBuilding(NPCEnterableBuilding building)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)building == (Object)null))
		{
			if ((Object)(object)LastEnteredDoor == (Object)null)
			{
				LastEnteredDoor = building.Doors[0];
			}
			((Component)Avatar).transform.localPosition = Vector3.zero;
			((Component)Avatar).transform.localRotation = Quaternion.identity;
			NavMeshHit hit;
			Vector3 position = (NavMeshUtility.SamplePosition(((Component)LastEnteredDoor.AccessPoint).transform.position, out hit, 2f, -1) ? ((NavMeshHit)(ref hit)).position : ((Component)LastEnteredDoor.AccessPoint).transform.position);
			Movement.Warp(position);
			Movement.FaceDirection(-((Component)LastEnteredDoor.AccessPoint).transform.forward, 0f);
			Awareness.SetAwarenessActive(active: true);
			building.NPCExitedBuilding(this, LastEnteredDoor);
			CurrentBuilding = null;
			SetVisible(visible: true);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetEquippable_Client(NetworkConnection conn, string assetPath)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetEquippable_Client_2971853958(conn, assetPath);
			RpcLogic___SetEquippable_Client_2971853958(conn, assetPath);
		}
		else
		{
			RpcWriter___Target_SetEquippable_Client_2971853958(conn, assetPath);
		}
	}

	public AvatarEquippable SetEquippable_Networked_Return(NetworkConnection conn, string assetPath)
	{
		SetEquippable_Networked_ExcludeServer(conn, assetPath);
		return Avatar.SetEquippable(assetPath);
	}

	public AvatarEquippable SetEquippable_Return(string assetPath)
	{
		return Avatar.SetEquippable(assetPath);
	}

	[ObserversRpc(RunLocally = false, ExcludeServer = true)]
	private void SetEquippable_Networked_ExcludeServer(NetworkConnection conn, string assetPath)
	{
		RpcWriter___Observers_SetEquippable_Networked_ExcludeServer_2971853958(conn, assetPath);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SendEquippableMessage_Networked(NetworkConnection conn, string message)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SendEquippableMessage_Networked_2971853958(conn, message);
			RpcLogic___SendEquippableMessage_Networked_2971853958(conn, message);
		}
		else
		{
			RpcWriter___Target_SendEquippableMessage_Networked_2971853958(conn, message);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SendEquippableMessage_Networked_Vector(NetworkConnection conn, string message, Vector3 data)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (conn == null)
		{
			RpcWriter___Observers_SendEquippableMessage_Networked_Vector_4022222929(conn, message, data);
			RpcLogic___SendEquippableMessage_Networked_Vector_4022222929(conn, message, data);
		}
		else
		{
			RpcWriter___Target_SendEquippableMessage_Networked_Vector_4022222929(conn, message, data);
		}
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

	[ServerRpc(RequireOwnership = false)]
	public void SendAnimationTrigger(string trigger)
	{
		RpcWriter___Server_SendAnimationTrigger_3615296227(trigger);
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

	[ObserversRpc(RunLocally = true)]
	public void SetCrouched_Networked(bool crouched)
	{
		RpcWriter___Observers_SetCrouched_Networked_1140765316(crouched);
		RpcLogic___SetCrouched_Networked_1140765316(crouched);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetAnimationBool_Networked(NetworkConnection conn, string id, bool value)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetAnimationBool_Networked_619441887(conn, id, value);
			RpcLogic___SetAnimationBool_Networked_619441887(conn, id, value);
		}
		else
		{
			RpcWriter___Target_SetAnimationBool_Networked_619441887(conn, id, value);
		}
	}

	public void SetAnimationBool(string trigger, bool val)
	{
		Avatar.Animation.SetBool(trigger, val);
	}

	protected virtual void SetUnsettled_30s(Player player)
	{
		SetUnsettled(30f);
	}

	protected void SetUnsettled(float duration)
	{
		bool num = isUnsettled;
		isUnsettled = true;
		if (!num)
		{
			Avatar.EmotionManager.AddEmotionOverride("Concerned", "unsettled", 0f, 5);
		}
		if (resetUnsettledCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(resetUnsettledCoroutine);
		}
		resetUnsettledCoroutine = ((MonoBehaviour)this).StartCoroutine(ResetUnsettled());
		IEnumerator ResetUnsettled()
		{
			Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("unsetttled", 10, 0.2f));
			yield return (object)new WaitForSeconds(duration);
			isUnsettled = false;
			Avatar.EmotionManager.RemoveEmotionOverride("unsettled");
			Movement.SpeedController.RemoveSpeedControl("unsettled");
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetPanicked_Server()
	{
		RpcWriter___Server_SetPanicked_Server_2166136261();
		RpcLogic___SetPanicked_Server_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void SetPanicked_Client()
	{
		RpcWriter___Observers_SetPanicked_Client_2166136261();
		RpcLogic___SetPanicked_Client_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void RemovePanicked()
	{
		RpcWriter___Observers_RemovePanicked_2166136261();
		RpcLogic___RemovePanicked_2166136261();
	}

	public virtual string GetNameAddress()
	{
		return FirstName;
	}

	public void PlayVO(EVOLineType lineType, bool network = false)
	{
		if (network)
		{
			PlayVO_Server(lineType);
		}
		else
		{
			VoiceOverEmitter.Play(lineType);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void PlayVO_Server(EVOLineType lineType)
	{
		RpcWriter___Server_PlayVO_Server_1710085680(lineType);
	}

	[ObserversRpc(RunLocally = true)]
	private void PlayVO_Client(EVOLineType lineType)
	{
		RpcWriter___Observers_PlayVO_Client_1710085680(lineType);
		RpcLogic___PlayVO_Client_1710085680(lineType);
	}

	[TargetRpc]
	public void ReceiveRelationshipData(NetworkConnection conn, float relationship, bool unlocked)
	{
		RpcWriter___Target_ReceiveRelationshipData_4052192084(conn, relationship, unlocked);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetIsBeingPickPocketed(bool pickpocketed)
	{
		RpcWriter___Server_SetIsBeingPickPocketed_1140765316(pickpocketed);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendRelationship(float relationship)
	{
		RpcWriter___Server_SendRelationship_431000436(relationship);
	}

	[ObserversRpc]
	private void SetRelationship(float relationship)
	{
		RpcWriter___Observers_SetRelationship_431000436(relationship);
	}

	private void RandomizeUseUmbrella()
	{
		HasUmbrella = Random.value < _useUmbrellaChance;
	}

	public void ShowOutline(Color color)
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		if ((Object)(object)OutlineEffect == (Object)null)
		{
			OutlineEffect = ((Component)this).gameObject.AddComponent<Outlinable>();
			OutlineEffect.OutlineParameters.BlurShift = 0f;
			OutlineEffect.OutlineParameters.DilateShift = 0.5f;
			OutlineEffect.OutlineParameters.FillPass.Shader = Resources.Load<Shader>("Easy performant outline/Shaders/Fills/ColorFill");
			foreach (GameObject outlineRenderer in OutlineRenderers)
			{
				SkinnedMeshRenderer[] array = (SkinnedMeshRenderer[])(object)new SkinnedMeshRenderer[0];
				array = (SkinnedMeshRenderer[])(object)new SkinnedMeshRenderer[1] { outlineRenderer.GetComponent<SkinnedMeshRenderer>() };
				for (int i = 0; i < array.Length; i++)
				{
					OutlineTarget val = new OutlineTarget((Renderer)(object)array[i], 0);
					OutlineEffect.TryAddTarget(val);
				}
			}
		}
		OutlineEffect.OutlineParameters.Color = color;
		Color32 val2 = Color32.op_Implicit(color);
		val2.a = 9;
		OutlineEffect.OutlineParameters.FillPass.SetColor("_PublicColor", Color32.op_Implicit(val2));
		((Behaviour)OutlineEffect).enabled = true;
	}

	public void HideOutline()
	{
		if ((Object)(object)OutlineEffect != (Object)null)
		{
			((Behaviour)OutlineEffect).enabled = false;
		}
	}

	public void OnWeatherChange(WeatherConditions newConditions)
	{
		_currentWeatherConditionsForEntity = newConditions;
	}

	public WeatherConditions GetWeatherTolerence()
	{
		return _weatherTolerence;
	}

	public WeatherConditions GetCurrentWeatherConditionsForEnitty()
	{
		return _currentWeatherConditionsForEntity;
	}

	public void OnUpdateWeatherEntity()
	{
		UpdateWetness();
	}

	public void UpdateWetness()
	{
		if (_currentWeatherConditionsForEntity == null)
		{
			return;
		}
		float num = (SyncAccessor__003CHasUmbrella_003Ek__BackingField ? 0.5f : 1f);
		float num2 = (IsUnderCover ? 0f : _currentWeatherConditionsForEntity.Rainy);
		float num3 = (IsUnderCover ? 1f : _currentWeatherConditionsForEntity.Sunny);
		float num4 = num2 * 0.1f;
		float num5 = num3 * 0.05f;
		float num6 = (num4 - num5) * Time.deltaTime;
		num6 = Mathf.Clamp(_wetness + num6, 0f, num);
		if (num6 != _wetness)
		{
			_wetness = num6;
			for (int i = 0; i < Avatar.BodyMeshes.Length; i++)
			{
				((Renderer)Avatar.BodyMeshes[i]).material.SetFloat("_Wetness", _wetness);
			}
		}
	}

	public virtual bool ShouldSave()
	{
		if (ShouldSaveRelationshipData())
		{
			return true;
		}
		if (ShouldSaveMessages())
		{
			return true;
		}
		if (ShouldSaveInventory())
		{
			return true;
		}
		if (ShouldSaveHealth())
		{
			return true;
		}
		return HasChanged;
	}

	protected virtual bool ShouldSaveRelationshipData()
	{
		if (RelationData.Unlocked)
		{
			return true;
		}
		if (2f != RelationData.RelationDelta)
		{
			return true;
		}
		return false;
	}

	protected bool ShouldSaveMessages()
	{
		if (MSGConversation == null)
		{
			return false;
		}
		if (MSGConversation.messageHistory.Count > 0)
		{
			return true;
		}
		return false;
	}

	protected virtual bool ShouldSaveInventory()
	{
		return ((IItemSlotOwner)Inventory).GetQuantitySum() > 0;
	}

	protected virtual bool ShouldSaveHealth()
	{
		if (!(Health.Health < Health.MaxHealth) && !Health.IsDead)
		{
			return Health.DaysPassedSinceDeath > 0;
		}
		return true;
	}

	public string GetSaveString()
	{
		return GetSaveData().GetJson();
	}

	public virtual NPCData GetNPCData()
	{
		return new NPCData(ID);
	}

	public virtual DynamicSaveData GetSaveData()
	{
		DynamicSaveData dynamicSaveData = new DynamicSaveData(GetNPCData());
		if (ShouldSaveRelationshipData())
		{
			dynamicSaveData.AddData("Relationship", RelationData.GetSaveData().GetJson());
		}
		if (ShouldSaveMessages())
		{
			dynamicSaveData.AddData("MessageConversation", MSGConversation.GetSaveData().GetJson());
		}
		if (ShouldSaveInventory())
		{
			dynamicSaveData.AddData("Inventory", new ItemSet(Inventory.ItemSlots).GetJSON());
		}
		if (ShouldSaveHealth())
		{
			dynamicSaveData.AddData("Health", new NPCHealthData(Health.Health, Health.IsDead, Health.DaysPassedSinceDeath, Health.HoursSinceAttackedByPlayer).GetJson());
		}
		Customer component = ((Component)this).GetComponent<Customer>();
		if ((Object)(object)component != (Object)null)
		{
			dynamicSaveData.AddData("CustomerData", component.GetSaveString());
		}
		return dynamicSaveData;
	}

	public virtual List<string> WriteData(string parentFolderPath)
	{
		return new List<string>();
	}

	public virtual void Load(NPCData data, string containerPath)
	{
	}

	public virtual void Load(DynamicSaveData dynamicData, NPCData npcData)
	{
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Expected O, but got Unknown
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Expected O, but got Unknown
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected O, but got Unknown
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Expected O, but got Unknown
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Expected O, but got Unknown
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Expected O, but got Unknown
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Expected O, but got Unknown
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Expected O, but got Unknown
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Expected O, but got Unknown
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Expected O, but got Unknown
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Expected O, but got Unknown
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Expected O, but got Unknown
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Expected O, but got Unknown
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Expected O, but got Unknown
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Expected O, but got Unknown
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Expected O, but got Unknown
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Expected O, but got Unknown
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Expected O, but got Unknown
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Expected O, but got Unknown
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Expected O, but got Unknown
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Expected O, but got Unknown
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Expected O, but got Unknown
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Expected O, but got Unknown
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Expected O, but got Unknown
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Expected O, but got Unknown
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Expected O, but got Unknown
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Expected O, but got Unknown
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Expected O, but got Unknown
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCAssembly_002DCSharp_002Edll_Excuted = true;
			syncVar____003CHasUmbrella_003Ek__BackingField = new SyncVar<bool>((NetworkBehaviour)(object)this, 0u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, HasUmbrella);
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_SetTransform_4260003484));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetVisible_Networked_1140765316));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_AimedAtByPlayer_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(3u, new ServerRpcDelegate(RpcReader___Server_SendImpact_427288424));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_ReceiveImpact_427288424));
			((NetworkBehaviour)this).RegisterObserversRpc(5u, new ClientRpcDelegate(RpcReader___Observers_EnterVehicle_3321926803));
			((NetworkBehaviour)this).RegisterTargetRpc(6u, new ClientRpcDelegate(RpcReader___Target_EnterVehicle_3321926803));
			((NetworkBehaviour)this).RegisterObserversRpc(7u, new ClientRpcDelegate(RpcReader___Observers_ExitVehicle_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(8u, new ServerRpcDelegate(RpcReader___Server_SendWorldspaceDialogueReaction_606697822));
			((NetworkBehaviour)this).RegisterObserversRpc(9u, new ClientRpcDelegate(RpcReader___Observers_PlayWorldspaceDialogueReaction_606697822));
			((NetworkBehaviour)this).RegisterServerRpc(10u, new ServerRpcDelegate(RpcReader___Server_SendWorldSpaceDialogue_606697822));
			((NetworkBehaviour)this).RegisterObserversRpc(11u, new ClientRpcDelegate(RpcReader___Observers_ShowWorldSpaceDialogue_606697822));
			((NetworkBehaviour)this).RegisterObserversRpc(12u, new ClientRpcDelegate(RpcReader___Observers_EnterBuilding_3905681115));
			((NetworkBehaviour)this).RegisterTargetRpc(13u, new ClientRpcDelegate(RpcReader___Target_EnterBuilding_3905681115));
			((NetworkBehaviour)this).RegisterObserversRpc(14u, new ClientRpcDelegate(RpcReader___Observers_ExitBuilding_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(15u, new ClientRpcDelegate(RpcReader___Observers_SetEquippable_Client_2971853958));
			((NetworkBehaviour)this).RegisterTargetRpc(16u, new ClientRpcDelegate(RpcReader___Target_SetEquippable_Client_2971853958));
			((NetworkBehaviour)this).RegisterObserversRpc(17u, new ClientRpcDelegate(RpcReader___Observers_SetEquippable_Networked_ExcludeServer_2971853958));
			((NetworkBehaviour)this).RegisterObserversRpc(18u, new ClientRpcDelegate(RpcReader___Observers_SendEquippableMessage_Networked_2971853958));
			((NetworkBehaviour)this).RegisterTargetRpc(19u, new ClientRpcDelegate(RpcReader___Target_SendEquippableMessage_Networked_2971853958));
			((NetworkBehaviour)this).RegisterObserversRpc(20u, new ClientRpcDelegate(RpcReader___Observers_SendEquippableMessage_Networked_Vector_4022222929));
			((NetworkBehaviour)this).RegisterTargetRpc(21u, new ClientRpcDelegate(RpcReader___Target_SendEquippableMessage_Networked_Vector_4022222929));
			((NetworkBehaviour)this).RegisterServerRpc(22u, new ServerRpcDelegate(RpcReader___Server_SendAnimationTrigger_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(23u, new ClientRpcDelegate(RpcReader___Observers_SetAnimationTrigger_Networked_2971853958));
			((NetworkBehaviour)this).RegisterTargetRpc(24u, new ClientRpcDelegate(RpcReader___Target_SetAnimationTrigger_Networked_2971853958));
			((NetworkBehaviour)this).RegisterObserversRpc(25u, new ClientRpcDelegate(RpcReader___Observers_ResetAnimationTrigger_Networked_2971853958));
			((NetworkBehaviour)this).RegisterTargetRpc(26u, new ClientRpcDelegate(RpcReader___Target_ResetAnimationTrigger_Networked_2971853958));
			((NetworkBehaviour)this).RegisterObserversRpc(27u, new ClientRpcDelegate(RpcReader___Observers_SetCrouched_Networked_1140765316));
			((NetworkBehaviour)this).RegisterObserversRpc(28u, new ClientRpcDelegate(RpcReader___Observers_SetAnimationBool_Networked_619441887));
			((NetworkBehaviour)this).RegisterTargetRpc(29u, new ClientRpcDelegate(RpcReader___Target_SetAnimationBool_Networked_619441887));
			((NetworkBehaviour)this).RegisterServerRpc(30u, new ServerRpcDelegate(RpcReader___Server_SetPanicked_Server_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(31u, new ClientRpcDelegate(RpcReader___Observers_SetPanicked_Client_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(32u, new ClientRpcDelegate(RpcReader___Observers_RemovePanicked_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(33u, new ServerRpcDelegate(RpcReader___Server_PlayVO_Server_1710085680));
			((NetworkBehaviour)this).RegisterObserversRpc(34u, new ClientRpcDelegate(RpcReader___Observers_PlayVO_Client_1710085680));
			((NetworkBehaviour)this).RegisterTargetRpc(35u, new ClientRpcDelegate(RpcReader___Target_ReceiveRelationshipData_4052192084));
			((NetworkBehaviour)this).RegisterServerRpc(36u, new ServerRpcDelegate(RpcReader___Server_SetIsBeingPickPocketed_1140765316));
			((NetworkBehaviour)this).RegisterServerRpc(37u, new ServerRpcDelegate(RpcReader___Server_SendRelationship_431000436));
			((NetworkBehaviour)this).RegisterObserversRpc(38u, new ClientRpcDelegate(RpcReader___Observers_SetRelationship_431000436));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002ENPCs_002ENPC));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCAssembly_002DCSharp_002Edll_Excuted = true;
			((SyncBase)syncVar____003CHasUmbrella_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_SetTransform_4260003484(NetworkConnection conn, Vector3 position, Quaternion rotation)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetTransform_4260003484(NetworkConnection conn, Vector3 position, Quaternion rotation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = position;
		((Component)this).transform.rotation = rotation;
	}

	private void RpcReader___Observers_SetTransform_4260003484(PooledReader PooledReader0, Channel channel)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		NetworkConnection conn = ((Reader)PooledReader0).ReadNetworkConnection();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetTransform_4260003484(conn, position, rotation);
		}
	}

	private void RpcWriter___Observers_SetVisible_Networked_1140765316(bool visible)
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
			((Writer)writer).WriteBoolean(visible);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetVisible_Networked_1140765316(bool visible)
	{
		SetVisible(visible);
	}

	private void RpcReader___Observers_SetVisible_Networked_1140765316(PooledReader PooledReader0, Channel channel)
	{
		bool visible = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetVisible_Networked_1140765316(visible);
		}
	}

	private void RpcWriter___Server_AimedAtByPlayer_3323014238(NetworkObject player)
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
			((Writer)writer).WriteNetworkObject(player);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public virtual void RpcLogic___AimedAtByPlayer_3323014238(NetworkObject player)
	{
		Responses.RespondToAimedAt(((Component)player).GetComponent<Player>());
	}

	private void RpcReader___Server_AimedAtByPlayer_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___AimedAtByPlayer_3323014238(player);
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
			((NetworkBehaviour)this).SendServerRpc(3u, writer, val, (DataOrderType)0);
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
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public virtual void RpcLogic___ReceiveImpact_427288424(Impact impact)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (!IgnoreImpacts && !impactHistory.Contains(impact.ImpactID))
		{
			impactHistory.Add(impact.ImpactID);
			float num = 1f;
			switch (Movement.Stance)
			{
			case NPCMovement.EStance.None:
				num = 1f;
				break;
			case NPCMovement.EStance.Stanced:
				num = 0.67f;
				break;
			}
			if (impact.IsPlayerImpact(out var player))
			{
				Health.NotifyAttackedByPlayer(player);
			}
			Health.TakeDamage(impact.ImpactDamage, Impact.IsLethal(impact.ImpactType));
			ProcessImpactForce(impact.HitPoint, impact.ImpactForceDirection, impact.ImpactForce * num);
			Responses.ImpactReceived(impact);
			if (impact.ImpactType == EImpactType.Explosion && impact.ExplosionType == EExplosionType.Lightning)
			{
				HitByLightning();
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

	private void RpcWriter___Observers_EnterVehicle_3321926803(NetworkConnection connection, LandVehicle veh)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EVehicles_002ELandVehicleFishNet_002ESerializing_002EGenerated((Writer)(object)writer, veh);
			((NetworkBehaviour)this).SendObserversRpc(5u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public virtual void RpcLogic___EnterVehicle_3321926803(NetworkConnection connection, LandVehicle veh)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)veh == (Object)(object)CurrentVehicle))
		{
			CurrentVehicle = veh;
			SetVisible(visible: false);
			Movement.SetAgentEnabled(enabled: false);
			((Component)this).transform.SetParent(((Component)veh).transform);
			veh.AddNPCOccupant(this);
			int num = CurrentVehicle.OccupantNPCs.ToList().IndexOf(this);
			((Component)this).transform.position = ((Component)CurrentVehicle.Seats[Mathf.Clamp(num, 0, CurrentVehicle.Seats.Length - 1)]).transform.position;
			((Component)this).transform.localRotation = Quaternion.identity;
			if (onEnterVehicle != null)
			{
				onEnterVehicle(veh);
			}
		}
	}

	private void RpcReader___Observers_EnterVehicle_3321926803(PooledReader PooledReader0, Channel channel)
	{
		LandVehicle veh = GeneratedReaders___Internal.Read___ScheduleOne_002EVehicles_002ELandVehicleFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___EnterVehicle_3321926803(null, veh);
		}
	}

	private void RpcWriter___Target_EnterVehicle_3321926803(NetworkConnection connection, LandVehicle veh)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EVehicles_002ELandVehicleFishNet_002ESerializing_002EGenerated((Writer)(object)writer, veh);
			((NetworkBehaviour)this).SendTargetRpc(6u, writer, val, (DataOrderType)0, connection, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_EnterVehicle_3321926803(PooledReader PooledReader0, Channel channel)
	{
		LandVehicle veh = GeneratedReaders___Internal.Read___ScheduleOne_002EVehicles_002ELandVehicleFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___EnterVehicle_3321926803(((NetworkBehaviour)this).LocalConnection, veh);
		}
	}

	private void RpcWriter___Observers_ExitVehicle_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(7u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public virtual void RpcLogic___ExitVehicle_2166136261()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)CurrentVehicle == (Object)null))
		{
			int seatIndex = CurrentVehicle.OccupantNPCs.ToList().IndexOf(this);
			CurrentVehicle.RemoveNPCOccupant(this);
			CurrentVehicle.Agent.Flags.ResetFlags();
			if ((Object)(object)((Component)CurrentVehicle).GetComponent<VehicleLights>() != (Object)null)
			{
				((Component)CurrentVehicle).GetComponent<VehicleLights>().HeadlightsOn = false;
			}
			Transform exitPoint = CurrentVehicle.GetExitPoint(seatIndex);
			((Component)this).transform.SetParent(OverrideParent ? OverriddenParent : NetworkSingleton<NPCManager>.Instance.NPCContainer);
			((Component)this).transform.position = exitPoint.position - exitPoint.up * 1f;
			Movement.FaceDirection(exitPoint.forward, 0f);
			if (InstanceFinder.IsServer)
			{
				Movement.SetAgentEnabled(enabled: true);
			}
			SetVisible(visible: true);
			if (onExitVehicle != null)
			{
				onExitVehicle(CurrentVehicle);
			}
			CurrentVehicle = null;
		}
	}

	private void RpcReader___Observers_ExitVehicle_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ExitVehicle_2166136261();
		}
	}

	private void RpcWriter___Server_SendWorldspaceDialogueReaction_606697822(string key, float duration)
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
			((Writer)writer).WriteString(key);
			((Writer)writer).WriteSingle(duration, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(8u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendWorldspaceDialogueReaction_606697822(string key, float duration)
	{
		PlayWorldspaceDialogueReaction(key, duration);
	}

	private void RpcReader___Server_SendWorldspaceDialogueReaction_606697822(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string key = ((Reader)PooledReader0).ReadString();
		float duration = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendWorldspaceDialogueReaction_606697822(key, duration);
		}
	}

	private void RpcWriter___Observers_PlayWorldspaceDialogueReaction_606697822(string key, float duration)
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
			((Writer)writer).WriteString(key);
			((Writer)writer).WriteSingle(duration, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(9u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___PlayWorldspaceDialogueReaction_606697822(string key, float duration)
	{
		DialogueHandler.PlayReaction(key, duration, network: false);
	}

	private void RpcReader___Observers_PlayWorldspaceDialogueReaction_606697822(PooledReader PooledReader0, Channel channel)
	{
		string key = ((Reader)PooledReader0).ReadString();
		float duration = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___PlayWorldspaceDialogueReaction_606697822(key, duration);
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
			((NetworkBehaviour)this).SendServerRpc(10u, writer, val, (DataOrderType)0);
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
			((NetworkBehaviour)this).SendObserversRpc(11u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___ShowWorldSpaceDialogue_606697822(string text, float duration)
	{
		DialogueHandler.ShowWorldspaceDialogue(text, duration);
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

	private void RpcWriter___Observers_EnterBuilding_3905681115(NetworkConnection connection, string buildingGUID, int doorIndex)
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
			((Writer)writer).WriteString(buildingGUID);
			((Writer)writer).WriteInt32(doorIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(12u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___EnterBuilding_3905681115(NetworkConnection connection, string buildingGUID, int doorIndex)
	{
		EnterBuilding(buildingGUID, doorIndex);
	}

	private void RpcReader___Observers_EnterBuilding_3905681115(PooledReader PooledReader0, Channel channel)
	{
		string buildingGUID = ((Reader)PooledReader0).ReadString();
		int doorIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___EnterBuilding_3905681115(null, buildingGUID, doorIndex);
		}
	}

	private void RpcWriter___Target_EnterBuilding_3905681115(NetworkConnection connection, string buildingGUID, int doorIndex)
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
			((Writer)writer).WriteString(buildingGUID);
			((Writer)writer).WriteInt32(doorIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(13u, writer, val, (DataOrderType)0, connection, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_EnterBuilding_3905681115(PooledReader PooledReader0, Channel channel)
	{
		string buildingGUID = ((Reader)PooledReader0).ReadString();
		int doorIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___EnterBuilding_3905681115(((NetworkBehaviour)this).LocalConnection, buildingGUID, doorIndex);
		}
	}

	private void RpcWriter___Observers_ExitBuilding_3615296227(string buildingID = "")
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
			((Writer)writer).WriteString(buildingID);
			((NetworkBehaviour)this).SendObserversRpc(14u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___ExitBuilding_3615296227(string buildingID = "")
	{
		if (buildingID == "" && (Object)(object)CurrentBuilding != (Object)null)
		{
			buildingID = CurrentBuilding.GUID.ToString();
		}
		if (!(buildingID == ""))
		{
			ExitBuilding(GUIDManager.GetObject<NPCEnterableBuilding>(new Guid(buildingID)));
		}
	}

	private void RpcReader___Observers_ExitBuilding_3615296227(PooledReader PooledReader0, Channel channel)
	{
		string buildingID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ExitBuilding_3615296227(buildingID);
		}
	}

	private void RpcWriter___Observers_SetEquippable_Client_2971853958(NetworkConnection conn, string assetPath)
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
			((NetworkBehaviour)this).SendObserversRpc(15u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetEquippable_Client_2971853958(NetworkConnection conn, string assetPath)
	{
		Avatar.SetEquippable(assetPath);
	}

	private void RpcReader___Observers_SetEquippable_Client_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string assetPath = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetEquippable_Client_2971853958(null, assetPath);
		}
	}

	private void RpcWriter___Target_SetEquippable_Client_2971853958(NetworkConnection conn, string assetPath)
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
			((NetworkBehaviour)this).SendTargetRpc(16u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetEquippable_Client_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string assetPath = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetEquippable_Client_2971853958(((NetworkBehaviour)this).LocalConnection, assetPath);
		}
	}

	private void RpcWriter___Observers_SetEquippable_Networked_ExcludeServer_2971853958(NetworkConnection conn, string assetPath)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((Writer)writer).WriteString(assetPath);
			((NetworkBehaviour)this).SendObserversRpc(17u, writer, val, (DataOrderType)0, false, true, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetEquippable_Networked_ExcludeServer_2971853958(NetworkConnection conn, string assetPath)
	{
		Avatar.SetEquippable(assetPath);
	}

	private void RpcReader___Observers_SetEquippable_Networked_ExcludeServer_2971853958(PooledReader PooledReader0, Channel channel)
	{
		NetworkConnection conn = ((Reader)PooledReader0).ReadNetworkConnection();
		string assetPath = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetEquippable_Networked_ExcludeServer_2971853958(conn, assetPath);
		}
	}

	private void RpcWriter___Observers_SendEquippableMessage_Networked_2971853958(NetworkConnection conn, string message)
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
			((Writer)writer).WriteString(message);
			((NetworkBehaviour)this).SendObserversRpc(18u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SendEquippableMessage_Networked_2971853958(NetworkConnection conn, string message)
	{
		Avatar.ReceiveEquippableMessage(message, null);
	}

	private void RpcReader___Observers_SendEquippableMessage_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string message = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SendEquippableMessage_Networked_2971853958(null, message);
		}
	}

	private void RpcWriter___Target_SendEquippableMessage_Networked_2971853958(NetworkConnection conn, string message)
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
			((Writer)writer).WriteString(message);
			((NetworkBehaviour)this).SendTargetRpc(19u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SendEquippableMessage_Networked_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string message = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SendEquippableMessage_Networked_2971853958(((NetworkBehaviour)this).LocalConnection, message);
		}
	}

	private void RpcWriter___Observers_SendEquippableMessage_Networked_Vector_4022222929(NetworkConnection conn, string message, Vector3 data)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(message);
			((Writer)writer).WriteVector3(data);
			((NetworkBehaviour)this).SendObserversRpc(20u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SendEquippableMessage_Networked_Vector_4022222929(NetworkConnection conn, string message, Vector3 data)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Avatar.ReceiveEquippableMessage(message, data);
	}

	private void RpcReader___Observers_SendEquippableMessage_Networked_Vector_4022222929(PooledReader PooledReader0, Channel channel)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		string message = ((Reader)PooledReader0).ReadString();
		Vector3 data = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SendEquippableMessage_Networked_Vector_4022222929(null, message, data);
		}
	}

	private void RpcWriter___Target_SendEquippableMessage_Networked_Vector_4022222929(NetworkConnection conn, string message, Vector3 data)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(message);
			((Writer)writer).WriteVector3(data);
			((NetworkBehaviour)this).SendTargetRpc(21u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SendEquippableMessage_Networked_Vector_4022222929(PooledReader PooledReader0, Channel channel)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		string message = ((Reader)PooledReader0).ReadString();
		Vector3 data = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SendEquippableMessage_Networked_Vector_4022222929(((NetworkBehaviour)this).LocalConnection, message, data);
		}
	}

	private void RpcWriter___Server_SendAnimationTrigger_3615296227(string trigger)
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
			((Writer)writer).WriteString(trigger);
			((NetworkBehaviour)this).SendServerRpc(22u, writer, val, (DataOrderType)0);
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
		if (((NetworkBehaviour)this).IsServerInitialized)
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
			((NetworkBehaviour)this).SendObserversRpc(23u, writer, val, (DataOrderType)0, false, false, false);
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
			((NetworkBehaviour)this).SendTargetRpc(24u, writer, val, (DataOrderType)0, conn, false, true);
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
			((NetworkBehaviour)this).SendObserversRpc(25u, writer, val, (DataOrderType)0, false, false, false);
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
			((NetworkBehaviour)this).SendTargetRpc(26u, writer, val, (DataOrderType)0, conn, false, true);
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

	private void RpcWriter___Observers_SetCrouched_Networked_1140765316(bool crouched)
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
			((NetworkBehaviour)this).SendObserversRpc(27u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetCrouched_Networked_1140765316(bool crouched)
	{
		Avatar.Animation.SetCrouched(crouched);
	}

	private void RpcReader___Observers_SetCrouched_Networked_1140765316(PooledReader PooledReader0, Channel channel)
	{
		bool crouched = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetCrouched_Networked_1140765316(crouched);
		}
	}

	private void RpcWriter___Observers_SetAnimationBool_Networked_619441887(NetworkConnection conn, string id, bool value)
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
			((Writer)writer).WriteString(id);
			((Writer)writer).WriteBoolean(value);
			((NetworkBehaviour)this).SendObserversRpc(28u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetAnimationBool_Networked_619441887(NetworkConnection conn, string id, bool value)
	{
		Avatar.Animation.SetBool(id, value);
	}

	private void RpcReader___Observers_SetAnimationBool_Networked_619441887(PooledReader PooledReader0, Channel channel)
	{
		string id = ((Reader)PooledReader0).ReadString();
		bool value = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetAnimationBool_Networked_619441887(null, id, value);
		}
	}

	private void RpcWriter___Target_SetAnimationBool_Networked_619441887(NetworkConnection conn, string id, bool value)
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
			((Writer)writer).WriteString(id);
			((Writer)writer).WriteBoolean(value);
			((NetworkBehaviour)this).SendTargetRpc(29u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetAnimationBool_Networked_619441887(PooledReader PooledReader0, Channel channel)
	{
		string id = ((Reader)PooledReader0).ReadString();
		bool value = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetAnimationBool_Networked_619441887(((NetworkBehaviour)this).LocalConnection, id, value);
		}
	}

	private void RpcWriter___Server_SetPanicked_Server_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(30u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetPanicked_Server_2166136261()
	{
		SetPanicked_Client();
	}

	private void RpcReader___Server_SetPanicked_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetPanicked_Server_2166136261();
		}
	}

	private void RpcWriter___Observers_SetPanicked_Client_2166136261()
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

	private void RpcLogic___SetPanicked_Client_2166136261()
	{
		IsPanicked = true;
		TimeSincePanicked = 0f;
		Avatar.EmotionManager.AddEmotionOverride("Scared", "panicked", 0f, 10);
		if ((Object)(object)CurrentVehicle != (Object)null)
		{
			CurrentVehicle.Agent.Flags.OverriddenSpeed = 50f;
			CurrentVehicle.Agent.Flags.OverriddenReverseSpeed = 20f;
			CurrentVehicle.Agent.Flags.OverrideSpeed = true;
			CurrentVehicle.Agent.Flags.IgnoreTrafficLights = true;
			CurrentVehicle.Agent.Flags.ObstacleMode = DriveFlags.EObstacleMode.IgnoreOnlySquishy;
		}
		else
		{
			Behaviour.CoweringBehaviour.Enable();
		}
	}

	private void RpcReader___Observers_SetPanicked_Client_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetPanicked_Client_2166136261();
		}
	}

	private void RpcWriter___Observers_RemovePanicked_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(32u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___RemovePanicked_2166136261()
	{
		IsPanicked = false;
		Avatar.EmotionManager.RemoveEmotionOverride("panicked");
		if ((Object)(object)CurrentVehicle != (Object)null)
		{
			CurrentVehicle.Agent.Flags.ResetFlags();
		}
		Behaviour.CoweringBehaviour.Disable();
	}

	private void RpcReader___Observers_RemovePanicked_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___RemovePanicked_2166136261();
		}
	}

	private void RpcWriter___Server_PlayVO_Server_1710085680(EVOLineType lineType)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EVoiceOver_002EEVOLineTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, lineType);
			((NetworkBehaviour)this).SendServerRpc(33u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___PlayVO_Server_1710085680(EVOLineType lineType)
	{
		PlayVO_Client(lineType);
	}

	private void RpcReader___Server_PlayVO_Server_1710085680(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		EVOLineType lineType = GeneratedReaders___Internal.Read___ScheduleOne_002EVoiceOver_002EEVOLineTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___PlayVO_Server_1710085680(lineType);
		}
	}

	private void RpcWriter___Observers_PlayVO_Client_1710085680(EVOLineType lineType)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EVoiceOver_002EEVOLineTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, lineType);
			((NetworkBehaviour)this).SendObserversRpc(34u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___PlayVO_Client_1710085680(EVOLineType lineType)
	{
		PlayVO(lineType);
	}

	private void RpcReader___Observers_PlayVO_Client_1710085680(PooledReader PooledReader0, Channel channel)
	{
		EVOLineType lineType = GeneratedReaders___Internal.Read___ScheduleOne_002EVoiceOver_002EEVOLineTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___PlayVO_Client_1710085680(lineType);
		}
	}

	private void RpcWriter___Target_ReceiveRelationshipData_4052192084(NetworkConnection conn, float relationship, bool unlocked)
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
			((Writer)writer).WriteSingle(relationship, (AutoPackType)0);
			((Writer)writer).WriteBoolean(unlocked);
			((NetworkBehaviour)this).SendTargetRpc(35u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	public void RpcLogic___ReceiveRelationshipData_4052192084(NetworkConnection conn, float relationship, bool unlocked)
	{
		RelationData.SetRelationship(relationship, network: false);
		Console.Log("Received relationship data for " + fullName + " Unlocked: " + unlocked);
		if (unlocked)
		{
			RelationData.Unlock(NPCRelationData.EUnlockType.DirectApproach, notify: false);
		}
	}

	private void RpcReader___Target_ReceiveRelationshipData_4052192084(PooledReader PooledReader0, Channel channel)
	{
		float relationship = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		bool unlocked = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveRelationshipData_4052192084(((NetworkBehaviour)this).LocalConnection, relationship, unlocked);
		}
	}

	private void RpcWriter___Server_SetIsBeingPickPocketed_1140765316(bool pickpocketed)
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
			((Writer)writer).WriteBoolean(pickpocketed);
			((NetworkBehaviour)this).SendServerRpc(36u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetIsBeingPickPocketed_1140765316(bool pickpocketed)
	{
		if (pickpocketed)
		{
			Behaviour.StationaryBehaviour.Enable_Networked();
		}
		else
		{
			Behaviour.StationaryBehaviour.Disable_Networked(null);
		}
	}

	private void RpcReader___Server_SetIsBeingPickPocketed_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool pickpocketed = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SetIsBeingPickPocketed_1140765316(pickpocketed);
		}
	}

	private void RpcWriter___Server_SendRelationship_431000436(float relationship)
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
			((Writer)writer).WriteSingle(relationship, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(37u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendRelationship_431000436(float relationship)
	{
		SetRelationship(relationship);
	}

	private void RpcReader___Server_SendRelationship_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		float relationship = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendRelationship_431000436(relationship);
		}
	}

	private void RpcWriter___Observers_SetRelationship_431000436(float relationship)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteSingle(relationship, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(38u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetRelationship_431000436(float relationship)
	{
		RelationData.SetRelationship(relationship, network: false);
	}

	private void RpcReader___Observers_SetRelationship_431000436(PooledReader PooledReader0, Channel channel)
	{
		float relationship = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetRelationship_431000436(relationship);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002ENPCs_002ENPC(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		if (UInt321 == 0)
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CHasUmbrella_003Ek__BackingField(syncVar____003CHasUmbrella_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			bool value = ((Reader)PooledReader0).ReadBoolean();
			this.sync___set_value__003CHasUmbrella_003Ek__BackingField(value, Boolean2);
			return true;
		}
		return false;
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002ENPC_Assembly_002DCSharp_002Edll()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		_networkedEquipper = ((Component)this).GetComponent<NetworkedEquipper>();
		intObj.onHovered.AddListener(new UnityAction(Hovered_Internal));
		intObj.onInteractStart.AddListener(new UnityAction(Interacted_Internal));
		CheckAndGetReferences();
		Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		if (!NPCManager.NPCRegistry.Contains(this))
		{
			NPCManager.NPCRegistry.Add(this);
		}
		Awareness.onNoticedGeneralCrime.AddListener((UnityAction<Player>)SetUnsettled_30s);
		Awareness.onNoticedPettyCrime.AddListener((UnityAction<Player>)SetUnsettled_30s);
		Health.onDie.AddListener(new UnityAction(OnDie));
		Health.onKnockedOut.AddListener(new UnityAction(OnKnockedOut));
		SkinnedMeshRenderer[] bodyMeshes = Avatar.BodyMeshes;
		foreach (SkinnedMeshRenderer val in bodyMeshes)
		{
			OutlineRenderers.Add(((Component)val).gameObject);
		}
		if ((Object)(object)VoiceOverEmitter == (Object)null)
		{
			VoiceOverEmitter = ((Component)Avatar.HeadBone).GetComponentInChildren<VOEmitter>();
		}
		RelationData.Init(this);
		if (RelationData.Unlocked)
		{
			Unlocked(NPCRelationData.EUnlockType.DirectApproach, notify: false);
		}
		else
		{
			NPCRelationData relationData = RelationData;
			relationData.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(Unlocked));
		}
		foreach (NPC connection in RelationData.Connections)
		{
			if (!((Object)(object)connection == (Object)null) && (Object)(object)connection == (Object)(object)this)
			{
				Console.LogWarning("NPC " + fullName + " has a connection to itself");
			}
		}
		headlightStartTime = 1700 + Mathf.RoundToInt(90f * Mathf.Clamp01((float)(fullName[0].GetHashCode() / 1000 % 10) / 10f));
		InitializeSaveable();
		defaultAggression = Aggression;
		_weatherTolerence = new WeatherConditions
		{
			Rainy = _rainTolerance
		};
		void Unlocked(NPCRelationData.EUnlockType unlockType, bool notify)
		{
			if (NPCUnlockedVariable != string.Empty)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(NPCUnlockedVariable, true.ToString());
			}
		}
	}
}
