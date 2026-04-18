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
using FishNet.Transporting;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using ScheduleOne.UI.Management;
using UnityEngine;

namespace ScheduleOne.Employees;

public class Packager : Employee, IConfigurable
{
	[Header("References")]
	public Sprite typeIcon;

	[SerializeField]
	protected ConfigurationReplicator configReplicator;

	public PackagingStationBehaviour PackagingBehaviour;

	public BrickPressBehaviour BrickPressBehaviour;

	[Header("UI")]
	public PackagerUIElement WorldspaceUIPrefab;

	public Transform uiPoint;

	[Header("Settings")]
	public int MaxAssignedStations = 3;

	[Header("Proficiency Settings")]
	public float PackagingSpeedMultiplier = 1f;

	[CompilerGenerated]
	[SyncVar]
	public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EEmployees_002EPackagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEmployees_002EPackagerAssembly_002DCSharp_002Edll_Excuted;

	public EntityConfiguration Configuration => configuration;

	protected PackagerConfiguration configuration { get; set; }

	public ConfigurationReplicator ConfigReplicator => configReplicator;

	public EConfigurableType ConfigurableType => EConfigurableType.Packager;

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

	public ScheduleOne.Property.Property ParentProperty => base.AssignedProperty;

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

	protected override void AssignProperty(ScheduleOne.Property.Property prop, bool warp)
	{
		base.AssignProperty(prop, warp);
		prop.AddConfigurable(this);
		configuration = new PackagerConfiguration(configReplicator, this, this);
		CreateWorldspaceUI();
	}

	protected override void UnassignProperty()
	{
		base.AssignedProperty.RemoveConfigurable(this);
		base.UnassignProperty();
	}

	protected override void ResetConfiguration()
	{
		if (configuration != null)
		{
			configuration.Reset();
		}
		base.ResetConfiguration();
	}

	protected override void Fire()
	{
		if (configuration != null)
		{
			configuration.Destroy();
			DestroyWorldspaceUI();
			if ((Object)(object)base.AssignedProperty != (Object)null)
			{
				base.AssignedProperty.RemoveConfigurable(this);
			}
		}
		base.Fire();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (!connection.IsHost)
		{
			SendConfigurationToClient(connection);
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

	protected override bool IsAnyWorkInProgress()
	{
		if (PackagingBehaviour.Active)
		{
			return true;
		}
		if (MoveItemBehaviour.Active)
		{
			return true;
		}
		return false;
	}

	protected override void UpdateBehaviour()
	{
		base.UpdateBehaviour();
		if (PackagingBehaviour.Active)
		{
			MarkIsWorking();
		}
		else if (MoveItemBehaviour.Active)
		{
			MarkIsWorking();
		}
		else
		{
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (base.Fired)
			{
				LeavePropertyAndDespawn();
			}
			else
			{
				if (!CanWork())
				{
					return;
				}
				if (configuration.AssignedStationCount + configuration.Routes.Routes.Count == 0)
				{
					SubmitNoWorkReason("I haven't been assigned to any stations or routes.", "You can use your management clipboards to assign stations or routes to me.");
					SetIdle(idle: true);
				}
				else
				{
					if (!InstanceFinder.IsServer)
					{
						return;
					}
					PackagingStation stationToAttend = GetStationToAttend();
					if ((Object)(object)stationToAttend != (Object)null)
					{
						StartPackaging(stationToAttend);
						return;
					}
					BrickPress brickPress = GetBrickPress();
					if ((Object)(object)brickPress != (Object)null)
					{
						StartPress(brickPress);
						return;
					}
					PackagingStation stationMoveItems = GetStationMoveItems();
					if ((Object)(object)stationMoveItems != (Object)null)
					{
						StartMoveItem(stationMoveItems);
						return;
					}
					BrickPress brickPressMoveItems = GetBrickPressMoveItems();
					if ((Object)(object)brickPressMoveItems != (Object)null)
					{
						StartMoveItem(brickPressMoveItems);
						return;
					}
					ItemInstance item;
					AdvancedTransitRoute transitRouteReady = GetTransitRouteReady(out item);
					if (transitRouteReady != null)
					{
						MoveItemBehaviour.Initialize(transitRouteReady, item, ((BaseItemInstance)item).Quantity);
						MoveItemBehaviour.Enable_Networked();
					}
					else
					{
						SubmitNoWorkReason("There's nothing for me to do right now.", "I need one of my assigned stations to have enough product and packaging to get to work.");
						SetIdle(idle: true);
					}
				}
			}
		}
	}

	private void StartPackaging(PackagingStation station)
	{
		Console.Log("Starting packaging at " + ((Object)((Component)station).gameObject).name);
		PackagingBehaviour.AssignStation(station);
		PackagingBehaviour.Enable_Networked();
	}

	private void StartPress(BrickPress press)
	{
		BrickPressBehaviour.AssignStation(press);
		BrickPressBehaviour.Enable_Networked();
	}

	private void StartMoveItem(PackagingStation station)
	{
		Console.Log("Starting moving items from " + ((Object)((Component)station).gameObject).name);
		MoveItemBehaviour.Initialize((station.Configuration as PackagingStationConfiguration).DestinationRoute, station.OutputSlot.ItemInstance);
		MoveItemBehaviour.Enable_Networked();
	}

	private void StartMoveItem(BrickPress press)
	{
		MoveItemBehaviour.Initialize((press.Configuration as BrickPressConfiguration).DestinationRoute, press.OutputSlot.ItemInstance);
		MoveItemBehaviour.Enable_Networked();
	}

	protected PackagingStation GetStationToAttend()
	{
		foreach (PackagingStation assignedStation in configuration.AssignedStations)
		{
			if (PackagingBehaviour.IsStationReady(assignedStation))
			{
				return assignedStation;
			}
		}
		return null;
	}

	protected BrickPress GetBrickPress()
	{
		foreach (BrickPress assignedBrickPress in configuration.AssignedBrickPresses)
		{
			if (BrickPressBehaviour.IsStationReady(assignedBrickPress))
			{
				return assignedBrickPress;
			}
		}
		return null;
	}

	protected PackagingStation GetStationMoveItems()
	{
		foreach (PackagingStation assignedStation in configuration.AssignedStations)
		{
			ItemSlot outputSlot = assignedStation.OutputSlot;
			if (outputSlot.Quantity != 0 && MoveItemBehaviour.IsTransitRouteValid((assignedStation.Configuration as PackagingStationConfiguration).DestinationRoute, ((BaseItemInstance)outputSlot.ItemInstance).ID))
			{
				return assignedStation;
			}
		}
		return null;
	}

	protected BrickPress GetBrickPressMoveItems()
	{
		foreach (BrickPress assignedBrickPress in configuration.AssignedBrickPresses)
		{
			ItemSlot outputSlot = assignedBrickPress.OutputSlot;
			if (outputSlot.Quantity != 0 && MoveItemBehaviour.IsTransitRouteValid((assignedBrickPress.Configuration as BrickPressConfiguration).DestinationRoute, ((BaseItemInstance)outputSlot.ItemInstance).ID))
			{
				return assignedBrickPress;
			}
		}
		return null;
	}

	protected AdvancedTransitRoute GetTransitRouteReady(out ItemInstance item)
	{
		item = null;
		foreach (AdvancedTransitRoute route in configuration.Routes.Routes)
		{
			item = route.GetItemReadyToMove();
			if (item != null && Movement.CanGetTo(route.Source) && Movement.CanGetTo(route.Destination) && Inventory.GetCapacityForItem(item) > 0)
			{
				return route;
			}
		}
		return null;
	}

	protected override bool ShouldIdle()
	{
		if (configuration.AssignedStationCount == 0)
		{
			return true;
		}
		return base.ShouldIdle();
	}

	public override EmployeeHome GetHome()
	{
		return configuration.assignedHome;
	}

	public WorldspaceUIElement CreateWorldspaceUI()
	{
		if ((Object)(object)WorldspaceUI != (Object)null)
		{
			Console.LogWarning(((Object)((Component)this).gameObject).name + " already has a worldspace UI element!");
		}
		ScheduleOne.Property.Property assignedProperty = base.AssignedProperty;
		if ((Object)(object)assignedProperty == (Object)null)
		{
			Console.LogError(((object)assignedProperty)?.ToString() + " is not a child of a property!");
			return null;
		}
		PackagerUIElement component = ((Component)Object.Instantiate<PackagerUIElement>(WorldspaceUIPrefab, (Transform)(object)assignedProperty.WorldspaceUIContainer)).GetComponent<PackagerUIElement>();
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

	public override NPCData GetNPCData()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		return new PackagerData(ID, base.AssignedProperty.PropertyCode, FirstName, LastName, base.IsMale, base.AppearanceIndex, ((Component)this).transform.position, ((Component)this).transform.rotation, base.GUID, base.PaidForToday, MoveItemBehaviour.GetSaveData());
	}

	public override DynamicSaveData GetSaveData()
	{
		DynamicSaveData saveData = base.GetSaveData();
		saveData.AddData("Configuration", Configuration.GetSaveString());
		return saveData;
	}

	public override List<string> WriteData(string parentFolderPath)
	{
		return new List<string>();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EEmployees_002EPackagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEmployees_002EPackagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 2u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, CurrentPlayerConfigurer);
			((NetworkBehaviour)this).RegisterServerRpc(46u, new ServerRpcDelegate(RpcReader___Server_SetConfigurer_3323014238));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EEmployees_002EPackager));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEmployees_002EPackagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEmployees_002EPackagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField).SetRegistered();
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
			((NetworkBehaviour)this).SendServerRpc(46u, writer, val, (DataOrderType)0);
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

	public override bool ReadSyncVar___ScheduleOne_002EEmployees_002EPackager(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		if (UInt321 == 2)
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CCurrentPlayerConfigurer_003Ek__BackingField(syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			NetworkObject value = ((Reader)PooledReader0).ReadNetworkObject();
			this.sync___set_value__003CCurrentPlayerConfigurer_003Ek__BackingField(value, Boolean2);
			return true;
		}
		return false;
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
