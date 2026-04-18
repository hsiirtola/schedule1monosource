using System;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Delivery;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.Property;
using ScheduleOne.UI.Items;
using ScheduleOne.UI.Shop;
using ScheduleOne.UI.Tooltips;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Delivery;

public class DeliveryReceiptDisplay : MonoBehaviour
{
	[Header("Prefabs")]
	[SerializeField]
	private ItemEntryUI ItemEntryPrefab;

	[Header("References")]
	[SerializeField]
	private Text _DestinationLabel;

	[SerializeField]
	private Text _loadingDockLabel;

	[SerializeField]
	private Text _shopLabel;

	[SerializeField]
	private Text _shopDescriptionLabel;

	[SerializeField]
	private RectTransform _ItemEntryContainer;

	[SerializeField]
	private Button _ReorderButton;

	[SerializeField]
	private Tooltip _ReorderTooltip;

	[SerializeField]
	private Text _reorderPriceLabel;

	[Header("Settings")]
	[SerializeField]
	private int _maxItemsShown = 8;

	[Header("Fonts")]
	[SerializeField]
	private ColorFont _generalColorFont;

	[SerializeField]
	private ColorFont _shopTextColorFont;

	private DeliveryReceipt _receipt;

	private ItemEntryUI[] _itemEntries;

	private Action<DeliveryReceipt> _onSelect;

	public Button ReorderButton => _ReorderButton;

	public DeliveryReceipt Receipt => _receipt;

	public void Initialise()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)_ReorderButton.onClick).AddListener((UnityAction)delegate
		{
			_onSelect?.Invoke(_receipt);
		});
		_itemEntries = new ItemEntryUI[_maxItemsShown];
		for (int num = 0; num < _maxItemsShown; num++)
		{
			ItemEntryUI itemEntryUI = Object.Instantiate<ItemEntryUI>(ItemEntryPrefab, (Transform)(object)_ItemEntryContainer);
			((Component)itemEntryUI).gameObject.SetActive(false);
			_itemEntries[num] = itemEntryUI;
		}
	}

	public void Set(DeliveryReceipt receipt, float deliveryCost, bool canAfford)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		_receipt = receipt;
		ShopInterface matchingShop = PlayerSingleton<DeliveryApp>.Instance.GetShop(receipt.StoreName).MatchingShop;
		ScheduleOne.Property.Property property = Singleton<PropertyManager>.Instance.GetProperty(receipt.DestinationCode);
		_DestinationLabel.text = property.PropertyName;
		_shopLabel.text = receipt.StoreName;
		((Graphic)_shopLabel).color = _shopTextColorFont.GetColour(receipt.StoreName);
		_loadingDockLabel.text = "Loading Dock " + (receipt.LoadingDockIndex + 1);
		_shopDescriptionLabel.text = matchingShop.ShopDescription;
		_reorderPriceLabel.text = MoneyManager.FormatAmount(deliveryCost);
		((Graphic)_reorderPriceLabel).color = (canAfford ? _generalColorFont.GetColour("Cash") : _generalColorFont.GetColour("Debt"));
		int num = receipt.Items.Length;
		for (int i = 0; i < _maxItemsShown; i++)
		{
			bool flag = i < num;
			((Component)_itemEntries[i]).gameObject.SetActive(flag);
			if (flag)
			{
				StringIntPair stringIntPair = receipt.Items[i];
				ItemDefinition item = Registry.GetItem(stringIntPair.String);
				if (i == _maxItemsShown - 1 && num > _maxItemsShown)
				{
					_itemEntries[i].SetLabelOnly("+" + (num - _maxItemsShown - 1) + " more...");
					break;
				}
				_itemEntries[i].Set(((BaseItemDefinition)item).Name, stringIntPair.Int, ((BaseItemDefinition)item).Icon);
			}
		}
	}

	public void SetTooltip(string tooltip)
	{
		_ReorderTooltip.text = tooltip;
	}

	public void SetActiveTooltip(bool active)
	{
		((Behaviour)_ReorderTooltip).enabled = active;
	}

	public void SubscribeToOnSelect(Action<DeliveryReceipt> callback)
	{
		_onSelect = (Action<DeliveryReceipt>)Delegate.Combine(_onSelect, callback);
	}

	public void UnsubscribeFromOnSelect(Action<DeliveryReceipt> callback)
	{
		_onSelect = (Action<DeliveryReceipt>)Delegate.Remove(_onSelect, callback);
	}
}
