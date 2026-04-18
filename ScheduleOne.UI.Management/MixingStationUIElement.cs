using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.Management;

public class MixingStationUIElement : WorldspaceUIElement
{
	public MixingStation AssignedStation { get; protected set; }

	public void Initialize(MixingStation station)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedStation = station;
		AssignedStation.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		MixingStationConfiguration mixingStationConfiguration = AssignedStation.Configuration as MixingStationConfiguration;
		((TMP_Text)TitleLabel).text = AssignedStation.GetManagementName();
		SetAssignedNPC(mixingStationConfiguration.AssignedChemist.SelectedNPC);
	}
}
