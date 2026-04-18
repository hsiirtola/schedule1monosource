using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.EntityFramework;

public class ToggleableItem : GridItem
{
	public enum EStartupAction
	{
		None,
		TurnOn,
		TurnOff,
		Toggle
	}

	[Header("Settings")]
	public EStartupAction StartupAction;

	public UnityEvent onTurnedOn;

	public UnityEvent onTurnedOff;

	public UnityEvent onTurnOnOrOff;

	private bool NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EToggleableItemAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEntityFramework_002EToggleableItemAssembly_002DCSharp_002Edll_Excuted;

	public bool IsOn { get; private set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EEntityFramework_002EToggleableItem_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (IsOn)
		{
			SetIsOn(connection, on: true);
		}
	}

	public void Toggle()
	{
		if (IsOn)
		{
			TurnOff();
		}
		else
		{
			TurnOn();
		}
	}

	public void TurnOn(bool network = true)
	{
		if (IsOn)
		{
			return;
		}
		if (network)
		{
			SendIsOn(on: true);
			return;
		}
		IsOn = true;
		if (onTurnedOn != null)
		{
			onTurnedOn.Invoke();
		}
		if (onTurnOnOrOff != null)
		{
			onTurnOnOrOff.Invoke();
		}
	}

	public void TurnOff(bool network = true)
	{
		if (!IsOn)
		{
			return;
		}
		if (network)
		{
			SendIsOn(on: false);
			return;
		}
		IsOn = false;
		if (onTurnedOff != null)
		{
			onTurnedOff.Invoke();
		}
		if (onTurnOnOrOff != null)
		{
			onTurnOnOrOff.Invoke();
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	private void SendIsOn(bool on)
	{
		RpcWriter___Server_SendIsOn_1140765316(on);
		RpcLogic___SendIsOn_1140765316(on);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetIsOn(NetworkConnection conn, bool on)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetIsOn_214505783(conn, on);
			RpcLogic___SetIsOn_214505783(conn, on);
		}
		else
		{
			RpcWriter___Target_SetIsOn_214505783(conn, on);
		}
	}

	public override BuildableItemData GetBaseData()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new ToggleableItemData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, _originCoordinate, _rotation, IsOn);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EToggleableItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EToggleableItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_SendIsOn_1140765316));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_SetIsOn_214505783));
			((NetworkBehaviour)this).RegisterTargetRpc(7u, new ClientRpcDelegate(RpcReader___Target_SetIsOn_214505783));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEntityFramework_002EToggleableItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEntityFramework_002EToggleableItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendIsOn_1140765316(bool on)
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
			((Writer)writer).WriteBoolean(on);
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendIsOn_1140765316(bool on)
	{
		base.HasChanged = true;
		SetIsOn(null, on);
	}

	private void RpcReader___Server_SendIsOn_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool flag = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendIsOn_1140765316(flag);
		}
	}

	private void RpcWriter___Observers_SetIsOn_214505783(NetworkConnection conn, bool on)
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
			((Writer)writer).WriteBoolean(on);
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetIsOn_214505783(NetworkConnection conn, bool on)
	{
		if (on)
		{
			TurnOn(network: false);
		}
		else
		{
			TurnOff(network: false);
		}
	}

	private void RpcReader___Observers_SetIsOn_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool flag = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetIsOn_214505783(null, flag);
		}
	}

	private void RpcWriter___Target_SetIsOn_214505783(NetworkConnection conn, bool on)
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
			((Writer)writer).WriteBoolean(on);
			((NetworkBehaviour)this).SendTargetRpc(7u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetIsOn_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool flag = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetIsOn_214505783(((NetworkBehaviour)this).LocalConnection, flag);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EEntityFramework_002EToggleableItem_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		switch (StartupAction)
		{
		case EStartupAction.TurnOn:
			TurnOn();
			break;
		case EStartupAction.TurnOff:
			TurnOff();
			break;
		case EStartupAction.Toggle:
			Toggle();
			break;
		}
	}
}
