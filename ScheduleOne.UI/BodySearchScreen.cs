using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Items;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class BodySearchScreen : Singleton<BodySearchScreen>
{
	public const float MAX_SPEED_BOOST = 2.5f;

	public Color SlotRedColor = new Color(1f, 0f, 0f, 0.5f);

	public Color SlotHighlightRedColor = new Color(1f, 0f, 0f, 0.5f);

	public float GapTime = 0.2f;

	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public RectTransform MinigameController;

	public RectTransform SlotContainer;

	public ItemSlotUI ItemSlotPrefab;

	public RectTransform SearchIndicator;

	public RectTransform SearchIndicatorStart;

	public RectTransform SearchIndicatorEnd;

	public Animation IndicatorAnimation;

	public Animation TutorialAnimation;

	public RectTransform TutorialContainer;

	public Animation ResetAnimation;

	public AudioSourceController FailSound;

	private List<ItemSlotUI> slots = new List<ItemSlotUI>();

	public UnityEvent onSearchClear;

	public UnityEvent onSearchFail;

	private Color defaultSlotColor = new Color(0f, 0f, 0f, 0f);

	private Color defaultSlotHighlightColor = new Color(0f, 0f, 0f, 0f);

	private ItemSlotUI concealedSlot;

	private ItemSlotUI hoveredSlot;

	private Color[] defaultItemIconColors;

	private float speedBoost;

	private NPC searcher;

	private bool _caught;

	public bool IsOpen { get; private set; }

	public bool TutorialOpen { get; private set; }

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		if ((Object)(object)Player.Local != (Object)null)
		{
			SetupSlots();
		}
		else
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(SetupSlots));
		}
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
	}

	private void SetupSlots()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(SetupSlots));
		for (int i = 0; i < 8; i++)
		{
			ItemSlotUI slot = Object.Instantiate<ItemSlotUI>(ItemSlotPrefab, (Transform)(object)SlotContainer);
			slot.AssignSlot(PlayerSingleton<PlayerInventory>.Instance.hotbarSlots[i]);
			slots.Add(slot);
			EventTrigger obj = ((Component)slot.Rect).gameObject.AddComponent<EventTrigger>();
			obj.triggers = new List<Entry>();
			Entry val = new Entry();
			val.eventID = (EventTriggerType)2;
			((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
			{
				SlotHeld(slot);
			});
			obj.triggers.Add(val);
			Entry val2 = new Entry();
			val2.eventID = (EventTriggerType)3;
			((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate
			{
				SlotReleased(slot);
			});
			obj.triggers.Add(val2);
		}
		defaultSlotColor = Color32.op_Implicit(slots[0].normalColor);
		defaultSlotHighlightColor = Color32.op_Implicit(slots[0].highlightColor);
	}

	private void Update()
	{
		if ((Object)(object)hoveredSlot != (Object)null)
		{
			hoveredSlot.SetHighlighted((Object)(object)hoveredSlot != (Object)(object)concealedSlot);
		}
		if (IsOpen)
		{
			if (GameInput.GetButton(GameInput.ButtonCode.Jump))
			{
				speedBoost = Mathf.MoveTowards(speedBoost, 2.5f, Time.deltaTime * 6f);
			}
			else
			{
				speedBoost = Mathf.MoveTowards(speedBoost, 0f, Time.deltaTime * 6f);
			}
			if ((Object)(object)Player.Local != (Object)null && Player.Local.CrimeData.CurrentPursuitLevel != PlayerCrimeData.EPursuitLevel.None && !_caught)
			{
				Close(clear: false);
			}
		}
	}

	public void Open(NPC _searcher, float searchTime = 0f)
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		IsOpen = true;
		_caught = false;
		searcher = _searcher;
		Singleton<GameInput>.Instance.ExitAll();
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0f);
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		for (int i = 0; i < slots.Count; i++)
		{
			if (slots[i].assignedSlot.ItemInstance != null && (int)((BaseItemDefinition)slots[i].assignedSlot.ItemInstance.Definition).legalStatus != 0)
			{
				slots[i].SetNormalColor(SlotRedColor);
				slots[i].SetHighlightColor(SlotHighlightRedColor);
			}
			else
			{
				slots[i].SetNormalColor(defaultSlotColor);
				slots[i].SetHighlightColor(defaultSlotHighlightColor);
			}
			slots[i].SetHighlighted(h: false);
		}
		concealedSlot = null;
		((MonoBehaviour)this).StartCoroutine(Search());
		IEnumerator Search()
		{
			_caught = false;
			SearchIndicator.anchoredPosition = SearchIndicatorStart.anchoredPosition;
			((Component)SearchIndicator).GetComponent<CanvasGroup>().alpha = 0f;
			((Behaviour)Canvas).enabled = true;
			((Component)Container).gameObject.SetActive(true);
			yield return (object)new WaitForSeconds(0.5f);
			if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("BodySearchTutorialDone") && GameManager.IS_TUTORIAL)
			{
				searchTime = 8f;
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("BodySearchTutorialDone", true.ToString());
				((Component)MinigameController).gameObject.SetActive(false);
				OpenTutorial();
				yield return (object)new WaitUntil((Func<bool>)(() => !TutorialOpen));
				((Component)MinigameController).gameObject.SetActive(true);
			}
			IndicatorAnimation.Play("Police icon start");
			yield return (object)new WaitForSeconds(0.6f);
			float num = searchTime * GapTime;
			int count = slots.Count;
			float perGap = num / (float)count;
			float num2 = (searchTime - num) / (float)slots.Count;
			float perBlock = perGap + num2;
			for (float i2 = 0f; i2 < searchTime; i2 += Time.deltaTime * (1f + speedBoost))
			{
				float num3 = i2 / searchTime;
				SearchIndicator.anchoredPosition = Vector2.op_Implicit(Vector3.Lerp(Vector2.op_Implicit(SearchIndicatorStart.anchoredPosition), Vector2.op_Implicit(SearchIndicatorEnd.anchoredPosition), num3));
				int num4 = Mathf.FloorToInt(i2 / perBlock);
				if (i2 - (float)num4 * perBlock < perGap)
				{
					if ((Object)(object)hoveredSlot != (Object)null)
					{
						hoveredSlot.SetHighlighted(h: false);
						hoveredSlot = null;
					}
				}
				else
				{
					int index = num4;
					hoveredSlot = slots[index];
					ItemInstance itemInstance = hoveredSlot.assignedSlot.ItemInstance;
					if (!IsSlotConcealed(hoveredSlot) && itemInstance != null && (int)((BaseItemDefinition)itemInstance.Definition).legalStatus != 0)
					{
						_caught = true;
						IndicatorAnimation.Play("Police icon discover");
						FailSound.Play();
						yield return (object)new WaitForSeconds(1f);
						ItemDetected(hoveredSlot);
						if (GameManager.IS_TUTORIAL)
						{
							ResetAnimation.Play();
							yield return (object)new WaitForSeconds(0.55f);
							((MonoBehaviour)this).StartCoroutine(Search());
						}
						else
						{
							Close(clear: false);
						}
						yield break;
					}
				}
				yield return (object)new WaitForEndOfFrame();
			}
			hoveredSlot?.SetHighlighted(h: false);
			hoveredSlot = null;
			IndicatorAnimation.Play("Police icon end");
			yield return (object)new WaitForSeconds(0.3f);
			Close(clear: true);
		}
	}

	private bool IsSlotConcealed(ItemSlotUI slot)
	{
		return (Object)(object)concealedSlot == (Object)(object)slot;
	}

	private void ItemDetected(ItemSlotUI slot)
	{
		if (onSearchFail != null)
		{
			onSearchFail.Invoke();
		}
	}

	public void SlotHeld(ItemSlotUI ui)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		concealedSlot = ui;
		Image[] componentsInChildren = ((Component)ui.ItemContainer).GetComponentsInChildren<Image>();
		defaultItemIconColors = (Color[])(object)new Color[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			defaultItemIconColors[i] = ((Graphic)componentsInChildren[i]).color;
			((Graphic)componentsInChildren[i]).color = Color.black;
		}
	}

	public void SlotReleased(ItemSlotUI ui)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		concealedSlot = null;
		Image[] componentsInChildren = ((Component)ui.ItemContainer).GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Graphic)componentsInChildren[i]).color = defaultItemIconColors[i];
		}
	}

	public void Close(bool clear)
	{
		IsOpen = false;
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0f);
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		if (clear && onSearchClear != null)
		{
			onSearchClear.Invoke();
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
}
