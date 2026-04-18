using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class ChemistUIElement : WorldspaceUIElement
{
	[Header("References")]
	public Image[] StationsIcons;

	public Chemist AssignedChemist { get; protected set; }

	public void Initialize(Chemist chemist)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedChemist = chemist;
		AssignedChemist.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		((TMP_Text)TitleLabel).text = chemist.fullName;
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		ChemistConfiguration chemistConfiguration = AssignedChemist.Configuration as ChemistConfiguration;
		for (int i = 0; i < StationsIcons.Length; i++)
		{
			if (chemistConfiguration.Stations.SelectedObjects.Count > i)
			{
				StationsIcons[i].sprite = ((BaseItemInstance)chemistConfiguration.Stations.SelectedObjects[i].ItemInstance).Icon;
				((Behaviour)StationsIcons[i]).enabled = true;
			}
			else
			{
				((Behaviour)StationsIcons[i]).enabled = false;
			}
		}
	}
}
