using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Stations;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.Management;

public class StationRecipeFieldUI : MonoBehaviour
{
	[Header("References")]
	public StationRecipeEntry RecipeEntry;

	public GameObject None;

	public GameObject Mixed;

	public GameObject ClearButton;

	public List<StationRecipeField> Fields { get; protected set; } = new List<StationRecipeField>();

	public void Bind(List<StationRecipeField> field)
	{
		Fields = new List<StationRecipeField>();
		Fields.AddRange(field);
		Fields[Fields.Count - 1].onRecipeChanged.AddListener((UnityAction<StationRecipe>)Refresh);
		Refresh(Fields[0].SelectedRecipe);
	}

	private void Refresh(StationRecipe newVal)
	{
		None.gameObject.SetActive(false);
		Mixed.gameObject.SetActive(false);
		ClearButton.gameObject.SetActive(false);
		((Component)RecipeEntry).gameObject.SetActive(false);
		if (AreFieldsUniform())
		{
			if ((Object)(object)newVal != (Object)null)
			{
				ClearButton.gameObject.SetActive(true);
				RecipeEntry.AssignRecipe(newVal);
				((Component)RecipeEntry).gameObject.SetActive(true);
			}
			else
			{
				None.SetActive(true);
			}
		}
		else
		{
			Mixed.gameObject.SetActive(true);
			ClearButton.gameObject.SetActive(true);
		}
		ClearButton.gameObject.SetActive(false);
	}

	private bool AreFieldsUniform()
	{
		for (int i = 0; i < Fields.Count - 1; i++)
		{
			if ((Object)(object)Fields[i].SelectedRecipe != (Object)(object)Fields[i + 1].SelectedRecipe)
			{
				return false;
			}
		}
		return true;
	}

	public void Clicked()
	{
		bool num = AreFieldsUniform();
		StationRecipe selectedOption = null;
		if (num)
		{
			selectedOption = Fields[0].SelectedRecipe;
		}
		List<StationRecipe> options = Fields[0].Options.Where((StationRecipe x) => x.Unlocked).ToList();
		Singleton<ManagementInterface>.Instance.RecipeSelectorScreen.Initialize("Select Recipe", options, selectedOption, OptionSelected);
		Singleton<ManagementInterface>.Instance.RecipeSelectorScreen.Open();
	}

	private void OptionSelected(StationRecipe option)
	{
		foreach (StationRecipeField field in Fields)
		{
			field.SetRecipe(option, network: true);
		}
	}

	public void ClearClicked()
	{
		foreach (StationRecipeField field in Fields)
		{
			field.SetRecipe(null, network: true);
		}
	}
}
