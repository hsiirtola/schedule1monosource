using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Map;

public class SewerManager : NetworkSingleton<SewerManager>, IBaseSaveable, ISaveable
{
	[Serializable]
	public class KeyPossessor
	{
		public NPC NPC;

		[Tooltip("Description of the NPC for Oscar's key location dialogue.")]
		public string NPCDescription;
	}

	public ItemDefinition SewerKeyItem;

	public AudioSourceController SewerUnlockSound;

	public NetworkedItemPickup RandomWorldSewerKeyPickup;

	public Transform[] RandomSewerKeyLocations;

	public SewerKing SewerKingNPC;

	public SewerGoblin SewerGoblinNPC;

	public KeyPossessor[] SewerKeyPossessors;

	public SewerMushrooms SewerMushrooms;

	private SewerLoader loader = new SewerLoader();

	private bool NetworkInitialize___EarlyScheduleOne_002EMap_002ESewerManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EMap_002ESewerManagerAssembly_002DCSharp_002Edll_Excuted;

	public bool IsSewerUnlocked { get; private set; }

	public bool IsRandomWorldKeyCollected { get; private set; }

	public int RandomSewerKeyLocationIndex { get; set; } = -1;

	public bool HasSewerKingBeenDefeated { get; private set; }

	public int RandomSewerPossessorIndex { get; set; } = -1;

	public string SaveFolderName => "Sewer";

	public string SaveFileName => "Sewer";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public int LoadOrder { get; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EMap_002ESewerManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		for (int i = 0; i < RandomSewerKeyLocations.Length; i++)
		{
			Singleton<Map>.Instance.GetRegionFromPosition(RandomSewerKeyLocations[i].position);
			((Component)RandomSewerKeyLocations[i]).gameObject.SetActive(false);
		}
		TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
		timeManager.onSleepEnd = (Action)Delegate.Combine(timeManager.onSleepEnd, new Action(EnsureKeyPosessorHasKey));
		InitializeSaveable();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost)
		{
			if (IsSewerUnlocked)
			{
				SetSewerUnlocked_Client(connection);
			}
			if (IsRandomWorldKeyCollected)
			{
				SetRandomKeyCollected_Client(connection);
			}
			else
			{
				SetSewerKeyLocation(null, RandomSewerKeyLocationIndex);
			}
			if ((Object)(object)SewerKingNPC != (Object)null && !((Component)SewerKingNPC).gameObject.activeSelf)
			{
				DisableSewerKing(connection);
			}
			SetRandomKeyPossessor(connection, RandomSewerPossessorIndex);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetSewerUnlocked_Server()
	{
		RpcWriter___Server_SetSewerUnlocked_Server_2166136261();
		RpcLogic___SetSewerUnlocked_Server_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetSewerUnlocked_Client(NetworkConnection conn)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetSewerUnlocked_Client_328543758(conn);
			RpcLogic___SetSewerUnlocked_Client_328543758(conn);
		}
		else
		{
			RpcWriter___Target_SetSewerUnlocked_Client_328543758(conn);
		}
	}

	public void SetRandomWorldKeyCollected()
	{
		SetRandomKeyCollected_Server();
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void SetRandomKeyCollected_Server()
	{
		RpcWriter___Server_SetRandomKeyCollected_Server_2166136261();
		RpcLogic___SetRandomKeyCollected_Server_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetRandomKeyCollected_Client(NetworkConnection conn)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetRandomKeyCollected_Client_328543758(conn);
			RpcLogic___SetRandomKeyCollected_Client_328543758(conn);
		}
		else
		{
			RpcWriter___Target_SetRandomKeyCollected_Client_328543758(conn);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetSewerKeyLocation(NetworkConnection conn, int locationIndex)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetSewerKeyLocation_2681120339(conn, locationIndex);
			RpcLogic___SetSewerKeyLocation_2681120339(conn, locationIndex);
		}
		else
		{
			RpcWriter___Target_SetSewerKeyLocation_2681120339(conn, locationIndex);
		}
	}

	private void SewerKingDefeated()
	{
		HasSewerKingBeenDefeated = true;
	}

	[ObserversRpc]
	[TargetRpc]
	private void DisableSewerKing(NetworkConnection conn)
	{
		if (conn == null)
		{
			RpcWriter___Observers_DisableSewerKing_328543758(conn);
		}
		else
		{
			RpcWriter___Target_DisableSewerKing_328543758(conn);
		}
	}

	public List<Player> GetPlayersInSewer()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		List<Player> list = new List<Player>();
		foreach (Player player in Player.PlayerList)
		{
			if (Singleton<SewerCameraPresense>.Instance.IsPointInSewerArea(player.CenterPointTransform.position))
			{
				list.Add(player);
			}
		}
		return list;
	}

	public virtual string GetSaveString()
	{
		return new SewerData(IsSewerUnlocked, IsRandomWorldKeyCollected, RandomSewerKeyLocationIndex, HasSewerKingBeenDefeated, SewerGoblinNPC.HoursSinceLastDeploy, RandomSewerPossessorIndex, SewerMushrooms.GetActiveMushroomLocationIndices()).GetJson();
	}

	public void Load(SewerData sewerData)
	{
		IsSewerUnlocked = sewerData.IsSewerUnlocked;
		IsRandomWorldKeyCollected = sewerData.IsRandomWorldKeyCollected;
		RandomSewerKeyLocationIndex = sewerData.RandomSewerKeyLocationIndex;
		if (IsRandomWorldKeyCollected)
		{
			SetRandomWorldKeyCollected();
		}
		else
		{
			SetSewerKeyLocation(null, RandomSewerKeyLocationIndex);
		}
		HasSewerKingBeenDefeated = sewerData.HasSewerKingBeenDefeated;
		if (HasSewerKingBeenDefeated)
		{
			DisableSewerKing(null);
		}
		SewerGoblinNPC.HoursSinceLastDeploy = sewerData.HoursSinceLastSewerGoblinAppearance;
		if (sewerData.RandomKeyPossessorIndex == -1 || !sewerData.RandomKeyPossessorSet)
		{
			sewerData.RandomKeyPossessorIndex = Random.Range(0, SewerKeyPossessors.Length - 1);
			Console.Log("Assigning new random sewer possessor index: " + sewerData.RandomKeyPossessorIndex);
		}
		SetRandomKeyPossessor(null, sewerData.RandomKeyPossessorIndex);
		SewerMushrooms.Load(sewerData);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetRandomKeyPossessor(NetworkConnection conn, int possessorIndex)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetRandomKeyPossessor_2681120339(conn, possessorIndex);
			RpcLogic___SetRandomKeyPossessor_2681120339(conn, possessorIndex);
		}
		else
		{
			RpcWriter___Target_SetRandomKeyPossessor_2681120339(conn, possessorIndex);
		}
	}

	private void AskedAboutSewerKey()
	{
		GetSewerKeyPossessor().NPC.ShowWorldSpaceDialogue("None of your business!", 3f);
		GetSewerKeyPossessor().NPC.PlayVO(EVOLineType.Annoyed, network: true);
	}

	private void EnsureKeyPosessorHasKey()
	{
		if (InstanceFinder.IsServer)
		{
			NPC nPC = GetSewerKeyPossessor().NPC;
			if (((IItemSlotOwner)nPC.Inventory).GetQuantityOfItem(((BaseItemDefinition)SewerKeyItem).ID) == 0)
			{
				nPC.Inventory.InsertItem(SewerKeyItem.GetDefaultInstance());
			}
		}
	}

	public KeyPossessor GetSewerKeyPossessor()
	{
		return SewerKeyPossessors[Mathf.Clamp(RandomSewerPossessorIndex, 0, SewerKeyPossessors.Length - 1)];
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
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EMap_002ESewerManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EMap_002ESewerManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SetSewerUnlocked_Server_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetSewerUnlocked_Client_328543758));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_SetSewerUnlocked_Client_328543758));
			((NetworkBehaviour)this).RegisterServerRpc(3u, new ServerRpcDelegate(RpcReader___Server_SetRandomKeyCollected_Server_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_SetRandomKeyCollected_Client_328543758));
			((NetworkBehaviour)this).RegisterTargetRpc(5u, new ClientRpcDelegate(RpcReader___Target_SetRandomKeyCollected_Client_328543758));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_SetSewerKeyLocation_2681120339));
			((NetworkBehaviour)this).RegisterTargetRpc(7u, new ClientRpcDelegate(RpcReader___Target_SetSewerKeyLocation_2681120339));
			((NetworkBehaviour)this).RegisterObserversRpc(8u, new ClientRpcDelegate(RpcReader___Observers_DisableSewerKing_328543758));
			((NetworkBehaviour)this).RegisterTargetRpc(9u, new ClientRpcDelegate(RpcReader___Target_DisableSewerKing_328543758));
			((NetworkBehaviour)this).RegisterObserversRpc(10u, new ClientRpcDelegate(RpcReader___Observers_SetRandomKeyPossessor_2681120339));
			((NetworkBehaviour)this).RegisterTargetRpc(11u, new ClientRpcDelegate(RpcReader___Target_SetRandomKeyPossessor_2681120339));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EMap_002ESewerManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EMap_002ESewerManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetSewerUnlocked_Server_2166136261()
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

	public void RpcLogic___SetSewerUnlocked_Server_2166136261()
	{
		IsSewerUnlocked = true;
	}

	private void RpcReader___Server_SetSewerUnlocked_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetSewerUnlocked_Server_2166136261();
		}
	}

	private void RpcWriter___Observers_SetSewerUnlocked_Client_328543758(NetworkConnection conn)
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

	private void RpcLogic___SetSewerUnlocked_Client_328543758(NetworkConnection conn)
	{
		Console.Log("Sewer unlocked!");
		IsSewerUnlocked = true;
	}

	private void RpcReader___Observers_SetSewerUnlocked_Client_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetSewerUnlocked_Client_328543758(null);
		}
	}

	private void RpcWriter___Target_SetSewerUnlocked_Client_328543758(NetworkConnection conn)
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

	private void RpcReader___Target_SetSewerUnlocked_Client_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetSewerUnlocked_Client_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	private void RpcWriter___Server_SetRandomKeyCollected_Server_2166136261()
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

	private void RpcLogic___SetRandomKeyCollected_Server_2166136261()
	{
		SetRandomKeyCollected_Client(null);
	}

	private void RpcReader___Server_SetRandomKeyCollected_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetRandomKeyCollected_Server_2166136261();
		}
	}

	private void RpcWriter___Observers_SetRandomKeyCollected_Client_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetRandomKeyCollected_Client_328543758(NetworkConnection conn)
	{
		IsRandomWorldKeyCollected = true;
		((Component)RandomWorldSewerKeyPickup).gameObject.SetActive(false);
	}

	private void RpcReader___Observers_SetRandomKeyCollected_Client_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetRandomKeyCollected_Client_328543758(null);
		}
	}

	private void RpcWriter___Target_SetRandomKeyCollected_Client_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendTargetRpc(5u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetRandomKeyCollected_Client_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetRandomKeyCollected_Client_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	private void RpcWriter___Observers_SetSewerKeyLocation_2681120339(NetworkConnection conn, int locationIndex)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(locationIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetSewerKeyLocation_2681120339(NetworkConnection conn, int locationIndex)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Console.Log("Setting sewer key location to index: " + locationIndex);
		RandomSewerKeyLocationIndex = locationIndex;
		((Component)RandomWorldSewerKeyPickup).transform.position = RandomSewerKeyLocations[RandomSewerKeyLocationIndex].position;
		((Component)RandomWorldSewerKeyPickup).transform.rotation = RandomSewerKeyLocations[RandomSewerKeyLocationIndex].rotation;
	}

	private void RpcReader___Observers_SetSewerKeyLocation_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int locationIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetSewerKeyLocation_2681120339(null, locationIndex);
		}
	}

	private void RpcWriter___Target_SetSewerKeyLocation_2681120339(NetworkConnection conn, int locationIndex)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(locationIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(7u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetSewerKeyLocation_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int locationIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetSewerKeyLocation_2681120339(((NetworkBehaviour)this).LocalConnection, locationIndex);
		}
	}

	private void RpcWriter___Observers_DisableSewerKing_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendObserversRpc(8u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___DisableSewerKing_328543758(NetworkConnection conn)
	{
		((Component)SewerKingNPC).gameObject.SetActive(false);
	}

	private void RpcReader___Observers_DisableSewerKing_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___DisableSewerKing_328543758(null);
		}
	}

	private void RpcWriter___Target_DisableSewerKing_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendTargetRpc(9u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_DisableSewerKing_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___DisableSewerKing_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	private void RpcWriter___Observers_SetRandomKeyPossessor_2681120339(NetworkConnection conn, int possessorIndex)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(possessorIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(10u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetRandomKeyPossessor_2681120339(NetworkConnection conn, int possessorIndex)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		if (possessorIndex != RandomSewerPossessorIndex)
		{
			RandomSewerPossessorIndex = Mathf.Clamp(possessorIndex, 0, SewerKeyPossessors.Length - 1);
			Console.Log("Setting sewer key possessor to index: " + RandomSewerPossessorIndex);
			if (InstanceFinder.IsServer)
			{
				EnsureKeyPosessorHasKey();
			}
			NPC nPC = GetSewerKeyPossessor().NPC;
			DialogueController.DialogueChoice dialogueChoice = new DialogueController.DialogueChoice
			{
				ChoiceText = "Do you have a sewer key?"
			};
			dialogueChoice.onChoosen.AddListener(new UnityAction(AskedAboutSewerKey));
			((Component)nPC.DialogueHandler).GetComponent<DialogueController>().AddDialogueChoice(dialogueChoice, -10);
		}
	}

	private void RpcReader___Observers_SetRandomKeyPossessor_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int possessorIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetRandomKeyPossessor_2681120339(null, possessorIndex);
		}
	}

	private void RpcWriter___Target_SetRandomKeyPossessor_2681120339(NetworkConnection conn, int possessorIndex)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(possessorIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(11u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetRandomKeyPossessor_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int possessorIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetRandomKeyPossessor_2681120339(((NetworkBehaviour)this).LocalConnection, possessorIndex);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EMap_002ESewerManager_Assembly_002DCSharp_002Edll()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		base.Awake();
		for (int i = 0; i < SewerKeyPossessors.Length; i++)
		{
			if (string.IsNullOrEmpty(SewerKeyPossessors[i].NPCDescription))
			{
				Console.LogError("SewerManager: NPCDescription for key possessor " + SewerKeyPossessors[i].NPC.fullName + " is empty!");
			}
		}
		SetSewerKeyLocation(null, Random.Range(0, RandomSewerKeyLocations.Length));
		SewerKingNPC.Health.onDieOrKnockedOut.AddListener(new UnityAction(SewerKingDefeated));
	}
}
