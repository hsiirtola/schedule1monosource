using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.UI.Stations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.Management;

public class ChemistryStationUIElement : WorldspaceUIElement
{
	[Header("References")]
	public StationRecipeEntry RecipeEntry;

	public GameObject NoRecipe;

	public ChemistryStation AssignedStation { get; protected set; }

	public void Initialize(ChemistryStation oven)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedStation = oven;
		AssignedStation.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		ChemistryStationConfiguration chemistryStationConfiguration = AssignedStation.Configuration as ChemistryStationConfiguration;
		SetAssignedNPC(chemistryStationConfiguration.AssignedChemist.SelectedNPC);
		((TMP_Text)TitleLabel).text = AssignedStation.GetManagementName();
		if ((Object)(object)chemistryStationConfiguration.Recipe.SelectedRecipe != (Object)null)
		{
			RecipeEntry.AssignRecipe(chemistryStationConfiguration.Recipe.SelectedRecipe);
			((Component)RecipeEntry).gameObject.SetActive(true);
			NoRecipe.SetActive(false);
		}
		else
		{
			((Component)RecipeEntry).gameObject.SetActive(false);
			NoRecipe.SetActive(true);
		}
	}
}
