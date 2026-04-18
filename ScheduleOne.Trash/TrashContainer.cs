using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Trash;

public class TrashContainer : NetworkBehaviour
{
	[Header("Settings")]
	[Range(1f, 50f)]
	public int TrashCapacity = 10;

	[Header("Settings")]
	public Transform TrashBagDropLocation;

	public UnityEvent<string> onTrashAdded;

	public UnityEvent onTrashLevelChanged;

	private bool NetworkInitialize___EarlyScheduleOne_002ETrash_002ETrashContainerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ETrash_002ETrashContainerAssembly_002DCSharp_002Edll_Excuted;

	public TrashContent Content { get; protected set; } = new TrashContent();

	public int TrashLevel => Content.GetTotalSize();

	public float NormalizedTrashLevel => (float)Content.GetTotalSize() / (float)TrashCapacity;

	public virtual void AddTrash(TrashItem item)
	{
		SendTrash(item.ID, 1);
		item.DestroyTrash();
		if (InstanceFinder.IsServer)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("ContainedTrashItems", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("ContainedTrashItems") + 1f).ToString());
		}
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (Content.GetTotalSize() > 0)
		{
			ReplicationQueue.Enqueue(((object)this).GetType().Name, connection, Replicate, 36 * Content.Entries.Count);
		}
		void Replicate(NetworkConnection conn)
		{
			LoadContent(conn, Content.GetData());
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void SendTrash(string trashID, int quantity)
	{
		RpcWriter___Server_SendTrash_3643459082(trashID, quantity);
		RpcLogic___SendTrash_3643459082(trashID, quantity);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void AddTrash(NetworkConnection conn, string trashID, int quantity)
	{
		if (conn == null)
		{
			RpcWriter___Observers_AddTrash_3905681115(conn, trashID, quantity);
			RpcLogic___AddTrash_3905681115(conn, trashID, quantity);
		}
		else
		{
			RpcWriter___Target_AddTrash_3905681115(conn, trashID, quantity);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void SendClear()
	{
		RpcWriter___Server_SendClear_2166136261();
		RpcLogic___SendClear_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void Clear()
	{
		RpcWriter___Observers_Clear_2166136261();
		RpcLogic___Clear_2166136261();
	}

	[TargetRpc]
	private void LoadContent(NetworkConnection conn, TrashContentData data)
	{
		RpcWriter___Target_LoadContent_189522235(conn, data);
	}

	public void TriggerEnter(Collider other)
	{
		if (InstanceFinder.IsServer && TrashLevel < TrashCapacity)
		{
			TrashItem componentInParent = ((Component)other).GetComponentInParent<TrashItem>();
			if (!((Object)(object)componentInParent == (Object)null) && componentInParent.CanGoInContainer)
			{
				AddTrash(componentInParent);
			}
		}
	}

	public bool CanBeBagged()
	{
		return TrashLevel > 0;
	}

	public void BagTrash()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		NetworkSingleton<TrashManager>.Instance.CreateTrashBag(NetworkSingleton<TrashManager>.Instance.TrashBagPrefab.ID, TrashBagDropLocation.position, TrashBagDropLocation.rotation, Content.GetData(), TrashBagDropLocation.forward * 3f);
		SendClear();
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("TrashContainersBagged", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("TrashContainersBagged") + 1f).ToString());
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ETrash_002ETrashContainerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ETrash_002ETrashContainerAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendTrash_3643459082));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_AddTrash_3905681115));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_AddTrash_3905681115));
			((NetworkBehaviour)this).RegisterServerRpc(3u, new ServerRpcDelegate(RpcReader___Server_SendClear_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_Clear_2166136261));
			((NetworkBehaviour)this).RegisterTargetRpc(5u, new ClientRpcDelegate(RpcReader___Target_LoadContent_189522235));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ETrash_002ETrashContainerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ETrash_002ETrashContainerAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendTrash_3643459082(string trashID, int quantity)
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
			((Writer)writer).WriteString(trashID);
			((Writer)writer).WriteInt32(quantity, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendTrash_3643459082(string trashID, int quantity)
	{
		AddTrash(null, trashID, quantity);
	}

	private void RpcReader___Server_SendTrash_3643459082(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string trashID = ((Reader)PooledReader0).ReadString();
		int quantity = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendTrash_3643459082(trashID, quantity);
		}
	}

	private void RpcWriter___Observers_AddTrash_3905681115(NetworkConnection conn, string trashID, int quantity)
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
			((Writer)writer).WriteString(trashID);
			((Writer)writer).WriteInt32(quantity, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___AddTrash_3905681115(NetworkConnection conn, string trashID, int quantity)
	{
		Content.AddTrash(trashID, quantity);
		if (onTrashAdded != null)
		{
			onTrashAdded.Invoke(trashID);
		}
		if (onTrashLevelChanged != null)
		{
			onTrashLevelChanged.Invoke();
		}
	}

	private void RpcReader___Observers_AddTrash_3905681115(PooledReader PooledReader0, Channel channel)
	{
		string trashID = ((Reader)PooledReader0).ReadString();
		int quantity = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___AddTrash_3905681115(null, trashID, quantity);
		}
	}

	private void RpcWriter___Target_AddTrash_3905681115(NetworkConnection conn, string trashID, int quantity)
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
			((Writer)writer).WriteString(trashID);
			((Writer)writer).WriteInt32(quantity, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(2u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_AddTrash_3905681115(PooledReader PooledReader0, Channel channel)
	{
		string trashID = ((Reader)PooledReader0).ReadString();
		int quantity = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___AddTrash_3905681115(((NetworkBehaviour)this).LocalConnection, trashID, quantity);
		}
	}

	private void RpcWriter___Server_SendClear_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(3u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendClear_2166136261()
	{
		Clear();
	}

	private void RpcReader___Server_SendClear_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendClear_2166136261();
		}
	}

	private void RpcWriter___Observers_Clear_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Clear_2166136261()
	{
		Content.Clear();
		if (onTrashLevelChanged != null)
		{
			onTrashLevelChanged.Invoke();
		}
	}

	private void RpcReader___Observers_Clear_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Clear_2166136261();
		}
	}

	private void RpcWriter___Target_LoadContent_189522235(NetworkConnection conn, TrashContentData data)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerated((Writer)(object)writer, data);
			((NetworkBehaviour)this).SendTargetRpc(5u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___LoadContent_189522235(NetworkConnection conn, TrashContentData data)
	{
		Content.LoadFromData(data);
		if (onTrashLevelChanged != null)
		{
			onTrashLevelChanged.Invoke();
		}
	}

	private void RpcReader___Target_LoadContent_189522235(PooledReader PooledReader0, Channel channel)
	{
		TrashContentData data = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___LoadContent_189522235(((NetworkBehaviour)this).LocalConnection, data);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}
}
