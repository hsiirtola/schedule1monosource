using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.ItemFramework;
using ScheduleOne.Law;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Product;
using ScheduleOne.Vehicles;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Behaviour;

public class CheckpointBehaviour : Behaviour
{
	public const float LOOK_TIME = 1.5f;

	private float currentLookTime;

	private bool trunkOpened;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECheckpointBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECheckpointBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public CheckpointManager.ECheckpointLocation AssignedCheckpoint { get; protected set; }

	public RoadCheckpoint Checkpoint { get; protected set; }

	public bool IsSearching { get; protected set; }

	public LandVehicle CurrentSearchedVehicle { get; protected set; }

	public Player Initiator { get; protected set; }

	private Transform standPoint => Checkpoint.StandPoints[Mathf.Clamp(Checkpoint.AssignedNPCs.IndexOf(base.Npc), 0, Checkpoint.StandPoints.Length - 1)];

	private DialogueDatabase dialogueDatabase => base.Npc.DialogueHandler.Database;

	public override void Activate()
	{
		base.Activate();
		Checkpoint = NetworkSingleton<CheckpointManager>.Instance.GetCheckpoint(AssignedCheckpoint);
		if (!Checkpoint.AssignedNPCs.Contains(base.Npc))
		{
			Checkpoint.AssignedNPCs.Add(base.Npc);
		}
		Checkpoint.onPlayerWalkThrough.AddListener((UnityAction<Player>)PlayerWalkedThroughCheckPoint);
	}

	public override void Resume()
	{
		base.Resume();
		Checkpoint = NetworkSingleton<CheckpointManager>.Instance.GetCheckpoint(AssignedCheckpoint);
		if (!Checkpoint.AssignedNPCs.Contains(base.Npc))
		{
			Checkpoint.AssignedNPCs.Add(base.Npc);
		}
		Checkpoint.onPlayerWalkThrough.AddListener((UnityAction<Player>)PlayerWalkedThroughCheckPoint);
	}

	public override void Deactivate()
	{
		base.Deactivate();
		IsSearching = false;
		if (Checkpoint.AssignedNPCs.Contains(base.Npc))
		{
			Checkpoint.AssignedNPCs.Remove(base.Npc);
		}
		if ((Object)(object)CurrentSearchedVehicle != (Object)null && trunkOpened)
		{
			CurrentSearchedVehicle.Trunk.SetIsOpen(open: false);
		}
		Checkpoint.onPlayerWalkThrough.RemoveListener((UnityAction<Player>)PlayerWalkedThroughCheckPoint);
	}

	public override void Pause()
	{
		base.Pause();
		IsSearching = false;
		if ((Object)(object)CurrentSearchedVehicle != (Object)null && trunkOpened)
		{
			CurrentSearchedVehicle.Trunk.SetIsOpen(open: false);
		}
		Checkpoint.onPlayerWalkThrough.RemoveListener((UnityAction<Player>)PlayerWalkedThroughCheckPoint);
	}

	public override void OnActiveTick()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (IsSearching && !base.Npc.Movement.IsMoving && base.Npc.Movement.IsAsCloseAsPossible(GetSearchPoint()))
		{
			if (!CurrentSearchedVehicle.Trunk.IsOpen)
			{
				CurrentSearchedVehicle.Trunk?.SetIsOpen(open: true);
				trunkOpened = true;
			}
		}
		else if (trunkOpened && (Object)(object)CurrentSearchedVehicle != (Object)null)
		{
			CurrentSearchedVehicle.Trunk?.SetIsOpen(open: false);
		}
		if ((Object)(object)Checkpoint == (Object)null || Checkpoint.ActivationState == RoadCheckpoint.ECheckpointState.Disabled)
		{
			Disable_Networked(null);
		}
		else if (!IsSearching)
		{
			if (base.Npc.Movement.IsMoving)
			{
				return;
			}
			if (base.Npc.Movement.IsAsCloseAsPossible(standPoint.position))
			{
				if (!base.Npc.Movement.FaceDirectionInProgress)
				{
					base.Npc.Movement.FaceDirection(standPoint.forward);
				}
			}
			else if (base.Npc.Movement.CanMove())
			{
				base.Npc.Movement.SetDestination(standPoint.position);
			}
		}
		else if (!Checkpoint.SearchArea1.vehicles.Contains(CurrentSearchedVehicle) && !Checkpoint.SearchArea2.vehicles.Contains(CurrentSearchedVehicle))
		{
			StopSearch();
		}
		else
		{
			if (base.Npc.Movement.IsMoving)
			{
				return;
			}
			if (base.Npc.Movement.IsAsCloseAsPossible(GetSearchPoint(), 1f))
			{
				if (!base.Npc.Movement.FaceDirectionInProgress)
				{
					base.Npc.Movement.FacePoint(((Component)CurrentSearchedVehicle).transform.position);
				}
				currentLookTime += 0.5f;
				if (currentLookTime >= 1.5f)
				{
					ConcludeSearch();
				}
			}
			else
			{
				currentLookTime = 0f;
				if (base.Npc.Movement.CanMove())
				{
					base.Npc.Movement.SetDestination(GetSearchPoint());
				}
			}
		}
	}

	[ObserversRpc(RunLocally = true)]
	public void SetCheckpoint(CheckpointManager.ECheckpointLocation loc)
	{
		RpcWriter___Observers_SetCheckpoint_4087078542(loc);
		RpcLogic___SetCheckpoint_4087078542(loc);
	}

	[ObserversRpc(RunLocally = true)]
	public void SetInitiator(NetworkObject init)
	{
		RpcWriter___Observers_SetInitiator_3323014238(init);
		RpcLogic___SetInitiator_3323014238(init);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void StartSearch(NetworkObject targetVehicle, NetworkObject initiator)
	{
		RpcWriter___Server_StartSearch_3694055493(targetVehicle, initiator);
		RpcLogic___StartSearch_3694055493(targetVehicle, initiator);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void StopSearch()
	{
		RpcWriter___Server_StopSearch_2166136261();
		RpcLogic___StopSearch_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	public void SetIsSearching(bool s)
	{
		RpcWriter___Observers_SetIsSearching_1140765316(s);
		RpcLogic___SetIsSearching_1140765316(s);
	}

	private Vector3 GetSearchPoint()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)CurrentSearchedVehicle).transform.position - ((Component)CurrentSearchedVehicle).transform.forward * (CurrentSearchedVehicle.BoundingBoxDimensions.z / 2f + 0.75f);
	}

	[ObserversRpc(RunLocally = true)]
	private void ConcludeSearch()
	{
		RpcWriter___Observers_ConcludeSearch_2166136261();
		RpcLogic___ConcludeSearch_2166136261();
	}

	private bool DoesVehicleContainIllicitItems()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)CurrentSearchedVehicle == (Object)null)
		{
			return false;
		}
		CurrentSearchedVehicle.Storage.ItemSlots.Select((ItemSlot x) => x.ItemInstance).ToList();
		foreach (ItemSlot itemSlot in CurrentSearchedVehicle.Storage.ItemSlots)
		{
			if (itemSlot.ItemInstance == null)
			{
				continue;
			}
			if (itemSlot.ItemInstance is ProductItemInstance)
			{
				ProductItemInstance productItemInstance = itemSlot.ItemInstance as ProductItemInstance;
				if ((Object)(object)productItemInstance.AppliedPackaging == (Object)null || productItemInstance.AppliedPackaging.StealthLevel <= Checkpoint.MaxStealthLevel)
				{
					return true;
				}
			}
			else if ((int)((BaseItemDefinition)itemSlot.ItemInstance.Definition).legalStatus != 0)
			{
				return true;
			}
		}
		return false;
	}

	private void PlayerWalkedThroughCheckPoint(Player player)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (!InstanceFinder.IsServer || player.CrimeData.TimeSinceLastBodySearch < 60f || player.CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None || NetworkSingleton<CurfewManager>.Instance.IsCurrentlyActive || Checkpoint.AssignedNPCs.Count == 0)
		{
			return;
		}
		List<NPC> list = new List<NPC>();
		for (int i = 0; i < Checkpoint.AssignedNPCs.Count; i++)
		{
			Transform val = Checkpoint.StandPoints[Mathf.Clamp(i, 0, Checkpoint.StandPoints.Length - 1)];
			if (Vector3.Distance(((Component)Checkpoint.AssignedNPCs[i]).transform.position, val.position) < 10f)
			{
				list.Add(Checkpoint.AssignedNPCs[i]);
			}
		}
		NPC nPC = null;
		float num = float.MaxValue;
		for (int j = 0; j < list.Count; j++)
		{
			float num2 = Vector3.Distance(((Component)player).transform.position, ((Component)list[j]).transform.position);
			if (num2 < num)
			{
				num = num2;
				nPC = list[j];
			}
		}
		if (!(num > 15f) && !((Object)(object)nPC != (Object)(object)base.Npc))
		{
			player.CrimeData.ResetBodysearchCooldown();
			(base.Npc as PoliceOfficer).ConductBodySearch(player);
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
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECheckpointBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ECheckpointBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_SetCheckpoint_4087078542));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetInitiator_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_StartSearch_3694055493));
			((NetworkBehaviour)this).RegisterServerRpc(3u, new ServerRpcDelegate(RpcReader___Server_StopSearch_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_SetIsSearching_1140765316));
			((NetworkBehaviour)this).RegisterObserversRpc(5u, new ClientRpcDelegate(RpcReader___Observers_ConcludeSearch_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECheckpointBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ECheckpointBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_SetCheckpoint_4087078542(CheckpointManager.ECheckpointLocation loc)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerated((Writer)(object)writer, loc);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetCheckpoint_4087078542(CheckpointManager.ECheckpointLocation loc)
	{
		AssignedCheckpoint = loc;
	}

	private void RpcReader___Observers_SetCheckpoint_4087078542(PooledReader PooledReader0, Channel channel)
	{
		CheckpointManager.ECheckpointLocation loc = GeneratedReaders___Internal.Read___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetCheckpoint_4087078542(loc);
		}
	}

	private void RpcWriter___Observers_SetInitiator_3323014238(NetworkObject init)
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
			((Writer)writer).WriteNetworkObject(init);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetInitiator_3323014238(NetworkObject init)
	{
		Initiator = ((Component)init).GetComponent<Player>();
	}

	private void RpcReader___Observers_SetInitiator_3323014238(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject init = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetInitiator_3323014238(init);
		}
	}

	private void RpcWriter___Server_StartSearch_3694055493(NetworkObject targetVehicle, NetworkObject initiator)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkObject(targetVehicle);
			((Writer)writer).WriteNetworkObject(initiator);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___StartSearch_3694055493(NetworkObject targetVehicle, NetworkObject initiator)
	{
		currentLookTime = 0f;
		SetIsSearching(s: true);
		SetInitiator(initiator);
		CurrentSearchedVehicle = ((Component)targetVehicle).GetComponent<LandVehicle>();
		if (InstanceFinder.IsServer)
		{
			base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("searchingvehicle", 20, 0.15f));
		}
	}

	private void RpcReader___Server_StartSearch_3694055493(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject targetVehicle = ((Reader)PooledReader0).ReadNetworkObject();
		NetworkObject initiator = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___StartSearch_3694055493(targetVehicle, initiator);
		}
	}

	private void RpcWriter___Server_StopSearch_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(3u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___StopSearch_2166136261()
	{
		SetIsSearching(s: false);
		if ((Object)(object)CurrentSearchedVehicle != (Object)null && trunkOpened)
		{
			CurrentSearchedVehicle.Trunk?.SetIsOpen(open: false);
		}
		CurrentSearchedVehicle = null;
		Initiator = null;
		if (InstanceFinder.IsServer)
		{
			base.Npc.Movement.SpeedController.RemoveSpeedControl("searchingvehicle");
		}
	}

	private void RpcReader___Server_StopSearch_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___StopSearch_2166136261();
		}
	}

	private void RpcWriter___Observers_SetIsSearching_1140765316(bool s)
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
			((Writer)writer).WriteBoolean(s);
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetIsSearching_1140765316(bool s)
	{
		IsSearching = s;
		if (IsSearching)
		{
			base.Npc.DialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Police, "checkpoint_search_start"), 3f);
		}
	}

	private void RpcReader___Observers_SetIsSearching_1140765316(PooledReader PooledReader0, Channel channel)
	{
		bool s = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetIsSearching_1140765316(s);
		}
	}

	private void RpcWriter___Observers_ConcludeSearch_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(5u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ConcludeSearch_2166136261()
	{
		if ((Object)(object)CurrentSearchedVehicle == (Object)null)
		{
			Console.LogWarning("ConcludeSearch called with null vehicle");
		}
		if ((Object)(object)CurrentSearchedVehicle != (Object)null && DoesVehicleContainIllicitItems() && (Object)(object)Initiator != (Object)null)
		{
			base.Npc.DialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Police, "checkpoint_items_found"), 3f);
			if ((Object)(object)Initiator == (Object)(object)Player.Local)
			{
				Player.Local.CrimeData.AddCrime(new TransportingIllicitItems());
				Player.Local.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
				(base.Npc as PoliceOfficer).BeginFootPursuit_Networked(Player.Local.PlayerCode);
			}
		}
		else
		{
			base.Npc.DialogueHandler.ShowWorldspaceDialogue(dialogueDatabase.GetLine(EDialogueModule.Police, "checkpoint_all_clear"), 3f);
			if (Checkpoint.SearchArea1.vehicles.Contains(CurrentSearchedVehicle))
			{
				Checkpoint.SetGate1Open(o: true);
			}
			else if (Checkpoint.SearchArea1.vehicles.Contains(CurrentSearchedVehicle))
			{
				Checkpoint.SetGate2Open(o: true);
			}
			else
			{
				Checkpoint.SetGate1Open(o: true);
				Checkpoint.SetGate2Open(o: true);
			}
		}
		StopSearch();
	}

	private void RpcReader___Observers_ConcludeSearch_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ConcludeSearch_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
