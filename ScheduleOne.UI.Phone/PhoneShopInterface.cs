using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.Messaging;
using ScheduleOne.Money;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone;

public class PhoneShopInterface : MonoBehaviour
{
	[Serializable]
	public class Listing
	{
		public StorableItemDefinition Item;

		public float Price => Item.BasePurchasePrice;

		public Listing(StorableItemDefinition item)
		{
			Item = item;
		}
	}

	[Serializable]
	public class CartEntry
	{
		public Listing Listing;

		public int Quantity;

		public CartEntry(Listing listing, int quantity)
		{
			Listing = listing;
			Quantity = quantity;
		}
	}

	public RectTransform EntryPrefab;

	public Color ValidAmountColor;

	public Color InvalidAmountColor;

	[Header("References")]
	public GameObject Container;

	public Text TitleLabel;

	public Text SubtitleLabel;

	public RectTransform EntryContainer;

	public Text OrderTotalLabel;

	public Text OrderLimitLabel;

	public Text DebtLabel;

	public Button ConfirmButton;

	public GameObject ItemLimitContainer;

	public Text ItemLimitLabel;

	[Header("Custom UI")]
	public UIScreen uiScreen;

	public UIPanel uiPanel;

	private List<RectTransform> _entries = new List<RectTransform>();

	private List<Listing> _items = new List<Listing>();

	private List<CartEntry> _cart = new List<CartEntry>();

	private float orderLimit;

	private Action<List<CartEntry>, float> orderConfirmedCallback;

	private MSGConversation conversation;

	public bool IsOpen { get; private set; }

	private void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		GameInput.RegisterExitListener(Exit, 4);
		((UnityEvent)ConfirmButton.onClick).AddListener(new UnityAction(ConfirmOrderPressed));
		ItemLimitContainer.gameObject.SetActive(true);
		Close();
	}

	public void Open(string title, string subtitle, MSGConversation _conversation, List<Listing> listings, float _orderLimit, float debt, Action<List<CartEntry>, float> _orderConfirmedCallback)
	{
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Expected O, but got Unknown
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Expected O, but got Unknown
		IsOpen = true;
		TitleLabel.text = title;
		SubtitleLabel.text = subtitle;
		OrderLimitLabel.text = MoneyManager.FormatAmount(_orderLimit);
		DebtLabel.text = MoneyManager.FormatAmount(debt);
		orderLimit = _orderLimit;
		conversation = _conversation;
		MSGConversation mSGConversation = conversation;
		mSGConversation.onMessageRendered = (Action)Delegate.Combine(mSGConversation.onMessageRendered, new Action(Close));
		orderConfirmedCallback = _orderConfirmedCallback;
		_items.Clear();
		_items.AddRange(listings);
		uiPanel.ClearAllSelectables();
		foreach (Listing entry in listings)
		{
			RectTransform val = Object.Instantiate<RectTransform>(EntryPrefab, (Transform)(object)EntryContainer);
			uiPanel.AddSelectable(((Component)val).GetComponent<UISelectable>());
			((Component)((Transform)val).Find("Icon")).GetComponent<Image>().sprite = ((BaseItemDefinition)entry.Item).Icon;
			((Component)((Transform)val).Find("Name")).GetComponent<Text>().text = ((BaseItemDefinition)entry.Item).Name;
			((Component)((Transform)val).Find("Price")).GetComponent<Text>().text = MoneyManager.FormatAmount(entry.Price);
			((Component)((Transform)val).Find("Quantity")).GetComponent<Text>().text = "0";
			StorableItemDefinition item = entry.Item;
			if (!item.RequiresLevelToPurchase || NetworkSingleton<LevelManager>.Instance.GetFullRank() >= item.RequiredRank)
			{
				((UnityEvent)((Component)((Transform)val).Find("Quantity/Remove")).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
				{
					ChangeListingQuantity(entry, -1);
				});
				((UnityEvent)((Component)((Transform)val).Find("Quantity/Add")).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
				{
					ChangeListingQuantity(entry, 1);
				});
				((Component)((Transform)val).Find("Locked")).gameObject.SetActive(false);
			}
			else
			{
				((Component)((Transform)val).Find("Locked/Title")).GetComponent<Text>().text = "Unlocks at " + item.RequiredRank.ToString();
				((Component)((Transform)val).Find("Locked")).gameObject.SetActive(true);
			}
			_entries.Add(val);
		}
		CartChanged();
		Container.gameObject.SetActive(true);
		Singleton<UIScreenManager>.Instance.AddScreen(uiScreen);
		((MonoBehaviour)this).StartCoroutine(DelaySelectPanel());
	}

	private IEnumerator DelaySelectPanel()
	{
		yield return null;
		uiScreen.SetCurrentSelectedPanel(uiPanel);
		uiPanel.SelectSelectable(returnFirstFound: true);
	}

	public void Close()
	{
		IsOpen = false;
		_items.Clear();
		_cart.Clear();
		if (conversation != null)
		{
			MSGConversation mSGConversation = conversation;
			mSGConversation.onMessageRendered = (Action)Delegate.Remove(mSGConversation.onMessageRendered, new Action(Close));
		}
		foreach (RectTransform entry in _entries)
		{
			Object.Destroy((Object)(object)((Component)entry).gameObject);
		}
		_entries.Clear();
		Container.gameObject.SetActive(false);
		Singleton<UIScreenManager>.Instance.RemoveScreen(uiScreen);
	}

	public void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen && PlayerSingleton<Phone>.Instance.IsOpen)
		{
			action.Used = true;
			Close();
		}
	}

	private void ChangeListingQuantity(Listing listing, int change)
	{
		CartEntry cartEntry = _cart.Find((CartEntry e) => ((BaseItemDefinition)e.Listing.Item).ID == ((BaseItemDefinition)listing.Item).ID);
		if (cartEntry == null)
		{
			cartEntry = new CartEntry(listing, 0);
			_cart.Add(cartEntry);
		}
		cartEntry.Quantity = Mathf.Clamp(cartEntry.Quantity + change, 0, 99);
		((Component)((Transform)_entries[_items.IndexOf(listing)]).Find("Quantity")).GetComponent<Text>().text = cartEntry.Quantity.ToString();
		CartChanged();
	}

	private void CartChanged()
	{
		UpdateOrderTotal();
		((Selectable)ConfirmButton).interactable = CanConfirmOrder();
	}

	private void ConfirmOrderPressed()
	{
		orderConfirmedCallback(_cart, GetOrderTotal(out var _));
		Close();
	}

	private bool CanConfirmOrder()
	{
		int itemCount;
		float orderTotal = GetOrderTotal(out itemCount);
		if (orderTotal > 0f && orderTotal <= orderLimit)
		{
			return itemCount <= 10;
		}
		return false;
	}

	private void UpdateOrderTotal()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		int itemCount;
		float orderTotal = GetOrderTotal(out itemCount);
		OrderTotalLabel.text = MoneyManager.FormatAmount(orderTotal);
		((Graphic)OrderTotalLabel).color = ((orderTotal <= orderLimit) ? ValidAmountColor : InvalidAmountColor);
		ItemLimitLabel.text = itemCount + "/" + 10;
		((Graphic)ItemLimitLabel).color = ((itemCount <= 10) ? Color.black : InvalidAmountColor);
	}

	private float GetOrderTotal(out int itemCount)
	{
		float num = 0f;
		itemCount = 0;
		foreach (CartEntry item in _cart)
		{
			num += item.Listing.Price * (float)item.Quantity;
			itemCount += item.Quantity;
		}
		return num;
	}
}
