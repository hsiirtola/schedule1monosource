using System;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Storage;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class CreateMixInterface : Singleton<CreateMixInterface>
{
	public const int BEAN_REQUIREMENT = 5;

	[Header("References")]
	public Canvas Canvas;

	public ItemSlotUI BeansSlot;

	public ItemSlotUI ProductSlot;

	public ItemSlotUI MixerSlot;

	public ItemSlotUI OutputSlot;

	public Image OutputIcon;

	public Button BeginButton;

	public WorldStorageEntity Storage;

	public TextMeshProUGUI ProductPropertiesLabel;

	public TextMeshProUGUI OutputPropertiesLabel;

	public TextMeshProUGUI BeanProblemLabel;

	public TextMeshProUGUI ProductProblemLabel;

	public TextMeshProUGUI MixerProblemLabel;

	public TextMeshProUGUI OutputProblemLabel;

	public Transform CameraPosition;

	public RectTransform UnknownOutputIcon;

	public UnityEvent onOpen;

	public UnityEvent onClose;

	public bool IsOpen { get; private set; }

	private ItemSlot beanSlot => Storage.ItemSlots[0];

	private ItemSlot mixerSlot => Storage.ItemSlots[1];

	private ItemSlot outputSlot => Storage.ItemSlots[2];

	private ItemSlot productSlot => Storage.ItemSlots[3];

	protected override void Awake()
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		base.Awake();
		((Behaviour)Canvas).enabled = false;
		BeansSlot.AssignSlot(beanSlot);
		MixerSlot.AssignSlot(mixerSlot);
		OutputSlot.AssignSlot(outputSlot);
		ProductSlot.AssignSlot(productSlot);
		beanSlot.AddFilter(new ItemFilter_ID(new List<string> { "megabean" }));
		productSlot.AddFilter(new ItemFilter_Category(new List<EItemCategory> { (EItemCategory)0 }));
		outputSlot.SetIsAddLocked(locked: true);
		WorldStorageEntity storage = Storage;
		storage.onContentsChanged = (Action)Delegate.Combine(storage.onContentsChanged, new Action(ContentsChanged));
		((UnityEvent)BeginButton.onClick).AddListener(new UnityAction(BeginPressed));
		GameInput.RegisterExitListener(Exit, 3);
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			Close();
		}
	}

	public void Open()
	{
		IsOpen = true;
		((Behaviour)Canvas).enabled = true;
		Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.2f);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0f);
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: true);
		List<ItemSlot> secondarySlots = new List<ItemSlot> { beanSlot, productSlot, mixerSlot };
		Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), secondarySlots);
		Singleton<CompassManager>.Instance.SetVisible(visible: false);
		ContentsChanged();
	}

	private void ContentsChanged()
	{
		UpdateCanBegin();
		UpdateOutput();
	}

	private void UpdateCanBegin()
	{
		((Behaviour)BeanProblemLabel).enabled = !HasBeans();
		((Behaviour)ProductProblemLabel).enabled = !HasProduct();
		if (HasProduct())
		{
			ProductDefinition productDefinition = productSlot.ItemInstance.Definition as ProductDefinition;
			((TMP_Text)ProductPropertiesLabel).text = GetPropertyListString(productDefinition.Properties);
			((Behaviour)ProductPropertiesLabel).enabled = true;
		}
		else
		{
			((Behaviour)ProductPropertiesLabel).enabled = false;
		}
		if (mixerSlot.Quantity == 0)
		{
			((TMP_Text)MixerProblemLabel).text = "Required";
			((Behaviour)MixerProblemLabel).enabled = true;
		}
		else if (!HasMixer())
		{
			((TMP_Text)MixerProblemLabel).text = "Invalid mixer";
			((Behaviour)MixerProblemLabel).enabled = true;
		}
		else
		{
			((Behaviour)MixerProblemLabel).enabled = false;
		}
		((Selectable)BeginButton).interactable = CanBegin();
	}

	private void UpdateOutput()
	{
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		ProductDefinition product = GetProduct();
		PropertyItemDefinition mixer = GetMixer();
		if ((Object)(object)product != (Object)null && (Object)(object)mixer != (Object)null)
		{
			List<Effect> outputProperties = GetOutputProperties(product, mixer);
			ProductDefinition knownProduct = NetworkSingleton<ProductManager>.Instance.GetKnownProduct(product.DrugTypes[0].DrugType, outputProperties);
			if ((Object)(object)knownProduct == (Object)null)
			{
				OutputIcon.sprite = ((BaseItemDefinition)product).Icon;
				((Graphic)OutputIcon).color = Color.black;
				((Behaviour)OutputIcon).enabled = true;
				((Component)UnknownOutputIcon).gameObject.SetActive(true);
				List<Color32> list = new List<Color32>();
				((TMP_Text)OutputPropertiesLabel).text = string.Empty;
				for (int i = 0; i < outputProperties.Count; i++)
				{
					if (((TMP_Text)OutputPropertiesLabel).text.Length > 0)
					{
						TextMeshProUGUI outputPropertiesLabel = OutputPropertiesLabel;
						((TMP_Text)outputPropertiesLabel).text = ((TMP_Text)outputPropertiesLabel).text + "\n";
					}
					if (product.Properties.Contains(outputProperties[i]))
					{
						TextMeshProUGUI outputPropertiesLabel2 = OutputPropertiesLabel;
						((TMP_Text)outputPropertiesLabel2).text = ((TMP_Text)outputPropertiesLabel2).text + GetPropertyString(outputProperties[i]);
					}
					else
					{
						list.Add(Color32.op_Implicit(outputProperties[i].LabelColor));
					}
				}
				for (int j = 0; j < list.Count; j++)
				{
					if (((TMP_Text)OutputPropertiesLabel).text.Length > 0)
					{
						TextMeshProUGUI outputPropertiesLabel3 = OutputPropertiesLabel;
						((TMP_Text)outputPropertiesLabel3).text = ((TMP_Text)outputPropertiesLabel3).text + "\n";
					}
					TextMeshProUGUI outputPropertiesLabel4 = OutputPropertiesLabel;
					((TMP_Text)outputPropertiesLabel4).text = ((TMP_Text)outputPropertiesLabel4).text + "<color=#" + ColorUtility.ToHtmlStringRGBA(Color32.op_Implicit(list[j])) + ">• ?</color>";
				}
				((Behaviour)OutputPropertiesLabel).enabled = true;
				((Behaviour)OutputProblemLabel).enabled = false;
				LayoutRebuilder.ForceRebuildLayoutImmediate(((TMP_Text)OutputPropertiesLabel).rectTransform);
			}
			else
			{
				OutputIcon.sprite = ((BaseItemDefinition)knownProduct).Icon;
				((Graphic)OutputIcon).color = Color.white;
				((Behaviour)OutputIcon).enabled = true;
				((Component)UnknownOutputIcon).gameObject.SetActive(false);
				((TMP_Text)OutputPropertiesLabel).text = GetPropertyListString(knownProduct.Properties);
				((Behaviour)OutputPropertiesLabel).enabled = true;
				((TMP_Text)OutputProblemLabel).text = "Mix already known. ";
				((Behaviour)OutputProblemLabel).enabled = true;
				LayoutRebuilder.ForceRebuildLayoutImmediate(((TMP_Text)OutputPropertiesLabel).rectTransform);
			}
		}
		else
		{
			((Behaviour)OutputIcon).enabled = false;
			((Behaviour)OutputPropertiesLabel).enabled = false;
			((Behaviour)OutputProblemLabel).enabled = false;
		}
	}

	private void BeginPressed()
	{
		if (CanBegin())
		{
			ProductDefinition product = GetProduct();
			NewMixOperation operation = new NewMixOperation(ingredientID: ((BaseItemDefinition)GetMixer()).ID, productID: ((BaseItemDefinition)product).ID);
			NetworkSingleton<ProductManager>.Instance.SendMixOperation(operation, complete: false);
			beanSlot.ChangeQuantity(-5);
			productSlot.ChangeQuantity(-1);
			mixerSlot.ChangeQuantity(-1);
			Close();
		}
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
		ProductDefinition product = GetProduct();
		PropertyItemDefinition mixer = GetMixer();
		if ((Object)(object)product != (Object)null && (Object)(object)mixer != (Object)null)
		{
			List<Effect> outputProperties = GetOutputProperties(product, mixer);
			knownProduct = NetworkSingleton<ProductManager>.Instance.GetKnownProduct(product.DrugTypes[0].DrugType, outputProperties);
		}
		return (Object)(object)knownProduct != (Object)null;
	}

	private string GetPropertyListString(List<Effect> properties)
	{
		((TMP_Text)ProductPropertiesLabel).text = "";
		for (int i = 0; i < properties.Count; i++)
		{
			if (i > 0)
			{
				TextMeshProUGUI productPropertiesLabel = ProductPropertiesLabel;
				((TMP_Text)productPropertiesLabel).text = ((TMP_Text)productPropertiesLabel).text + "\n";
			}
			TextMeshProUGUI productPropertiesLabel2 = ProductPropertiesLabel;
			((TMP_Text)productPropertiesLabel2).text = ((TMP_Text)productPropertiesLabel2).text + GetPropertyString(properties[i]);
		}
		return ((TMP_Text)ProductPropertiesLabel).text;
	}

	private string GetPropertyString(Effect property)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return "<color=#" + ColorUtility.ToHtmlStringRGBA(property.LabelColor) + ">• " + property.Name + "</color>";
	}

	private bool CanBegin()
	{
		ProductDefinition knownProduct;
		if (HasBeans() && HasProduct() && HasMixer())
		{
			return !IsOutputKnown(out knownProduct);
		}
		return false;
	}

	public void Close()
	{
		IsOpen = false;
		((Behaviour)Canvas).enabled = false;
		if (beanSlot.ItemInstance != null)
		{
			PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(beanSlot.ItemInstance.GetCopy());
			beanSlot.ClearStoredInstance();
		}
		if (productSlot.ItemInstance != null)
		{
			PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(productSlot.ItemInstance.GetCopy());
			productSlot.ClearStoredInstance();
		}
		if (mixerSlot.ItemInstance != null)
		{
			PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(mixerSlot.ItemInstance.GetCopy());
			mixerSlot.ClearStoredInstance();
		}
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.2f, reenableCameraLook: true, returnToOriginalRotation: false);
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.2f);
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0f);
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
		Singleton<CompassManager>.Instance.SetVisible(visible: true);
	}

	private bool HasProduct()
	{
		return (Object)(object)GetProduct() != (Object)null;
	}

	private bool HasBeans()
	{
		return beanSlot.Quantity >= 5;
	}

	private bool HasMixer()
	{
		return (Object)(object)GetMixer() != (Object)null;
	}

	private ProductDefinition GetProduct()
	{
		if (productSlot.ItemInstance != null)
		{
			return productSlot.ItemInstance.Definition as ProductDefinition;
		}
		return null;
	}

	private PropertyItemDefinition GetMixer()
	{
		if (mixerSlot.ItemInstance != null)
		{
			PropertyItemDefinition propertyItemDefinition = mixerSlot.ItemInstance.Definition as PropertyItemDefinition;
			if ((Object)(object)propertyItemDefinition != (Object)null && NetworkSingleton<ProductManager>.Instance.ValidMixIngredients.Contains(propertyItemDefinition))
			{
				return propertyItemDefinition;
			}
		}
		return null;
	}
}
