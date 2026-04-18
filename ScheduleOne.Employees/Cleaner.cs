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
using FishNet.Transporting;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.ObjectScripts.WateringCan;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using ScheduleOne.Trash;
using ScheduleOne.UI.Management;
using UnityEngine;

namespace ScheduleOne.Employees;

public class Cleaner : Employee, IConfigurable
{
	public const int MAX_ASSIGNED_BINS = 6;

	public TrashGrabberDefinition TrashGrabberDef;

	[Header("References")]
	public PickUpTrashBehaviour PickUpTrashBehaviour;

	public EmptyTrashGrabberBehaviour EmptyTrashGrabberBehaviour;

	public BagTrashCanBehaviour BagTrashCanBehaviour;

	public DisposeTrashBagBehaviour DisposeTrashBagBehaviour;

	public Sprite typeIcon;

	[SerializeField]
	protected ConfigurationReplicator configReplicator;

	[Header("UI")]
	public CleanerUIElement WorldspaceUIPrefab;

	public Transform uiPoint;

	[CompilerGenerated]
	[SyncVar]
	public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EEmployees_002ECleanerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEmployees_002ECleanerAssembly_002DCSharp_002Edll_Excuted;

	public TrashGrabberInstance trashGrabberInstance { get; private set; }

	public EntityConfiguration Configuration => configuration;

	protected CleanerConfiguration configuration { get; set; }

	public ConfigurationReplicator ConfigReplicator => configReplicator;

	public EConfigurableType ConfigurableType => EConfigurableType.Cleaner;

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
		configuration = new CleanerConfiguration(configReplicator, this, this);
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

	protected override void UpdateBehaviour()
	{
		base.UpdateBehaviour();
		if (IsAnyWorkInProgress())
		{
			MarkIsWorking();
		}
		else
		{
			if (!InstanceFinder.IsServer || Singleton<LoadManager>.Instance.IsLoading)
			{
				return;
			}
			if (base.Fired)
			{
				LeavePropertyAndDespawn();
			}
			else if (CanWork())
			{
				if (configuration.binItems.Count == 0)
				{
					SubmitNoWorkReason("I haven't been assigned any trash cans", "You can use your management clipboards to assign trash cans to me.");
					SetIdle(idle: true);
				}
				else if (InstanceFinder.IsServer)
				{
					TryStartNewTask();
				}
			}
		}
	}

	private void TryStartNewTask()
	{
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		TrashContainerItem[] trashContainersOrderedByDistance = GetTrashContainersOrderedByDistance();
		EnsureTrashGrabberInInventory();
		TrashContainerItem[] array = trashContainersOrderedByDistance;
		foreach (TrashContainerItem trashContainerItem in array)
		{
			if (trashContainerItem.TrashBagsInRadius.Count > 0)
			{
				if ((Object)(object)base.AssignedProperty.DisposalArea != (Object)null)
				{
					TrashBag targetBag = trashContainerItem.TrashBagsInRadius[0];
					DisposeTrashBagBehaviour.SetTargetBag(targetBag);
					DisposeTrashBagBehaviour.Enable_Networked();
					return;
				}
				Console.LogError("No disposal area assigned to property " + base.AssignedProperty.PropertyCode);
			}
		}
		if (GetTrashGrabberAmount() < 20)
		{
			array = trashContainersOrderedByDistance;
			foreach (TrashContainerItem trashContainerItem2 in array)
			{
				if (trashContainerItem2.TrashItemsInRadius.Count <= 0)
				{
					continue;
				}
				int num = 0;
				TrashItem trashItem = trashContainerItem2.TrashItemsInRadius[num];
				while ((Object)(object)trashItem == (Object)null || !Movement.CanGetTo(((Component)trashItem).transform.position))
				{
					num++;
					if (num >= trashContainerItem2.TrashItemsInRadius.Count)
					{
						trashItem = null;
						break;
					}
					trashItem = trashContainerItem2.TrashItemsInRadius[num];
				}
				if ((Object)(object)trashItem != (Object)null)
				{
					PickUpTrashBehaviour.SetTargetTrash(trashItem);
					PickUpTrashBehaviour.Enable_Networked();
					return;
				}
			}
		}
		if (GetTrashGrabberAmount() >= 20 && (Object)(object)GetFirstNonFullBin(trashContainersOrderedByDistance) != (Object)null)
		{
			EmptyTrashGrabberBehaviour.SetTargetTrashCan(GetFirstNonFullBin(trashContainersOrderedByDistance));
			EmptyTrashGrabberBehaviour.Enable_Networked();
			return;
		}
		array = trashContainersOrderedByDistance;
		foreach (TrashContainerItem trashContainerItem3 in array)
		{
			if (trashContainerItem3.Container.NormalizedTrashLevel >= 0.75f)
			{
				BagTrashCanBehaviour.SetTargetTrashCan(trashContainerItem3);
				BagTrashCanBehaviour.Enable_Networked();
				return;
			}
		}
		SubmitNoWorkReason("There's nothing for me to do right now.", string.Empty);
		SetIdle(idle: true);
	}

	private TrashContainerItem GetFirstNonFullBin(TrashContainerItem[] bins)
	{
		return bins.FirstOrDefault((TrashContainerItem bin) => bin.Container.NormalizedTrashLevel < 1f);
	}

	public override void SetIdle(bool idle)
	{
		base.SetIdle(idle);
		if (idle && (Object)(object)Avatar.CurrentEquippable != (Object)null)
		{
			SetEquippable_Return(string.Empty);
		}
	}

	private TrashContainerItem[] GetTrashContainersOrderedByDistance()
	{
		TrashContainerItem[] array = configuration.binItems.ToArray();
		Array.Sort(array, delegate(TrashContainerItem x, TrashContainerItem y)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			float num = Vector3.Distance(((Component)x).transform.position, ((Component)this).transform.position);
			float value = Vector3.Distance(((Component)y).transform.position, ((Component)this).transform.position);
			return num.CompareTo(value);
		});
		return array;
	}

	public override EmployeeHome GetHome()
	{
		return configuration.assignedHome;
	}

	private void EnsureTrashGrabberInInventory()
	{
		if (InstanceFinder.IsServer)
		{
			if (((IItemSlotOwner)Inventory).GetQuantityOfItem(((BaseItemDefinition)TrashGrabberDef).ID) == 0)
			{
				Inventory.InsertItem(TrashGrabberDef.GetDefaultInstance());
			}
			trashGrabberInstance = Inventory.GetFirstItem(((BaseItemDefinition)TrashGrabberDef).ID) as TrashGrabberInstance;
		}
	}

	protected override bool IsAnyWorkInProgress()
	{
		if (PickUpTrashBehaviour.Active)
		{
			return true;
		}
		if (EmptyTrashGrabberBehaviour.Active)
		{
			return true;
		}
		if (BagTrashCanBehaviour.Active)
		{
			return true;
		}
		if (DisposeTrashBagBehaviour.Active)
		{
			return true;
		}
		if (MoveItemBehaviour.Active)
		{
			return true;
		}
		return false;
	}

	private int GetTrashGrabberAmount()
	{
		return trashGrabberInstance.GetTotalSize();
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
		CleanerUIElement component = ((Component)Object.Instantiate<CleanerUIElement>(WorldspaceUIPrefab, (Transform)(object)assignedProperty.WorldspaceUIContainer)).GetComponent<CleanerUIElement>();
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
		return new CleanerData(ID, base.AssignedProperty.PropertyCode, FirstName, LastName, base.IsMale, base.AppearanceIndex, ((Component)this).transform.position, ((Component)this).transform.rotation, base.GUID, base.PaidForToday, MoveItemBehaviour.GetSaveData());
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
		if (!NetworkInitialize___EarlyScheduleOne_002EEmployees_002ECleanerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEmployees_002ECleanerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 2u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, CurrentPlayerConfigurer);
			((NetworkBehaviour)this).RegisterServerRpc(46u, new ServerRpcDelegate(RpcReader___Server_SetConfigurer_3323014238));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EEmployees_002ECleaner));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEmployees_002ECleanerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEmployees_002ECleanerAssembly_002DCSharp_002Edll_Excuted = true;
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

	public override bool ReadSyncVar___ScheduleOne_002EEmployees_002ECleaner(PooledReader PooledReader0, uint UInt321, bool Boolean2)
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
