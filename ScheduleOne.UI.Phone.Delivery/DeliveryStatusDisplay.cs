using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Delivery;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.UI.Items;
using ScheduleOne.UI.Shop;
using ScheduleOne.UI.Tooltips;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Delivery;

public class DeliveryStatusDisplay : MonoBehaviour
{
	[Header("Prefabs")]
	[SerializeField]
	private ItemEntryUI ItemEntryPrefab;

	[Header("References")]
	public Text DestinationLabel;

	[SerializeField]
	private Text _loadingDockLabel;

	public Text ShopLabel;

	[SerializeField]
	private Text _shopDescriptionLabel;

	public Image StatusImage;

	public Text StatusLabel;

	public Tooltip StatusTooltip;

	public RectTransform ItemEntryContainer;

	public Animation FlashAnimation;

	public GameObject FlashObject;

	[Header("Settings")]
	[SerializeField]
	private int _maxItemsShown = 8;

	public Color StatusColor_Transit;

	public Color StatusColor_Waiting;

	public Color StatusColor_Arrived;

	[Header("Fonts")]
	[SerializeField]
	private ColorFont _shopTextColorFont;

	public DeliveryInstance DeliveryInstance { get; private set; }

	public void AssignDelivery(DeliveryInstance instance)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		DeliveryInstance = instance;
		ShopInterface matchingShop = PlayerSingleton<DeliveryApp>.Instance.GetShop(DeliveryInstance.StoreName).MatchingShop;
		DestinationLabel.text = DeliveryInstance.Destination.PropertyName;
		ShopLabel.text = DeliveryInstance.StoreName;
		((Graphic)ShopLabel).color = _shopTextColorFont.GetColour(DeliveryInstance.StoreName);
		_loadingDockLabel.text = "Loading Dock " + (DeliveryInstance.LoadingDockIndex + 1);
		_shopDescriptionLabel.text = matchingShop.ShopDescription;
		int num = DeliveryInstance.Items.Length;
		for (int i = 0; i < num; i++)
		{
			StringIntPair stringIntPair = DeliveryInstance.Items[i];
			ItemEntryUI itemEntryUI = Object.Instantiate<ItemEntryUI>(ItemEntryPrefab, (Transform)(object)ItemEntryContainer);
			ItemDefinition item = Registry.GetItem(stringIntPair.String);
			if (i == _maxItemsShown - 1 && num > _maxItemsShown)
			{
				itemEntryUI.SetLabelOnly("+" + (num - _maxItemsShown - 1) + " more...");
				break;
			}
			itemEntryUI.Set(((BaseItemDefinition)item).Name, stringIntPair.Int, ((BaseItemDefinition)item).Icon);
		}
		RefreshStatus();
	}

	public void RefreshStatus()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		if (DeliveryInstance.Status == EDeliveryStatus.InTransit)
		{
			((Graphic)StatusImage).color = StatusColor_Transit;
			int timeUntilArrival = DeliveryInstance.TimeUntilArrival;
			int num = timeUntilArrival / 60;
			int num2 = timeUntilArrival % 60;
			StatusLabel.text = num + "h " + num2 + "m";
			StatusTooltip.text = "This delivery is currently in transit.";
		}
		else if (DeliveryInstance.Status == EDeliveryStatus.Waiting)
		{
			((Graphic)StatusImage).color = StatusColor_Waiting;
			StatusLabel.text = "Waiting";
			StatusTooltip.text = "This delivery is waiting for loading dock " + (DeliveryInstance.LoadingDockIndex + 1) + " to be empty.";
		}
		else if (DeliveryInstance.Status == EDeliveryStatus.Arrived)
		{
			((Graphic)StatusImage).color = StatusColor_Arrived;
			StatusLabel.text = "Arrived";
			StatusTooltip.text = "This delivery has arrived and is ready to be unloaded.";
		}
	}

	public void Flash()
	{
		FlashAnimation.Play();
	}

	private void OnDisable()
	{
		FlashAnimation.Stop();
		FlashObject.SetActive(false);
	}
}
