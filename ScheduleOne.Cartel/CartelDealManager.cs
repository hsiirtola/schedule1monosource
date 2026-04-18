using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Core;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using ScheduleOne.Quests;
using ScheduleOne.Storage;
using ScheduleOne.UI.Phone.Messages;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScheduleOne.Cartel;

public class CartelDealManager : NetworkBehaviour
{
	public const int DEAL_DUE_TIME_DAYS = 3;

	public const float PAYMENT_MULTIPLIER = 0.65f;

	public const int DEAL_COOLDOWN_HOURS = 24;

	[Header("References")]
	public NPC RequestingNPC;

	public Quest_DealForCartel DealQuest;

	public WorldStorageEntity DeliveryEntity;

	public Transform CashSpawnPoint;

	public Quest MethRequestPrereqQuest;

	public Supplier CokeRequestPrereqSupplier;

	[Header("Settings")]
	public CashPickup CashPrefab;

	public ProductDefinition[] RequestableWeed;

	public ProductDefinition MethDefinition;

	public ProductDefinition CocaineDefinition;

	public int ProductQuantityMin = 10;

	public int ProductQuantityMax = 60;

	private bool NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelDealManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ECartel_002ECartelDealManagerAssembly_002DCSharp_002Edll_Excuted;

	public CartelDealInfo ActiveDeal { get; private set; }

	public int HoursUntilNextDealRequest { get; private set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ECartel_002ECartelDealManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void Start()
	{
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepEnd = (Action)Delegate.Combine(instance.onSleepEnd, new Action(SleepEnd));
		TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
		instance2.onTimeSkip = (Action<int>)Delegate.Combine(instance2.onTimeSkip, new Action<int>(OnTimeSkip));
		TimeManager instance3 = NetworkSingleton<TimeManager>.Instance;
		instance3.onHourPass = (Action)Delegate.Combine(instance3.onHourPass, new Action(HourPass));
		Cartel instance4 = NetworkSingleton<Cartel>.Instance;
		instance4.OnStatusChange = (Action<ECartelStatus, ECartelStatus>)Delegate.Combine(instance4.OnStatusChange, new Action<ECartelStatus, ECartelStatus>(CartelStatusChange));
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost && ActiveDeal != null)
		{
			InitializeDealQuest(null, ActiveDeal);
		}
	}

	private void MinPass()
	{
		if (NetworkSingleton<Cartel>.Instance.Status == ECartelStatus.Truced && InstanceFinder.IsServer)
		{
			if (ActiveDeal == null && HoursUntilNextDealRequest <= 0)
			{
				StartDeal();
			}
			if (ActiveDeal != null)
			{
				CheckDealCompletion();
			}
		}
	}

	private void OnTimeSkip(int mins)
	{
		if (InstanceFinder.IsServer)
		{
			HoursUntilNextDealRequest = Mathf.Max(0, HoursUntilNextDealRequest - Mathf.CeilToInt((float)mins / 60f));
		}
	}

	private void HourPass()
	{
		if (NetworkSingleton<Cartel>.Instance.Status == ECartelStatus.Truced && InstanceFinder.IsServer && ActiveDeal == null && HoursUntilNextDealRequest > 0)
		{
			HoursUntilNextDealRequest--;
		}
	}

	public void SetHoursUntilDealRequest(int hours)
	{
		HoursUntilNextDealRequest = hours;
	}

	private void SleepEnd()
	{
		if (!InstanceFinder.IsServer || ActiveDeal == null)
		{
			return;
		}
		if (ActiveDeal.Status == CartelDealInfo.EStatus.Pending)
		{
			if (NetworkSingleton<TimeManager>.Instance.GetDateTime().GetMinSum() > ActiveDeal.DueTime.GetMinSum())
			{
				MarkDealOverdue();
			}
		}
		else if (NetworkSingleton<TimeManager>.Instance.GetDateTime().GetMinSum() > ActiveDeal.DueTime.GetMinSum() + 1440)
		{
			ExpireDeal();
		}
	}

	private void MarkDealOverdue()
	{
		if (ActiveDeal != null && ActiveDeal.Status != CartelDealInfo.EStatus.Overdue)
		{
			ActiveDeal.Status = CartelDealInfo.EStatus.Overdue;
			SendOverdueMessage();
		}
	}

	private void ExpireDeal()
	{
		DealQuest.Expire();
		SendExpiryMessage();
		ActiveDeal = null;
		NetworkSingleton<Cartel>.Instance.SetStatus(null, ECartelStatus.Hostile, resetStatusChangeTimer: true);
	}

	private void CheckDealCompletion()
	{
		if (!InstanceFinder.IsServer || (Object)(object)DeliveryEntity.CurrentPlayerAccessor != (Object)null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < DeliveryEntity.SlotCount; i++)
		{
			if (DeliveryEntity.ItemSlots[i].ItemInstance != null && ((BaseItemInstance)DeliveryEntity.ItemSlots[i].ItemInstance).ID == ActiveDeal.RequestedProductID)
			{
				ProductItemInstance productItemInstance = DeliveryEntity.ItemSlots[i].ItemInstance as ProductItemInstance;
				num += productItemInstance.Amount * ((BaseItemInstance)productItemInstance).Quantity;
			}
		}
		if (num >= ActiveDeal.RequestedProductQuantity)
		{
			CompleteDeal();
		}
	}

	private void CompleteDeal()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		DealQuest.NotifyDealCompleted();
		int num = ActiveDeal.RequestedProductQuantity;
		for (int i = 0; i < DeliveryEntity.SlotCount; i++)
		{
			if (num <= 0)
			{
				break;
			}
			if (DeliveryEntity.ItemSlots[i].ItemInstance != null && ((BaseItemInstance)DeliveryEntity.ItemSlots[i].ItemInstance).ID == ActiveDeal.RequestedProductID)
			{
				ProductItemInstance productItemInstance = DeliveryEntity.ItemSlots[i].ItemInstance as ProductItemInstance;
				while (((BaseItemInstance)productItemInstance).Quantity > 0 && num > 0)
				{
					num -= productItemInstance.Amount;
					((BaseItemInstance)productItemInstance).ChangeQuantity(-1);
				}
			}
		}
		float num2 = ActiveDeal.PaymentAmount;
		if (ActiveDeal.Status == CartelDealInfo.EStatus.Overdue)
		{
			num2 *= 0.5f;
		}
		DepositCash(num2);
		int minSum = NetworkSingleton<TimeManager>.Instance.GetDateTime().GetMinSum();
		int minSum2 = ActiveDeal.DueTime.GetMinSum();
		int num3 = Mathf.Max(0, minSum2 - minSum);
		ActiveDeal = null;
		HoursUntilNextDealRequest = 24 + Mathf.CeilToInt((float)num3 / 60f);
	}

	private void DepositCash(float amount)
	{
		if (InstanceFinder.IsServer)
		{
			((MonoBehaviour)this).StartCoroutine(Wait());
		}
		IEnumerator Wait()
		{
			while (amount > 0f)
			{
				yield return (object)new WaitForSeconds(0.5f);
				float num = Mathf.Min(amount, 1000f);
				amount -= num;
				GameObject val = Object.Instantiate<GameObject>(((Component)CashPrefab).gameObject, CashSpawnPoint.position, CashSpawnPoint.rotation);
				Vector3 forward = CashSpawnPoint.forward;
				forward += Random.insideUnitSphere * 0.1f;
				val.GetComponent<Rigidbody>().AddForce(forward * 6f, (ForceMode)2);
				val.GetComponent<Rigidbody>().AddTorque(Random.insideUnitSphere * 2f, (ForceMode)2);
				val.GetComponent<CashPickup>().Value = num;
				((NetworkBehaviour)this).Spawn(val.gameObject, (NetworkConnection)null, default(Scene));
			}
		}
	}

	[Button]
	private void StartDeal()
	{
		if (InstanceFinder.IsServer)
		{
			FullRank fullRank = new FullRank(ERank.Kingpin, 0);
			float num = Mathf.Clamp01(NetworkSingleton<LevelManager>.Instance.GetFullRank().ToFloat() / fullRank.ToFloat());
			List<ProductDefinition> list = new List<ProductDefinition>();
			if ((Object)(object)MethRequestPrereqQuest != (Object)null && MethRequestPrereqQuest.State == EQuestState.Completed)
			{
				list.Add(MethDefinition);
			}
			if ((Object)(object)CokeRequestPrereqSupplier != (Object)null && CokeRequestPrereqSupplier.RelationData.Unlocked)
			{
				list.Add(CocaineDefinition);
			}
			ProductDefinition productDefinition = null;
			productDefinition = ((!(num > Random.Range(0f, 1.2f)) || list.Count <= 0) ? RequestableWeed[Random.Range(0, RequestableWeed.Length)] : list[Random.Range(0, list.Count)]);
			int num2 = Mathf.RoundToInt(Mathf.Lerp((float)ProductQuantityMin, (float)ProductQuantityMax, num));
			if (!(productDefinition is WeedDefinition))
			{
				num2 = Mathf.CeilToInt((float)num2 * 0.7f);
			}
			num2 = Mathf.RoundToInt((float)num2 / 5f) * 5;
			GameDateTime dateTime = NetworkSingleton<TimeManager>.Instance.GetDateTime();
			dateTime.elapsedDays += 3;
			dateTime.time = 401;
			int num3 = Mathf.RoundToInt(productDefinition.MarketValue * (float)num2 * 0.65f);
			num3 = Mathf.RoundToInt((float)num3 / 10f) * 10;
			CartelDealInfo cartelDealInfo = new CartelDealInfo(((BaseItemDefinition)productDefinition).ID, num2, num3, dateTime, CartelDealInfo.EStatus.Pending);
			InitializeDealQuest(null, cartelDealInfo);
			SendRequestMessage(cartelDealInfo);
			ActiveDeal = cartelDealInfo;
		}
	}

	public void LoadDeal(CartelDealInfo dealInfo)
	{
		InitializeDealQuest(null, dealInfo);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void InitializeDealQuest(NetworkConnection conn, CartelDealInfo dealInfo)
	{
		if (conn == null)
		{
			RpcWriter___Observers_InitializeDealQuest_2137933519(conn, dealInfo);
			RpcLogic___InitializeDealQuest_2137933519(conn, dealInfo);
		}
		else
		{
			RpcWriter___Target_InitializeDealQuest_2137933519(conn, dealInfo);
		}
	}

	private void SendRequestMessage(CartelDealInfo dealInfo)
	{
		MessageChain messageChain = RequestingNPC.DialogueHandler.Database.GetChain(EDialogueModule.Generic, "cartel_deal_request").GetMessageChain();
		for (int i = 0; i < messageChain.Messages.Count; i++)
		{
			string text = messageChain.Messages[i];
			text = text.Replace("<PRODUCT>", dealInfo.RequestedProductQuantity + "x " + ((BaseItemDefinition)Registry.GetItem(dealInfo.RequestedProductID)).Name);
			text = text.Replace("<PAYMENT>", MoneyManager.FormatAmount(dealInfo.PaymentAmount));
			text = text.Replace("<DUE_DAYS>", 3.ToString());
			messageChain.Messages[i] = text;
		}
		RequestingNPC.MSGConversation.SendMessageChain(messageChain);
	}

	private void SendOverdueMessage()
	{
		MessageChain messageChain = RequestingNPC.DialogueHandler.Database.GetChain(EDialogueModule.Generic, "cartel_deal_overdue").GetMessageChain();
		RequestingNPC.MSGConversation.SendMessageChain(messageChain);
	}

	private void SendExpiryMessage()
	{
		MessageChain messageChain = RequestingNPC.DialogueHandler.Database.GetChain(EDialogueModule.Generic, "cartel_deal_expired").GetMessageChain();
		RequestingNPC.MSGConversation.SendMessageChain(messageChain);
	}

	public void Load(CartelData data)
	{
		if (data.ActiveCartelDeal != null && data.ActiveCartelDeal.IsValid())
		{
			LoadDeal(data.ActiveCartelDeal);
		}
		HoursUntilNextDealRequest = data.HoursUntilNextDealRequest;
	}

	private void CartelStatusChange(ECartelStatus oldStatus, ECartelStatus newStatus)
	{
		if (oldStatus == ECartelStatus.Truced && newStatus == ECartelStatus.Hostile && ActiveDeal != null)
		{
			DealQuest.NotifyTruceEnded();
			ActiveDeal = null;
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelDealManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelDealManagerAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_InitializeDealQuest_2137933519));
			((NetworkBehaviour)this).RegisterTargetRpc(1u, new ClientRpcDelegate(RpcReader___Target_InitializeDealQuest_2137933519));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ECartel_002ECartelDealManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ECartel_002ECartelDealManagerAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_InitializeDealQuest_2137933519(NetworkConnection conn, CartelDealInfo dealInfo)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ECartel_002ECartelDealInfoFishNet_002ESerializing_002EGenerated((Writer)(object)writer, dealInfo);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___InitializeDealQuest_2137933519(NetworkConnection conn, CartelDealInfo dealInfo)
	{
		if (dealInfo != null)
		{
			ActiveDeal = dealInfo;
			DealQuest.Initialize(dealInfo);
		}
	}

	private void RpcReader___Observers_InitializeDealQuest_2137933519(PooledReader PooledReader0, Channel channel)
	{
		CartelDealInfo dealInfo = GeneratedReaders___Internal.Read___ScheduleOne_002ECartel_002ECartelDealInfoFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___InitializeDealQuest_2137933519(null, dealInfo);
		}
	}

	private void RpcWriter___Target_InitializeDealQuest_2137933519(NetworkConnection conn, CartelDealInfo dealInfo)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ECartel_002ECartelDealInfoFishNet_002ESerializing_002EGenerated((Writer)(object)writer, dealInfo);
			((NetworkBehaviour)this).SendTargetRpc(1u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_InitializeDealQuest_2137933519(PooledReader PooledReader0, Channel channel)
	{
		CartelDealInfo dealInfo = GeneratedReaders___Internal.Read___ScheduleOne_002ECartel_002ECartelDealInfoFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___InitializeDealQuest_2137933519(((NetworkBehaviour)this).LocalConnection, dealInfo);
		}
	}

	private void Awake_UserLogic_ScheduleOne_002ECartel_002ECartelDealManager_Assembly_002DCSharp_002Edll()
	{
		HoursUntilNextDealRequest = 24;
	}
}
