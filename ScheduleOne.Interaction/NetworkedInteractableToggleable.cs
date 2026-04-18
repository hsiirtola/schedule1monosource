using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Interaction;

public class NetworkedInteractableToggleable : NetworkBehaviour
{
	public string ActivateMessage = "Activate";

	public string DeactivateMessage = "Deactivate";

	public float CoolDown;

	[Header("References")]
	public InteractableObject IntObj;

	public UnityEvent onToggle = new UnityEvent();

	public UnityEvent onActivate = new UnityEvent();

	public UnityEvent onDeactivate = new UnityEvent();

	private float lastActivated;

	private bool NetworkInitialize___EarlyScheduleOne_002EInteraction_002ENetworkedInteractableToggleableAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EInteraction_002ENetworkedInteractableToggleableAssembly_002DCSharp_002Edll_Excuted;

	public bool IsActivated { get; private set; }

	public void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		IntObj.onHovered.AddListener(new UnityAction(Hovered));
		IntObj.onInteractStart.AddListener(new UnityAction(Interacted));
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (IsActivated)
		{
			SetState(connection, activated: true);
		}
	}

	public void Hovered()
	{
		if (Time.time - lastActivated < CoolDown)
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			return;
		}
		IntObj.SetMessage(IsActivated ? DeactivateMessage : ActivateMessage);
		IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
	}

	public void Interacted()
	{
		SendToggle();
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendToggle()
	{
		RpcWriter___Server_SendToggle_2166136261();
		RpcLogic___SendToggle_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetState(NetworkConnection conn, bool activated)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetState_214505783(conn, activated);
			RpcLogic___SetState_214505783(conn, activated);
		}
		else
		{
			RpcWriter___Target_SetState_214505783(conn, activated);
		}
	}

	public void PoliceDetected()
	{
		if (InstanceFinder.IsServer && !IsActivated)
		{
			SendToggle();
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EInteraction_002ENetworkedInteractableToggleableAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EInteraction_002ENetworkedInteractableToggleableAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendToggle_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetState_214505783));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_SetState_214505783));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EInteraction_002ENetworkedInteractableToggleableAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EInteraction_002ENetworkedInteractableToggleableAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendToggle_2166136261()
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

	public void RpcLogic___SendToggle_2166136261()
	{
		SetState(null, !IsActivated);
	}

	private void RpcReader___Server_SendToggle_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendToggle_2166136261();
		}
	}

	private void RpcWriter___Observers_SetState_214505783(NetworkConnection conn, bool activated)
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
			((Writer)writer).WriteBoolean(activated);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetState_214505783(NetworkConnection conn, bool activated)
	{
		if (IsActivated != activated)
		{
			lastActivated = Time.time;
			IsActivated = !IsActivated;
			if (onToggle != null)
			{
				onToggle.Invoke();
			}
			if (IsActivated)
			{
				onActivate.Invoke();
			}
			else
			{
				onDeactivate.Invoke();
			}
		}
	}

	private void RpcReader___Observers_SetState_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool activated = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetState_214505783(null, activated);
		}
	}

	private void RpcWriter___Target_SetState_214505783(NetworkConnection conn, bool activated)
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
			((Writer)writer).WriteBoolean(activated);
			((NetworkBehaviour)this).SendTargetRpc(2u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetState_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool activated = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetState_214505783(((NetworkBehaviour)this).LocalConnection, activated);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}
}
