using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Configuration;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Core.Settings.Framework;
using ScheduleOne.Delivery;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Money;
using ScheduleOne.Property;
using ScheduleOne.UI.Shop;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Delivery;

public class DeliveryShop : MonoBehaviour
{
	[Header("References")]
	public Button BackButton;

	public RectTransform ListingContainer;

	public Text DeliveryFeeLabel;

	public Text ItemTotalLabel;

	public Text OrderTotalLabel;

	public Text DeliveryTimeLabel;

	public Button OrderButton;

	public Text OrderButtonNote;

	public Dropdown DestinationDropdown;

	public Dropdown LoadingDockDropdown;

	[Header("Settings")]
	public string MatchingShopInterfaceName = "ShopInterface";

	public Color ShopColor;

	public bool AvailableByDefault;

	public ListingEntry ListingEntryPrefab;

	private List<ListingEntry> listingEntries = new List<ListingEntry>();

	private ScheduleOne.Property.Property destinationProperty;

	private int loadingDockIndex;

	private Action<DeliveryShop> _onSelect;

	public ShopInterface MatchingShop { get; private set; }

	public bool IsOpen { get; private set; }

	public Action<DeliveryShop> OnSelect
	{
		get
		{
			return _onSelect;
		}
		set
		{
			_onSelect = value;
		}
	}

	public void Initialize()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		MatchingShop = ShopInterface.AllShops.Find((ShopInterface x) => x.ShopName == MatchingShopInterfaceName);
		if ((Object)(object)MatchingShop == (Object)null)
		{
			Debug.LogError((object)("Could not find shop interface with name " + MatchingShopInterfaceName));
			return;
		}
		foreach (ShopListing listing in MatchingShop.Listings)
		{
			if (listing.CanBeDelivered)
			{
				ListingEntry listingEntry = Object.Instantiate<ListingEntry>(ListingEntryPrefab, (Transform)(object)ListingContainer);
				listingEntry.Initialize(listing);
				listingEntry.onQuantityChanged.AddListener(new UnityAction(RefreshCart));
				listingEntries.Add(listingEntry);
			}
		}
		((UnityEvent)BackButton.onClick).AddListener((UnityAction)delegate
		{
			_onSelect?.Invoke(this);
		});
		((UnityEvent)OrderButton.onClick).AddListener((UnityAction)delegate
		{
			SubmitOrder(string.Empty);
		});
		((UnityEvent<int>)(object)DestinationDropdown.onValueChanged).AddListener((UnityAction<int>)DestinationDropdownSelected);
		((UnityEvent<int>)(object)LoadingDockDropdown.onValueChanged).AddListener((UnityAction<int>)LoadingDockDropdownSelected);
		((Component)this).gameObject.SetActive(false);
	}

	private void FixedUpdate()
	{
		if (IsOpen && PlayerSingleton<DeliveryApp>.Instance.isOpen)
		{
			RefreshOrderButton();
		}
	}

	public void Open()
	{
		IsOpen = true;
		PlayerSingleton<DeliveryApp>.Instance.RefreshContent();
	}

	public void Close()
	{
		IsOpen = false;
	}

	public void SubmitOrder(string originalDeliveryID)
	{
		if (!CanOrder(out var reason))
		{
			Debug.LogWarning((object)("Cannot order: " + reason));
			return;
		}
		if (!Singleton<ConfigurationService>.Instance.TryGetConfiguration<DeliveryConfiguration>(out var _))
		{
			Debug.LogError((object)"Could not get delivery configuration");
			return;
		}
		float num = GetDeliveryFee() + GetCartCost();
		List<StringIntPair> list = new List<StringIntPair>();
		foreach (ListingEntry listingEntry in listingEntries)
		{
			if (listingEntry.SelectedQuantity > 0)
			{
				list.Add(new StringIntPair(((BaseItemDefinition)listingEntry.MatchingListing.Item).ID, listingEntry.SelectedQuantity));
				NetworkSingleton<VariableDatabase>.Instance.NotifyItemAcquired(((BaseItemDefinition)listingEntry.MatchingListing.Item).ID, listingEntry.SelectedQuantity);
			}
		}
		int deliveryTime = GetDeliveryTime(GetOrderItemCount());
		DeliveryInstance deliveryInstance = new DeliveryInstance(GUIDManager.GenerateUniqueGUID().ToString(), MatchingShopInterfaceName, destinationProperty.PropertyCode, loadingDockIndex - 1, list.ToArray(), EDeliveryStatus.InTransit, deliveryTime);
		NetworkSingleton<DeliveryManager>.Instance.SendDelivery(deliveryInstance);
		NetworkSingleton<DeliveryManager>.Instance.RecordDeliveryReceipt_Server(deliveryInstance.GetReceipt(), originalDeliveryID);
		NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Delivery from " + MatchingShop.ShopName, 0f - num, 1f, string.Empty);
		PlayerSingleton<DeliveryApp>.Instance.PlayOrderSubmittedAnim();
		ResetCart();
		PlayerSingleton<DeliveryApp>.Instance.OnSubmitOrder(this);
	}

	private int GetDeliveryTime(int itemCount)
	{
		if (!Singleton<ConfigurationService>.Instance.TryGetConfiguration<DeliveryConfiguration>(out var configuration))
		{
			Debug.LogError((object)"Could not get delivery configuration");
			return 0;
		}
		return Mathf.Min(Mathf.Max(Mathf.CeilToInt((float)itemCount * ((SettingsField<float>)(object)configuration.Settings.DeliveryTimePerItem).Value), ((SettingsField<int>)(object)configuration.Settings.MinimumDeliveryTime).Value), ((SettingsField<int>)(object)configuration.Settings.MaximumDeliveryTime).Value);
	}

	public void Reorder(DeliveryReceipt receipt)
	{
		StringIntPair[] items = receipt.Items;
		foreach (StringIntPair item in items)
		{
			ListingEntry listingEntry = listingEntries.Find((ListingEntry x) => ((BaseItemDefinition)x.MatchingListing.Item).ID == item.String);
			if ((Object)(object)listingEntry != (Object)null)
			{
				listingEntry.SetQuantity(item.Int, notify: false);
			}
		}
		destinationProperty = Singleton<PropertyManager>.Instance.GetProperty(receipt.DestinationCode);
		loadingDockIndex = receipt.LoadingDockIndex + 1;
		SubmitOrder(receipt.DeliveryID);
	}

	public bool CanReorder(DeliveryReceipt receipt, out string reason)
	{
		if (NetworkSingleton<DeliveryManager>.Instance.GetActiveShopDelivery(this) != null)
		{
			reason = "Delivery already in progress";
			return false;
		}
		if (GetDeliveryCost(receipt) > NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance)
		{
			reason = "Insufficient online balance";
			return false;
		}
		reason = string.Empty;
		return true;
	}

	public float GetDeliveryCost(DeliveryReceipt receipt)
	{
		float num = 0f;
		StringIntPair[] items = receipt.Items;
		foreach (StringIntPair item in items)
		{
			ListingEntry listingEntry = listingEntries.Find((ListingEntry x) => ((BaseItemDefinition)x.MatchingListing.Item).ID == item.String);
			if ((Object)(object)listingEntry != (Object)null)
			{
				num += (float)item.Int * listingEntry.MatchingListing.Price;
			}
		}
		return num + GetDeliveryFee();
	}

	public void RefreshShop()
	{
		RefreshCart();
		RefreshOrderButton();
		RefreshDestinationUI();
		RefreshLoadingDockUI();
		RefreshEntryOrder();
		RefreshEntriesLocked();
		Singleton<ConfigurationService>.Instance.TryGetConfiguration<DeliveryConfiguration>(out var configuration);
		DeliveryFeeLabel.text = MoneyManager.FormatAmount(((SettingsField<float>)(object)configuration.Settings.DeliveryFee).Value);
	}

	public void ResetCart()
	{
		foreach (ListingEntry listingEntry in listingEntries)
		{
			listingEntry.SetQuantity(0, notify: false);
		}
		RefreshCart();
		RefreshOrderButton();
	}

	private void RefreshCart()
	{
		float cartCost = GetCartCost();
		ItemTotalLabel.text = MoneyManager.FormatAmount(cartCost);
		OrderTotalLabel.text = MoneyManager.FormatAmount(cartCost + GetDeliveryFee());
		int deliveryTime = GetDeliveryTime(GetOrderItemCount());
		DeliveryTimeLabel.text = TimeManager.GetMinutesToDisplayTime(deliveryTime);
	}

	private void RefreshOrderButton()
	{
		if (CanOrder(out var reason))
		{
			((Selectable)OrderButton).interactable = true;
			((Behaviour)OrderButtonNote).enabled = false;
		}
		else
		{
			((Selectable)OrderButton).interactable = false;
			OrderButtonNote.text = reason;
			((Behaviour)OrderButtonNote).enabled = true;
		}
	}

	public bool CanOrder(out string reason)
	{
		reason = string.Empty;
		if (HasActiveDelivery())
		{
			reason = "Delivery already in progress";
			return false;
		}
		float cartCost = GetCartCost();
		if (cartCost + GetDeliveryFee() > NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance)
		{
			reason = "Insufficient online balance";
			return false;
		}
		if ((Object)(object)destinationProperty == (Object)null)
		{
			reason = "Select a destination";
			return false;
		}
		if (destinationProperty.LoadingDockCount == 0)
		{
			reason = "Selected destination has no loading docks";
			return false;
		}
		if (loadingDockIndex == 0)
		{
			reason = "Select a loading dock";
			return false;
		}
		if (!WillCartFitInVehicle())
		{
			reason = "Order is too large for delivery vehicle";
			return false;
		}
		return cartCost > 0f;
	}

	public bool HasActiveDelivery()
	{
		return NetworkSingleton<DeliveryManager>.Instance.GetActiveShopDelivery(this) != null;
	}

	public bool WillCartFitInVehicle()
	{
		int num = 0;
		foreach (ListingEntry listingEntry in listingEntries)
		{
			if (listingEntry.SelectedQuantity != 0)
			{
				int num2 = listingEntry.SelectedQuantity;
				int stackLimit = ((BaseItemDefinition)listingEntry.MatchingListing.Item).StackLimit;
				while (num2 > 0)
				{
					num2 = ((num2 > stackLimit) ? (num2 - stackLimit) : 0);
					num++;
				}
			}
		}
		return num <= MatchingShop.DeliveryVehicle.Vehicle.Storage.SlotCount;
	}

	public void RefreshDestinationUI()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		ScheduleOne.Property.Property property = destinationProperty;
		destinationProperty = null;
		DestinationDropdown.ClearOptions();
		List<OptionData> list = new List<OptionData>();
		list.Add(new OptionData("-"));
		List<ScheduleOne.Property.Property> potentialDestinations = GetPotentialDestinations();
		int num = 0;
		for (int i = 0; i < potentialDestinations.Count; i++)
		{
			list.Add(new OptionData(potentialDestinations[i].PropertyName));
			if ((Object)(object)potentialDestinations[i] == (Object)(object)property)
			{
				num = i + 1;
			}
		}
		DestinationDropdown.AddOptions(list);
		DestinationDropdown.SetValueWithoutNotify(num);
		DestinationDropdownSelected(num);
	}

	private void DestinationDropdownSelected(int index)
	{
		if (index > 0 && index <= GetPotentialDestinations().Count)
		{
			destinationProperty = GetPotentialDestinations()[index - 1];
			if (loadingDockIndex == 0 && destinationProperty.LoadingDockCount > 0)
			{
				loadingDockIndex = 1;
			}
		}
		else
		{
			destinationProperty = null;
		}
		RefreshLoadingDockUI();
	}

	private List<ScheduleOne.Property.Property> GetPotentialDestinations()
	{
		return new List<ScheduleOne.Property.Property>(ScheduleOne.Property.Property.OwnedProperties.Where((ScheduleOne.Property.Property x) => x.CanDeliverToProperty()));
	}

	public void RefreshLoadingDockUI()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		int num = loadingDockIndex;
		loadingDockIndex = 0;
		LoadingDockDropdown.ClearOptions();
		List<OptionData> list = new List<OptionData>();
		list.Add(new OptionData("-"));
		if ((Object)(object)destinationProperty != (Object)null)
		{
			for (int i = 0; i < destinationProperty.LoadingDockCount; i++)
			{
				list.Add(new OptionData((i + 1).ToString()));
			}
		}
		LoadingDockDropdown.AddOptions(list);
		int num2 = Mathf.Clamp(num, 0, list.Count - 1);
		LoadingDockDropdown.SetValueWithoutNotify(num2);
		LoadingDockDropdownSelected(num2);
	}

	private void LoadingDockDropdownSelected(int index)
	{
		loadingDockIndex = index;
	}

	private float GetCartCost()
	{
		float num = 0f;
		foreach (ListingEntry listingEntry in listingEntries)
		{
			num += (float)listingEntry.SelectedQuantity * listingEntry.MatchingListing.Price;
		}
		return num;
	}

	private float GetDeliveryFee()
	{
		if (!Singleton<ConfigurationService>.Instance.TryGetConfiguration<DeliveryConfiguration>(out var configuration))
		{
			Debug.LogError((object)"Could not get delivery configuration");
			return 0f;
		}
		return ((SettingsField<float>)(object)configuration.Settings.DeliveryFee).Value;
	}

	private int GetOrderItemCount()
	{
		int num = 0;
		foreach (ListingEntry listingEntry in listingEntries)
		{
			num += listingEntry.SelectedQuantity;
		}
		return num;
	}

	private void RefreshEntryOrder()
	{
		List<ListingEntry> list = new List<ListingEntry>();
		List<ListingEntry> list2 = new List<ListingEntry>();
		foreach (ListingEntry listingEntry in listingEntries)
		{
			if (!listingEntry.MatchingListing.Item.IsUnlocked)
			{
				list2.Add(listingEntry);
			}
			else
			{
				list.Add(listingEntry);
			}
		}
		list.AddRange(list2);
		for (int i = 0; i < list.Count; i++)
		{
			((Component)list[i]).transform.SetSiblingIndex(i);
		}
	}

	private void RefreshEntriesLocked()
	{
		foreach (ListingEntry listingEntry in listingEntries)
		{
			listingEntry.RefreshLocked();
		}
	}
}
