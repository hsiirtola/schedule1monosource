using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.Product;
using ScheduleOne.UI.Tooltips;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Messages;

public class DealerManagementApp : App<DealerManagementApp>
{
	private class InventoryItem
	{
		public string ID;

		public int Quantity;

		public int Quality;

		public InventoryItem(string id, int quantity, int quality)
		{
			ID = id;
			Quantity = quantity;
			Quality = quality;
		}
	}

	[Header("References")]
	public Text NoDealersLabel;

	public RectTransform Content;

	public CustomerSelector CustomerSelector;

	[Header("Selector")]
	public Image SelectorImage;

	public Text SelectorTitle;

	public Button BackButton;

	public Button NextButton;

	[SerializeField]
	private DropdownUI _dropdown;

	[SerializeField]
	private Image _dropdownBackground;

	[SerializeField]
	private Image _dropdownCaptionImage;

	[SerializeField]
	private Text _dropDownCaptionText;

	[Header("Basic Info")]
	public Text CashLabel;

	public Text CutLabel;

	public Text HomeLabel;

	[Header("Inventory")]
	[SerializeField]
	private Text _inventoryTextLabel;

	[SerializeField]
	private RectTransform _inventoryEntryContainer;

	public RectTransform[] InventoryEntries;

	[Header("Customers")]
	public Text CustomerTitleLabel;

	public RectTransform[] CustomerEntries;

	public Button AssignCustomerButton;

	[Header("Fonts")]
	[SerializeField]
	private SpriteFont _uiGeneralSpriteFont;

	[SerializeField]
	private ColorFont _productColorFont;

	private List<Dealer> dealers = new List<Dealer>();

	private bool _isOpen;

	public Dealer SelectedDealer { get; private set; }

	protected override void Awake()
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		base.Awake();
		foreach (Dealer allPlayerDealer in Dealer.AllPlayerDealers)
		{
			if (allPlayerDealer.IsRecruited)
			{
				AddDealer(allPlayerDealer);
			}
		}
		Dealer.onDealerRecruited = (Action<Dealer>)Delegate.Combine(Dealer.onDealerRecruited, new Action<Dealer>(AddDealer));
		Phone phone = PlayerSingleton<Phone>.Instance;
		phone.onPhoneOpened = (Action)Delegate.Combine(phone.onPhoneOpened, new Action(Refresh));
		((UnityEvent)BackButton.onClick).AddListener(new UnityAction(BackPressed));
		((UnityEvent)NextButton.onClick).AddListener(new UnityAction(NextPressed));
		((UnityEvent)AssignCustomerButton.onClick).AddListener(new UnityAction(AssignCustomer));
		((UnityEvent<int>)(object)((Dropdown)_dropdown).onValueChanged).AddListener((UnityAction<int>)OnDropdownValueChanged);
		_dropdown.OnOpen += OnDropdownOpen;
		RefreshDropdown();
	}

	protected override void Start()
	{
		base.Start();
		CustomerSelector.onCustomerSelected.AddListener((UnityAction<Customer>)AddCustomer);
	}

	protected override void OnDestroy()
	{
		Dealer.onDealerRecruited = (Action<Dealer>)Delegate.Remove(Dealer.onDealerRecruited, new Action<Dealer>(AddDealer));
		base.OnDestroy();
	}

	public void Refresh()
	{
		if (_isOpen)
		{
			if ((Object)(object)SelectedDealer != (Object)null)
			{
				SetDisplayedDealer(SelectedDealer);
				return;
			}
			if (dealers.Count > 0)
			{
				SetDisplayedDealer(dealers[0]);
				return;
			}
			((Component)NoDealersLabel).gameObject.SetActive(true);
			((Component)Content).gameObject.SetActive(false);
		}
	}

	public override void SetOpen(bool open)
	{
		_isOpen = open;
		if ((Object)(object)SelectedDealer != (Object)null)
		{
			SetDisplayedDealer(SelectedDealer);
		}
		else if (dealers.Count > 0)
		{
			SetDisplayedDealer(dealers[0]);
		}
		else
		{
			((Component)NoDealersLabel).gameObject.SetActive(true);
			((Component)Content).gameObject.SetActive(false);
		}
		base.SetOpen(open);
	}

	public void SetDisplayedDealer(Dealer dealer)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0519: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_057e: Expected O, but got Unknown
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)dealer == (Object)null)
		{
			Console.LogError("Cannot set displayed dealer to null!");
			return;
		}
		ColorFont generalColorFont = PlayerSingleton<Phone>.Instance.GeneralColorFont;
		Color val = (((Object)(object)generalColorFont != (Object)null) ? generalColorFont.GetColour("FadedText") : Color.gray);
		SelectedDealer = dealer;
		_dropdownCaptionImage.sprite = dealer.MugshotSprite;
		_dropDownCaptionText.text = dealer.fullName + $"<color=#{ColorUtility.ToHtmlStringRGBA(val)}> ({dealer.Region})</color>";
		CashLabel.text = MoneyManager.FormatAmount(dealer.Cash);
		CutLabel.text = Mathf.RoundToInt(dealer.Cut * 100f) + "%";
		HomeLabel.text = dealer.HomeName;
		List<InventoryItem> list = new List<InventoryItem>();
		List<string> list2 = new List<string>();
		bool flag = dealer.GetTotalInventoryItemCount() > 0;
		((Component)_inventoryEntryContainer).gameObject.SetActive(flag);
		((Component)_inventoryTextLabel).gameObject.SetActive(!flag);
		if (flag)
		{
			foreach (ItemSlot slot in dealer.GetAllSlots())
			{
				if (slot.Quantity != 0)
				{
					int num = slot.Quantity;
					int quality = -1;
					if (slot.ItemInstance is ProductItemInstance productItemInstance)
					{
						num *= productItemInstance.Amount;
						quality = (int)productItemInstance.Quality;
					}
					InventoryItem inventoryItem = list.Find((InventoryItem i) => i.ID == ((BaseItemInstance)slot.ItemInstance).ID && i.Quality == quality);
					if (inventoryItem != null)
					{
						inventoryItem.Quantity += num;
						continue;
					}
					list2.Add(((BaseItemInstance)slot.ItemInstance).ID);
					list.Add(new InventoryItem(((BaseItemInstance)slot.ItemInstance).ID, num, quality));
				}
			}
			for (int num2 = 0; num2 < InventoryEntries.Length; num2++)
			{
				if (list2.Count > num2)
				{
					ItemDefinition item = Registry.GetItem(list2[num2]);
					if (!((Object)(object)item == (Object)null))
					{
						List<Image> list3 = ((Component)InventoryEntries[num2]).GetComponentsInChildren<Image>(true).ToList();
						Image val2 = list3.Find((Image img) => ((Object)((Component)img).gameObject).name == "Icon");
						Image val3 = list3.Find((Image img) => ((Object)((Component)img).gameObject).name == "Quality");
						val2.sprite = ((BaseItemDefinition)item).Icon;
						bool flag2 = list[num2].Quality >= 0;
						((Component)val3).gameObject.SetActive(flag2);
						if (flag2)
						{
							EQuality quality2 = (EQuality)list[num2].Quality;
							((Graphic)val3).color = _productColorFont.GetColour(quality2.ToString());
						}
						((Component)InventoryEntries[num2]).GetComponentInChildren<Text>().text = list[num2].Quantity + "x " + ((BaseItemDefinition)item).Name;
						((Component)InventoryEntries[num2]).gameObject.SetActive(true);
					}
				}
				else
				{
					((Component)InventoryEntries[num2]).gameObject.SetActive(false);
				}
			}
		}
		CustomerTitleLabel.text = "Assigned Customers (" + dealer.AssignedCustomers.Count + "/" + 10 + ")";
		for (int num3 = 0; num3 < CustomerEntries.Length; num3++)
		{
			if (dealer.AssignedCustomers.Count > num3)
			{
				Customer customer = dealer.AssignedCustomers[num3];
				Color color = ItemQuality.GetColor(customer.CustomerData.Standards.GetCorrespondingQuality());
				((Component)((Transform)CustomerEntries[num3]).Find("Mugshot")).GetComponent<Image>().sprite = customer.NPC.MugshotSprite;
				((Component)((Transform)CustomerEntries[num3]).Find("Mugshot")).GetComponent<Tooltip>().text = "This customer has <color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + customer.CustomerData.Standards.GetName() + "</color> standards";
				((Graphic)((Component)((Transform)CustomerEntries[num3]).Find("Mugshot/ExpectedQuality")).GetComponent<Image>()).color = color;
				((Component)((Transform)CustomerEntries[num3]).Find("Name")).GetComponent<Text>().text = customer.NPC.fullName + $"<color=#{ColorUtility.ToHtmlStringRGBA(val)}> ({customer.NPC.Region})</color>";
				Button component = ((Component)((Transform)CustomerEntries[num3]).Find("Remove")).GetComponent<Button>();
				((UnityEventBase)component.onClick).RemoveAllListeners();
				((UnityEvent)component.onClick).AddListener((UnityAction)delegate
				{
					RemoveCustomer(customer);
				});
				((Component)CustomerEntries[num3]).gameObject.SetActive(true);
			}
			else
			{
				((Component)CustomerEntries[num3]).gameObject.SetActive(false);
			}
		}
		((Selectable)BackButton).interactable = dealers.IndexOf(dealer) > 0;
		((Selectable)NextButton).interactable = dealers.IndexOf(dealer) < dealers.Count - 1;
		((Component)AssignCustomerButton).gameObject.SetActive(dealer.AssignedCustomers.Count < 10);
		((Component)NoDealersLabel).gameObject.SetActive(false);
		((Component)Content).gameObject.SetActive(true);
	}

	private void AddDealer(Dealer dealer)
	{
		if (!dealers.Contains(dealer))
		{
			dealers.Add(dealer);
			dealers = dealers.OrderBy((Dealer d) => d.Region).ToList();
			RefreshDropdown();
		}
	}

	private void AddCustomer(Customer customer)
	{
		SelectedDealer.AddCustomer_Server(customer.NPC.ID);
		if (customer.OfferedContractInfo != null)
		{
			Console.Log("Expiring...");
			customer.ExpireOffer();
		}
		SetDisplayedDealer(SelectedDealer);
	}

	private void RemoveCustomer(Customer customer)
	{
		SelectedDealer.SendRemoveCustomer(customer.NPC.ID);
		SetDisplayedDealer(SelectedDealer);
	}

	private void BackPressed()
	{
		int num = dealers.IndexOf(SelectedDealer);
		if (num > 0)
		{
			SetDisplayedDealer(dealers[num - 1]);
		}
	}

	private void NextPressed()
	{
		int num = dealers.IndexOf(SelectedDealer);
		if (num < dealers.Count - 1)
		{
			SetDisplayedDealer(dealers[num + 1]);
		}
	}

	public void AssignCustomer()
	{
		CustomerSelector.Open();
	}

	private void RefreshDropdown()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		ColorFont generalColorFont = PlayerSingleton<Phone>.Instance.GeneralColorFont;
		Color val = (((Object)(object)generalColorFont != (Object)null) ? generalColorFont.GetColour("FadedText") : Color.gray);
		((Dropdown)_dropdown).ClearOptions();
		List<OptionData> list = new List<OptionData>();
		foreach (Dealer dealer in dealers)
		{
			string text = dealer.fullName + $"<color=#{ColorUtility.ToHtmlStringRGBA(val)}> ({dealer.Region})</color>";
			list.Add(new OptionData(text, dealer.MugshotSprite));
		}
		((Dropdown)_dropdown).AddOptions(list);
	}

	private void OnDropdownValueChanged(int value)
	{
		Sprite sprite = _uiGeneralSpriteFont.GetSprite("RectangleRoundedEdges");
		_dropdownBackground.sprite = sprite;
		SelectedDealer = dealers[value];
		SetDisplayedDealer(SelectedDealer);
	}

	private void OnDropdownOpen()
	{
		Sprite sprite = _uiGeneralSpriteFont.GetSprite("RectangleRoundedEdgesBottom");
		_dropdownBackground.sprite = sprite;
	}
}
