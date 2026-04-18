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
using ScheduleOne.Audio;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Misc;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Product;
using ScheduleOne.StationFramework;
using ScheduleOne.Storage;
using ScheduleOne.Tiles;
using ScheduleOne.Tools;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Management;
using ScheduleOne.UI.Stations;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts;

public class MixingStation : GridItem, IUsable, IItemSlotOwner, ITransitEntity, IConfigurable
{
	public ItemSlot ProductSlot;

	public ItemSlot MixerSlot;

	public ItemSlot OutputSlot;

	[CompilerGenerated]
	[HideInInspector]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public NetworkObject _003CNPCUserObject_003Ek__BackingField;

	[CompilerGenerated]
	[HideInInspector]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public NetworkObject _003CPlayerUserObject_003Ek__BackingField;

	public bool RequiresIngredientInsertion = true;

	[CompilerGenerated]
	[SyncVar]
	public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

	[Header("Settings")]
	public int MixTimePerItem = 15;

	public int MaxMixQuantity = 10;

	[Header("Prefabs")]
	public GameObject JugPrefab;

	[Header("References")]
	public InteractableObject IntObj;

	public Transform CameraPosition;

	public Transform CameraPosition_CombineIngredients;

	public Transform CameraPosition_StartMachine;

	public StorageVisualizer InputVisuals;

	public StorageVisualizer OutputVisuals;

	public DigitalAlarm Clock;

	public ToggleableLight Light;

	public NewMixDiscoveryBox DiscoveryBox;

	public Transform ItemContainer;

	public Transform[] IngredientTransforms;

	public Fillable BowlFillable;

	public Clickable StartButton;

	public Transform JugAlignment;

	public Rigidbody Anchor;

	public BoxCollider TrashSpawnVolume;

	public Transform uiPoint;

	public Transform[] accessPoints;

	public ConfigurationReplicator configReplicator;

	[Header("Sounds")]
	public StartLoopStopAudio MachineSound;

	public AudioSourceController StartSound;

	public AudioSourceController StopSound;

	[Header("Mix Timing")]
	[Header("UI")]
	public MixingStationUIElement WorldspaceUIPrefab;

	public Sprite typeIcon;

	public UnityEvent onMixStart;

	public UnityEvent onMixDone;

	public UnityEvent onOutputCollected;

	public UnityEvent onStartButtonClicked;

	[Header("Animations")]
	[SerializeField]
	protected PlayAnimation _mixerAnimator;

	public SyncVar<NetworkObject> syncVar____003CNPCUserObject_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CPlayerUserObject_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMixingStationAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMixingStationAssembly_002DCSharp_002Edll_Excuted;

	public bool IsOpen { get; private set; }

	public MixOperation CurrentMixOperation { get; set; }

	public bool IsMixingDone
	{
		get
		{
			if (CurrentMixOperation != null)
			{
				return CurrentMixTime >= GetMixTimeForCurrentOperation();
			}
			return false;
		}
	}

	public int CurrentMixTime { get; protected set; }

	public List<ItemSlot> ItemSlots { get; set; } = new List<ItemSlot>();

	public NetworkObject NPCUserObject
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CNPCUserObject_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.sync___set_value__003CNPCUserObject_003Ek__BackingField(value, true);
		}
	}

	public NetworkObject PlayerUserObject
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CPlayerUserObject_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.sync___set_value__003CPlayerUserObject_003Ek__BackingField(value, true);
		}
	}

	public string Name => GetManagementName();

	public List<ItemSlot> InputSlots { get; set; } = new List<ItemSlot>();

	public List<ItemSlot> OutputSlots { get; set; } = new List<ItemSlot>();

	public Transform LinkOrigin => uiPoint;

	public Transform[] AccessPoints => accessPoints;

	public bool Selectable { get; } = true;

	public bool IsAcceptingItems { get; set; } = true;

	public EntityConfiguration Configuration => stationConfiguration;

	protected MixingStationConfiguration stationConfiguration { get; set; }

	public ConfigurationReplicator ConfigReplicator => configReplicator;

	public EConfigurableType ConfigurableType => EConfigurableType.MixingStation;

	public WorldspaceUIElement WorldspaceUI { get; set; }

	public NetworkObject CurrentPlayerConfigurer
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CCurrentPlayerConfigurer_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.sync___set_value__003CCurrentPlayerConfigurer_003Ek__BackingField(value, true);
		}
	}

	public Sprite TypeIcon => typeIcon;

	public Transform Transform => ((Component)this).transform;

	public Transform UIPoint => uiPoint;

	public bool CanBeSelected => true;

	public Vector3 DiscoveryBoxOffset { get; private set; }

	public Quaternion DiscoveryBoxRotation { get; private set; }

	public NetworkObject SyncAccessor__003CNPCUserObject_003Ek__BackingField
	{
		get
		{
			return NPCUserObject;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				NPCUserObject = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public NetworkObject SyncAccessor__003CPlayerUserObject_003Ek__BackingField
	{
		get
		{
			return PlayerUserObject;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				PlayerUserObject = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public NetworkObject SyncAccessor__003CCurrentPlayerConfigurer_003Ek__BackingField
	{
		get
		{
			return CurrentPlayerConfigurer;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				CurrentPlayerConfigurer = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetConfigurer(NetworkObject player)
	{
		RpcWriter___Server_SetConfigurer_3323014238(player);
		RpcLogic___SetConfigurer_3323014238(player);
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EObjectScripts_002EMixingStation_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		base.Start();
		if (!isGhost)
		{
			NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnMinPass);
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onTimeSkip = (Action<int>)Delegate.Combine(instance.onTimeSkip, new Action<int>(OnTimePass));
			if ((Object)(object)StartButton != (Object)null)
			{
				StartButton.onClickStart.AddListener((UnityAction<RaycastHit>)StartButtonClicked);
			}
		}
	}

	public override void InitializeGridItem(ItemInstance instance, Grid grid, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeGridItem(instance, grid, originCoordinate, rotation, GUID);
		base.ParentProperty.AddConfigurable(this);
		stationConfiguration = new MixingStationConfiguration(configReplicator, this, this);
		CreateWorldspaceUI();
	}

	public override string GetManagementName()
	{
		return Configuration.Name.Value;
	}

	public override string GetDefaultManagementName()
	{
		return ScheduleOne.Management.ConfigurableType.GetTypeName(ConfigurableType);
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (connection.IsHost)
		{
			return;
		}
		Debug.Log((object)"Server has spawned and is sending data to client for mix operation");
		((IItemSlotOwner)this).SendItemSlotDataToClient(connection);
		if (CurrentMixOperation != null)
		{
			Debug.Log((object)"Current mix operation is not null; sending to client");
			if (NetworkSingleton<ProductManager>.InstanceExists && !NetworkSingleton<ProductManager>.Instance.HasSentProductDataToConnection(connection))
			{
				Debug.Log((object)"Deferring sending mix operation until product data has been sent!");
				ProductManager instance = NetworkSingleton<ProductManager>.Instance;
				instance.onProductDataSentToConnection = (Action<NetworkConnection>)Delegate.Combine(instance.onProductDataSentToConnection, new Action<NetworkConnection>(SendMixOperation));
			}
			else
			{
				SendMixOperation(connection);
			}
		}
		SendConfigurationToClient(connection);
		void SendMixOperation(NetworkConnection conn)
		{
			SetMixOperation(conn, CurrentMixOperation, CurrentMixTime);
		}
	}

	public void SendConfigurationToClient(NetworkConnection conn)
	{
		if (!conn.IsHost)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(WaitForConfig());
		}
		IEnumerator WaitForConfig()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => Configuration != null));
			Configuration.ReplicateAllFields(conn);
		}
	}

	public override bool CanBeDestroyed(out string reason)
	{
		if (((IItemSlotOwner)this).GetQuantitySum() > 0)
		{
			reason = "Contains items";
			return false;
		}
		if (CurrentMixOperation != null && IsMixingDone)
		{
			reason = "Contains items";
			return false;
		}
		if (CurrentMixOperation != null)
		{
			reason = "Mixing in progress";
			return false;
		}
		if (((IUsable)this).IsInUse)
		{
			reason = "Currently in use";
			return false;
		}
		return base.CanBeDestroyed(out reason);
	}

	protected override void Destroy()
	{
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(OnMinPass);
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onTimeSkip = (Action<int>)Delegate.Remove(instance.onTimeSkip, new Action<int>(OnTimePass));
		if (Configuration != null)
		{
			Configuration.Destroy();
			DestroyWorldspaceUI();
			base.ParentProperty.RemoveConfigurable(this);
		}
		base.Destroy();
	}

	protected virtual void OnMinPass()
	{
		OnTimePass(1);
	}

	protected virtual void OnTimePass(int minutes)
	{
		if (CurrentMixOperation != null || OutputSlot.Quantity > 0)
		{
			int num = 0;
			if (CurrentMixOperation != null)
			{
				int currentMixTime = CurrentMixTime;
				CurrentMixTime += minutes;
				num = GetMixTimeForCurrentOperation();
				if (CurrentMixTime >= num && currentMixTime < num && InstanceFinder.IsServer)
				{
					NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Mixing_Operations_Completed", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Mixing_Operations_Completed") + 1f).ToString());
					MixingDone_Networked();
				}
			}
			if ((Object)(object)Clock != (Object)null)
			{
				Clock.SetScreenLit(lit: true);
				Clock.DisplayMinutes(Mathf.Max(num - CurrentMixTime, 0));
			}
			if ((Object)(object)Light != (Object)null)
			{
				if (IsMixingDone)
				{
					Light.isOn = NetworkSingleton<TimeManager>.Instance.CurrentTime % 2 == 0;
				}
				else
				{
					Light.isOn = true;
				}
			}
		}
		else
		{
			if ((Object)(object)Clock != (Object)null)
			{
				Clock.SetScreenLit(lit: false);
				Clock.DisplayText(string.Empty);
			}
			if ((Object)(object)Light != (Object)null && IsMixingDone)
			{
				Light.isOn = false;
			}
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendMixingOperation(MixOperation operation, int mixTime)
	{
		RpcWriter___Server_SendMixingOperation_2669582547(operation, mixTime);
		RpcLogic___SendMixingOperation_2669582547(operation, mixTime);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public virtual void SetMixOperation(NetworkConnection conn, MixOperation operation, int mixTime)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetMixOperation_1073078804(conn, operation, mixTime);
			RpcLogic___SetMixOperation_1073078804(conn, operation, mixTime);
		}
		else
		{
			RpcWriter___Target_SetMixOperation_1073078804(conn, operation, mixTime);
		}
	}

	protected virtual void SetMixerToLowered()
	{
		_mixerAnimator.Play("Stand mixer lower");
	}

	public virtual void MixingStart()
	{
		StartSound.Play();
		MachineSound.StartAudio();
		if (onMixStart != null)
		{
			onMixStart.Invoke();
		}
	}

	[ObserversRpc]
	public void MixingDone_Networked()
	{
		RpcWriter___Observers_MixingDone_Networked_2166136261();
	}

	public virtual void MixingDone()
	{
		MachineSound.StopAudio();
		if (!Singleton<LoadManager>.Instance.IsLoading)
		{
			StopSound.Play();
		}
		TryCreateOutputItems();
		if (onMixDone != null)
		{
			onMixDone.Invoke();
		}
	}

	public bool DoesOutputHaveSpace(StationRecipe recipe)
	{
		StorableItemInstance productInstance = recipe.GetProductInstance(GetIngredients());
		return OutputSlot.GetCapacityForItem(productInstance) >= 1;
	}

	private bool IsCurrentMixingOperationComplete()
	{
		return CurrentMixTime >= GetMixTimeForCurrentOperation();
	}

	public List<ItemInstance> GetIngredients()
	{
		List<ItemInstance> list = new List<ItemInstance>();
		if (ProductSlot.ItemInstance != null)
		{
			list.Add(ProductSlot.ItemInstance);
		}
		if (MixerSlot.ItemInstance != null)
		{
			list.Add(MixerSlot.ItemInstance);
		}
		return list;
	}

	public int GetMixQuantity()
	{
		if ((Object)(object)GetProduct() == (Object)null || (Object)(object)GetMixer() == (Object)null)
		{
			return 0;
		}
		return Mathf.Min(Mathf.Min(ProductSlot.Quantity, MixerSlot.Quantity), MaxMixQuantity);
	}

	public bool CanStartMix()
	{
		if (GetMixQuantity() > 0)
		{
			return OutputSlot.Quantity == 0;
		}
		return false;
	}

	public ProductDefinition GetProduct()
	{
		if (ProductSlot.ItemInstance != null)
		{
			return ProductSlot.ItemInstance.Definition as ProductDefinition;
		}
		return null;
	}

	public PropertyItemDefinition GetMixer()
	{
		if (MixerSlot.ItemInstance != null)
		{
			PropertyItemDefinition propertyItemDefinition = MixerSlot.ItemInstance.Definition as PropertyItemDefinition;
			if ((Object)(object)propertyItemDefinition != (Object)null && NetworkSingleton<ProductManager>.Instance.ValidMixIngredients.Contains(propertyItemDefinition))
			{
				return propertyItemDefinition;
			}
		}
		return null;
	}

	public int GetMixTimeForCurrentOperation()
	{
		if (CurrentMixOperation == null)
		{
			return 0;
		}
		return MixTimePerItem * CurrentMixOperation.Quantity;
	}

	[ServerRpc(RequireOwnership = false)]
	public void TryCreateOutputItems()
	{
		RpcWriter___Server_TryCreateOutputItems_2166136261();
	}

	public void SetStartButtonClickable(bool clickable)
	{
		StartButton.ClickableEnabled = clickable;
	}

	private void OutputChanged()
	{
		if (OutputSlot.Quantity == 0)
		{
			if (onOutputCollected != null)
			{
				onOutputCollected.Invoke();
			}
			if (InstanceFinder.IsServer)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Mixing_Operations_Collected", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Mixing_Operations_Collected") + 1f).ToString());
			}
		}
	}

	private void StartButtonClicked(RaycastHit hit)
	{
		SetStartButtonClickable(clickable: false);
		if (onStartButtonClicked != null)
		{
			onStartButtonClicked.Invoke();
		}
	}

	public void Open()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		IsOpen = true;
		if (CurrentMixOperation != null && IsMixingDone)
		{
			TryCreateOutputItems();
		}
		SetPlayerUser(((NetworkBehaviour)Player.Local).NetworkObject);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraPosition.position, CameraPosition.rotation, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		Singleton<MixingStationCanvas>.Instance.Open(this);
		Singleton<CompassManager>.Instance.SetVisible(visible: false);
	}

	public void Close()
	{
		IsOpen = false;
		SetPlayerUser(null);
		if ((Object)(object)DiscoveryBox != (Object)null)
		{
			((Component)DiscoveryBox).transform.SetParent(((Component)CameraPosition).transform);
			((Component)DiscoveryBox).gameObject.SetActive(false);
		}
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.2f);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.2f);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
			PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
			Singleton<CompassManager>.Instance.SetVisible(visible: true);
		}
	}

	public void Hovered()
	{
		if (((IUsable)this).IsInUse || Singleton<ManagementClipboard>.Instance.IsEquipped)
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			return;
		}
		IntObj.SetMessage("Use " + ((BaseItemInstance)base.ItemInstance).Name);
		IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
	}

	public void Interacted()
	{
		if (!((IUsable)this).IsInUse && !Singleton<ManagementClipboard>.Instance.IsEquipped)
		{
			Open();
		}
	}

	public WorldspaceUIElement CreateWorldspaceUI()
	{
		if ((Object)(object)WorldspaceUI != (Object)null)
		{
			Console.LogWarning(((Object)((Component)this).gameObject).name + " already has a worldspace UI element!");
		}
		if ((Object)(object)base.ParentProperty == (Object)null)
		{
			Console.LogError(((object)base.ParentProperty)?.ToString() + " is not a child of a property!");
			return null;
		}
		MixingStationUIElement component = ((Component)Object.Instantiate<MixingStationUIElement>(WorldspaceUIPrefab, (Transform)(object)base.ParentProperty.WorldspaceUIContainer)).GetComponent<MixingStationUIElement>();
		component.Initialize(this);
		WorldspaceUI = component;
		return component;
	}

	public void DestroyWorldspaceUI()
	{
		if ((Object)(object)WorldspaceUI != (Object)null)
		{
			WorldspaceUI.Destroy();
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetStoredInstance(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		RpcWriter___Server_SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
		RpcLogic___SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetStoredInstance_Internal(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
			RpcLogic___SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
		}
		else
		{
			RpcWriter___Target_SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetItemSlotQuantity(int itemSlotIndex, int quantity)
	{
		RpcWriter___Server_SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
		RpcLogic___SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetItemSlotQuantity_Internal(int itemSlotIndex, int quantity)
	{
		RpcWriter___Observers_SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
		RpcLogic___SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetSlotLocked(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		RpcWriter___Server_SetSlotLocked_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		RpcLogic___SetSlotLocked_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	private void SetSlotLocked_Internal(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
			RpcLogic___SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}
		else
		{
			RpcWriter___Target_SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetSlotFilter(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		RpcWriter___Server_SetSlotFilter_527532783(conn, itemSlotIndex, filter);
		RpcLogic___SetSlotFilter_527532783(conn, itemSlotIndex, filter);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetSlotFilter_Internal(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetSlotFilter_Internal_527532783(conn, itemSlotIndex, filter);
			RpcLogic___SetSlotFilter_Internal_527532783(conn, itemSlotIndex, filter);
		}
		else
		{
			RpcWriter___Target_SetSlotFilter_Internal_527532783(conn, itemSlotIndex, filter);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetPlayerUser(NetworkObject playerObject)
	{
		RpcWriter___Server_SetPlayerUser_3323014238(playerObject);
		RpcLogic___SetPlayerUser_3323014238(playerObject);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetNPCUser(NetworkObject npcObject)
	{
		RpcWriter___Server_SetNPCUser_3323014238(npcObject);
		RpcLogic___SetNPCUser_3323014238(npcObject);
	}

	public override BuildableItemData GetBaseData()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new MixingStationData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, _originCoordinate, _rotation, new ItemSet(new List<ItemSlot> { ProductSlot }), new ItemSet(new List<ItemSlot> { MixerSlot }), new ItemSet(new List<ItemSlot> { OutputSlot }), CurrentMixOperation, CurrentMixTime);
	}

	public override DynamicSaveData GetSaveData()
	{
		DynamicSaveData saveData = base.GetSaveData();
		if (Configuration.ShouldSave())
		{
			saveData.AddData("Configuration", Configuration.GetSaveString());
		}
		return saveData;
	}

	public override void NetworkInitialize___Early()
	{
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
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMixingStationAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMixingStationAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 2u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, CurrentPlayerConfigurer);
			syncVar____003CPlayerUserObject_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 1u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, PlayerUserObject);
			syncVar____003CNPCUserObject_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 0u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, NPCUserObject);
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_SetConfigurer_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(6u, new ServerRpcDelegate(RpcReader___Server_SendMixingOperation_2669582547));
			((NetworkBehaviour)this).RegisterObserversRpc(7u, new ClientRpcDelegate(RpcReader___Observers_SetMixOperation_1073078804));
			((NetworkBehaviour)this).RegisterTargetRpc(8u, new ClientRpcDelegate(RpcReader___Target_SetMixOperation_1073078804));
			((NetworkBehaviour)this).RegisterObserversRpc(9u, new ClientRpcDelegate(RpcReader___Observers_MixingDone_Networked_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(10u, new ServerRpcDelegate(RpcReader___Server_TryCreateOutputItems_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(11u, new ServerRpcDelegate(RpcReader___Server_SetStoredInstance_2652194801));
			((NetworkBehaviour)this).RegisterObserversRpc(12u, new ClientRpcDelegate(RpcReader___Observers_SetStoredInstance_Internal_2652194801));
			((NetworkBehaviour)this).RegisterTargetRpc(13u, new ClientRpcDelegate(RpcReader___Target_SetStoredInstance_Internal_2652194801));
			((NetworkBehaviour)this).RegisterServerRpc(14u, new ServerRpcDelegate(RpcReader___Server_SetItemSlotQuantity_1692629761));
			((NetworkBehaviour)this).RegisterObserversRpc(15u, new ClientRpcDelegate(RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761));
			((NetworkBehaviour)this).RegisterServerRpc(16u, new ServerRpcDelegate(RpcReader___Server_SetSlotLocked_3170825843));
			((NetworkBehaviour)this).RegisterTargetRpc(17u, new ClientRpcDelegate(RpcReader___Target_SetSlotLocked_Internal_3170825843));
			((NetworkBehaviour)this).RegisterObserversRpc(18u, new ClientRpcDelegate(RpcReader___Observers_SetSlotLocked_Internal_3170825843));
			((NetworkBehaviour)this).RegisterServerRpc(19u, new ServerRpcDelegate(RpcReader___Server_SetSlotFilter_527532783));
			((NetworkBehaviour)this).RegisterObserversRpc(20u, new ClientRpcDelegate(RpcReader___Observers_SetSlotFilter_Internal_527532783));
			((NetworkBehaviour)this).RegisterTargetRpc(21u, new ClientRpcDelegate(RpcReader___Target_SetSlotFilter_Internal_527532783));
			((NetworkBehaviour)this).RegisterServerRpc(22u, new ServerRpcDelegate(RpcReader___Server_SetPlayerUser_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(23u, new ServerRpcDelegate(RpcReader___Server_SetNPCUser_3323014238));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EObjectScripts_002EMixingStation));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMixingStationAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMixingStationAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CPlayerUserObject_003Ek__BackingField).SetRegistered();
			((SyncBase)syncVar____003CNPCUserObject_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetConfigurer_3323014238(NetworkObject player)
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
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetConfigurer_3323014238(NetworkObject player)
	{
		CurrentPlayerConfigurer = player;
	}

	private void RpcReader___Server_SetConfigurer_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetConfigurer_3323014238(player);
		}
	}

	private void RpcWriter___Server_SendMixingOperation_2669582547(MixOperation operation, int mixTime)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerated((Writer)(object)writer, operation);
			((Writer)writer).WriteInt32(mixTime, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(6u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendMixingOperation_2669582547(MixOperation operation, int mixTime)
	{
		SetMixOperation(null, operation, mixTime);
	}

	private void RpcReader___Server_SendMixingOperation_2669582547(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		MixOperation operation = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		int mixTime = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendMixingOperation_2669582547(operation, mixTime);
		}
	}

	private void RpcWriter___Observers_SetMixOperation_1073078804(NetworkConnection conn, MixOperation operation, int mixTime)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerated((Writer)(object)writer, operation);
			((Writer)writer).WriteInt32(mixTime, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(7u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public virtual void RpcLogic___SetMixOperation_1073078804(NetworkConnection conn, MixOperation operation, int mixTime)
	{
		if (operation != null && string.IsNullOrEmpty(operation.ProductID))
		{
			operation = null;
		}
		MixOperation currentMixOperation = CurrentMixOperation;
		CurrentMixOperation = operation;
		CurrentMixTime = mixTime;
		if (operation != null)
		{
			if (IsCurrentMixingOperationComplete())
			{
				SetMixerToLowered();
			}
			else
			{
				MixingStart();
			}
		}
		else if (currentMixOperation != null)
		{
			UnityEvent obj = onMixDone;
			if (obj != null)
			{
				obj.Invoke();
			}
		}
	}

	private void RpcReader___Observers_SetMixOperation_1073078804(PooledReader PooledReader0, Channel channel)
	{
		MixOperation operation = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		int mixTime = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetMixOperation_1073078804(null, operation, mixTime);
		}
	}

	private void RpcWriter___Target_SetMixOperation_1073078804(NetworkConnection conn, MixOperation operation, int mixTime)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerated((Writer)(object)writer, operation);
			((Writer)writer).WriteInt32(mixTime, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(8u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetMixOperation_1073078804(PooledReader PooledReader0, Channel channel)
	{
		MixOperation operation = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		int mixTime = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetMixOperation_1073078804(((NetworkBehaviour)this).LocalConnection, operation, mixTime);
		}
	}

	private void RpcWriter___Observers_MixingDone_Networked_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(9u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___MixingDone_Networked_2166136261()
	{
		MixingDone();
	}

	private void RpcReader___Observers_MixingDone_Networked_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___MixingDone_Networked_2166136261();
		}
	}

	private void RpcWriter___Server_TryCreateOutputItems_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(10u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___TryCreateOutputItems_2166136261()
	{
		if (CurrentMixOperation != null && CurrentMixOperation.IsOutputKnown(out var knownProduct))
		{
			QualityItemInstance qualityItemInstance = knownProduct.GetDefaultInstance(CurrentMixOperation.Quantity) as QualityItemInstance;
			qualityItemInstance.SetQuality(CurrentMixOperation.ProductQuality);
			OutputSlot.AddItem(qualityItemInstance);
			if ((Object)(object)NetworkSingleton<ProductManager>.Instance.GetRecipe(CurrentMixOperation.ProductID, CurrentMixOperation.IngredientID) == (Object)null)
			{
				NetworkSingleton<ProductManager>.Instance.SendMixRecipe(CurrentMixOperation.ProductID, CurrentMixOperation.IngredientID, ((BaseItemInstance)qualityItemInstance).ID);
			}
			SetMixOperation(null, null, 0);
		}
	}

	private void RpcReader___Server_TryCreateOutputItems_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___TryCreateOutputItems_2166136261();
		}
	}

	private void RpcWriter___Server_SetStoredInstance_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)(object)writer).WriteItemInstance(instance);
			((NetworkBehaviour)this).SendServerRpc(11u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetStoredInstance_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		if (conn == (NetworkConnection)null || conn.ClientId == -1)
		{
			SetStoredInstance_Internal(null, itemSlotIndex, instance);
		}
		else
		{
			SetStoredInstance_Internal(conn, itemSlotIndex, instance);
		}
	}

	private void RpcReader___Server_SetStoredInstance_2652194801(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = ((Reader)PooledReader0).ReadNetworkConnection();
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetStoredInstance_2652194801(conn2, itemSlotIndex, instance);
		}
	}

	private void RpcWriter___Observers_SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)(object)writer).WriteItemInstance(instance);
			((NetworkBehaviour)this).SendObserversRpc(12u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		if (instance != null)
		{
			ItemSlots[itemSlotIndex].SetStoredItem(instance, _internal: true);
		}
		else
		{
			ItemSlots[itemSlotIndex].ClearStoredInstance(_internal: true);
		}
	}

	private void RpcReader___Observers_SetStoredInstance_Internal_2652194801(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetStoredInstance_Internal_2652194801(null, itemSlotIndex, instance);
		}
	}

	private void RpcWriter___Target_SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)(object)writer).WriteItemInstance(instance);
			((NetworkBehaviour)this).SendTargetRpc(13u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetStoredInstance_Internal_2652194801(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetStoredInstance_Internal_2652194801(((NetworkBehaviour)this).LocalConnection, itemSlotIndex, instance);
		}
	}

	private void RpcWriter___Server_SetItemSlotQuantity_1692629761(int itemSlotIndex, int quantity)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteInt32(quantity, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(14u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetItemSlotQuantity_1692629761(int itemSlotIndex, int quantity)
	{
		SetItemSlotQuantity_Internal(itemSlotIndex, quantity);
	}

	private void RpcReader___Server_SetItemSlotQuantity_1692629761(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int quantity = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
		}
	}

	private void RpcWriter___Observers_SetItemSlotQuantity_Internal_1692629761(int itemSlotIndex, int quantity)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteInt32(quantity, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(15u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetItemSlotQuantity_Internal_1692629761(int itemSlotIndex, int quantity)
	{
		ItemSlots[itemSlotIndex].SetQuantity(quantity, _internal: true);
	}

	private void RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int quantity = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
		}
	}

	private void RpcWriter___Server_SetSlotLocked_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteBoolean(locked);
			((Writer)writer).WriteNetworkObject(lockOwner);
			((Writer)writer).WriteString(lockReason);
			((NetworkBehaviour)this).SendServerRpc(16u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetSlotLocked_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		if (conn == (NetworkConnection)null || conn.ClientId == -1)
		{
			SetSlotLocked_Internal(null, itemSlotIndex, locked, lockOwner, lockReason);
		}
		else
		{
			SetSlotLocked_Internal(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcReader___Server_SetSlotLocked_3170825843(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = ((Reader)PooledReader0).ReadNetworkConnection();
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool locked = ((Reader)PooledReader0).ReadBoolean();
		NetworkObject lockOwner = ((Reader)PooledReader0).ReadNetworkObject();
		string lockReason = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetSlotLocked_3170825843(conn2, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcWriter___Target_SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteBoolean(locked);
			((Writer)writer).WriteNetworkObject(lockOwner);
			((Writer)writer).WriteString(lockReason);
			((NetworkBehaviour)this).SendTargetRpc(17u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		if (locked)
		{
			ItemSlots[itemSlotIndex].ApplyLock(lockOwner, lockReason, _internal: true);
		}
		else
		{
			ItemSlots[itemSlotIndex].RemoveLock(_internal: true);
		}
	}

	private void RpcReader___Target_SetSlotLocked_Internal_3170825843(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool locked = ((Reader)PooledReader0).ReadBoolean();
		NetworkObject lockOwner = ((Reader)PooledReader0).ReadNetworkObject();
		string lockReason = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetSlotLocked_Internal_3170825843(((NetworkBehaviour)this).LocalConnection, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcWriter___Observers_SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteBoolean(locked);
			((Writer)writer).WriteNetworkObject(lockOwner);
			((Writer)writer).WriteString(lockReason);
			((NetworkBehaviour)this).SendObserversRpc(18u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_SetSlotLocked_Internal_3170825843(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool locked = ((Reader)PooledReader0).ReadBoolean();
		NetworkObject lockOwner = ((Reader)PooledReader0).ReadNetworkObject();
		string lockReason = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetSlotLocked_Internal_3170825843(null, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcWriter___Server_SetSlotFilter_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerated((Writer)(object)writer, filter);
			((NetworkBehaviour)this).SendServerRpc(19u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetSlotFilter_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		if (conn == (NetworkConnection)null || conn.ClientId == -1)
		{
			SetSlotFilter_Internal(null, itemSlotIndex, filter);
		}
		else
		{
			SetSlotFilter_Internal(conn, itemSlotIndex, filter);
		}
	}

	private void RpcReader___Server_SetSlotFilter_527532783(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = ((Reader)PooledReader0).ReadNetworkConnection();
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		SlotFilter filter = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetSlotFilter_527532783(conn2, itemSlotIndex, filter);
		}
	}

	private void RpcWriter___Observers_SetSlotFilter_Internal_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerated((Writer)(object)writer, filter);
			((NetworkBehaviour)this).SendObserversRpc(20u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetSlotFilter_Internal_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		ItemSlots[itemSlotIndex].SetPlayerFilter(filter, _internal: true);
	}

	private void RpcReader___Observers_SetSlotFilter_Internal_527532783(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		SlotFilter filter = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetSlotFilter_Internal_527532783(null, itemSlotIndex, filter);
		}
	}

	private void RpcWriter___Target_SetSlotFilter_Internal_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerated((Writer)(object)writer, filter);
			((NetworkBehaviour)this).SendTargetRpc(21u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetSlotFilter_Internal_527532783(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		SlotFilter filter = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetSlotFilter_Internal_527532783(((NetworkBehaviour)this).LocalConnection, itemSlotIndex, filter);
		}
	}

	private void RpcWriter___Server_SetPlayerUser_3323014238(NetworkObject playerObject)
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
			((Writer)writer).WriteNetworkObject(playerObject);
			((NetworkBehaviour)this).SendServerRpc(22u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetPlayerUser_3323014238(NetworkObject playerObject)
	{
		if ((Object)(object)SyncAccessor__003CPlayerUserObject_003Ek__BackingField != (Object)null && SyncAccessor__003CPlayerUserObject_003Ek__BackingField.Owner.IsLocalClient && (Object)(object)playerObject != (Object)null && !playerObject.Owner.IsLocalClient)
		{
			Singleton<GameInput>.Instance.ExitAll();
		}
		PlayerUserObject = playerObject;
	}

	private void RpcReader___Server_SetPlayerUser_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject playerObject = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetPlayerUser_3323014238(playerObject);
		}
	}

	private void RpcWriter___Server_SetNPCUser_3323014238(NetworkObject npcObject)
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
			((Writer)writer).WriteNetworkObject(npcObject);
			((NetworkBehaviour)this).SendServerRpc(23u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetNPCUser_3323014238(NetworkObject npcObject)
	{
		NPCUserObject = npcObject;
	}

	private void RpcReader___Server_SetNPCUser_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject npcObject = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetNPCUser_3323014238(npcObject);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EObjectScripts_002EMixingStation(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		switch (UInt321)
		{
		case 2u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CCurrentPlayerConfigurer_003Ek__BackingField(syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			NetworkObject value3 = ((Reader)PooledReader0).ReadNetworkObject();
			this.sync___set_value__003CCurrentPlayerConfigurer_003Ek__BackingField(value3, Boolean2);
			return true;
		}
		case 1u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CPlayerUserObject_003Ek__BackingField(syncVar____003CPlayerUserObject_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			NetworkObject value2 = ((Reader)PooledReader0).ReadNetworkObject();
			this.sync___set_value__003CPlayerUserObject_003Ek__BackingField(value2, Boolean2);
			return true;
		}
		case 0u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CNPCUserObject_003Ek__BackingField(syncVar____003CNPCUserObject_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			NetworkObject value = ((Reader)PooledReader0).ReadNetworkObject();
			this.sync___set_value__003CNPCUserObject_003Ek__BackingField(value, Boolean2);
			return true;
		}
		default:
			return false;
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EObjectScripts_002EMixingStation_Assembly_002DCSharp_002Edll()
	{
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		if (!isGhost)
		{
			ProductSlot = new ItemSlot(canPlayerSetFilter: true);
			ProductSlot.AddFilter(new ItemFilter_UnpackagedProduct());
			ProductSlot.SetSlotOwner(this);
			InputVisuals.AddSlot(ProductSlot);
			ItemSlot productSlot = ProductSlot;
			productSlot.onItemDataChanged = (Action)Delegate.Combine(productSlot.onItemDataChanged, (Action)delegate
			{
				base.HasChanged = true;
			});
			MixerSlot = new ItemSlot(canPlayerSetFilter: true);
			MixerSlot.AddFilter(new ItemFilter_MixingIngredient());
			MixerSlot.SetSlotOwner(this);
			InputVisuals.AddSlot(MixerSlot);
			ItemSlot mixerSlot = MixerSlot;
			mixerSlot.onItemDataChanged = (Action)Delegate.Combine(mixerSlot.onItemDataChanged, (Action)delegate
			{
				base.HasChanged = true;
			});
			OutputSlot.SetIsAddLocked(locked: true);
			OutputSlot.SetSlotOwner(this);
			OutputVisuals.AddSlot(OutputSlot);
			ItemSlot outputSlot = OutputSlot;
			outputSlot.onItemDataChanged = (Action)Delegate.Combine(outputSlot.onItemDataChanged, (Action)delegate
			{
				base.HasChanged = true;
			});
			ItemSlot outputSlot2 = OutputSlot;
			outputSlot2.onItemDataChanged = (Action)Delegate.Combine(outputSlot2.onItemDataChanged, new Action(OutputChanged));
			DiscoveryBoxOffset = ((Component)DiscoveryBox).transform.localPosition;
			DiscoveryBoxRotation = ((Component)DiscoveryBox).transform.localRotation;
			InputSlots.AddRange(new List<ItemSlot> { ProductSlot, MixerSlot });
			OutputSlots.Add(OutputSlot);
			new ItemSlotSiblingSet(InputSlots);
		}
	}
}
