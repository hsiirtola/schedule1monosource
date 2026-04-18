using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FishNet.Transporting;
using FishySteamworks.Client;
using Steamworks;

namespace FishySteamworks.Server;

public class ServerSocket : CommonSocket
{
	public struct ConnectionChange
	{
		public int ConnectionId;

		public HSteamNetConnection SteamConnection;

		public CSteamID SteamId;

		public bool IsConnect => ((CSteamID)(ref SteamId)).IsValid();

		public ConnectionChange(int id)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			ConnectionId = id;
			SteamId = CSteamID.Nil;
			SteamConnection = default(HSteamNetConnection);
		}

		public ConnectionChange(int id, HSteamNetConnection steamConnection, CSteamID steamId)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			ConnectionId = id;
			SteamConnection = steamConnection;
			SteamId = steamId;
		}
	}

	private BidirectionalDictionary<HSteamNetConnection, int> _steamConnections = new BidirectionalDictionary<HSteamNetConnection, int>();

	private BidirectionalDictionary<CSteamID, int> _steamIds = new BidirectionalDictionary<CSteamID, int>();

	private int _maximumClients;

	private int _nextConnectionId;

	private HSteamListenSocket _socket = new HSteamListenSocket(0u);

	private Queue<LocalPacket> _clientHostIncoming = new Queue<LocalPacket>();

	private bool _clientHostStarted;

	private Callback<SteamNetConnectionStatusChangedCallback_t> _onRemoteConnectionStateCallback;

	private Queue<int> _cachedConnectionIds = new Queue<int>();

	private ClientHostSocket _clientHost;

	private bool _iteratingConnections;

	private List<ConnectionChange> _pendingConnectionChanges = new List<ConnectionChange>();

	internal RemoteConnectionState GetConnectionState(int connectionId)
	{
		if (!_steamConnections.Second.ContainsKey(connectionId))
		{
			return (RemoteConnectionState)0;
		}
		return (RemoteConnectionState)2;
	}

	internal void ResetInvalidSocket()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (_socket == HSteamListenSocket.Invalid)
		{
			base.SetLocalConnectionState((LocalConnectionState)0, server: true);
		}
	}

	internal bool StartConnection(string address, ushort port, int maximumClients, bool peerToPeer)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (_onRemoteConnectionStateCallback == null)
			{
				_onRemoteConnectionStateCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create((DispatchDelegate<SteamNetConnectionStatusChangedCallback_t>)OnRemoteConnectionState);
			}
			PeerToPeer = peerToPeer;
			byte[] array = ((!peerToPeer) ? GetIPBytes(address) : null);
			PeerToPeer = peerToPeer;
			SetMaximumClients(maximumClients);
			_nextConnectionId = 0;
			_cachedConnectionIds.Clear();
			_iteratingConnections = false;
			base.SetLocalConnectionState((LocalConnectionState)1, server: true);
			SteamNetworkingConfigValue_t[] array2 = (SteamNetworkingConfigValue_t[])(object)new SteamNetworkingConfigValue_t[0];
			if (PeerToPeer)
			{
				_socket = SteamNetworkingSockets.CreateListenSocketP2P(0, array2.Length, array2);
			}
			else
			{
				SteamNetworkingIPAddr val = default(SteamNetworkingIPAddr);
				((SteamNetworkingIPAddr)(ref val)).Clear();
				if (array != null)
				{
					((SteamNetworkingIPAddr)(ref val)).SetIPv6(array, port);
				}
				_socket = SteamNetworkingSockets.CreateListenSocketIP(ref val, 0, array2);
			}
		}
		catch
		{
			base.SetLocalConnectionState((LocalConnectionState)0, server: true);
			return false;
		}
		if (_socket == HSteamListenSocket.Invalid)
		{
			base.SetLocalConnectionState((LocalConnectionState)0, server: true);
			return false;
		}
		base.SetLocalConnectionState((LocalConnectionState)2, server: true);
		return true;
	}

	internal bool StopConnection()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (_socket != HSteamListenSocket.Invalid)
		{
			SteamNetworkingSockets.CloseListenSocket(_socket);
			if (_onRemoteConnectionStateCallback != null)
			{
				_onRemoteConnectionStateCallback.Dispose();
				_onRemoteConnectionStateCallback = null;
			}
			_socket = HSteamListenSocket.Invalid;
		}
		_pendingConnectionChanges.Clear();
		if ((int)GetLocalConnectionState() == 0)
		{
			return false;
		}
		base.SetLocalConnectionState((LocalConnectionState)3, server: true);
		base.SetLocalConnectionState((LocalConnectionState)0, server: true);
		return true;
	}

	internal bool StopConnection(int connectionId)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (connectionId == 32767)
		{
			if (_clientHost != null)
			{
				_clientHost.StopConnection();
				return true;
			}
			return false;
		}
		if (_steamConnections.Second.TryGetValue(connectionId, out var value))
		{
			return StopConnection(connectionId, value);
		}
		Transport.NetworkManager.LogError($"Steam connection not found for connectionId {connectionId}.");
		return false;
	}

	private bool StopConnection(int connectionId, HSteamNetConnection socket)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		SteamNetworkingSockets.CloseConnection(socket, 0, string.Empty, false);
		if (!_iteratingConnections)
		{
			RemoveConnection(connectionId);
		}
		else
		{
			_pendingConnectionChanges.Add(new ConnectionChange(connectionId));
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void OnRemoteConnectionState(SteamNetConnectionStatusChangedCallback_t args)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Invalid comparison between Unknown and I4
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Invalid comparison between Unknown and I4
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Invalid comparison between Unknown and I4
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Invalid comparison between Unknown and I4
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		ulong steamID = ((SteamNetworkingIdentity)(ref args.m_info.m_identityRemote)).GetSteamID64();
		if ((int)args.m_info.m_eState == 1)
		{
			if (_steamConnections.Count >= GetMaximumClients())
			{
				Transport.NetworkManager.Log($"Incoming connection {steamID} was rejected because would exceed the maximum connection count.");
				SteamNetworkingSockets.CloseConnection(args.m_hConn, 0, "Max Connection Count", false);
				return;
			}
			EResult val = SteamNetworkingSockets.AcceptConnection(args.m_hConn);
			if ((int)val == 1)
			{
				Transport.NetworkManager.Log($"Accepting connection {steamID}");
			}
			else
			{
				Transport.NetworkManager.Log($"Connection {steamID} could not be accepted: {((object)(*(EResult*)(&val))/*cast due to .constrained prefix*/).ToString()}");
			}
		}
		else if ((int)args.m_info.m_eState == 3)
		{
			int num = ((_cachedConnectionIds.Count > 0) ? _cachedConnectionIds.Dequeue() : _nextConnectionId++);
			if (!_iteratingConnections)
			{
				AddConnection(num, args.m_hConn, ((SteamNetworkingIdentity)(ref args.m_info.m_identityRemote)).GetSteamID());
			}
			else
			{
				_pendingConnectionChanges.Add(new ConnectionChange(num, args.m_hConn, ((SteamNetworkingIdentity)(ref args.m_info.m_identityRemote)).GetSteamID()));
			}
		}
		else if ((int)args.m_info.m_eState == 4 || (int)args.m_info.m_eState == 5)
		{
			if (_steamConnections.TryGetValue(args.m_hConn, out var value))
			{
				StopConnection(value, args.m_hConn);
			}
		}
		else
		{
			Transport.NetworkManager.Log($"Connection {steamID} state changed: {((object)(*(ESteamNetworkingConnectionState*)(&args.m_info.m_eState))/*cast due to .constrained prefix*/).ToString()}");
		}
	}

	private void AddConnection(int connectionId, HSteamNetConnection steamConnection, CSteamID steamId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		_steamConnections.Add(steamConnection, connectionId);
		_steamIds.Add(steamId, connectionId);
		Transport.NetworkManager.Log($"Client with SteamID {steamId.m_SteamID} connected. Assigning connection id {connectionId}");
		Transport.HandleRemoteConnectionState(new RemoteConnectionStateArgs((RemoteConnectionState)2, connectionId, Transport.Index));
	}

	private void RemoveConnection(int connectionId)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		_steamConnections.Remove(connectionId);
		_steamIds.Remove(connectionId);
		Transport.NetworkManager.Log($"Client with ConnectionID {connectionId} disconnected.");
		Transport.HandleRemoteConnectionState(new RemoteConnectionStateArgs((RemoteConnectionState)0, connectionId, Transport.Index));
		_cachedConnectionIds.Enqueue(connectionId);
	}

	internal void IterateOutgoing()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if ((int)GetLocalConnectionState() != 2)
		{
			return;
		}
		_iteratingConnections = true;
		foreach (HSteamNetConnection firstType in _steamConnections.FirstTypes)
		{
			SteamNetworkingSockets.FlushMessagesOnConnection(firstType);
		}
		_iteratingConnections = false;
		ProcessPendingConnectionChanges();
	}

	internal void IterateIncoming()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if ((int)GetLocalConnectionState() == 0 || (int)GetLocalConnectionState() == 3)
		{
			return;
		}
		_iteratingConnections = true;
		while (_clientHostIncoming.Count > 0)
		{
			LocalPacket localPacket = _clientHostIncoming.Dequeue();
			ArraySegment<byte> arraySegment = new ArraySegment<byte>(localPacket.Data, 0, localPacket.Length);
			Transport.HandleServerReceivedDataArgs(new ServerReceivedDataArgs(arraySegment, (Channel)localPacket.Channel, 32767, Transport.Index));
		}
		foreach (KeyValuePair<HSteamNetConnection, int> item in _steamConnections.First)
		{
			HSteamNetConnection key = item.Key;
			int value = item.Value;
			int num = SteamNetworkingSockets.ReceiveMessagesOnConnection(key, MessagePointers, 256);
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					GetMessage(MessagePointers[i], InboundBuffer, out var segment, out var channel);
					Transport.HandleServerReceivedDataArgs(new ServerReceivedDataArgs(segment, (Channel)channel, value, Transport.Index));
				}
			}
		}
		_iteratingConnections = false;
		ProcessPendingConnectionChanges();
	}

	private void ProcessPendingConnectionChanges()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		foreach (ConnectionChange pendingConnectionChange in _pendingConnectionChanges)
		{
			if (pendingConnectionChange.IsConnect)
			{
				AddConnection(pendingConnectionChange.ConnectionId, pendingConnectionChange.SteamConnection, pendingConnectionChange.SteamId);
			}
			else
			{
				RemoveConnection(pendingConnectionChange.ConnectionId);
			}
		}
		_pendingConnectionChanges.Clear();
	}

	internal unsafe void SendToClient(byte channelId, ArraySegment<byte> segment, int connectionId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Invalid comparison between Unknown and I4
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Invalid comparison between Unknown and I4
		if ((int)GetLocalConnectionState() != 2)
		{
			return;
		}
		HSteamNetConnection value;
		if (connectionId == 32767)
		{
			if (_clientHost != null)
			{
				LocalPacket packet = new LocalPacket(segment, channelId);
				_clientHost.ReceivedFromLocalServer(packet);
			}
		}
		else if (_steamConnections.TryGetValue(connectionId, out value))
		{
			EResult val = Send(value, segment, channelId);
			if ((int)val == 3 || (int)val == 8)
			{
				Transport.NetworkManager.Log($"Connection to {connectionId} was lost.");
				StopConnection(connectionId, value);
			}
			else if ((int)val != 1)
			{
				Transport.NetworkManager.LogError("Could not send: " + ((object)(*(EResult*)(&val))/*cast due to .constrained prefix*/).ToString());
			}
		}
		else
		{
			Transport.NetworkManager.LogError($"ConnectionId {connectionId} does not exist, data will not be sent.");
		}
	}

	internal unsafe string GetConnectionAddress(int connectionId)
	{
		if (_steamIds.TryGetValue(connectionId, out var value))
		{
			return ((object)(*(CSteamID*)(&value))/*cast due to .constrained prefix*/).ToString();
		}
		Transport.NetworkManager.LogError($"ConnectionId {connectionId} is invalid; address cannot be returned.");
		return string.Empty;
	}

	internal void SetMaximumClients(int value)
	{
		_maximumClients = Math.Min(value, 32766);
	}

	internal int GetMaximumClients()
	{
		return _maximumClients;
	}

	internal void SetClientHostSocket(ClientHostSocket socket)
	{
		_clientHost = socket;
	}

	internal void OnClientHostState(bool started)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		FishySteamworks fishySteamworks = (FishySteamworks)(object)Transport;
		CSteamID key = default(CSteamID);
		((CSteamID)(ref key))._002Ector(fishySteamworks.LocalUserSteamID);
		if (!started && _clientHostStarted)
		{
			ClearQueue(_clientHostIncoming);
			Transport.HandleRemoteConnectionState(new RemoteConnectionStateArgs((RemoteConnectionState)0, 32767, Transport.Index));
			_steamIds.Remove(key);
		}
		else if (started)
		{
			_steamIds[key] = 32767;
			Transport.HandleRemoteConnectionState(new RemoteConnectionStateArgs((RemoteConnectionState)2, 32767, Transport.Index));
		}
		_clientHostStarted = started;
	}

	internal void ReceivedFromClientHost(LocalPacket packet)
	{
		if (_clientHostStarted)
		{
			_clientHostIncoming.Enqueue(packet);
		}
	}
}
