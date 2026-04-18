using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.GameTime;
using ScheduleOne.Levelling;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Map;

public class DarkMarket : NetworkSingleton<DarkMarket>
{
	public DarkMarketAccessZone AccessZone;

	public DarkMarketMainDoor MainDoor;

	public Oscar Oscar;

	public FullRank UnlockRank;

	private bool NetworkInitialize___EarlyScheduleOne_002EMap_002EDarkMarketAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EMap_002EDarkMarketAssembly_002DCSharp_002Edll_Excuted;

	public bool IsOpen { get; protected set; } = true;

	public bool Unlocked { get; protected set; }

	protected override void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		base.Start();
		Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(OnLoad));
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (Unlocked)
		{
			SetUnlocked(connection);
		}
	}

	private void Update()
	{
		IsOpen = ShouldBeOpen();
	}

	private bool ShouldBeOpen()
	{
		if (!NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(AccessZone.OpenTime, AccessZone.CloseTime))
		{
			return false;
		}
		for (int i = 0; i < Player.PlayerList.Count; i++)
		{
			if (Player.PlayerList[i].CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None)
			{
				return false;
			}
		}
		return true;
	}

	private void OnLoad()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(new UnityAction(OnLoad));
		if (NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("WarehouseUnlocked"))
		{
			SendUnlocked();
		}
		else
		{
			MainDoor.SetKnockingEnabled(enabled: true);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendUnlocked()
	{
		RpcWriter___Server_SendUnlocked_2166136261();
		RpcLogic___SendUnlocked_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetUnlocked(NetworkConnection conn)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetUnlocked_328543758(conn);
			RpcLogic___SetUnlocked_328543758(conn);
		}
		else
		{
			RpcWriter___Target_SetUnlocked_328543758(conn);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EMap_002EDarkMarketAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EMap_002EDarkMarketAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendUnlocked_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetUnlocked_328543758));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_SetUnlocked_328543758));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EMap_002EDarkMarketAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EMap_002EDarkMarketAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendUnlocked_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendUnlocked_2166136261()
	{
		SetUnlocked(null);
	}

	private void RpcReader___Server_SendUnlocked_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendUnlocked_2166136261();
		}
	}

	private void RpcWriter___Observers_SetUnlocked_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetUnlocked_328543758(NetworkConnection conn)
	{
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("WarehouseUnlocked", true.ToString());
		MainDoor.SetKnockingEnabled(enabled: false);
		((Component)MainDoor.Igor).gameObject.SetActive(false);
		Unlocked = true;
		Oscar.EnableDeliveries();
		DoorController[] doors = AccessZone.Doors;
		for (int i = 0; i < doors.Length; i++)
		{
			doors[i].noAccessErrorMessage = "Only open after 6PM";
		}
	}

	private void RpcReader___Observers_SetUnlocked_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetUnlocked_328543758(null);
		}
	}

	private void RpcWriter___Target_SetUnlocked_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendTargetRpc(2u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetUnlocked_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetUnlocked_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
