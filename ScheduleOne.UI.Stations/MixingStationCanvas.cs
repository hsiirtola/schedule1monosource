using System;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks.Tasks;
using ScheduleOne.Product;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using ScheduleOne.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Stations;

public class MixingStationCanvas : Singleton<MixingStationCanvas>
{
	[Header("Prefabs")]
	public StationRecipeEntry RecipeEntryPrefab;

	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public UIScreen UIScreen;

	public ItemSlotUI ProductSlotUI;

	public TextMeshProUGUI ProductPropertiesLabel;

	public ItemSlotUI IngredientSlotUI;

	public TextMeshProUGUI IngredientProblemLabel;

	public ItemSlotUI PreviewSlotUI;

	public Image PreviewIcon;

	public TextMeshProUGUI PreviewLabel;

	public RectTransform UnknownOutputIcon;

	public TextMeshProUGUI PreviewPropertiesLabel;

	public ItemSlotUI OutputSlotUI;

	public TextMeshProUGUI InstructionLabel;

	public RectTransform TitleContainer;

	public RectTransform MainContainer;

	public Button BeginButton;

	public RectTransform ProductHint;

	public RectTransform MixerHint;

	private StationRecipe selectedRecipe;

	public bool isOpen { get; protected set; }

	public MixingStation MixingStation { get; protected set; }

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.Awake();
		((UnityEvent)BeginButton.onClick).AddListener(new UnityAction(BeginButtonPressed));
	}

	protected override void Start()
	{
		base.Start();
		isOpen = false;
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(true);
		Singleton<ItemUIManager>.Instance.AddRaycaster(((Component)Canvas).GetComponent<GraphicRaycaster>());
		GameInput.RegisterExitListener(Exit, 4);
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && isOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			if (Singleton<NewMixScreen>.Instance.IsOpen)
			{
				Singleton<NewMixScreen>.Instance.Close();
			}
			Close();
		}
	}

	protected virtual void Update()
	{
		if (isOpen)
		{
			if (GameInput.GetCurrentInputDeviceIsKeyboardMouse() && ((Selectable)BeginButton).interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
			{
				BeginButtonPressed();
				return;
			}
			UpdateInput();
			UpdateUI();
		}
	}

	private void UpdateUI()
	{
	}

	private void UpdateInput()
	{
		UpdateDisplayMode();
		UpdateInstruction();
	}

	public void Open(MixingStation station)
	{
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Expected O, but got Unknown
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Expected O, but got Unknown
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		isOpen = true;
		MixingStation = station;
		UpdateUI();
		((Behaviour)Canvas).enabled = true;
		((Component)Container).gameObject.SetActive(true);
		if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("MixingHintsShown"))
		{
			((Component)MixerHint).gameObject.SetActive(true);
			((Component)ProductHint).gameObject.SetActive(true);
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("MixingHintsShown", true.ToString());
		}
		else
		{
			((Component)MixerHint).gameObject.SetActive(false);
			((Component)ProductHint).gameObject.SetActive(false);
		}
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		}
		ProductSlotUI.AssignSlot(station.ProductSlot);
		IngredientSlotUI.AssignSlot(station.MixerSlot);
		OutputSlotUI.AssignSlot(station.OutputSlot);
		ItemSlot productSlot = station.ProductSlot;
		productSlot.onItemDataChanged = (Action)Delegate.Combine(productSlot.onItemDataChanged, new Action(StationContentsChanged));
		ItemSlot mixerSlot = station.MixerSlot;
		mixerSlot.onItemDataChanged = (Action)Delegate.Combine(mixerSlot.onItemDataChanged, new Action(StationContentsChanged));
		ItemSlot outputSlot = station.OutputSlot;
		outputSlot.onItemDataChanged = (Action)Delegate.Combine(outputSlot.onItemDataChanged, new Action(StationContentsChanged));
		Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
		Singleton<UIScreenManager>.Instance.AddScreen(UIScreen);
		if (PlayerSingleton<PlayerInventory>.InstanceExists)
		{
			PlayerSingleton<PlayerInventory>.Instance.AttachToScreen(UIScreen);
		}
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: true);
		List<ItemSlot> list = new List<ItemSlot>();
		list.Add(station.ProductSlot);
		list.Add(station.MixerSlot);
		list.Add(station.OutputSlot);
		Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), list);
		UpdateDisplayMode();
		UpdateInstruction();
		UpdatePreview();
		UpdateBeginButton();
		if (station.IsMixingDone && !station.CurrentMixOperation.IsOutputKnown(out var _))
		{
			station.CurrentMixOperation.GetOutput(out var properties);
			ProductDefinition item = Registry.GetItem<ProductDefinition>(MixingStation.CurrentMixOperation.ProductID);
			station.DiscoveryBox.ShowProduct(item, properties);
			((Component)station.DiscoveryBox).transform.SetParent(((Component)PlayerSingleton<PlayerCamera>.Instance).transform);
			((Component)station.DiscoveryBox).transform.localPosition = station.DiscoveryBoxOffset;
			((Component)station.DiscoveryBox).transform.localRotation = station.DiscoveryBoxRotation;
			float productMarketValue = ProductManager.CalculateProductValue(item.BasePrice, properties);
			Singleton<NewMixScreen>.Instance.Open(properties, item.DrugType, productMarketValue);
			NewMixScreen newMixScreen = Singleton<NewMixScreen>.Instance;
			newMixScreen.onMixNamed = (Action<string>)Delegate.Remove(newMixScreen.onMixNamed, new Action<string>(MixNamed));
			NewMixScreen newMixScreen2 = Singleton<NewMixScreen>.Instance;
			newMixScreen2.onMixNamed = (Action<string>)Delegate.Combine(newMixScreen2.onMixNamed, new Action<string>(MixNamed));
		}
		else
		{
			station.onMixDone.RemoveListener(new UnityAction(MixingDone));
			station.onMixDone.AddListener(new UnityAction(MixingDone));
		}
		Singleton<CompassManager>.Instance.SetVisible(visible: false);
	}

	public void Close(bool enablePlayerControl = true)
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		isOpen = false;
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		}
		ProductSlotUI.ClearSlot();
		IngredientSlotUI.ClearSlot();
		OutputSlotUI.ClearSlot();
		ItemSlot productSlot = MixingStation.ProductSlot;
		productSlot.onItemDataChanged = (Action)Delegate.Remove(productSlot.onItemDataChanged, new Action(StationContentsChanged));
		ItemSlot mixerSlot = MixingStation.MixerSlot;
		mixerSlot.onItemDataChanged = (Action)Delegate.Remove(mixerSlot.onItemDataChanged, new Action(StationContentsChanged));
		ItemSlot outputSlot = MixingStation.OutputSlot;
		outputSlot.onItemDataChanged = (Action)Delegate.Remove(outputSlot.onItemDataChanged, new Action(StationContentsChanged));
		MixingStation.onMixDone.RemoveListener(new UnityAction(MixingDone));
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
		if (PlayerSingleton<PlayerInventory>.InstanceExists)
		{
			PlayerSingleton<PlayerInventory>.Instance.DetachFromScreen();
		}
		Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
		if (enablePlayerControl)
		{
			MixingStation.Close();
			MixingStation = null;
		}
	}

	private void MixingDone()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if (MixingStation.IsMixingDone && !MixingStation.CurrentMixOperation.IsOutputKnown(out var _))
		{
			MixingStation.CurrentMixOperation.GetOutput(out var properties);
			ProductDefinition item = Registry.GetItem<ProductDefinition>(MixingStation.CurrentMixOperation.ProductID);
			MixingStation.DiscoveryBox.ShowProduct(item, properties);
			((Component)MixingStation.DiscoveryBox).transform.SetParent(((Component)PlayerSingleton<PlayerCamera>.Instance).transform);
			((Component)MixingStation.DiscoveryBox).transform.localPosition = MixingStation.DiscoveryBoxOffset;
			((Component)MixingStation.DiscoveryBox).transform.localRotation = MixingStation.DiscoveryBoxRotation;
			float productMarketValue = ProductManager.CalculateProductValue(item.BasePrice, properties);
			Singleton<NewMixScreen>.Instance.Open(properties, item.DrugType, productMarketValue);
			NewMixScreen newMixScreen = Singleton<NewMixScreen>.Instance;
			newMixScreen.onMixNamed = (Action<string>)Delegate.Remove(newMixScreen.onMixNamed, new Action<string>(MixNamed));
			NewMixScreen newMixScreen2 = Singleton<NewMixScreen>.Instance;
			newMixScreen2.onMixNamed = (Action<string>)Delegate.Combine(newMixScreen2.onMixNamed, new Action<string>(MixNamed));
		}
		UpdateDisplayMode();
		UpdateInstruction();
		UpdatePreview();
		UpdateBeginButton();
	}

	private void StationContentsChanged()
	{
		UpdateDisplayMode();
		UpdatePreview();
		UpdateBeginButton();
		if (MixingStation.ProductSlot.Quantity > 0)
		{
			((Component)ProductHint).gameObject.SetActive(false);
		}
		if (MixingStation.MixerSlot.Quantity > 0)
		{
			((Component)MixerHint).gameObject.SetActive(false);
		}
	}

	private void UpdateDisplayMode()
	{
		((Component)TitleContainer).gameObject.SetActive(true);
		((Component)MainContainer).gameObject.SetActive(true);
		((Component)OutputSlotUI).gameObject.SetActive(false);
		ProductDefinition knownProduct;
		if (MixingStation.OutputSlot.Quantity > 0)
		{
			((Component)MainContainer).gameObject.SetActive(false);
			((Component)OutputSlotUI).gameObject.SetActive(true);
		}
		else if (MixingStation.CurrentMixOperation != null && MixingStation.IsMixingDone && !MixingStation.CurrentMixOperation.IsOutputKnown(out knownProduct))
		{
			((Component)TitleContainer).gameObject.SetActive(false);
			((Component)MainContainer).gameObject.SetActive(false);
			((Component)OutputSlotUI).gameObject.SetActive(false);
		}
	}

	private void UpdateInstruction()
	{
		((Behaviour)InstructionLabel).enabled = true;
		if (MixingStation.OutputSlot.Quantity > 0)
		{
			((TMP_Text)InstructionLabel).text = "Collect output";
		}
		else if (MixingStation.CurrentMixOperation != null)
		{
			((TMP_Text)InstructionLabel).text = "Mixing in progress...";
		}
		else if (!MixingStation.CanStartMix())
		{
			((TMP_Text)InstructionLabel).text = "Insert unpackaged product and mixing ingredient";
		}
		else
		{
			((Behaviour)InstructionLabel).enabled = false;
		}
	}

	private void UpdatePreview()
	{
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		ProductDefinition product = MixingStation.GetProduct();
		PropertyItemDefinition mixer = MixingStation.GetMixer();
		if ((Object)(object)product != (Object)null)
		{
			((TMP_Text)ProductPropertiesLabel).text = GetPropertyListString(product.Properties);
			((Behaviour)ProductPropertiesLabel).enabled = true;
		}
		else
		{
			((Behaviour)ProductPropertiesLabel).enabled = false;
		}
		if ((Object)(object)mixer == (Object)null && MixingStation.MixerSlot.Quantity > 0)
		{
			((Behaviour)IngredientProblemLabel).enabled = true;
		}
		else
		{
			((Behaviour)IngredientProblemLabel).enabled = false;
		}
		((Component)UnknownOutputIcon).gameObject.SetActive(false);
		if ((Object)(object)product != (Object)null && (Object)(object)mixer != (Object)null)
		{
			List<Effect> outputProperties = GetOutputProperties(product, mixer);
			ProductDefinition knownProduct = NetworkSingleton<ProductManager>.Instance.GetKnownProduct(product.DrugTypes[0].DrugType, outputProperties);
			if ((Object)(object)knownProduct == (Object)null)
			{
				PreviewIcon.sprite = ((BaseItemDefinition)product).Icon;
				((Graphic)PreviewIcon).color = Color.black;
				((Behaviour)PreviewIcon).enabled = true;
				((TMP_Text)PreviewLabel).text = "Unknown";
				((Behaviour)PreviewLabel).enabled = true;
				((Component)UnknownOutputIcon).gameObject.SetActive(true);
				((TMP_Text)PreviewPropertiesLabel).text = string.Empty;
				for (int i = 0; i < outputProperties.Count; i++)
				{
					if (product.Properties.Contains(outputProperties[i]))
					{
						if (((TMP_Text)PreviewPropertiesLabel).text.Length > 0)
						{
							TextMeshProUGUI previewPropertiesLabel = PreviewPropertiesLabel;
							((TMP_Text)previewPropertiesLabel).text = ((TMP_Text)previewPropertiesLabel).text + "\n";
						}
						TextMeshProUGUI previewPropertiesLabel2 = PreviewPropertiesLabel;
						((TMP_Text)previewPropertiesLabel2).text = ((TMP_Text)previewPropertiesLabel2).text + GetPropertyString(outputProperties[i]);
					}
					else
					{
						if (((TMP_Text)PreviewPropertiesLabel).text.Length > 0)
						{
							TextMeshProUGUI previewPropertiesLabel3 = PreviewPropertiesLabel;
							((TMP_Text)previewPropertiesLabel3).text = ((TMP_Text)previewPropertiesLabel3).text + "\n";
						}
						TextMeshProUGUI previewPropertiesLabel4 = PreviewPropertiesLabel;
						((TMP_Text)previewPropertiesLabel4).text = ((TMP_Text)previewPropertiesLabel4).text + "<color=#" + ColorUtility.ToHtmlStringRGBA(outputProperties[i].LabelColor) + ">• ?</color>";
					}
				}
				((Behaviour)PreviewPropertiesLabel).enabled = true;
				LayoutRebuilder.ForceRebuildLayoutImmediate(((TMP_Text)PreviewPropertiesLabel).rectTransform);
			}
			else
			{
				PreviewIcon.sprite = ((BaseItemDefinition)knownProduct).Icon;
				((Graphic)PreviewIcon).color = Color.white;
				((Behaviour)PreviewIcon).enabled = true;
				((TMP_Text)PreviewLabel).text = ((BaseItemDefinition)knownProduct).Name;
				((Behaviour)PreviewLabel).enabled = true;
				((Component)UnknownOutputIcon).gameObject.SetActive(false);
				((TMP_Text)PreviewPropertiesLabel).text = GetPropertyListString(knownProduct.Properties);
				((Behaviour)PreviewPropertiesLabel).enabled = true;
				LayoutRebuilder.ForceRebuildLayoutImmediate(((TMP_Text)PreviewPropertiesLabel).rectTransform);
			}
		}
		else
		{
			((Behaviour)PreviewIcon).enabled = false;
			((Behaviour)PreviewLabel).enabled = false;
			((Behaviour)PreviewPropertiesLabel).enabled = false;
		}
	}

	private string GetPropertyListString(List<Effect> properties)
	{
		string text = "";
		for (int i = 0; i < properties.Count; i++)
		{
			if (i > 0)
			{
				text += "\n";
			}
			text += GetPropertyString(properties[i]);
		}
		return text;
	}

	private string GetPropertyString(Effect property)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return "<color=#" + ColorUtility.ToHtmlStringRGBA(property.LabelColor) + ">• " + property.Name + "</color>";
	}

	private List<Effect> GetOutputProperties(ProductDefinition product, PropertyItemDefinition mixer)
	{
		List<Effect> properties = product.Properties;
		List<Effect> properties2 = mixer.Properties;
		return EffectMixCalculator.MixProperties(properties, properties2[0], product.DrugType);
	}

	private bool IsOutputKnown(out ProductDefinition knownProduct)
	{
		knownProduct = null;
		ProductDefinition product = MixingStation.GetProduct();
		PropertyItemDefinition mixer = MixingStation.GetMixer();
		if ((Object)(object)product != (Object)null && (Object)(object)mixer != (Object)null)
		{
			List<Effect> outputProperties = GetOutputProperties(product, mixer);
			knownProduct = NetworkSingleton<ProductManager>.Instance.GetKnownProduct(product.DrugTypes[0].DrugType, outputProperties);
		}
		return (Object)(object)knownProduct != (Object)null;
	}

	private void UpdateBeginButton()
	{
		if (MixingStation.CurrentMixOperation != null || MixingStation.OutputSlot.Quantity > 0)
		{
			((Component)BeginButton).gameObject.SetActive(false);
			return;
		}
		((Component)BeginButton).gameObject.SetActive(true);
		((Selectable)BeginButton).interactable = MixingStation.CanStartMix();
	}

	public void BeginButtonPressed()
	{
		if (!((Behaviour)BeginButton).isActiveAndEnabled || !((Selectable)BeginButton).interactable)
		{
			return;
		}
		int mixQuantity = MixingStation.GetMixQuantity();
		if (mixQuantity > 0)
		{
			bool flag = false;
			if (Application.isEditor && Input.GetKey((KeyCode)114))
			{
				flag = true;
			}
			if (MixingStation.RequiresIngredientInsertion && !flag)
			{
				MixingStation mixingStation = MixingStation;
				Close(enablePlayerControl: false);
				new UseMixingStationTask(mixingStation);
				return;
			}
			ProductItemInstance productItemInstance = MixingStation.ProductSlot.ItemInstance as ProductItemInstance;
			string iD = ((BaseItemInstance)MixingStation.MixerSlot.ItemInstance).ID;
			MixingStation.ProductSlot.ChangeQuantity(-mixQuantity);
			MixingStation.MixerSlot.ChangeQuantity(-mixQuantity);
			StartMixOperation(new MixOperation(((BaseItemInstance)productItemInstance).ID, productItemInstance.Quality, iD, mixQuantity));
			Close();
		}
		else
		{
			Console.LogWarning("Failed to start mixing operation, not enough ingredients or output slot is full");
		}
	}

	public void StartMixOperation(MixOperation mixOperation)
	{
		MixingStation.SendMixingOperation(mixOperation, 0);
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Mixing_Operations_Started", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Mixing_Operations_Started") + 1f).ToString());
	}

	private void MixNamed(string mixName)
	{
		if ((Object)(object)MixingStation == (Object)null)
		{
			Console.LogWarning("Mixing station is null, cannot finish mix operation");
			return;
		}
		if (MixingStation.CurrentMixOperation == null)
		{
			Console.LogWarning("Mixing station current mix operation is null, cannot finish mix operation");
			return;
		}
		NetworkSingleton<ProductManager>.Instance.FinishAndNameMix(MixingStation.CurrentMixOperation.ProductID, MixingStation.CurrentMixOperation.IngredientID, mixName);
		MixingStation.TryCreateOutputItems();
		((Component)MixingStation.DiscoveryBox).gameObject.SetActive(false);
		UpdateDisplayMode();
	}
}
