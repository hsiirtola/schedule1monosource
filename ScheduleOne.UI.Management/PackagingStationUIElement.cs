using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.Management;

public class PackagingStationUIElement : WorldspaceUIElement
{
	public PackagingStation AssignedStation { get; protected set; }

	public void Initialize(PackagingStation pack)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedStation = pack;
		AssignedStation.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		PackagingStationConfiguration packagingStationConfiguration = AssignedStation.Configuration as PackagingStationConfiguration;
		((TMP_Text)TitleLabel).text = AssignedStation.GetManagementName();
		SetAssignedNPC(packagingStationConfiguration.AssignedPackager.SelectedNPC);
	}
}
