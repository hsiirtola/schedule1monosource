using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using ScheduleOne.Money;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCustomization;

public class CharacterCustomizationOption : MonoBehaviour
{
	public string Name = "Option";

	public string Label = "AssetPath or Label";

	public float Price;

	public bool RequireLevel;

	public FullRank RequiredLevel = new FullRank(ERank.Street_Rat, 1);

	[Header("References")]
	public TextMeshProUGUI NameLabel;

	public TextMeshProUGUI PriceLabel;

	public TextMeshProUGUI LevelLabel;

	public RectTransform LockDisplay;

	public Button MainButton;

	public Button BuyButton;

	public RectTransform SelectionIndicator;

	[Header("Events")]
	public UnityEvent onSelect;

	public UnityEvent onDeselect;

	public UnityEvent onPurchase;

	private bool selected;

	public bool purchased { get; private set; }

	private bool purchaseable
	{
		get
		{
			if (RequireLevel)
			{
				return RequiredLevel <= NetworkSingleton<LevelManager>.Instance.GetFullRank();
			}
			return true;
		}
	}

	private void Awake()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		((TMP_Text)NameLabel).text = Name;
		if (Price > 0f)
		{
			((TMP_Text)PriceLabel).text = MoneyManager.FormatAmount(Price);
		}
		else
		{
			((TMP_Text)PriceLabel).text = "Free";
		}
		UpdatePriceColor();
		((TMP_Text)LevelLabel).text = RequiredLevel.ToString();
		((UnityEvent)MainButton.onClick).AddListener(new UnityAction(Selected));
		((UnityEvent)BuyButton.onClick).AddListener(new UnityAction(Purchased));
	}

	private void OnValidate()
	{
		((Object)((Component)this).gameObject).name = Name;
	}

	private void FixedUpdate()
	{
		((Selectable)BuyButton).interactable = NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= Price;
	}

	private void Start()
	{
		UpdateUI();
	}

	private void Selected()
	{
		SetSelected(_selected: true);
	}

	private void Purchased()
	{
		if (purchaseable)
		{
			if (onPurchase != null)
			{
				onPurchase.Invoke();
			}
			if (Price > 0f)
			{
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Character customization", 0f - Price, 1f, string.Empty);
			}
			SetPurchased(_purchased: true);
		}
	}

	private void UpdatePriceColor()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (Price > 0f)
		{
			if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= Price)
			{
				Color val = default(Color);
				((Graphic)PriceLabel).color = (ColorUtility.TryParseHtmlString("#4CBFFF", ref val) ? val : Color.white);
			}
			else
			{
				((Graphic)PriceLabel).color = Color32.op_Implicit(new Color32((byte)200, (byte)75, (byte)70, byte.MaxValue));
			}
		}
		else
		{
			Color val2 = default(Color);
			((Graphic)PriceLabel).color = (ColorUtility.TryParseHtmlString("#4CBFFF", ref val2) ? val2 : Color.white);
		}
	}

	public void SetSelected(bool _selected)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		selected = _selected;
		((Component)SelectionIndicator).gameObject.SetActive(selected);
		((TMP_Text)NameLabel).rectTransform.offsetMin = new Vector2(selected ? 30f : 10f, ((TMP_Text)NameLabel).rectTransform.offsetMin.y);
		UpdateUI();
		if (selected)
		{
			if (onSelect != null)
			{
				onSelect.Invoke();
			}
		}
		else if (onDeselect != null)
		{
			onDeselect.Invoke();
		}
	}

	public void SetPurchased(bool _purchased)
	{
		purchased = _purchased;
		((Component)BuyButton).gameObject.SetActive(!purchased);
		((Component)PriceLabel).gameObject.SetActive(!purchased);
		if (_purchased)
		{
			SetSelected(_selected: true);
		}
		UpdateUI();
	}

	private void UpdateUI()
	{
		((Component)LockDisplay).gameObject.SetActive(!purchaseable);
		((Component)PriceLabel).gameObject.SetActive(purchaseable && !purchased);
		((Component)BuyButton).gameObject.SetActive(purchaseable && !purchased);
		UpdatePriceColor();
	}

	public void ParentCategoryClosed()
	{
		if (selected && !purchased)
		{
			SetSelected(_selected: false);
		}
		else if (purchased && !selected)
		{
			SetSelected(_selected: true);
		}
	}

	public void SiblingOptionSelected(CharacterCustomizationOption option)
	{
		if ((Object)(object)option != (Object)(object)this && selected)
		{
			SetSelected(_selected: false);
		}
	}

	public void SiblingOptionPurchased(CharacterCustomizationOption option)
	{
		if ((Object)(object)option != (Object)(object)this && purchased)
		{
			SetPurchased(_purchased: false);
		}
	}
}
