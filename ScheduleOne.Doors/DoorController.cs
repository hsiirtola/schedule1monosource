using System;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Doors;

public class DoorController : NetworkBehaviour
{
	public const float DISTANT_PLAYER_THRESHOLD = 40f;

	public EDoorAccess PlayerAccess;

	public bool AutoOpenForPlayer;

	[Header("References")]
	[SerializeField]
	protected InteractableObject[] InteriorIntObjs;

	[SerializeField]
	protected InteractableObject[] ExteriorIntObjs;

	[Tooltip("Used to block player from entering when the door is open for an NPC, but player isn't permitted access.")]
	[SerializeField]
	protected BoxCollider PlayerBlocker;

	[Header("Animation")]
	[SerializeField]
	protected Animation InteriorDoorHandleAnimation;

	[SerializeField]
	protected Animation ExteriorDoorHandleAnimation;

	[Header("Settings")]
	[SerializeField]
	protected bool AutoCloseOnSleep = true;

	[SerializeField]
	protected bool AutoCloseOnDistantPlayer = true;

	[Header("NPC Access")]
	[SerializeField]
	protected bool OpenableByNPCs = true;

	[Tooltip("How many seconds to wait after NPC passes through to return to original state")]
	[SerializeField]
	protected float ReturnToOriginalTime = 0.5f;

	public UnityEvent<EDoorSide> onDoorOpened;

	public UnityEvent onDoorClosed;

	private EDoorSide lastOpenSide = EDoorSide.Exterior;

	private bool autoOpenedForPlayer;

	[HideInInspector]
	public string noAccessErrorMessage = string.Empty;

	private bool NetworkInitialize___EarlyScheduleOne_002EDoors_002EDoorControllerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EDoors_002EDoorControllerAssembly_002DCSharp_002Edll_Excuted;

	public bool IsOpen { get; protected set; }

	public bool openedByNPC { get; protected set; }

	public int detectedNPCCount { get; protected set; }

	public float timeSinceNPCSensed { get; protected set; } = float.MaxValue;

	public bool playerDetectedSinceOpened { get; protected set; }

	public int detectedPlayerCount { get; protected set; }

	public float timeSincePlayerSensed { get; protected set; } = float.MaxValue;

	public float timeInCurrentState { get; protected set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EDoors_002EDoorController_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected virtual void Start()
	{
		if (!AutoCloseOnSleep)
		{
			return;
		}
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepStart = (Action)Delegate.Combine(instance.onSleepStart, (Action)delegate
		{
			if (IsOpen)
			{
				SetIsOpen(open: false, EDoorSide.Interior);
			}
		});
	}

	protected virtual void Update()
	{
		if (detectedNPCCount == 0)
		{
			timeSinceNPCSensed += Time.deltaTime;
		}
		if (detectedPlayerCount == 0)
		{
			timeSincePlayerSensed += Time.deltaTime;
		}
		timeInCurrentState += Time.deltaTime;
		if (InstanceFinder.IsServer && IsOpen && ((openedByNPC && timeSinceNPCSensed > ReturnToOriginalTime) || (autoOpenedForPlayer && timeSincePlayerSensed > ReturnToOriginalTime)))
		{
			openedByNPC = false;
			autoOpenedForPlayer = false;
			((Collider)PlayerBlocker).enabled = false;
			SetIsOpen_Server(open: false, EDoorSide.Interior, openedForPlayer: false);
		}
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (IsOpen)
		{
			SetIsOpen(connection, open: true, lastOpenSide);
		}
	}

	public virtual void InteriorHandleHovered()
	{
		InteractableObject[] interiorIntObjs;
		if (CanPlayerAccess(EDoorSide.Interior, out var reason))
		{
			interiorIntObjs = InteriorIntObjs;
			foreach (InteractableObject obj in interiorIntObjs)
			{
				obj.SetMessage(IsOpen ? "Close" : "Open");
				obj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			return;
		}
		interiorIntObjs = InteriorIntObjs;
		foreach (InteractableObject interactableObject in interiorIntObjs)
		{
			if (reason != string.Empty)
			{
				interactableObject.SetMessage(reason);
				interactableObject.SetInteractableState(InteractableObject.EInteractableState.Invalid);
			}
			else
			{
				interactableObject.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
		}
	}

	public virtual void InteriorHandleInteracted()
	{
		if (CanPlayerAccess(EDoorSide.Interior))
		{
			if (!IsOpen && (Object)(object)InteriorDoorHandleAnimation != (Object)null)
			{
				InteriorDoorHandleAnimation.Play();
			}
			SetIsOpen_Server(!IsOpen, EDoorSide.Interior, openedForPlayer: false);
		}
	}

	public virtual void ExteriorHandleHovered()
	{
		InteractableObject[] exteriorIntObjs;
		if (CanPlayerAccess(EDoorSide.Exterior, out var reason))
		{
			exteriorIntObjs = ExteriorIntObjs;
			foreach (InteractableObject obj in exteriorIntObjs)
			{
				obj.SetMessage(IsOpen ? "Close" : "Open");
				obj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			return;
		}
		exteriorIntObjs = ExteriorIntObjs;
		foreach (InteractableObject interactableObject in exteriorIntObjs)
		{
			if (reason != string.Empty)
			{
				interactableObject.SetMessage(reason);
				interactableObject.SetInteractableState(InteractableObject.EInteractableState.Invalid);
			}
			else
			{
				interactableObject.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
		}
	}

	public virtual void ExteriorHandleInteracted()
	{
		if (CanPlayerAccess(EDoorSide.Exterior))
		{
			if (!IsOpen && (Object)(object)ExteriorDoorHandleAnimation != (Object)null)
			{
				ExteriorDoorHandleAnimation.Play();
			}
			SetIsOpen_Server(!IsOpen, EDoorSide.Exterior, openedForPlayer: false);
		}
	}

	public bool CanPlayerAccess(EDoorSide side)
	{
		string reason;
		return CanPlayerAccess(side, out reason);
	}

	protected virtual bool CanPlayerAccess(EDoorSide side, out string reason)
	{
		reason = noAccessErrorMessage;
		switch (side)
		{
		case EDoorSide.Interior:
			if (PlayerAccess != EDoorAccess.Open)
			{
				return PlayerAccess == EDoorAccess.ExitOnly;
			}
			return true;
		case EDoorSide.Exterior:
			if (PlayerAccess != EDoorAccess.Open)
			{
				return PlayerAccess == EDoorAccess.EnterOnly;
			}
			return true;
		default:
			return false;
		}
	}

	public virtual void NPCVicinityEnter(EDoorSide side)
	{
		if (InstanceFinder.IsServer)
		{
			timeSinceNPCSensed = 0f;
			detectedNPCCount++;
			if (OpenableByNPCs && PlayerAccess != EDoorAccess.Open)
			{
				((Collider)PlayerBlocker).enabled = true;
			}
			if (!IsOpen && OpenableByNPCs)
			{
				openedByNPC = true;
				SetIsOpen_Server(open: true, side, openedForPlayer: false);
			}
		}
	}

	public virtual void NPCVicinityExit(EDoorSide side)
	{
		if (InstanceFinder.IsServer)
		{
			detectedNPCCount--;
		}
	}

	public virtual void PlayerVicinityEnter(EDoorSide side)
	{
		if (InstanceFinder.IsServer)
		{
			timeSincePlayerSensed = 0f;
			detectedPlayerCount++;
			if (IsOpen)
			{
				playerDetectedSinceOpened = true;
			}
			if (!IsOpen && AutoOpenForPlayer && CanPlayerAccess(side))
			{
				autoOpenedForPlayer = true;
				SetIsOpen_Server(open: true, side, openedForPlayer: true);
			}
		}
	}

	public virtual void PlayerVicinityExit(EDoorSide side)
	{
		if (InstanceFinder.IsServer)
		{
			detectedPlayerCount--;
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetIsOpen_Server(bool open, EDoorSide accessSide, bool openedForPlayer)
	{
		RpcWriter___Server_SetIsOpen_Server_1319291243(open, accessSide, openedForPlayer);
		RpcLogic___SetIsOpen_Server_1319291243(open, accessSide, openedForPlayer);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetIsOpen(NetworkConnection conn, bool open, EDoorSide openSide)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetIsOpen_3381113727(conn, open, openSide);
			RpcLogic___SetIsOpen_3381113727(conn, open, openSide);
		}
		else
		{
			RpcWriter___Target_SetIsOpen_3381113727(conn, open, openSide);
		}
	}

	public virtual void SetIsOpen(bool open, EDoorSide openSide)
	{
		if (IsOpen != open)
		{
			timeInCurrentState = 0f;
		}
		IsOpen = open;
		if (IsOpen)
		{
			playerDetectedSinceOpened = false;
		}
		lastOpenSide = openSide;
		if (IsOpen)
		{
			onDoorOpened.Invoke(openSide);
		}
		else
		{
			onDoorClosed.Invoke();
		}
	}

	protected virtual void CheckAutoCloseForDistantPlayer()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (InstanceFinder.IsServer && IsOpen && !(timeSinceNPCSensed < ReturnToOriginalTime) && !(timeSincePlayerSensed < ReturnToOriginalTime))
		{
			Player.GetClosestPlayer(((Component)this).transform.position, out var distance);
			if (distance > 40f)
			{
				SetIsOpen_Server(open: false, EDoorSide.Interior, openedForPlayer: false);
			}
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
		if (!NetworkInitialize___EarlyScheduleOne_002EDoors_002EDoorControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EDoors_002EDoorControllerAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SetIsOpen_Server_1319291243));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetIsOpen_3381113727));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_SetIsOpen_3381113727));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EDoors_002EDoorControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EDoors_002EDoorControllerAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetIsOpen_Server_1319291243(bool open, EDoorSide accessSide, bool openedForPlayer)
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
			((Writer)writer).WriteBoolean(open);
			GeneratedWriters___Internal.Write___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerated((Writer)(object)writer, accessSide);
			((Writer)writer).WriteBoolean(openedForPlayer);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetIsOpen_Server_1319291243(bool open, EDoorSide accessSide, bool openedForPlayer)
	{
		autoOpenedForPlayer = openedForPlayer;
		if (openedForPlayer)
		{
			timeSincePlayerSensed = 0f;
		}
		SetIsOpen(null, open, accessSide);
	}

	private void RpcReader___Server_SetIsOpen_Server_1319291243(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		bool open = ((Reader)PooledReader0).ReadBoolean();
		EDoorSide accessSide = GeneratedReaders___Internal.Read___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool openedForPlayer = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetIsOpen_Server_1319291243(open, accessSide, openedForPlayer);
		}
	}

	private void RpcWriter___Observers_SetIsOpen_3381113727(NetworkConnection conn, bool open, EDoorSide openSide)
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
			((Writer)writer).WriteBoolean(open);
			GeneratedWriters___Internal.Write___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerated((Writer)(object)writer, openSide);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetIsOpen_3381113727(NetworkConnection conn, bool open, EDoorSide openSide)
	{
		SetIsOpen(open, openSide);
	}

	private void RpcReader___Observers_SetIsOpen_3381113727(PooledReader PooledReader0, Channel channel)
	{
		bool open = ((Reader)PooledReader0).ReadBoolean();
		EDoorSide openSide = GeneratedReaders___Internal.Read___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetIsOpen_3381113727(null, open, openSide);
		}
	}

	private void RpcWriter___Target_SetIsOpen_3381113727(NetworkConnection conn, bool open, EDoorSide openSide)
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
			((Writer)writer).WriteBoolean(open);
			GeneratedWriters___Internal.Write___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerated((Writer)(object)writer, openSide);
			((NetworkBehaviour)this).SendTargetRpc(2u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetIsOpen_3381113727(PooledReader PooledReader0, Channel channel)
	{
		bool open = ((Reader)PooledReader0).ReadBoolean();
		EDoorSide openSide = GeneratedReaders___Internal.Read___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetIsOpen_3381113727(((NetworkBehaviour)this).LocalConnection, open, openSide);
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EDoors_002EDoorController_Assembly_002DCSharp_002Edll()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		((Collider)PlayerBlocker).enabled = false;
		InteractableObject[] interiorIntObjs = InteriorIntObjs;
		foreach (InteractableObject obj in interiorIntObjs)
		{
			obj.onHovered.AddListener(new UnityAction(InteriorHandleHovered));
			obj.onInteractStart.AddListener(new UnityAction(InteriorHandleInteracted));
			obj.SetMessage(IsOpen ? "Close" : "Open");
		}
		interiorIntObjs = ExteriorIntObjs;
		foreach (InteractableObject obj2 in interiorIntObjs)
		{
			obj2.onHovered.AddListener(new UnityAction(ExteriorHandleHovered));
			obj2.onInteractStart.AddListener(new UnityAction(ExteriorHandleInteracted));
			obj2.SetMessage(IsOpen ? "Close" : "Open");
		}
		if (((Component)this).gameObject.isStatic)
		{
			Console.LogError("DoorController is static! Doors should not be static!", (Object)(object)((Component)this).gameObject);
		}
		if (AutoCloseOnDistantPlayer)
		{
			((MonoBehaviour)this).InvokeRepeating("CheckAutoCloseForDistantPlayer", 2f, 2f);
		}
	}
}
