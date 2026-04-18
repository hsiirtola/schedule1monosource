using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using FishNet.Transporting;
using FishNet.Utility.Performance;
using Steamworks;

namespace FishySteamworks;

public abstract class CommonSocket
{
	private LocalConnectionState _connectionState;

	protected bool PeerToPeer;

	protected Transport Transport;

	protected IntPtr[] MessagePointers = new IntPtr[256];

	protected byte[] InboundBuffer;

	protected const int MAX_MESSAGES = 256;

	internal LocalConnectionState GetLocalConnectionState()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _connectionState;
	}

	protected virtual void SetLocalConnectionState(LocalConnectionState connectionState, bool server)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (connectionState != _connectionState)
		{
			_connectionState = connectionState;
			if (server)
			{
				Transport.HandleServerConnectionState(new ServerConnectionStateArgs(connectionState, Transport.Index));
			}
			else
			{
				Transport.HandleClientConnectionState(new ClientConnectionStateArgs(connectionState, Transport.Index));
			}
		}
	}

	internal virtual void Initialize(Transport t)
	{
		Transport = t;
		int mTU = Transport.GetMTU((byte)0);
		mTU = Math.Max(mTU, Transport.GetMTU((byte)1));
		InboundBuffer = new byte[mTU];
	}

	protected byte[] GetIPBytes(string address)
	{
		if (!string.IsNullOrEmpty(address))
		{
			if (!IPAddress.TryParse(address, out var address2))
			{
				Transport.NetworkManager.LogError("Could not parse address " + address + " to IPAddress.");
				return null;
			}
			return address2.GetAddressBytes();
		}
		return null;
	}

	protected EResult Send(HSteamNetConnection steamConnection, ArraySegment<byte> segment, byte channelId)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Invalid comparison between Unknown and I4
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (segment.Array.Length - 1 <= segment.Offset + segment.Count)
		{
			byte[] array = segment.Array;
			Array.Resize(ref array, array.Length + 1);
			array[array.Length - 1] = channelId;
		}
		else
		{
			segment.Array[segment.Offset + segment.Count] = channelId;
		}
		segment = new ArraySegment<byte>(segment.Array, segment.Offset, segment.Count + 1);
		GCHandle gCHandle = GCHandle.Alloc(segment.Array, GCHandleType.Pinned);
		IntPtr intPtr = gCHandle.AddrOfPinnedObject() + segment.Offset;
		int num = ((channelId != 1) ? 8 : 0);
		long num2 = default(long);
		EResult val = SteamNetworkingSockets.SendMessageToConnection(steamConnection, intPtr, (uint)segment.Count, num, ref num2);
		if ((int)val != 1)
		{
			Transport.NetworkManager.LogWarning($"Send issue: {val}");
		}
		gCHandle.Free();
		return val;
	}

	internal void ClearQueue(ConcurrentQueue<LocalPacket> queue)
	{
		LocalPacket result;
		while (queue.TryDequeue(out result))
		{
			ByteArrayPool.Store(result.Data);
		}
	}

	internal void ClearQueue(Queue<LocalPacket> queue)
	{
		while (queue.Count > 0)
		{
			ByteArrayPool.Store(queue.Dequeue().Data);
		}
	}

	protected void GetMessage(IntPtr ptr, byte[] buffer, out ArraySegment<byte> segment, out byte channel)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		SteamNetworkingMessage_t val = Marshal.PtrToStructure<SteamNetworkingMessage_t>(ptr);
		int cbSize = val.m_cbSize;
		Marshal.Copy(val.m_pData, buffer, 0, cbSize);
		SteamNetworkingMessage_t.Release(ptr);
		channel = buffer[cbSize - 1];
		segment = new ArraySegment<byte>(buffer, 0, cbSize - 1);
	}
}
