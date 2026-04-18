using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop;

public class Cart : MonoBehaviour
{
	[Header("References")]
	public ShopInterface Shop;

	public RectTransform CartEntryContainer;

	public TextMeshProUGUI ProblemText;

	public TextMeshProUGUI WarningText;

	public RectTransform CartContainer;

	public Image CartArea;

	public TextMeshProUGUI TotalText;

	public Toggle LoadVehicleToggle;

	[Header("Prefabs")]
	public CartEntry EntryPrefab;

	public Dictionary<ShopListing, int> cartDictionary = new Dictionary<ShopListing, int>();

	private List<CartEntry> cartEntries = new List<CartEntry>();

	[Header("Custom UI")]
	[SerializeField]
	private UIContentPanel cartPanel;

	[SerializeField]
	private UITrigger buyUITrigger;

	protected virtual void Update()
	{
		if (Shop.IsOpen)
		{
			UpdateEntries();
			UpdateLoadVehicleToggle();
			UpdateTotal();
			UpdateProblem();
		}
	}

	public void SetItemQuantity(ShopListing listing, int quantity)
	{
		if (!cartDictionary.ContainsKey(listing))
		{
			cartDictionary.Add(listing, 0);
		}
		cartDictionary[listing] = quantity;
		listing.SetQuantityInCart(cartDictionary[listing]);
		UpdateEntries();
	}

	public void AddItem(ShopListing listing, int quantity)
	{
		if (!cartDictionary.ContainsKey(listing))
		{
			cartDictionary.Add(listing, 0);
		}
		Console.Log("Adding " + quantity + " " + ((BaseItemDefinition)listing.Item).Name + " to cart");
		cartDictionary[listing] += quantity;
		listing.SetQuantityInCart(cartDictionary[listing]);
		UpdateEntries();
	}

	public void RemoveItem(ShopListing listing, int quantity)
	{
		cartDictionary[listing] -= quantity;
		if (cartDictionary[listing] <= 0)
		{
			cartDictionary.Remove(listing);
		}
		listing.SetQuantityInCart(cartDictionary.ContainsKey(listing) ? cartDictionary[listing] : 0);
		Shop.RemoveItemSound.Play();
		UpdateProblem();
		UpdateEntries();
		UpdateTotal();
	}

	public void ClearCart()
	{
		foreach (KeyValuePair<ShopListing, int> item in cartDictionary)
		{
			item.Key.SetQuantityInCart(0);
		}
		cartDictionary.Clear();
		UpdateEntries();
		UpdateTotal();
	}

	public int GetCartCount(ShopListing listing)
	{
		if (cartDictionary.ContainsKey(listing))
		{
			return cartDictionary[listing];
		}
		return 0;
	}

	public bool CanPlayerAffordCart()
	{
		float priceSum = GetPriceSum();
		switch (Shop.PaymentType)
		{
		case ShopInterface.EPaymentType.Cash:
			return NetworkSingleton<MoneyManager>.Instance.cashBalance >= priceSum;
		case ShopInterface.EPaymentType.Online:
			return NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= priceSum;
		case ShopInterface.EPaymentType.PreferCash:
			if (!(NetworkSingleton<MoneyManager>.Instance.cashBalance >= priceSum))
			{
				return NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= priceSum;
			}
			return true;
		case ShopInterface.EPaymentType.PreferOnline:
			if (!(NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= priceSum))
			{
				return NetworkSingleton<MoneyManager>.Instance.cashBalance >= priceSum;
			}
			return true;
		default:
			return false;
		}
	}

	public void Buy()
	{
		if (!CanCheckout(out var _) && cartDictionary.Count > 0)
		{
			return;
		}
		foreach (KeyValuePair<ShopListing, int> item in cartDictionary)
		{
			ShopListing key = item.Key;
			int value = item.Value;
			if (!key.IsUnlimitedStock)
			{
				key.RemoveStock(value);
			}
		}
		Shop.HandoverItems();
		float priceSum = GetPriceSum();
		switch (Shop.PaymentType)
		{
		case ShopInterface.EPaymentType.Cash:
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - priceSum);
			break;
		case ShopInterface.EPaymentType.Online:
			NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Purchase from " + Shop.ShopName, 0f - priceSum, 1f, string.Empty);
			break;
		case ShopInterface.EPaymentType.PreferCash:
			if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= priceSum)
			{
				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - priceSum);
			}
			else
			{
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Purchase from " + Shop.ShopName, 0f - priceSum, 1f, string.Empty);
			}
			break;
		case ShopInterface.EPaymentType.PreferOnline:
			if (NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= priceSum)
			{
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Purchase from " + Shop.ShopName, 0f - priceSum, 1f, string.Empty);
			}
			else
			{
				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - priceSum);
			}
			break;
		}
		ClearCart();
		Shop.CheckoutSound.Play();
		Shop.SetIsOpen(isOpen: false);
		if (Shop.onOrderCompleted != null)
		{
			Shop.onOrderCompleted.Invoke();
		}
		if (Shop.onOrderCompletedWithSpend != null)
		{
			Shop.onOrderCompletedWithSpend(priceSum);
		}
	}

	private void UpdateEntries()
	{
		List<ShopListing> list = cartDictionary.Keys.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			CartEntry cartEntry = GetEntry(list[i]);
			if ((Object)(object)cartEntry == (Object)null)
			{
				cartEntry = Object.Instantiate<CartEntry>(EntryPrefab, (Transform)(object)CartEntryContainer);
				cartEntry.Initialize(this, list[i], cartDictionary[list[i]]);
				cartEntries.Add(cartEntry);
				cartPanel.AddSelectable(((Component)cartEntry).GetComponent<UISelectable>());
			}
			if (cartEntry.Quantity != cartDictionary[list[i]])
			{
				cartEntry.SetQuantity(cartDictionary[list[i]]);
			}
		}
		for (int j = 0; j < cartEntries.Count; j++)
		{
			if (!cartDictionary.ContainsKey(cartEntries[j].Listing))
			{
				cartPanel.RemoveSelectable(((Component)cartEntries[j]).GetComponent<UISelectable>());
				Object.Destroy((Object)(object)((Component)cartEntries[j]).gameObject);
				cartEntries.RemoveAt(j);
				j--;
			}
		}
	}

	private void UpdateTotal()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)TotalText).text = "Total: <color=#" + ColorUtility.ToHtmlStringRGBA(Color32.op_Implicit(ListingUI.PriceLabelColor_Normal)) + ">" + MoneyManager.FormatAmount(GetPriceSum()) + "</color>";
	}

	private void UpdateProblem()
	{
		string reason;
		bool flag = CanCheckout(out reason);
		buyUITrigger.Interactable = flag && cartDictionary.Count > 0;
		if (flag)
		{
			((Behaviour)ProblemText).enabled = false;
		}
		else
		{
			((TMP_Text)ProblemText).text = reason;
			((Behaviour)ProblemText).enabled = true;
		}
		if (GetWarning(out var warning) && !((Behaviour)ProblemText).enabled)
		{
			((TMP_Text)WarningText).text = warning;
			((Behaviour)WarningText).enabled = true;
		}
		else
		{
			((Behaviour)WarningText).enabled = false;
		}
	}

	private bool CanCheckout(out string reason)
	{
		if (!Shop.WillCartFit())
		{
			if (Shop.DeliveryBays.Length != 0)
			{
				reason = "Order too large";
			}
			else
			{
				reason = "Order won't fit in inventory";
			}
			return false;
		}
		if (!CanPlayerAffordCart())
		{
			if (Shop.PaymentType == ShopInterface.EPaymentType.Cash)
			{
				reason = "Insufficient cash. Visit an ATM to withdraw cash.";
			}
			else if (Shop.PaymentType == ShopInterface.EPaymentType.Online)
			{
				reason = "Insufficient online balance. Visit an ATM to deposit cash.";
			}
			else
			{
				reason = "Insufficient funds";
			}
			return false;
		}
		reason = string.Empty;
		return true;
	}

	private bool GetWarning(out string warning)
	{
		warning = string.Empty;
		if ((Object)(object)Shop.GetLoadingBayVehicle() != (Object)null && LoadVehicleToggle.isOn)
		{
			List<ItemSlot> itemSlots = Shop.GetLoadingBayVehicle().Storage.ItemSlots;
			if (!Shop.WillCartFit(itemSlots))
			{
				warning = "Vehicle won't fit everything. Some items will be placed on the pallets.";
				return true;
			}
		}
		else
		{
			List<ItemSlot> availableSlots = PlayerSingleton<PlayerInventory>.Instance.hotbarSlots.Cast<ItemSlot>().ToList();
			if (!Shop.WillCartFit(availableSlots))
			{
				warning = "Inventory won't fit everything. Some items will be placed on the pallets.";
				return true;
			}
		}
		return false;
	}

	private void UpdateLoadVehicleToggle()
	{
		((Component)LoadVehicleToggle).gameObject.SetActive((Object)(object)Shop.GetLoadingBayVehicle() != (Object)null);
	}

	private int GetItemSum()
	{
		int num = 0;
		List<ShopListing> list = cartDictionary.Keys.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			num += cartDictionary[list[i]];
		}
		return num;
	}

	private float GetPriceSum()
	{
		float num = 0f;
		List<ShopListing> list = cartDictionary.Keys.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			num += (float)cartDictionary[list[i]] * list[i].Price;
		}
		return num;
	}

	private CartEntry GetEntry(ShopListing listing)
	{
		return cartEntries.Find((CartEntry x) => x.Listing == listing);
	}

	private bool IsMouseOverMenuArea()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return RectTransformUtility.RectangleContainsScreenPoint(((Graphic)CartArea).rectTransform, Vector2.op_Implicit(GameInput.MousePosition));
	}

	public int GetTotalSlotRequirement()
	{
		ShopListing[] array = cartDictionary.Keys.ToArray();
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			int num2 = cartDictionary[array[i]];
			num += Mathf.CeilToInt((float)num2 / (float)((BaseItemDefinition)array[i].Item).StackLimit);
		}
		return num;
	}
}
