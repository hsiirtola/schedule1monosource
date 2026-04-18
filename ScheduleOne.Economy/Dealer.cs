using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.AvatarFramework;
using ScheduleOne.Cartel;
using ScheduleOne.Core;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using ScheduleOne.Messaging;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.NPCs.Schedules;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Quests;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Economy;

public class Dealer : NPC, IItemSlotOwner
{
	private enum EAmountSortOrder
	{
		LowToHigh,
		HighToLow
	}

	public const int MAX_CUSTOMERS = 10;

	public const int DEAL_ARRIVAL_DELAY = 30;

	public const int MIN_TRAVEL_TIME = 15;

	public const int MAX_TRAVEL_TIME = 360;

	public const int OVERFLOW_SLOT_COUNT = 10;

	public const float CASH_REMINDER_THRESHOLD = 500f;

	public const float RELATIONSHIP_CHANGE_PER_DEAL = 0.05f;

	public static Color32 DealerLabelColor = new Color32((byte)120, (byte)200, byte.MaxValue, byte.MaxValue);

	public const int NegativeQualityTolerance = -2;

	public const int PositiveQualityTolerance = 5;

	public static Action<Dealer> onDealerRecruited;

	public static List<Dealer> AllPlayerDealers = new List<Dealer>();

	[CompilerGenerated]
	[SyncVar(OnChange = "UpdateCollectCashChoice")]
	public float _003CCash_003Ek__BackingField;

	public Action onContractAccepted;

	[Header("Dealer References")]
	public NPCEnterableBuilding Home;

	public NPCEvent_StayInBuilding HomeEvent;

	public DialogueController_Dealer DialogueController;

	[Header("Dialogue stuff")]
	public DialogueContainer RecruitDialogue;

	public DialogueContainer CollectCashDialogue;

	public DialogueContainer AssignCustomersDialogue;

	[Header("Dealer Settings")]
	public EDealerType DealerType;

	public string HomeName = "Home";

	public float SigningFee = 500f;

	public float Cut = 0.2f;

	[Header("Variables")]
	public string CompletedDealsVariable = string.Empty;

	[Header("UnityEvents")]
	public UnityEvent onRecommended = new UnityEvent();

	public UnityEvent onCompleteDeal = new UnityEvent();

	[Header("Seasonal Events")]
	public AvatarSettings ChristmasOutfit;

	private ItemSlot[] overflowSlots;

	private Contract currentContract;

	private DialogueController.DialogueChoice recruitChoice;

	private DialogueController.DialogueChoice collectCashChoice;

	private DialogueController.DialogueChoice assignCustomersChoice;

	private int itemCountOnTradeStart;

	private DealerAttendDealBehaviour _attendDealBehaviour;

	public SyncVar<float> syncVar____003CCash_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EEconomy_002EDealerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEconomy_002EDealerAssembly_002DCSharp_002Edll_Excuted;

	public bool IsRecruited { get; private set; }

	public List<ItemSlot> ItemSlots { get; set; } = new List<ItemSlot>();

	public NPCPoI PotentialDealerPoI { get; private set; }

	public NPCPoI DealerPoI { get; private set; }

	public float Cash
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CCash_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			this.sync___set_value__003CCash_003Ek__BackingField(value, true);
		}
	}

	public List<Customer> AssignedCustomers { get; private set; } = new List<Customer>();

	public List<Contract> ActiveContracts { get; private set; } = new List<Contract>();

	public bool HasBeenRecommended { get; private set; }

	public float SyncAccessor__003CCash_003Ek__BackingField
	{
		get
		{
			return Cash;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				Cash = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CCash_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EEconomy_002EDealer_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void OnValidate()
	{
		((NetworkBehaviour)this).OnValidate();
		HomeEvent.Building = Home;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		AllPlayerDealers.Remove(this);
	}

	protected override void Start()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		base.Start();
		if (DealerType == EDealerType.PlayerDealer)
		{
			SetUpDialogue();
			SetupPoI();
			NPCRelationData relationData = RelationData;
			relationData.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(OnDealerUnlocked));
		}
		Health.onDieOrKnockedOut.AddListener(new UnityAction(DealerUnconscious));
		if (Settings.ChristmasEventActive && (Object)(object)ChristmasOutfit != (Object)null)
		{
			Avatar.LoadAvatarSettings(ChristmasOutfit);
		}
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (connection.IsLocalClient)
		{
			return;
		}
		if (IsRecruited)
		{
			SetIsRecruited(connection);
		}
		foreach (Customer assignedCustomer in AssignedCustomers)
		{
			AddCustomer_Client(connection, assignedCustomer.NPC.ID);
		}
	}

	private void SetupPoI()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)DealerPoI == (Object)null)
		{
			DealerPoI = Object.Instantiate<NPCPoI>(NetworkSingleton<NPCManager>.Instance.NPCPoIPrefab, ((Component)this).transform);
			DealerPoI.SetMainText(base.fullName + "\n(Dealer)");
			DealerPoI.SetNPC(this);
			((Component)DealerPoI).transform.localPosition = Vector3.zero;
			((Behaviour)DealerPoI).enabled = IsRecruited;
		}
		if ((Object)(object)PotentialDealerPoI == (Object)null)
		{
			PotentialDealerPoI = Object.Instantiate<NPCPoI>(NetworkSingleton<NPCManager>.Instance.PotentialDealerPoIPrefab, ((Component)this).transform);
			PotentialDealerPoI.SetMainText("Potential Dealer\n" + base.fullName);
			PotentialDealerPoI.SetNPC(this);
			float num = (float)(FirstName[0] % 36) * 10f;
			float num2 = Mathf.Clamp((float)FirstName.Length * 1.5f, 1f, 10f);
			Vector3 forward = ((Component)this).transform.forward;
			forward = Quaternion.Euler(0f, num, 0f) * forward;
			((Component)PotentialDealerPoI).transform.localPosition = forward * num2;
		}
		UpdatePotentialDealerPoI();
	}

	private void SetUpDialogue()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		recruitChoice = new DialogueController.DialogueChoice();
		recruitChoice.ChoiceText = "Do you want to work for me as a distributor?";
		recruitChoice.Enabled = !IsRecruited;
		recruitChoice.Conversation = RecruitDialogue;
		recruitChoice.onChoosen.AddListener(new UnityAction(RecruitmentRequested));
		recruitChoice.isValidCheck = CanOfferRecruitment;
		DialogueController.AddDialogueChoice(recruitChoice);
		DialogueController.DialogueChoice dialogueChoice = new DialogueController.DialogueChoice();
		dialogueChoice.ChoiceText = "Nevermind";
		dialogueChoice.Enabled = true;
		DialogueController.AddDialogueChoice(dialogueChoice);
	}

	protected override void OnTick()
	{
		base.OnTick();
		UpdatePotentialDealerPoI();
		if (InstanceFinder.IsServer && !Singleton<LoadManager>.Instance.IsLoading)
		{
			if ((Object)(object)currentContract != (Object)null)
			{
				CheckCurrentDealValidity();
			}
			else
			{
				CheckAttendStart();
			}
			((Component)HomeEvent).gameObject.SetActive(true);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void MarkAsRecommended()
	{
		RpcWriter___Server_MarkAsRecommended_2166136261();
		RpcLogic___MarkAsRecommended_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void SetRecommended()
	{
		RpcWriter___Observers_SetRecommended_2166136261();
		RpcLogic___SetRecommended_2166136261();
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void InitialRecruitment()
	{
		RpcWriter___Server_InitialRecruitment_2166136261();
		RpcLogic___InitialRecruitment_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public virtual void SetIsRecruited(NetworkConnection conn)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetIsRecruited_328543758(conn);
			RpcLogic___SetIsRecruited_328543758(conn);
		}
		else
		{
			RpcWriter___Target_SetIsRecruited_328543758(conn);
		}
	}

	protected virtual void OnDealerUnlocked(NPCRelationData.EUnlockType unlockType, bool b)
	{
		UpdatePotentialDealerPoI();
		if (!Singleton<LoadManager>.Instance.IsLoading)
		{
			NetworkSingleton<MoneyManager>.Instance.PlayCashSound();
		}
	}

	protected virtual void UpdatePotentialDealerPoI()
	{
		if ((Object)(object)PotentialDealerPoI != (Object)null)
		{
			((Behaviour)PotentialDealerPoI).enabled = RelationData.IsMutuallyKnown() && !RelationData.Unlocked;
		}
	}

	private void DealerUnconscious()
	{
		List<Contract> list = new List<Contract>(ActiveContracts);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Fail();
		}
	}

	private void TradeItems()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		DialogueHandler.SkipNextDialogueBehaviourEnd();
		itemCountOnTradeStart = ((IItemSlotOwner)Inventory).GetQuantitySum();
		Singleton<StorageMenu>.Instance.Open(Inventory, base.fullName + "'s Inventory", "Place <color=#4CB0FF>packaged product</color> here and the dealer will sell it to assigned customers");
		Singleton<StorageMenu>.Instance.onClosed.AddListener(new UnityAction(TradeItemsDone));
	}

	private void TradeItemsDone()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Singleton<StorageMenu>.Instance.onClosed.RemoveListener(new UnityAction(TradeItemsDone));
		Behaviour.GenericDialogueBehaviour.Disable_Server();
		if (((IItemSlotOwner)Inventory).GetQuantitySum() > itemCountOnTradeStart)
		{
			DialogueHandler.WorldspaceRend.ShowText("Thanks boss", 2.5f);
			PlayVO(EVOLineType.Thanks);
		}
		TryMoveOverflowItems();
	}

	private bool CanCollectCash(out string reason)
	{
		reason = string.Empty;
		if (SyncAccessor__003CCash_003Ek__BackingField <= 0f)
		{
			return false;
		}
		return true;
	}

	private void UpdateCollectCashChoice(float oldCash, float newCash, bool asServer)
	{
		if (collectCashChoice != null)
		{
			collectCashChoice.ChoiceText = "I need to collect the earnings <color=#54E717>(" + MoneyManager.FormatAmount(SyncAccessor__003CCash_003Ek__BackingField) + ")</color>";
		}
	}

	private void CollectCash()
	{
		NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(SyncAccessor__003CCash_003Ek__BackingField, visualizeChange: true, playCashSound: true);
		SetCash(0f);
	}

	private void CheckCurrentDealValidity()
	{
		if (currentContract.State != EQuestState.Active)
		{
			_attendDealBehaviour.Disable_Server();
			currentContract.SetDealer(null);
			currentContract = null;
		}
	}

	private bool CanOfferRecruitment(out string reason)
	{
		reason = string.Empty;
		if (IsRecruited)
		{
			return false;
		}
		if (!RelationData.IsMutuallyKnown())
		{
			reason = "Unlock one of " + FirstName + "'s connections";
			return false;
		}
		if (!HasBeenRecommended)
		{
			reason = "Must be recommended by one of " + FirstName + "'s connections";
			return false;
		}
		return true;
	}

	private void CheckAttendStart()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Contract contract = ActiveContracts.FirstOrDefault();
		if ((Object)(object)contract == (Object)null)
		{
			return;
		}
		int time = TimeManager.AddMinutesTo24HourTime(contract.DeliveryWindow.WindowStartTime, 30);
		int num = Mathf.CeilToInt(Vector3.Distance(Avatar.CenterPoint, contract.DeliveryLocation.CustomerStandPoint.position) / Movement.WalkSpeed * 1.5f);
		num = Mathf.Clamp(num, 15, 360);
		int min = TimeManager.AddMinutesTo24HourTime(time, -num);
		int minsUntilExpiry = contract.GetMinsUntilExpiry();
		if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(min, contract.DeliveryWindow.WindowEndTime) || minsUntilExpiry <= 240)
		{
			Debug.Log((object)(base.fullName + " start attend deal: " + contract.Title));
			currentContract = contract;
			_attendDealBehaviour.Enable_Server();
			_attendDealBehaviour.AssignContract(currentContract);
			if (this is CartelDealer cartelDealer)
			{
				CheckNotifyPlayerOfDeal(cartelDealer, contract);
			}
		}
	}

	public virtual bool ShouldAcceptContract(ContractInfo contractInfo, Customer customer)
	{
		foreach (ProductList.Entry entry in contractInfo.Products.entries)
		{
			string productID = entry.ProductID;
			EQuality minQuality = ItemQuality.ShiftQuality(customer.CustomerData.Standards.GetCorrespondingQuality(), -2);
			EQuality maxQuality = ItemQuality.ShiftQuality(customer.CustomerData.Standards.GetCorrespondingQuality(), 5);
			if (GetOrderableProductQuantity(productID, minQuality, maxQuality) < entry.Quantity)
			{
				Console.LogWarning("Dealer " + base.fullName + " does not have enough " + productID + " for " + customer.NPC.fullName);
				return false;
			}
		}
		return true;
	}

	public virtual void ContractedOffered(ContractInfo contractInfo, Customer customer)
	{
		if (ShouldAcceptContract(contractInfo, customer))
		{
			EDealWindow dealWindow = GetDealWindow();
			Console.Log("Contract accepted by dealer " + base.fullName + " in window " + dealWindow);
			AddContract(customer.ContractAccepted(dealWindow, trackContract: false, this));
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void AddCustomer_Server(string npcID)
	{
		RpcWriter___Server_AddCustomer_Server_3615296227(npcID);
		RpcLogic___AddCustomer_Server_3615296227(npcID);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void AddCustomer_Client(NetworkConnection conn, string npcID)
	{
		if (conn == null)
		{
			RpcWriter___Observers_AddCustomer_Client_2971853958(conn, npcID);
			RpcLogic___AddCustomer_Client_2971853958(conn, npcID);
		}
		else
		{
			RpcWriter___Target_AddCustomer_Client_2971853958(conn, npcID);
		}
	}

	protected virtual void AddCustomer(Customer customer)
	{
		if (!AssignedCustomers.Contains(customer))
		{
			AssignedCustomers.Add(customer);
			customer.AssignDealer(this);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendRemoveCustomer(string npcID)
	{
		RpcWriter___Server_SendRemoveCustomer_3615296227(npcID);
		RpcLogic___SendRemoveCustomer_3615296227(npcID);
	}

	[ObserversRpc(RunLocally = true)]
	private void RemoveCustomer(string npcID)
	{
		RpcWriter___Observers_RemoveCustomer_3615296227(npcID);
		RpcLogic___RemoveCustomer_3615296227(npcID);
	}

	public virtual void RemoveCustomer(Customer customer)
	{
		if (AssignedCustomers.Contains(customer))
		{
			AssignedCustomers.Remove(customer);
			customer.AssignDealer(null);
		}
	}

	public void ChangeCash(float change)
	{
		SetCash(SyncAccessor__003CCash_003Ek__BackingField + change);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetCash(float cash)
	{
		RpcWriter___Server_SetCash_431000436(cash);
	}

	[ServerRpc(RequireOwnership = false)]
	public virtual void CompletedDeal()
	{
		RpcWriter___Server_CompletedDeal_2166136261();
	}

	[ServerRpc(RequireOwnership = false)]
	public void SubmitPayment(float payment)
	{
		RpcWriter___Server_SubmitPayment_431000436(payment);
	}

	public void TryRobDealer()
	{
		float num = 0f;
		foreach (ItemSlot itemSlot2 in Inventory.ItemSlots)
		{
			if (itemSlot2.ItemInstance != null)
			{
				num = Mathf.Max(num, (itemSlot2.ItemInstance.Definition as StorableItemDefinition).CombatUtility);
			}
		}
		Console.Log("Dealer " + base.fullName + " has highest combat utility weapon: " + num);
		float num2 = Random.Range(0f, 1f);
		num2 = Mathf.Lerp(num2, 1f, num * 0.5f);
		Console.Log("Dealer " + base.fullName + " defense delta: " + num2);
		if (num2 > 0.67f)
		{
			base.MSGConversation.SendMessage(new Message(DialogueHandler.Database.GetLine(EDialogueModule.Dealer, "dealer_rob_defended"), Message.ESenderType.Other));
			return;
		}
		if (num2 > 0.25f)
		{
			base.MSGConversation.SendMessage(new Message(DialogueHandler.Database.GetLine(EDialogueModule.Dealer, "dealer_rob_partially_defended"), Message.ESenderType.Other));
			List<ItemInstance> list = new List<ItemInstance>();
			float num3 = 1f - Mathf.InverseLerp(0.25f, 0.67f, num2);
			for (int i = 0; i < Inventory.ItemSlots.Count; i++)
			{
				if (Inventory.ItemSlots[i].ItemInstance != null)
				{
					float num4 = num3 * 0.8f;
					if (Random.Range(0f, 1f) < num4)
					{
						int num5 = Mathf.RoundToInt((float)((BaseItemInstance)Inventory.ItemSlots[i].ItemInstance).Quantity * num3);
						list.Add(Inventory.ItemSlots[i].ItemInstance.GetCopy(num5));
						Inventory.ItemSlots[i].ChangeQuantity(-num5);
					}
				}
			}
			TryMoveOverflowItems();
			float num6 = SyncAccessor__003CCash_003Ek__BackingField * num3;
			ChangeCash(0f - num6);
			SummariseLosses(list, num6);
			return;
		}
		base.MSGConversation.SendMessage(new Message(DialogueHandler.Database.GetLine(EDialogueModule.Dealer, "dealer_rob_loss"), Message.ESenderType.Other));
		List<ItemInstance> list2 = new List<ItemInstance>();
		foreach (ItemSlot itemSlot3 in Inventory.ItemSlots)
		{
			if (itemSlot3.ItemInstance != null)
			{
				list2.Add(itemSlot3.ItemInstance.GetCopy(((BaseItemInstance)itemSlot3.ItemInstance).Quantity));
			}
		}
		Inventory.Clear();
		ItemSlot[] array = overflowSlots;
		foreach (ItemSlot itemSlot in array)
		{
			if (itemSlot.ItemInstance != null)
			{
				list2.Add(itemSlot.ItemInstance.GetCopy(((BaseItemInstance)itemSlot.ItemInstance).Quantity));
				itemSlot.ClearStoredInstance();
			}
		}
		float num7 = SyncAccessor__003CCash_003Ek__BackingField;
		ChangeCash(0f - num7);
		SummariseLosses(list2, num7);
		void SummariseLosses(List<ItemInstance> items, float cash)
		{
			if (items.Count != 0 || !(cash <= 0f))
			{
				List<string> list3 = new List<string>();
				for (int k = 0; k < items.Count; k++)
				{
					string text = ((BaseItemInstance)items[k]).Quantity + "x ";
					if (items[k] is ProductItemInstance && (Object)(object)(items[k] as ProductItemInstance).AppliedPackaging != (Object)null)
					{
						text = text + ((BaseItemDefinition)(items[k] as ProductItemInstance).AppliedPackaging).Name + " of ";
					}
					text += ((BaseItemDefinition)items[k].Definition).Name;
					if (items[k] is QualityItemInstance)
					{
						text = text + " (" + (items[k] as QualityItemInstance).Quality.ToString() + " quality)";
					}
					list3.Add(text);
				}
				if (cash > 0f)
				{
					list3.Add(MoneyManager.FormatAmount(cash) + " cash");
				}
				string text2 = "This is what they got:\n" + string.Join("\n", list3);
				base.MSGConversation.SendMessage(new Message(text2, Message.ESenderType.Other, _endOfGroup: true), notify: false);
			}
		}
	}

	public List<Tuple<ProductDefinition, EQuality, int>> GetOrderableProducts(EQuality minQuality)
	{
		return (from x in GetAvailableProducts()
			where x.Item2 >= minQuality
			select x).ToList();
	}

	public int GetOrderableProductQuantity(string productID, EQuality minQuality, EQuality maxQuality)
	{
		List<Tuple<ProductDefinition, EQuality, int>> availableProducts = GetAvailableProducts();
		int num = 0;
		foreach (Tuple<ProductDefinition, EQuality, int> item in availableProducts)
		{
			if (((BaseItemDefinition)item.Item1).ID == productID && item.Item2 >= minQuality && item.Item2 <= maxQuality)
			{
				num += item.Item3;
			}
		}
		return num;
	}

	[Button]
	private List<Tuple<ProductDefinition, EQuality, int>> GetAvailableProducts()
	{
		List<Tuple<ProductDefinition, EQuality, int>> list = new List<Tuple<ProductDefinition, EQuality, int>>();
		foreach (ItemSlot allSlot in GetAllSlots())
		{
			if (allSlot.ItemInstance != null && allSlot.ItemInstance is ProductItemInstance)
			{
				ProductItemInstance product = allSlot.ItemInstance as ProductItemInstance;
				Tuple<ProductDefinition, EQuality, int> tuple = list.FirstOrDefault((Tuple<ProductDefinition, EQuality, int> x) => ((BaseItemDefinition)x.Item1).ID == ((BaseItemInstance)product).ID && x.Item2 == product.Quality);
				if (tuple != null)
				{
					list.Remove(tuple);
					list.Add(new Tuple<ProductDefinition, EQuality, int>(tuple.Item1, tuple.Item2, tuple.Item3 + allSlot.Quantity * product.Amount));
				}
				else
				{
					list.Add(new Tuple<ProductDefinition, EQuality, int>(product.Definition as ProductDefinition, product.Quality, allSlot.Quantity * product.Amount));
				}
			}
		}
		list = list.OrderBy((Tuple<ProductDefinition, EQuality, int> x) => x.Item2).ToList();
		foreach (Contract activeContract in ActiveContracts)
		{
			foreach (ProductList.Entry entry in activeContract.ProductList.entries)
			{
				EQuality requiredQuality = ItemQuality.ShiftQuality(entry.Quality, -2);
				Tuple<ProductDefinition, EQuality, int> tuple2 = list.FirstOrDefault((Tuple<ProductDefinition, EQuality, int> x) => ((BaseItemDefinition)x.Item1).ID == entry.ProductID && x.Item2 >= requiredQuality);
				if (tuple2 != null)
				{
					list.Remove(tuple2);
					int num = tuple2.Item3 - entry.Quantity;
					if (num > 0)
					{
						list.Add(new Tuple<ProductDefinition, EQuality, int>(tuple2.Item1, tuple2.Item2, num));
					}
				}
			}
		}
		return list;
	}

	private EDealWindow GetDealWindow()
	{
		EDealWindow window = DealWindowInfo.GetWindow(NetworkSingleton<TimeManager>.Instance.CurrentTime);
		int num = (int)window;
		int num2 = TimeManager.GetMinSumFrom24HourTime(DealWindowInfo.GetWindowInfo(window).EndTime) - TimeManager.GetMinSumFrom24HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime);
		List<EDealWindow> list = new List<EDealWindow>();
		if (num2 > 120)
		{
			list.Add(window);
		}
		for (int i = 1; i < 4; i++)
		{
			int item = (num + i) % 4;
			list.Add((EDealWindow)item);
		}
		int num3 = 2;
		while (true)
		{
			foreach (EDealWindow item2 in list)
			{
				if (GetContractCountInWindow(item2) <= num3)
				{
					return item2;
				}
			}
			num3++;
		}
	}

	private int GetContractCountInWindow(EDealWindow window)
	{
		int num = 0;
		foreach (Contract activeContract in ActiveContracts)
		{
			if (DealWindowInfo.GetWindow(TimeManager.AddMinutesTo24HourTime(activeContract.DeliveryWindow.WindowStartTime, 1)) == window)
			{
				num++;
			}
		}
		return num;
	}

	private void AddContract(Contract contract)
	{
		ActiveContracts.Add(contract);
		contract.SetDealer(this);
		contract.onQuestEnd.AddListener((UnityAction<EQuestState>)delegate
		{
			CustomerContractEnded(contract);
		});
		if (onContractAccepted != null)
		{
			onContractAccepted();
		}
		((MonoBehaviour)this).Invoke("SortContracts", 0.05f);
	}

	private void CustomerContractEnded(Contract contract)
	{
		if (ActiveContracts.Contains(contract))
		{
			ActiveContracts.Remove(contract);
			contract.SetDealer(null);
			((MonoBehaviour)this).Invoke("SortContracts", 0.05f);
		}
	}

	private void SortContracts()
	{
		ActiveContracts = ActiveContracts.OrderBy((Contract x) => x.GetMinsUntilExpiry()).ToList();
	}

	protected virtual void RecruitmentRequested()
	{
	}

	public void RemoveContractItems(Contract contract, EQuality targetQuality, out List<ItemInstance> items)
	{
		int packagedProductAmount = GetPackagedProductAmount();
		items = new List<ItemInstance>();
		foreach (ProductList.Entry entry in contract.ProductList.entries)
		{
			List<ProductItemInstance> list = RemoveAndReturnProductFromInventory(entry.ProductID, entry.Quantity, targetQuality);
			if (list.Sum((ProductItemInstance x) => ((BaseItemInstance)x).GetTotalAmount()) < entry.Quantity)
			{
				Console.LogWarning("Could not find enough items for contract entry: " + entry.ProductID);
			}
			items.AddRange(list);
		}
		TryMoveOverflowItems();
		if (DealerType == EDealerType.PlayerDealer && GetPackagedProductAmount() == 0 && packagedProductAmount > 0)
		{
			base.MSGConversation.SendMessageChain(DialogueHandler.Database.GetChain(EDialogueModule.Dealer, "inventory_depleted").GetMessageChain());
		}
	}

	private List<ProductItemInstance> RemoveAndReturnProductFromInventory(string productID, int requiredQuantity, EQuality targetQuality)
	{
		List<ProductItemInstance> products = new List<ProductItemInstance>();
		int num = 0;
		int remainingRequiredQuantity = requiredQuantity;
		while (remainingRequiredQuantity > 0)
		{
			if (num >= 50)
			{
				Console.LogError("Looping too many times trying to remove products from dealer inventory: " + productID);
				break;
			}
			List<ItemSlot> list = FilterAndSortSlots(GetAllSlots(), productID, targetQuality, EAmountSortOrder.HighToLow);
			if (list.Count == 0)
			{
				break;
			}
			RemoveProduct(list, split: false, onlyRemoveIdealQuality: true);
			List<ItemSlot> orderedSlots = FilterAndSortSlots(GetAllSlots(), productID, targetQuality, EAmountSortOrder.LowToHigh);
			RemoveProduct(orderedSlots, split: true, onlyRemoveIdealQuality: false);
			num++;
		}
		if (remainingRequiredQuantity > 0)
		{
			Console.LogWarning("Could not fully satisfy required quantity for product " + productID + ". Remaining quantity: " + remainingRequiredQuantity);
		}
		int num2 = products.Sum((ProductItemInstance x) => ((BaseItemInstance)x).GetTotalAmount());
		if (num2 > requiredQuantity)
		{
			Console.LogWarning($"Removed more product ({num2}) than required ({requiredQuantity}) for product " + productID);
		}
		return products;
		void RemoveProduct(List<ItemSlot> list2, bool split, bool onlyRemoveIdealQuality)
		{
			if (list2.Count == 0 || remainingRequiredQuantity <= 0)
			{
				return;
			}
			EQuality quality = (list2[0].ItemInstance as ProductItemInstance).Quality;
			foreach (ItemSlot item in list2)
			{
				if (remainingRequiredQuantity <= 0 || (onlyRemoveIdealQuality && (item.ItemInstance as ProductItemInstance).Quality != quality))
				{
					break;
				}
				int amount = (item.ItemInstance as ProductItemInstance).Amount;
				if (split && amount > remainingRequiredQuantity)
				{
					SplitItemSlot(item);
					break;
				}
				for (; remainingRequiredQuantity >= amount; remainingRequiredQuantity -= amount)
				{
					if (item.Quantity <= 0)
					{
						break;
					}
					products.Add(item.ItemInstance.GetCopy(1) as ProductItemInstance);
					item.ChangeQuantity(-1);
				}
			}
		}
	}

	private void SplitItemSlot(ItemSlot slot)
	{
		if (slot.ItemInstance == null)
		{
			Console.LogWarning("Cannot split empty item slot");
			return;
		}
		if (!(slot.ItemInstance is ProductItemInstance))
		{
			Console.LogWarning("Cannot split non-product item slot: " + ((BaseItemInstance)slot.ItemInstance).Name);
			return;
		}
		PackagingDefinition appliedPackaging = (slot.ItemInstance as ProductItemInstance).AppliedPackaging;
		ProductDefinition productDefinition = (slot.ItemInstance as ProductItemInstance).Definition as ProductDefinition;
		PackagingDefinition packagingDefinition = null;
		for (int i = 0; i < productDefinition.ValidPackaging.Length; i++)
		{
			if (((BaseItemDefinition)productDefinition.ValidPackaging[i]).ID == ((BaseItemDefinition)appliedPackaging).ID && i > 0)
			{
				packagingDefinition = productDefinition.ValidPackaging[i - 1];
			}
		}
		if ((Object)(object)packagingDefinition == (Object)null)
		{
			Console.LogWarning("Failed to find next packaging smaller than " + ((BaseItemDefinition)appliedPackaging).ID);
			return;
		}
		int quantity = packagingDefinition.Quantity;
		int overrideQuantity = appliedPackaging.Quantity / quantity;
		Console.Log("Splitting 1x " + ((BaseItemInstance)slot.ItemInstance).Name + " (" + ((BaseItemDefinition)appliedPackaging).Name + ") into " + overrideQuantity + "x " + ((BaseItemDefinition)packagingDefinition).Name);
		ProductItemInstance productItemInstance = slot.ItemInstance.GetCopy(overrideQuantity) as ProductItemInstance;
		productItemInstance.SetPackaging(packagingDefinition);
		slot.ChangeQuantity(-1);
		AddItemToInventory(productItemInstance);
	}

	private List<ItemSlot> FilterAndSortSlots(List<ItemSlot> slots, string productID, EQuality productQuality, EAmountSortOrder amountSortOrder)
	{
		List<ItemSlot> list = new List<ItemSlot>();
		for (int i = 0; i < slots.Count; i++)
		{
			if (slots[i].ItemInstance != null && slots[i].ItemInstance is ProductItemInstance productItemInstance && !(((BaseItemInstance)productItemInstance).ID != productID) && !((Object)(object)productItemInstance.AppliedPackaging == (Object)null))
			{
				list.Add(slots[i]);
			}
		}
		list.Sort(delegate(ItemSlot x, ItemSlot y)
		{
			if (x.ItemInstance == null)
			{
				return 1;
			}
			if (y.ItemInstance == null)
			{
				return -1;
			}
			int num = (x.ItemInstance as QualityItemInstance).Quality - productQuality;
			int num2 = (y.ItemInstance as QualityItemInstance).Quality - productQuality;
			if (num != num2)
			{
				if (num >= 0 && num2 >= 0)
				{
					return num.CompareTo(num2);
				}
				if (num <= 0)
				{
					_ = 0;
					return num2.CompareTo(num);
				}
				return num2.CompareTo(num);
			}
			return (amountSortOrder == EAmountSortOrder.HighToLow) ? (y.ItemInstance as ProductItemInstance).Amount.CompareTo((x.ItemInstance as ProductItemInstance).Amount) : (x.ItemInstance as ProductItemInstance).Amount.CompareTo((y.ItemInstance as ProductItemInstance).Amount);
		});
		return list;
	}

	public List<ItemSlot> GetAllSlots()
	{
		List<ItemSlot> list = new List<ItemSlot>(Inventory.ItemSlots);
		list.AddRange(overflowSlots);
		return list;
	}

	public void AddItemToInventory(ItemInstance item)
	{
		while (Inventory.CanItemFit(item) && ((BaseItemInstance)item).Quantity > 0)
		{
			Inventory.InsertItem(item.GetCopy(1));
			((BaseItemInstance)item).ChangeQuantity(-1);
		}
		if (((BaseItemInstance)item).Quantity > 0 && !ItemSlot.TryInsertItemIntoSet(overflowSlots.ToList(), item))
		{
			Console.LogWarning("Dealer " + base.fullName + " has doesn't have enough space for item " + ((BaseItemInstance)item).ID);
		}
	}

	public void TryMoveOverflowItems()
	{
		ItemSlot[] array = overflowSlots;
		foreach (ItemSlot itemSlot in array)
		{
			if (itemSlot.ItemInstance != null)
			{
				while (Inventory.CanItemFit(itemSlot.ItemInstance) && ((BaseItemInstance)itemSlot.ItemInstance).Quantity > 0)
				{
					Inventory.InsertItem(itemSlot.ItemInstance.GetCopy(1));
					((BaseItemInstance)itemSlot.ItemInstance).ChangeQuantity(-1);
				}
			}
		}
	}

	public int GetTotalInventoryItemCount()
	{
		List<ItemSlot> allSlots = GetAllSlots();
		int num = 0;
		foreach (ItemSlot item in allSlots)
		{
			if (item.ItemInstance != null)
			{
				num += ((BaseItemInstance)item.ItemInstance).Quantity;
			}
		}
		return num;
	}

	public int GetPackagedProductAmount()
	{
		List<ItemSlot> allSlots = GetAllSlots();
		int num = 0;
		foreach (ItemSlot item in allSlots)
		{
			if (item.ItemInstance != null && item.ItemInstance is ProductItemInstance)
			{
				ProductItemInstance productItemInstance = item.ItemInstance as ProductItemInstance;
				if ((Object)(object)productItemInstance.AppliedPackaging != (Object)null)
				{
					num += ((BaseItemInstance)productItemInstance).Quantity * productItemInstance.Amount;
				}
			}
		}
		return num;
	}

	public virtual void CheckNotifyPlayerOfDeal(Dealer cartelDealer, Contract contract)
	{
		if (Random.value > 0.3f)
		{
			return;
		}
		MapRegionData regionData = Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(cartelDealer.Region);
		if (!regionData.IsUnlocked)
		{
			List<EMapRegion> list = Enum.GetValues(typeof(EMapRegion)).Cast<EMapRegion>().ToList();
			list.Shuffle();
			foreach (EMapRegion item in list)
			{
				regionData = Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(item);
				if (regionData.IsUnlocked)
				{
					break;
				}
			}
		}
		if (!regionData.IsUnlocked)
		{
			Debug.Log((object)"No unlocked region found, not notifying the player");
			return;
		}
		NPC nPC = (from x in NPCManager.GetNPCsInRegion(regionData.Region)
			where x is Dealer { DealerType: EDealerType.PlayerDealer } dealer && dealer.IsRecruited
			select x).FirstOrDefault();
		if ((Object)(object)nPC != (Object)null)
		{
			nPC.SendTextMessage($"Hey boss, I've heard there's a Benzies deal happening in {regionData.Region}, {contract.DeliveryLocation.LocationDescription}. Might be worth checking out.");
		}
		else
		{
			Debug.Log((object)"No dealer found to notify the player");
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetStoredInstance(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		RpcWriter___Server_SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
		RpcLogic___SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetStoredInstance_Internal(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
			RpcLogic___SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
		}
		else
		{
			RpcWriter___Target_SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetItemSlotQuantity(int itemSlotIndex, int quantity)
	{
		RpcWriter___Server_SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
		RpcLogic___SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetItemSlotQuantity_Internal(int itemSlotIndex, int quantity)
	{
		RpcWriter___Observers_SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
		RpcLogic___SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetSlotLocked(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		RpcWriter___Server_SetSlotLocked_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		RpcLogic___SetSlotLocked_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	private void SetSlotLocked_Internal(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
			RpcLogic___SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}
		else
		{
			RpcWriter___Target_SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	[ServerRpc(RunLocally = true, RequireOwnership = false)]
	public void SetSlotFilter(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		RpcWriter___Server_SetSlotFilter_527532783(conn, itemSlotIndex, filter);
		RpcLogic___SetSlotFilter_527532783(conn, itemSlotIndex, filter);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetSlotFilter_Internal(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetSlotFilter_Internal_527532783(conn, itemSlotIndex, filter);
			RpcLogic___SetSlotFilter_Internal_527532783(conn, itemSlotIndex, filter);
		}
		else
		{
			RpcWriter___Target_SetSlotFilter_Internal_527532783(conn, itemSlotIndex, filter);
		}
	}

	public override NPCData GetNPCData()
	{
		string[] array = new string[AssignedCustomers.Count];
		for (int i = 0; i < AssignedCustomers.Count; i++)
		{
			array[i] = AssignedCustomers[i].NPC.ID;
		}
		string[] array2 = new string[ActiveContracts.Count];
		for (int j = 0; j < ActiveContracts.Count; j++)
		{
			array2[j] = ActiveContracts[j].GUID.ToString();
		}
		return new DealerData(ID, IsRecruited, array, array2, SyncAccessor__003CCash_003Ek__BackingField, new ItemSet(overflowSlots), HasBeenRecommended);
	}

	public override void Load(DynamicSaveData dynamicData, NPCData npcData)
	{
		base.Load(dynamicData, npcData);
		if (!dynamicData.TryExtractBaseData<DealerData>(out var data))
		{
			return;
		}
		if (data.Recruited)
		{
			SetIsRecruited(null);
		}
		SetCash(data.Cash);
		for (int i = 0; i < data.AssignedCustomerIDs.Length; i++)
		{
			NPC nPC = NPCManager.GetNPC(data.AssignedCustomerIDs[i]);
			if ((Object)(object)nPC == (Object)null)
			{
				Console.LogWarning("Failed to find customer NPC with ID " + data.AssignedCustomerIDs[i]);
				continue;
			}
			Customer component = ((Component)nPC).GetComponent<Customer>();
			if ((Object)(object)component == (Object)null)
			{
				Console.LogWarning("NPC is not a customer: " + nPC.fullName);
			}
			else
			{
				AddCustomer_Server(component.NPC.ID);
			}
		}
		if (data.ActiveContractGUIDs != null)
		{
			for (int j = 0; j < data.ActiveContractGUIDs.Length; j++)
			{
				if (!GUIDManager.IsGUIDValid(data.ActiveContractGUIDs[j]))
				{
					Console.LogWarning("Invalid contract GUID: " + data.ActiveContractGUIDs[j]);
					continue;
				}
				Contract contract = GUIDManager.GetObject<Contract>(new Guid(data.ActiveContractGUIDs[j]));
				if ((Object)(object)contract != (Object)null)
				{
					AddContract(contract);
				}
			}
		}
		if (data.HasBeenRecommended)
		{
			MarkAsRecommended();
		}
		data.OverflowItems.LoadTo(overflowSlots);
	}

	public override void Load(NPCData data, string containerPath)
	{
		base.Load(data, containerPath);
		if (!((ISaveable)this).TryLoadFile(containerPath, "NPC", out string contents))
		{
			return;
		}
		DealerData dealerData = null;
		try
		{
			dealerData = JsonUtility.FromJson<DealerData>(contents);
		}
		catch (Exception ex)
		{
			Console.LogWarning("Failed to deserialize character data: " + ex.Message);
			return;
		}
		if (dealerData == null)
		{
			return;
		}
		if (dealerData.Recruited)
		{
			SetIsRecruited(null);
		}
		SetCash(dealerData.Cash);
		for (int i = 0; i < dealerData.AssignedCustomerIDs.Length; i++)
		{
			NPC nPC = NPCManager.GetNPC(dealerData.AssignedCustomerIDs[i]);
			if ((Object)(object)nPC == (Object)null)
			{
				Console.LogWarning("Failed to find customer NPC with ID " + dealerData.AssignedCustomerIDs[i]);
				continue;
			}
			Customer component = ((Component)nPC).GetComponent<Customer>();
			if ((Object)(object)component == (Object)null)
			{
				Console.LogWarning("NPC is not a customer: " + nPC.fullName);
			}
			else
			{
				AddCustomer_Server(component.NPC.ID);
			}
		}
		if (dealerData.ActiveContractGUIDs != null)
		{
			for (int j = 0; j < dealerData.ActiveContractGUIDs.Length; j++)
			{
				if (!GUIDManager.IsGUIDValid(dealerData.ActiveContractGUIDs[j]))
				{
					Console.LogWarning("Invalid contract GUID: " + dealerData.ActiveContractGUIDs[j]);
					continue;
				}
				Contract contract = GUIDManager.GetObject<Contract>(new Guid(dealerData.ActiveContractGUIDs[j]));
				if ((Object)(object)contract != (Object)null)
				{
					AddContract(contract);
				}
			}
		}
		if (dealerData.HasBeenRecommended)
		{
			MarkAsRecommended();
		}
		dealerData.OverflowItems.LoadTo(overflowSlots);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Expected O, but got Unknown
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Expected O, but got Unknown
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Expected O, but got Unknown
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Expected O, but got Unknown
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected O, but got Unknown
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Expected O, but got Unknown
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Expected O, but got Unknown
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Expected O, but got Unknown
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Expected O, but got Unknown
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Expected O, but got Unknown
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Expected O, but got Unknown
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Expected O, but got Unknown
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Expected O, but got Unknown
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EEconomy_002EDealerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEconomy_002EDealerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CCash_003Ek__BackingField = new SyncVar<float>((NetworkBehaviour)(object)this, 1u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, Cash);
			syncVar____003CCash_003Ek__BackingField.OnChange += UpdateCollectCashChoice;
			((NetworkBehaviour)this).RegisterServerRpc(39u, new ServerRpcDelegate(RpcReader___Server_MarkAsRecommended_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(40u, new ClientRpcDelegate(RpcReader___Observers_SetRecommended_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(41u, new ServerRpcDelegate(RpcReader___Server_InitialRecruitment_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(42u, new ClientRpcDelegate(RpcReader___Observers_SetIsRecruited_328543758));
			((NetworkBehaviour)this).RegisterTargetRpc(43u, new ClientRpcDelegate(RpcReader___Target_SetIsRecruited_328543758));
			((NetworkBehaviour)this).RegisterServerRpc(44u, new ServerRpcDelegate(RpcReader___Server_AddCustomer_Server_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(45u, new ClientRpcDelegate(RpcReader___Observers_AddCustomer_Client_2971853958));
			((NetworkBehaviour)this).RegisterTargetRpc(46u, new ClientRpcDelegate(RpcReader___Target_AddCustomer_Client_2971853958));
			((NetworkBehaviour)this).RegisterServerRpc(47u, new ServerRpcDelegate(RpcReader___Server_SendRemoveCustomer_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(48u, new ClientRpcDelegate(RpcReader___Observers_RemoveCustomer_3615296227));
			((NetworkBehaviour)this).RegisterServerRpc(49u, new ServerRpcDelegate(RpcReader___Server_SetCash_431000436));
			((NetworkBehaviour)this).RegisterServerRpc(50u, new ServerRpcDelegate(RpcReader___Server_CompletedDeal_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(51u, new ServerRpcDelegate(RpcReader___Server_SubmitPayment_431000436));
			((NetworkBehaviour)this).RegisterServerRpc(52u, new ServerRpcDelegate(RpcReader___Server_SetStoredInstance_2652194801));
			((NetworkBehaviour)this).RegisterObserversRpc(53u, new ClientRpcDelegate(RpcReader___Observers_SetStoredInstance_Internal_2652194801));
			((NetworkBehaviour)this).RegisterTargetRpc(54u, new ClientRpcDelegate(RpcReader___Target_SetStoredInstance_Internal_2652194801));
			((NetworkBehaviour)this).RegisterServerRpc(55u, new ServerRpcDelegate(RpcReader___Server_SetItemSlotQuantity_1692629761));
			((NetworkBehaviour)this).RegisterObserversRpc(56u, new ClientRpcDelegate(RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761));
			((NetworkBehaviour)this).RegisterServerRpc(57u, new ServerRpcDelegate(RpcReader___Server_SetSlotLocked_3170825843));
			((NetworkBehaviour)this).RegisterTargetRpc(58u, new ClientRpcDelegate(RpcReader___Target_SetSlotLocked_Internal_3170825843));
			((NetworkBehaviour)this).RegisterObserversRpc(59u, new ClientRpcDelegate(RpcReader___Observers_SetSlotLocked_Internal_3170825843));
			((NetworkBehaviour)this).RegisterServerRpc(60u, new ServerRpcDelegate(RpcReader___Server_SetSlotFilter_527532783));
			((NetworkBehaviour)this).RegisterObserversRpc(61u, new ClientRpcDelegate(RpcReader___Observers_SetSlotFilter_Internal_527532783));
			((NetworkBehaviour)this).RegisterTargetRpc(62u, new ClientRpcDelegate(RpcReader___Target_SetSlotFilter_Internal_527532783));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EEconomy_002EDealer));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEconomy_002EDealerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEconomy_002EDealerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)syncVar____003CCash_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_MarkAsRecommended_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(39u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___MarkAsRecommended_2166136261()
	{
		SetRecommended();
	}

	private void RpcReader___Server_MarkAsRecommended_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___MarkAsRecommended_2166136261();
		}
	}

	private void RpcWriter___Observers_SetRecommended_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(40u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetRecommended_2166136261()
	{
		if (!HasBeenRecommended)
		{
			HasBeenRecommended = true;
			base.HasChanged = true;
			if (onRecommended != null)
			{
				onRecommended.Invoke();
			}
		}
	}

	private void RpcReader___Observers_SetRecommended_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetRecommended_2166136261();
		}
	}

	private void RpcWriter___Server_InitialRecruitment_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(41u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___InitialRecruitment_2166136261()
	{
		SetIsRecruited(null);
	}

	private void RpcReader___Server_InitialRecruitment_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___InitialRecruitment_2166136261();
		}
	}

	private void RpcWriter___Observers_SetIsRecruited_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendObserversRpc(42u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public virtual void RpcLogic___SetIsRecruited_328543758(NetworkConnection conn)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		if (!IsRecruited)
		{
			Console.Log("Dealer " + base.fullName + " set as recruited");
			IsRecruited = true;
			DialogueController.GreetingOverride greetingOverride = new DialogueController.GreetingOverride();
			greetingOverride.Greeting = "Hi boss, what do you need?";
			greetingOverride.PlayVO = true;
			greetingOverride.VOType = EVOLineType.Greeting;
			greetingOverride.ShouldShow = true;
			DialogueController.AddGreetingOverride(greetingOverride);
			DialogueController.DialogueChoice dialogueChoice = new DialogueController.DialogueChoice();
			dialogueChoice.ChoiceText = "I need to trade some items";
			dialogueChoice.Enabled = true;
			dialogueChoice.onChoosen.AddListener(new UnityAction(TradeItems));
			DialogueController.AddDialogueChoice(dialogueChoice, 5);
			collectCashChoice = new DialogueController.DialogueChoice();
			UpdateCollectCashChoice(0f, 0f, asServer: false);
			collectCashChoice.Enabled = true;
			collectCashChoice.isValidCheck = CanCollectCash;
			collectCashChoice.onChoosen.AddListener(new UnityAction(CollectCash));
			collectCashChoice.Conversation = CollectCashDialogue;
			DialogueController.AddDialogueChoice(collectCashChoice, 4);
			assignCustomersChoice = new DialogueController.DialogueChoice();
			assignCustomersChoice.ChoiceText = "How do I assign customers to you?";
			assignCustomersChoice.Enabled = true;
			assignCustomersChoice.Conversation = AssignCustomersDialogue;
			DialogueController.AddDialogueChoice(assignCustomersChoice, 3);
			if ((Object)(object)DealerPoI != (Object)null)
			{
				((Behaviour)DealerPoI).enabled = true;
			}
			if (!RelationData.Unlocked)
			{
				RelationData.Unlock(NPCRelationData.EUnlockType.DirectApproach, notify: false);
			}
			if (recruitChoice != null)
			{
				recruitChoice.Enabled = false;
			}
			if (onDealerRecruited != null)
			{
				onDealerRecruited(this);
			}
			base.HasChanged = true;
		}
	}

	private void RpcReader___Observers_SetIsRecruited_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetIsRecruited_328543758(null);
		}
	}

	private void RpcWriter___Target_SetIsRecruited_328543758(NetworkConnection conn)
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
			((NetworkBehaviour)this).SendTargetRpc(43u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetIsRecruited_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetIsRecruited_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	private void RpcWriter___Server_AddCustomer_Server_3615296227(string npcID)
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
			((Writer)writer).WriteString(npcID);
			((NetworkBehaviour)this).SendServerRpc(44u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___AddCustomer_Server_3615296227(string npcID)
	{
		AddCustomer_Client(null, npcID);
	}

	private void RpcReader___Server_AddCustomer_Server_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string npcID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___AddCustomer_Server_3615296227(npcID);
		}
	}

	private void RpcWriter___Observers_AddCustomer_Client_2971853958(NetworkConnection conn, string npcID)
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
			((Writer)writer).WriteString(npcID);
			((NetworkBehaviour)this).SendObserversRpc(45u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___AddCustomer_Client_2971853958(NetworkConnection conn, string npcID)
	{
		NPC nPC = NPCManager.GetNPC(npcID);
		if ((Object)(object)nPC == (Object)null)
		{
			Console.LogWarning("Failed to find NPC with ID: " + npcID);
			return;
		}
		Customer component = ((Component)nPC).GetComponent<Customer>();
		if ((Object)(object)component == (Object)null)
		{
			Console.LogWarning("NPC " + npcID + " is not a customer");
		}
		else
		{
			AddCustomer(component);
		}
	}

	private void RpcReader___Observers_AddCustomer_Client_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string npcID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___AddCustomer_Client_2971853958(null, npcID);
		}
	}

	private void RpcWriter___Target_AddCustomer_Client_2971853958(NetworkConnection conn, string npcID)
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
			((Writer)writer).WriteString(npcID);
			((NetworkBehaviour)this).SendTargetRpc(46u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_AddCustomer_Client_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string npcID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___AddCustomer_Client_2971853958(((NetworkBehaviour)this).LocalConnection, npcID);
		}
	}

	private void RpcWriter___Server_SendRemoveCustomer_3615296227(string npcID)
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
			((Writer)writer).WriteString(npcID);
			((NetworkBehaviour)this).SendServerRpc(47u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendRemoveCustomer_3615296227(string npcID)
	{
		RemoveCustomer(npcID);
	}

	private void RpcReader___Server_SendRemoveCustomer_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string npcID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendRemoveCustomer_3615296227(npcID);
		}
	}

	private void RpcWriter___Observers_RemoveCustomer_3615296227(string npcID)
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
			((Writer)writer).WriteString(npcID);
			((NetworkBehaviour)this).SendObserversRpc(48u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___RemoveCustomer_3615296227(string npcID)
	{
		NPC nPC = NPCManager.GetNPC(npcID);
		if ((Object)(object)nPC == (Object)null)
		{
			Console.LogWarning("Failed to find NPC with ID: " + npcID);
			return;
		}
		Customer component = ((Component)nPC).GetComponent<Customer>();
		if ((Object)(object)component == (Object)null)
		{
			Console.LogWarning("NPC " + npcID + " is not a customer");
		}
		else
		{
			RemoveCustomer(component);
		}
	}

	private void RpcReader___Observers_RemoveCustomer_3615296227(PooledReader PooledReader0, Channel channel)
	{
		string npcID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___RemoveCustomer_3615296227(npcID);
		}
	}

	private void RpcWriter___Server_SetCash_431000436(float cash)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteSingle(cash, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(49u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetCash_431000436(float cash)
	{
		Cash = Mathf.Clamp(cash, 0f, float.MaxValue);
		base.HasChanged = true;
		UpdateCollectCashChoice(0f, 0f, asServer: false);
	}

	private void RpcReader___Server_SetCash_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		float cash = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SetCash_431000436(cash);
		}
	}

	private void RpcWriter___Server_CompletedDeal_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(50u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public virtual void RpcLogic___CompletedDeal_2166136261()
	{
		if (DealerType == EDealerType.PlayerDealer)
		{
			RelationData.ChangeRelationship(0.05f);
			if (CompletedDealsVariable != string.Empty)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(CompletedDealsVariable, (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>(CompletedDealsVariable) + 1f).ToString());
			}
		}
		UnityEvent obj = onCompleteDeal;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	private void RpcReader___Server_CompletedDeal_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___CompletedDeal_2166136261();
		}
	}

	private void RpcWriter___Server_SubmitPayment_431000436(float payment)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteSingle(payment, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(51u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SubmitPayment_431000436(float payment)
	{
		if (!(payment <= 0f))
		{
			Console.Log("Dealer " + base.fullName + " received payment: " + payment);
			float num = SyncAccessor__003CCash_003Ek__BackingField;
			ChangeCash(payment * (1f - Cut));
			if (InstanceFinder.IsServer && DealerType == EDealerType.PlayerDealer && SyncAccessor__003CCash_003Ek__BackingField >= 500f && num < 500f)
			{
				base.MSGConversation.SendMessage(new Message("Hey boss, just letting you know I've got " + MoneyManager.FormatAmount(SyncAccessor__003CCash_003Ek__BackingField) + " ready for you to collect.", Message.ESenderType.Other, _endOfGroup: true));
			}
		}
	}

	private void RpcReader___Server_SubmitPayment_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		float payment = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SubmitPayment_431000436(payment);
		}
	}

	private void RpcWriter___Server_SetStoredInstance_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)(object)writer).WriteItemInstance(instance);
			((NetworkBehaviour)this).SendServerRpc(52u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetStoredInstance_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		if (conn == (NetworkConnection)null || conn.ClientId == -1)
		{
			SetStoredInstance_Internal(null, itemSlotIndex, instance);
		}
		else
		{
			SetStoredInstance_Internal(conn, itemSlotIndex, instance);
		}
	}

	private void RpcReader___Server_SetStoredInstance_2652194801(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = ((Reader)PooledReader0).ReadNetworkConnection();
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetStoredInstance_2652194801(conn2, itemSlotIndex, instance);
		}
	}

	private void RpcWriter___Observers_SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)(object)writer).WriteItemInstance(instance);
			((NetworkBehaviour)this).SendObserversRpc(53u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		if (instance != null)
		{
			ItemSlots[itemSlotIndex].SetStoredItem(instance, _internal: true);
		}
		else
		{
			ItemSlots[itemSlotIndex].ClearStoredInstance(_internal: true);
		}
	}

	private void RpcReader___Observers_SetStoredInstance_Internal_2652194801(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetStoredInstance_Internal_2652194801(null, itemSlotIndex, instance);
		}
	}

	private void RpcWriter___Target_SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)(object)writer).WriteItemInstance(instance);
			((NetworkBehaviour)this).SendTargetRpc(54u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetStoredInstance_Internal_2652194801(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		ItemInstance instance = ((Reader)(object)PooledReader0).ReadItemInstance();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetStoredInstance_Internal_2652194801(((NetworkBehaviour)this).LocalConnection, itemSlotIndex, instance);
		}
	}

	private void RpcWriter___Server_SetItemSlotQuantity_1692629761(int itemSlotIndex, int quantity)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteInt32(quantity, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(55u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetItemSlotQuantity_1692629761(int itemSlotIndex, int quantity)
	{
		SetItemSlotQuantity_Internal(itemSlotIndex, quantity);
	}

	private void RpcReader___Server_SetItemSlotQuantity_1692629761(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int quantity = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
		}
	}

	private void RpcWriter___Observers_SetItemSlotQuantity_Internal_1692629761(int itemSlotIndex, int quantity)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteInt32(quantity, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(56u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetItemSlotQuantity_Internal_1692629761(int itemSlotIndex, int quantity)
	{
		ItemSlots[itemSlotIndex].SetQuantity(quantity, _internal: true);
	}

	private void RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int quantity = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
		}
	}

	private void RpcWriter___Server_SetSlotLocked_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteBoolean(locked);
			((Writer)writer).WriteNetworkObject(lockOwner);
			((Writer)writer).WriteString(lockReason);
			((NetworkBehaviour)this).SendServerRpc(57u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetSlotLocked_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		if (conn == (NetworkConnection)null || conn.ClientId == -1)
		{
			SetSlotLocked_Internal(null, itemSlotIndex, locked, lockOwner, lockReason);
		}
		else
		{
			SetSlotLocked_Internal(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcReader___Server_SetSlotLocked_3170825843(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = ((Reader)PooledReader0).ReadNetworkConnection();
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool locked = ((Reader)PooledReader0).ReadBoolean();
		NetworkObject lockOwner = ((Reader)PooledReader0).ReadNetworkObject();
		string lockReason = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetSlotLocked_3170825843(conn2, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcWriter___Target_SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteBoolean(locked);
			((Writer)writer).WriteNetworkObject(lockOwner);
			((Writer)writer).WriteString(lockReason);
			((NetworkBehaviour)this).SendTargetRpc(58u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		if (locked)
		{
			ItemSlots[itemSlotIndex].ApplyLock(lockOwner, lockReason, _internal: true);
		}
		else
		{
			ItemSlots[itemSlotIndex].RemoveLock(_internal: true);
		}
	}

	private void RpcReader___Target_SetSlotLocked_Internal_3170825843(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool locked = ((Reader)PooledReader0).ReadBoolean();
		NetworkObject lockOwner = ((Reader)PooledReader0).ReadNetworkObject();
		string lockReason = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetSlotLocked_Internal_3170825843(((NetworkBehaviour)this).LocalConnection, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcWriter___Observers_SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			((Writer)writer).WriteBoolean(locked);
			((Writer)writer).WriteNetworkObject(lockOwner);
			((Writer)writer).WriteString(lockReason);
			((NetworkBehaviour)this).SendObserversRpc(59u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_SetSlotLocked_Internal_3170825843(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		bool locked = ((Reader)PooledReader0).ReadBoolean();
		NetworkObject lockOwner = ((Reader)PooledReader0).ReadNetworkObject();
		string lockReason = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetSlotLocked_Internal_3170825843(null, itemSlotIndex, locked, lockOwner, lockReason);
		}
	}

	private void RpcWriter___Server_SetSlotFilter_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerated((Writer)(object)writer, filter);
			((NetworkBehaviour)this).SendServerRpc(60u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetSlotFilter_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		if (conn == (NetworkConnection)null || conn.ClientId == -1)
		{
			SetSlotFilter_Internal(null, itemSlotIndex, filter);
		}
		else
		{
			SetSlotFilter_Internal(conn, itemSlotIndex, filter);
		}
	}

	private void RpcReader___Server_SetSlotFilter_527532783(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection conn2 = ((Reader)PooledReader0).ReadNetworkConnection();
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		SlotFilter filter = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetSlotFilter_527532783(conn2, itemSlotIndex, filter);
		}
	}

	private void RpcWriter___Observers_SetSlotFilter_Internal_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerated((Writer)(object)writer, filter);
			((NetworkBehaviour)this).SendObserversRpc(61u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetSlotFilter_Internal_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		ItemSlots[itemSlotIndex].SetPlayerFilter(filter, _internal: true);
	}

	private void RpcReader___Observers_SetSlotFilter_Internal_527532783(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		SlotFilter filter = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetSlotFilter_Internal_527532783(null, itemSlotIndex, filter);
		}
	}

	private void RpcWriter___Target_SetSlotFilter_Internal_527532783(NetworkConnection conn, int itemSlotIndex, SlotFilter filter)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(itemSlotIndex, (AutoPackType)1);
			GeneratedWriters___Internal.Write___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerated((Writer)(object)writer, filter);
			((NetworkBehaviour)this).SendTargetRpc(62u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetSlotFilter_Internal_527532783(PooledReader PooledReader0, Channel channel)
	{
		int itemSlotIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		SlotFilter filter = GeneratedReaders___Internal.Read___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetSlotFilter_Internal_527532783(((NetworkBehaviour)this).LocalConnection, itemSlotIndex, filter);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EEconomy_002EDealer(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		if (UInt321 == 1)
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CCash_003Ek__BackingField(syncVar____003CCash_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			float value = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
			this.sync___set_value__003CCash_003Ek__BackingField(value, Boolean2);
			return true;
		}
		return false;
	}

	protected override void Awake_UserLogic_ScheduleOne_002EEconomy_002EDealer_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		_attendDealBehaviour = Behaviour.GetBehaviour<DealerAttendDealBehaviour>();
		HomeEvent.Building = Home;
		overflowSlots = new ItemSlot[10];
		for (int i = 0; i < 10; i++)
		{
			overflowSlots[i] = new ItemSlot();
			overflowSlots[i].SetSlotOwner(this);
		}
		if (RelationData.Unlocked)
		{
			SetIsRecruited(null);
		}
		else
		{
			NPCRelationData relationData = RelationData;
			relationData.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData.onUnlocked, (Action<NPCRelationData.EUnlockType, bool>)delegate
			{
				SetIsRecruited(null);
			});
		}
		if (DealerType == EDealerType.PlayerDealer && !AllPlayerDealers.Contains(this))
		{
			AllPlayerDealers.Add(this);
		}
	}
}
