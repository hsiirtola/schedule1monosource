using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.Management;

public class CauldronUIElement : WorldspaceUIElement
{
	public Cauldron AssignedCauldron { get; protected set; }

	public void Initialize(Cauldron cauldron)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedCauldron = cauldron;
		AssignedCauldron.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		CauldronConfiguration cauldronConfiguration = AssignedCauldron.Configuration as CauldronConfiguration;
		((TMP_Text)TitleLabel).text = AssignedCauldron.GetManagementName();
		SetAssignedNPC(cauldronConfiguration.AssignedChemist.SelectedNPC);
	}
}
