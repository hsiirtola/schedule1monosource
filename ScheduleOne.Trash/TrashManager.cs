using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Trash;

public class TrashManager : NetworkSingleton<TrashManager>, IBaseSaveable, ISaveable
{
	[Serializable]
	public class TrashItemData
	{
		public TrashItem Item;

		[Range(0f, 1f)]
		public float GenerationChance = 0.5f;
	}

	public const int TRASH_ITEM_LIMIT = 2000;

	public const int TRASH_REPLICATIONS_PER_SECOND = 100;

	public TrashItem[] TrashPrefabs;

	public TrashItem TrashBagPrefab;

	public TrashItemData[] GenerateableTrashItems;

	private List<TrashItem> trashItems = new List<TrashItem>();

	public float TrashForceMultiplier = 0.3f;

	private TrashLoader loader = new TrashLoader();

	private List<string> writtenItemFiles = new List<string>();

	private bool NetworkInitialize___EarlyScheduleOne_002ETrash_002ETrashManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ETrash_002ETrashManagerAssembly_002DCSharp_002Edll_Excuted;

	public string SaveFolderName => "Trash";

	public string SaveFileName => "Trash";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; } = true;

	public int LoadOrder { get; }

	protected override void Start()
	{
		base.Start();
		InitializeSaveable();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		int byteSizeEstimate;
		if (!connection.IsHost)
		{
			byteSizeEstimate = 120 * trashItems.Count;
			ReplicationQueue.Enqueue(((object)this).GetType().Name, connection, ReplicateAllTrash, byteSizeEstimate);
		}
		void ReplicateAllTrash(NetworkConnection conn)
		{
			Console.Log("Sending " + trashItems.Count + " trash items to new player");
			Singleton<StaggeredCallbackUtility>.Instance.InvokeStaggered(trashItems.Count, ReplicationQueue.GetReplicationDuration(byteSizeEstimate), ReplicateTrashItem);
		}
		void ReplicateTrashItem(int i)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			TrashItem trashItem = trashItems[i];
			CreateTrashItem(connection, trashItem.ID, ((Component)trashItem).transform.position, ((Component)trashItem).transform.rotation, Vector3.zero, null, trashItem.GUID.ToString());
		}
	}

	public void ReplicateTransformData(TrashItem trash)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		SendTransformData(trash.GUID.ToString(), ((Component)trash).transform.position, ((Component)trash).transform.rotation, trash.Rigidbody.velocity, ((NetworkBehaviour)Player.Local).LocalConnection);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendTransformData(string guid, Vector3 position, Quaternion rotation, Vector3 velocity, NetworkConnection sender)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_SendTransformData_2990100769(guid, position, rotation, velocity, sender);
	}

	[ObserversRpc]
	private void ReceiveTransformData(string guid, Vector3 position, Quaternion rotation, Vector3 velocity, NetworkConnection sender)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_ReceiveTransformData_2990100769(guid, position, rotation, velocity, sender);
	}

	public TrashItem CreateTrashItem(string id, Vector3 posiiton, Quaternion rotation, Vector3 initialVelocity = default(Vector3), string guid = "", bool startKinematic = false)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (guid == "")
		{
			guid = Guid.NewGuid().ToString();
		}
		SendTrashItem(id, posiiton, rotation, initialVelocity, ((NetworkBehaviour)Player.Local).LocalConnection, guid);
		return CreateAndReturnTrashItem(id, posiiton, rotation, initialVelocity, guid, startKinematic);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendTrashItem(string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_SendTrashItem_478112418(id, position, rotation, initialVelocity, sender, guid, startKinematic);
	}

	[ObserversRpc]
	[TargetRpc]
	private void CreateTrashItem(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (conn == null)
		{
			RpcWriter___Observers_CreateTrashItem_2385526393(conn, id, position, rotation, initialVelocity, sender, guid, startKinematic);
		}
		else
		{
			RpcWriter___Target_CreateTrashItem_2385526393(conn, id, position, rotation, initialVelocity, sender, guid, startKinematic);
		}
	}

	private TrashItem CreateAndReturnTrashItem(string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, string guid, bool startKinematic)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		TrashItem trashPrefab = GetTrashPrefab(id);
		if ((Object)(object)trashPrefab == (Object)null)
		{
			Debug.LogError((object)("Trash item with ID " + id + " not found."));
			return null;
		}
		if (GUIDManager.IsGUIDAlreadyRegistered(new Guid(guid)))
		{
			return null;
		}
		trashPrefab.Draggable.CreateCoM = false;
		((Component)trashPrefab).GetComponent<PhysicsDamageable>().ForceMultiplier = TrashForceMultiplier;
		TrashItem trashItem = Object.Instantiate<TrashItem>(trashPrefab, position, rotation, NetworkSingleton<GameManager>.Instance.Temp);
		trashItem.SetGUID(new Guid(guid));
		if (!startKinematic)
		{
			trashItem.SetContinuousCollisionDetection();
		}
		if (initialVelocity != default(Vector3))
		{
			trashItem.SetVelocity(initialVelocity);
		}
		trashItems.Add(trashItem);
		HasChanged = true;
		return trashItem;
	}

	public TrashItem CreateTrashBag(string id, Vector3 posiiton, Quaternion rotation, TrashContentData content, Vector3 initialVelocity = default(Vector3), string guid = "", bool startKinematic = false)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (guid == "")
		{
			guid = Guid.NewGuid().ToString();
		}
		SendTrashBag(id, posiiton, rotation, content, initialVelocity, ((NetworkBehaviour)Player.Local).LocalConnection, guid);
		return CreateAndReturnTrashBag(id, posiiton, rotation, content, initialVelocity, guid, startKinematic);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendTrashBag(string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_SendTrashBag_3965031115(id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
	}

	[ObserversRpc]
	[TargetRpc]
	private void CreateTrashBag(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (conn == null)
		{
			RpcWriter___Observers_CreateTrashBag_680856992(conn, id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
		}
		else
		{
			RpcWriter___Target_CreateTrashBag_680856992(conn, id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
		}
	}

	private TrashItem CreateAndReturnTrashBag(string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, string guid, bool startKinematic)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		TrashBag trashBag = GetTrashPrefab(id) as TrashBag;
		if ((Object)(object)trashBag == (Object)null)
		{
			Debug.LogError((object)("Trash item with ID " + id + " not found."));
			return null;
		}
		TrashBag trashBag2 = Object.Instantiate<TrashBag>(trashBag, position, rotation, NetworkSingleton<GameManager>.Instance.Temp);
		trashBag2.SetGUID(new Guid(guid));
		trashBag2.LoadContent(content);
		if (!startKinematic)
		{
			trashBag2.SetContinuousCollisionDetection();
		}
		if (initialVelocity != default(Vector3))
		{
			trashBag2.SetVelocity(initialVelocity);
		}
		trashItems.Add(trashBag2);
		HasChanged = true;
		return trashBag2;
	}

	public void DestroyAllTrash()
	{
		if (InstanceFinder.IsServer)
		{
			List<TrashItem> list = new List<TrashItem>();
			list.AddRange(trashItems);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].DestroyTrash();
			}
		}
	}

	public void DestroyTrash(TrashItem trash)
	{
		SendDestroyTrash(trash.GUID.ToString());
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void SendDestroyTrash(string guid)
	{
		RpcWriter___Server_SendDestroyTrash_3615296227(guid);
		RpcLogic___SendDestroyTrash_3615296227(guid);
	}

	[ObserversRpc(RunLocally = true)]
	private void DestroyTrash(string guid)
	{
		RpcWriter___Observers_DestroyTrash_3615296227(guid);
		RpcLogic___DestroyTrash_3615296227(guid);
	}

	public TrashItem GetTrashPrefab(string id)
	{
		return TrashPrefabs.FirstOrDefault((TrashItem t) => t.ID == id);
	}

	public TrashItem GetRandomGeneratableTrashPrefab()
	{
		float num = GenerateableTrashItems.Sum((TrashItemData t) => t.GenerationChance);
		float num2 = Random.Range(0f, num);
		TrashItemData[] generateableTrashItems = GenerateableTrashItems;
		foreach (TrashItemData trashItemData in generateableTrashItems)
		{
			if (num2 < trashItemData.GenerationChance)
			{
				return trashItemData.Item;
			}
			num2 -= trashItemData.GenerationChance;
		}
		return GenerateableTrashItems[GenerateableTrashItems.Length - 1].Item;
	}

	public virtual string GetSaveString()
	{
		List<ScheduleOne.Persistence.Datas.TrashItemData> list = new List<ScheduleOne.Persistence.Datas.TrashItemData>();
		for (int i = 0; i < trashItems.Count && i < 2000; i++)
		{
			list.Add(trashItems[i].GetData());
		}
		List<TrashGeneratorData> list2 = new List<TrashGeneratorData>();
		foreach (TrashGenerator allGenerator in TrashGenerator.AllGenerators)
		{
			if (allGenerator.ShouldSave())
			{
				list2.Add(allGenerator.GetSaveData());
			}
		}
		return new TrashData(list.ToArray(), list2.ToArray()).GetJson(prettyPrint: false);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ETrash_002ETrashManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ETrash_002ETrashManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendTransformData_2990100769));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_ReceiveTransformData_2990100769));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_SendTrashItem_478112418));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_CreateTrashItem_2385526393));
			((NetworkBehaviour)this).RegisterTargetRpc(4u, new ClientRpcDelegate(RpcReader___Target_CreateTrashItem_2385526393));
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_SendTrashBag_3965031115));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_CreateTrashBag_680856992));
			((NetworkBehaviour)this).RegisterTargetRpc(7u, new ClientRpcDelegate(RpcReader___Target_CreateTrashBag_680856992));
			((NetworkBehaviour)this).RegisterServerRpc(8u, new ServerRpcDelegate(RpcReader___Server_SendDestroyTrash_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(9u, new ClientRpcDelegate(RpcReader___Observers_DestroyTrash_3615296227));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ETrash_002ETrashManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ETrash_002ETrashManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendTransformData_2990100769(string guid, Vector3 position, Quaternion rotation, Vector3 velocity, NetworkConnection sender)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((Writer)writer).WriteVector3(velocity);
			((Writer)writer).WriteNetworkConnection(sender);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendTransformData_2990100769(string guid, Vector3 position, Quaternion rotation, Vector3 velocity, NetworkConnection sender)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		ReceiveTransformData(guid, position, rotation, velocity, sender);
	}

	private void RpcReader___Server_SendTransformData_2990100769(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		string guid = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		Vector3 velocity = ((Reader)PooledReader0).ReadVector3();
		NetworkConnection sender = ((Reader)PooledReader0).ReadNetworkConnection();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendTransformData_2990100769(guid, position, rotation, velocity, sender);
		}
	}

	private void RpcWriter___Observers_ReceiveTransformData_2990100769(string guid, Vector3 position, Quaternion rotation, Vector3 velocity, NetworkConnection sender)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((Writer)writer).WriteVector3(velocity);
			((Writer)writer).WriteNetworkConnection(sender);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveTransformData_2990100769(string guid, Vector3 position, Quaternion rotation, Vector3 velocity, NetworkConnection sender)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (!sender.IsLocalClient)
		{
			TrashItem trashItem = GUIDManager.GetObject<TrashItem>(new Guid(guid));
			if (!((Object)(object)trashItem == (Object)null))
			{
				((Component)trashItem).transform.position = position;
				((Component)trashItem).transform.rotation = rotation;
				trashItem.Rigidbody.velocity = velocity;
			}
		}
	}

	private void RpcReader___Observers_ReceiveTransformData_2990100769(PooledReader PooledReader0, Channel channel)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		string guid = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		Vector3 velocity = ((Reader)PooledReader0).ReadVector3();
		NetworkConnection sender = ((Reader)PooledReader0).ReadNetworkConnection();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveTransformData_2990100769(guid, position, rotation, velocity, sender);
		}
	}

	private void RpcWriter___Server_SendTrashItem_478112418(string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(id);
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((Writer)writer).WriteVector3(initialVelocity);
			((Writer)writer).WriteNetworkConnection(sender);
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteBoolean(startKinematic);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendTrashItem_478112418(string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (trashItems.Count >= 2000)
		{
			trashItems[Random.Range(0, trashItems.Count)].DestroyTrash();
		}
		CreateTrashItem(null, id, position, rotation, initialVelocity, sender, guid, startKinematic);
	}

	private void RpcReader___Server_SendTrashItem_478112418(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		string id = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		Vector3 initialVelocity = ((Reader)PooledReader0).ReadVector3();
		NetworkConnection sender = ((Reader)PooledReader0).ReadNetworkConnection();
		string guid = ((Reader)PooledReader0).ReadString();
		bool startKinematic = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendTrashItem_478112418(id, position, rotation, initialVelocity, sender, guid, startKinematic);
		}
	}

	private void RpcWriter___Observers_CreateTrashItem_2385526393(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((Writer)writer).WriteVector3(initialVelocity);
			((Writer)writer).WriteNetworkConnection(sender);
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteBoolean(startKinematic);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___CreateTrashItem_2385526393(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!sender.IsLocalClient)
		{
			CreateAndReturnTrashItem(id, position, rotation, initialVelocity, guid, startKinematic);
		}
	}

	private void RpcReader___Observers_CreateTrashItem_2385526393(PooledReader PooledReader0, Channel channel)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		string id = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		Vector3 initialVelocity = ((Reader)PooledReader0).ReadVector3();
		NetworkConnection sender = ((Reader)PooledReader0).ReadNetworkConnection();
		string guid = ((Reader)PooledReader0).ReadString();
		bool startKinematic = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___CreateTrashItem_2385526393(null, id, position, rotation, initialVelocity, sender, guid, startKinematic);
		}
	}

	private void RpcWriter___Target_CreateTrashItem_2385526393(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			((Writer)writer).WriteVector3(initialVelocity);
			((Writer)writer).WriteNetworkConnection(sender);
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteBoolean(startKinematic);
			((NetworkBehaviour)this).SendTargetRpc(4u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_CreateTrashItem_2385526393(PooledReader PooledReader0, Channel channel)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		string id = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		Vector3 initialVelocity = ((Reader)PooledReader0).ReadVector3();
		NetworkConnection sender = ((Reader)PooledReader0).ReadNetworkConnection();
		string guid = ((Reader)PooledReader0).ReadString();
		bool startKinematic = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___CreateTrashItem_2385526393(((NetworkBehaviour)this).LocalConnection, id, position, rotation, initialVelocity, sender, guid, startKinematic);
		}
	}

	private void RpcWriter___Server_SendTrashBag_3965031115(string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(id);
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerated((Writer)(object)writer, content);
			((Writer)writer).WriteVector3(initialVelocity);
			((Writer)writer).WriteNetworkConnection(sender);
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteBoolean(startKinematic);
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendTrashBag_3965031115(string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (trashItems.Count >= 2000)
		{
			trashItems[Random.Range(0, trashItems.Count)].DestroyTrash();
		}
		CreateTrashBag(null, id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
	}

	private void RpcReader___Server_SendTrashBag_3965031115(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		string id = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		TrashContentData content = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		Vector3 initialVelocity = ((Reader)PooledReader0).ReadVector3();
		NetworkConnection sender = ((Reader)PooledReader0).ReadNetworkConnection();
		string guid = ((Reader)PooledReader0).ReadString();
		bool startKinematic = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendTrashBag_3965031115(id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
		}
	}

	private void RpcWriter___Observers_CreateTrashBag_680856992(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerated((Writer)(object)writer, content);
			((Writer)writer).WriteVector3(initialVelocity);
			((Writer)writer).WriteNetworkConnection(sender);
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteBoolean(startKinematic);
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___CreateTrashBag_680856992(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!sender.IsLocalClient)
		{
			CreateAndReturnTrashBag(id, position, rotation, content, initialVelocity, guid, startKinematic);
		}
	}

	private void RpcReader___Observers_CreateTrashBag_680856992(PooledReader PooledReader0, Channel channel)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		string id = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		TrashContentData content = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		Vector3 initialVelocity = ((Reader)PooledReader0).ReadVector3();
		NetworkConnection sender = ((Reader)PooledReader0).ReadNetworkConnection();
		string guid = ((Reader)PooledReader0).ReadString();
		bool startKinematic = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___CreateTrashBag_680856992(null, id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
		}
	}

	private void RpcWriter___Target_CreateTrashBag_680856992(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteQuaternion(rotation, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerated((Writer)(object)writer, content);
			((Writer)writer).WriteVector3(initialVelocity);
			((Writer)writer).WriteNetworkConnection(sender);
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteBoolean(startKinematic);
			((NetworkBehaviour)this).SendTargetRpc(7u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_CreateTrashBag_680856992(PooledReader PooledReader0, Channel channel)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		string id = ((Reader)PooledReader0).ReadString();
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		Quaternion rotation = ((Reader)PooledReader0).ReadQuaternion((AutoPackType)1);
		TrashContentData content = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		Vector3 initialVelocity = ((Reader)PooledReader0).ReadVector3();
		NetworkConnection sender = ((Reader)PooledReader0).ReadNetworkConnection();
		string guid = ((Reader)PooledReader0).ReadString();
		bool startKinematic = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___CreateTrashBag_680856992(((NetworkBehaviour)this).LocalConnection, id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
		}
	}

	private void RpcWriter___Server_SendDestroyTrash_3615296227(string guid)
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
			((Writer)writer).WriteString(guid);
			((NetworkBehaviour)this).SendServerRpc(8u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendDestroyTrash_3615296227(string guid)
	{
		DestroyTrash(guid);
	}

	private void RpcReader___Server_SendDestroyTrash_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string guid = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendDestroyTrash_3615296227(guid);
		}
	}

	private void RpcWriter___Observers_DestroyTrash_3615296227(string guid)
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
			((Writer)writer).WriteString(guid);
			((NetworkBehaviour)this).SendObserversRpc(9u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___DestroyTrash_3615296227(string guid)
	{
		TrashItem trashItem = GUIDManager.GetObject<TrashItem>(new Guid(guid));
		if (!((Object)(object)trashItem == (Object)null))
		{
			trashItems.Remove(trashItem);
			GUIDManager.DeregisterObject(trashItem);
			if (trashItem.onDestroyed != null)
			{
				trashItem.onDestroyed(trashItem);
			}
			trashItem.Deinitialize();
			Object.Destroy((Object)(object)((Component)trashItem).gameObject);
			HasChanged = true;
		}
	}

	private void RpcReader___Observers_DestroyTrash_3615296227(PooledReader PooledReader0, Channel channel)
	{
		string guid = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___DestroyTrash_3615296227(guid);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
