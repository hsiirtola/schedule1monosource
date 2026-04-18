using System;
using System.Collections;
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
using ScheduleOne.Audio;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.Money;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.Quests;

public class QuestManager : NetworkSingleton<QuestManager>, IBaseSaveable, ISaveable
{
	public enum EQuestAction
	{
		Begin,
		Success,
		Fail,
		Expire,
		Cancel
	}

	public const EQuestState DEFAULT_QUEST_STATE = EQuestState.Inactive;

	public Quest[] DefaultQuests;

	[Header("References")]
	public Transform QuestContainer;

	public Transform ContractContainer;

	public AudioSourceController QuestCompleteSound;

	public AudioSourceController QuestEntryCompleteSound;

	[Header("Prefabs")]
	public Contract ContractPrefab;

	public DeaddropQuest DeaddropCollectionPrefab;

	private QuestsLoader loader = new QuestsLoader();

	private bool NetworkInitialize___EarlyScheduleOne_002EQuests_002EQuestManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EQuests_002EQuestManagerAssembly_002DCSharp_002Edll_Excuted;

	public string SaveFolderName => "Quests";

	public string SaveFileName => "Quests";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; } = true;

	public int LoadOrder { get; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EQuests_002EQuestManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	protected override void Start()
	{
		base.Start();
		((MonoBehaviour)this).InvokeRepeating("UpdateVariables", 0f, 0.5f);
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsLocalClient)
		{
			((MonoBehaviour)this).StartCoroutine(SendQuestStuff());
		}
		IEnumerator SendQuestStuff()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => (Object)(object)Player.GetPlayer(connection) != (Object)null && Player.GetPlayer(connection).playerDataRetrieveReturned));
			foreach (Contract contract in Contract.Contracts)
			{
				if (!((Object)(object)contract == (Object)null) && contract.State == EQuestState.Active)
				{
					ContractInfo contractData = new ContractInfo(contract.Payment, contract.ProductList, contract.DeliveryLocation.GUID.ToString(), contract.DeliveryWindow, contract.Expires, 0, contract.PickupScheduleIndex, isCounterOffer: false);
					NetworkObject dealerObj = null;
					if ((Object)(object)contract.Dealer != (Object)null)
					{
						dealerObj = ((NetworkBehaviour)contract.Dealer).NetworkObject;
					}
					CreateContract_Networked(connection, contract.Title, contract.Description, contract.GUID.ToString(), contract.IsTracked, contract.Customer, contractData, contract.Expiry, contract.AcceptTime, dealerObj);
				}
			}
			foreach (DeaddropQuest deaddropQuest in DeaddropQuest.DeaddropQuests)
			{
				if (!((Object)(object)deaddropQuest == (Object)null) && deaddropQuest.State == EQuestState.Active)
				{
					CreateDeaddropCollectionQuest(connection, deaddropQuest.Drop.GUID.ToString(), deaddropQuest.GUID.ToString());
				}
			}
			Quest[] defaultQuests = DefaultQuests;
			foreach (Quest quest in defaultQuests)
			{
				if ((Object)(object)quest == (Object)null)
				{
					Console.LogError("Default quest is null!");
				}
				else
				{
					for (int num2 = 0; num2 < quest.Entries.Count; num2++)
					{
						if (quest.Entries[num2].State != EQuestState.Inactive)
						{
							ReceiveQuestEntryState(connection, quest.GUID.ToString(), num2, quest.Entries[num2].State);
						}
					}
					if (quest.State != EQuestState.Inactive)
					{
						ReceiveQuestState(connection, quest.GUID.ToString(), quest.State);
					}
					if (quest.IsTracked)
					{
						SetQuestTracked(connection, quest.GUID.ToString(), tracked: true);
					}
				}
			}
		}
	}

	private void UpdateVariables()
	{
		if (InstanceFinder.IsServer)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Active_Contract_Count", Contract.Contracts.Count.ToString());
		}
	}

	public Contract ContractAccepted(Customer customer, ContractInfo contractData, bool track, string guid, Dealer dealer)
	{
		if (!InstanceFinder.IsServer)
		{
			Console.LogError("SendContractAccepted can only be called on the server!");
			return null;
		}
		GameDateTime expiry = new GameDateTime
		{
			time = contractData.DeliveryWindow.WindowEndTime,
			elapsedDays = NetworkSingleton<TimeManager>.Instance.ElapsedDays
		};
		if (NetworkSingleton<TimeManager>.Instance.CurrentTime > contractData.DeliveryWindow.WindowEndTime)
		{
			expiry.elapsedDays++;
		}
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Accepted_Contract_Count", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Accepted_Contract_Count") + 1f).ToString());
		string nameAddress = customer.NPC.GetNameAddress();
		GameDateTime dateTime = NetworkSingleton<TimeManager>.Instance.GetDateTime();
		DeliveryLocation deliveryLocation = GUIDManager.GetObject<DeliveryLocation>(new Guid(contractData.DeliveryLocationGUID));
		string title = "Deal for " + nameAddress;
		string description = nameAddress + " has requested a delivery of " + contractData.Products.GetCommaSeperatedString() + " " + deliveryLocation.GetDescription() + " for " + MoneyManager.FormatAmount(contractData.Payment) + ".";
		QuestEntryData questEntryData = new QuestEntryData(contractData.Products.GetCommaSeperatedString() + ", " + deliveryLocation.LocationName, EQuestState.Inactive);
		Contract result = CreateContract_Local(title, description, new QuestEntryData[1] { questEntryData }, guid, track, customer, contractData.Payment, contractData.Products, contractData.DeliveryLocationGUID, contractData.DeliveryWindow, contractData.Expires, expiry, contractData.PickupScheduleIndex, dateTime, dealer);
		CreateContract_Networked(dealerObj: ((Object)(object)dealer != (Object)null) ? ((NetworkBehaviour)dealer).NetworkObject : null, conn: null, title: title, description: description, guid: guid, tracked: track, customer: ((NetworkBehaviour)customer).NetworkObject, contractData: contractData, expiry: expiry, acceptTime: dateTime);
		return result;
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void CreateContract_Networked(NetworkConnection conn, string title, string description, string guid, bool tracked, NetworkObject customer, ContractInfo contractData, GameDateTime expiry, GameDateTime acceptTime, NetworkObject dealerObj = null)
	{
		if (conn == null)
		{
			RpcWriter___Observers_CreateContract_Networked_2526053753(conn, title, description, guid, tracked, customer, contractData, expiry, acceptTime, dealerObj);
			RpcLogic___CreateContract_Networked_2526053753(conn, title, description, guid, tracked, customer, contractData, expiry, acceptTime, dealerObj);
		}
		else
		{
			RpcWriter___Target_CreateContract_Networked_2526053753(conn, title, description, guid, tracked, customer, contractData, expiry, acceptTime, dealerObj);
		}
	}

	public Contract CreateContract_Local(string title, string description, QuestEntryData[] entries, string guid, bool tracked, Customer customer, float payment, ProductList products, string deliveryLocationGUID, QuestWindowConfig deliveryWindow, bool expires, GameDateTime expiry, int pickupScheduleIndex, GameDateTime acceptTime, Dealer dealer = null)
	{
		Contract component = Object.Instantiate<GameObject>(((Component)ContractPrefab).gameObject, ContractContainer).GetComponent<Contract>();
		component.InitializeContract(title, description, entries, guid, customer, payment, products, deliveryLocationGUID, deliveryWindow, pickupScheduleIndex, acceptTime);
		component.Entries[0].PoILocation = component.DeliveryLocation.CustomerStandPoint;
		component.Entries[0].CreatePoI();
		component.UpdatePoI();
		if (tracked)
		{
			component.SetIsTracked(tracked: true);
		}
		if (expires)
		{
			component.ConfigureExpiry(expires: true, expiry);
		}
		if ((Object)(object)dealer != (Object)null)
		{
			component.SetDealer(dealer);
		}
		component.Begin(network: false);
		return component;
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendQuestAction(string guid, EQuestAction action)
	{
		RpcWriter___Server_SendQuestAction_2848227116(guid, action);
		RpcLogic___SendQuestAction_2848227116(guid, action);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void ReceiveQuestAction(NetworkConnection conn, string guid, EQuestAction action)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ReceiveQuestAction_920727549(conn, guid, action);
			RpcLogic___ReceiveQuestAction_920727549(conn, guid, action);
		}
		else
		{
			RpcWriter___Target_ReceiveQuestAction_920727549(conn, guid, action);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendQuestState(string guid, EQuestState state)
	{
		RpcWriter___Server_SendQuestState_4117703421(guid, state);
		RpcLogic___SendQuestState_4117703421(guid, state);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void ReceiveQuestState(NetworkConnection conn, string guid, EQuestState state)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ReceiveQuestState_3887376304(conn, guid, state);
			RpcLogic___ReceiveQuestState_3887376304(conn, guid, state);
		}
		else
		{
			RpcWriter___Target_ReceiveQuestState_3887376304(conn, guid, state);
		}
	}

	[TargetRpc]
	private void SetQuestTracked(NetworkConnection conn, string guid, bool tracked)
	{
		RpcWriter___Target_SetQuestTracked_619441887(conn, guid, tracked);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendQuestEntryState(string guid, int entryIndex, EQuestState state)
	{
		RpcWriter___Server_SendQuestEntryState_375159588(guid, entryIndex, state);
		RpcLogic___SendQuestEntryState_375159588(guid, entryIndex, state);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void ReceiveQuestEntryState(NetworkConnection conn, string guid, int entryIndex, EQuestState state)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ReceiveQuestEntryState_311789429(conn, guid, entryIndex, state);
			RpcLogic___ReceiveQuestEntryState_311789429(conn, guid, entryIndex, state);
		}
		else
		{
			RpcWriter___Target_ReceiveQuestEntryState_311789429(conn, guid, entryIndex, state);
		}
	}

	[Button]
	public void PrintQuestStates()
	{
		for (int i = 0; i < Quest.Quests.Count; i++)
		{
			Console.Log(Quest.Quests[i].GetQuestTitle() + " state: " + Quest.Quests[i].State);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void CreateDeaddropCollectionQuest(NetworkConnection conn, string dropGUID, string guidString = "")
	{
		if (conn == null)
		{
			RpcWriter___Observers_CreateDeaddropCollectionQuest_3895153758(conn, dropGUID, guidString);
			RpcLogic___CreateDeaddropCollectionQuest_3895153758(conn, dropGUID, guidString);
		}
		else
		{
			RpcWriter___Target_CreateDeaddropCollectionQuest_3895153758(conn, dropGUID, guidString);
		}
	}

	public DeaddropQuest CreateDeaddropCollectionQuest(string dropGUID, string guidString = "")
	{
		Guid guid = ((guidString != "") ? new Guid(guidString) : GUIDManager.GenerateUniqueGUID());
		if (GUIDManager.IsGUIDAlreadyRegistered(guid))
		{
			return null;
		}
		DeadDrop deadDrop = GUIDManager.GetObject<DeadDrop>(new Guid(dropGUID));
		if ((Object)(object)deadDrop == (Object)null)
		{
			Console.LogWarning("Failed to find dead drop with GUID: " + dropGUID);
			return null;
		}
		DeaddropQuest component = Object.Instantiate<GameObject>(((Component)DeaddropCollectionPrefab).gameObject, QuestContainer).GetComponent<DeaddropQuest>();
		component.SetDrop(deadDrop);
		component.Description = "Collect the dead drop " + deadDrop.DeadDropDescription;
		component.SetGUID(guid);
		component.Entries[0].SetEntryTitle(deadDrop.DeadDropName);
		component.Begin();
		return component;
	}

	public void PlayCompleteQuestSound()
	{
		if (QuestEntryCompleteSound.IsPlaying)
		{
			QuestEntryCompleteSound.Stop();
		}
		QuestCompleteSound.Play();
	}

	public void PlayCompleteQuestEntrySound()
	{
		QuestEntryCompleteSound.Play();
	}

	public virtual string GetSaveString()
	{
		List<QuestData> list = new List<QuestData>();
		List<ContractData> list2 = new List<ContractData>();
		List<DeaddropQuestData> list3 = new List<DeaddropQuestData>();
		for (int i = 0; i < Quest.Quests.Count; i++)
		{
			if (!(Quest.Quests[i] is Contract))
			{
				list.Add(Quest.Quests[i].GetSaveData() as QuestData);
			}
		}
		for (int j = 0; j < Contract.Contracts.Count; j++)
		{
			list2.Add(Contract.Contracts[j].GetSaveData() as ContractData);
		}
		for (int k = 0; k < DeaddropQuest.DeaddropQuests.Count; k++)
		{
			list3.Add(DeaddropQuest.DeaddropQuests[k].GetSaveData() as DeaddropQuestData);
		}
		return new QuestManagerData(list.ToArray(), list2.ToArray(), list3.ToArray()).GetJson();
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
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EQuests_002EQuestManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EQuests_002EQuestManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_CreateContract_Networked_2526053753));
			((NetworkBehaviour)this).RegisterTargetRpc(1u, new ClientRpcDelegate(RpcReader___Target_CreateContract_Networked_2526053753));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_SendQuestAction_2848227116));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_ReceiveQuestAction_920727549));
			((NetworkBehaviour)this).RegisterTargetRpc(4u, new ClientRpcDelegate(RpcReader___Target_ReceiveQuestAction_920727549));
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_SendQuestState_4117703421));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_ReceiveQuestState_3887376304));
			((NetworkBehaviour)this).RegisterTargetRpc(7u, new ClientRpcDelegate(RpcReader___Target_ReceiveQuestState_3887376304));
			((NetworkBehaviour)this).RegisterTargetRpc(8u, new ClientRpcDelegate(RpcReader___Target_SetQuestTracked_619441887));
			((NetworkBehaviour)this).RegisterServerRpc(9u, new ServerRpcDelegate(RpcReader___Server_SendQuestEntryState_375159588));
			((NetworkBehaviour)this).RegisterObserversRpc(10u, new ClientRpcDelegate(RpcReader___Observers_ReceiveQuestEntryState_311789429));
			((NetworkBehaviour)this).RegisterTargetRpc(11u, new ClientRpcDelegate(RpcReader___Target_ReceiveQuestEntryState_311789429));
			((NetworkBehaviour)this).RegisterObserversRpc(12u, new ClientRpcDelegate(RpcReader___Observers_CreateDeaddropCollectionQuest_3895153758));
			((NetworkBehaviour)this).RegisterTargetRpc(13u, new ClientRpcDelegate(RpcReader___Target_CreateDeaddropCollectionQuest_3895153758));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EQuests_002EQuestManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EQuests_002EQuestManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_CreateContract_Networked_2526053753(NetworkConnection conn, string title, string description, string guid, bool tracked, NetworkObject customer, ContractInfo contractData, GameDateTime expiry, GameDateTime acceptTime, NetworkObject dealerObj = null)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(title);
			((Writer)writer).WriteString(description);
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteBoolean(tracked);
			((Writer)writer).WriteNetworkObject(customer);
			GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerated((Writer)(object)writer, contractData);
			GeneratedWriters___Internal.Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, expiry);
			GeneratedWriters___Internal.Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, acceptTime);
			((Writer)writer).WriteNetworkObject(dealerObj);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___CreateContract_Networked_2526053753(NetworkConnection conn, string title, string description, string guid, bool tracked, NetworkObject customer, ContractInfo contractData, GameDateTime expiry, GameDateTime acceptTime, NetworkObject dealerObj = null)
	{
		if (!GUIDManager.IsGUIDAlreadyRegistered(new Guid(guid)))
		{
			DeliveryLocation deliveryLocation = GUIDManager.GetObject<DeliveryLocation>(new Guid(contractData.DeliveryLocationGUID));
			QuestEntryData questEntryData = new QuestEntryData(contractData.Products.GetCommaSeperatedString() + ", " + deliveryLocation.LocationName, EQuestState.Inactive);
			Dealer dealer = null;
			if ((Object)(object)dealerObj != (Object)null)
			{
				dealer = ((Component)dealerObj).GetComponent<Dealer>();
			}
			CreateContract_Local(title, description, new QuestEntryData[1] { questEntryData }, guid, tracked, ((Component)customer).GetComponent<Customer>(), contractData.Payment, contractData.Products, contractData.DeliveryLocationGUID, contractData.DeliveryWindow, contractData.Expires, expiry, contractData.PickupScheduleIndex, acceptTime, dealer);
		}
	}

	private void RpcReader___Observers_CreateContract_Networked_2526053753(PooledReader PooledReader0, Channel channel)
	{
		string title = ((Reader)PooledReader0).ReadString();
		string description = ((Reader)PooledReader0).ReadString();
		string guid = ((Reader)PooledReader0).ReadString();
		bool tracked = ((Reader)PooledReader0).ReadBoolean();
		NetworkObject customer = ((Reader)PooledReader0).ReadNetworkObject();
		ContractInfo contractData = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		GameDateTime expiry = GeneratedReaders___Internal.Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		GameDateTime acceptTime = GeneratedReaders___Internal.Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		NetworkObject dealerObj = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___CreateContract_Networked_2526053753(null, title, description, guid, tracked, customer, contractData, expiry, acceptTime, dealerObj);
		}
	}

	private void RpcWriter___Target_CreateContract_Networked_2526053753(NetworkConnection conn, string title, string description, string guid, bool tracked, NetworkObject customer, ContractInfo contractData, GameDateTime expiry, GameDateTime acceptTime, NetworkObject dealerObj = null)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(title);
			((Writer)writer).WriteString(description);
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteBoolean(tracked);
			((Writer)writer).WriteNetworkObject(customer);
			GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerated((Writer)(object)writer, contractData);
			GeneratedWriters___Internal.Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, expiry);
			GeneratedWriters___Internal.Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, acceptTime);
			((Writer)writer).WriteNetworkObject(dealerObj);
			((NetworkBehaviour)this).SendTargetRpc(1u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_CreateContract_Networked_2526053753(PooledReader PooledReader0, Channel channel)
	{
		string title = ((Reader)PooledReader0).ReadString();
		string description = ((Reader)PooledReader0).ReadString();
		string guid = ((Reader)PooledReader0).ReadString();
		bool tracked = ((Reader)PooledReader0).ReadBoolean();
		NetworkObject customer = ((Reader)PooledReader0).ReadNetworkObject();
		ContractInfo contractData = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		GameDateTime expiry = GeneratedReaders___Internal.Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		GameDateTime acceptTime = GeneratedReaders___Internal.Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		NetworkObject dealerObj = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___CreateContract_Networked_2526053753(((NetworkBehaviour)this).LocalConnection, title, description, guid, tracked, customer, contractData, expiry, acceptTime, dealerObj);
		}
	}

	private void RpcWriter___Server_SendQuestAction_2848227116(string guid, EQuestAction action)
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
			((Writer)writer).WriteString(guid);
			GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerated((Writer)(object)writer, action);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendQuestAction_2848227116(string guid, EQuestAction action)
	{
		ReceiveQuestAction(null, guid, action);
	}

	private void RpcReader___Server_SendQuestAction_2848227116(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string guid = ((Reader)PooledReader0).ReadString();
		EQuestAction action = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendQuestAction_2848227116(guid, action);
		}
	}

	private void RpcWriter___Observers_ReceiveQuestAction_920727549(NetworkConnection conn, string guid, EQuestAction action)
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
			((Writer)writer).WriteString(guid);
			GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerated((Writer)(object)writer, action);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveQuestAction_920727549(NetworkConnection conn, string guid, EQuestAction action)
	{
		Quest quest = GUIDManager.GetObject<Quest>(new Guid(guid));
		if ((Object)(object)quest == (Object)null)
		{
			Console.LogWarning("Failed to find quest with GUID: " + guid);
			return;
		}
		switch (action)
		{
		case EQuestAction.Begin:
			quest.Begin(network: false);
			break;
		case EQuestAction.Success:
			quest.Complete(network: false);
			break;
		case EQuestAction.Fail:
			quest.Fail(network: false);
			break;
		case EQuestAction.Expire:
			quest.Expire(network: false);
			break;
		case EQuestAction.Cancel:
			quest.Cancel(network: false);
			break;
		}
	}

	private void RpcReader___Observers_ReceiveQuestAction_920727549(PooledReader PooledReader0, Channel channel)
	{
		string guid = ((Reader)PooledReader0).ReadString();
		EQuestAction action = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceiveQuestAction_920727549(null, guid, action);
		}
	}

	private void RpcWriter___Target_ReceiveQuestAction_920727549(NetworkConnection conn, string guid, EQuestAction action)
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
			((Writer)writer).WriteString(guid);
			GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerated((Writer)(object)writer, action);
			((NetworkBehaviour)this).SendTargetRpc(4u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ReceiveQuestAction_920727549(PooledReader PooledReader0, Channel channel)
	{
		string guid = ((Reader)PooledReader0).ReadString();
		EQuestAction action = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveQuestAction_920727549(((NetworkBehaviour)this).LocalConnection, guid, action);
		}
	}

	private void RpcWriter___Server_SendQuestState_4117703421(string guid, EQuestState state)
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
			((Writer)writer).WriteString(guid);
			GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated((Writer)(object)writer, state);
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendQuestState_4117703421(string guid, EQuestState state)
	{
		ReceiveQuestState(null, guid, state);
	}

	private void RpcReader___Server_SendQuestState_4117703421(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string guid = ((Reader)PooledReader0).ReadString();
		EQuestState state = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendQuestState_4117703421(guid, state);
		}
	}

	private void RpcWriter___Observers_ReceiveQuestState_3887376304(NetworkConnection conn, string guid, EQuestState state)
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
			((Writer)writer).WriteString(guid);
			GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated((Writer)(object)writer, state);
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveQuestState_3887376304(NetworkConnection conn, string guid, EQuestState state)
	{
		Quest quest = GUIDManager.GetObject<Quest>(new Guid(guid));
		if ((Object)(object)quest == (Object)null)
		{
			Console.LogWarning("Failed to find quest with GUID: " + guid);
		}
		else
		{
			quest.SetQuestState(state, network: false);
		}
	}

	private void RpcReader___Observers_ReceiveQuestState_3887376304(PooledReader PooledReader0, Channel channel)
	{
		string guid = ((Reader)PooledReader0).ReadString();
		EQuestState state = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceiveQuestState_3887376304(null, guid, state);
		}
	}

	private void RpcWriter___Target_ReceiveQuestState_3887376304(NetworkConnection conn, string guid, EQuestState state)
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
			((Writer)writer).WriteString(guid);
			GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated((Writer)(object)writer, state);
			((NetworkBehaviour)this).SendTargetRpc(7u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ReceiveQuestState_3887376304(PooledReader PooledReader0, Channel channel)
	{
		string guid = ((Reader)PooledReader0).ReadString();
		EQuestState state = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveQuestState_3887376304(((NetworkBehaviour)this).LocalConnection, guid, state);
		}
	}

	private void RpcWriter___Target_SetQuestTracked_619441887(NetworkConnection conn, string guid, bool tracked)
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
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteBoolean(tracked);
			((NetworkBehaviour)this).SendTargetRpc(8u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___SetQuestTracked_619441887(NetworkConnection conn, string guid, bool tracked)
	{
		Quest quest = GUIDManager.GetObject<Quest>(new Guid(guid));
		if ((Object)(object)quest == (Object)null)
		{
			Console.LogWarning("Failed to find quest with GUID: " + guid);
		}
		else
		{
			quest.SetIsTracked(tracked);
		}
	}

	private void RpcReader___Target_SetQuestTracked_619441887(PooledReader PooledReader0, Channel channel)
	{
		string guid = ((Reader)PooledReader0).ReadString();
		bool tracked = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetQuestTracked_619441887(((NetworkBehaviour)this).LocalConnection, guid, tracked);
		}
	}

	private void RpcWriter___Server_SendQuestEntryState_375159588(string guid, int entryIndex, EQuestState state)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteInt32(entryIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated((Writer)(object)writer, state);
			((NetworkBehaviour)this).SendServerRpc(9u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendQuestEntryState_375159588(string guid, int entryIndex, EQuestState state)
	{
		ReceiveQuestEntryState(null, guid, entryIndex, state);
	}

	private void RpcReader___Server_SendQuestEntryState_375159588(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string guid = ((Reader)PooledReader0).ReadString();
		int entryIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		EQuestState state = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendQuestEntryState_375159588(guid, entryIndex, state);
		}
	}

	private void RpcWriter___Observers_ReceiveQuestEntryState_311789429(NetworkConnection conn, string guid, int entryIndex, EQuestState state)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteInt32(entryIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated((Writer)(object)writer, state);
			((NetworkBehaviour)this).SendObserversRpc(10u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ReceiveQuestEntryState_311789429(NetworkConnection conn, string guid, int entryIndex, EQuestState state)
	{
		Quest quest = GUIDManager.GetObject<Quest>(new Guid(guid));
		if ((Object)(object)quest == (Object)null)
		{
			Console.LogWarning("Failed to find quest with GUID: " + guid);
		}
		else
		{
			quest.SetQuestEntryState(entryIndex, state, network: false);
		}
	}

	private void RpcReader___Observers_ReceiveQuestEntryState_311789429(PooledReader PooledReader0, Channel channel)
	{
		string guid = ((Reader)PooledReader0).ReadString();
		int entryIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		EQuestState state = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ReceiveQuestEntryState_311789429(null, guid, entryIndex, state);
		}
	}

	private void RpcWriter___Target_ReceiveQuestEntryState_311789429(NetworkConnection conn, string guid, int entryIndex, EQuestState state)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(guid);
			((Writer)writer).WriteInt32(entryIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated((Writer)(object)writer, state);
			((NetworkBehaviour)this).SendTargetRpc(11u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ReceiveQuestEntryState_311789429(PooledReader PooledReader0, Channel channel)
	{
		string guid = ((Reader)PooledReader0).ReadString();
		int entryIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		EQuestState state = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ReceiveQuestEntryState_311789429(((NetworkBehaviour)this).LocalConnection, guid, entryIndex, state);
		}
	}

	private void RpcWriter___Observers_CreateDeaddropCollectionQuest_3895153758(NetworkConnection conn, string dropGUID, string guidString = "")
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
			((Writer)writer).WriteString(dropGUID);
			((Writer)writer).WriteString(guidString);
			((NetworkBehaviour)this).SendObserversRpc(12u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___CreateDeaddropCollectionQuest_3895153758(NetworkConnection conn, string dropGUID, string guidString = "")
	{
		CreateDeaddropCollectionQuest(dropGUID, guidString);
	}

	private void RpcReader___Observers_CreateDeaddropCollectionQuest_3895153758(PooledReader PooledReader0, Channel channel)
	{
		string dropGUID = ((Reader)PooledReader0).ReadString();
		string guidString = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___CreateDeaddropCollectionQuest_3895153758(null, dropGUID, guidString);
		}
	}

	private void RpcWriter___Target_CreateDeaddropCollectionQuest_3895153758(NetworkConnection conn, string dropGUID, string guidString = "")
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
			((Writer)writer).WriteString(dropGUID);
			((Writer)writer).WriteString(guidString);
			((NetworkBehaviour)this).SendTargetRpc(13u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_CreateDeaddropCollectionQuest_3895153758(PooledReader PooledReader0, Channel channel)
	{
		string dropGUID = ((Reader)PooledReader0).ReadString();
		string guidString = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___CreateDeaddropCollectionQuest_3895153758(((NetworkBehaviour)this).LocalConnection, dropGUID, guidString);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EQuests_002EQuestManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		InitializeSaveable();
		Quest[] componentsInChildren = ((Component)QuestContainer).GetComponentsInChildren<Quest>();
		foreach (Quest quest in componentsInChildren)
		{
			if (!DefaultQuests.Contains(quest))
			{
				Console.LogError("Quest " + quest.GetQuestTitle() + " is not in the default quests list!");
			}
		}
	}
}
