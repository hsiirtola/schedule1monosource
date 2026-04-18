using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.Messaging;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Quests;
using ScheduleOne.Storage;
using ScheduleOne.UI.Phone;
using ScheduleOne.UI.Phone.Delivery;
using ScheduleOne.UI.Phone.Messages;
using ScheduleOne.UI.Shop;
using ScheduleOne.Variables;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Economy;

public class Supplier : NPC
{
	public enum ESupplierStatus
	{
		Idle,
		PreppingDeadDrop,
		Meeting
	}

	public const float MEETUP_RELATIONSHIP_REQUIREMENT = 4f;

	public const int MEETUP_DURATION_MINS = 360;

	public const int MEETING_COOLDOWN_MINS = 720;

	public const int DEADDROP_WAIT_PER_ITEM = 30;

	public const int DEADDROP_MAX_WAIT = 360;

	public const int DEADDROP_ITEM_LIMIT = 10;

	public const float MeetingEndDistance = 20f;

	public const float DELIVERY_RELATIONSHIP_REQUIREMENT = 5f;

	public static Color32 SupplierLabelColor = new Color32(byte.MaxValue, (byte)150, (byte)145, byte.MaxValue);

	[Header("Supplier Settings")]
	public float MinOrderLimit = 100f;

	public float MaxOrderLimit = 500f;

	public PhoneShopInterface.Listing[] OnlineShopItems;

	[TextArea(3, 10)]
	public string SupplierRecommendMessage = "My friend <NAME> can hook you up with <PRODUCT>. I've passed your number on to them.";

	[TextArea(3, 10)]
	public string SupplierUnlockHint = "You can now order <PRODUCT> from <NAME>. <PRODUCT> can be used to <PURPOSE>.";

	[Header("References")]
	public ShopInterface Shop;

	public SupplierStash Stash;

	public UnityEvent onDeaddropReady;

	private int minsSinceMeetingStart = -1;

	private int minsSinceLastMeetingEnd = 720;

	private float playerSpendSinceMeetingStart;

	private SupplierLocation currentLocation;

	private DialogueController dialogueController;

	private DialogueController.GreetingOverride meetingGreeting;

	private DialogueController.DialogueChoice meetingChoice;

	[SyncVar]
	public float debt;

	[SyncVar]
	public bool deadDropPreparing;

	private StringIntPair[] deaddropItems;

	private int minsSinceDeaddropOrder;

	private bool repaymentReminderSent;

	public SyncVar<float> syncVar___debt;

	public SyncVar<bool> syncVar___deadDropPreparing;

	private bool NetworkInitialize___EarlyScheduleOne_002EEconomy_002ESupplierAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEconomy_002ESupplierAssembly_002DCSharp_002Edll_Excuted;

	public ESupplierStatus Status { get; private set; }

	public bool DeliveriesEnabled { get; private set; }

	public float Debt => SyncAccessor_debt;

	public int minsUntilDeaddropReady { get; private set; } = -1;

	public float SyncAccessor_debt
	{
		get
		{
			return debt;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				debt = value;
			}
			if (Application.isPlaying)
			{
				syncVar___debt.SetValue(value, value);
			}
		}
	}

	public bool SyncAccessor_deadDropPreparing
	{
		get
		{
			return deadDropPreparing;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				deadDropPreparing = value;
			}
			if (Application.isPlaying)
			{
				syncVar___deadDropPreparing.SetValue(value, value);
			}
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EEconomy_002ESupplier_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		base.Start();
		NPCRelationData relationData = RelationData;
		relationData.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(SupplierUnlocked));
		NPCRelationData relationData2 = RelationData;
		relationData2.onRelationshipChange = (Action<float>)Delegate.Combine(relationData2.onRelationshipChange, new Action<float>(RelationshipChange));
		ShopInterface shop = Shop;
		shop.onOrderCompletedWithSpend = (Action<float>)Delegate.Combine(shop.onOrderCompletedWithSpend, new Action<float>(MeetupOrderCompleted));
		dialogueController = ((Component)DialogueHandler).GetComponent<DialogueController>();
		meetingGreeting = new DialogueController.GreetingOverride();
		meetingGreeting.Greeting = DialogueHandler.Database.GetLine(EDialogueModule.Generic, "supplier_meeting_greeting");
		meetingGreeting.PlayVO = true;
		meetingGreeting.VOType = EVOLineType.Question;
		dialogueController.AddGreetingOverride(meetingGreeting);
		meetingChoice = new DialogueController.DialogueChoice();
		meetingChoice.ChoiceText = "Yes";
		meetingChoice.onChoosen.AddListener((UnityAction)delegate
		{
			Shop.SetIsOpen(isOpen: true);
		});
		meetingChoice.Enabled = false;
		dialogueController.AddDialogueChoice(meetingChoice);
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onTimeSkip = (Action<int>)Delegate.Combine(instance.onTimeSkip, new Action<int>(OnTimeSkip));
		PhoneShopInterface.Listing[] onlineShopItems = OnlineShopItems;
		foreach (PhoneShopInterface.Listing listing in onlineShopItems)
		{
			if (listing.Item.RequiresLevelToPurchase)
			{
				NetworkSingleton<LevelManager>.Instance.AddUnlockable(new Unlockable(listing.Item.RequiredRank, ((BaseItemDefinition)listing.Item).Name, ((BaseItemDefinition)listing.Item).Icon));
			}
		}
		TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
		instance2.onHourPass = (Action)Delegate.Remove(instance2.onHourPass, new Action(HourPass));
		TimeManager instance3 = NetworkSingleton<TimeManager>.Instance;
		instance3.onHourPass = (Action)Delegate.Combine(instance3.onHourPass, new Action(HourPass));
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (!connection.IsLocalClient)
		{
			if (Status == ESupplierStatus.Meeting)
			{
				MeetAtLocation(connection, SupplierLocation.AllLocations.IndexOf(currentLocation), 360);
			}
			if (DeliveriesEnabled)
			{
				EnableDeliveries(connection);
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendUnlocked()
	{
		RpcWriter___Server_SendUnlocked_2166136261();
	}

	[ObserversRpc]
	private void SetUnlocked()
	{
		RpcWriter___Observers_SetUnlocked_2166136261();
	}

	protected override void MinPass()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		base.MinPass();
		minsSinceDeaddropOrder++;
		if (Status == ESupplierStatus.Meeting)
		{
			minsSinceMeetingStart++;
			minsSinceLastMeetingEnd = 0;
			Player.GetClosestPlayer(((Component)this).transform.position, out var distance);
			if (minsSinceMeetingStart > 360 && distance > 20f)
			{
				EndMeeting();
			}
		}
		else
		{
			minsSinceLastMeetingEnd++;
		}
		if (InstanceFinder.IsServer && SyncAccessor_deadDropPreparing)
		{
			minsUntilDeaddropReady--;
			if (minsUntilDeaddropReady <= 0)
			{
				CompleteDeaddrop();
			}
		}
	}

	protected override void OnTick()
	{
		base.OnTick();
		if (InstanceFinder.IsServer && SyncAccessor_debt > 0f && !Stash.Storage.IsOpened && Stash.CashAmount > 1f && minsSinceDeaddropOrder > 3)
		{
			TryRecoverDebt();
		}
	}

	protected void HourPass()
	{
		if (InstanceFinder.IsServer && !repaymentReminderSent && SyncAccessor_debt > GetDeadDropLimit() * 0.5f && !SyncAccessor_deadDropPreparing)
		{
			float num = 1f / 48f;
			if (Random.Range(0f, 1f) < num)
			{
				SendDebtReminder();
			}
		}
	}

	private void OnTimeSkip(int minsSlept)
	{
		if (InstanceFinder.IsServer)
		{
			if (Status == ESupplierStatus.Meeting)
			{
				minsSinceMeetingStart += minsSlept;
			}
			if (SyncAccessor_deadDropPreparing)
			{
				minsUntilDeaddropReady -= minsSlept;
			}
		}
	}

	[ObserversRpc(RunLocally = true)]
	public void MeetAtLocation(NetworkConnection conn, int locationIndex, int expireIn)
	{
		RpcWriter___Observers_MeetAtLocation_3470796954(conn, locationIndex, expireIn);
		RpcLogic___MeetAtLocation_3470796954(conn, locationIndex, expireIn);
	}

	public void EndMeeting()
	{
		Console.Log("Meeting ended");
		Status = ESupplierStatus.Idle;
		minsSinceMeetingStart = -1;
		playerSpendSinceMeetingStart = 0f;
		meetingGreeting.ShouldShow = false;
		meetingChoice.Enabled = false;
		currentLocation.SetActiveSupplier(null);
		SetVisible(visible: false);
	}

	protected virtual void SupplierUnlocked(NPCRelationData.EUnlockType type, bool notify)
	{
		if (notify)
		{
			SendUnlockMessage();
		}
		((MonoBehaviour)this).StartCoroutine(WaitForPlayer());
		IEnumerator WaitForPlayer()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => (Object)(object)Player.Local != (Object)null));
			base.MSGConversation.EnsureUIExists();
		}
	}

	protected virtual void RelationshipChange(float change)
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (Singleton<LoadManager>.Instance.IsLoading)
		{
			if (RelationData.RelationDelta >= 5f && !DeliveriesEnabled)
			{
				EnableDeliveries(null);
			}
			return;
		}
		float num = RelationData.RelationDelta - change;
		float relationDelta = RelationData.RelationDelta;
		if (num < 4f && relationDelta >= 4f)
		{
			Console.Log("Supplier relationship high enough for meetings");
			DialogueChain chain = DialogueHandler.Database.GetChain(EDialogueModule.Generic, "supplier_meetings_unlocked");
			if (chain == null)
			{
				return;
			}
			base.MSGConversation.SendMessageChain(chain.GetMessageChain(), 3f);
		}
		if (relationDelta >= 5f && !DeliveriesEnabled)
		{
			Console.Log("Supplier relationship high enough for deliveries");
			EnableDeliveries(null);
			DialogueChain chain2 = DialogueHandler.Database.GetChain(EDialogueModule.Generic, "supplier_deliveries_unlocked");
			if (chain2 != null)
			{
				base.MSGConversation.SendMessageChain(chain2.GetMessageChain(), 3f);
			}
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void EnableDeliveries(NetworkConnection conn)
	{
		if (conn == null)
		{
			RpcWriter___Observers_EnableDeliveries_328543758(conn);
			RpcLogic___EnableDeliveries_328543758(conn);
		}
		else
		{
			RpcWriter___Target_EnableDeliveries_328543758(conn);
		}
	}

	private void SendUnlockMessage()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (!RelationData.Unlocked)
		{
			RelationData.Unlock(NPCRelationData.EUnlockType.Recommendation, notify: false);
		}
		DialogueChain chain = DialogueHandler.Database.GetChain(EDialogueModule.Generic, "supplier_unlocked");
		if (chain != null)
		{
			if (base.MSGConversation == null)
			{
				CreateMessageConversation();
			}
			base.MSGConversation.SendMessageChain(chain.GetMessageChain());
		}
	}

	protected override void CreateMessageConversation()
	{
		if (base.MSGConversation != null)
		{
			Console.LogWarning("Message conversation already exists for " + base.fullName);
			return;
		}
		base.CreateMessageConversation();
		SendableMessage sendableMessage = base.MSGConversation.CreateSendableMessage("I need to order a dead drop");
		sendableMessage.IsValidCheck = IsDeadDropValid;
		sendableMessage.disableDefaultSendBehaviour = true;
		sendableMessage.onSelected = (Action)Delegate.Combine(sendableMessage.onSelected, new Action(DeaddropRequested));
		SendableMessage sendableMessage2 = base.MSGConversation.CreateSendableMessage("We need to meet up");
		sendableMessage2.IsValidCheck = IsMeetupValid;
		sendableMessage2.onSent = (Action)Delegate.Combine(sendableMessage2.onSent, new Action(MeetupRequested));
		SendableMessage sendableMessage3 = base.MSGConversation.CreateSendableMessage("I want to pay off my debt");
		sendableMessage3.onSent = (Action)Delegate.Combine(sendableMessage3.onSent, new Action(PayDebtRequested));
	}

	protected virtual void DeaddropRequested()
	{
		float orderLimit = Mathf.Max(GetDeadDropLimit() - SyncAccessor_debt, 0f);
		PlayerSingleton<MessagesApp>.Instance.PhoneShopInterface.Open("Request Dead Drop", "Select items to order from " + FirstName, base.MSGConversation, OnlineShopItems.ToList(), orderLimit, SyncAccessor_debt, DeaddropConfirmed);
	}

	protected virtual void DeaddropConfirmed(List<PhoneShopInterface.CartEntry> cart, float totalPrice)
	{
		if (SyncAccessor_deadDropPreparing)
		{
			Console.LogWarning("Already preparing a dead drop");
			return;
		}
		int num = cart.Sum((PhoneShopInterface.CartEntry x) => x.Quantity);
		StringIntPair[] array = new StringIntPair[cart.Count];
		for (int num2 = 0; num2 < cart.Count; num2++)
		{
			array[num2] = new StringIntPair(((BaseItemDefinition)cart[num2].Listing.Item).ID, cart[num2].Quantity);
			NetworkSingleton<VariableDatabase>.Instance.NotifyItemAcquired(((BaseItemDefinition)cart[num2].Listing.Item).ID, cart[num2].Quantity);
		}
		string text = "I need a dead drop:\n";
		for (int num3 = 0; num3 < cart.Count; num3++)
		{
			if (cart[num3].Quantity > 0)
			{
				text = text + cart[num3].Quantity + "x " + ((BaseItemDefinition)cart[num3].Listing.Item).Name;
				if (num3 < cart.Count - 1)
				{
					text += "\n";
				}
			}
		}
		base.MSGConversation.SendMessage(new Message(text, Message.ESenderType.Player));
		int num4 = Mathf.Clamp(num * 30, 30, 360);
		string line = DialogueHandler.Database.GetLine(EDialogueModule.Supplier, "deaddrop_requested");
		if (num4 < 60)
		{
			line = line.Replace("<TIME>", num4 + ((num4 == 1) ? " min" : " mins"));
		}
		else
		{
			float num5 = Mathf.FloorToInt((float)num4 / 60f);
			float num6 = (float)num4 - num5 * 60f;
			string text2 = num5 + ((num5 == 1f) ? " hour" : " hours");
			if (num6 > 0f)
			{
				text2 = text2 + " " + num6 + " min";
			}
			line = line.Replace("<TIME>", text2);
		}
		base.MSGConversation.SendMessageChain(new MessageChain
		{
			Messages = new List<string> { line },
			id = Random.Range(int.MinValue, int.MaxValue)
		}, 0.5f, notify: false);
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Deaddrops_Ordered", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Deaddrops_Ordered") + 1f).ToString());
		SetDeaddrop(array, num4);
		minsSinceDeaddropOrder = 0;
		ChangeDebt(totalPrice);
		base.MSGConversation.DisplayRelationshipInfo();
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetDeaddrop(StringIntPair[] items, int minsUntilReady)
	{
		RpcWriter___Server_SetDeaddrop_3971994486(items, minsUntilReady);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void ChangeDebt(float amount)
	{
		RpcWriter___Server_ChangeDebt_431000436(amount);
		RpcLogic___ChangeDebt_431000436(amount);
	}

	private void TryRecoverDebt()
	{
		float num = Mathf.Min(SyncAccessor_debt, Stash.CashAmount);
		if (num > 0f)
		{
			Debug.Log((object)("Recovering debt: " + num));
			float num2 = SyncAccessor_debt;
			Stash.RemoveCash(num);
			ChangeDebt(0f - num);
			RelationData.ChangeRelationship(num / MaxOrderLimit * 0.5f);
			float num3 = num2 - num;
			string text = "I've received " + MoneyManager.FormatAmount(num) + " cash from you.";
			text = ((!(num3 <= 0f)) ? (text + " Your debt is now " + MoneyManager.FormatAmount(num3)) : (text + " Your debt is now paid off."));
			repaymentReminderSent = false;
			base.MSGConversation.SendMessageChain(new MessageChain
			{
				Messages = new List<string> { text },
				id = Random.Range(int.MinValue, int.MaxValue)
			});
		}
	}

	private void CompleteDeaddrop()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		DeadDrop randomEmptyDrop = DeadDrop.GetRandomEmptyDrop(((Component)Player.Local).transform.position);
		if ((Object)(object)randomEmptyDrop == (Object)null)
		{
			Console.LogError("No empty dead drop locations");
			this.sync___set_value_deadDropPreparing(false, true);
			minsUntilDeaddropReady = -1;
			deaddropItems = null;
			return;
		}
		Console.Log("Dead drop ready");
		StringIntPair[] array = deaddropItems;
		foreach (StringIntPair stringIntPair in array)
		{
			ItemDefinition item = Registry.GetItem(stringIntPair.String);
			if ((Object)(object)item == (Object)null)
			{
				Console.LogError("Item not found: " + stringIntPair.String);
				continue;
			}
			int num = stringIntPair.Int;
			while (num > 0)
			{
				int num2 = Mathf.Min(num, ((BaseItemDefinition)item).StackLimit);
				ItemInstance defaultInstance = item.GetDefaultInstance(num2);
				randomEmptyDrop.Storage.InsertItem(defaultInstance);
				num -= num2;
			}
		}
		string line = DialogueHandler.Database.GetLine(EDialogueModule.Supplier, "deaddrop_ready");
		line = line.Replace("<LOCATION>", randomEmptyDrop.DeadDropDescription);
		base.MSGConversation.SendMessageChain(new MessageChain
		{
			Messages = new List<string> { line },
			id = Random.Range(int.MinValue, int.MaxValue)
		});
		if (onDeaddropReady != null)
		{
			onDeaddropReady.Invoke();
		}
		string guidString = GUIDManager.GenerateUniqueGUID().ToString();
		NetworkSingleton<QuestManager>.Instance.CreateDeaddropCollectionQuest(null, randomEmptyDrop.GUID.ToString(), guidString);
		SetDeaddrop(null, -1);
		this.sync___set_value_deadDropPreparing(false, true);
		minsUntilDeaddropReady = -1;
		deaddropItems = null;
	}

	private void SendDebtReminder()
	{
		repaymentReminderSent = true;
		DialogueChain chain = DialogueHandler.Database.GetChain(EDialogueModule.Supplier, "supplier_request_repayment");
		chain.Lines[0] = chain.Lines[0].Replace("<DEBT>", "<color=#46CB4F>" + MoneyManager.FormatAmount(SyncAccessor_debt) + "</color>");
		base.MSGConversation.SendMessageChain(chain.GetMessageChain());
	}

	protected virtual void MeetupRequested()
	{
		if (InstanceFinder.IsServer)
		{
			int locationIndex;
			SupplierLocation appropriateLocation = GetAppropriateLocation(out locationIndex);
			string line = DialogueHandler.Database.GetLine(EDialogueModule.Generic, "supplier_meet_confirm");
			line = line.Replace("<LOCATION>", appropriateLocation.LocationDescription);
			MessageChain messageChain = new MessageChain();
			messageChain.Messages.Add(line);
			messageChain.id = Random.Range(int.MinValue, int.MaxValue);
			base.MSGConversation.SendMessageChain(messageChain, 0.5f);
			MeetAtLocation(null, locationIndex, 360);
		}
	}

	protected virtual void PayDebtRequested()
	{
		if (InstanceFinder.IsServer)
		{
			MessageChain messageChain = new MessageChain();
			messageChain.Messages.Add("You can pay off your debt by placing cash in my stash. It's " + Stash.locationDescription + ".");
			messageChain.id = Random.Range(int.MinValue, int.MaxValue);
			base.MSGConversation.SendMessageChain(messageChain, 0.5f);
		}
	}

	protected SupplierLocation GetAppropriateLocation(out int locationIndex)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		locationIndex = -1;
		List<SupplierLocation> list = new List<SupplierLocation>();
		list.AddRange(SupplierLocation.AllLocations);
		foreach (SupplierLocation allLocation in SupplierLocation.AllLocations)
		{
			if (allLocation.IsOccupied)
			{
				list.Remove(allLocation);
			}
		}
		foreach (SupplierLocation allLocation2 in SupplierLocation.AllLocations)
		{
			foreach (Player player in Player.PlayerList)
			{
				if (Vector3.Distance(((Component)allLocation2).transform.position, player.Avatar.CenterPoint) < 30f)
				{
					list.Remove(allLocation2);
				}
			}
		}
		if (list.Count == 0)
		{
			Console.LogError("No available locations for supplier");
			return null;
		}
		SupplierLocation supplierLocation = list[Random.Range(0, list.Count)];
		locationIndex = SupplierLocation.AllLocations.IndexOf(supplierLocation);
		return supplierLocation;
	}

	private bool IsDeadDropValid(SendableMessage message, out string invalidReason)
	{
		invalidReason = string.Empty;
		if (SyncAccessor_deadDropPreparing)
		{
			invalidReason = "Already waiting for a dead drop";
			return false;
		}
		return true;
	}

	private bool IsMeetupValid(SendableMessage message, out string invalidReason)
	{
		if (RelationData.RelationDelta < 4f)
		{
			invalidReason = "Insufficient trust";
			return false;
		}
		if (Status != ESupplierStatus.Idle)
		{
			invalidReason = "Busy";
			return false;
		}
		invalidReason = "";
		return true;
	}

	public virtual float GetDeadDropLimit()
	{
		return Mathf.Lerp(MinOrderLimit, MaxOrderLimit, RelationData.RelationDelta / 5f);
	}

	public override NPCData GetNPCData()
	{
		return new SupplierData(ID, minsSinceMeetingStart, minsSinceLastMeetingEnd, SyncAccessor_debt, minsUntilDeaddropReady, deaddropItems, repaymentReminderSent);
	}

	public override void Load(NPCData data, string containerPath)
	{
		base.Load(data, containerPath);
		if (((ISaveable)this).TryLoadFile(containerPath, "NPC", out string contents))
		{
			SupplierData supplierData = null;
			try
			{
				supplierData = JsonUtility.FromJson<SupplierData>(contents);
			}
			catch (Exception ex)
			{
				Console.LogWarning("Failed to deserialize character data: " + ex.Message);
				return;
			}
			minsSinceMeetingStart = supplierData.timeSinceMeetingStart;
			minsSinceLastMeetingEnd = supplierData.timeSinceLastMeetingEnd;
			this.sync___set_value_debt(supplierData.debt, true);
			minsUntilDeaddropReady = supplierData.minsUntilDeadDropReady;
			if (minsUntilDeaddropReady > 0)
			{
				this.sync___set_value_deadDropPreparing(true, true);
			}
			if (supplierData.deaddropItems != null)
			{
				deaddropItems = supplierData.deaddropItems.ToArray();
			}
			repaymentReminderSent = supplierData.debtReminderSent;
		}
	}

	public override void Load(DynamicSaveData dynamicData, NPCData npcData)
	{
		base.Load(dynamicData, npcData);
		if (dynamicData.TryExtractBaseData<SupplierData>(out var data))
		{
			minsSinceMeetingStart = data.timeSinceMeetingStart;
			minsSinceLastMeetingEnd = data.timeSinceLastMeetingEnd;
			this.sync___set_value_debt(data.debt, true);
			minsUntilDeaddropReady = data.minsUntilDeadDropReady;
			if (minsUntilDeaddropReady > 0)
			{
				this.sync___set_value_deadDropPreparing(true, true);
			}
			if (data.deaddropItems != null)
			{
				deaddropItems = data.deaddropItems.ToArray();
			}
			repaymentReminderSent = data.debtReminderSent;
		}
	}

	private void MeetupOrderCompleted(float spend)
	{
		DialogueHandler.ShowWorldspaceDialogue(DialogueHandler.Database.GetLine(EDialogueModule.Generic, "meeting_order_complete"), 3f);
		float num = Mathf.Min(spend, Mathf.Max(MaxOrderLimit - playerSpendSinceMeetingStart, 0f));
		Debug.Log((object)("Spend under threshold: " + num));
		RelationData.ChangeRelationship(num / MaxOrderLimit * 0.5f);
		playerSpendSinceMeetingStart += spend;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EEconomy_002ESupplierAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEconomy_002ESupplierAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar___deadDropPreparing = new SyncVar<bool>((NetworkBehaviour)(object)this, 2u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, deadDropPreparing);
			syncVar___debt = new SyncVar<float>((NetworkBehaviour)(object)this, 1u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, debt);
			((NetworkBehaviour)this).RegisterServerRpc(39u, new ServerRpcDelegate(RpcReader___Server_SendUnlocked_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(40u, new ClientRpcDelegate(RpcReader___Observers_SetUnlocked_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(41u, new ClientRpcDelegate(RpcReader___Observers_MeetAtLocation_3470796954));
			((NetworkBehaviour)this).RegisterObserversRpc(42u, new ClientRpcDelegate(RpcReader___Observers_EnableDeliveries_328543758));
			((NetworkBehaviour)this).RegisterTargetRpc(43u, new ClientRpcDelegate(RpcReader___Target_EnableDeliveries_328543758));
			((NetworkBehaviour)this).RegisterServerRpc(44u, new ServerRpcDelegate(RpcReader___Server_SetDeaddrop_3971994486));
			((NetworkBehaviour)this).RegisterServerRpc(45u, new ServerRpcDelegate(RpcReader___Server_ChangeDebt_431000436));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EEconomy_002ESupplier));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEconomy_002ESupplierAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEconomy_002ESupplierAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)syncVar___deadDropPreparing).SetRegistered();
			((SyncBase)syncVar___debt).SetRegistered();
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
			((NetworkBehaviour)this).SendServerRpc(39u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendUnlocked_2166136261()
	{
		SetUnlocked();
	}

	private void RpcReader___Server_SendUnlocked_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendUnlocked_2166136261();
		}
	}

	private void RpcWriter___Observers_SetUnlocked_2166136261()
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

	private void RpcLogic___SetUnlocked_2166136261()
	{
		RelationData.Unlock(NPCRelationData.EUnlockType.Recommendation);
	}

	private void RpcReader___Observers_SetUnlocked_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetUnlocked_2166136261();
		}
	}

	private void RpcWriter___Observers_MeetAtLocation_3470796954(NetworkConnection conn, int locationIndex, int expireIn)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkConnection(conn);
			((Writer)writer).WriteInt32(locationIndex, (AutoPackType)1);
			((Writer)writer).WriteInt32(expireIn, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(41u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___MeetAtLocation_3470796954(NetworkConnection conn, int locationIndex, int expireIn)
	{
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		SupplierLocation supplierLocation = SupplierLocation.AllLocations[locationIndex];
		if ((Object)(object)supplierLocation == (Object)null)
		{
			Console.LogError("Location not found: " + locationIndex);
			return;
		}
		if ((Object)(object)supplierLocation.SupplierStandPoint == (Object)null)
		{
			Console.LogError("Supplier stand point not set up for location: " + ((Object)supplierLocation).name);
			return;
		}
		if (meetingGreeting == null || meetingChoice == null)
		{
			Console.LogError("Meeting greeting or choice not set up");
			return;
		}
		Console.Log(base.fullName + " meeting at " + ((Object)supplierLocation).name + " for " + expireIn + " minutes");
		Status = ESupplierStatus.Meeting;
		currentLocation = supplierLocation;
		minsSinceMeetingStart = 0;
		playerSpendSinceMeetingStart = 0f;
		supplierLocation.SetActiveSupplier(this);
		ShopInterface shop = Shop;
		StorageEntity[] deliveryBays = supplierLocation.DeliveryBays;
		shop.DeliveryBays = deliveryBays;
		meetingGreeting.ShouldShow = true;
		meetingChoice.Enabled = true;
		Movement.Warp(supplierLocation.SupplierStandPoint.position);
		Movement.FaceDirection(supplierLocation.SupplierStandPoint.forward);
		SetVisible(visible: true);
	}

	private void RpcReader___Observers_MeetAtLocation_3470796954(PooledReader PooledReader0, Channel channel)
	{
		NetworkConnection conn = ((Reader)PooledReader0).ReadNetworkConnection();
		int locationIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int expireIn = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___MeetAtLocation_3470796954(conn, locationIndex, expireIn);
		}
	}

	private void RpcWriter___Observers_EnableDeliveries_328543758(NetworkConnection conn)
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

	private void RpcLogic___EnableDeliveries_328543758(NetworkConnection conn)
	{
		DeliveriesEnabled = true;
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => PlayerSingleton<DeliveryApp>.InstanceExists));
			PlayerSingleton<DeliveryApp>.Instance.SetIsAvailable(Shop, available: true);
		}
	}

	private void RpcReader___Observers_EnableDeliveries_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___EnableDeliveries_328543758(null);
		}
	}

	private void RpcWriter___Target_EnableDeliveries_328543758(NetworkConnection conn)
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

	private void RpcReader___Target_EnableDeliveries_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___EnableDeliveries_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	private void RpcWriter___Server_SetDeaddrop_3971994486(StringIntPair[] items, int minsUntilReady)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerated((Writer)(object)writer, items);
			((Writer)writer).WriteInt32(minsUntilReady, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(44u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SetDeaddrop_3971994486(StringIntPair[] items, int minsUntilReady)
	{
		if (items != null)
		{
			minsSinceDeaddropOrder = 0;
			this.sync___set_value_deadDropPreparing(true, true);
		}
		else
		{
			this.sync___set_value_deadDropPreparing(false, true);
		}
		minsUntilDeaddropReady = minsUntilReady;
		deaddropItems = items;
	}

	private void RpcReader___Server_SetDeaddrop_3971994486(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		StringIntPair[] items = GeneratedReaders___Internal.Read___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		int minsUntilReady = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SetDeaddrop_3971994486(items, minsUntilReady);
		}
	}

	private void RpcWriter___Server_ChangeDebt_431000436(float amount)
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
			((Writer)writer).WriteSingle(amount, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(45u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___ChangeDebt_431000436(float amount)
	{
		this.sync___set_value_debt(Mathf.Clamp(SyncAccessor_debt + amount, 0f, GetDeadDropLimit()), true);
	}

	private void RpcReader___Server_ChangeDebt_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		float amount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___ChangeDebt_431000436(amount);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EEconomy_002ESupplier(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		switch (UInt321)
		{
		case 2u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value_deadDropPreparing(syncVar___deadDropPreparing.GetValue(true), true);
				return true;
			}
			bool value2 = ((Reader)PooledReader0).ReadBoolean();
			this.sync___set_value_deadDropPreparing(value2, Boolean2);
			return true;
		}
		case 1u:
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value_debt(syncVar___debt.GetValue(true), true);
				return true;
			}
			float value = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
			this.sync___set_value_debt(value, Boolean2);
			return true;
		}
		default:
			return false;
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EEconomy_002ESupplier_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		SetVisible(visible: false);
	}
}
