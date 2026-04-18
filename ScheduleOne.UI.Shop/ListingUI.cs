using System;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Money;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop;

public class ListingUI : MonoBehaviour
{
	public static Color32 PriceLabelColor_Normal = new Color32((byte)90, (byte)185, (byte)90, byte.MaxValue);

	public static Color32 PriceLabelColor_NoStock = new Color32((byte)165, (byte)70, (byte)60, byte.MaxValue);

	[Header("Colors")]
	public Color32 StockLabelDefault = new Color32((byte)40, (byte)40, (byte)40, byte.MaxValue);

	public Color32 StockLabelNone = new Color32((byte)185, (byte)55, (byte)55, byte.MaxValue);

	[Header("References")]
	public Image Icon;

	public TextMeshProUGUI NameLabel;

	public TextMeshProUGUI PriceLabel;

	public TextMeshProUGUI StockLabel;

	public GameObject LockedContainer;

	public Button BuyButton;

	public Button DropdownButton;

	public EventTrigger Trigger;

	public RectTransform DetailPanelAnchor;

	public RectTransform DropdownAnchor;

	public RectTransform TopDropdownAnchor;

	public Action hoverStart;

	public Action hoverEnd;

	public Action onClicked;

	public Action onDropdownClicked;

	public ShopListing Listing { get; protected set; }

	public virtual void Initialize(ShopListing listing)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected O, but got Unknown
		Listing = listing;
		Icon.sprite = ((BaseItemDefinition)listing.Item).Icon;
		((Graphic)Icon).color = (listing.UseIconTint ? listing.IconTint : Color.white);
		((TMP_Text)NameLabel).text = ((BaseItemDefinition)listing.Item).Name;
		UpdatePrice();
		UpdateStock();
		Entry val = new Entry();
		val.eventID = (EventTriggerType)0;
		((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			HoverStart();
		});
		Trigger.triggers.Add(val);
		Entry val2 = new Entry();
		val2.eventID = (EventTriggerType)1;
		((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			HoverEnd();
		});
		Trigger.triggers.Add(val2);
		Entry val3 = new Entry();
		val3.eventID = (EventTriggerType)9;
		((UnityEvent<BaseEventData>)(object)val3.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			HoverStart();
		});
		Trigger.triggers.Add(val3);
		Entry val4 = new Entry();
		val4.eventID = (EventTriggerType)10;
		((UnityEvent<BaseEventData>)(object)val4.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			HoverEnd();
		});
		Trigger.triggers.Add(val4);
		listing.onStockChanged = (Action)Delegate.Combine(listing.onStockChanged, new Action(StockChanged));
		((UnityEvent)BuyButton.onClick).AddListener(new UnityAction(Clicked));
		((UnityEvent)DropdownButton.onClick).AddListener(new UnityAction(DropdownClicked));
		UpdateLockStatus();
	}

	public virtual RectTransform GetIconCopy(RectTransform parent)
	{
		return Object.Instantiate<GameObject>(((Component)Icon).gameObject, (Transform)(object)parent).GetComponent<RectTransform>();
	}

	public void Update()
	{
		UpdateButtons();
	}

	private void Clicked()
	{
		if (onClicked != null)
		{
			onClicked();
		}
	}

	private void DropdownClicked()
	{
		if (onDropdownClicked != null)
		{
			onDropdownClicked();
		}
	}

	private void HoverStart()
	{
		if (hoverStart != null)
		{
			hoverStart();
		}
	}

	private void HoverEnd()
	{
		if (hoverEnd != null)
		{
			hoverEnd();
		}
	}

	private void StockChanged()
	{
		UpdateButtons();
		UpdatePrice();
		UpdateStock();
	}

	private void UpdatePrice()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)PriceLabel).text = MoneyManager.FormatAmount(Listing.Price);
		((Graphic)PriceLabel).color = Color32.op_Implicit(PriceLabelColor_Normal);
	}

	private void UpdateStock()
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)StockLabel == (Object)null)
		{
			return;
		}
		if (Listing.IsUnlimitedStock)
		{
			((Behaviour)StockLabel).enabled = false;
			return;
		}
		int currentStockMinusCart = Listing.CurrentStockMinusCart;
		if (Listing.TieStockToNumberVariable)
		{
			((TMP_Text)StockLabel).text = currentStockMinusCart.ToString();
		}
		else
		{
			((TMP_Text)StockLabel).text = currentStockMinusCart + " / " + Listing.DefaultStock;
		}
		if (currentStockMinusCart > 0)
		{
			((Graphic)StockLabel).color = Color32.op_Implicit(StockLabelDefault);
		}
		else
		{
			((TMP_Text)StockLabel).text = "Out of stock";
			((Graphic)StockLabel).color = Color32.op_Implicit(StockLabelNone);
		}
		if (currentStockMinusCart == 1 && Listing.RestockRate == ShopListing.ERestockRate.Never)
		{
			((TMP_Text)StockLabel).text = "1 of 1";
		}
		((Behaviour)StockLabel).enabled = true;
	}

	private void UpdateButtons()
	{
		bool interactable = CanAddToCart();
		((Selectable)BuyButton).interactable = interactable;
		((Selectable)DropdownButton).interactable = interactable;
	}

	public bool CanAddToCart()
	{
		if (Listing.IsUnlimitedStock || Listing.CurrentStockMinusCart > 0)
		{
			return Listing.Item.IsUnlocked;
		}
		return false;
	}

	public void UpdateLockStatus()
	{
		LockedContainer.gameObject.SetActive(!Listing.Item.IsUnlocked);
	}
}
