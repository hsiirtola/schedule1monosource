using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevTools.Extensions;
using GameKit.Utilities;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.UI.Input;
using ScheduleOne.UI.Items;
using ScheduleOne.Vision;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class PickpocketScreen : Singleton<PickpocketScreen>
{
	public const int PICKPOCKET_XP = 2;

	[Header("Settings")]
	public float GreenAreaMaxWidth = 70f;

	public float GreenAreaMinWidth = 5f;

	public float SlideTime = 1f;

	public float SlideTimeMaxMultiplier = 2f;

	public float ValueDivisor = 300f;

	public float Tolerance = 0.01f;

	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public ItemSlotUI[] Slots;

	public RectTransform[] GreenAreas;

	public Animation TutorialAnimation;

	public RectTransform TutorialContainer;

	public RectTransform SliderContainer;

	public Slider Slider;

	public InputPrompt InputPrompt;

	public RectTransform ActionsContainer;

	public UnityEvent onFail;

	public UnityEvent onStop;

	public UnityEvent onHitGreen;

	private NPC npc;

	private bool isSliding;

	private int slideDirection = 1;

	private float sliderPosition;

	private float slideTimeMultiplier = 1f;

	private bool isFail;

	public bool IsOpen { get; private set; }

	public bool TutorialOpen { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		GameInput.RegisterExitListener(Exit, 3);
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
	}

	protected override void Start()
	{
		base.Start();
		Singleton<ItemUIManager>.Instance.AddRaycaster(((Component)Canvas).GetComponent<GraphicRaycaster>());
	}

	public void Open(NPC _npc)
	{
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		IsOpen = true;
		npc = _npc;
		npc.SetIsBeingPickPocketed(pickpocketed: true);
		Singleton<GameInput>.Instance.ExitAll();
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0f);
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: true);
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		Player.Local.VisualState.ApplyState("pickpocketing", EVisualState.Pickpocketing);
		ItemSlot[] array = _npc.Inventory.ItemSlots.ToArray();
		Arrays.Shuffle<ItemSlot>(array);
		for (int i = 0; i < Slots.Length; i++)
		{
			if (i < array.Length)
			{
				Slots[i].AssignSlot(array[i]);
			}
			else
			{
				Slots[i].ClearSlot();
			}
		}
		Singleton<ItemUIManager>.Instance.EnableQuickMove(new List<ItemSlot>(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots()), array.ToList());
		for (int j = 0; j < Slots.Length; j++)
		{
			ItemSlotUI itemSlotUI = Slots[j];
			SetSlotLocked(j, locked: true);
			if (itemSlotUI.assignedSlot == null || itemSlotUI.assignedSlot.Quantity == 0)
			{
				((Component)GreenAreas[j]).gameObject.SetActive(false);
				continue;
			}
			float num = ((BaseItemInstance)itemSlotUI.assignedSlot.ItemInstance).GetMonetaryValue() * (itemSlotUI.assignedSlot.ItemInstance.Definition as StorableItemDefinition).PickpocketDifficultyMultiplier;
			float num2 = Mathf.Lerp(GreenAreaMaxWidth, GreenAreaMinWidth, Mathf.Pow(Mathf.Clamp01(num / ValueDivisor), 0.3f)) / npc.Inventory.PickpocketDifficultyMultiplier;
			RectTransform val = GreenAreas[j];
			val.sizeDelta = new Vector2(num2, val.sizeDelta.y);
			((Component)val).gameObject.SetActive(true);
			val.anchoredPosition = new Vector2(37.5f + 90f * (float)j, val.anchoredPosition.y);
		}
		((Component)ActionsContainer).gameObject.SetActive(true);
		InputPrompt.SetLabel("Stop Arrow");
		isFail = false;
		isSliding = true;
		sliderPosition = 0f;
		slideDirection = 1;
		slideTimeMultiplier = 1f;
		if (!npc.IsConscious)
		{
			for (int k = 0; k < Slots.Length; k++)
			{
				SetSlotLocked(k, locked: false);
			}
		}
		((Behaviour)Canvas).enabled = true;
		((Component)Container).gameObject.SetActive(true);
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			Close();
		}
	}

	private void Update()
	{
		if (!IsOpen || isFail)
		{
			return;
		}
		if (GameInput.GetButtonDown(GameInput.ButtonCode.Jump))
		{
			if (isSliding)
			{
				StopArrow();
			}
			else
			{
				InputPrompt.SetLabel("Stop Arrow");
				isSliding = true;
				if (GetHoveredSlot()?.assignedSlot != null)
				{
					((Component)GreenAreas[ArrayExt.IndexOf<ItemSlotUI>(Slots, GetHoveredSlot())]).gameObject.SetActive(false);
				}
			}
		}
		if (isSliding)
		{
			slideTimeMultiplier = Mathf.Clamp(slideTimeMultiplier + Time.deltaTime / 20f, 0f, SlideTimeMaxMultiplier);
			if (slideDirection == 1)
			{
				sliderPosition = Mathf.Clamp01(sliderPosition + Time.deltaTime / SlideTime * slideTimeMultiplier);
				if (sliderPosition >= 1f)
				{
					slideDirection = -1;
				}
			}
			else
			{
				sliderPosition = Mathf.Clamp01(sliderPosition - Time.deltaTime / SlideTime * slideTimeMultiplier);
				if (sliderPosition <= 0f)
				{
					slideDirection = 1;
				}
			}
		}
		Slider.value = sliderPosition;
	}

	private void StopArrow()
	{
		if (onStop != null)
		{
			onStop.Invoke();
		}
		isSliding = false;
		ItemSlotUI hoveredSlot = GetHoveredSlot();
		InputPrompt.SetLabel("Continue");
		if ((Object)(object)hoveredSlot != (Object)null)
		{
			NetworkSingleton<LevelManager>.Instance.AddXP(2);
			SetSlotLocked(ArrayExt.IndexOf<ItemSlotUI>(Slots, hoveredSlot), locked: false);
			Customer component = ((Component)npc).GetComponent<Customer>();
			if ((Object)(object)component != (Object)null && component.TimeSinceLastDealCompleted < 60 && hoveredSlot.assignedSlot != null && hoveredSlot.assignedSlot.ItemInstance != null && hoveredSlot.assignedSlot.ItemInstance is ProductItemInstance)
			{
				AchievementManager.UnlockAchievement(AchievementManager.EAchievement.INDIAN_DEALER);
			}
			if (onHitGreen != null)
			{
				onHitGreen.Invoke();
			}
		}
		else
		{
			Fail();
		}
	}

	public void SetSlotLocked(int index, bool locked)
	{
		((Component)((Transform)Slots[index].Rect).Find("Locked")).gameObject.SetActive(locked);
		Slots[index].assignedSlot.SetIsAddLocked(locked);
		Slots[index].assignedSlot.SetIsRemovalLocked(locked);
		int num = 0;
		for (int i = 0; i < Slots.Length; i++)
		{
			if (Slots[i].assignedSlot != null && Slots[i].assignedSlot.Quantity != 0 && Slots[i].assignedSlot.IsRemovalLocked)
			{
				num++;
			}
		}
		if (num == 0)
		{
			((Component)ActionsContainer).gameObject.SetActive(false);
		}
	}

	private ItemSlotUI GetHoveredSlot()
	{
		for (int i = 0; i < GreenAreas.Length; i++)
		{
			if (((Component)GreenAreas[i]).gameObject.activeSelf)
			{
				float num = GetGreenAreaNormalizedPosition(i) - GetGreenAreaNormalizedWidth(i) / 2f;
				float num2 = GetGreenAreaNormalizedPosition(i) + GetGreenAreaNormalizedWidth(i) / 2f;
				if (Slider.value >= num - Tolerance && Slider.value <= num2 + Tolerance)
				{
					return Slots[i];
				}
			}
		}
		return null;
	}

	private void Fail()
	{
		isFail = true;
		if (onFail != null)
		{
			onFail.Invoke();
		}
		((MonoBehaviour)this).StartCoroutine(FailCoroutine());
		IEnumerator FailCoroutine()
		{
			yield return (object)new WaitForSeconds(0.9f);
			if (IsOpen)
			{
				Close();
			}
		}
	}

	public void Close()
	{
		IsOpen = false;
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		for (int i = 0; i < Slots.Length; i++)
		{
			if (Slots[i].assignedSlot != null)
			{
				Slots[i].assignedSlot.SetIsRemovalLocked(locked: false);
			}
		}
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0f);
		Player.Local.VisualState.RemoveState("pickpocketing");
		PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		npc.SetIsBeingPickPocketed(pickpocketed: false);
		if (isFail)
		{
			npc.Responses.PlayerFailedPickpocket(Player.Local);
			npc.Inventory.ExpirePickpocket();
		}
	}

	private void OpenTutorial()
	{
		TutorialOpen = true;
		((Component)TutorialContainer).gameObject.SetActive(true);
		TutorialAnimation.Play();
	}

	public void CloseTutorial()
	{
		TutorialOpen = false;
		((Component)TutorialContainer).gameObject.SetActive(false);
	}

	private float GetGreenAreaNormalizedPosition(int index)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return GreenAreas[index].anchoredPosition.x / SliderContainer.sizeDelta.x;
	}

	private float GetGreenAreaNormalizedWidth(int index)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return GreenAreas[index].sizeDelta.x / SliderContainer.sizeDelta.x;
	}
}
