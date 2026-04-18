using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Stations;

public class BrickPressCanvas : Singleton<BrickPressCanvas>
{
	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public UIScreen UIScreen;

	public ItemSlotUI[] ProductSlotUIs;

	public ItemSlotUI OutputSlotUI;

	public TextMeshProUGUI InstructionLabel;

	public Button BeginButton;

	public bool isOpen { get; protected set; }

	public BrickPress Press { get; protected set; }

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
		if (GameInput.GetCurrentInputDeviceIsKeyboardMouse() && ((Selectable)BeginButton).interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
		{
			BeginButtonPressed();
			return;
		}
		switch (Press.GetState())
		{
		case PackagingStation.EState.CanBegin:
			((Behaviour)InstructionLabel).enabled = false;
			((Selectable)BeginButton).interactable = true;
			return;
		case PackagingStation.EState.InsufficentProduct:
			((TMP_Text)InstructionLabel).text = "Drag 20x product into input slots";
			break;
		case PackagingStation.EState.OutputSlotFull:
			((TMP_Text)InstructionLabel).text = "Output slot is full!";
			break;
		case PackagingStation.EState.Mismatch:
			((TMP_Text)InstructionLabel).text = "Output slot is full!";
			break;
		}
		((Behaviour)InstructionLabel).enabled = true;
		((Selectable)BeginButton).interactable = false;
	}

	public void SetIsOpen(BrickPress press, bool open, bool removeUI = true)
	{
		isOpen = open;
		((Behaviour)Canvas).enabled = open;
		((Component)Container).gameObject.SetActive(open);
		Press = press;
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			if (open)
			{
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
			}
		}
		if ((Object)(object)press != (Object)null)
		{
			for (int i = 0; i < ProductSlotUIs.Length; i++)
			{
				ProductSlotUIs[i].AssignSlot(press.InputSlots[i]);
			}
			OutputSlotUI.AssignSlot(press.OutputSlot);
		}
		else
		{
			for (int j = 0; j < ProductSlotUIs.Length; j++)
			{
				ProductSlotUIs[j].ClearSlot();
			}
			OutputSlotUI.ClearSlot();
		}
		if (open)
		{
			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			Singleton<UIScreenManager>.Instance.AddScreen(UIScreen);
			if (PlayerSingleton<PlayerInventory>.InstanceExists)
			{
				PlayerSingleton<PlayerInventory>.Instance.AttachToScreen(UIScreen);
			}
			Update();
		}
		else if (removeUI)
		{
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			if (PlayerSingleton<PlayerInventory>.InstanceExists)
			{
				PlayerSingleton<PlayerInventory>.Instance.DetachFromScreen();
			}
			Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
		}
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(open);
		if (open)
		{
			List<ItemSlot> list = new List<ItemSlot>();
			list.AddRange(press.InputSlots);
			list.Add(press.OutputSlot);
			Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), list);
		}
		if (isOpen)
		{
			Singleton<CompassManager>.Instance.SetVisible(visible: false);
		}
	}

	public void BeginButtonPressed()
	{
		if (((Behaviour)BeginButton).isActiveAndEnabled && ((Selectable)BeginButton).interactable && Press.GetState() == PackagingStation.EState.CanBegin && Press.HasSufficientProduct(out var product))
		{
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			new UseBrickPress(Press, product);
			SetIsOpen(null, open: false, removeUI: false);
		}
	}
}
