using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class StorageMenu : Singleton<StorageMenu>
{
	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public UIScreen UIScreen;

	public TextMeshProUGUI TitleLabel;

	public TextMeshProUGUI SubtitleLabel;

	public RectTransform SlotContainer;

	public ItemSlotUI[] SlotsUIs;

	public GridLayoutGroup SlotGridLayout;

	public RectTransform CloseButton;

	public UnityEvent onClosed;

	public bool IsOpen { get; protected set; }

	public StorageEntity OpenedStorageEntity { get; protected set; }

	protected override void Awake()
	{
		base.Awake();
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		GameInput.RegisterExitListener(Exit, 3);
	}

	protected override void Start()
	{
		base.Start();
		Singleton<ItemUIManager>.Instance.AddRaycaster(((Component)Canvas).GetComponent<GraphicRaycaster>());
	}

	public virtual void Open(IItemSlotOwner owner, string title, string subtitle)
	{
		IsOpen = true;
		OpenedStorageEntity = null;
		SlotGridLayout.constraintCount = 1;
		Open(title, subtitle, owner);
	}

	public virtual void Open(StorageEntity entity)
	{
		IsOpen = true;
		OpenedStorageEntity = entity;
		SlotGridLayout.constraintCount = entity.DisplayRowCount;
		Open(entity.StorageEntityName, entity.StorageEntitySubtitle, entity);
	}

	private void Open(string title, string subtitle, IItemSlotOwner owner)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		IsOpen = true;
		((TMP_Text)TitleLabel).text = title;
		((TMP_Text)SubtitleLabel).text = subtitle;
		for (int i = 0; i < SlotsUIs.Length; i++)
		{
			if (owner.ItemSlots.Count > i)
			{
				((Component)SlotsUIs[i]).gameObject.SetActive(true);
				SlotsUIs[i].AssignSlot(owner.ItemSlots[i]);
			}
			else
			{
				SlotsUIs[i].ClearSlot();
				((Component)SlotsUIs[i]).gameObject.SetActive(false);
			}
		}
		int constraintCount = SlotGridLayout.constraintCount;
		CloseButton.anchoredPosition = new Vector2(0f, (float)constraintCount * (0f - SlotGridLayout.cellSize.y) - 60f);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0.06f);
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
		Singleton<UIScreenManager>.Instance.AddScreen(UIScreen);
		if (PlayerSingleton<PlayerInventory>.InstanceExists)
		{
			PlayerSingleton<PlayerInventory>.Instance.AttachToScreen(UIScreen);
		}
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: true);
		Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), owner.ItemSlots.ToList());
		Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
		((Behaviour)Canvas).enabled = true;
		((Component)Container).gameObject.SetActive(true);
	}

	public void Close()
	{
		if ((Object)(object)OpenedStorageEntity != (Object)null)
		{
			OpenedStorageEntity.Close();
		}
		else
		{
			CloseMenu();
		}
	}

	public virtual void CloseMenu()
	{
		IsOpen = false;
		OpenedStorageEntity = null;
		for (int i = 0; i < SlotsUIs.Length; i++)
		{
			SlotsUIs[i].ClearSlot();
			((Component)SlotsUIs[i]).gameObject.SetActive(false);
		}
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
		PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0.06f);
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		if (PlayerSingleton<PlayerInventory>.InstanceExists)
		{
			PlayerSingleton<PlayerInventory>.Instance.DetachFromScreen();
		}
		Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		if (onClosed != null)
		{
			onClosed.Invoke();
		}
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			if ((Object)(object)OpenedStorageEntity != (Object)null)
			{
				OpenedStorageEntity.Close();
			}
			else
			{
				CloseMenu();
			}
		}
	}
}
