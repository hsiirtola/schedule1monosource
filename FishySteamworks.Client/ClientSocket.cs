using System;
using System.Diagnostics;
using System.Threading;
using FishNet.Transporting;
using Steamworks;
using UnityEngine;

namespace FishySteamworks.Client;

public class ClientSocket : CommonSocket
{
	private Callback<SteamNetConnectionStatusChangedCallback_t> _onLocalConnectionStateCallback;

	private CSteamID _hostSteamID = CSteamID.Nil;

	private HSteamNetConnection _socket;

	private Thread _timeoutThread;

	private float _connectTimeout = -1f;

	private const float CONNECT_TIMEOUT_DURATION = 8000f;

	private void CheckTimeout()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		do
		{
			if ((float)(stopwatch.ElapsedMilliseconds / 1000) > _connectTimeout)
			{
				StopConnection();
			}
			Thread.Sleep(50);
		}
		while ((int)GetLocalConnectionState() == 1);
		stopwatch.Stop();
		_timeoutThread.Abort();
	}

	internal bool StartConnection(string address, ushort port, bool peerToPeer)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (_onLocalConnectionStateCallback == null)
			{
				_onLocalConnectionStateCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create((DispatchDelegate<SteamNetConnectionStatusChangedCallback_t>)OnLocalConnectionState);
			}
			PeerToPeer = peerToPeer;
			byte[] array = ((!peerToPeer) ? GetIPBytes(address) : null);
			if (!peerToPeer && array == null)
			{
				base.SetLocalConnectionState((LocalConnectionState)0, server: false);
				return false;
			}
			base.SetLocalConnectionState((LocalConnectionState)1, server: false);
			_connectTimeout = Time.unscaledTime + 8000f;
			_timeoutThread = new Thread(CheckTimeout);
			_timeoutThread.Start();
			_hostSteamID = new CSteamID(ulong.Parse(address));
			SteamNetworkingIdentity val = default(SteamNetworkingIdentity);
			((SteamNetworkingIdentity)(ref val)).SetSteamID(_hostSteamID);
			SteamNetworkingConfigValue_t[] array2 = (SteamNetworkingConfigValue_t[])(object)new SteamNetworkingConfigValue_t[0];
			if (PeerToPeer)
			{
				_socket = SteamNetworkingSockets.ConnectP2P(ref val, 0, array2.Length, array2);
			}
			else
			{
				SteamNetworkingIPAddr val2 = default(SteamNetworkingIPAddr);
				((SteamNetworkingIPAddr)(ref val2)).Clear();
				((SteamNetworkingIPAddr)(ref val2)).SetIPv6(array, port);
				_socket = SteamNetworkingSockets.ConnectByIPAddress(ref val2, 0, array2);
			}
		}
		catch
		{
			base.SetLocalConnectionState((LocalConnectionState)0, server: false);
			return false;
		}
		return true;
	}

	private unsafe void OnLocalConnectionState(SteamNetConnectionStatusChangedCallback_t args)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		if ((int)args.m_info.m_eState == 3)
		{
			base.SetLocalConnectionState((LocalConnectionState)2, server: false);
		}
		else if ((int)args.m_info.m_eState == 4 || (int)args.m_info.m_eState == 5)
		{
			Transport.NetworkManager.Log("Connection was closed by peer, " + ((SteamNetConnectionInfo_t)(ref args.m_info)).m_szEndDebug);
			StopConnection();
		}
		else
		{
			Transport.NetworkManager.Log("Connection state changed: " + ((object)(*(ESteamNetworkingConnectionState*)(&args.m_info.m_eState))/*cast due to .constrained prefix*/).ToString() + " - " + ((SteamNetConnectionInfo_t)(ref args.m_info)).m_szEndDebug);
		}
	}

	internal bool StopConnection()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (_timeoutThread != null && _timeoutThread.IsAlive)
		{
			_timeoutThread.Abort();
		}
		if (_socket != HSteamNetConnection.Invalid)
		{
			if (_onLocalConnectionStateCallback != null)
			{
				_onLocalConnectionStateCallback.Dispose();
				_onLocalConnectionStateCallback = null;
			}
			SteamNetworkingSockets.CloseConnection(_socket, 0, string.Empty, false);
			_socket = HSteamNetConnection.Invalid;
		}
		if ((int)GetLocalConnectionState() == 0 || (int)GetLocalConnectionState() == 3)
		{
			return false;
		}
		base.SetLocalConnectionState((LocalConnectionState)3, server: false);
		base.SetLocalConnectionState((LocalConnectionState)0, server: false);
		return true;
	}

	internal void IterateIncoming()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if ((int)GetLocalConnectionState() != 2)
		{
			return;
		}
		int num = SteamNetworkingSockets.ReceiveMessagesOnConnection(_socket, MessagePointers, 256);
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				GetMessage(MessagePointers[i], InboundBuffer, out var segment, out var channel);
				Transport.HandleClientReceivedDataArgs(new ClientReceivedDataArgs(segment, (Channel)channel, Transport.Index));
			}
		}
	}

	internal unsafe void SendToServer(byte channelId, ArraySegment<byte> segment)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		if ((int)GetLocalConnectionState() == 2)
		{
			EResult val = Send(_socket, segment, channelId);
			if ((int)val == 3 || (int)val == 8)
			{
				Transport.NetworkManager.Log("Connection to server was lost.");
				StopConnection();
			}
			else if ((int)val != 1)
			{
				Transport.NetworkManager.LogError("Could not send: " + ((object)(*(EResult*)(&val))/*cast due to .constrained prefix*/).ToString());
			}
		}
	}

	internal void IterateOutgoing()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if ((int)GetLocalConnectionState() == 2)
		{
			SteamNetworkingSockets.FlushMessagesOnConnection(_socket);
		}
	}
}
