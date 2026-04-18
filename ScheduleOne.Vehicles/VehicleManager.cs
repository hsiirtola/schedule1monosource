using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Math;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScheduleOne.Vehicles;

public class VehicleManager : NetworkSingleton<VehicleManager>, IBaseSaveable, ISaveable
{
	public List<LandVehicle> AllVehicles = new List<LandVehicle>();

	[Header("Vehicles")]
	public List<LandVehicle> VehiclePrefabs = new List<LandVehicle>();

	public List<LandVehicle> PlayerOwnedVehicles = new List<LandVehicle>();

	private VehiclesLoader loader = new VehiclesLoader();

	private bool NetworkInitialize___EarlyScheduleOne_002EVehicles_002EVehicleManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EVehicles_002EVehicleManagerAssembly_002DCSharp_002Edll_Excuted;

	public string SaveFolderName => "OwnedVehicles";

	public string SaveFileName => "OwnedVehicles";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; } = true;

	public int LoadOrder { get; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EVehicles_002EVehicleManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		base.Start();
		PathSmoothingUtility.EnsureSplineInitialized();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SpawnVehicle(string vehicleCode, Vector3 position, Quaternion rotation, bool playerOwned)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_SpawnVehicle_3323115898(vehicleCode, position, rotation, playerOwned);
	}

	public LandVehicle SpawnAndReturnVehicle(string vehicleCode, Vector3 position, Quaternion rotation, bool playerOwned)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		LandVehicle vehiclePrefab = GetVehiclePrefab(vehicleCode);
		if ((Object)(object)vehiclePrefab == (Object)null)
		{
			Console.LogError("SpawnVehicle: '" + vehicleCode + "' is not a valid vehicle code!");
			return null;
		}
		LandVehicle component = Object.Instantiate<GameObject>(((Component)vehiclePrefab).gameObject).GetComponent<LandVehicle>();
		((Component)component).transform.position = position;
		((Component)component).transform.rotation = rotation;
		Physics.SyncTransforms();
		((NetworkBehaviour)this).NetworkObject.Spawn(((Component)component).gameObject, (NetworkConnection)null, default(Scene));
		component.SetIsPlayerOwned(null, playerOwned);
		if (playerOwned)
		{
			PlayerOwnedVehicles.Add(component);
		}
		return component;
	}

	public LandVehicle GetVehiclePrefab(string vehicleCode)
	{
		return VehiclePrefabs.Find((LandVehicle x) => x.VehicleCode.ToLower() == vehicleCode.ToLower());
	}

	public LandVehicle SpawnAndLoadVehicle(VehicleData data, string path, bool playerOwned)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		LandVehicle landVehicle = SpawnAndReturnVehicle(data.VehicleCode, data.Position, data.Rotation, playerOwned);
		landVehicle.Load(data, path);
		return landVehicle;
	}

	public void LoadVehicle(VehicleData data, string path)
	{
		LandVehicle landVehicle = GUIDManager.GetObject<LandVehicle>(new Guid(data.GUID));
		if ((Object)(object)landVehicle == (Object)null)
		{
			Console.LogError("LoadVehicle: Vehicle not found with GUID " + data.GUID);
		}
		else
		{
			landVehicle.Load(data, path);
		}
	}

	public virtual string GetSaveString()
	{
		List<VehicleData> list = new List<VehicleData>();
		for (int i = 0; i < PlayerOwnedVehicles.Count; i++)
		{
			list.Add(PlayerOwnedVehicles[i].GetVehicleData());
		}
		return new VehicleCollectionData(list.ToArray()).GetJson(prettyPrint: false);
	}

	public void SpawnLoanSharkVehicle(Vector3 position, Quaternion rot)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		LandVehicle landVehicle = NetworkSingleton<VehicleManager>.Instance.SpawnAndReturnVehicle("shitbox", position, rot, playerOwned: true);
		EnableLoanSharkVisuals(((NetworkBehaviour)landVehicle).NetworkObject);
	}

	[ObserversRpc(RunLocally = true)]
	private void EnableLoanSharkVisuals(NetworkObject veh)
	{
		RpcWriter___Observers_EnableLoanSharkVisuals_3323014238(veh);
		RpcLogic___EnableLoanSharkVisuals_3323014238(veh);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EVehicles_002EVehicleManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EVehicles_002EVehicleManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SpawnVehicle_3323115898));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_EnableLoanSharkVisuals_3323014238));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EVehicles_002EVehicleManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EVehicles_002EVehicleManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SpawnVehicle_3323115898(string vehicleCode, Vector3 position, Quaternion rotation, bool playerOwned)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(vehicleCode);
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((Writer)writer).WriteBoolean(playerOwned);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SpawnVehicle_3323115898(string vehicleCode, Vector3 position, Quaternion rotation, bool playerOwned)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		SpawnAndReturnVehicle(vehicleCode, position, rotation, playerOwned);
	}

	private void RpcReader___Server_SpawnVehicle_3323115898(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		string vehicleCode = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		bool playerOwned = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SpawnVehicle_3323115898(vehicleCode, position, rotation, playerOwned);
		}
	}

	private void RpcWriter___Observers_EnableLoanSharkVisuals_3323014238(NetworkObject veh)
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
			((Writer)writer).WriteNetworkObject(veh);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___EnableLoanSharkVisuals_3323014238(NetworkObject veh)
	{
		if ((Object)(object)veh == (Object)null)
		{
			Console.LogWarning("Vehicle not found");
		}
		else
		{
			((Component)veh).GetComponent<LoanSharkCarVisuals>().Configure(enabled: true, noteVisible: true);
		}
	}

	private void RpcReader___Observers_EnableLoanSharkVisuals_3323014238(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject veh = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___EnableLoanSharkVisuals_3323014238(veh);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EVehicles_002EVehicleManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		InitializeSaveable();
	}
}
