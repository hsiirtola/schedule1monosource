using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using ScheduleOne.UI;
using ScheduleOne.UI.Handover;
using ScheduleOne.UI.Phone;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.Quests;

public class Contract : Quest
{
	public class BonusPayment
	{
		public string Title;

		public float Amount;

		public BonusPayment(string title, float amount)
		{
			Title = title;
			Amount = Mathf.Clamp(amount, 0f, float.MaxValue);
		}
	}

	public const int DefaultExpiryTime = 2880;

	public const float ExcessProductsMatchSumMultiplier = 0.5f;

	public static List<Contract> Contracts = new List<Contract>();

	[Header("Contract Settings")]
	public ProductList ProductList;

	public DeliveryLocation DeliveryLocation;

	public QuestWindowConfig DeliveryWindow;

	private bool completedContractsIncremented;

	public NetworkObject Customer { get; protected set; }

	public Dealer Dealer { get; protected set; }

	public float Payment { get; protected set; }

	public int PickupScheduleIndex { get; protected set; }

	public GameDateTime AcceptTime { get; protected set; }

	protected override void Start()
	{
		autoInitialize = false;
		base.Start();
	}

	public virtual void InitializeContract(string title, string description, QuestEntryData[] entries, string guid, Customer customer, float payment, ProductList products, string deliveryLocationGUID, QuestWindowConfig deliveryWindow, int pickupScheduleIndex, GameDateTime acceptTime)
	{
		SilentlyInitializeContract(base.title, Description, entries, guid, customer, payment, products, deliveryLocationGUID, deliveryWindow, pickupScheduleIndex, acceptTime);
		Debug.Log((object)"Contract initialized");
		Contracts.Add(this);
		base.InitializeQuest(title, description, entries, guid);
		((Component)Customer).GetComponent<Customer>().AssignContract(this);
		if ((Object)(object)DeliveryLocation != (Object)null && !DeliveryLocation.ScheduledContracts.Contains(this))
		{
			DeliveryLocation.ScheduledContracts.Add(this);
		}
		UpdateTiming();
		UpdatePoI();
	}

	public virtual void SilentlyInitializeContract(string title, string description, QuestEntryData[] entries, string guid, Customer customer, float payment, ProductList products, string deliveryLocationGUID, QuestWindowConfig deliveryWindow, int pickupScheduleIndex, GameDateTime acceptTime)
	{
		Customer = ((NetworkBehaviour)customer).NetworkObject;
		Payment = Mathf.Clamp(payment, 0f, float.MaxValue);
		ProductList = products;
		if (GUIDManager.IsGUIDValid(deliveryLocationGUID))
		{
			DeliveryLocation = GUIDManager.GetObject<DeliveryLocation>(new Guid(deliveryLocationGUID));
		}
		DeliveryWindow = deliveryWindow;
		PickupScheduleIndex = pickupScheduleIndex;
		AcceptTime = acceptTime;
	}

	protected override void OnUncappedMinPass()
	{
		base.OnUncappedMinPass();
		UpdateTiming();
		UpdatePoI();
	}

	private void OnDestroy()
	{
		Contracts.Remove(this);
	}

	private void UpdateTiming()
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Expires || ExpiryVisibility == EExpiryVisibility.Never)
		{
			return;
		}
		int minsUntilExpiry = GetMinsUntilExpiry();
		int num = Mathf.FloorToInt((float)minsUntilExpiry / 60f);
		int num2 = minsUntilExpiry - 360;
		int num3 = Mathf.FloorToInt((float)num2 / 60f);
		if (num2 > 0)
		{
			if (num3 > 0)
			{
				SetSubtitle("<color=#c0c0c0ff> (Begins in " + num3 + " hrs)</color>");
			}
			else
			{
				SetSubtitle("<color=#c0c0c0ff> (Begins in " + num2 + " min)</color>");
			}
		}
		else if (minsUntilExpiry < 120)
		{
			if (num > 0)
			{
				SetSubtitle("<color=#" + ColorUtility.ToHtmlStringRGBA(((Graphic)criticalTimeBackground).color) + "> (Expires in " + num + " hrs)</color>");
			}
			else
			{
				SetSubtitle("<color=#" + ColorUtility.ToHtmlStringRGBA(((Graphic)criticalTimeBackground).color) + "> (Expires in " + minsUntilExpiry + " min)</color>");
			}
		}
		else if (num > 0)
		{
			SetSubtitle("<color=green> (Expires in " + num + " hrs)</color>");
		}
		else
		{
			SetSubtitle("<color=green> (Expires in " + num + " min)</color>");
		}
	}

	public void UpdatePoI()
	{
		int minsUntilExpiry = GetMinsUntilExpiry();
		int num = minsUntilExpiry - 360;
		if ((Object)(object)Entries[0].PoI != (Object)null)
		{
			if (num > 0)
			{
				Entries[0].SetPoIColor("Background", "Future");
			}
			else if (minsUntilExpiry < 120)
			{
				Entries[0].SetPoIColor("Background", "ExpiringSoon");
			}
			else
			{
				Entries[0].SetPoIColor("Background", "Active");
			}
		}
	}

	public override void End()
	{
		base.End();
		if ((Object)(object)DeliveryLocation != (Object)null)
		{
			DeliveryLocation.ScheduledContracts.Remove(this);
		}
		Contracts.Remove(this);
	}

	public override void Complete(bool network = true)
	{
		if (InstanceFinder.IsServer && !completedContractsIncremented)
		{
			completedContractsIncremented = true;
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Completed_Contracts_Count", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Completed_Contracts_Count") + 1f).ToString());
		}
		base.Complete(network);
	}

	public override void Expire(bool network = true)
	{
		if ((Object)(object)Dealer != (Object)null)
		{
			Debug.LogWarning((object)("Dealer contract expired! It was assigned to dealer: " + Dealer.fullName));
		}
		base.Expire(network);
	}

	public override void Fail(bool network = true)
	{
		if ((Object)(object)Dealer != (Object)null)
		{
			Debug.LogWarning((object)("Dealer contract failed! It was assigned to dealer: " + Dealer.fullName));
		}
		base.Fail(network);
	}

	public void SetDealer(Dealer dealer)
	{
		Dealer = dealer;
		if ((Object)(object)dealer != (Object)null)
		{
			ShouldSendExpiryReminder = false;
			ShouldSendExpiredNotification = false;
		}
		if ((Object)(object)journalEntry != (Object)null)
		{
			((Component)journalEntry).gameObject.SetActive(ShouldShowJournalEntry());
		}
	}

	public virtual void SubmitPayment(float bonusTotal)
	{
		NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(Payment + bonusTotal);
	}

	protected override void SendExpiryReminder()
	{
		Singleton<NotificationsManager>.Instance.SendNotification("<color=#FFB43C>Deal Expiring Soon</color>", title, PlayerSingleton<JournalApp>.Instance.AppIcon);
	}

	protected override void SendExpiredNotification()
	{
		Singleton<NotificationsManager>.Instance.SendNotification("<color=#FF6455>Deal Expired</color>", title, PlayerSingleton<JournalApp>.Instance.AppIcon);
	}

	protected override bool ShouldShowJournalEntry()
	{
		if ((Object)(object)Dealer != (Object)null)
		{
			return false;
		}
		return base.ShouldShowJournalEntry();
	}

	protected override bool CanExpire()
	{
		if ((Object)(object)Singleton<HandoverScreen>.Instance.CurrentContract == (Object)(object)this)
		{
			return false;
		}
		if (((Component)Customer).GetComponent<NPC>().DialogueHandler.IsDialogueInProgress)
		{
			return false;
		}
		return base.CanExpire();
	}

	public bool DoesProductListMatchSpecified(List<ItemInstance> items, bool enforceQuality)
	{
		foreach (ProductList.Entry entry in ProductList.entries)
		{
			List<ItemInstance> list = items.Where((ItemInstance x) => ((BaseItemInstance)x).ID == entry.ProductID).ToList();
			List<ProductItemInstance> list2 = new List<ProductItemInstance>();
			for (int num = 0; num < list.Count; num++)
			{
				list2.Add(list[num] as ProductItemInstance);
			}
			List<ProductItemInstance> list3 = new List<ProductItemInstance>();
			for (int num2 = 0; num2 < items.Count; num2++)
			{
				ProductItemInstance productItemInstance = items[num2] as ProductItemInstance;
				if (productItemInstance.Quality >= entry.Quality)
				{
					list3.Add(productItemInstance);
				}
			}
			int num3 = 0;
			for (int num4 = 0; num4 < list2.Count; num4++)
			{
				num3 += ((BaseItemInstance)list2[num4]).Quantity * list2[num4].Amount;
			}
			int num5 = 0;
			for (int num6 = 0; num6 < list3.Count; num6++)
			{
				num5 += ((BaseItemInstance)list3[num6]).Quantity * list2[num6].Amount;
			}
			if (enforceQuality)
			{
				if (num5 < entry.Quantity)
				{
					return false;
				}
			}
			else if (num3 < entry.Quantity)
			{
				return false;
			}
		}
		return true;
	}

	public float GetProductListMatch(List<ItemInstance> items, out int matchedProductCount)
	{
		float num = 0f;
		int totalQuantity = ProductList.GetTotalQuantity();
		matchedProductCount = 0;
		List<ItemInstance> list = new List<ItemInstance>();
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] is ProductItemInstance productItemInstance && !((Object)(object)productItemInstance.AppliedPackaging == (Object)null))
			{
				list.Add(items[i].GetCopy());
			}
		}
		foreach (ProductList.Entry entry in ProductList.entries)
		{
			int num2 = entry.Quantity;
			Dictionary<ProductItemInstance, float> descendingMatchRatings = GetDescendingMatchRatings(entry, list);
			List<ProductItemInstance> list2 = descendingMatchRatings.Keys.ToList();
			for (int j = 0; j < list2.Count; j++)
			{
				if (((BaseItemInstance)list2[j]).Quantity > 0)
				{
					int amount = list2[j].Amount;
					_ = ((BaseItemInstance)list2[j]).Quantity;
					int num3 = Mathf.Min(Mathf.CeilToInt((float)num2 / (float)amount), ((BaseItemInstance)list2[j]).Quantity);
					int num4 = num3 * amount;
					num += descendingMatchRatings[list2[j]] * (float)num4;
					num2 -= num4;
					if (descendingMatchRatings[list2[j]] >= 1f)
					{
						matchedProductCount += num3 * amount;
					}
					((BaseItemInstance)list2[j]).ChangeQuantity(-num3);
				}
			}
		}
		foreach (ProductList.Entry entry2 in ProductList.entries)
		{
			Dictionary<ProductItemInstance, float> descendingMatchRatings2 = GetDescendingMatchRatings(entry2, list);
			List<ProductItemInstance> list3 = descendingMatchRatings2.Keys.ToList();
			for (int k = 0; k < list3.Count; k++)
			{
				if (((BaseItemInstance)list3[k]).Quantity > 0)
				{
					int num5 = ((BaseItemInstance)list3[k]).Quantity * list3[k].Amount;
					num += descendingMatchRatings2[list3[k]] * (float)num5 * 0.5f;
					if (descendingMatchRatings2[list3[k]] >= 1f)
					{
						matchedProductCount += num5;
					}
					((BaseItemInstance)list3[k]).SetQuantity(0);
				}
			}
		}
		return Mathf.Clamp01(num / (float)totalQuantity);
	}

	private Dictionary<ProductItemInstance, float> GetDescendingMatchRatings(ProductList.Entry requestedItem, List<ItemInstance> providedItems)
	{
		Dictionary<ProductItemInstance, float> matchRatings = new Dictionary<ProductItemInstance, float>();
		foreach (ItemInstance providedItem in providedItems)
		{
			if (((BaseItemInstance)providedItem).Quantity != 0)
			{
				ProductItemInstance productItemInstance = providedItem as ProductItemInstance;
				matchRatings.Add(productItemInstance, productItemInstance.GetSimilarity(Registry.GetItem(requestedItem.ProductID) as ProductDefinition, requestedItem.Quality));
			}
		}
		List<ProductItemInstance> list = matchRatings.Keys.ToList();
		list.Sort((ProductItemInstance a, ProductItemInstance b) => matchRatings[b].CompareTo(matchRatings[a]));
		Dictionary<ProductItemInstance, float> dictionary = new Dictionary<ProductItemInstance, float>();
		for (int num = 0; num < list.Count; num++)
		{
			dictionary.Add(list[num], matchRatings[list[num]]);
		}
		return dictionary;
	}

	public override SaveData GetSaveData()
	{
		List<QuestEntryData> list = new List<QuestEntryData>();
		for (int i = 0; i < Entries.Count; i++)
		{
			list.Add(Entries[i].GetSaveData());
		}
		return new ContractData(base.GUID.ToString(), base.State, base.IsTracked, title, Description, base.Expires, new GameDateTimeData(base.Expiry), list.ToArray(), ((Component)Customer).GetComponent<NPC>().GUID.ToString(), Payment, ProductList, DeliveryLocation.GUID.ToString(), DeliveryWindow, PickupScheduleIndex, new GameDateTimeData(AcceptTime));
	}

	public new bool ShouldSave()
	{
		if ((Object)(object)((Component)this).gameObject == (Object)null)
		{
			return false;
		}
		return base.State == EQuestState.Active;
	}
}
