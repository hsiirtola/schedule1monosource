using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Product;
using ScheduleOne.UI.Tooltips;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.ProductManagerApp;

public class ProductAppDetailPanel : MonoBehaviour
{
	public Color AddictionColor_Min;

	public Color AddictionColor_Max;

	[Header("References")]
	public GameObject NothingSelected;

	public GameObject Container;

	public Text NameLabel;

	public InputField ValueLabel;

	public Text SuggestedPriceLabel;

	public Toggle ListedForSale;

	public Text DescLabel;

	public Text[] PropertyLabels;

	public RectTransform Listed;

	public RectTransform Delisted;

	public RectTransform NotDiscovered;

	public RectTransform RecipesLabel;

	public RectTransform[] RecipeEntries;

	public VerticalLayoutGroup LayoutGroup;

	public Scrollbar AddictionSlider;

	public Text AddictionLabel;

	public ScrollRect ScrollRect;

	public ProductDefinition ActiveProduct { get; protected set; }

	public void Awake()
	{
		((UnityEvent<bool>)(object)ListedForSale.onValueChanged).AddListener((UnityAction<bool>)delegate
		{
			ListingToggled();
		});
		((UnityEvent<string>)(object)ValueLabel.onEndEdit).AddListener((UnityAction<string>)delegate(string value)
		{
			PriceSubmitted(value);
		});
	}

	public void SetActiveProduct(ProductDefinition productDefinition)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		ActiveProduct = productDefinition;
		bool flag = ProductManager.DiscoveredProducts.Contains(productDefinition);
		if ((Object)(object)ActiveProduct != (Object)null)
		{
			NameLabel.text = ((BaseItemDefinition)productDefinition).Name;
			SuggestedPriceLabel.text = "Suggested: " + MoneyManager.FormatAmount(productDefinition.MarketValue);
			UpdatePrice();
			if (flag)
			{
				DescLabel.text = ((BaseItemDefinition)productDefinition).Description;
			}
			else
			{
				DescLabel.text = "???";
			}
			for (int i = 0; i < PropertyLabels.Length; i++)
			{
				if (productDefinition.Properties.Count > i)
				{
					PropertyLabels[i].text = "•  " + productDefinition.Properties[i].Name;
					((Graphic)PropertyLabels[i]).color = productDefinition.Properties[i].LabelColor;
					((Component)PropertyLabels[i]).gameObject.SetActive(true);
				}
				else
				{
					((Component)PropertyLabels[i]).gameObject.SetActive(false);
				}
			}
			for (int j = 0; j < RecipeEntries.Length; j++)
			{
				if (productDefinition.Recipes.Count > j)
				{
					((Component)RecipeEntries[j]).gameObject.SetActive(true);
					if (productDefinition.Recipes[j].Ingredients[0].Item is ProductDefinition)
					{
						((Component)((Transform)RecipeEntries[j]).Find("Product")).GetComponent<Image>().sprite = ((BaseItemDefinition)productDefinition.Recipes[j].Ingredients[0].Item).Icon;
						((Component)((Transform)RecipeEntries[j]).Find("Product")).GetComponent<Tooltip>().text = ((BaseItemDefinition)productDefinition.Recipes[j].Ingredients[0].Item).Name;
						((Component)((Transform)RecipeEntries[j]).Find("Mixer")).GetComponent<Image>().sprite = ((BaseItemDefinition)productDefinition.Recipes[j].Ingredients[1].Item).Icon;
						((Component)((Transform)RecipeEntries[j]).Find("Mixer")).GetComponent<Tooltip>().text = ((BaseItemDefinition)productDefinition.Recipes[j].Ingredients[1].Item).Name;
					}
					else
					{
						((Component)((Transform)RecipeEntries[j]).Find("Product")).GetComponent<Image>().sprite = ((BaseItemDefinition)productDefinition.Recipes[j].Ingredients[1].Item).Icon;
						((Component)((Transform)RecipeEntries[j]).Find("Product")).GetComponent<Tooltip>().text = ((BaseItemDefinition)productDefinition.Recipes[j].Ingredients[1].Item).Name;
						((Component)((Transform)RecipeEntries[j]).Find("Mixer")).GetComponent<Image>().sprite = ((BaseItemDefinition)productDefinition.Recipes[j].Ingredients[0].Item).Icon;
						((Component)((Transform)RecipeEntries[j]).Find("Mixer")).GetComponent<Tooltip>().text = ((BaseItemDefinition)productDefinition.Recipes[j].Ingredients[0].Item).Name;
					}
					((Component)((Transform)RecipeEntries[j]).Find("Output")).GetComponent<Image>().sprite = ((BaseItemDefinition)productDefinition).Icon;
					((Component)((Transform)RecipeEntries[j]).Find("Output")).GetComponent<Tooltip>().text = ((BaseItemDefinition)productDefinition).Name;
				}
				else
				{
					((Component)RecipeEntries[j]).gameObject.SetActive(false);
				}
			}
			((Component)RecipesLabel).gameObject.SetActive(productDefinition.Recipes.Count > 0);
			NothingSelected.gameObject.SetActive(false);
			Container.gameObject.SetActive(true);
			AddictionSlider.value = productDefinition.GetAddictiveness();
			AddictionLabel.text = Mathf.FloorToInt(productDefinition.GetAddictiveness() * 100f) + "%";
			((Graphic)AddictionLabel).color = Color.Lerp(AddictionColor_Min, AddictionColor_Max, productDefinition.GetAddictiveness());
			ContentSizeFitter[] componentsInChildren = ((Component)this).GetComponentsInChildren<ContentSizeFitter>();
			for (int k = 0; k < componentsInChildren.Length; k++)
			{
				((Behaviour)componentsInChildren[k]).enabled = false;
				((Behaviour)componentsInChildren[k]).enabled = true;
			}
			((Behaviour)LayoutGroup).enabled = false;
			((Behaviour)LayoutGroup).enabled = true;
			LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)LayoutGroup).GetComponent<RectTransform>());
			((Behaviour)ScrollRect).enabled = false;
			((Behaviour)ScrollRect).enabled = true;
			ScrollRect.verticalNormalizedPosition = 1f;
			LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)ScrollRect).GetComponent<RectTransform>());
		}
		else
		{
			NothingSelected.gameObject.SetActive(true);
			Container.gameObject.SetActive(false);
		}
		UpdateListed();
	}

	private void Update()
	{
		if (PlayerSingleton<ProductManagerApp>.Instance.isOpen)
		{
			UpdateListed();
		}
	}

	private void UpdateListed()
	{
		ListedForSale.SetIsOnWithoutNotify(ProductManager.ListedProducts.Contains(ActiveProduct));
	}

	private void UpdatePrice()
	{
		ValueLabel.SetTextWithoutNotify(NetworkSingleton<ProductManager>.Instance.GetPrice(ActiveProduct).ToString());
	}

	private void ListingToggled()
	{
		if (NetworkSingleton<ProductManager>.InstanceExists && !((Object)(object)ActiveProduct == (Object)null))
		{
			if (ProductManager.ListedProducts.Contains(ActiveProduct))
			{
				NetworkSingleton<ProductManager>.Instance.SetProductListed(((BaseItemDefinition)ActiveProduct).ID, listed: false);
			}
			else
			{
				NetworkSingleton<ProductManager>.Instance.SetProductListed(((BaseItemDefinition)ActiveProduct).ID, listed: true);
			}
			Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
			UpdateListed();
		}
	}

	private void PriceSubmitted(string value)
	{
		if (NetworkSingleton<ProductManager>.InstanceExists && PlayerSingleton<ProductManagerApp>.Instance.isOpen && PlayerSingleton<Phone>.Instance.IsOpen && !((Object)(object)ActiveProduct == (Object)null))
		{
			if (float.TryParse(value, out var result))
			{
				NetworkSingleton<ProductManager>.Instance.SendPrice(((BaseItemDefinition)ActiveProduct).ID, result);
				Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
			}
			UpdatePrice();
		}
	}
}
