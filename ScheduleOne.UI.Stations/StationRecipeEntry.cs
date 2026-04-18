using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.ItemFramework;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Stations;

public class StationRecipeEntry : MonoBehaviour
{
	public static Color ValidColor = Color.white;

	public static Color InvalidColor = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)80, (byte)80, byte.MaxValue));

	public Button Button;

	public Image Icon;

	public TextMeshProUGUI TitleLabel;

	public TextMeshProUGUI CookingTimeLabel;

	public RectTransform[] IngredientRects;

	private TextMeshProUGUI[] IngredientQuantities;

	public bool IsValid { get; private set; }

	public StationRecipe Recipe { get; private set; }

	public void AssignRecipe(StationRecipe recipe)
	{
		Recipe = recipe;
		Icon.sprite = ((BaseItemDefinition)recipe.Product.Item).Icon;
		((TMP_Text)TitleLabel).text = recipe.RecipeTitle;
		if (recipe.Product.Quantity > 1)
		{
			((TMP_Text)TitleLabel).text = ((TMP_Text)TitleLabel).text + "(" + recipe.Product.Quantity + "x)";
		}
		((Component)Icon).GetComponent<ItemDefinitionInfoHoverable>().AssignedItem = recipe.Product.Item;
		int num = recipe.CookTime_Mins / 60;
		int num2 = recipe.CookTime_Mins % 60;
		((TMP_Text)CookingTimeLabel).text = $"{num}h";
		if (num2 > 0)
		{
			TextMeshProUGUI cookingTimeLabel = CookingTimeLabel;
			((TMP_Text)cookingTimeLabel).text = ((TMP_Text)cookingTimeLabel).text + $" {num2}m";
		}
		IngredientQuantities = (TextMeshProUGUI[])(object)new TextMeshProUGUI[IngredientRects.Length];
		for (int i = 0; i < IngredientRects.Length; i++)
		{
			if (i < recipe.Ingredients.Count)
			{
				((Component)((Transform)IngredientRects[i]).Find("Icon")).GetComponent<Image>().sprite = ((BaseItemDefinition)recipe.Ingredients[i].Item).Icon;
				IngredientQuantities[i] = ((Component)((Transform)IngredientRects[i]).Find("Quantity")).GetComponent<TextMeshProUGUI>();
				((TMP_Text)IngredientQuantities[i]).text = recipe.Ingredients[i].Quantity + "x";
				((Component)IngredientRects[i]).GetComponent<ItemDefinitionInfoHoverable>().AssignedItem = recipe.Ingredients[i].Item;
				((Component)IngredientRects[i]).gameObject.SetActive(true);
			}
			else
			{
				((Component)IngredientRects[i]).gameObject.SetActive(false);
			}
		}
	}

	public void RefreshValidity(List<ItemInstance> ingredients)
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (!Recipe.Unlocked)
		{
			IsValid = false;
			((Component)this).gameObject.SetActive(false);
			return;
		}
		IsValid = true;
		for (int i = 0; i < Recipe.Ingredients.Count; i++)
		{
			List<ItemInstance> list = new List<ItemInstance>();
			foreach (ItemDefinition ingredientVariant in Recipe.Ingredients[i].Items)
			{
				List<ItemInstance> collection = ingredients.Where((ItemInstance x) => ((BaseItemInstance)x).ID == ((BaseItemDefinition)ingredientVariant).ID).ToList();
				list.AddRange(collection);
			}
			int num = 0;
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				num += ((BaseItemInstance)list[num2]).Quantity;
			}
			if (num >= Recipe.Ingredients[i].Quantity)
			{
				((Graphic)IngredientQuantities[i]).color = ValidColor;
				continue;
			}
			((Graphic)IngredientQuantities[i]).color = InvalidColor;
			IsValid = false;
		}
		((Component)this).gameObject.SetActive(true);
		((Selectable)Button).interactable = IsValid;
	}

	public float GetIngredientsMatchDelta(List<ItemInstance> ingredients)
	{
		int num = Recipe.Ingredients.Sum((StationRecipe.IngredientQuantity x) => x.Quantity);
		int num2 = 0;
		for (int num3 = 0; num3 < Recipe.Ingredients.Count; num3++)
		{
			List<ItemInstance> list = new List<ItemInstance>();
			foreach (ItemDefinition ingredientVariant in Recipe.Ingredients[num3].Items)
			{
				List<ItemInstance> collection = ingredients.Where((ItemInstance x) => ((BaseItemInstance)x).ID == ((BaseItemDefinition)ingredientVariant).ID).ToList();
				list.AddRange(collection);
			}
			int num4 = 0;
			for (int num5 = 0; num5 < list.Count; num5++)
			{
				num4 += ((BaseItemInstance)list[num5]).Quantity;
			}
			num2 += Mathf.Min(num4, Recipe.Ingredients[num3].Quantity);
		}
		return (float)num2 / (float)num;
	}
}
