using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Stations;

public class LabOvenCanvas : Singleton<LabOvenCanvas>
{
	[Header("References")]
	public Canvas Canvas;

	public GameObject Container;

	public UIScreen UIScreen;

	public ItemSlotUI IngredientSlotUI;

	public ItemSlotUI OutputSlotUI;

	public TextMeshProUGUI InstructionLabel;

	public TextMeshProUGUI ErrorLabel;

	public Button BeginButton;

	public TextMeshProUGUI BeginButtonLabel;

	public RectTransform ProgressContainer;

	public Image IngredientIcon;

	public Image ProgressImg;

	public Image ProductIcon;

	public bool isOpen { get; protected set; }

	public LabOven Oven { get; protected set; }

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
		Singleton<ItemUIManager>.Instance.AddRaycaster(((Component)Canvas).GetComponent<GraphicRaycaster>());
		SetIsOpen(null, open: false);
	}

	protected virtual void Update()
	{
		if (!isOpen)
		{
			return;
		}
		((Selectable)BeginButton).interactable = CanBegin();
		if (GameInput.GetCurrentInputDeviceIsKeyboardMouse() && ((Selectable)BeginButton).interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
		{
			BeginButtonPressed();
			return;
		}
		if (Oven.CurrentOperation != null)
		{
			ProgressImg.fillAmount = Mathf.Clamp01((float)Oven.CurrentOperation.CookProgress / (float)Oven.CurrentOperation.GetCookDuration());
			((TMP_Text)BeginButtonLabel).text = "COLLECT";
			if (Oven.CurrentOperation.CookProgress >= Oven.CurrentOperation.GetCookDuration())
			{
				if (DoesOvenOutputHaveSpace())
				{
					((TMP_Text)InstructionLabel).text = "Ready to collect product";
					((Behaviour)InstructionLabel).enabled = true;
					((Behaviour)ErrorLabel).enabled = false;
				}
				else
				{
					((TMP_Text)ErrorLabel).text = "Not enough space in output slot";
					((Behaviour)ErrorLabel).enabled = true;
					((Behaviour)InstructionLabel).enabled = false;
				}
			}
			else
			{
				((TMP_Text)InstructionLabel).text = "Cooking in progress...";
				((Behaviour)InstructionLabel).enabled = true;
				((Behaviour)ErrorLabel).enabled = false;
			}
			return;
		}
		((Component)ProgressContainer).gameObject.SetActive(false);
		((TMP_Text)BeginButtonLabel).text = "BEGIN";
		if (Oven.IngredientSlot.ItemInstance != null)
		{
			if (Oven.IsIngredientCookable())
			{
				((TMP_Text)InstructionLabel).text = "Ready to begin cooking";
				((Behaviour)InstructionLabel).enabled = true;
			}
			else
			{
				((Behaviour)InstructionLabel).enabled = false;
				((Behaviour)ErrorLabel).enabled = true;
				((TMP_Text)ErrorLabel).text = "Ingredient is not cookable";
			}
		}
		else
		{
			((TMP_Text)InstructionLabel).text = "Place cookable item in ingredient slot";
			((Behaviour)InstructionLabel).enabled = true;
			((Behaviour)ErrorLabel).enabled = false;
		}
	}

	public void SetIsOpen(LabOven oven, bool open, bool removeUI = true)
	{
		isOpen = open;
		((Behaviour)Canvas).enabled = open;
		Container.gameObject.SetActive(open);
		Oven = oven;
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			if (open)
			{
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
			}
		}
		if ((Object)(object)oven != (Object)null)
		{
			IngredientSlotUI.AssignSlot(oven.IngredientSlot);
			OutputSlotUI.AssignSlot(oven.OutputSlot);
		}
		else
		{
			IngredientSlotUI.ClearSlot();
			OutputSlotUI.ClearSlot();
		}
		if (open)
		{
			RefreshActiveOperation();
			Update();
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			Singleton<UIScreenManager>.Instance.AddScreen(UIScreen);
			if (PlayerSingleton<PlayerInventory>.InstanceExists)
			{
				PlayerSingleton<PlayerInventory>.Instance.AttachToScreen(UIScreen);
			}
		}
		else if (removeUI)
		{
			if (PlayerSingleton<PlayerInventory>.InstanceExists)
			{
				PlayerSingleton<PlayerInventory>.Instance.DetachFromScreen();
			}
			Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		}
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(open);
		if (open)
		{
			Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), new List<ItemSlot> { Oven.IngredientSlot, Oven.OutputSlot });
		}
		if (isOpen)
		{
			Singleton<CompassManager>.Instance.SetVisible(visible: false);
		}
	}

	public void BeginButtonPressed()
	{
		if (((Behaviour)BeginButton).isActiveAndEnabled && ((Selectable)BeginButton).interactable)
		{
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			if (Oven.CurrentOperation != null)
			{
				new FinalizeLabOven(Oven);
			}
			else if ((Oven.IngredientSlot.ItemInstance.Definition as StorableItemDefinition).StationItem.GetModule<CookableModule>().CookType == CookableModule.ECookableType.Liquid)
			{
				new StartLabOvenTask(Oven);
			}
			else
			{
				new LabOvenSolidTask(Oven);
			}
			SetIsOpen(null, open: false, removeUI: false);
		}
	}

	public bool CanBegin()
	{
		if ((Object)(object)Oven == (Object)null)
		{
			return false;
		}
		if (Oven.CurrentOperation != null)
		{
			if (Oven.CurrentOperation.CookProgress >= Oven.CurrentOperation.GetCookDuration())
			{
				if (DoesOvenOutputHaveSpace())
				{
					return true;
				}
				return false;
			}
			return false;
		}
		return Oven.IsIngredientCookable();
	}

	private bool DoesOvenOutputHaveSpace()
	{
		return Oven.OutputSlot.GetCapacityForItem(Oven.CurrentOperation.Product.GetDefaultInstance()) >= Oven.CurrentOperation.Cookable.ProductQuantity;
	}

	private void RefreshActiveOperation()
	{
		if (Oven.CurrentOperation != null)
		{
			IngredientIcon.sprite = ((BaseItemDefinition)Oven.CurrentOperation.Ingredient).Icon;
			ProductIcon.sprite = ((BaseItemDefinition)Oven.CurrentOperation.Product).Icon;
		}
	}
}
