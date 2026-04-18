using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core.Equipping.Framework;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using ScheduleOne.UI;
using ScheduleOne.UI.Items;
using ScheduleOne.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.PlayerScripts;

public class PlayerInventory : PlayerSingleton<PlayerInventory>, IFirstPersonReferencesProvider
{
	[Serializable]
	public class ItemVariable
	{
		public ItemDefinition Definition;

		public string VariableName;
	}

	[Serializable]
	private class ItemAmount
	{
		public ItemDefinition Definition;

		public int Amount = 10;
	}

	public const float LABEL_DISPLAY_TIME = 2f;

	public const float LABEL_FADE_TIME = 0.5f;

	public const float DISCARD_TIME = 1.25f;

	public const int INVENTORY_SLOT_COUNT = 8;

	[Header("Startup Items (Editor only)")]
	[SerializeField]
	private bool giveStartupItems;

	[SerializeField]
	private List<ItemAmount> startupItems = new List<ItemAmount>();

	[Header("References")]
	public Transform equipContainer;

	public List<HotbarSlot> hotbarSlots = new List<HotbarSlot>();

	private ClipboardSlot clipboardSlot;

	private List<ItemSlotUI> slotUIs = new List<ItemSlotUI>();

	private ItemSlot discardSlot;

	[Header("Item Variables")]
	public List<ItemVariable> ItemVariables = new List<ItemVariable>();

	private int _equippedSlotIndex = -1;

	public Action<bool> onInventoryStateChanged;

	public Action<int> onEquippedSlotChanged;

	private int PriorEquippedSlotIndex = -1;

	private int PreviousEquippedSlotIndex = -1;

	public UnityEvent onPreItemEquipped;

	public UnityEvent onItemEquipped;

	private bool ManagementSlotEnabled;

	public float currentEquipTime;

	protected float currentDiscardTime;

	protected UIScreen attachedScreen;

	protected UIPanel uiPanel;

	protected UIPanel originalSelectedPanel;

	public int TOTAL_SLOT_COUNT => 9 + (ManagementSlotEnabled ? 1 : 0);

	public Transform EquipContainer => equipContainer;

	public CashSlot cashSlot { get; private set; }

	public CashInstance cashInstance { get; protected set; }

	public int EquippedSlotIndex
	{
		get
		{
			return _equippedSlotIndex;
		}
		set
		{
			_equippedSlotIndex = value;
			Player.Local.SetEquippedSlotIndex(value);
		}
	}

	public bool HotbarEnabled { get; protected set; } = true;

	public bool EquippingEnabled { get; protected set; } = true;

	public bool HolsterEnabled { get; set; } = true;

	public Equippable equippable { get; protected set; }

	public HotbarSlot equippedSlot
	{
		get
		{
			if (EquippedSlotIndex == -1)
			{
				return null;
			}
			return IndexAllSlots(EquippedSlotIndex);
		}
	}

	public ItemInstance EquippedItem
	{
		get
		{
			if (equippedSlot != null)
			{
				return equippedSlot.ItemInstance;
			}
			return null;
		}
	}

	public bool isAnythingEquipped => EquippedItem != null;

	public HotbarSlot IndexAllSlots(int index)
	{
		if (index < 0)
		{
			return null;
		}
		if (ManagementSlotEnabled)
		{
			if (index < hotbarSlots.Count)
			{
				return hotbarSlots[index];
			}
			return index switch
			{
				8 => clipboardSlot, 
				9 => cashSlot, 
				_ => null, 
			};
		}
		if (index < hotbarSlots.Count)
		{
			return hotbarSlots[index];
		}
		if (index == 8)
		{
			return cashSlot;
		}
		return null;
	}

	protected override void Awake()
	{
		base.Awake();
		uiPanel = ((Component)Singleton<HUD>.Instance.HotbarContainer).GetComponent<UIPanel>();
		if (!Object.op_Implicit((Object)(object)uiPanel))
		{
			uiPanel = ((Component)Singleton<HUD>.Instance.HotbarContainer).gameObject.AddComponent<UIPanel>();
		}
		SetupInventoryUI();
	}

	private void SetupInventoryUI()
	{
		for (int i = 0; i < 8; i++)
		{
			HotbarSlot hotbarSlot = new HotbarSlot();
			hotbarSlots.Add(hotbarSlot);
			ItemSlotUI component = ((Component)Object.Instantiate<ItemSlotUI>(Singleton<ItemUIManager>.Instance.HotbarSlotUIPrefab, (Transform)(object)Singleton<HUD>.Instance.SlotContainer)).GetComponent<ItemSlotUI>();
			component.AssignSlot(hotbarSlot);
			slotUIs.Add(component);
			uiPanel.AddSelectable(((Component)component).GetComponent<UISelectable>());
		}
		clipboardSlot = new ClipboardSlot();
		clipboardSlot.SetStoredItem(Registry.GetItem("managementclipboard").GetDefaultInstance());
		clipboardSlot.AddFilter(new ItemFilter_ID(new List<string> { "managementclipboard" }));
		clipboardSlot.SetIsRemovalLocked(locked: true);
		clipboardSlot.SetIsAddLocked(locked: true);
		Singleton<HUD>.Instance.managementSlotUI.AssignSlot(clipboardSlot);
		((Component)Singleton<HUD>.Instance.managementSlotContainer).gameObject.SetActive(false);
		slotUIs.Add(Singleton<HUD>.Instance.managementSlotUI);
		uiPanel.AddSelectable(((Component)Singleton<HUD>.Instance.managementSlotUI).GetComponent<UISelectable>());
		cashSlot = new CashSlot();
		cashSlot.SetStoredItem(Registry.GetItem("cash").GetDefaultInstance());
		cashInstance = cashSlot.ItemInstance as CashInstance;
		cashSlot.AddFilter(new ItemFilter_Category(new List<EItemCategory> { (EItemCategory)6 }));
		((Component)Singleton<HUD>.Instance.cashSlotUI).GetComponent<CashSlotUI>().AssignSlot(cashSlot);
		ItemSlotUI component2 = ((Component)Singleton<HUD>.Instance.cashSlotUI).GetComponent<ItemSlotUI>();
		slotUIs.Add(component2);
		uiPanel.AddSelectable(((Component)component2).GetComponent<UISelectable>());
		discardSlot = new ItemSlot();
		ItemSlotUI itemSlotUI = Singleton<HUD>.Instance.discardSlot;
		itemSlotUI.AssignSlot(discardSlot);
		uiPanel.AddSelectable(((Component)itemSlotUI).GetComponent<UISelectable>());
		RepositionUI();
	}

	private void RepositionUI()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 20f;
		for (int i = 0; i < 8; i++)
		{
			ItemSlotUI itemSlotUI = slotUIs[i];
			((TMP_Text)((Component)((Transform)itemSlotUI.Rect).Find("Background/Index")).GetComponent<TextMeshProUGUI>()).text = ((i + 1) % 10).ToString();
			itemSlotUI.Rect.anchoredPosition = new Vector2(num + itemSlotUI.Rect.sizeDelta.x / 2f + num2, 0f);
			num += itemSlotUI.Rect.sizeDelta.x + num2;
			if (i == 7)
			{
				((Component)((Transform)itemSlotUI.Rect).Find("Seperator")).gameObject.SetActive(true);
				((Component)((Transform)itemSlotUI.Rect).Find("Seperator")).GetComponent<RectTransform>().anchoredPosition = new Vector2(num2, 0f);
				num += num2;
			}
		}
		int num3 = 8;
		if (ManagementSlotEnabled)
		{
			((Component)((Component)Singleton<HUD>.Instance.managementSlotUI).transform.Find("Background/Index")).GetComponent<Text>().text = ((num3 + 1) % 10).ToString();
			Singleton<HUD>.Instance.managementSlotContainer.anchoredPosition = new Vector2(num + Singleton<HUD>.Instance.managementSlotContainer.sizeDelta.x / 2f + num2, 0f);
			num += Singleton<HUD>.Instance.managementSlotContainer.sizeDelta.x + num2;
			num3++;
		}
		((Component)Singleton<HUD>.Instance.managementSlotContainer).gameObject.SetActive(ManagementSlotEnabled);
		((TMP_Text)((Component)((Transform)Singleton<HUD>.Instance.cashSlotUI).Find("Background/Index")).GetComponent<TextMeshProUGUI>()).text = ((num3 + 1) % 10).ToString();
		Singleton<HUD>.Instance.cashSlotContainer.anchoredPosition = new Vector2(num + Singleton<HUD>.Instance.cashSlotContainer.sizeDelta.x / 2f + num2, 0f);
		num += Singleton<HUD>.Instance.cashSlotContainer.sizeDelta.x + num2;
		Singleton<HUD>.Instance.SlotContainer.anchoredPosition = new Vector2((0f - num) / 2f, Singleton<HUD>.Instance.SlotContainer.anchoredPosition.y);
	}

	protected override void Start()
	{
		base.Start();
		for (int i = 0; i < hotbarSlots.Count; i++)
		{
			HotbarSlot slot = hotbarSlots[i];
			Player.Local.SetInventoryItem(i, slot.ItemInstance);
			int index = i;
			HotbarSlot hotbarSlot = slot;
			hotbarSlot.onItemDataChanged = (Action)Delegate.Combine(hotbarSlot.onItemDataChanged, (Action)delegate
			{
				UpdateInventoryVariables();
				Player.Local.SetInventoryItem(index, slot.ItemInstance);
			});
		}
		Player.Local.SetInventoryItem(8, cashSlot.ItemInstance);
		CashSlot obj = cashSlot;
		obj.onItemDataChanged = (Action)Delegate.Combine(obj.onItemDataChanged, (Action)delegate
		{
			UpdateInventoryVariables();
			Player.Local.SetInventoryItem(8, cashSlot.ItemInstance);
		});
		if (giveStartupItems)
		{
			GiveStartupItems();
		}
		if (!GameManager.IS_TUTORIAL)
		{
			BoolVariable boolVariable = NetworkSingleton<VariableDatabase>.Instance.GetVariable("ClipboardAcquired") as BoolVariable;
			if (boolVariable.Value)
			{
				ClipboardAcquiredVarChange(newVal: true);
			}
			else
			{
				boolVariable.OnValueChanged.AddListener((UnityAction<bool>)ClipboardAcquiredVarChange);
			}
		}
	}

	private void GiveStartupItems()
	{
		if (!Application.isEditor && !Debug.isDebugBuild)
		{
			return;
		}
		foreach (ItemAmount startupItem in startupItems)
		{
			AddItemToInventory(startupItem.Definition.GetDefaultInstance(startupItem.Amount));
		}
	}

	protected virtual void Update()
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		UpdateHotbarSelection();
		if (isAnythingEquipped && HotbarEnabled)
		{
			currentEquipTime += Time.deltaTime;
		}
		else
		{
			currentEquipTime = 0f;
		}
		if (isAnythingEquipped)
		{
			((TMP_Text)Singleton<HUD>.Instance.selectedItemLabel).text = ((BaseItemInstance)equippedSlot.ItemInstance).Name;
			if (currentEquipTime > 2f)
			{
				float num = Mathf.Clamp01((currentEquipTime - 2f) / 0.5f);
				((Graphic)Singleton<HUD>.Instance.selectedItemLabel).color = new Color(((Graphic)Singleton<HUD>.Instance.selectedItemLabel).color.r, ((Graphic)Singleton<HUD>.Instance.selectedItemLabel).color.g, ((Graphic)Singleton<HUD>.Instance.selectedItemLabel).color.b, 1f - num);
			}
			else
			{
				((Graphic)Singleton<HUD>.Instance.selectedItemLabel).color = new Color(((Graphic)Singleton<HUD>.Instance.selectedItemLabel).color.r, ((Graphic)Singleton<HUD>.Instance.selectedItemLabel).color.g, ((Graphic)Singleton<HUD>.Instance.selectedItemLabel).color.b, 1f);
			}
		}
		else
		{
			((TMP_Text)Singleton<HUD>.Instance.selectedItemLabel).text = string.Empty;
		}
		if (discardSlot.ItemInstance != null && !Singleton<HUD>.Instance.discardSlot.IsBeingDragged)
		{
			currentDiscardTime += Time.deltaTime;
			Singleton<HUD>.Instance.discardSlotFill.fillAmount = currentDiscardTime / 1.25f;
			if (currentDiscardTime >= 1.25f)
			{
				discardSlot.ClearStoredInstance();
			}
		}
		else
		{
			currentDiscardTime = 0f;
			Singleton<HUD>.Instance.discardSlotFill.fillAmount = 0f;
		}
	}

	private void UpdateHotbarSelection()
	{
		if (!HotbarEnabled || !EquippingEnabled || GameInput.IsTyping || Singleton<PauseMenu>.Instance.IsPaused)
		{
			return;
		}
		int num = -1;
		if (Input.GetKeyDown((KeyCode)49))
		{
			num = 0;
		}
		else if (Input.GetKeyDown((KeyCode)50))
		{
			num = 1;
		}
		else if (Input.GetKeyDown((KeyCode)51))
		{
			num = 2;
		}
		else if (Input.GetKeyDown((KeyCode)52))
		{
			num = 3;
		}
		else if (Input.GetKeyDown((KeyCode)53))
		{
			num = 4;
		}
		else if (Input.GetKeyDown((KeyCode)54))
		{
			num = 5;
		}
		else if (Input.GetKeyDown((KeyCode)55))
		{
			num = 6;
		}
		else if (Input.GetKeyDown((KeyCode)56))
		{
			num = 7;
		}
		else if (Input.GetKeyDown((KeyCode)57))
		{
			num = 8;
		}
		else if (Input.GetKeyDown((KeyCode)48))
		{
			num = 9;
		}
		if (GameInput.GetButtonDown(GameInput.ButtonCode.Holster) && GameInput.GetCurrentInputDeviceIsGamepad() && HolsterEnabled)
		{
			num = ((EquippedSlotIndex != -1) ? EquippedSlotIndex : ((PreviousEquippedSlotIndex != -1) ? PreviousEquippedSlotIndex : 0));
		}
		if (num == -1)
		{
			float mouseScrollDelta = GameInput.MouseScrollDelta;
			int num2 = ((EquippedSlotIndex == -1) ? PreviousEquippedSlotIndex : EquippedSlotIndex);
			if (mouseScrollDelta < 0f)
			{
				num = num2 + 1;
				if (num >= TOTAL_SLOT_COUNT)
				{
					num = 0;
				}
			}
			else if (mouseScrollDelta > 0f)
			{
				num = num2 - 1;
				if (num < 0)
				{
					num = TOTAL_SLOT_COUNT - 1;
				}
			}
			bool buttonDown = GameInput.GetButtonDown(GameInput.ButtonCode.InventoryLeft);
			if (GameInput.GetButtonDown(GameInput.ButtonCode.InventoryRight))
			{
				num = num2 + 1;
				if (num >= TOTAL_SLOT_COUNT)
				{
					num = 0;
				}
			}
			else if (buttonDown)
			{
				num = num2 - 1;
				if (num < 0)
				{
					num = TOTAL_SLOT_COUNT - 1;
				}
			}
		}
		if (num == -1 && GameInput.GetButtonDown(GameInput.ButtonCode.TertiaryClick))
		{
			if (EquippedSlotIndex != -1)
			{
				num = EquippedSlotIndex;
			}
			else if (PreviousEquippedSlotIndex != -1)
			{
				num = PreviousEquippedSlotIndex;
			}
		}
		if (num != -1 && num < TOTAL_SLOT_COUNT)
		{
			if (num != EquippedSlotIndex && EquippedSlotIndex != -1)
			{
				IndexAllSlots(EquippedSlotIndex).Deselect();
				currentEquipTime = 0f;
			}
			PreviousEquippedSlotIndex = EquippedSlotIndex;
			EquippedSlotIndex = -1;
			if (IndexAllSlots(num).IsSelected)
			{
				IndexAllSlots(num).Deselect();
				return;
			}
			EquippedSlotIndex = num;
			Equip(IndexAllSlots(num));
			PlayerSingleton<ViewmodelSway>.Instance.RefreshViewmodel();
		}
	}

	public void Equip(HotbarSlot slot)
	{
		slot.Select();
	}

	public void SetInventoryEnabled(bool enabled)
	{
		HotbarEnabled = enabled;
		if (onInventoryStateChanged != null)
		{
			onInventoryStateChanged(enabled);
		}
		((Component)Singleton<HUD>.Instance.HotbarContainer).gameObject.SetActive(enabled);
		SetEquippingEnabled(enabled);
	}

	public void SetEquippingEnabled(bool enabled)
	{
		if (EquippingEnabled == enabled)
		{
			return;
		}
		EquippingEnabled = enabled;
		((Component)equipContainer).gameObject.SetActive(enabled);
		if (enabled)
		{
			if (PriorEquippedSlotIndex != -1)
			{
				EquippedSlotIndex = PriorEquippedSlotIndex;
				Equip(IndexAllSlots(EquippedSlotIndex));
			}
		}
		else
		{
			PriorEquippedSlotIndex = EquippedSlotIndex;
			if (EquippedSlotIndex != -1)
			{
				IndexAllSlots(EquippedSlotIndex).Deselect();
				EquippedSlotIndex = -1;
			}
		}
		foreach (ItemSlotUI slotUI in slotUIs)
		{
			((Component)((Transform)slotUI.Rect).Find("Background/Index")).gameObject.SetActive(enabled);
		}
	}

	public void AttachToScreen(UIScreen screen)
	{
		if (!((Object)(object)attachedScreen != (Object)null) && !((Object)(object)screen == (Object)null))
		{
			attachedScreen = screen;
			originalSelectedPanel = screen.CurrentSelectedPanel;
			screen.AddPanel(uiPanel);
			screen.SetCurrentSelectedPanel(uiPanel);
			Debug.Log((object)("Attaching inventory to screen " + ((Object)((Component)screen).gameObject).name));
		}
	}

	public void DetachFromScreen()
	{
		if (!((Object)(object)attachedScreen == (Object)null))
		{
			if ((Object)(object)uiPanel.CurrentSelectedSelectable != (Object)null)
			{
				uiPanel.CurrentSelectedSelectable.OnDeselected.Invoke();
				uiPanel.CurrentSelectedSelectable = null;
			}
			attachedScreen.RemovePanel(uiPanel);
			attachedScreen.SetCurrentSelectedPanel(originalSelectedPanel);
			Debug.Log((object)("Detaching inventory from screen " + ((Object)((Component)attachedScreen).gameObject).name));
			attachedScreen = null;
		}
	}

	private void ClipboardAcquiredVarChange(bool newVal)
	{
		SetManagementClipboardEnabled(newVal);
	}

	public void SetManagementClipboardEnabled(bool enabled)
	{
		if (GameManager.IS_TUTORIAL)
		{
			enabled = false;
		}
		ManagementSlotEnabled = enabled;
		RepositionUI();
	}

	public void SetViewmodelVisible(bool visible)
	{
		PlayerSingleton<PlayerCamera>.Instance.Camera.cullingMask = (visible ? (PlayerSingleton<PlayerCamera>.Instance.Camera.cullingMask | (1 << LayerMask.NameToLayer("Viewmodel"))) : (PlayerSingleton<PlayerCamera>.Instance.Camera.cullingMask & ~(1 << LayerMask.NameToLayer("Viewmodel"))));
	}

	public bool CanItemFitInInventory(ItemInstance item, int quantity = 1)
	{
		if (item == null)
		{
			Console.LogWarning("CanItemFitInInventory: item is null!");
			return false;
		}
		for (int i = 0; i < hotbarSlots.Count; i++)
		{
			if (hotbarSlots[i].ItemInstance == null)
			{
				quantity -= ((BaseItemInstance)item).StackLimit;
			}
			else if (hotbarSlots[i].ItemInstance.CanStackWith(item))
			{
				quantity -= ((BaseItemInstance)item).StackLimit - ((BaseItemInstance)hotbarSlots[i].ItemInstance).Quantity;
			}
		}
		return quantity <= 0;
	}

	public void AddItemToInventory(ItemInstance item)
	{
		if (item == null)
		{
			Console.LogError("AddItemToInventory: item is null!");
			return;
		}
		if (!((BaseItemInstance)item).IsValidInstance())
		{
			Console.LogError("AddItemToInventory: item is not valid!");
			return;
		}
		if (!CanItemFitInInventory(item))
		{
			Console.LogWarning("AddItemToInventory: item won't fit!");
			return;
		}
		int num = ((BaseItemInstance)item).Quantity;
		for (int i = 0; i < hotbarSlots.Count; i++)
		{
			if (num == 0)
			{
				break;
			}
			if (hotbarSlots[i].ItemInstance != null && hotbarSlots[i].ItemInstance.CanStackWith(item, checkQuantities: false))
			{
				int num2 = Mathf.Min(num, ((BaseItemInstance)hotbarSlots[i].ItemInstance).StackLimit - hotbarSlots[i].Quantity);
				if (num2 > 0)
				{
					hotbarSlots[i].ChangeQuantity(num2);
					num -= num2;
				}
			}
		}
		for (int j = 0; j < hotbarSlots.Count; j++)
		{
			if (num == 0)
			{
				break;
			}
			if (hotbarSlots[j].ItemInstance == null)
			{
				hotbarSlots[j].SetStoredItem(item.GetCopy(num));
				num = 0;
			}
		}
		if (num > 0)
		{
			Console.LogWarning("Could not add full amount of '" + ((BaseItemInstance)item).Name + "' to inventory!");
		}
	}

	public uint GetAmountOfItem(string ID)
	{
		uint num = 0u;
		for (int i = 0; i < hotbarSlots.Count; i++)
		{
			if (hotbarSlots[i].ItemInstance != null && ((BaseItemInstance)hotbarSlots[i].ItemInstance).ID.ToLower() == ID.ToLower())
			{
				num += (uint)hotbarSlots[i].Quantity;
			}
		}
		return num;
	}

	public void RemoveAmountOfItem(string ID, uint amount = 1u)
	{
		uint num = amount;
		for (int i = 0; i < hotbarSlots.Count; i++)
		{
			if (hotbarSlots[i].ItemInstance != null && ((BaseItemInstance)hotbarSlots[i].ItemInstance).ID.ToLower() == ID.ToLower())
			{
				uint num2 = num;
				if (num2 > hotbarSlots[i].Quantity)
				{
					num2 = (uint)hotbarSlots[i].Quantity;
				}
				num -= num2;
				hotbarSlots[i].ChangeQuantity((int)(0 - num2));
				if (num == 0)
				{
					break;
				}
			}
		}
		if (num != 0)
		{
			Console.LogWarning("Could not fully remove " + amount + " " + ID);
		}
	}

	public void ClearInventory()
	{
		for (int i = 0; i < hotbarSlots.Count; i++)
		{
			if (hotbarSlots[i].ItemInstance != null)
			{
				hotbarSlots[i].ClearStoredInstance();
			}
		}
	}

	public void RemoveProductFromInventory(EStealthLevel maxStealth)
	{
		for (int i = 0; i < hotbarSlots.Count; i++)
		{
			if (hotbarSlots[i].ItemInstance != null && hotbarSlots[i].ItemInstance is ProductItemInstance)
			{
				ProductItemInstance productItemInstance = hotbarSlots[i].ItemInstance as ProductItemInstance;
				EStealthLevel eStealthLevel = EStealthLevel.None;
				if ((Object)(object)productItemInstance.AppliedPackaging != (Object)null)
				{
					eStealthLevel = productItemInstance.AppliedPackaging.StealthLevel;
				}
				if (eStealthLevel <= maxStealth)
				{
					hotbarSlots[i].ClearStoredInstance();
				}
			}
		}
	}

	public void RemoveRandomItemsFromInventory()
	{
		for (int i = 0; i < hotbarSlots.Count; i++)
		{
			if (hotbarSlots[i].ItemInstance != null && Random.Range(0, 3) == 0)
			{
				int num = Random.Range(1, ((BaseItemInstance)hotbarSlots[i].ItemInstance).Quantity + 1);
				hotbarSlots[i].ChangeQuantity(-num);
			}
		}
	}

	public void SetEquippable(Equippable eq)
	{
		equippable = eq;
		if ((Object)(object)equippable != (Object)null && onItemEquipped != null)
		{
			onItemEquipped.Invoke();
		}
	}

	public void EquippedSlotChanged()
	{
		if (onEquippedSlotChanged != null)
		{
			onEquippedSlotChanged(EquippedSlotIndex);
		}
	}

	public void Reequip()
	{
		HotbarSlot hotbarSlot = equippedSlot;
		if (hotbarSlot != null)
		{
			hotbarSlot.Deselect();
			currentEquipTime = 0f;
			Equip(hotbarSlot);
		}
	}

	public List<ItemSlot> GetAllInventorySlots()
	{
		List<ItemSlot> list = new List<ItemSlot>();
		for (int i = 0; i < hotbarSlots.Count; i++)
		{
			list.Add(hotbarSlots[i]);
		}
		list.Add(cashSlot);
		return list;
	}

	private void UpdateInventoryVariables()
	{
		if (!NetworkSingleton<VariableDatabase>.InstanceExists)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < ItemVariables.Count; i++)
		{
			int num3 = 0;
			for (int j = 0; j < hotbarSlots.Count; j++)
			{
				if (hotbarSlots[j].ItemInstance != null && ((BaseItemInstance)hotbarSlots[j].ItemInstance).ID.ToLower() == ((BaseItemDefinition)ItemVariables[i].Definition).ID.ToLower())
				{
					num3 += hotbarSlots[j].Quantity;
				}
				if (hotbarSlots[j].ItemInstance != null && NetworkSingleton<ProductManager>.Instance.ValidMixIngredients.Contains(hotbarSlots[j].ItemInstance.Definition))
				{
					num += hotbarSlots[j].Quantity;
				}
			}
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(ItemVariables[i].VariableName, num3.ToString(), network: false);
		}
		int num4 = 0;
		for (int k = 0; k < hotbarSlots.Count; k++)
		{
			if (hotbarSlots[k].ItemInstance != null && hotbarSlots[k].ItemInstance is ProductItemInstance)
			{
				if (hotbarSlots[k].ItemInstance is ProductItemInstance && (Object)(object)(hotbarSlots[k].ItemInstance as ProductItemInstance).AppliedPackaging != (Object)null)
				{
					num4 += hotbarSlots[k].Quantity;
				}
				if (hotbarSlots[k].ItemInstance is WeedInstance)
				{
					num2 += hotbarSlots[k].Quantity;
				}
			}
		}
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Inventory_Weed_Count", num2.ToString(), network: false);
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Inventory_Packaged_Product", num4.ToString(), network: false);
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Inventory_MixingIngredients", num.ToString(), network: false);
	}
}
