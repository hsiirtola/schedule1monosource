using System;
using System.Collections;
using System.Collections.Generic;
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
using ScheduleOne.NPCs;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Phone.Messages;
using UnityEngine;

namespace ScheduleOne.Messaging;

public class MessagingManager : NetworkSingleton<MessagingManager>
{
	protected Dictionary<NPC, MSGConversation> ConversationMap = new Dictionary<NPC, MSGConversation>();

	private bool NetworkInitialize___EarlyScheduleOne_002EMessaging_002EMessagingManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EMessaging_002EMessagingManagerAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EMessaging_002EMessagingManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (connection.IsLocalClient)
		{
			return;
		}
		foreach (NPC key in ConversationMap.Keys)
		{
			if (ConversationMap[key].ShouldReplicate())
			{
				NPC npcCache = key;
				ReplicationQueue.Enqueue(((object)this).GetType().Name, connection, delegate(NetworkConnection conn)
				{
					Replicate(npcCache, conn);
				}, ConversationMap[key].GetReplicationByteSize());
			}
		}
		void Replicate(NPC npc, NetworkConnection conn)
		{
			((MonoBehaviour)this).StartCoroutine(SendMessages());
			IEnumerator SendMessages()
			{
				yield return (object)new WaitUntil((Func<bool>)(() => (Object)(object)Player.GetPlayer(conn) != (Object)null));
				if (ConversationMap[npc].ShouldReplicate())
				{
					MSGConversationData saveData = ConversationMap[npc].GetSaveData();
					ReceiveMSGConversationData(conn, npc.ID, saveData);
				}
			}
		}
	}

	public MSGConversation GetConversation(NPC npc)
	{
		if (!ConversationMap.ContainsKey(npc))
		{
			Console.LogError("No conversation found for " + npc.fullName);
			return null;
		}
		return ConversationMap[npc];
	}

	public void Register(NPC npc, MSGConversation convs)
	{
		if (ConversationMap.ContainsKey(npc))
		{
			Console.LogError("Conversation already registered for " + npc.fullName);
		}
		else
		{
			ConversationMap.Add(npc, convs);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendMessage(Message m, bool notify, string npcID)
	{
		RpcWriter___Server_SendMessage_2134336246(m, notify, npcID);
		RpcLogic___SendMessage_2134336246(m, notify, npcID);
	}

	[ObserversRpc(RunLocally = true)]
	private void ReceiveMessage(Message m, bool notify, string npcID)
	{
		RpcWriter___Observers_ReceiveMessage_2134336246(m, notify, npcID);
		RpcLogic___ReceiveMessage_2134336246(m, notify, npcID);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendMessageChain(MessageChain m, string npcID, float initialDelay, bool notify)
	{
		RpcWriter___Server_SendMessageChain_3949292778(m, npcID, initialDelay, notify);
		RpcLogic___SendMessageChain_3949292778(m, npcID, initialDelay, notify);
	}

	[ObserversRpc(RunLocally = true)]
	private void ReceiveMessageChain(MessageChain m, string npcID, float initialDelay, bool notify)
	{
		RpcWriter___Observers_ReceiveMessageChain_3949292778(m, npcID, initialDelay, notify);
		RpcLogic___ReceiveMessageChain_3949292778(m, npcID, initialDelay, notify);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendResponse(int responseIndex, string npcID)
	{
		RpcWriter___Server_SendResponse_2801973956(responseIndex, npcID);
		RpcLogic___SendResponse_2801973956(responseIndex, npcID);
	}

	[ObserversRpc(RunLocally = true)]
	private void ReceiveResponse(int responseIndex, string npcID)
	{
		RpcWriter___Observers_ReceiveResponse_2801973956(responseIndex, npcID);
		RpcLogic___ReceiveResponse_2801973956(responseIndex, npcID);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendPlayerMessage(int sendableIndex, int sentIndex, string npcID)
	{
		RpcWriter___Server_SendPlayerMessage_1952281135(sendableIndex, sentIndex, npcID);
		RpcLogic___SendPlayerMessage_1952281135(sendableIndex, sentIndex, npcID);
	}

	[ObserversRpc(RunLocally = true)]
	private void ReceivePlayerMessage(int sendableIndex, int sentIndex, string npcID)
	{
		RpcWriter___Observers_ReceivePlayerMessage_1952281135(sendableIndex, sentIndex, npcID);
		RpcLogic___ReceivePlayerMessage_1952281135(sendableIndex, sentIndex, npcID);
	}

	[TargetRpc]
	private void ReceiveMSGConversationData(NetworkConnection conn, string npcID, MSGConversationData data)
	{
		RpcWriter___Target_ReceiveMSGConversationData_2662241369(conn, npcID, data);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ClearResponses(string npcID)
	{
		RpcWriter___Server_ClearResponses_3615296227(npcID);
	}

	[ObserversRpc]
	private void ReceiveClearResponses(string npcID)
	{
		RpcWriter___Observers_ReceiveClearResponses_3615296227(npcID);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void ShowResponses(string npcID, List<Response> responses, float delay)
	{
		RpcWriter___Server_ShowResponses_995803534(npcID, responses, delay);
		RpcLogic___ShowResponses_995803534(npcID, responses, delay);
	}

	[ObserversRpc(RunLocally = true)]
	private void ReceiveShowResponses(string npcID, List<Response> responses, float delay)
	{
		RpcWriter___Observers_ReceiveShowResponses_995803534(npcID, responses, delay);
		RpcLogic___ReceiveShowResponses_995803534(npcID, responses, delay);
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
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EMessaging_002EMessagingManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EMessaging_002EMessagingManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendMessage_2134336246));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_ReceiveMessage_2134336246));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_SendMessageChain_3949292778));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_ReceiveMessageChain_3949292778));
			((NetworkBehaviour)this).RegisterServerRpc(4u, new ServerRpcDelegate(RpcReader___Server_SendResponse_2801973956));
			((NetworkBehaviour)this).RegisterObserversRpc(5u, new ClientRpcDelegate(RpcReader___Observers_ReceiveResponse_2801973956));
			((NetworkBehaviour)this).RegisterServerRpc(6u, new ServerRpcDelegate(RpcReader___Server_SendPlayerMessage_1952281135));
			((NetworkBehaviour)this).RegisterObserversRpc(7u, new ClientRpcDelegate(RpcReader___Observers_ReceivePlayerMessage_1952281135));
			((NetworkBehaviour)this).RegisterTargetRpc(8u, new ClientRpcDelegate(RpcReader___Target_ReceiveMSGConversationData_2662241369));
			((NetworkBehaviour)this).RegisterServerRpc(9u, new ServerRpcDelegate(RpcReader___Server_ClearResponses_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(10u, new ClientRpcDelegate(RpcReader___Observers_ReceiveClearResponses_3615296227));
			((NetworkBehaviour)this).RegisterServerRpc(11u, new ServerRpcDelegate(RpcReader___Server_ShowResponses_995803534));
			((NetworkBehaviour)this).RegisterObserversRpc(12u, new ClientRpcDelegate(RpcReader___Observers_ReceiveShowResponses_995803534));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EMessaging_002EMessagingManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EMessaging_002EMessagingManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendMessage_2134336246(Message m, bool notify, string npcID)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerated((Writer)(object)writer, m);
			((Writer)writer).WriteBoolean(notify);
			((Writer)writer).WriteString(npcID);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendMessage_2134336246(Message m, bool notify, string npcID)
	{
		ReceiveMessage(m, notify, npcID);
	}

	private void RpcReader___Server_SendMessage_2134336246(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Message m = GeneratedReaders___Internal.Read___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool notify = ((Reader)PooledReader0).ReadBoolean();
		string npcID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendMessage_2134336246(m, notify, npcID);
		}
	}

	private void RpcWriter___Observers_ReceiveMessage_2134336246(Message m, bool notify, string npcID)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerated((Writer)(object)writer, m);
			((Writer)writer).WriteBoolean(notify);
			((Writer)writer).WriteString(npcID);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveMessage_2134336246(Message m, bool notify, string npcID)
	{
		NPC nPC = NPCManager.GetNPC(npcID);
		if ((Object)(object)nPC == (Object)null)
		{
			Console.LogError("NPC not found with ID " + npcID);
		}
		else
		{
			ConversationMap[nPC].SendMessage(m, notify, network: false);
		}
	}

	private void RpcReader___Observers_ReceiveMessage_2134336246(PooledReader PooledReader0, Channel channel)
	{
		Message m = GeneratedReaders___Internal.Read___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool notify = ((Reader)PooledReader0).ReadBoolean();
		string npcID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceiveMessage_2134336246(m, notify, npcID);
		}
	}

	private void RpcWriter___Server_SendMessageChain_3949292778(MessageChain m, string npcID, float initialDelay, bool notify)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerated((Writer)(object)writer, m);
			((Writer)writer).WriteString(npcID);
			((Writer)writer).WriteSingle(initialDelay, (AutoPackType)0);
			((Writer)writer).WriteBoolean(notify);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendMessageChain_3949292778(MessageChain m, string npcID, float initialDelay, bool notify)
	{
		ReceiveMessageChain(m, npcID, initialDelay, notify);
	}

	private void RpcReader___Server_SendMessageChain_3949292778(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		MessageChain m = GeneratedReaders___Internal.Read___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		string npcID = ((Reader)PooledReader0).ReadString();
		float initialDelay = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		bool notify = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendMessageChain_3949292778(m, npcID, initialDelay, notify);
		}
	}

	private void RpcWriter___Observers_ReceiveMessageChain_3949292778(MessageChain m, string npcID, float initialDelay, bool notify)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerated((Writer)(object)writer, m);
			((Writer)writer).WriteString(npcID);
			((Writer)writer).WriteSingle(initialDelay, (AutoPackType)0);
			((Writer)writer).WriteBoolean(notify);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveMessageChain_3949292778(MessageChain m, string npcID, float initialDelay, bool notify)
	{
		NPC nPC = NPCManager.GetNPC(npcID);
		if ((Object)(object)nPC == (Object)null)
		{
			Console.LogError("NPC not found with ID " + npcID);
		}
		else
		{
			ConversationMap[nPC].SendMessageChain(m, initialDelay, notify, network: false);
		}
	}

	private void RpcReader___Observers_ReceiveMessageChain_3949292778(PooledReader PooledReader0, Channel channel)
	{
		MessageChain m = GeneratedReaders___Internal.Read___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		string npcID = ((Reader)PooledReader0).ReadString();
		float initialDelay = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		bool notify = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceiveMessageChain_3949292778(m, npcID, initialDelay, notify);
		}
	}

	private void RpcWriter___Server_SendResponse_2801973956(int responseIndex, string npcID)
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
			((Writer)writer).WriteInt32(responseIndex, (AutoPackType)1);
			((Writer)writer).WriteString(npcID);
			((NetworkBehaviour)this).SendServerRpc(4u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendResponse_2801973956(int responseIndex, string npcID)
	{
		ReceiveResponse(responseIndex, npcID);
	}

	private void RpcReader___Server_SendResponse_2801973956(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int responseIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		string npcID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendResponse_2801973956(responseIndex, npcID);
		}
	}

	private void RpcWriter___Observers_ReceiveResponse_2801973956(int responseIndex, string npcID)
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
			((Writer)writer).WriteInt32(responseIndex, (AutoPackType)1);
			((Writer)writer).WriteString(npcID);
			((NetworkBehaviour)this).SendObserversRpc(5u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveResponse_2801973956(int responseIndex, string npcID)
	{
		NPC nPC = NPCManager.GetNPC(npcID);
		if ((Object)(object)nPC == (Object)null)
		{
			Console.LogError("NPC not found with ID " + npcID);
			return;
		}
		MSGConversation mSGConversation = ConversationMap[nPC];
		if (mSGConversation.currentResponses.Count <= responseIndex)
		{
			Console.LogWarning("Response index out of range for " + nPC.fullName);
		}
		else
		{
			mSGConversation.ResponseChosen(mSGConversation.currentResponses[responseIndex], network: false);
		}
	}

	private void RpcReader___Observers_ReceiveResponse_2801973956(PooledReader PooledReader0, Channel channel)
	{
		int responseIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		string npcID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceiveResponse_2801973956(responseIndex, npcID);
		}
	}

	private void RpcWriter___Server_SendPlayerMessage_1952281135(int sendableIndex, int sentIndex, string npcID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(sendableIndex, (AutoPackType)1);
			((Writer)writer).WriteInt32(sentIndex, (AutoPackType)1);
			((Writer)writer).WriteString(npcID);
			((NetworkBehaviour)this).SendServerRpc(6u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendPlayerMessage_1952281135(int sendableIndex, int sentIndex, string npcID)
	{
		ReceivePlayerMessage(sendableIndex, sentIndex, npcID);
	}

	private void RpcReader___Server_SendPlayerMessage_1952281135(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int sendableIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int sentIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		string npcID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendPlayerMessage_1952281135(sendableIndex, sentIndex, npcID);
		}
	}

	private void RpcWriter___Observers_ReceivePlayerMessage_1952281135(int sendableIndex, int sentIndex, string npcID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(sendableIndex, (AutoPackType)1);
			((Writer)writer).WriteInt32(sentIndex, (AutoPackType)1);
			((Writer)writer).WriteString(npcID);
			((NetworkBehaviour)this).SendObserversRpc(7u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceivePlayerMessage_1952281135(int sendableIndex, int sentIndex, string npcID)
	{
		NPC nPC = NPCManager.GetNPC(npcID);
		if ((Object)(object)nPC == (Object)null)
		{
			Console.LogError("NPC not found with ID " + npcID);
		}
		else
		{
			ConversationMap[nPC].SendPlayerMessage(sendableIndex, sentIndex, network: false);
		}
	}

	private void RpcReader___Observers_ReceivePlayerMessage_1952281135(PooledReader PooledReader0, Channel channel)
	{
		int sendableIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int sentIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		string npcID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceivePlayerMessage_1952281135(sendableIndex, sentIndex, npcID);
		}
	}

	private void RpcWriter___Target_ReceiveMSGConversationData_2662241369(NetworkConnection conn, string npcID, MSGConversationData data)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(npcID);
			GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002EDatas_002EMSGConversationDataFishNet_002ESerializing_002EGenerated((Writer)(object)writer, data);
			((NetworkBehaviour)this).SendTargetRpc(8u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveMSGConversationData_2662241369(NetworkConnection conn, string npcID, MSGConversationData data)
	{
		NPC nPC = NPCManager.GetNPC(npcID);
		if ((Object)(object)nPC == (Object)null)
		{
			Console.LogError("NPC not found with ID " + npcID);
		}
		else
		{
			ConversationMap[nPC].Load(data);
		}
	}

	private void RpcReader___Target_ReceiveMSGConversationData_2662241369(PooledReader PooledReader0, Channel channel)
	{
		string npcID = ((Reader)PooledReader0).ReadString();
		MSGConversationData data = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002EDatas_002EMSGConversationDataFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveMSGConversationData_2662241369(((NetworkBehaviour)this).LocalConnection, npcID, data);
		}
	}

	private void RpcWriter___Server_ClearResponses_3615296227(string npcID)
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
			((Writer)writer).WriteString(npcID);
			((NetworkBehaviour)this).SendServerRpc(9u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___ClearResponses_3615296227(string npcID)
	{
		ReceiveClearResponses(npcID);
	}

	private void RpcReader___Server_ClearResponses_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string npcID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___ClearResponses_3615296227(npcID);
		}
	}

	private void RpcWriter___Observers_ReceiveClearResponses_3615296227(string npcID)
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
			((Writer)writer).WriteString(npcID);
			((NetworkBehaviour)this).SendObserversRpc(10u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveClearResponses_3615296227(string npcID)
	{
		NPC nPC = NPCManager.GetNPC(npcID);
		if ((Object)(object)nPC == (Object)null)
		{
			Console.LogError("NPC not found with ID " + npcID);
		}
		else
		{
			ConversationMap[nPC].ClearResponses();
		}
	}

	private void RpcReader___Observers_ReceiveClearResponses_3615296227(PooledReader PooledReader0, Channel channel)
	{
		string npcID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveClearResponses_3615296227(npcID);
		}
	}

	private void RpcWriter___Server_ShowResponses_995803534(string npcID, List<Response> responses, float delay)
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
			((Writer)writer).WriteString(npcID);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, responses);
			((Writer)writer).WriteSingle(delay, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(11u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___ShowResponses_995803534(string npcID, List<Response> responses, float delay)
	{
		ReceiveShowResponses(npcID, responses, delay);
	}

	private void RpcReader___Server_ShowResponses_995803534(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string npcID = ((Reader)PooledReader0).ReadString();
		List<Response> responses = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		float delay = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___ShowResponses_995803534(npcID, responses, delay);
		}
	}

	private void RpcWriter___Observers_ReceiveShowResponses_995803534(string npcID, List<Response> responses, float delay)
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
			((Writer)writer).WriteString(npcID);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, responses);
			((Writer)writer).WriteSingle(delay, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(12u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveShowResponses_995803534(string npcID, List<Response> responses, float delay)
	{
		NPC nPC = NPCManager.GetNPC(npcID);
		if ((Object)(object)nPC == (Object)null)
		{
			Console.LogError("NPC not found with ID " + npcID);
		}
		else
		{
			ConversationMap[nPC].ShowResponses(responses, delay, network: false);
		}
	}

	private void RpcReader___Observers_ReceiveShowResponses_995803534(PooledReader PooledReader0, Channel channel)
	{
		string npcID = ((Reader)PooledReader0).ReadString();
		List<Response> responses = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		float delay = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceiveShowResponses_995803534(npcID, responses, delay);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EMessaging_002EMessagingManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
	}
}
