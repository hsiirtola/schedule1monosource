using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Money;
using ScheduleOne.UI.Shop;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Delivery;

public class ListingEntry : MonoBehaviour
{
	[Header("References")]
	public Image Icon;

	public Text ItemNameLabel;

	public Text ItemPriceLabel;

	public InputField QuantityInput;

	public Button IncrementButton;

	public Button DecrementButton;

	public RectTransform LockedContainer;

	public UnityEvent onQuantityChanged;

	public ShopListing MatchingListing { get; private set; }

	public int SelectedQuantity { get; private set; }

	public void Initialize(ShopListing match)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		MatchingListing = match;
		Icon.sprite = ((BaseItemDefinition)MatchingListing.Item).Icon;
		ItemNameLabel.text = ((BaseItemDefinition)MatchingListing.Item).Name;
		ItemPriceLabel.text = MoneyManager.FormatAmount(MatchingListing.Price);
		((UnityEvent<string>)(object)QuantityInput.onSubmit).AddListener((UnityAction<string>)OnQuantityInputSubmitted);
		((UnityEvent<string>)(object)QuantityInput.onEndEdit).AddListener((UnityAction<string>)delegate
		{
			ValidateInput();
		});
		((UnityEvent)IncrementButton.onClick).AddListener((UnityAction)delegate
		{
			ChangeQuantity(1);
		});
		((UnityEvent)DecrementButton.onClick).AddListener((UnityAction)delegate
		{
			ChangeQuantity(-1);
		});
		QuantityInput.SetTextWithoutNotify(SelectedQuantity.ToString());
		RefreshLocked();
	}

	public void RefreshLocked()
	{
		if (MatchingListing.Item.IsUnlocked)
		{
			((Component)LockedContainer).gameObject.SetActive(false);
		}
		else
		{
			((Component)LockedContainer).gameObject.SetActive(true);
		}
	}

	public void SetQuantity(int quant, bool notify = true)
	{
		if (!MatchingListing.Item.IsUnlocked)
		{
			quant = 0;
		}
		SelectedQuantity = Mathf.Clamp(quant, 0, 999);
		QuantityInput.SetTextWithoutNotify(SelectedQuantity.ToString());
		if (notify && onQuantityChanged != null)
		{
			onQuantityChanged.Invoke();
		}
	}

	private void ChangeQuantity(int change)
	{
		SetQuantity(SelectedQuantity + change);
	}

	private void OnQuantityInputSubmitted(string value)
	{
		if (int.TryParse(value, out var result))
		{
			SetQuantity(result);
		}
		else
		{
			SetQuantity(0);
		}
	}

	private void ValidateInput()
	{
		OnQuantityInputSubmitted(QuantityInput.text);
	}
}
