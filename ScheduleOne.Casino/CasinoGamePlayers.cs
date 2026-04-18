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
using FluffyUnderware.DevTools.Extensions;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Casino;

public class CasinoGamePlayers : NetworkBehaviour
{
	public int PlayerLimit = 4;

	private Player[] Players;

	public UnityEvent onPlayerListChanged;

	public UnityEvent onPlayerScoresChanged;

	private Dictionary<Player, int> playerScores = new Dictionary<Player, int>();

	private Dictionary<Player, CasinoGamePlayerData> playerDatas = new Dictionary<Player, CasinoGamePlayerData>();

	private bool NetworkInitialize___EarlyScheduleOne_002ECasino_002ECasinoGamePlayersAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ECasino_002ECasinoGamePlayersAssembly_002DCSharp_002Edll_Excuted;

	public int CurrentPlayerCount => Players.Count((Player p) => (Object)(object)p != (Object)null);

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ECasino_002ECasinoGamePlayers_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (CurrentPlayerCount <= 0)
		{
			return;
		}
		SetPlayerList(connection, GetPlayerObjects());
		Player[] players = Players;
		foreach (Player player in players)
		{
			if (!((Object)(object)player == (Object)null) && playerScores[player] != 0)
			{
				SetPlayerScore(connection, ((NetworkBehaviour)player).NetworkObject, playerScores[player]);
			}
		}
	}

	public void AddPlayer(Player player)
	{
		RequestAddPlayer(((NetworkBehaviour)player).NetworkObject);
	}

	public void RemovePlayer(Player player)
	{
		RequestRemovePlayer(((NetworkBehaviour)player).NetworkObject);
	}

	public void SetPlayerScore(Player player, int score)
	{
		RequestSetScore(((NetworkBehaviour)player).NetworkObject, score);
	}

	public int GetPlayerScore(Player player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return 0;
		}
		if (playerScores.ContainsKey(player))
		{
			return playerScores[player];
		}
		return 0;
	}

	public Player GetPlayer(int index)
	{
		if (index < Players.Length)
		{
			return Players[index];
		}
		return null;
	}

	public int GetPlayerIndex(Player player)
	{
		return ArrayExt.IndexOf<Player>(Players, player);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void RequestAddPlayer(NetworkObject playerObject)
	{
		RpcWriter___Server_RequestAddPlayer_3323014238(playerObject);
		RpcLogic___RequestAddPlayer_3323014238(playerObject);
	}

	private void AddPlayerToArray(Player player)
	{
		for (int i = 0; i < PlayerLimit; i++)
		{
			if ((Object)(object)Players[i] == (Object)null)
			{
				Players[i] = player;
				break;
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void RequestRemovePlayer(NetworkObject playerObject)
	{
		RpcWriter___Server_RequestRemovePlayer_3323014238(playerObject);
	}

	private void RemovePlayerFromArray(Player player)
	{
		for (int i = 0; i < PlayerLimit; i++)
		{
			if ((Object)(object)Players[i] == (Object)(object)player)
			{
				Players[i] = null;
				break;
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void RequestSetScore(NetworkObject playerObject, int score)
	{
		RpcWriter___Server_RequestSetScore_4172557123(playerObject, score);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetPlayerScore(NetworkConnection conn, NetworkObject playerObject, int score)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetPlayerScore_1865307316(conn, playerObject, score);
			RpcLogic___SetPlayerScore_1865307316(conn, playerObject, score);
		}
		else
		{
			RpcWriter___Target_SetPlayerScore_1865307316(conn, playerObject, score);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetPlayerList(NetworkConnection conn, NetworkObject[] playerObjects)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetPlayerList_204172449(conn, playerObjects);
			RpcLogic___SetPlayerList_204172449(conn, playerObjects);
		}
		else
		{
			RpcWriter___Target_SetPlayerList_204172449(conn, playerObjects);
		}
	}

	public CasinoGamePlayerData GetPlayerData()
	{
		return GetPlayerData(Player.Local);
	}

	public CasinoGamePlayerData GetPlayerData(Player player)
	{
		if (!playerDatas.ContainsKey(player))
		{
			playerDatas.Add(player, new CasinoGamePlayerData(this, player));
		}
		return playerDatas[player];
	}

	public CasinoGamePlayerData GetPlayerData(int index)
	{
		if (index < Players.Length && (Object)(object)Players[index] != (Object)null)
		{
			return GetPlayerData(Players[index]);
		}
		return null;
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendPlayerBool(NetworkObject playerObject, string key, bool value)
	{
		RpcWriter___Server_SendPlayerBool_77262511(playerObject, key, value);
		RpcLogic___SendPlayerBool_77262511(playerObject, key, value);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void ReceivePlayerBool(NetworkConnection conn, NetworkObject playerObject, string key, bool value)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ReceivePlayerBool_1748594478(conn, playerObject, key, value);
			RpcLogic___ReceivePlayerBool_1748594478(conn, playerObject, key, value);
		}
		else
		{
			RpcWriter___Target_ReceivePlayerBool_1748594478(conn, playerObject, key, value);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendPlayerFloat(NetworkObject playerObject, string key, float value)
	{
		RpcWriter___Server_SendPlayerFloat_2931762093(playerObject, key, value);
		RpcLogic___SendPlayerFloat_2931762093(playerObject, key, value);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void ReceivePlayerFloat(NetworkConnection conn, NetworkObject playerObject, string key, float value)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ReceivePlayerFloat_2317689966(conn, playerObject, key, value);
			RpcLogic___ReceivePlayerFloat_2317689966(conn, playerObject, key, value);
		}
		else
		{
			RpcWriter___Target_ReceivePlayerFloat_2317689966(conn, playerObject, key, value);
		}
	}

	private NetworkObject[] GetPlayerObjects()
	{
		NetworkObject[] array = (NetworkObject[])(object)new NetworkObject[PlayerLimit];
		for (int i = 0; i < PlayerLimit; i++)
		{
			if ((Object)(object)Players[i] != (Object)null)
			{
				array[i] = ((NetworkBehaviour)Players[i]).NetworkObject;
			}
		}
		return array;
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
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ECasino_002ECasinoGamePlayersAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ECasino_002ECasinoGamePlayersAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_RequestAddPlayer_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(1u, new ServerRpcDelegate(RpcReader___Server_RequestRemovePlayer_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_RequestSetScore_4172557123));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_SetPlayerScore_1865307316));
			((NetworkBehaviour)this).RegisterTargetRpc(4u, new ClientRpcDelegate(RpcReader___Target_SetPlayerScore_1865307316));
			((NetworkBehaviour)this).RegisterObserversRpc(5u, new ClientRpcDelegate(RpcReader___Observers_SetPlayerList_204172449));
			((NetworkBehaviour)this).RegisterTargetRpc(6u, new ClientRpcDelegate(RpcReader___Target_SetPlayerList_204172449));
			((NetworkBehaviour)this).RegisterServerRpc(7u, new ServerRpcDelegate(RpcReader___Server_SendPlayerBool_77262511));
			((NetworkBehaviour)this).RegisterObserversRpc(8u, new ClientRpcDelegate(RpcReader___Observers_ReceivePlayerBool_1748594478));
			((NetworkBehaviour)this).RegisterTargetRpc(9u, new ClientRpcDelegate(RpcReader___Target_ReceivePlayerBool_1748594478));
			((NetworkBehaviour)this).RegisterServerRpc(10u, new ServerRpcDelegate(RpcReader___Server_SendPlayerFloat_2931762093));
			((NetworkBehaviour)this).RegisterObserversRpc(11u, new ClientRpcDelegate(RpcReader___Observers_ReceivePlayerFloat_2317689966));
			((NetworkBehaviour)this).RegisterTargetRpc(12u, new ClientRpcDelegate(RpcReader___Target_ReceivePlayerFloat_2317689966));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ECasino_002ECasinoGamePlayersAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ECasino_002ECasinoGamePlayersAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_RequestAddPlayer_3323014238(NetworkObject playerObject)
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
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___RequestAddPlayer_3323014238(NetworkObject playerObject)
	{
		Player component = ((Component)playerObject).GetComponent<Player>();
		if ((Object)(object)component != (Object)null && !Players.Contains(component))
		{
			AddPlayerToArray(component);
			if (!playerScores.ContainsKey(component))
			{
				playerScores.Add(component, 0);
			}
			if (!playerDatas.ContainsKey(component))
			{
				playerDatas.Add(component, new CasinoGamePlayerData(this, component));
			}
		}
		SetPlayerList(null, GetPlayerObjects());
	}

	private void RpcReader___Server_RequestAddPlayer_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject playerObject = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___RequestAddPlayer_3323014238(playerObject);
		}
	}

	private void RpcWriter___Server_RequestRemovePlayer_3323014238(NetworkObject playerObject)
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
			((NetworkBehaviour)this).SendServerRpc(1u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___RequestRemovePlayer_3323014238(NetworkObject playerObject)
	{
		Player component = ((Component)playerObject).GetComponent<Player>();
		if ((Object)(object)component != (Object)null && Players.Contains(component))
		{
			RemovePlayerFromArray(component);
		}
		SetPlayerList(null, GetPlayerObjects());
	}

	private void RpcReader___Server_RequestRemovePlayer_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject playerObject = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___RequestRemovePlayer_3323014238(playerObject);
		}
	}

	private void RpcWriter___Server_RequestSetScore_4172557123(NetworkObject playerObject, int score)
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
			((Writer)writer).WriteNetworkObject(playerObject);
			((Writer)writer).WriteInt32(score, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___RequestSetScore_4172557123(NetworkObject playerObject, int score)
	{
		SetPlayerScore(null, playerObject, score);
	}

	private void RpcReader___Server_RequestSetScore_4172557123(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject playerObject = ((Reader)PooledReader0).ReadNetworkObject();
		int score = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___RequestSetScore_4172557123(playerObject, score);
		}
	}

	private void RpcWriter___Observers_SetPlayerScore_1865307316(NetworkConnection conn, NetworkObject playerObject, int score)
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
			((Writer)writer).WriteNetworkObject(playerObject);
			((Writer)writer).WriteInt32(score, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetPlayerScore_1865307316(NetworkConnection conn, NetworkObject playerObject, int score)
	{
		Player component = ((Component)playerObject).GetComponent<Player>();
		if (!((Object)(object)component == (Object)null))
		{
			if (!playerScores.ContainsKey(component))
			{
				playerScores.Add(component, score);
			}
			else
			{
				playerScores[component] = score;
			}
			if (onPlayerScoresChanged != null)
			{
				onPlayerScoresChanged.Invoke();
			}
		}
	}

	private void RpcReader___Observers_SetPlayerScore_1865307316(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject playerObject = ((Reader)PooledReader0).ReadNetworkObject();
		int score = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetPlayerScore_1865307316(null, playerObject, score);
		}
	}

	private void RpcWriter___Target_SetPlayerScore_1865307316(NetworkConnection conn, NetworkObject playerObject, int score)
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
			((Writer)writer).WriteNetworkObject(playerObject);
			((Writer)writer).WriteInt32(score, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(4u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetPlayerScore_1865307316(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject playerObject = ((Reader)PooledReader0).ReadNetworkObject();
		int score = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetPlayerScore_1865307316(((NetworkBehaviour)this).LocalConnection, playerObject, score);
		}
	}

	private void RpcWriter___Observers_SetPlayerList_204172449(NetworkConnection conn, NetworkObject[] playerObjects)
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
			GeneratedWriters___Internal.Write___FishNet_002EObject_002ENetworkObject_005B_005DFishNet_002ESerializing_002EGenerated((Writer)(object)writer, playerObjects);
			((NetworkBehaviour)this).SendObserversRpc(5u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetPlayerList_204172449(NetworkConnection conn, NetworkObject[] playerObjects)
	{
		Players = new Player[PlayerLimit];
		for (int i = 0; i < PlayerLimit; i++)
		{
			Players[i] = null;
		}
		for (int j = 0; j < playerObjects.Length; j++)
		{
			if ((Object)(object)playerObjects[j] == (Object)null)
			{
				continue;
			}
			Player component = ((Component)playerObjects[j]).GetComponent<Player>();
			if ((Object)(object)component != (Object)null)
			{
				Players[j] = component;
				if (!playerScores.ContainsKey(component))
				{
					playerScores.Add(component, 0);
				}
				if (!playerDatas.ContainsKey(component))
				{
					playerDatas.Add(component, new CasinoGamePlayerData(this, component));
				}
			}
		}
		if (onPlayerListChanged != null)
		{
			onPlayerListChanged.Invoke();
		}
	}

	private void RpcReader___Observers_SetPlayerList_204172449(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject[] playerObjects = GeneratedReaders___Internal.Read___FishNet_002EObject_002ENetworkObject_005B_005DFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetPlayerList_204172449(null, playerObjects);
		}
	}

	private void RpcWriter___Target_SetPlayerList_204172449(NetworkConnection conn, NetworkObject[] playerObjects)
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
			GeneratedWriters___Internal.Write___FishNet_002EObject_002ENetworkObject_005B_005DFishNet_002ESerializing_002EGenerated((Writer)(object)writer, playerObjects);
			((NetworkBehaviour)this).SendTargetRpc(6u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetPlayerList_204172449(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject[] playerObjects = GeneratedReaders___Internal.Read___FishNet_002EObject_002ENetworkObject_005B_005DFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetPlayerList_204172449(((NetworkBehaviour)this).LocalConnection, playerObjects);
		}
	}

	private void RpcWriter___Server_SendPlayerBool_77262511(NetworkObject playerObject, string key, bool value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(key);
			((Writer)writer).WriteBoolean(value);
			((NetworkBehaviour)this).SendServerRpc(7u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendPlayerBool_77262511(NetworkObject playerObject, string key, bool value)
	{
		ReceivePlayerBool(null, playerObject, key, value);
	}

	private void RpcReader___Server_SendPlayerBool_77262511(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject playerObject = ((Reader)PooledReader0).ReadNetworkObject();
		string key = ((Reader)PooledReader0).ReadString();
		bool value = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendPlayerBool_77262511(playerObject, key, value);
		}
	}

	private void RpcWriter___Observers_ReceivePlayerBool_1748594478(NetworkConnection conn, NetworkObject playerObject, string key, bool value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkObject(playerObject);
			((Writer)writer).WriteString(key);
			((Writer)writer).WriteBoolean(value);
			((NetworkBehaviour)this).SendObserversRpc(8u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceivePlayerBool_1748594478(NetworkConnection conn, NetworkObject playerObject, string key, bool value)
	{
		Player component = ((Component)playerObject).GetComponent<Player>();
		if (!((Object)(object)component == (Object)null))
		{
			if (!playerDatas.ContainsKey(component))
			{
				playerDatas.Add(component, new CasinoGamePlayerData(this, component));
			}
			playerDatas[component].SetData(key, value, network: false);
		}
	}

	private void RpcReader___Observers_ReceivePlayerBool_1748594478(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject playerObject = ((Reader)PooledReader0).ReadNetworkObject();
		string key = ((Reader)PooledReader0).ReadString();
		bool value = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceivePlayerBool_1748594478(null, playerObject, key, value);
		}
	}

	private void RpcWriter___Target_ReceivePlayerBool_1748594478(NetworkConnection conn, NetworkObject playerObject, string key, bool value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkObject(playerObject);
			((Writer)writer).WriteString(key);
			((Writer)writer).WriteBoolean(value);
			((NetworkBehaviour)this).SendTargetRpc(9u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ReceivePlayerBool_1748594478(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject playerObject = ((Reader)PooledReader0).ReadNetworkObject();
		string key = ((Reader)PooledReader0).ReadString();
		bool value = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceivePlayerBool_1748594478(((NetworkBehaviour)this).LocalConnection, playerObject, key, value);
		}
	}

	private void RpcWriter___Server_SendPlayerFloat_2931762093(NetworkObject playerObject, string key, float value)
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
			((Writer)writer).WriteNetworkObject(playerObject);
			((Writer)writer).WriteString(key);
			((Writer)writer).WriteSingle(value, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(10u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendPlayerFloat_2931762093(NetworkObject playerObject, string key, float value)
	{
		ReceivePlayerFloat(null, playerObject, key, value);
	}

	private void RpcReader___Server_SendPlayerFloat_2931762093(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject playerObject = ((Reader)PooledReader0).ReadNetworkObject();
		string key = ((Reader)PooledReader0).ReadString();
		float value = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendPlayerFloat_2931762093(playerObject, key, value);
		}
	}

	private void RpcWriter___Observers_ReceivePlayerFloat_2317689966(NetworkConnection conn, NetworkObject playerObject, string key, float value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkObject(playerObject);
			((Writer)writer).WriteString(key);
			((Writer)writer).WriteSingle(value, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(11u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceivePlayerFloat_2317689966(NetworkConnection conn, NetworkObject playerObject, string key, float value)
	{
		Player component = ((Component)playerObject).GetComponent<Player>();
		if (!((Object)(object)component == (Object)null))
		{
			if (!playerDatas.ContainsKey(component))
			{
				playerDatas.Add(component, new CasinoGamePlayerData(this, component));
			}
			playerDatas[component].SetData(key, value, network: false);
		}
	}

	private void RpcReader___Observers_ReceivePlayerFloat_2317689966(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject playerObject = ((Reader)PooledReader0).ReadNetworkObject();
		string key = ((Reader)PooledReader0).ReadString();
		float value = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceivePlayerFloat_2317689966(null, playerObject, key, value);
		}
	}

	private void RpcWriter___Target_ReceivePlayerFloat_2317689966(NetworkConnection conn, NetworkObject playerObject, string key, float value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkObject(playerObject);
			((Writer)writer).WriteString(key);
			((Writer)writer).WriteSingle(value, (AutoPackType)0);
			((NetworkBehaviour)this).SendTargetRpc(12u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ReceivePlayerFloat_2317689966(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject playerObject = ((Reader)PooledReader0).ReadNetworkObject();
		string key = ((Reader)PooledReader0).ReadString();
		float value = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceivePlayerFloat_2317689966(((NetworkBehaviour)this).LocalConnection, playerObject, key, value);
		}
	}

	private void Awake_UserLogic_ScheduleOne_002ECasino_002ECasinoGamePlayers_Assembly_002DCSharp_002Edll()
	{
		Players = new Player[PlayerLimit];
	}
}
