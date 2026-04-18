using ScheduleOne.Management;
using ScheduleOne.StationFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.Management;

public class SpawnStationUIElement : WorldspaceUIElement
{
	public MushroomSpawnStation AssignedStation { get; protected set; }

	public void Initialize(MushroomSpawnStation pack)
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
		SpawnStationConfiguration spawnStationConfiguration = AssignedStation.Configuration as SpawnStationConfiguration;
		((TMP_Text)TitleLabel).text = AssignedStation.GetManagementName();
		SetAssignedNPC(spawnStationConfiguration.AssignedBotanist.SelectedNPC);
	}
}
