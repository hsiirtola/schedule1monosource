using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.Management;

public class LabOvenUIElement : WorldspaceUIElement
{
	public LabOven AssignedOven { get; protected set; }

	public void Initialize(LabOven oven)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedOven = oven;
		AssignedOven.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		LabOvenConfiguration labOvenConfiguration = AssignedOven.Configuration as LabOvenConfiguration;
		((TMP_Text)TitleLabel).text = AssignedOven.GetManagementName();
		SetAssignedNPC(labOvenConfiguration.AssignedChemist.SelectedNPC);
	}
}
