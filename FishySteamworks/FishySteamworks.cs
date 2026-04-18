using System;
using FishNet.Managing;
using FishNet.Transporting;
using FishySteamworks.Client;
using FishySteamworks.Server;
using Steamworks;
using UnityEngine;

namespace FishySteamworks;

public class FishySteamworks : Transport
{
	[NonSerialized]
	public ulong LocalUserSteamID;

	[Tooltip("Address server should bind to.")]
	[SerializeField]
	private string _serverBindAddress = string.Empty;

	[Tooltip("Port to use.")]
	[SerializeField]
	private ushort _port = 7770;

	[Tooltip("Maximum number of players which may be connected at once.")]
	[Range(1f, 65535f)]
	[SerializeField]
	private ushort _maximumClients = 9001;

	[Tooltip("True if using peer to peer socket.")]
	[SerializeField]
	private bool _peerToPeer;

	[Tooltip("Address client should connect to.")]
	[SerializeField]
	private string _clientAddress = string.Empty;

	private int[] _mtus;

	private ClientSocket _client;

	private ClientHostSocket _clientHost;

	private ServerSocket _server;

	private bool _shutdownCalled = true;

	internal const int CLIENT_HOST_ID = 32767;

	public override event Action<ClientConnectionStateArgs> OnClientConnectionState;

	public override event Action<ServerConnectionStateArgs> OnServerConnectionState;

	public override event Action<RemoteConnectionStateArgs> OnRemoteConnectionState;

	public override event Action<ClientReceivedDataArgs> OnClientReceivedData;

	public override event Action<ServerReceivedDataArgs> OnServerReceivedData;

	~FishySteamworks()
	{
		try
		{
			((Transport)this).Shutdown();
		}
		finally
		{
			((object)this).Finalize();
		}
	}

	public override void Initialize(NetworkManager networkManager, int transportIndex)
	{
		((Transport)this).Initialize(networkManager, transportIndex);
		_client = new ClientSocket();
		_clientHost = new ClientHostSocket();
		_server = new ServerSocket();
		CreateChannelData();
		_client.Initialize((Transport)(object)this);
		_clientHost.Initialize((Transport)(object)this);
		_server.Initialize((Transport)(object)this);
	}

	private void OnDestroy()
	{
		((Transport)this).Shutdown();
	}

	private void Update()
	{
		_clientHost.CheckSetStarted();
	}

	private void CreateChannelData()
	{
		_mtus = new int[2] { 1048576, 1200 };
	}

	private bool InitializeRelayNetworkAccess()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			SteamNetworkingUtils.InitRelayNetworkAccess();
			if (IsNetworkAccessAvailable())
			{
				LocalUserSteamID = SteamUser.GetSteamID().m_SteamID;
			}
			_shutdownCalled = false;
			return true;
		}
		catch
		{
			return false;
		}
	}

	public bool IsNetworkAccessAvailable()
	{
		try
		{
			InteropHelp.TestIfAvailableClient();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public override string GetConnectionAddress(int connectionId)
	{
		return _server.GetConnectionAddress(connectionId);
	}

	public override LocalConnectionState GetConnectionState(bool server)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (server)
		{
			return _server.GetLocalConnectionState();
		}
		return _client.GetLocalConnectionState();
	}

	public override RemoteConnectionState GetConnectionState(int connectionId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return _server.GetConnectionState(connectionId);
	}

	public override void HandleClientConnectionState(ClientConnectionStateArgs connectionStateArgs)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		this.OnClientConnectionState?.Invoke(connectionStateArgs);
	}

	public override void HandleServerConnectionState(ServerConnectionStateArgs connectionStateArgs)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		this.OnServerConnectionState?.Invoke(connectionStateArgs);
	}

	public override void HandleRemoteConnectionState(RemoteConnectionStateArgs connectionStateArgs)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		this.OnRemoteConnectionState?.Invoke(connectionStateArgs);
	}

	public override void IterateIncoming(bool server)
	{
		if (server)
		{
			_server.IterateIncoming();
			return;
		}
		_client.IterateIncoming();
		_clientHost.IterateIncoming();
	}

	public override void IterateOutgoing(bool server)
	{
		if (server)
		{
			_server.IterateOutgoing();
		}
		else
		{
			_client.IterateOutgoing();
		}
	}

	public override void HandleClientReceivedDataArgs(ClientReceivedDataArgs receivedDataArgs)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		this.OnClientReceivedData?.Invoke(receivedDataArgs);
	}

	public override void HandleServerReceivedDataArgs(ServerReceivedDataArgs receivedDataArgs)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		this.OnServerReceivedData?.Invoke(receivedDataArgs);
	}

	public override void SendToServer(byte channelId, ArraySegment<byte> segment)
	{
		_client.SendToServer(channelId, segment);
		_clientHost.SendToServer(channelId, segment);
	}

	public override void SendToClient(byte channelId, ArraySegment<byte> segment, int connectionId)
	{
		_server.SendToClient(channelId, segment, connectionId);
	}

	public override int GetMaximumClients()
	{
		return _server.GetMaximumClients();
	}

	public override void SetMaximumClients(int value)
	{
		_server.SetMaximumClients(value);
	}

	public override void SetClientAddress(string address)
	{
		_clientAddress = address;
	}

	public override void SetServerBindAddress(string address, IPAddressType addressType)
	{
		_serverBindAddress = address;
	}

	public override void SetPort(ushort port)
	{
		_port = port;
	}

	public override bool StartConnection(bool server)
	{
		if (server)
		{
			return StartServer();
		}
		return StartClient(_clientAddress);
	}

	public override bool StopConnection(bool server)
	{
		if (server)
		{
			return StopServer();
		}
		return StopClient();
	}

	public override bool StopConnection(int connectionId, bool immediately)
	{
		return StopClient(connectionId, immediately);
	}

	public override void Shutdown()
	{
		if (!_shutdownCalled)
		{
			_shutdownCalled = true;
			((Transport)this).StopConnection(false);
			((Transport)this).StopConnection(true);
		}
	}

	private bool StartServer()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Invalid comparison between Unknown and I4
		if (!InitializeRelayNetworkAccess())
		{
			((Transport)this).NetworkManager.LogError("RelayNetworkAccess could not be initialized.");
			return false;
		}
		if (!IsNetworkAccessAvailable())
		{
			((Transport)this).NetworkManager.LogError("Server network access is not available.");
			return false;
		}
		_server.ResetInvalidSocket();
		if ((int)_server.GetLocalConnectionState() != 0)
		{
			((Transport)this).NetworkManager.LogError("Server is already running.");
			return false;
		}
		bool flag = (int)_client.GetLocalConnectionState() > 0;
		if (flag)
		{
			_client.StopConnection();
		}
		bool num = _server.StartConnection(_serverBindAddress, _port, _maximumClients, _peerToPeer);
		if (num && flag)
		{
			((Transport)this).StartConnection(false);
		}
		return num;
	}

	private bool StopServer()
	{
		if (_server != null)
		{
			return _server.StopConnection();
		}
		return false;
	}

	private bool StartClient(string address)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if ((int)_server.GetLocalConnectionState() == 0)
		{
			if ((int)_client.GetLocalConnectionState() != 0)
			{
				((Transport)this).NetworkManager.LogError("Client is already running.");
				return false;
			}
			if ((int)_clientHost.GetLocalConnectionState() != 0)
			{
				_clientHost.StopConnection();
			}
			if (!InitializeRelayNetworkAccess())
			{
				((Transport)this).NetworkManager.LogError("RelayNetworkAccess could not be initialized.");
				return false;
			}
			if (!IsNetworkAccessAvailable())
			{
				((Transport)this).NetworkManager.LogError("Client network access is not available.");
				return false;
			}
			_client.StartConnection(address, _port, _peerToPeer);
		}
		else
		{
			_clientHost.StartConnection(_server);
		}
		return true;
	}

	private bool StopClient()
	{
		bool flag = false;
		if (_client != null)
		{
			flag |= _client.StopConnection();
		}
		if (_clientHost != null)
		{
			flag |= _clientHost.StopConnection();
		}
		return flag;
	}

	private bool StopClient(int connectionId, bool immediately)
	{
		return _server.StopConnection(connectionId);
	}

	public override int GetMTU(byte channel)
	{
		if (channel >= _mtus.Length)
		{
			Debug.LogError((object)$"Channel {channel} is out of bounds.");
			return 0;
		}
		return _mtus[channel];
	}
}
