using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone;

public class CounterOfferProductSelector : MonoBehaviour
{
	public const int ENTRIES_PER_PAGE = 25;

	public RectTransform Container;

	public InputField SearchBar;

	public RectTransform ProductContainer;

	public Text PageLabel;

	public GameObject ProductEntryPrefab;

	public Action<ProductDefinition> onProductPreviewed;

	public Action<ProductDefinition> onProductSelected;

	[Header("Custom UI")]
	public UIScreen uiSelectionScreen;

	public UIPanel uiSearchPanel;

	public UIPanel uiWindowPanel;

	private List<RectTransform> productEntries = new List<RectTransform>();

	private Dictionary<ProductDefinition, RectTransform> productEntriesDict = new Dictionary<ProductDefinition, RectTransform>();

	private string searchTerm = string.Empty;

	private int pageIndex;

	private int pageCount;

	private List<ProductDefinition> results = new List<ProductDefinition>();

	private ProductDefinition lastPreviewedResult;

	public bool IsOpen { get; private set; }

	public void Awake()
	{
		((UnityEvent<string>)(object)SearchBar.onValueChanged).AddListener((UnityAction<string>)SetSearchTerm);
	}

	public void Open()
	{
		IsOpen = true;
		((Component)Container).gameObject.SetActive(true);
		EnsureAllEntriesExist();
		SetSearchTerm(string.Empty);
		SearchBar.ActivateInputField();
		Singleton<UIScreenManager>.Instance.AddScreen(uiSelectionScreen);
		((MonoBehaviour)this).StartCoroutine(DelaySelectSearchPanel());
	}

	private IEnumerator DelaySelectSearchPanel()
	{
		yield return null;
		uiSelectionScreen.SetCurrentSelectedPanel(uiSearchPanel);
		uiSearchPanel.SelectSelectable(returnFirstFound: true);
	}

	public void Close()
	{
		IsOpen = false;
		((Component)Container).gameObject.SetActive(false);
		Singleton<UIScreenManager>.Instance.RemoveScreen(uiSelectionScreen);
	}

	private void Update()
	{
		if (IsOpen && GameInput.GetButtonDown(GameInput.ButtonCode.Submit) && (Object)(object)lastPreviewedResult != (Object)null)
		{
			ProductSelected(lastPreviewedResult);
		}
	}

	public void SetSearchTerm(string search)
	{
		searchTerm = search.ToLower();
		SearchBar.SetTextWithoutNotify(searchTerm);
		RebuildResultsList();
		if (search != string.Empty && results.Count > 0)
		{
			ProductHovered(results[0]);
		}
	}

	private void RebuildResultsList()
	{
		results = GetMatchingProducts(searchTerm);
		results.Sort(delegate(ProductDefinition a, ProductDefinition b)
		{
			int num = a.DrugType.CompareTo(b.DrugType);
			return (num != 0) ? num : ((BaseItemDefinition)a).Name.CompareTo(((BaseItemDefinition)b).Name);
		});
		Console.Log($"Found {results.Count} results for {searchTerm}");
		pageCount = Mathf.CeilToInt((float)results.Count / 25f);
		SetPage(pageIndex);
	}

	private List<ProductDefinition> GetMatchingProducts(string searchTerm)
	{
		List<ProductDefinition> list = new List<ProductDefinition>();
		List<EDrugType> list2 = new List<EDrugType>();
		foreach (EDrugType value in Enum.GetValues(typeof(EDrugType)))
		{
			if (searchTerm.ToLower().Contains(value.ToString().ToLower()))
			{
				list2.Add(value);
			}
		}
		if (searchTerm.ToLower().Contains("weed"))
		{
			list2.Add(EDrugType.Marijuana);
		}
		if (searchTerm.ToLower().Contains("coke"))
		{
			list2.Add(EDrugType.Cocaine);
		}
		if (searchTerm.ToLower().Contains("meth"))
		{
			list2.Add(EDrugType.Methamphetamine);
		}
		if (searchTerm.ToLower().Contains("shroom"))
		{
			list2.Add(EDrugType.Shrooms);
		}
		foreach (ProductDefinition discoveredProduct in ProductManager.DiscoveredProducts)
		{
			if (list2.Contains(discoveredProduct.DrugType))
			{
				list.Add(discoveredProduct);
			}
			else if (((BaseItemDefinition)discoveredProduct).Name.ToLower().Contains(searchTerm))
			{
				list.Add(discoveredProduct);
			}
		}
		return list;
	}

	private void EnsureAllEntriesExist()
	{
		foreach (ProductDefinition discoveredProduct in ProductManager.DiscoveredProducts)
		{
			if (!productEntriesDict.ContainsKey(discoveredProduct))
			{
				CreateProductEntry(discoveredProduct);
			}
		}
	}

	private void CreateProductEntry(ProductDefinition product)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (!productEntriesDict.ContainsKey(product))
		{
			RectTransform component = Object.Instantiate<GameObject>(ProductEntryPrefab, (Transform)(object)ProductContainer).GetComponent<RectTransform>();
			((Component)((Transform)component).Find("Icon")).GetComponent<Image>().sprite = ((BaseItemDefinition)product).Icon;
			((UnityEvent)((Component)component).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				ProductSelected(product);
			});
			Entry val = new Entry();
			val.eventID = (EventTriggerType)0;
			((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
			{
				ProductHovered(product);
			});
			Entry val2 = new Entry();
			val2.eventID = (EventTriggerType)9;
			((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate
			{
				ProductHovered(product);
			});
			EventTrigger obj = ((Component)component).gameObject.AddComponent<EventTrigger>();
			obj.triggers.Add(val);
			obj.triggers.Add(val2);
			productEntries.Add(component);
			productEntriesDict.Add(product, component);
		}
	}

	public void ChangePage(int change)
	{
		SetPage(pageIndex + change);
	}

	private void SetPage(int page)
	{
		pageIndex = Mathf.Clamp(page, 0, Mathf.Max(pageCount - 1, 0));
		int num = pageIndex * 25;
		int num2 = Mathf.Min(num + 25, results.Count);
		Console.Log($"Page {pageIndex + 1} / {pageCount} ({num} - {num2})");
		List<ProductDefinition> range = results.GetRange(num, num2 - num);
		List<ProductDefinition> list = productEntriesDict.Keys.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			RectTransform val = productEntriesDict[list[i]];
			if (range.Contains(list[i]))
			{
				((Component)val).gameObject.SetActive(true);
				uiWindowPanel.AddSelectable(((Component)val).GetComponent<UISelectable>());
			}
			else
			{
				((Component)val).gameObject.SetActive(false);
				uiWindowPanel.RemoveSelectable(((Component)val).GetComponent<UISelectable>());
			}
		}
		for (int j = 0; j < range.Count; j++)
		{
			((Transform)productEntriesDict[range[j]]).SetSiblingIndex(j);
		}
		PageLabel.text = $"{pageIndex + 1} / {pageCount}";
	}

	private void ProductHovered(ProductDefinition def)
	{
		if (onProductPreviewed != null)
		{
			onProductPreviewed(def);
		}
		lastPreviewedResult = def;
	}

	private void ProductSelected(ProductDefinition def)
	{
		if (onProductSelected != null)
		{
			onProductSelected(def);
		}
		Close();
	}

	public bool IsMouseOverSelector()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		bool flag = RectTransformUtility.RectangleContainsScreenPoint(Container, Vector2.op_Implicit(GameInput.MousePosition), PlayerSingleton<PlayerCamera>.Instance.OverlayCamera);
		Console.Log($"Mouse over selector: {flag}");
		return flag;
	}
}
