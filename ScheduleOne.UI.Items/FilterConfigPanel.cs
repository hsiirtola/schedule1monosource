using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items;

public class FilterConfigPanel : MonoBehaviour
{
	public class SearchCategory
	{
		public class Item
		{
			public ItemDefinition ItemDefinition;

			public RectTransform Entry;
		}

		public EItemCategory Category;

		public RectTransform Container;

		public List<Item> Items = new List<Item>();

		public void AddItem(ItemDefinition item, RectTransform entry)
		{
			Item item2 = new Item
			{
				ItemDefinition = item,
				Entry = entry
			};
			Items.Add(item2);
		}

		public void SetSearch(string search)
		{
			bool flag = ((object)System.Runtime.CompilerServices.Unsafe.As<EItemCategory, EItemCategory>(ref Category)/*cast due to .constrained prefix*/).ToString().ToLower() == search.ToLower();
			int num = 0;
			for (int i = 0; i < Items.Count; i++)
			{
				if (flag || ((BaseItemDefinition)Items[i].ItemDefinition).Name.ToLower().Contains(search.ToLower()))
				{
					((Component)Items[i].Entry).gameObject.SetActive(true);
					num++;
				}
				else
				{
					((Component)Items[i].Entry).gameObject.SetActive(false);
				}
			}
			((Component)Container).gameObject.SetActive(num > 0);
		}

		public Item GetItem(string itemID)
		{
			for (int i = 0; i < Items.Count; i++)
			{
				if (((BaseItemDefinition)Items[i].ItemDefinition).ID == itemID)
				{
					return Items[i];
				}
			}
			return null;
		}
	}

	public GameObject ItemEntryPrefab;

	public GameObject CategoryPrefab;

	public GameObject SearchItemPrefab;

	[Header("References")]
	public RectTransform Rect;

	public GameObject Container;

	public Button TypeButton_None;

	public Button TypeButton_Whitelist;

	public Button TypeButton_Blacklist;

	public TextMeshProUGUI TypeLabel;

	public TextMeshProUGUI ListLabel;

	public RectTransform ListContainer;

	public GameObject ListBlocker;

	public Button[] QualityButtons;

	public ScrollRect ListScrollRect;

	public RectTransform Dropdown;

	public Button CopyButton;

	public Button PasteButton;

	public Button ApplyToSiblingsButton;

	public Button ClearButton;

	[Header("Search")]
	public RectTransform SearchContainer;

	public TMP_InputField SearchInput;

	public RectTransform CategoryContainer;

	private bool mouseUp;

	private List<SearchCategory> searchCategories = new List<SearchCategory>();

	private List<RectTransform> itemEntries = new List<RectTransform>();

	private static SlotFilter copiedFilter;

	public bool IsOpen { get; private set; }

	public ItemSlot OpenSlot { get; private set; }

	private void Awake()
	{
		GameInput.RegisterExitListener(Exit, 12);
		((UnityEvent<string>)(object)SearchInput.onValueChanged).AddListener((UnityAction<string>)SearchChanged);
		Close();
	}

	private void Start()
	{
		UpdateSearch();
	}

	private void Exit(ExitAction exit)
	{
		if (!exit.Used && IsOpen && exit.exitType == ExitType.Escape)
		{
			exit.Use();
			if (((Component)SearchContainer).gameObject.activeSelf)
			{
				CloseSearch();
			}
			else
			{
				Close();
			}
		}
	}

	private void Update()
	{
		if (!IsOpen || !Input.GetMouseButtonUp(0))
		{
			return;
		}
		if (mouseUp)
		{
			bool num = IsMouseOverPanel();
			bool flag = IsMouseOverSearch();
			bool flag2 = IsMouseOverDropdown() && ((Component)Dropdown).gameObject.activeSelf;
			if ((num || flag) && ((Component)Dropdown).gameObject.activeSelf && !flag2)
			{
				CloseDropdown();
			}
			if (num && !flag && ((Component)SearchContainer).gameObject.activeSelf)
			{
				CloseSearch();
			}
			if (!num && (!((Component)SearchContainer).gameObject.activeSelf || !flag) && !flag2)
			{
				Close();
			}
		}
		else
		{
			mouseUp = true;
		}
	}

	public void Open(ItemSlotUI ui)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		if (ui.assignedSlot == null)
		{
			Console.LogError("ItemSlotUI has no assigned slot! Cannot open filter config panel");
			return;
		}
		IsOpen = true;
		OpenSlot = ui.assignedSlot;
		Container.gameObject.SetActive(true);
		Vector2 val = Vector2.op_Implicit(((Transform)ui.Rect).position) + Vector2.one * ui.Rect.sizeDelta.x / 2f * ((Component)ui.Rect).GetComponentInParent<Canvas>().scaleFactor;
		val += Vector2.right * (Rect.sizeDelta.x / 2f) * ((Component)this).GetComponentInParent<Canvas>().scaleFactor;
		val -= Vector2.up * (Rect.sizeDelta.y / 2f) * ((Component)this).GetComponentInParent<Canvas>().scaleFactor;
		val += Vector2.up * 18f;
		((Transform)Rect).position = Vector2.op_Implicit(val);
		mouseUp = false;
		ItemSlot openSlot = OpenSlot;
		openSlot.onFilterChange = (Action)Delegate.Combine(openSlot.onFilterChange, new Action(RefreshDisplay));
		UpdateSearch();
		RefreshDisplay();
		((MonoBehaviour)this).StartCoroutine(Open());
		IEnumerator Open()
		{
			ListScrollRect.verticalNormalizedPosition = 1f;
			yield return (object)new WaitForEndOfFrame();
			ListScrollRect.verticalNormalizedPosition = 1f;
		}
	}

	public void Close()
	{
		IsOpen = false;
		if (OpenSlot != null)
		{
			ItemSlot openSlot = OpenSlot;
			openSlot.onFilterChange = (Action)Delegate.Remove(openSlot.onFilterChange, new Action(RefreshDisplay));
			OpenSlot = null;
		}
		for (int i = 0; i < itemEntries.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)itemEntries[i]).gameObject);
		}
		itemEntries.Clear();
		CloseSearch();
		CloseDropdown();
		Container.gameObject.SetActive(false);
	}

	private void UpdateSearch()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		foreach (ItemDefinition item in Singleton<Registry>.Instance.GetAllItems())
		{
			if (!((BaseItemDefinition)item).UsableInFilters)
			{
				continue;
			}
			SearchCategory searchCategory = GetSearchCategory(((BaseItemDefinition)item).Category);
			if (searchCategory.GetItem(((BaseItemDefinition)item).ID) == null)
			{
				RectTransform component = Object.Instantiate<GameObject>(SearchItemPrefab, (Transform)(object)searchCategory.Container).GetComponent<RectTransform>();
				((Component)((Transform)component).Find("Icon")).GetComponent<Image>().sprite = ((BaseItemDefinition)item).Icon;
				((Component)component).GetComponent<Tooltip>().text = ((BaseItemDefinition)item).Name;
				((UnityEvent)((Component)component).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
				{
					ItemClicked(((BaseItemDefinition)item).ID);
				});
				searchCategory.AddItem(item, component);
			}
		}
		foreach (SearchCategory searchCategory2 in searchCategories)
		{
			searchCategory2.Items.Sort((SearchCategory.Item a, SearchCategory.Item b) => ((BaseItemDefinition)a.ItemDefinition).Name.CompareTo(((BaseItemDefinition)b.ItemDefinition).Name));
			for (int num = 0; num < searchCategory2.Items.Count; num++)
			{
				((Transform)searchCategory2.Items[num].Entry).SetSiblingIndex(num + 1);
			}
		}
	}

	public void FilterModeSelected(int filterType)
	{
		FilterModeSelected((SlotFilter.EType)filterType);
	}

	public void FilterModeSelected(SlotFilter.EType filterType)
	{
		SlotFilter playerFilter = OpenSlot.PlayerFilter;
		playerFilter.Type = filterType;
		OpenSlot.SetPlayerFilter(playerFilter);
	}

	public void QualitySelected(int quality)
	{
		QualitySelected((EQuality)quality);
	}

	public void QualitySelected(EQuality quality)
	{
		SlotFilter playerFilter = OpenSlot.PlayerFilter;
		if (playerFilter.AllowedQualities.Contains(quality))
		{
			playerFilter.AllowedQualities.Remove(quality);
		}
		else
		{
			playerFilter.AllowedQualities.Add(quality);
		}
		OpenSlot.SetPlayerFilter(playerFilter);
	}

	public void AddClicked()
	{
		mouseUp = false;
		OpenSearch();
	}

	public void CopyClicked()
	{
		mouseUp = false;
		copiedFilter = OpenSlot.PlayerFilter.Clone();
		GUIUtility.systemCopyBuffer = JsonUtility.ToJson((object)copiedFilter, false);
		CloseDropdown();
	}

	public void PasteClicked()
	{
		mouseUp = false;
		OpenSlot.SetPlayerFilter(copiedFilter);
		CloseDropdown();
	}

	public void ApplyToSiblingsClicked()
	{
		mouseUp = false;
		foreach (ItemSlot slot in OpenSlot.SiblingSet.Slots)
		{
			if (slot != OpenSlot)
			{
				slot.SetPlayerFilter(OpenSlot.PlayerFilter.Clone());
			}
		}
		CloseDropdown();
	}

	public void ClearClicked()
	{
		mouseUp = false;
		OpenSlot.SetPlayerFilter(new SlotFilter());
		CloseDropdown();
	}

	public void ToggleDropdown()
	{
		if (((Component)Dropdown).gameObject.activeSelf)
		{
			CloseDropdown();
		}
		else
		{
			OpenDropdown();
		}
	}

	public void OpenDropdown()
	{
		mouseUp = false;
		CloseSearch();
		string systemCopyBuffer = GUIUtility.systemCopyBuffer;
		if (!string.IsNullOrEmpty(systemCopyBuffer))
		{
			try
			{
				SlotFilter slotFilter = JsonUtility.FromJson<SlotFilter>(systemCopyBuffer);
				if (slotFilter != null)
				{
					copiedFilter = slotFilter;
				}
			}
			catch
			{
				Console.Log("Failed to parse clipboard text as SlotFilter JSON!");
			}
		}
		((Selectable)PasteButton).interactable = copiedFilter != null;
		((Component)Dropdown).gameObject.SetActive(true);
	}

	public void CloseDropdown()
	{
		((Component)Dropdown).gameObject.SetActive(false);
	}

	private void ItemClicked(string itemID)
	{
		mouseUp = false;
		if (!Input.GetKey((KeyCode)304))
		{
			CloseSearch();
		}
		AddItem(itemID);
	}

	private void AddItem(string itemID)
	{
		SlotFilter playerFilter = OpenSlot.PlayerFilter;
		if (!playerFilter.ItemIDs.Contains(itemID))
		{
			playerFilter.ItemIDs.Add(itemID);
		}
		OpenSlot.SetPlayerFilter(playerFilter);
	}

	private void RemoveItem(string itemID)
	{
		SlotFilter playerFilter = OpenSlot.PlayerFilter;
		playerFilter.ItemIDs.Remove(itemID);
		OpenSlot.SetPlayerFilter(playerFilter);
	}

	private void RefreshDisplay()
	{
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Expected O, but got Unknown
		((Selectable)TypeButton_None).interactable = OpenSlot.PlayerFilter.Type != SlotFilter.EType.None;
		((Selectable)TypeButton_Whitelist).interactable = OpenSlot.PlayerFilter.Type != SlotFilter.EType.Whitelist;
		((Selectable)TypeButton_Blacklist).interactable = OpenSlot.PlayerFilter.Type != SlotFilter.EType.Blacklist;
		((TMP_Text)TypeLabel).text = OpenSlot.PlayerFilter.Type.ToString();
		if (OpenSlot.PlayerFilter.Type == SlotFilter.EType.Blacklist)
		{
			((TMP_Text)ListLabel).text = "Unallowed Items";
		}
		else
		{
			((TMP_Text)ListLabel).text = "Allowed Items";
		}
		ListBlocker.SetActive(OpenSlot.PlayerFilter.Type == SlotFilter.EType.None);
		for (int i = 0; i < QualityButtons.Length; i++)
		{
			((Component)((Component)QualityButtons[i]).transform.Find("Image")).gameObject.SetActive(OpenSlot.PlayerFilter.AllowedQualities.Contains((EQuality)i));
		}
		if (OpenSlot.PlayerFilter.ItemIDs.Count > 0)
		{
			TextMeshProUGUI listLabel = ListLabel;
			((TMP_Text)listLabel).text = ((TMP_Text)listLabel).text + " (" + OpenSlot.PlayerFilter.ItemIDs.Count + ")";
		}
		for (int j = 0; j < itemEntries.Count; j++)
		{
			Object.Destroy((Object)(object)((Component)itemEntries[j]).gameObject);
		}
		itemEntries.Clear();
		for (int k = 0; k < OpenSlot.PlayerFilter.ItemIDs.Count; k++)
		{
			ItemDefinition item = Registry.GetItem(OpenSlot.PlayerFilter.ItemIDs[k]);
			if ((Object)(object)item == (Object)null)
			{
				Console.LogError("Item with ID " + OpenSlot.PlayerFilter.ItemIDs[k] + " not found!");
				continue;
			}
			RectTransform component = Object.Instantiate<GameObject>(ItemEntryPrefab, (Transform)(object)ListContainer).GetComponent<RectTransform>();
			((Component)((Component)component).transform.Find("Icon")).GetComponent<Image>().sprite = ((BaseItemDefinition)item).Icon;
			((TMP_Text)((Component)((Component)component).transform.Find("Text")).GetComponent<TextMeshProUGUI>()).text = ((BaseItemDefinition)item).Name;
			((UnityEvent)((Component)((Component)component).transform.Find("Remove")).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				RemoveItem(((BaseItemDefinition)item).ID);
			});
			((Component)component).transform.SetSiblingIndex(k);
			itemEntries.Add(component);
		}
	}

	private bool IsMouseOverPanel()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Rect == (Object)null)
		{
			return false;
		}
		Vector2 val = Vector2.op_Implicit(((Transform)Rect).InverseTransformPoint(GameInput.MousePosition));
		Rect rect = Rect.rect;
		return ((Rect)(ref rect)).Contains(val);
	}

	private bool IsMouseOverSearch()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(((Transform)SearchContainer).InverseTransformPoint(GameInput.MousePosition));
		Rect rect = SearchContainer.rect;
		return ((Rect)(ref rect)).Contains(val);
	}

	private bool IsMouseOverDropdown()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(((Transform)Dropdown).InverseTransformPoint(GameInput.MousePosition));
		Rect rect = Dropdown.rect;
		return ((Rect)(ref rect)).Contains(val);
	}

	private unsafe SearchCategory GetSearchCategory(EItemCategory category)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < searchCategories.Count; i++)
		{
			if (searchCategories[i].Category == category)
			{
				return searchCategories[i];
			}
		}
		RectTransform component = Object.Instantiate<GameObject>(CategoryPrefab, (Transform)(object)CategoryContainer).GetComponent<RectTransform>();
		SearchCategory searchCategory = new SearchCategory
		{
			Category = category,
			Container = component
		};
		((TMP_Text)((Component)((Transform)component).Find("Text")).GetComponent<TextMeshProUGUI>()).text = ((object)(*(EItemCategory*)(&category))/*cast due to .constrained prefix*/).ToString();
		searchCategories.Add(searchCategory);
		searchCategories.Sort((SearchCategory a, SearchCategory b) => ((object)System.Runtime.CompilerServices.Unsafe.As<EItemCategory, EItemCategory>(ref a.Category)/*cast due to .constrained prefix*/).ToString().CompareTo(((object)System.Runtime.CompilerServices.Unsafe.As<EItemCategory, EItemCategory>(ref b.Category)/*cast due to .constrained prefix*/).ToString()));
		for (int num = 0; num < searchCategories.Count; num++)
		{
			((Transform)searchCategories[num].Container).SetSiblingIndex(num);
		}
		return searchCategory;
	}

	private void OpenSearch()
	{
		SearchInput.text = "";
		((Component)SearchContainer).gameObject.SetActive(true);
		RefreshSearchResults();
		CloseDropdown();
		((Selectable)SearchInput).Select();
	}

	private void CloseSearch()
	{
		((Component)SearchContainer).gameObject.SetActive(false);
	}

	private void SearchChanged(string search)
	{
		RefreshSearchResults();
	}

	private void RefreshSearchResults()
	{
		foreach (SearchCategory searchCategory in searchCategories)
		{
			searchCategory.SetSearch(SearchInput.text);
		}
	}
}
