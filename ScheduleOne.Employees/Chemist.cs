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
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Management;
using UnityEngine;

namespace ScheduleOne.Employees;

public class Chemist : Employee, IConfigurable
{
	public const int MAX_ASSIGNED_STATIONS = 4;

	[Header("References")]
	public Sprite typeIcon;

	[SerializeField]
	protected ConfigurationReplicator configReplicator;

	[Header("Behaviours")]
	public StartChemistryStationBehaviour StartChemistryStationBehaviour;

	public StartLabOvenBehaviour StartLabOvenBehaviour;

	public FinishLabOvenBehaviour FinishLabOvenBehaviour;

	public StartCauldronBehaviour StartCauldronBehaviour;

	public StartMixingStationBehaviour StartMixingStationBehaviour;

	[Header("UI")]
	public ChemistUIElement WorldspaceUIPrefab;

	public Transform uiPoint;

	[CompilerGenerated]
	[SyncVar]
	public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

	public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EEmployees_002EChemistAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEmployees_002EChemistAssembly_002DCSharp_002Edll_Excuted;

	public EntityConfiguration Configuration => configuration;

	protected ChemistConfiguration configuration { get; set; }

	public ConfigurationReplicator ConfigReplicator => configReplicator;

	public EConfigurableType ConfigurableType => EConfigurableType.Chemist;

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
		configuration = new ChemistConfiguration(configReplicator, this, this);
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
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (base.Fired)
			{
				LeavePropertyAndDespawn();
			}
			else if (CanWork())
			{
				if (configuration.TotalStations == 0)
				{
					SubmitNoWorkReason("I haven't been assigned any stations", "You can use your management clipboards to assign stations to me.");
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
		List<LabOven> labOvensReadyToFinish = GetLabOvensReadyToFinish();
		if (labOvensReadyToFinish.Count > 0)
		{
			FinishLabOven(labOvensReadyToFinish[0]);
			return;
		}
		List<LabOven> labOvensReadyToStart = GetLabOvensReadyToStart();
		if (labOvensReadyToStart.Count > 0)
		{
			StartLabOven(labOvensReadyToStart[0]);
			return;
		}
		List<ChemistryStation> chemistryStationsReadyToStart = GetChemistryStationsReadyToStart();
		if (chemistryStationsReadyToStart.Count > 0)
		{
			StartChemistryStation(chemistryStationsReadyToStart[0]);
			return;
		}
		List<Cauldron> cauldronsReadyToStart = GetCauldronsReadyToStart();
		if (cauldronsReadyToStart.Count > 0)
		{
			StartCauldron(cauldronsReadyToStart[0]);
			return;
		}
		List<MixingStation> mixingStationsReadyToStart = GetMixingStationsReadyToStart();
		if (mixingStationsReadyToStart.Count > 0)
		{
			StartMixingStation(mixingStationsReadyToStart[0]);
			return;
		}
		List<LabOven> labOvensReadyToMove = GetLabOvensReadyToMove();
		if (labOvensReadyToMove.Count > 0)
		{
			MoveItemBehaviour.Initialize((labOvensReadyToMove[0].Configuration as LabOvenConfiguration).DestinationRoute, labOvensReadyToMove[0].OutputSlot.ItemInstance);
			MoveItemBehaviour.Enable_Networked();
			return;
		}
		List<ChemistryStation> chemStationsReadyToMove = GetChemStationsReadyToMove();
		if (chemStationsReadyToMove.Count > 0)
		{
			MoveItemBehaviour.Initialize((chemStationsReadyToMove[0].Configuration as ChemistryStationConfiguration).DestinationRoute, chemStationsReadyToMove[0].OutputSlot.ItemInstance);
			MoveItemBehaviour.Enable_Networked();
			return;
		}
		List<Cauldron> cauldronsReadyToMove = GetCauldronsReadyToMove();
		if (cauldronsReadyToMove.Count > 0)
		{
			MoveItemBehaviour.Initialize((cauldronsReadyToMove[0].Configuration as CauldronConfiguration).DestinationRoute, cauldronsReadyToMove[0].OutputSlot.ItemInstance);
			MoveItemBehaviour.Enable_Networked();
			return;
		}
		List<MixingStation> mixStationsReadyToMove = GetMixStationsReadyToMove();
		if (mixStationsReadyToMove.Count > 0)
		{
			MoveItemBehaviour.Initialize((mixStationsReadyToMove[0].Configuration as MixingStationConfiguration).DestinationRoute, mixStationsReadyToMove[0].OutputSlot.ItemInstance);
			MoveItemBehaviour.Enable_Networked();
		}
		else
		{
			SubmitNoWorkReason("There's nothing for me to do right now.", string.Empty);
			SetIdle(idle: true);
		}
	}

	protected override bool IsAnyWorkInProgress()
	{
		if (StartChemistryStationBehaviour.Active)
		{
			return true;
		}
		if (StartLabOvenBehaviour.Active)
		{
			return true;
		}
		if (FinishLabOvenBehaviour.Active)
		{
			return true;
		}
		if (MoveItemBehaviour.Active)
		{
			return true;
		}
		if (StartMixingStationBehaviour.Active)
		{
			return true;
		}
		return false;
	}

	protected override bool ShouldIdle()
	{
		if (configuration.Stations.SelectedObjects.Count == 0)
		{
			return true;
		}
		return base.ShouldIdle();
	}

	private void StartChemistryStation(ChemistryStation station)
	{
		StartChemistryStationBehaviour.SetTargetStation(station);
		StartChemistryStationBehaviour.Enable_Networked();
	}

	private void StartCauldron(Cauldron cauldron)
	{
		StartCauldronBehaviour.AssignStation(cauldron);
		StartCauldronBehaviour.Enable_Networked();
	}

	private void StartLabOven(LabOven oven)
	{
		StartLabOvenBehaviour.SetTargetOven(oven);
		StartLabOvenBehaviour.Enable_Networked();
	}

	private void FinishLabOven(LabOven oven)
	{
		FinishLabOvenBehaviour.SetTargetOven(oven);
		FinishLabOvenBehaviour.Enable_Networked();
	}

	private void StartMixingStation(MixingStation station)
	{
		StartMixingStationBehaviour.AssignStation(station);
		StartMixingStationBehaviour.Enable_Networked();
	}

	public override EmployeeHome GetHome()
	{
		return configuration.assignedHome;
	}

	public List<LabOven> GetLabOvensReadyToFinish()
	{
		List<LabOven> list = new List<LabOven>();
		foreach (LabOven labOven in configuration.LabOvens)
		{
			if (!((IUsable)labOven).IsInUse && labOven.CurrentOperation != null && labOven.IsReadyForHarvest() && labOven.CanOutputSpaceFitCurrentOperation())
			{
				list.Add(labOven);
			}
		}
		return list;
	}

	public List<LabOven> GetLabOvensReadyToStart()
	{
		List<LabOven> list = new List<LabOven>();
		foreach (LabOven labOven in configuration.LabOvens)
		{
			if (!((IUsable)labOven).IsInUse && labOven.CurrentOperation == null && labOven.IsReadyToStart())
			{
				list.Add(labOven);
			}
		}
		return list;
	}

	public List<ChemistryStation> GetChemistryStationsReadyToStart()
	{
		List<ChemistryStation> list = new List<ChemistryStation>();
		foreach (ChemistryStation chemStation in configuration.ChemStations)
		{
			if (!((IUsable)chemStation).IsInUse && chemStation.CurrentCookOperation == null)
			{
				StationRecipe selectedRecipe = (chemStation.Configuration as ChemistryStationConfiguration).Recipe.SelectedRecipe;
				if (!((Object)(object)selectedRecipe == (Object)null) && chemStation.HasIngredientsForRecipe(selectedRecipe))
				{
					list.Add(chemStation);
				}
			}
		}
		return list;
	}

	public List<Cauldron> GetCauldronsReadyToStart()
	{
		List<Cauldron> list = new List<Cauldron>();
		foreach (Cauldron cauldron in configuration.Cauldrons)
		{
			if (!((IUsable)cauldron).IsInUse && cauldron.RemainingCookTime <= 0 && cauldron.GetState() == Cauldron.EState.Ready)
			{
				list.Add(cauldron);
			}
		}
		return list;
	}

	public List<MixingStation> GetMixingStationsReadyToStart()
	{
		List<MixingStation> list = new List<MixingStation>();
		foreach (MixingStation mixStation in configuration.MixStations)
		{
			if (!((IUsable)mixStation).IsInUse && mixStation.CanStartMix() && mixStation.CurrentMixOperation == null && !((float)mixStation.GetMixQuantity() < (mixStation.Configuration as MixingStationConfiguration).StartThrehold.Value))
			{
				list.Add(mixStation);
			}
		}
		return list;
	}

	protected List<LabOven> GetLabOvensReadyToMove()
	{
		List<LabOven> list = new List<LabOven>();
		foreach (LabOven labOven in configuration.LabOvens)
		{
			ItemSlot outputSlot = labOven.OutputSlot;
			if (outputSlot.Quantity != 0 && MoveItemBehaviour.IsTransitRouteValid((labOven.Configuration as LabOvenConfiguration).DestinationRoute, ((BaseItemInstance)outputSlot.ItemInstance).ID))
			{
				list.Add(labOven);
			}
		}
		return list;
	}

	protected List<ChemistryStation> GetChemStationsReadyToMove()
	{
		List<ChemistryStation> list = new List<ChemistryStation>();
		foreach (ChemistryStation chemStation in configuration.ChemStations)
		{
			ItemSlot outputSlot = chemStation.OutputSlot;
			if (outputSlot.Quantity != 0 && MoveItemBehaviour.IsTransitRouteValid((chemStation.Configuration as ChemistryStationConfiguration).DestinationRoute, ((BaseItemInstance)outputSlot.ItemInstance).ID))
			{
				list.Add(chemStation);
			}
		}
		return list;
	}

	protected List<Cauldron> GetCauldronsReadyToMove()
	{
		List<Cauldron> list = new List<Cauldron>();
		foreach (Cauldron cauldron in configuration.Cauldrons)
		{
			ItemSlot outputSlot = cauldron.OutputSlot;
			if (outputSlot.Quantity != 0 && MoveItemBehaviour.IsTransitRouteValid((cauldron.Configuration as CauldronConfiguration).DestinationRoute, ((BaseItemInstance)outputSlot.ItemInstance).ID))
			{
				list.Add(cauldron);
			}
		}
		return list;
	}

	protected List<MixingStation> GetMixStationsReadyToMove()
	{
		List<MixingStation> list = new List<MixingStation>();
		foreach (MixingStation mixStation in configuration.MixStations)
		{
			ItemSlot outputSlot = mixStation.OutputSlot;
			if (outputSlot.Quantity != 0)
			{
				if (MoveItemBehaviour.IsTransitRouteValid((mixStation.Configuration as MixingStationConfiguration).DestinationRoute, ((BaseItemInstance)outputSlot.ItemInstance).ID, out var invalidReason))
				{
					list.Add(mixStation);
				}
				else if (DEBUG)
				{
					Debug.Log((object)("Mixing station " + ((Object)mixStation).name + " does not have a valid route for item " + ((BaseItemInstance)outputSlot.ItemInstance).ID + ": " + invalidReason));
				}
			}
		}
		return list;
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
		ChemistUIElement component = ((Component)Object.Instantiate<ChemistUIElement>(WorldspaceUIPrefab, (Transform)(object)assignedProperty.WorldspaceUIContainer)).GetComponent<ChemistUIElement>();
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
		return new ChemistData(ID, base.AssignedProperty.PropertyCode, FirstName, LastName, base.IsMale, base.AppearanceIndex, ((Component)this).transform.position, ((Component)this).transform.rotation, base.GUID, base.PaidForToday, MoveItemBehaviour.GetSaveData());
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
		if (!NetworkInitialize___EarlyScheduleOne_002EEmployees_002EChemistAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEmployees_002EChemistAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 2u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, CurrentPlayerConfigurer);
			((NetworkBehaviour)this).RegisterServerRpc(46u, new ServerRpcDelegate(RpcReader___Server_SetConfigurer_3323014238));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EEmployees_002EChemist));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEmployees_002EChemistAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEmployees_002EChemistAssembly_002DCSharp_002Edll_Excuted = true;
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

	public override bool ReadSyncVar___ScheduleOne_002EEmployees_002EChemist(PooledReader PooledReader0, uint UInt321, bool Boolean2)
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
