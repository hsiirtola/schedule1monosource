using System;
using System.Collections.Generic;
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

public class ChemistryStationCanvas : Singleton<ChemistryStationCanvas>
{
	public List<StationRecipe> Recipes = new List<StationRecipe>();

	[Header("Prefabs")]
	public StationRecipeEntry RecipeEntryPrefab;

	[Header("References")]
	public Canvas Canvas;

	public UIScreen UIScreen;

	public RectTransform Container;

	public RectTransform InputSlotsContainer;

	public ItemSlotUI[] InputSlotUIs;

	public ItemSlotUI OutputSlotUI;

	public RectTransform RecipeSelectionContainer;

	public TextMeshProUGUI InstructionLabel;

	public Button BeginButton;

	public RectTransform SelectionIndicator;

	public RectTransform RecipeContainer;

	public RectTransform CookingInProgressContainer;

	public StationRecipeEntry InProgressRecipeEntry;

	public TextMeshProUGUI InProgressLabel;

	public TextMeshProUGUI ErrorLabel;

	private List<StationRecipeEntry> recipeEntries = new List<StationRecipeEntry>();

	private StationRecipeEntry selectedRecipe;

	public bool isOpen { get; protected set; }

	public ChemistryStation ChemistryStation { get; protected set; }

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.Awake();
		((UnityEvent)BeginButton.onClick).AddListener(new UnityAction(BeginButtonPressed));
		for (int i = 0; i < Recipes.Count; i++)
		{
			StationRecipeEntry component = ((Component)Object.Instantiate<StationRecipeEntry>(RecipeEntryPrefab, (Transform)(object)RecipeContainer)).GetComponent<StationRecipeEntry>();
			component.AssignRecipe(Recipes[i]);
			recipeEntries.Add(component);
		}
	}

	protected override void Start()
	{
		base.Start();
		Singleton<ItemUIManager>.Instance.AddRaycaster(((Component)Canvas).GetComponent<GraphicRaycaster>());
		Close(removeUI: false);
	}

	protected virtual void Update()
	{
		if (isOpen)
		{
			if (ChemistryStation.CurrentCookOperation != null)
			{
				((Selectable)BeginButton).interactable = ChemistryStation.CurrentCookOperation.CurrentTime >= ChemistryStation.CurrentCookOperation.Recipe.CookTime_Mins;
				((Component)BeginButton).gameObject.SetActive(false);
			}
			else
			{
				((Selectable)BeginButton).interactable = (Object)(object)selectedRecipe != (Object)null && selectedRecipe.IsValid && ChemistryStation.DoesOutputHaveSpace(selectedRecipe.Recipe);
				((Component)BeginButton).gameObject.SetActive(true);
			}
			if (!GameInput.GetCurrentInputDeviceIsGamepad() && ((Selectable)BeginButton).interactable && GameInput.GetButtonDown(GameInput.ButtonCode.Submit))
			{
				BeginButtonPressed();
			}
			UpdateInput();
			UpdateUI();
		}
	}

	private void LateUpdate()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (isOpen && (Object)(object)selectedRecipe != (Object)null)
		{
			((Transform)SelectionIndicator).position = ((Component)selectedRecipe).transform.position;
		}
	}

	private void UpdateUI()
	{
		((Behaviour)ErrorLabel).enabled = false;
		if (ChemistryStation.CurrentCookOperation != null)
		{
			((Component)CookingInProgressContainer).gameObject.SetActive(true);
			((Component)RecipeSelectionContainer).gameObject.SetActive(false);
			if (ChemistryStation.CurrentCookOperation.CurrentTime >= ChemistryStation.CurrentCookOperation.Recipe.CookTime_Mins)
			{
				((TMP_Text)InProgressLabel).text = "Ready to finish";
			}
			else
			{
				((TMP_Text)InProgressLabel).text = "Cooking in progress...";
			}
			if ((Object)(object)InProgressRecipeEntry.Recipe != (Object)(object)ChemistryStation.CurrentCookOperation.Recipe)
			{
				InProgressRecipeEntry.AssignRecipe(ChemistryStation.CurrentCookOperation.Recipe);
			}
		}
		else
		{
			((Component)RecipeSelectionContainer).gameObject.SetActive(true);
			((Component)CookingInProgressContainer).gameObject.SetActive(false);
			if ((Object)(object)selectedRecipe != (Object)null && !ChemistryStation.DoesOutputHaveSpace(selectedRecipe.Recipe))
			{
				((TMP_Text)ErrorLabel).text = "Output slot does not have enough space";
				((Behaviour)ErrorLabel).enabled = true;
			}
		}
	}

	private void UpdateInput()
	{
		if (!((Object)(object)selectedRecipe != (Object)null))
		{
			return;
		}
		if (GameInput.MouseScrollDelta < 0f || GameInput.GetButtonDown(GameInput.ButtonCode.Backward) || Input.GetKeyDown((KeyCode)274))
		{
			if (recipeEntries.IndexOf(selectedRecipe) < recipeEntries.Count - 1)
			{
				StationRecipeEntry stationRecipeEntry = recipeEntries[recipeEntries.IndexOf(selectedRecipe) + 1];
				if (stationRecipeEntry.IsValid)
				{
					SetSelectedRecipe(stationRecipeEntry);
				}
			}
		}
		else if ((GameInput.MouseScrollDelta > 0f || GameInput.GetButtonDown(GameInput.ButtonCode.Forward) || Input.GetKeyDown((KeyCode)273)) && recipeEntries.IndexOf(selectedRecipe) > 0)
		{
			StationRecipeEntry stationRecipeEntry2 = recipeEntries[recipeEntries.IndexOf(selectedRecipe) - 1];
			if (stationRecipeEntry2.IsValid)
			{
				SetSelectedRecipe(stationRecipeEntry2);
			}
		}
	}

	public void Open(ChemistryStation station)
	{
		isOpen = true;
		ChemistryStation = station;
		UpdateUI();
		((Behaviour)Canvas).enabled = true;
		((Component)Container).gameObject.SetActive(true);
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		}
		for (int i = 0; i < station.IngredientSlots.Length; i++)
		{
			InputSlotUIs[i].AssignSlot(station.IngredientSlots[i]);
			ItemSlot obj = station.IngredientSlots[i];
			obj.onItemDataChanged = (Action)Delegate.Combine(obj.onItemDataChanged, new Action(StationSlotsChanged));
		}
		OutputSlotUI.AssignSlot(station.OutputSlot);
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
		list.AddRange(station.IngredientSlots);
		list.Add(station.OutputSlot);
		Singleton<ItemUIManager>.Instance.EnableQuickMove(PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots(), list);
		Singleton<CompassManager>.Instance.SetVisible(visible: false);
		StationSlotsChanged();
	}

	public void Close(bool removeUI)
	{
		isOpen = false;
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		}
		for (int i = 0; i < InputSlotUIs.Length; i++)
		{
			InputSlotUIs[i].ClearSlot();
			if ((Object)(object)ChemistryStation != (Object)null)
			{
				ItemSlot obj = ChemistryStation.IngredientSlots[i];
				obj.onItemDataChanged = (Action)Delegate.Remove(obj.onItemDataChanged, new Action(StationSlotsChanged));
			}
		}
		OutputSlotUI.ClearSlot();
		if (removeUI)
		{
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		}
		Singleton<ItemUIManager>.Instance.SetDraggingEnabled(enabled: false);
		if (PlayerSingleton<PlayerInventory>.InstanceExists)
		{
			PlayerSingleton<PlayerInventory>.Instance.DetachFromScreen();
		}
		Singleton<UIScreenManager>.Instance.RemoveScreen(UIScreen);
		ChemistryStation = null;
	}

	public void BeginButtonPressed()
	{
		if (((Behaviour)BeginButton).isActiveAndEnabled && ((Selectable)BeginButton).interactable)
		{
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			new UseChemistryStationTask(ChemistryStation, selectedRecipe.Recipe);
			Close(removeUI: false);
		}
	}

	private void StationSlotsChanged()
	{
		List<ItemInstance> list = new List<ItemInstance>();
		for (int i = 0; i < InputSlotUIs.Length; i++)
		{
			if (InputSlotUIs[i].assignedSlot.ItemInstance != null)
			{
				list.Add(InputSlotUIs[i].assignedSlot.ItemInstance);
			}
		}
		for (int j = 0; j < recipeEntries.Count; j++)
		{
			recipeEntries[j].RefreshValidity(list);
		}
		SortRecipes(list);
	}

	private void SortRecipes(List<ItemInstance> ingredients)
	{
		Dictionary<StationRecipeEntry, float> recipes = new Dictionary<StationRecipeEntry, float>();
		for (int i = 0; i < recipeEntries.Count; i++)
		{
			float ingredientsMatchDelta = recipeEntries[i].GetIngredientsMatchDelta(ingredients);
			recipes.Add(recipeEntries[i], ingredientsMatchDelta);
		}
		recipeEntries.Sort((StationRecipeEntry a, StationRecipeEntry b) => recipes[b].CompareTo(recipes[a]));
		for (int num = 0; num < recipeEntries.Count; num++)
		{
			((Component)recipeEntries[num]).transform.SetAsLastSibling();
		}
		if (recipeEntries.Count > 0 && recipeEntries[0].IsValid)
		{
			SetSelectedRecipe(recipeEntries[0]);
		}
		else
		{
			SetSelectedRecipe(null);
		}
	}

	private void SetSelectedRecipe(StationRecipeEntry entry)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		selectedRecipe = entry;
		if ((Object)(object)entry != (Object)null)
		{
			((Transform)SelectionIndicator).position = ((Component)entry).transform.position;
			((Component)SelectionIndicator).gameObject.SetActive(true);
		}
		else
		{
			((Component)SelectionIndicator).gameObject.SetActive(false);
		}
	}
}
