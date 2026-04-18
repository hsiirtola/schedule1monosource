using System;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop;

public class CartEntry : MonoBehaviour
{
	[Header("References")]
	public TextMeshProUGUI NameLabel;

	public TextMeshProUGUI PriceLabel;

	public Button IncrementButton;

	public Button DecrementButton;

	public Button RemoveButton;

	public UITrigger ModifyButton;

	public int Quantity { get; protected set; }

	public Cart Cart { get; protected set; }

	public ShopListing Listing { get; protected set; }

	public void Initialize(Cart cart, ShopListing listing, int quantity)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		Cart = cart;
		Listing = listing;
		Quantity = quantity;
		((UnityEvent)IncrementButton.onClick).AddListener((UnityAction)delegate
		{
			ChangeAmount(1);
		});
		((UnityEvent)DecrementButton.onClick).AddListener((UnityAction)delegate
		{
			ChangeAmount(-1);
		});
		((UnityEvent)RemoveButton.onClick).AddListener((UnityAction)delegate
		{
			ChangeAmount(-999);
		});
		if ((Object)(object)ModifyButton != (Object)null)
		{
			ModifyButton.OnTrigger.AddListener((UnityAction)delegate
			{
				SetAmount(1);
			});
		}
		UpdateTitle();
		UpdatePrice();
	}

	public void SetQuantity(int quantity)
	{
		Quantity = quantity;
		UpdateTitle();
		UpdatePrice();
	}

	protected virtual void UpdateTitle()
	{
		((TMP_Text)NameLabel).text = Quantity + "x " + ((BaseItemDefinition)Listing.Item).Name;
	}

	private void UpdatePrice()
	{
		((TMP_Text)PriceLabel).text = MoneyManager.FormatAmount((float)Quantity * Listing.Price);
	}

	private void ChangeAmount(int change)
	{
		if (change > 0)
		{
			Cart.AddItem(Listing, change);
		}
		else if (change < 0)
		{
			Cart.RemoveItem(Listing, -change);
		}
	}

	private void SetAmount(int amount)
	{
		Singleton<UIScreenManager>.Instance.OpenPopupScreen("ModifyAmountMenu", "Cart", "Modify Amount", "", (float)Quantity, (Action<float>)delegate(float num)
		{
			SetItemQuantity((int)num);
		}, null, UIPopupScreen_ModifyAmountMenu.ModifyAmountMenuMode.Store, (float)Cart.Shop.minModifyAmount, (float)Cart.Shop.modifyTier1Amount, (float)Cart.Shop.modifyTier2Amount, (float)Cart.Shop.modifyTier3Amount, 3, Listing);
	}

	private void SetItemQuantity(int amount)
	{
		Cart.SetItemQuantity(Listing, amount);
	}
}
