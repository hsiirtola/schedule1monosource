using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Networking;

public class ReplicationQueue : NetworkSingleton<ReplicationQueue>
{
	public class ReplicationRequest
	{
		public string TaskName;

		public NetworkConnection Target;

		public Action<NetworkConnection> Callback;

		public int ApproximateSizeBytes;

		public bool IsValid()
		{
			if (Target != (NetworkConnection)null && Callback != null)
			{
				return Target.IsValid;
			}
			return false;
		}
	}

	public const int RATE_LIMIT_BYTES_PER_SECOND = 51200;

	public const int MAX_REPLICATION_DURATION = 45;

	private Dictionary<NetworkConnection, List<ReplicationRequest>> requestsByConnection = new Dictionary<NetworkConnection, List<ReplicationRequest>>();

	private List<ReplicationRequest> queue = new List<ReplicationRequest>();

	private int currentByteBudget = 51200;

	private float timeOnLastReplicationTaskRPC;

	private float timeOnReplicationStart;

	private bool NetworkInitialize___EarlyScheduleOne_002ENetworking_002EReplicationQueueAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENetworking_002EReplicationQueueAssembly_002DCSharp_002Edll_Excuted;

	public bool ReplicationDoneForLocalPlayer { get; private set; }

	public bool LocalPlayerReplicationTimedOut
	{
		get
		{
			if (!ReplicationDoneForLocalPlayer && Time.timeSinceLevelLoad - timeOnReplicationStart >= 45f)
			{
				return true;
			}
			return false;
		}
	}

	public string CurrentReplicationTask { get; private set; } = string.Empty;

	public static void Enqueue(string taskName, NetworkConnection target, Action<NetworkConnection> callback, int approximateSizeBytes = 32)
	{
		NetworkSingleton<ReplicationQueue>.Instance.Enqueue_(taskName, target, callback, approximateSizeBytes);
	}

	public static float GetReplicationDuration(int approximateSizeBytes)
	{
		return (float)approximateSizeBytes / 51200f;
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		ReplicationDoneForLocalPlayer = true;
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		timeOnReplicationStart = Time.timeSinceLevelLoad;
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost)
		{
			((MonoBehaviour)this).StartCoroutine(WaitForReplicationComplete());
		}
		IEnumerator WaitForReplicationComplete()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => requestsByConnection.ContainsKey(connection)));
			yield return (object)new WaitUntil((Func<bool>)(() => requestsByConnection[connection].Count == 0));
			SetReplicationDone(connection);
		}
	}

	[TargetRpc]
	private void SetReplicationDone(NetworkConnection conn)
	{
		RpcWriter___Target_SetReplicationDone_328543758(conn);
	}

	[TargetRpc]
	private void SetReplicationTask(NetworkConnection conn, string task)
	{
		RpcWriter___Target_SetReplicationTask_2971853958(conn, task);
	}

	private void Enqueue_(string taskName, NetworkConnection target, Action<NetworkConnection> callback, int approximateSizeBytes = 32)
	{
		ReplicationRequest item = new ReplicationRequest
		{
			TaskName = taskName,
			Target = target,
			Callback = callback,
			ApproximateSizeBytes = approximateSizeBytes
		};
		queue.Add(item);
		if (!requestsByConnection.ContainsKey(target))
		{
			requestsByConnection[target] = new List<ReplicationRequest>();
		}
		requestsByConnection[target].Add(item);
	}

	private void Update()
	{
		if (queue.Count == 0)
		{
			return;
		}
		currentByteBudget += Mathf.RoundToInt(51200f * Time.deltaTime);
		while (queue.Count > 0 && currentByteBudget >= queue[0].ApproximateSizeBytes)
		{
			ReplicationRequest replicationRequest = queue[0];
			queue.RemoveAt(0);
			if (requestsByConnection.ContainsKey(replicationRequest.Target))
			{
				requestsByConnection[replicationRequest.Target].Remove(replicationRequest);
			}
			if (!replicationRequest.IsValid())
			{
				continue;
			}
			currentByteBudget -= replicationRequest.ApproximateSizeBytes;
			try
			{
				if (replicationRequest.Callback != null)
				{
					replicationRequest.Callback(replicationRequest.Target);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Error during replication task '" + replicationRequest.TaskName + "': " + ex.Message));
			}
			NotifyActiveReplicationTask(replicationRequest);
		}
	}

	private void NotifyActiveReplicationTask(ReplicationRequest request)
	{
		if (request != null && !(request.Target == (NetworkConnection)null) && Time.realtimeSinceStartup - timeOnLastReplicationTaskRPC > 0.1f)
		{
			SetReplicationTask(request.Target, request.TaskName);
			timeOnLastReplicationTaskRPC = Time.realtimeSinceStartup;
		}
	}

	public List<ReplicationRequest> GetRequestsForConnection(NetworkConnection conn)
	{
		if (requestsByConnection.TryGetValue(conn, out var value))
		{
			return value;
		}
		return new List<ReplicationRequest>();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENetworking_002EReplicationQueueAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENetworking_002EReplicationQueueAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterTargetRpc(0u, new ClientRpcDelegate(RpcReader___Target_SetReplicationDone_328543758));
			((NetworkBehaviour)this).RegisterTargetRpc(1u, new ClientRpcDelegate(RpcReader___Target_SetReplicationTask_2971853958));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENetworking_002EReplicationQueueAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENetworking_002EReplicationQueueAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Target_SetReplicationDone_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendTargetRpc(0u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___SetReplicationDone_328543758(NetworkConnection conn)
	{
		ReplicationDoneForLocalPlayer = true;
	}

	private void RpcReader___Target_SetReplicationDone_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetReplicationDone_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	private void RpcWriter___Target_SetReplicationTask_2971853958(NetworkConnection conn, string task)
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
			((Writer)writer).WriteString(task);
			((NetworkBehaviour)this).SendTargetRpc(1u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___SetReplicationTask_2971853958(NetworkConnection conn, string task)
	{
		CurrentReplicationTask = task;
	}

	private void RpcReader___Target_SetReplicationTask_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string task = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetReplicationTask_2971853958(((NetworkBehaviour)this).LocalConnection, task);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
