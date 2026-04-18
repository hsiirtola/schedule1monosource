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
using ScheduleOne.Misc;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.StationFramework;
using ScheduleOne.Storage;
using ScheduleOne.Tiles;
using ScheduleOne.Tools;
using ScheduleOne.Trash;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Management;
using ScheduleOne.UI.Stations;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts;

public class Cauldron : GridItem, IUsable, IItemSlotOwner, ITransitEntity, IConfigurable
{
	public enum EState
	{
		MissingIngredients,
		Ready,
		Cooking,
		OutputFull
	}

	public const int INGREDIENT_SLOT_COUNT = 4;

	public const int COCA_LEAF_REQUIRED = 20;

	public ItemSlot[] IngredientSlots;

	public ItemSlot LiquidSlot;

	public ItemSlot OutputSlot;

	public int CookTime = 360;

	[Header("References")]
	public Transform CameraPosition;

	public Transform CameraPosition_CombineIngredients;

	public Transform CameraPosition_StartMachine;

	public InteractableObject IntObj;

	public Transform[] accessPoints;

	public Transform StandPoint;

	public Transform uiPoint;

	public StorageVisualizer LiquidVisuals;

	public StorageVisualizer OutputVisuals;

	public CauldronDisplayTub PrimaryTub;

	public CauldronDisplayTub SecondaryTub;

	public Transform ItemContainer;

	public Transform GasolineSpawnPoint;

	public Transform TubSpawnPoint;

	public Transform[] LeafSpawns;

	public Light OverheadLight;

	public Fillable CauldronFillable;

	public Clickable StartButtonClickable;

	public DigitalAlarm Alarm;

	public ToggleableLight Light;

	public ConfigurationReplicator configReplicator;

	public BoxCollider TrashSpawnVolume;

	public Transform LeafDragProjectionPlane;

	[Header("Prefabs")]
	public StationItem CocaLeafPrefab;

	public StationItem GasolinePrefab;

	public Draggable TubPrefab;

	public QualityItemDefinition CocaineBaseDefinition;

	[Header("UI")]
	public CauldronUIElement WorldspaceUIPrefab;

	public Sprite typeIcon;

	public UnityEvent onStartButtonClicked;

	public UnityEvent onCookStart;

	public UnityEvent onCookEnd;

	public int RemainingCookTime;

	public EQuality InputQuality = EQuality.Standard;

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

	public SyncVar<NetworkObject> syncVar____003CNPCUserObject_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CPlayerUserObject_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ECauldronAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002ECauldronAssembly_002DCSharp_002Edll_Excuted;

	public bool isOpen
	{
		get
		{
			if (Singleton<CauldronCanvas>.Instance.isOpen)
			{
				return (Object)(object)Singleton<CauldronCanvas>.Instance.Cauldron == (Object)(object)this;
			}
			return false;
		}
	}

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

	public Transform LinkOrigin => UIPoint;

	public Transform[] AccessPoints => accessPoints;

	public bool Selectable { get; } = true;

	public bool IsAcceptingItems { get; set; } = true;

	public EntityConfiguration Configuration => cauldronConfiguration;

	protected CauldronConfiguration cauldronConfiguration { get; set; }

	public ConfigurationReplicator ConfigReplicator => configReplicator;

	public EConfigurableType ConfigurableType => EConfigurableType.Cauldron;

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

	private bool isCooking => RemainingCookTime > 0;

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
		Awake_UserLogic_ScheduleOne_002EObjectScripts_002ECauldron_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void InitializeGridItem(ItemInstance instance, Grid grid, Vector2 originCoordinate, int rotation, string GUID)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeGridItem(instance, grid, originCoordinate, rotation, GUID);
		base.ParentProperty.AddConfigurable(this);
		cauldronConfiguration = new CauldronConfiguration(configReplicator, this, this);
		CreateWorldspaceUI();
		GameInput.RegisterExitListener(Exit, 4);
	}

	public override string GetManagementName()
	{
		return Configuration.Name.Value;
	}

	protected override void Start()
	{
		base.Start();
		if (!isGhost)
		{
			NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnMinPass);
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onTimeSkip = (Action<int>)Delegate.Combine(instance.onTimeSkip, new Action<int>(OnTimePass));
			StartButtonClickable.onClickStart.AddListener((UnityAction<RaycastHit>)ButtonClicked);
		}
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (RemainingCookTime > 0)
		{
			StartCookOperation(connection, RemainingCookTime, InputQuality);
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

	protected override void Destroy()
	{
		NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(OnMinPass);
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onTimeSkip = (Action<int>)Delegate.Remove(instance.onTimeSkip, new Action<int>(OnTimePass));
		GameInput.DeregisterExitListener(Exit);
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
		if (RemainingCookTime > 0)
		{
			Alarm.SetScreenLit(lit: true);
			Alarm.DisplayMinutes(RemainingCookTime);
			Light.isOn = true;
			RemainingCookTime -= minutes;
			if (RemainingCookTime <= 0 && InstanceFinder.IsServer)
			{
				FinishCookOperation();
			}
		}
		else
		{
			Alarm.SetScreenLit(lit: false);
			Alarm.DisplayMinutes(0);
			if (OutputSlot.Quantity > 0)
			{
				Light.isOn = NetworkSingleton<TimeManager>.Instance.CurrentTime % 2 == 0;
			}
			else
			{
				Light.isOn = false;
			}
		}
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && isOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			Close();
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

	public void Open()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		SetPlayerUser(((NetworkBehaviour)Player.Local).NetworkObject);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraPosition.position, CameraPosition.rotation, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(65f, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		Singleton<CompassManager>.Instance.SetVisible(visible: false);
		Singleton<CauldronCanvas>.Instance.SetIsOpen(this, open: true);
	}

	public void Close()
	{
		Singleton<CauldronCanvas>.Instance.SetIsOpen(null, open: false);
		SetPlayerUser(null);
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.2f);
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.2f);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		Singleton<CompassManager>.Instance.SetVisible(visible: true);
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
	}

	public override bool CanBeDestroyed(out string reason)
	{
		if (((IItemSlotOwner)this).GetQuantitySum() > 0)
		{
			reason = "Contains items";
			return false;
		}
		if (((IUsable)this).IsInUse)
		{
			reason = "Currently in use";
			return false;
		}
		if (isCooking)
		{
			reason = "Currently cooking";
			return false;
		}
		return base.CanBeDestroyed(out reason);
	}

	private void UpdateIngredientVisuals()
	{
		GetMainInputs(out var primaryItem, out var primaryItemQuantity, out var secondaryItem, out var secondaryItemQuantity);
		if (primaryItem != null)
		{
			PrimaryTub.Configure(CauldronDisplayTub.EContents.CocaLeaf, (float)primaryItemQuantity / 20f);
		}
		else
		{
			PrimaryTub.Configure(CauldronDisplayTub.EContents.None, 0f);
		}
		if (secondaryItem != null)
		{
			SecondaryTub.Configure(CauldronDisplayTub.EContents.CocaLeaf, (float)secondaryItemQuantity / 20f);
		}
		else
		{
			SecondaryTub.Configure(CauldronDisplayTub.EContents.None, 0f);
		}
	}

	public void GetMainInputs(out ItemInstance primaryItem, out int primaryItemQuantity, out ItemInstance secondaryItem, out int secondaryItemQuantity)
	{
		List<ItemInstance> list = new List<ItemInstance>();
		Dictionary<ItemInstance, int> itemQuantities = new Dictionary<ItemInstance, int>();
		int i;
		for (i = 0; i < IngredientSlots.Length; i++)
		{
			if (IngredientSlots[i].ItemInstance == null)
			{
				continue;
			}
			ItemInstance itemInstance = list.Find((ItemInstance x) => ((BaseItemInstance)x).ID == ((BaseItemInstance)IngredientSlots[i].ItemInstance).ID);
			if (itemInstance == null || !itemInstance.CanStackWith(IngredientSlots[i].ItemInstance, checkQuantities: false))
			{
				itemInstance = IngredientSlots[i].ItemInstance;
				list.Add(itemInstance);
				if (!itemQuantities.ContainsKey(IngredientSlots[i].ItemInstance))
				{
					itemQuantities.Add(IngredientSlots[i].ItemInstance, 0);
				}
			}
			itemQuantities[itemInstance] += IngredientSlots[i].Quantity;
		}
		for (int num = 0; num < list.Count; num++)
		{
			if (itemQuantities[list[num]] > 20)
			{
				int num2 = itemQuantities[list[num]] - 20;
				itemQuantities[list[num]] = 20;
				ItemInstance copy = list[num].GetCopy(num2);
				list.Add(copy);
				itemQuantities.Add(copy, num2);
			}
		}
		list = list.OrderByDescending((ItemInstance x) => itemQuantities[x]).ToList();
		if (list.Count > 0)
		{
			primaryItem = list[0];
			primaryItemQuantity = itemQuantities[list[0]];
		}
		else
		{
			primaryItem = null;
			primaryItemQuantity = 0;
		}
		if (list.Count > 1)
		{
			secondaryItem = list[1];
			secondaryItemQuantity = itemQuantities[list[1]];
		}
		else
		{
			secondaryItem = null;
			secondaryItemQuantity = 0;
		}
	}

	public EState GetState()
	{
		if (isCooking)
		{
			return EState.Cooking;
		}
		if (!HasIngredients())
		{
			return EState.MissingIngredients;
		}
		if (!HasOutputSpace())
		{
			return EState.OutputFull;
		}
		return EState.Ready;
	}

	public bool HasOutputSpace()
	{
		ItemInstance defaultInstance = CocaineBaseDefinition.GetDefaultInstance();
		return OutputSlot.GetCapacityForItem(defaultInstance) >= 10;
	}

	public EQuality RemoveIngredients()
	{
		LiquidSlot.ChangeQuantity(-1);
		EQuality eQuality = EQuality.Heavenly;
		int num = 20;
		int num2 = IngredientSlots.Length - 1;
		while (num2 >= 0 && num > 0)
		{
			if (IngredientSlots[num2].Quantity > 0)
			{
				EQuality quality = (IngredientSlots[num2].ItemInstance as QualityItemInstance).Quality;
				if (quality < eQuality)
				{
					eQuality = quality;
				}
				int num3 = Mathf.Min(num, IngredientSlots[num2].Quantity);
				IngredientSlots[num2].ChangeQuantity(-num3);
				num -= num3;
			}
			num2--;
		}
		return eQuality;
	}

	public bool HasIngredients()
	{
		int num = 0;
		int quantity = LiquidSlot.Quantity;
		for (int i = 0; i < IngredientSlots.Length; i++)
		{
			if (IngredientSlots[i].ItemInstance != null)
			{
				num += IngredientSlots[i].Quantity;
			}
		}
		if (num >= 20)
		{
			return quantity > 0;
		}
		return false;
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendCookOperation(int remainingCookTime, EQuality quality)
	{
		RpcWriter___Server_SendCookOperation_3536682170(remainingCookTime, quality);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void StartCookOperation(NetworkConnection conn, int remainingCookTime, EQuality quality)
	{
		if (conn == null)
		{
			RpcWriter___Observers_StartCookOperation_4210838825(conn, remainingCookTime, quality);
			RpcLogic___StartCookOperation_4210838825(conn, remainingCookTime, quality);
		}
		else
		{
			RpcWriter___Target_StartCookOperation_4210838825(conn, remainingCookTime, quality);
		}
	}

	[ObserversRpc]
	public void FinishCookOperation()
	{
		RpcWriter___Observers_FinishCookOperation_2166136261();
	}

	private void ButtonClicked(RaycastHit hit)
	{
		if (onStartButtonClicked != null)
		{
			onStartButtonClicked.Invoke();
		}
	}

	public void CreateTrash(List<StationItem> mixerItems)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < mixerItems.Count; i++)
		{
			if (!((Object)(object)mixerItems[i].TrashPrefab == (Object)null))
			{
				Vector3 posiiton = ((Component)TrashSpawnVolume).transform.TransformPoint(new Vector3(Random.Range((0f - TrashSpawnVolume.size.x) / 2f, TrashSpawnVolume.size.x / 2f), 0f, Random.Range((0f - TrashSpawnVolume.size.z) / 2f, TrashSpawnVolume.size.z / 2f)));
				Vector3 forward = ((Component)TrashSpawnVolume).transform.forward;
				forward = Quaternion.Euler(0f, Random.Range(-45f, 45f), 0f) * forward;
				float num = Random.Range(0.25f, 0.4f);
				NetworkSingleton<TrashManager>.Instance.CreateTrashItem(mixerItems[i].TrashPrefab.ID, posiiton, Random.rotation, forward * num);
			}
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
		CauldronUIElement component = ((Component)Object.Instantiate<CauldronUIElement>(WorldspaceUIPrefab, (Transform)(object)base.ParentProperty.WorldspaceUIContainer)).GetComponent<CauldronUIElement>();
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

	public override BuildableItemData GetBaseData()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new CauldronData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, _originCoordinate, _rotation, new ItemSet(new List<ItemSlot>(IngredientSlots)), new ItemSet(new List<ItemSlot> { LiquidSlot }), new ItemSet(new List<ItemSlot> { OutputSlot }), RemainingCookTime, InputQuality);
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
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ECauldronAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ECauldronAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 2u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, CurrentPlayerConfigurer);
			syncVar____003CPlayerUserObject_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 1u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, PlayerUserObject);
			syncVar____003CNPCUserObject_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 0u, (WritePermission)1, (ReadPermission)0, -1f, (Channel)0, NPCUserObject);
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_SetConfigurer_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(6u, new ServerRpcDelegate(RpcReader___Server_SendCookOperation_3536682170));
			((NetworkBehaviour)this).RegisterObserversRpc(7u, new ClientRpcDelegate(RpcReader___Observers_StartCookOperation_4210838825));
			((NetworkBehaviour)this).RegisterTargetRpc(8u, new ClientRpcDelegate(RpcReader___Target_StartCookOperation_4210838825));
			((NetworkBehaviour)this).RegisterObserversRpc(9u, new ClientRpcDelegate(RpcReader___Observers_FinishCookOperation_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(10u, new ServerRpcDelegate(RpcReader___Server_SetPlayerUser_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(11u, new ServerRpcDelegate(RpcReader___Server_SetNPCUser_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(12u, new ServerRpcDelegate(RpcReader___Server_SetStoredInstance_2652194801));
			((NetworkBehaviour)this).RegisterObserversRpc(13u, new ClientRpcDelegate(RpcReader___Observers_SetStoredInstance_Internal_2652194801));
			((NetworkBehaviour)this).RegisterTargetRpc(14u, new ClientRpcDelegate(RpcReader___Target_SetStoredInstance_Internal_2652194801));
			((NetworkBehaviour)this).RegisterServerRpc(15u, new ServerRpcDelegate(RpcReader___Server_SetItemSlotQuantity_1692629761));
			((NetworkBehaviour)this).RegisterObserversRpc(16u, new ClientRpcDelegate(RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761));
			((NetworkBehaviour)this).RegisterServerRpc(17u, new ServerRpcDelegate(RpcReader___Server_SetSlotLocked_3170825843));
			((NetworkBehaviour)this).RegisterTargetRpc(18u, new ClientRpcDelegate(RpcReader___Target_SetSlotLocked_Internal_3170825843));
			((NetworkBehaviour)this).RegisterObserversRpc(19u, new ClientRpcDelegate(RpcReader___Observers_SetSlotLocked_Internal_3170825843));
			((NetworkBehaviour)this).RegisterServerRpc(20u, new ServerRpcDelegate(RpcReader___Server_SetSlotFilter_527532783));
			((NetworkBehaviour)this).RegisterObserversRpc(21u, new ClientRpcDelegate(RpcReader___Observers_SetSlotFilter_Internal_527532783));
			((NetworkBehaviour)this).RegisterTargetRpc(22u, new ClientRpcDelegate(RpcReader___Target_SetSlotFilter_Internal_527532783));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EObjectScripts_002ECauldron));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002ECauldronAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002ECauldronAssembly_002DCSharp_002Edll_Excuted = true;
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

	private void RpcWriter___Server_SendCookOperation_3536682170(int remainingCookTime, EQuality quality)
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
			((Writer)writer).WriteInt32(remainingCookTime, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated((Writer)(object)writer, quality);
			((NetworkBehaviour)this).SendServerRpc(6u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendCookOperation_3536682170(int remainingCookTime, EQuality quality)
	{
		StartCookOperation(null, remainingCookTime, quality);
	}

	private void RpcReader___Server_SendCookOperation_3536682170(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int remainingCookTime = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		EQuality quality = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendCookOperation_3536682170(remainingCookTime, quality);
		}
	}

	private void RpcWriter___Observers_StartCookOperation_4210838825(NetworkConnection conn, int remainingCookTime, EQuality quality)
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
			((Writer)writer).WriteInt32(remainingCookTime, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated((Writer)(object)writer, quality);
			((NetworkBehaviour)this).SendObserversRpc(7u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___StartCookOperation_4210838825(NetworkConnection conn, int remainingCookTime, EQuality quality)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		RemainingCookTime = remainingCookTime;
		InputQuality = quality;
		CauldronFillable.AddLiquid("gasoline", 1f, Color.white);
		if (onCookStart != null)
		{
			onCookStart.Invoke();
		}
	}

	private void RpcReader___Observers_StartCookOperation_4210838825(PooledReader PooledReader0, Channel channel)
	{
		int remainingCookTime = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		EQuality quality = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartCookOperation_4210838825(null, remainingCookTime, quality);
		}
	}

	private void RpcWriter___Target_StartCookOperation_4210838825(NetworkConnection conn, int remainingCookTime, EQuality quality)
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
			((Writer)writer).WriteInt32(remainingCookTime, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated((Writer)(object)writer, quality);
			((NetworkBehaviour)this).SendTargetRpc(8u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_StartCookOperation_4210838825(PooledReader PooledReader0, Channel channel)
	{
		int remainingCookTime = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		EQuality quality = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___StartCookOperation_4210838825(((NetworkBehaviour)this).LocalConnection, remainingCookTime, quality);
		}
	}

	private void RpcWriter___Observers_FinishCookOperation_2166136261()
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

	public void RpcLogic___FinishCookOperation_2166136261()
	{
		if (InstanceFinder.IsServer)
		{
			QualityItemInstance qualityItemInstance = CocaineBaseDefinition.GetDefaultInstance(10) as QualityItemInstance;
			qualityItemInstance.SetQuality(InputQuality);
			OutputSlot.InsertItem(qualityItemInstance);
		}
		CauldronFillable.ResetContents();
		if (onCookEnd != null)
		{
			onCookEnd.Invoke();
		}
	}

	private void RpcReader___Observers_FinishCookOperation_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___FinishCookOperation_2166136261();
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
			((NetworkBehaviour)this).SendServerRpc(10u, writer, val, (DataOrderType)0);
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
			((NetworkBehaviour)this).SendServerRpc(11u, writer, val, (DataOrderType)0);
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
			((NetworkBehaviour)this).SendServerRpc(12u, writer, val, (DataOrderType)0);
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
			((NetworkBehaviour)this).SendObserversRpc(13u, writer, val, (DataOrderType)0, false, false, false);
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
			((NetworkBehaviour)this).SendTargetRpc(14u, writer, val, (DataOrderType)0, conn, false, true);
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
			((NetworkBehaviour)this).SendServerRpc(15u, writer, val, (DataOrderType)0);
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
			((NetworkBehaviour)this).SendObserversRpc(16u, writer, val, (DataOrderType)0, false, false, false);
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
			((NetworkBehaviour)this).SendServerRpc(17u, writer, val, (DataOrderType)0);
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
			((NetworkBehaviour)this).SendTargetRpc(18u, writer, val, (DataOrderType)0, conn, false, true);
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
			((NetworkBehaviour)this).SendObserversRpc(19u, writer, val, (DataOrderType)0, false, false, false);
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
			((NetworkBehaviour)this).SendServerRpc(20u, writer, val, (DataOrderType)0);
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
			((NetworkBehaviour)this).SendObserversRpc(21u, writer, val, (DataOrderType)0, false, false, false);
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
			((NetworkBehaviour)this).SendTargetRpc(22u, writer, val, (DataOrderType)0, conn, false, true);
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

	public override bool ReadSyncVar___ScheduleOne_002EObjectScripts_002ECauldron(PooledReader PooledReader0, uint UInt321, bool Boolean2)
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

	protected override void Awake_UserLogic_ScheduleOne_002EObjectScripts_002ECauldron_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		if (!isGhost)
		{
			IngredientSlots = new ItemSlot[4];
			for (int i = 0; i < 4; i++)
			{
				IngredientSlots[i] = new ItemSlot(canPlayerSetFilter: true);
				IngredientSlots[i].SetSlotOwner(this);
				IngredientSlots[i].AddFilter(new ItemFilter_ID(new List<string> { "cocaleaf" }));
				ItemSlot obj = IngredientSlots[i];
				obj.onItemDataChanged = (Action)Delegate.Combine(obj.onItemDataChanged, new Action(UpdateIngredientVisuals));
			}
			LiquidSlot = new ItemSlot(canPlayerSetFilter: true);
			LiquidSlot.SetSlotOwner(this);
			LiquidSlot.AddFilter(new ItemFilter_ID(new List<string> { "gasoline" }));
			LiquidVisuals.AddSlot(LiquidSlot);
			OutputSlot = new ItemSlot();
			OutputSlot.SetSlotOwner(this);
			OutputSlot.SetIsAddLocked(locked: true);
			OutputVisuals.AddSlot(OutputSlot);
			InputSlots.AddRange(IngredientSlots);
			InputSlots.Add(LiquidSlot);
			OutputSlots.Add(OutputSlot);
			new ItemSlotSiblingSet(InputSlots);
			((Component)PrimaryTub).gameObject.SetActive(true);
			((Component)SecondaryTub).gameObject.SetActive(true);
		}
	}
}
