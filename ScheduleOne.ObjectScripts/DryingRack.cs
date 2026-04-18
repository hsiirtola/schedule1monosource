using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
using ScheduleOne.Tiles;
using ScheduleOne.Tools;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Management;
using ScheduleOne.UI.Stations;
using UnityEngine;

namespace ScheduleOne.ObjectScripts;

public class DryingRack : GridItem, IUsable, IItemSlotOwner, ITransitEntity, IConfigurable
{
	public const int DRY_MINS_PER_TIER = 720;

	public const float MAX_DRY_MULTIPLIER = 1.5f;

	public const float WARMTH_MIN_THRESHOLD = 20f;

	public const float WARMTH_MAX_THRESHOLD = 40f;

	[Header("Settings")]
	public int ItemCapacity = 20;

	[Header("References")]
	public Transform[] CameraPositions;

	public InteractableObject IntObj;

	public Transform uiPoint;

	public Transform[] accessPoints;

	public StorageVisualizer InputVisuals;

	public StorageVisualizer OutputVisuals;

	public StorageVisualizer HangingVisuals;

	public Transform[] HangAlignments;

	public ConfigurationReplicator configReplicator;

	[SerializeField]
	private Transform _qualityEffectContianer;

	[Header("UI")]
	public DryingRackUIElement WorldspaceUIPrefab;

	public Sprite typeIcon;

	[Header("Fonts")]
	[SerializeField]
	private ColorFont _qualityColourFont;

	[CompilerGenerated]
	[HideInInspector]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public NetworkObject _003CNPCUserObject_003Ek__BackingField;

	[CompilerGenerated]
	[HideInInspector]
	[SyncVar(/*Could not decode attribute arguments.*/)]
	public NetworkObject _003CPlayerUserObject_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

	public Action<DryingOperation> onOperationStart;

	public Action<DryingOperation> onOperationComplete;

	public Action onOperationsChanged;

	private ItemSlot[] hangSlots;

	private ParticleSystem[] _qualityParticleEffect;

	private List<int> requestIDs = new List<int>();

	public SyncVar<NetworkObject> syncVar____003CNPCUserObject_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CPlayerUserObject_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EDryingRackAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EDryingRackAssembly_002DCSharp_002Edll_Excuted;

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

	public List<ItemSlot> ItemSlots { get; set; } = new List<ItemSlot>();

	public string Name => GetManagementName();

	public List<ItemSlot> InputSlots { get; set; } = new List<ItemSlot>();

	public List<ItemSlot> OutputSlots { get; set; } = new List<ItemSlot>();

	public Transform LinkOrigin => uiPoint;

	public Transform[] AccessPoints => accessPoints;

	public bool Selectable { get; } = true;

	public bool IsAcceptingItems { get; set; } = true;

	public EntityConfiguration Configuration => stationConfiguration;

	protected DryingRackConfiguration stationConfiguration { get; set; }

	public ConfigurationReplicator ConfigReplicator => configReplicator;

	public EConfigurableType ConfigurableType => EConfigurableType.DryingRack;

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

	public ItemSlot InputSlot { get; private set; }

	public ItemSlot OutputSlot { get; private set; }

	public bool IsOpen { get; private set; }

	public List<DryingOperation> DryingOperations { get; set; } = new List<DryingOperation>();

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
		Awake_UserLogic_ScheduleOne_002EObjectScripts_002EDryingRack_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void InitializeGridItem(ItemInstance instance, Grid grid, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeGridItem(instance, grid, originCoordinate, rotation, GUID);
		base.ParentProperty.AddConfigurable(this);
		stationConfiguration = new DryingRackConfiguration(configReplicator, this, this);
		CreateWorldspaceUI();
		GameInput.RegisterExitListener(Exit, 4);
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnMinPass);
		TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
		instance2.onTimeSkip = (Action<int>)Delegate.Combine(instance2.onTimeSkip, new Action<int>(OnTimePass));
	}

	public override string GetManagementName()
	{
		return Configuration.Name.Value;
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (connection.IsHost)
		{
			return;
		}
		((IItemSlotOwner)this).SendItemSlotDataToClient(connection);
		if (DryingOperations != null)
		{
			foreach (DryingOperation dryingOperation in DryingOperations)
			{
				PleaseReceiveOp(connection, dryingOperation);
			}
		}
		SendConfigurationToClient(connection);
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

	private void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			Close();
		}
	}

	public override bool CanBeDestroyed(out string reason)
	{
		if (((IUsable)this).IsInUse)
		{
			reason = "In use";
			return false;
		}
		if (((IItemSlotOwner)this).GetQuantitySum() > 0 || DryingOperations.Count > 0)
		{
			reason = "Contains items";
			return false;
		}
		return base.CanBeDestroyed(out reason);
	}

	protected override void Destroy()
	{
		GameInput.DeregisterExitListener(Exit);
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

	private void OnMinPass()
	{
		OnTimePass(1);
	}

	private void OnTimePass(int minutes)
	{
		DryingOperation[] array = DryingOperations.ToArray();
		float num = (float)minutes * GetDryMultiplier();
		for (int i = 0; i < array.Length; i++)
		{
			DryingOperation dryingOperation = array[i];
			dryingOperation.Time += num;
			if (!(dryingOperation.Time >= 720f))
			{
				continue;
			}
			if (dryingOperation.StartQuality >= EQuality.Premium)
			{
				if (InstanceFinder.IsServer && GetOutputCapacityForOperation(dryingOperation, EQuality.Heavenly) >= dryingOperation.Quantity)
				{
					TryEndOperation(i, allowSplitting: false, EQuality.Heavenly, Random.Range(int.MinValue, int.MaxValue));
				}
			}
			else
			{
				dryingOperation.IncreaseQuality();
				RefreshDryingEffects();
			}
		}
	}

	public bool CanStartOperation()
	{
		if (GetTotalDryingItems() >= ItemCapacity)
		{
			return false;
		}
		if (InputSlot.Quantity == 0)
		{
			return false;
		}
		if (InputSlot.IsLocked || InputSlot.IsRemovalLocked)
		{
			return false;
		}
		return true;
	}

	public void StartOperation()
	{
		int num = Mathf.Min(InputSlot.Quantity, ItemCapacity - GetTotalDryingItems());
		EQuality quality = (InputSlot.ItemInstance as QualityItemInstance).Quality;
		DryingOperation op = new DryingOperation(((BaseItemInstance)InputSlot.ItemInstance).ID, num, quality, 0f);
		SendOperation(op);
		InputSlot.ChangeQuantity(-num);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void TryEndOperation(int operationIndex, bool allowSplitting, EQuality quality, int requestID)
	{
		RpcWriter___Server_TryEndOperation_4146970406(operationIndex, allowSplitting, quality, requestID);
		RpcLogic___TryEndOperation_4146970406(operationIndex, allowSplitting, quality, requestID);
	}

	public List<DryingOperation> GetOperationsAtTargetQuality()
	{
		EQuality targetQuality = (Configuration as DryingRackConfiguration).TargetQuality.Value;
		return DryingOperations.Where((DryingOperation x) => x.StartQuality >= targetQuality).ToList();
	}

	public int GetOutputCapacityForOperation(DryingOperation operation, EQuality quality)
	{
		QualityItemInstance qualityItemInstance = Registry.GetItem(operation.ItemID).GetDefaultInstance() as QualityItemInstance;
		qualityItemInstance.SetQuality(quality);
		return OutputSlot.GetCapacityForItem(qualityItemInstance);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendOperation(DryingOperation op)
	{
		RpcWriter___Server_SendOperation_1307702229(op);
	}

	[TargetRpc]
	[ObserversRpc]
	private void PleaseReceiveOp(NetworkConnection conn, DryingOperation op)
	{
		if (conn == null)
		{
			RpcWriter___Observers_PleaseReceiveOp_1575047616(conn, op);
		}
		else
		{
			RpcWriter___Target_PleaseReceiveOp_1575047616(conn, op);
		}
	}

	[ObserversRpc(RunLocally = true, ExcludeServer = true)]
	private void RemoveOperation(int opIndex)
	{
		RpcWriter___Observers_RemoveOperation_3316948804(opIndex);
		RpcLogic___RemoveOperation_3316948804(opIndex);
	}

	[ObserversRpc]
	private void SetOperationQuantity(int opIndex, int quantity)
	{
		RpcWriter___Observers_SetOperationQuantity_1692629761(opIndex, quantity);
	}

	public int GetTotalDryingItems()
	{
		return DryingOperations.Sum((DryingOperation x) => x.Quantity);
	}

	public void RefreshHangingVisuals()
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < hangSlots.Length; i++)
		{
			if (DryingOperations.Count > i)
			{
				QualityItemInstance qualityItemInstance = DryingOperations[i].GetQualityItemInstance();
				hangSlots[i].SetStoredItem(qualityItemInstance);
			}
			else
			{
				hangSlots[i].ClearStoredInstance();
			}
		}
		RefreshDryingEffects();
		HangingVisuals.RefreshVisuals();
		StoredItem[] array = (from x in ((Component)HangingVisuals.ItemContainer).GetComponentsInChildren<StoredItem>()
			where !x.Destroyed
			select x).ToArray();
		for (int num = 0; num < array.Length && num < HangAlignments.Length; num++)
		{
			Transform val = ((Component)array[num]).GetComponentsInChildren<Transform>().FirstOrDefault((Transform x) => ((Object)x).name == "HangingAlignment");
			if ((Object)(object)val == (Object)null)
			{
				Console.LogError("Missing alignment transform on stored item: " + ((Object)array[num]).name);
				continue;
			}
			Transform obj = HangAlignments[num];
			Quaternion val2 = obj.rotation * Quaternion.Inverse(val.rotation);
			((Component)array[num]).transform.rotation = val2 * ((Component)array[num]).transform.rotation;
			Vector3 val3 = obj.position - val.position;
			Transform transform = ((Component)array[num]).transform;
			transform.position += val3;
		}
	}

	private void RefreshDryingEffects()
	{
		int i = 0;
		int num = 0;
		for (; i < hangSlots.Length; i++)
		{
			if (DryingOperations.Count > num)
			{
				for (int j = 0; j < DryingOperations[num].Quantity; j++)
				{
					SetQualityEffect(i, isActive: true, DryingOperations[num].StartQuality);
					i++;
					if (i >= hangSlots.Length)
					{
						break;
					}
				}
				num++;
			}
			else
			{
				SetQualityEffect(i, isActive: false);
			}
		}
	}

	public float GetDryMultiplier()
	{
		float num = 1f;
		float averageTileTemperature = GetAverageTileTemperature();
		if (averageTileTemperature > 20f)
		{
			num = Mathf.Lerp(num, 1.5f, (averageTileTemperature - 20f) / 20f);
		}
		return num;
	}

	private void SetQualityEffect(int index, bool isActive, EQuality quality = EQuality.Standard)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		((Component)_qualityParticleEffect[index]).gameObject.SetActive(isActive);
		if (isActive)
		{
			MainModule main = _qualityParticleEffect[index].main;
			Color colour = _qualityColourFont.GetColour(quality.ToString());
			colour.a = 0.6f;
			((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(colour);
			_qualityParticleEffect[index].Play();
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
		DryingRackUIElement component = ((Component)Object.Instantiate<DryingRackUIElement>(WorldspaceUIPrefab, (Transform)(object)base.ParentProperty.WorldspaceUIContainer)).GetComponent<DryingRackUIElement>();
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

	public void Open()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		IsOpen = true;
		SetPlayerUser(((NetworkBehaviour)Player.Local).NetworkObject);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		Transform val = CameraPositions[0];
		if (Vector3.Distance(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, CameraPositions[1].position) < Vector3.Distance(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, CameraPositions[0].position))
		{
			val = CameraPositions[1];
		}
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(val.position, val.rotation, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		Singleton<CompassManager>.Instance.SetVisible(visible: false);
		Singleton<DryingRackCanvas>.Instance.SetIsOpen(this, open: true);
	}

	public void Close()
	{
		IsOpen = false;
		Singleton<DryingRackCanvas>.Instance.SetIsOpen(null, open: false);
		SetPlayerUser(null);
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.2f);
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.2f);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		Singleton<CompassManager>.Instance.SetVisible(visible: true);
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
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

	public override BuildableItemData GetBaseData()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new DryingRackData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, _originCoordinate, _rotation, new ItemSet(new List<ItemSlot> { InputSlot }), new ItemSet(new List<ItemSlot> { OutputSlot }), DryingOperations.ToArray());
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
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Expected O, but got Unknown
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EDryingRackAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EDryingRackAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 2u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, CurrentPlayerConfigurer);
			syncVar____003CPlayerUserObject_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 1u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, PlayerUserObject);
			syncVar____003CNPCUserObject_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 0u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, NPCUserObject);
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_SetConfigurer_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(6u, new ServerRpcDelegate(RpcReader___Server_TryEndOperation_4146970406));
			((NetworkBehaviour)this).RegisterServerRpc(7u, new ServerRpcDelegate(RpcReader___Server_SendOperation_1307702229));
			((NetworkBehaviour)this).RegisterTargetRpc(8u, new ClientRpcDelegate(RpcReader___Target_PleaseReceiveOp_1575047616));
			((NetworkBehaviour)this).RegisterObserversRpc(9u, new ClientRpcDelegate(RpcReader___Observers_PleaseReceiveOp_1575047616));
			((NetworkBehaviour)this).RegisterObserversRpc(10u, new ClientRpcDelegate(RpcReader___Observers_RemoveOperation_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(11u, new ClientRpcDelegate(RpcReader___Observers_SetOperationQuantity_1692629761));
			((NetworkBehaviour)this).RegisterServerRpc(12u, new ServerRpcDelegate(RpcReader___Server_SetPlayerUser_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(13u, new ServerRpcDelegate(RpcReader___Server_SetNPCUser_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(14u, new ServerRpcDelegate(RpcReader___Server_SetStoredInstance_2652194801));
			((NetworkBehaviour)this).RegisterObserversRpc(15u, new ClientRpcDelegate(RpcReader___Observers_SetStoredInstance_Internal_2652194801));
			((NetworkBehaviour)this).RegisterTargetRpc(16u, new ClientRpcDelegate(RpcReader___Target_SetStoredInstance_Internal_2652194801));
			((NetworkBehaviour)this).RegisterServerRpc(17u, new ServerRpcDelegate(RpcReader___Server_SetItemSlotQuantity_1692629761));
			((NetworkBehaviour)this).RegisterObserversRpc(18u, new ClientRpcDelegate(RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761));
			((NetworkBehaviour)this).RegisterServerRpc(19u, new ServerRpcDelegate(RpcReader___Server_SetSlotLocked_3170825843));
			((NetworkBehaviour)this).RegisterTargetRpc(20u, new ClientRpcDelegate(RpcReader___Target_SetSlotLocked_Internal_3170825843));
			((NetworkBehaviour)this).RegisterObserversRpc(21u, new ClientRpcDelegate(RpcReader___Observers_SetSlotLocked_Internal_3170825843));
			((NetworkBehaviour)this).RegisterServerRpc(22u, new ServerRpcDelegate(RpcReader___Server_SetSlotFilter_527532783));
			((NetworkBehaviour)this).RegisterObserversRpc(23u, new ClientRpcDelegate(RpcReader___Observers_SetSlotFilter_Internal_527532783));
			((NetworkBehaviour)this).RegisterTargetRpc(24u, new ClientRpcDelegate(RpcReader___Target_SetSlotFilter_Internal_527532783));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EObjectScripts_002EDryingRack));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EDryingRackAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002EDryingRackAssembly_002DCSharp_002Edll_Excuted = true;
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

	private void RpcWriter___Server_TryEndOperation_4146970406(int operationIndex, bool allowSplitting, EQuality quality, int requestID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(operationIndex, (AutoPackType)1);
			((Writer)writer).WriteBoolean(allowSplitting);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated((Writer)(object)writer, quality);
			((Writer)writer).WriteInt32(requestID, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(6u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___TryEndOperation_4146970406(int operationIndex, bool allowSplitting, EQuality quality, int requestID)
	{
		if (requestIDs.Contains(requestID))
		{
			return;
		}
		requestIDs.Add(requestID);
		if (operationIndex >= DryingOperations.Count)
		{
			Console.LogError("Invalid operation index: " + operationIndex);
			return;
		}
		DryingOperation dryingOperation = DryingOperations[operationIndex];
		int outputCapacityForOperation = GetOutputCapacityForOperation(dryingOperation, quality);
		int num = Mathf.Min(dryingOperation.Quantity, outputCapacityForOperation);
		if (num == 0)
		{
			Console.LogWarning("No space in output slot for operation: " + operationIndex);
			return;
		}
		if (!allowSplitting && num < dryingOperation.Quantity)
		{
			Console.LogWarning("Operation would be split, but splitting is not allowed");
			return;
		}
		QualityItemInstance qualityItemInstance = Registry.GetItem(dryingOperation.ItemID).GetDefaultInstance(num) as QualityItemInstance;
		qualityItemInstance.SetQuality(quality);
		OutputSlot.InsertItem(qualityItemInstance);
		if (num == dryingOperation.Quantity)
		{
			RemoveOperation(DryingOperations.IndexOf(dryingOperation));
		}
		else
		{
			SetOperationQuantity(DryingOperations.IndexOf(dryingOperation), dryingOperation.Quantity - num);
		}
	}

	private void RpcReader___Server_TryEndOperation_4146970406(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int operationIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool allowSplitting = ((Reader)PooledReader0).ReadBoolean();
		EQuality quality = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		int requestID = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___TryEndOperation_4146970406(operationIndex, allowSplitting, quality, requestID);
		}
	}

	private void RpcWriter___Server_SendOperation_1307702229(DryingOperation op)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002EDryingOperationFishNet_002ESerializing_002EGenerated((Writer)(object)writer, op);
			((NetworkBehaviour)this).SendServerRpc(7u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendOperation_1307702229(DryingOperation op)
	{
		PleaseReceiveOp(null, op);
	}

	private void RpcReader___Server_SendOperation_1307702229(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		DryingOperation op = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002EDryingOperationFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendOperation_1307702229(op);
		}
	}

	private void RpcWriter___Target_PleaseReceiveOp_1575047616(NetworkConnection conn, DryingOperation op)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002EDryingOperationFishNet_002ESerializing_002EGenerated((Writer)(object)writer, op);
			((NetworkBehaviour)this).SendTargetRpc(8u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___PleaseReceiveOp_1575047616(NetworkConnection conn, DryingOperation op)
	{
		if (op.Quantity == 0)
		{
			Console.LogWarning("Operation quantity is 0. Ignoring");
			return;
		}
		DryingOperations.Add(op);
		if (onOperationStart != null)
		{
			onOperationStart(op);
		}
		if (onOperationsChanged != null)
		{
			onOperationsChanged();
		}
		RefreshHangingVisuals();
	}

	private void RpcReader___Target_PleaseReceiveOp_1575047616(PooledReader PooledReader0, Channel channel)
	{
		DryingOperation op = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002EDryingOperationFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___PleaseReceiveOp_1575047616(((NetworkBehaviour)this).LocalConnection, op);
		}
	}

	private void RpcWriter___Observers_PleaseReceiveOp_1575047616(NetworkConnection conn, DryingOperation op)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002EDryingOperationFishNet_002ESerializing_002EGenerated((Writer)(object)writer, op);
			((NetworkBehaviour)this).SendObserversRpc(9u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_PleaseReceiveOp_1575047616(PooledReader PooledReader0, Channel channel)
	{
		DryingOperation op = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002EDryingOperationFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___PleaseReceiveOp_1575047616(null, op);
		}
	}

	private void RpcWriter___Observers_RemoveOperation_3316948804(int opIndex)
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
			((Writer)writer).WriteInt32(opIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(10u, writer, val, (DataOrderType)0, false, true, false);
			writer.Store();
		}
	}

	private void RpcLogic___RemoveOperation_3316948804(int opIndex)
	{
		if (opIndex < DryingOperations.Count)
		{
			DryingOperation dryingOperation = DryingOperations[opIndex];
			DryingOperations.Remove(dryingOperation);
			if (onOperationComplete != null)
			{
				onOperationComplete(dryingOperation);
			}
			if (onOperationsChanged != null)
			{
				onOperationsChanged();
			}
			RefreshHangingVisuals();
		}
		else
		{
			Console.LogError("Invalid operation index: " + opIndex);
		}
	}

	private void RpcReader___Observers_RemoveOperation_3316948804(PooledReader PooledReader0, Channel channel)
	{
		int opIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___RemoveOperation_3316948804(opIndex);
		}
	}

	private void RpcWriter___Observers_SetOperationQuantity_1692629761(int opIndex, int quantity)
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
			((Writer)writer).WriteInt32(opIndex, (AutoPackType)1);
			((Writer)writer).WriteInt32(quantity, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(11u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetOperationQuantity_1692629761(int opIndex, int quantity)
	{
		if (opIndex < DryingOperations.Count)
		{
			DryingOperations[opIndex].Quantity = quantity;
			if (onOperationsChanged != null)
			{
				onOperationsChanged();
			}
			RefreshHangingVisuals();
		}
		else
		{
			Console.LogError("Invalid operation index: " + opIndex);
		}
	}

	private void RpcReader___Observers_SetOperationQuantity_1692629761(PooledReader PooledReader0, Channel channel)
	{
		int opIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int quantity = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetOperationQuantity_1692629761(opIndex, quantity);
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
			((NetworkBehaviour)this).SendServerRpc(12u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetPlayerUser_3323014238(NetworkObject playerObject)
	{
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
			((NetworkBehaviour)this).SendServerRpc(13u, writer, val, (DataOrderType)0);
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
			((NetworkBehaviour)this).SendServerRpc(14u, writer, val, (DataOrderType)0);
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
			((NetworkBehaviour)this).SendObserversRpc(15u, writer, val, (DataOrderType)0, false, false, false);
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
			((NetworkBehaviour)this).SendTargetRpc(16u, writer, val, (DataOrderType)0, conn, false, true);
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
			((NetworkBehaviour)this).SendServerRpc(17u, writer, val, (DataOrderType)0);
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
			((NetworkBehaviour)this).SendObserversRpc(18u, writer, val, (DataOrderType)0, false, false, false);
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
			((NetworkBehaviour)this).SendServerRpc(19u, writer, val, (DataOrderType)0);
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
			((NetworkBehaviour)this).SendTargetRpc(20u, writer, val, (DataOrderType)0, conn, false, true);
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
			((NetworkBehaviour)this).SendObserversRpc(21u, writer, val, (DataOrderType)0, false, false, false);
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
			((NetworkBehaviour)this).SendServerRpc(22u, writer, val, (DataOrderType)0);
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
			((NetworkBehaviour)this).SendObserversRpc(23u, writer, val, (DataOrderType)0, false, false, false);
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
			((NetworkBehaviour)this).SendTargetRpc(24u, writer, val, (DataOrderType)0, conn, false, true);
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

	public override bool ReadSyncVar___ScheduleOne_002EObjectScripts_002EDryingRack(PooledReader PooledReader0, uint UInt321, bool Boolean2)
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

	protected override void Awake_UserLogic_ScheduleOne_002EObjectScripts_002EDryingRack_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		if (!isGhost)
		{
			InputSlot = new ItemSlot(canPlayerSetFilter: true);
			InputSlot.SetSlotOwner(this);
			InputSlot.AddFilter(new ItemFilter_Dryable());
			InputVisuals.AddSlot(InputSlot);
			OutputSlot = new ItemSlot();
			OutputSlot.SetSlotOwner(this);
			OutputSlot.SetIsAddLocked(locked: true);
			OutputVisuals.AddSlot(OutputSlot);
			InputSlots.Add(InputSlot);
			OutputSlots.Add(OutputSlot);
			new ItemSlotSiblingSet(InputSlots);
			HangingVisuals.BlockRefreshes = true;
			hangSlots = new ItemSlot[HangAlignments.Length];
			for (int i = 0; i < HangAlignments.Length; i++)
			{
				hangSlots[i] = new ItemSlot();
				HangingVisuals.AddSlot(hangSlots[i]);
			}
			_qualityParticleEffect = ((Component)_qualityEffectContianer).GetComponentsInChildren<ParticleSystem>(true);
		}
	}
}
