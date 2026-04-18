using FishNet.Serializing.Helping;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Stations;
using UnityEngine;

namespace ScheduleOne.ObjectScripts;

public class ChemistryCookOperation
{
	[CodegenExclude]
	private StationRecipe recipe;

	public string RecipeID;

	public EQuality ProductQuality;

	public Color StartLiquidColor;

	public float LiquidLevel;

	public int CurrentTime;

	[CodegenExclude]
	public StationRecipe Recipe
	{
		get
		{
			if ((Object)(object)recipe == (Object)null)
			{
				recipe = Singleton<ChemistryStationCanvas>.Instance.Recipes.Find((StationRecipe r) => r.RecipeID == RecipeID);
			}
			return recipe;
		}
	}

	public ChemistryCookOperation(StationRecipe recipe, EQuality productQuality, Color startLiquidColor, float liquidLevel, int currentTime = 0)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		RecipeID = recipe.RecipeID;
		ProductQuality = productQuality;
		StartLiquidColor = startLiquidColor;
		LiquidLevel = liquidLevel;
		CurrentTime = currentTime;
	}

	public ChemistryCookOperation(string recipeID, EQuality productQuality, Color startLiquidColor, float liquidLevel, int currentTime = 0)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		RecipeID = recipeID;
		ProductQuality = productQuality;
		StartLiquidColor = startLiquidColor;
		LiquidLevel = liquidLevel;
		CurrentTime = currentTime;
	}

	public ChemistryCookOperation()
	{
	}

	public void Progress(int mins)
	{
		CurrentTime += mins;
		_ = CurrentTime;
		_ = Recipe.CookTime_Mins;
	}

	public bool IsComplete()
	{
		return CurrentTime >= Recipe.CookTime_Mins;
	}
}
