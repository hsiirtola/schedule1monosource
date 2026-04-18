using System;
using System.Collections.Generic;
using FishNet.Transporting;
using FishNet.Utility.Performance;
using FishySteamworks.Server;

namespace FishySteamworks.Client;

public class ClientHostSocket : CommonSocket
{
	private ServerSocket _server;

	private Queue<LocalPacket> _incoming = new Queue<LocalPacket>();

	internal void CheckSetStarted()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		if (_server != null && (int)GetLocalConnectionState() == 1 && (int)_server.GetLocalConnectionState() == 2)
		{
			SetLocalConnectionState((LocalConnectionState)2, server: false);
		}
	}

	internal bool StartConnection(ServerSocket serverSocket)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		_server = serverSocket;
		_server.SetClientHostSocket(this);
		if ((int)_server.GetLocalConnectionState() != 2)
		{
			return false;
		}
		SetLocalConnectionState((LocalConnectionState)1, server: false);
		return true;
	}

	protected override void SetLocalConnectionState(LocalConnectionState connectionState, bool server)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		base.SetLocalConnectionState(connectionState, server);
		if ((int)connectionState == 2)
		{
			_server.OnClientHostState(started: true);
		}
		else
		{
			_server.OnClientHostState(started: false);
		}
	}

	internal bool StopConnection()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		if ((int)GetLocalConnectionState() == 0 || (int)GetLocalConnectionState() == 3)
		{
			return false;
		}
		ClearQueue(_incoming);
		SetLocalConnectionState((LocalConnectionState)3, server: false);
		SetLocalConnectionState((LocalConnectionState)0, server: false);
		_server.SetClientHostSocket(null);
		return true;
	}

	internal void IterateIncoming()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if ((int)GetLocalConnectionState() == 2)
		{
			while (_incoming.Count > 0)
			{
				LocalPacket localPacket = _incoming.Dequeue();
				ArraySegment<byte> arraySegment = new ArraySegment<byte>(localPacket.Data, 0, localPacket.Length);
				Transport.HandleClientReceivedDataArgs(new ClientReceivedDataArgs(arraySegment, (Channel)localPacket.Channel, Transport.Index));
				ByteArrayPool.Store(localPacket.Data);
			}
		}
	}

	internal void ReceivedFromLocalServer(LocalPacket packet)
	{
		_incoming.Enqueue(packet);
	}

	internal void SendToServer(byte channelId, ArraySegment<byte> segment)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if ((int)GetLocalConnectionState() == 2 && (int)_server.GetLocalConnectionState() == 2)
		{
			LocalPacket packet = new LocalPacket(segment, channelId);
			_server.ReceivedFromClientHost(packet);
		}
	}
}
