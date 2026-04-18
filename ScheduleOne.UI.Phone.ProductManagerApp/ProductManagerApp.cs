using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Product;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.ProductManagerApp;

public class ProductManagerApp : App<ProductManagerApp>
{
	[Serializable]
	public class ProductTypeContainer
	{
		public EDrugType DrugType;

		public RectTransform Container;

		public RectTransform NoneDisplay;

		public void RefreshNoneDisplay()
		{
			((Component)NoneDisplay).gameObject.SetActive(((Transform)Container).childCount == 0);
		}
	}

	[Header("References")]
	public ProductTypeContainer FavouritesContainer;

	public List<ProductTypeContainer> ProductTypeContainers;

	public ProductAppDetailPanel DetailPanel;

	public RectTransform SelectionIndicator;

	public GameObject EntryPrefab;

	private List<ProductEntry> favouriteEntries = new List<ProductEntry>();

	private List<ProductEntry> entries = new List<ProductEntry>();

	private ProductEntry selectedEntry;

	protected override void Awake()
	{
		base.Awake();
		DetailPanel.SetActiveProduct(null);
	}

	protected override void Start()
	{
		base.Start();
		ProductManager productManager = NetworkSingleton<ProductManager>.Instance;
		productManager.onProductDiscovered = (Action<ProductDefinition>)Delegate.Combine(productManager.onProductDiscovered, new Action<ProductDefinition>(CreateEntry));
		ProductManager productManager2 = NetworkSingleton<ProductManager>.Instance;
		productManager2.onProductFavourited = (Action<ProductDefinition>)Delegate.Combine(productManager2.onProductFavourited, new Action<ProductDefinition>(ProductFavourited));
		ProductManager productManager3 = NetworkSingleton<ProductManager>.Instance;
		productManager3.onProductUnfavourited = (Action<ProductDefinition>)Delegate.Combine(productManager3.onProductUnfavourited, new Action<ProductDefinition>(ProductUnfavourited));
		foreach (ProductDefinition favouritedProduct in ProductManager.FavouritedProducts)
		{
			CreateFavouriteEntry(favouritedProduct);
		}
		foreach (ProductDefinition discoveredProduct in ProductManager.DiscoveredProducts)
		{
			CreateEntry(discoveredProduct);
		}
	}

	private void LateUpdate()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (base.isOpen && (Object)(object)selectedEntry != (Object)null)
		{
			((Transform)SelectionIndicator).position = ((Component)selectedEntry).transform.position;
		}
	}

	public virtual void CreateEntry(ProductDefinition definition)
	{
		ProductTypeContainer productTypeContainer = ProductTypeContainers.Find((ProductTypeContainer x) => x.DrugType == definition.DrugTypes[0].DrugType);
		ProductEntry component = Object.Instantiate<GameObject>(EntryPrefab, (Transform)(object)productTypeContainer.Container).GetComponent<ProductEntry>();
		component.Initialize(definition);
		entries.Add(component);
		productTypeContainer.RefreshNoneDisplay();
		LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)this).GetComponent<RectTransform>());
	}

	private void ProductFavourited(ProductDefinition product)
	{
		CreateFavouriteEntry(product);
	}

	private void ProductUnfavourited(ProductDefinition product)
	{
		RemoveFavouriteEntry(product);
	}

	private void CreateFavouriteEntry(ProductDefinition definition)
	{
		if (!((Object)(object)favouriteEntries.Find((ProductEntry x) => (Object)(object)x.Definition == (Object)(object)definition) != (Object)null))
		{
			ProductEntry component = Object.Instantiate<GameObject>(EntryPrefab, (Transform)(object)FavouritesContainer.Container).GetComponent<ProductEntry>();
			component.Initialize(definition);
			favouriteEntries.Add(component);
			FavouritesContainer.RefreshNoneDisplay();
			DelayedRebuildLayout();
		}
	}

	private void RemoveFavouriteEntry(ProductDefinition definition)
	{
		ProductEntry productEntry = favouriteEntries.Find((ProductEntry x) => (Object)(object)x.Definition == (Object)(object)definition);
		if ((Object)(object)selectedEntry == (Object)(object)productEntry)
		{
			selectedEntry = null;
			((Component)SelectionIndicator).gameObject.SetActive(false);
			DetailPanel.SetActiveProduct(null);
		}
		if ((Object)(object)productEntry != (Object)null)
		{
			favouriteEntries.Remove(productEntry);
			productEntry.Destroy();
		}
		FavouritesContainer.RefreshNoneDisplay();
		DelayedRebuildLayout();
	}

	private void DelayedRebuildLayout()
	{
		((MonoBehaviour)this).StartCoroutine(Delay());
		IEnumerator Delay()
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)this).GetComponent<RectTransform>());
			yield return (object)new WaitForEndOfFrame();
			LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)this).GetComponent<RectTransform>());
			ContentSizeFitter[] componentsInChildren = ((Component)this).GetComponentsInChildren<ContentSizeFitter>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Behaviour)componentsInChildren[i]).enabled = false;
				((Behaviour)componentsInChildren[i]).enabled = true;
			}
		}
	}

	public void SelectProduct(ProductEntry entry)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		selectedEntry = entry;
		DetailPanel.SetActiveProduct(entry.Definition);
		((Transform)SelectionIndicator).position = ((Component)entry).transform.position;
		((Component)SelectionIndicator).gameObject.SetActive(true);
	}

	public override void SetOpen(bool open)
	{
		base.SetOpen(open);
		VerticalLayoutGroup[] layoutGroups;
		if (open)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				entries[i].UpdateDiscovered(entries[i].Definition);
				entries[i].UpdateListed();
			}
			for (int j = 0; j < favouriteEntries.Count; j++)
			{
				favouriteEntries[j].UpdateDiscovered(favouriteEntries[j].Definition);
				favouriteEntries[j].UpdateListed();
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)this).GetComponent<RectTransform>());
			((Component)this).gameObject.SetActive(false);
			((Component)this).gameObject.SetActive(true);
			layoutGroups = ((Component)this).GetComponentsInChildren<VerticalLayoutGroup>();
			for (int k = 0; k < layoutGroups.Length; k++)
			{
				((Behaviour)layoutGroups[k]).enabled = false;
				((Behaviour)layoutGroups[k]).enabled = true;
			}
			if ((Object)(object)selectedEntry != (Object)null)
			{
				DetailPanel.SetActiveProduct(selectedEntry.Definition);
			}
			((MonoBehaviour)this).StartCoroutine(Delay());
		}
		IEnumerator Delay()
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)this).GetComponent<RectTransform>());
			for (int l = 0; l < layoutGroups.Length; l++)
			{
				((Behaviour)layoutGroups[l]).enabled = false;
				((Behaviour)layoutGroups[l]).enabled = true;
			}
			yield return (object)new WaitForEndOfFrame();
			LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)this).GetComponent<RectTransform>());
			for (int m = 0; m < layoutGroups.Length; m++)
			{
				((Behaviour)layoutGroups[m]).enabled = false;
				((Behaviour)layoutGroups[m]).enabled = true;
			}
			yield return (object)new WaitForEndOfFrame();
			LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)this).GetComponent<RectTransform>());
			for (int n = 0; n < layoutGroups.Length; n++)
			{
				((Behaviour)layoutGroups[n]).enabled = false;
				((Behaviour)layoutGroups[n]).enabled = true;
			}
		}
	}
}
