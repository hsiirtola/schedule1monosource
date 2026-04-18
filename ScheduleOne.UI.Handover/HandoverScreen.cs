using System;
using System.Collections.Generic;
using ScheduleOne.Core;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Quests;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using ScheduleOne.Variables;
using ScheduleOne.Vision;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Handover;

public class HandoverScreen : Singleton<HandoverScreen>
{
	public enum EMode
	{
		Contract,
		Sample,
		Offer
	}

	public enum EHandoverOutcome
	{
		Cancelled,
		Finalize
	}

	private enum EItemSource
	{
		Player,
		Vehicle
	}

	public const int CUSTOMER_SLOT_COUNT = 4;

	public const float VEHICLE_MAX_DIST = 20f;

	[Header("Settings")]
	public Gradient SuccessColorMap;

	[Header("References")]
	public Canvas Canvas;

	public GameObject Container;

	public UIScreen UIScreen;

	public UIScreen AltScreen;

	public CanvasGroup CanvasGroup;

	public TextMeshProUGUI DescriptionLabel;

	public TextMeshProUGUI CustomerSubtitle;

	public TextMeshProUGUI FavouriteDrugLabel;

	public TextMeshProUGUI FavouritePropertiesLabel;

	public TextMeshProUGUI[] PropertiesEntries;

	public RectTransform[] ExpectationEntries;

	public GameObject NoVehicle;

	public RectTransform VehicleSlotContainer;

	public RectTransform CustomerSlotContainer;

	public TextMeshProUGUI VehicleSubtitle;

	public TextMeshProUGUI SuccessLabel;

	public TextMeshProUGUI ErrorLabel;

	public TextMeshProUGUI WarningLabel;

	public Button DoneButton;

	public RectTransform VehicleContainer;

	public TextMeshProUGUI TitleLabel;

	public HandoverScreenPriceSelector PriceSelector;

	public TextMeshProUGUI FairPriceLabel;

	public Animation TutorialAnimation;

	public RectTransform TutorialContainer;

	public HandoverScreenDetailPanel DetailPanel;

	public Action<EHandoverOutcome, List<ItemInstance>, float> onHandoverComplete;

	public Func<List<ItemInstance>, float, float> SuccessChanceMethod;

	private ItemSlotUI[] VehicleSlotUIs;

	private ItemSlotUI[] CustomerSlotUIs;

	private ItemSlot[] CustomerSlots = new ItemSlot[4];

	private Dictionary<ItemInstance, EItemSource> OriginalItemLocations = new Dictionary<ItemInstance, EItemSource>();

	private bool ignoreCustomerChangedEvents;

	private bool requireFullChanceOfSuccess;

	private bool activeScreenChangedThisFrame;

	public Contract CurrentContract { get; protected set; }

	public bool IsOpen { get; protected set; }

	public bool TutorialOpen { get; private set; }

	public EMode Mode { get; protected set; }

	public Customer CurrentCustomer { get; private set; }

	protected override void Start()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		base.Start();
		GameInput.RegisterExitListener(Exit, 8);
		VehicleSlotUIs = ((Component)VehicleSlotContainer).GetComponentsInChildren<ItemSlotUI>();
		CustomerSlotUIs = ((Component)CustomerSlotContainer).GetComponentsInChildren<ItemSlotUI>();
		((UnityEvent)DoneButton.onClick).AddListener(new UnityAction(DonePressed));
		for (int i = 0; i < CustomerSlots.Length; i++)
		{
			CustomerSlots[i] = new ItemSlot();
			CustomerSlotUIs[i].AssignSlot(CustomerSlots[i]);
			ItemSlot obj = CustomerSlots[i];
			obj.onItemDataChanged = (Action)Delegate.Combine(obj.onItemDataChanged, new Action(CustomerItemsChanged));
		}
		((TMP_Text)VehicleSubtitle).text = "This is the vehicle you last drove.\nMust be within " + 20f + " meters.";
		ClearCustomerSlots(returnToOriginals: false);
		((Component)PriceSelector).gameObject.SetActive(false);
		PriceSelector.onPriceChanged.AddListener(new UnityAction(UpdateSuccessChance));
		((Behaviour)Canvas).enabled = false;
		Container.gameObject.SetActive(false);
		IsOpen = false;
		Singleton<ItemUIManager>.Instance.AddRaycaster(((Component)Canvas).GetComponent<GraphicRaycaster>());
	}

	private void Update()
	{
		if (IsOpen)
		{
			if ((Player.Local.CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None && Player.Local.CrimeData.TimeSinceSighted < 5f) || Player.Local.CrimeData.CurrentArrestProgress > 0.01f)
			{
				Close(EHandoverOutcome.Cancelled);
			}
			activeScreenChangedThisFrame = false;
		}
	}

	private void OpenTutorial()
	{
		CanvasGroup.alpha = 0f;
		TutorialOpen = true;
		((Component)TutorialContainer).gameObject.SetActive(true);
		TutorialAnimation.Play();
	}

	public void CloseTutorial()
	{
		CanvasGroup.alpha = 1f;
		TutorialOpen = false;
		((Component)TutorialContainer).gameObject.SetActive(false);
	}

	[Button]
	public void TestOpen()
	{
		Customer customer = Object.FindObjectOfType<Customer>();
		if ((Object)(object)customer == (Object)null)
		{
			Console.LogWarning("No customer found in scene for handover screen test");
			return;
		}
		DeliveryLocation deliveryLocation = Object.FindObjectOfType<DeliveryLocation>();
		if ((Object)(object)deliveryLocation == (Object)null)
		{
			Console.LogWarning("No delivery location found in scene for handover screen test");
			return;
		}
		QuestEntryData questEntryData = new QuestEntryData(string.Empty, EQuestState.Inactive);
		Contract contract = NetworkSingleton<QuestManager>.Instance.CreateContract_Local("Test", "Test", new QuestEntryData[1] { questEntryData }, GUIDManager.GenerateUniqueGUID().ToString(), tracked: false, customer, 100f, new ProductList(), deliveryLocation.GUID.ToString(), new QuestWindowConfig(), expires: false, default(GameDateTime), 0, default(GameDateTime));
		Open(contract, customer, EMode.Contract, null, null);
	}

	public virtual void Open(Contract contract, Customer customer, EMode mode, Action<EHandoverOutcome, List<ItemInstance>, float> callback, Func<List<ItemInstance>, float, float> successChanceMethod, bool _requireFullChanceOfSuccess = false)
	{
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		if (mode == EMode.Contract && (Object)(object)contract == (Object)null)
		{
			Console.LogWarning("Contract is null");
			return;
		}
		CurrentContract = contract;
		CurrentCustomer = customer;
		requireFullChanceOfSuccess = _requireFullChanceOfSuccess;
		Mode = mode;
		if (Mode == EMode.Contract)
		{
			((TMP_Text)TitleLabel).text = "Complete Deal";
		}
		else if (Mode == EMode.Sample)
		{
			((TMP_Text)TitleLabel).text = "Give Free Sample";
		}
		else if (Mode == EMode.Offer)
		{
			((TMP_Text)TitleLabel).text = "Offer Deal";
		}
		DetailPanel.Open(customer);
		onHandoverComplete = callback;
		SuccessChanceMethod = successChanceMethod;
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
		Singleton<UIScreenManager>.Instance.AddScreen(UIScreen);
		if (PlayerSingleton<PlayerInventory>.InstanceExists)
		{
			PlayerSingleton<PlayerInventory>.Instance.AttachToScreen(UIScreen);
		}
		Singleton<CompassManager>.Instance.SetVisible(visible: false);
		List<ItemSlot> allInventorySlots = PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots();
		List<ItemSlot> secondarySlots = new List<ItemSlot>(CustomerSlots);
		if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("ItemAmountSelectionTutorialDone") && GameManager.IS_TUTORIAL)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("ItemAmountSelectionTutorialDone", true.ToString());
			OpenTutorial();
		}
		else
		{
			Player.Local.VisualState.ApplyState("drugdeal", EVisualState.DrugDealing);
		}
		Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
		if (Mode == EMode.Contract)
		{
			((TMP_Text)DescriptionLabel).text = customer.NPC.FirstName + " is paying <color=#50E65A>" + MoneyManager.FormatAmount(contract.Payment) + "</color> for:";
			((Behaviour)DescriptionLabel).enabled = true;
		}
		else
		{
			((Behaviour)DescriptionLabel).enabled = false;
		}
		if (Mode == EMode.Sample)
		{
			EDrugType property = customer.GetOrderedDrugTypes()[0];
			string text = ColorUtility.ToHtmlStringRGB(property.GetColor());
			((TMP_Text)FavouriteDrugLabel).text = customer.NPC.FirstName + "'s favourite drug: <color=#" + text + ">" + property.ToString() + "</color>";
			((Behaviour)FavouriteDrugLabel).enabled = true;
			((TMP_Text)FavouritePropertiesLabel).text = customer.NPC.FirstName + "'s favourite effects:";
			for (int i = 0; i < PropertiesEntries.Length; i++)
			{
				if (customer.CustomerData.PreferredProperties.Count > i)
				{
					((TMP_Text)PropertiesEntries[i]).text = "•  " + customer.CustomerData.PreferredProperties[i].Name;
					((Graphic)PropertiesEntries[i]).color = customer.CustomerData.PreferredProperties[i].LabelColor;
					((Behaviour)PropertiesEntries[i]).enabled = true;
				}
				else
				{
					((Behaviour)PropertiesEntries[i]).enabled = false;
				}
			}
			((Component)FavouritePropertiesLabel).gameObject.SetActive(true);
		}
		else
		{
			((Behaviour)FavouriteDrugLabel).enabled = false;
			((Component)FavouritePropertiesLabel).gameObject.SetActive(false);
		}
		for (int j = 0; j < ExpectationEntries.Length; j++)
		{
			if ((Object)(object)contract != (Object)null && contract.ProductList.entries.Count > j)
			{
				((TMP_Text)((Component)((Transform)ExpectationEntries[j]).Find("Container/Quantity")).gameObject.GetComponent<TextMeshProUGUI>()).text = contract.ProductList.entries[j].Quantity + "x";
				((Graphic)((Component)((Transform)ExpectationEntries[j]).Find("Container/Star")).GetComponent<Image>()).color = ItemQuality.GetColor(contract.ProductList.entries[j].Quality);
				((TMP_Text)((Component)((Transform)ExpectationEntries[j]).Find("Container/Name")).gameObject.GetComponent<TextMeshProUGUI>()).text = ((BaseItemDefinition)Registry.GetItem(contract.ProductList.entries[j].ProductID)).Name;
				((Component)ExpectationEntries[j]).gameObject.SetActive(true);
			}
			else
			{
				((Component)ExpectationEntries[j]).gameObject.SetActive(false);
			}
		}
		if ((Object)(object)Player.Local.LastDrivenVehicle != (Object)null && (Object)(object)Player.Local.LastDrivenVehicle.Storage != (Object)null && Vector3.Distance(((Component)Player.Local.LastDrivenVehicle).transform.position, ((Component)Player.Local).transform.position) < 20f)
		{
			if ((Object)(object)Player.Local.LastDrivenVehicle.Storage != (Object)null)
			{
				for (int k = 0; k < VehicleSlotUIs.Length; k++)
				{
					ItemSlot itemSlot = null;
					if (k < Player.Local.LastDrivenVehicle.Storage.ItemSlots.Count)
					{
						itemSlot = Player.Local.LastDrivenVehicle.Storage.ItemSlots[k];
					}
					if (itemSlot != null)
					{
						VehicleSlotUIs[k].AssignSlot(itemSlot);
						((Component)VehicleSlotUIs[k]).gameObject.SetActive(true);
						allInventorySlots.Add(itemSlot);
					}
					else
					{
						((Component)VehicleSlotUIs[k]).gameObject.SetActive(false);
					}
				}
			}
			NoVehicle.gameObject.SetActive(false);
			((Component)VehicleContainer).gameObject.SetActive(true);
		}
		else
		{
			NoVehicle.gameObject.SetActive(true);
			((Component)VehicleContainer).gameObject.SetActive(false);
		}
		if (Mode == EMode.Contract)
		{
			((TMP_Text)CustomerSubtitle).text = "Place the expected products here";
		}
		else if (Mode == EMode.Sample)
		{
			((TMP_Text)CustomerSubtitle).text = "Place a product here for " + customer.NPC.FirstName + " to try";
		}
		else if (Mode == EMode.Offer)
		{
			((TMP_Text)CustomerSubtitle).text = "Place product here";
		}
		if (mode == EMode.Offer)
		{
			((Component)PriceSelector).gameObject.SetActive(true);
			PriceSelector.SetPrice(1f);
		}
		else
		{
			((Component)PriceSelector).gameObject.SetActive(false);
		}
		RecordOriginalLocations();
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: true);
		Singleton<ItemUIManager>.Instance.EnableQuickMove(allInventorySlots, secondarySlots);
		CustomerItemsChanged();
		((Behaviour)Canvas).enabled = true;
		Container.gameObject.SetActive(true);
		IsOpen = true;
	}

	public void SwapActiveScreen()
	{
		if (!activeScreenChangedThisFrame)
		{
			if ((Object)(object)Singleton<UIScreenManager>.Instance.TopScreen == (Object)(object)UIScreen)
			{
				Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
				Singleton<UIScreenManager>.Instance.AddScreen(AltScreen);
			}
			else if ((Object)(object)Singleton<UIScreenManager>.Instance.TopScreen == (Object)(object)AltScreen)
			{
				Singleton<UIScreenManager>.Instance.RemoveScreen(AltScreen);
				Singleton<UIScreenManager>.Instance.AddScreen(UIScreen);
			}
			activeScreenChangedThisFrame = true;
		}
	}

	public virtual void Close(EHandoverOutcome outcome)
	{
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		List<ItemInstance> list = new List<ItemInstance>();
		if (outcome == EHandoverOutcome.Finalize)
		{
			for (int i = 0; i < CustomerSlots.Length; i++)
			{
				if (CustomerSlots[i].ItemInstance != null)
				{
					list.Add(CustomerSlots[i].ItemInstance);
				}
			}
		}
		Singleton<CompassManager>.Instance.SetVisible(visible: true);
		CurrentContract = null;
		CurrentCustomer = null;
		IsOpen = false;
		((Behaviour)Canvas).enabled = false;
		Container.gameObject.SetActive(false);
		float arg = 0f;
		if (Mode == EMode.Offer)
		{
			PriceSelector.RefreshPrice();
			arg = PriceSelector.Price;
		}
		if (onHandoverComplete != null)
		{
			onHandoverComplete(outcome, list, arg);
		}
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
		if (PlayerSingleton<PlayerInventory>.InstanceExists)
		{
			PlayerSingleton<PlayerInventory>.Instance.DetachFromScreen();
		}
		Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
		Singleton<UIScreenManager>.Instance.RemoveScreen(AltScreen);
		Player.Local.VisualState.RemoveState("drugdeal");
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		if (outcome == EHandoverOutcome.Cancelled)
		{
			ClearCustomerSlots(returnToOriginals: true);
		}
	}

	public void DonePressed()
	{
		if (((Behaviour)DoneButton).isActiveAndEnabled && ((Selectable)DoneButton).IsInteractable())
		{
			Close(EHandoverOutcome.Finalize);
		}
	}

	private void RecordOriginalLocations()
	{
		foreach (HotbarSlot hotbarSlot in PlayerSingleton<PlayerInventory>.Instance.hotbarSlots)
		{
			if (hotbarSlot.ItemInstance != null)
			{
				if (OriginalItemLocations.ContainsKey(hotbarSlot.ItemInstance))
				{
					Console.LogWarning("Item already exists in original locations");
				}
				else
				{
					OriginalItemLocations.Add(hotbarSlot.ItemInstance, EItemSource.Player);
				}
			}
		}
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			if (TutorialOpen)
			{
				CloseTutorial();
			}
			else
			{
				Close(EHandoverOutcome.Cancelled);
			}
		}
	}

	public void ClearCustomerSlots(bool returnToOriginals)
	{
		ignoreCustomerChangedEvents = true;
		ItemSlot[] customerSlots = CustomerSlots;
		foreach (ItemSlot itemSlot in customerSlots)
		{
			if (itemSlot.ItemInstance != null)
			{
				if (returnToOriginals)
				{
					PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(itemSlot.ItemInstance);
				}
				itemSlot.ClearStoredInstance();
			}
		}
		OriginalItemLocations.Clear();
		ignoreCustomerChangedEvents = false;
		CustomerItemsChanged();
	}

	private void CustomerItemsChanged()
	{
		if (!ignoreCustomerChangedEvents)
		{
			UpdateDoneButton();
			UpdateSuccessChance();
			if (Mode == EMode.Offer)
			{
				float customerItemsValue = GetCustomerItemsValue();
				PriceSelector.SetPrice(customerItemsValue);
				((TMP_Text)FairPriceLabel).text = "Fair price: " + MoneyManager.FormatAmount(customerItemsValue);
			}
		}
	}

	private void UpdateDoneButton()
	{
		if (GetError(out var err))
		{
			((Selectable)DoneButton).interactable = false;
			((TMP_Text)ErrorLabel).text = err;
			((Behaviour)ErrorLabel).enabled = true;
		}
		else
		{
			((Selectable)DoneButton).interactable = true;
			((Behaviour)ErrorLabel).enabled = false;
		}
		if (!((Behaviour)ErrorLabel).enabled && GetWarning(out var warning))
		{
			((TMP_Text)WarningLabel).text = warning;
			((Behaviour)WarningLabel).enabled = true;
		}
		else
		{
			((Behaviour)WarningLabel).enabled = false;
		}
	}

	private void UpdateSuccessChance()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		if (GetCustomerItems(onlyPackagedProduct: false).Count == 0)
		{
			((Behaviour)SuccessLabel).enabled = false;
			return;
		}
		float num = 0f;
		if (Mode == EMode.Sample)
		{
			num = SuccessChanceMethod?.Invoke(GetCustomerItems(), 0f) ?? 0f;
			((TMP_Text)SuccessLabel).text = Mathf.RoundToInt(num * 100f) + "% chance of success";
			((Graphic)SuccessLabel).color = SuccessColorMap.Evaluate(num);
			((Behaviour)SuccessLabel).enabled = true;
		}
		else if (Mode == EMode.Contract)
		{
			if ((Object)(object)CurrentContract == (Object)null)
			{
				Console.LogWarning("Current contract is null");
				return;
			}
			num = Mathf.Clamp(CurrentContract.GetProductListMatch(GetCustomerItems(), out var _), 0.01f, 1f);
			if (num < 1f && !requireFullChanceOfSuccess)
			{
				((TMP_Text)SuccessLabel).text = Mathf.RoundToInt(num * 100f) + "% chance of customer accepting";
				((Graphic)SuccessLabel).color = SuccessColorMap.Evaluate(num);
				((Behaviour)SuccessLabel).enabled = true;
			}
			else
			{
				((Behaviour)SuccessLabel).enabled = false;
			}
		}
		else if (Mode == EMode.Offer)
		{
			float price = PriceSelector.Price;
			num = SuccessChanceMethod?.Invoke(GetCustomerItems(), price) ?? 0f;
			((TMP_Text)SuccessLabel).text = Mathf.RoundToInt(num * 100f) + "% chance of success";
			((Graphic)SuccessLabel).color = SuccessColorMap.Evaluate(num);
			((Behaviour)SuccessLabel).enabled = true;
		}
	}

	private bool GetError(out string err)
	{
		err = string.Empty;
		if (requireFullChanceOfSuccess && Mode == EMode.Contract && (Object)(object)CurrentContract != (Object)null && CurrentContract.GetProductListMatch(GetCustomerItems(), out var _) < 1f)
		{
			err = "Customer expectations not met";
			return true;
		}
		if (Mode == EMode.Contract && (Object)(object)CurrentContract != (Object)null)
		{
			if (GetCustomerItemsCount(onlyPackagedProduct: false) == 0)
			{
				err = string.Empty;
				return true;
			}
			if (NetworkSingleton<GameManager>.Instance.IsTutorial && GetCustomerItemsCount() > CurrentContract.ProductList.GetTotalQuantity())
			{
				err = "You are providing more product than required.";
				return true;
			}
		}
		if ((Mode == EMode.Sample || Mode == EMode.Offer) && GetCustomerItemsCount() == 0)
		{
			bool flag = false;
			for (int i = 0; i < CustomerSlots.Length; i++)
			{
				if (CustomerSlots[i].ItemInstance != null && CustomerSlots[i].ItemInstance is ProductItemInstance && (Object)(object)(CustomerSlots[i].ItemInstance as ProductItemInstance).AppliedPackaging == (Object)null)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				err = "Product must be packaged";
			}
			return true;
		}
		return false;
	}

	private bool GetWarning(out string warning)
	{
		warning = string.Empty;
		if (Mode == EMode.Contract)
		{
			if ((Object)(object)CurrentContract != (Object)null)
			{
				if (CurrentContract.GetProductListMatch(GetCustomerItems(), out var matchedProductCount) < 1f)
				{
					warning = "Customer expectations not met";
					return true;
				}
				if (matchedProductCount > CurrentContract.ProductList.GetTotalQuantity())
				{
					warning = "You are providing more items than required.";
					return true;
				}
			}
		}
		else if (Mode == EMode.Sample && GetCustomerItemsCount(onlyPackagedProduct: false) > 1)
		{
			warning = "Only 1 sample product is required.";
			return true;
		}
		bool flag = false;
		for (int i = 0; i < CustomerSlots.Length; i++)
		{
			if (CustomerSlots[i].ItemInstance != null && CustomerSlots[i].ItemInstance is ProductItemInstance && (Object)(object)(CustomerSlots[i].ItemInstance as ProductItemInstance).AppliedPackaging == (Object)null)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			warning = "Product must be packaged";
			return true;
		}
		return false;
	}

	private List<ItemInstance> GetCustomerItems(bool onlyPackagedProduct = true)
	{
		List<ItemInstance> list = new List<ItemInstance>();
		for (int i = 0; i < CustomerSlots.Length; i++)
		{
			if (CustomerSlots[i].ItemInstance != null && (!onlyPackagedProduct || (CustomerSlots[i].ItemInstance is ProductItemInstance productItemInstance && !((Object)(object)productItemInstance.AppliedPackaging == (Object)null))))
			{
				list.Add(CustomerSlots[i].ItemInstance);
			}
		}
		return list;
	}

	private float GetCustomerItemsValue()
	{
		float num = 0f;
		foreach (ItemInstance customerItem in GetCustomerItems())
		{
			if (customerItem is ProductItemInstance)
			{
				ProductItemInstance productItemInstance = customerItem as ProductItemInstance;
				num += (productItemInstance.Definition as ProductDefinition).MarketValue * (float)((BaseItemInstance)productItemInstance).Quantity * (float)productItemInstance.Amount;
			}
		}
		return num;
	}

	private int GetCustomerItemsCount(bool onlyPackagedProduct = true)
	{
		int num = 0;
		for (int i = 0; i < CustomerSlots.Length; i++)
		{
			if (CustomerSlots[i].ItemInstance == null)
			{
				continue;
			}
			ProductItemInstance productItemInstance = CustomerSlots[i].ItemInstance as ProductItemInstance;
			if (!onlyPackagedProduct || (productItemInstance != null && !((Object)(object)productItemInstance.AppliedPackaging == (Object)null)))
			{
				int num2 = 1;
				if (productItemInstance != null)
				{
					num2 = productItemInstance.Amount;
				}
				num += ((BaseItemInstance)CustomerSlots[i].ItemInstance).Quantity * num2;
			}
		}
		return num;
	}
}
