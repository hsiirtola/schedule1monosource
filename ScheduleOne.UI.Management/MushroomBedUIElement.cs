using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class MushroomBedUIElement : WorldspaceUIElement
{
	[Header("References")]
	public Image SpawnIcon;

	public GameObject NoSpawn;

	public Image Additive1Icon;

	public Image Additive2Icon;

	public Image Additive3Icon;

	public MushroomBed AssignedMustroomBed { get; protected set; }

	public void Initialize(MushroomBed bed)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedMustroomBed = bed;
		AssignedMustroomBed.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		MushroomBedConfiguration mushroomBedConfiguration = AssignedMustroomBed.Configuration as MushroomBedConfiguration;
		((TMP_Text)TitleLabel).text = AssignedMustroomBed.GetManagementName();
		NoSpawn.gameObject.SetActive((Object)(object)mushroomBedConfiguration.Spawn.SelectedItem == (Object)null);
		((Component)SpawnIcon).gameObject.SetActive((Object)(object)mushroomBedConfiguration.Spawn.SelectedItem != (Object)null);
		if ((Object)(object)mushroomBedConfiguration.Spawn.SelectedItem != (Object)null)
		{
			SpawnIcon.sprite = ((BaseItemDefinition)mushroomBedConfiguration.Spawn.SelectedItem).Icon;
		}
		if ((Object)(object)mushroomBedConfiguration.Additive1.SelectedItem != (Object)null)
		{
			Additive1Icon.sprite = ((BaseItemDefinition)mushroomBedConfiguration.Additive1.SelectedItem).Icon;
			((Component)Additive1Icon).gameObject.SetActive(true);
		}
		else
		{
			((Component)Additive1Icon).gameObject.SetActive(false);
		}
		if ((Object)(object)mushroomBedConfiguration.Additive2.SelectedItem != (Object)null)
		{
			Additive2Icon.sprite = ((BaseItemDefinition)mushroomBedConfiguration.Additive2.SelectedItem).Icon;
			((Component)Additive2Icon).gameObject.SetActive(true);
		}
		else
		{
			((Component)Additive2Icon).gameObject.SetActive(false);
		}
		if ((Object)(object)mushroomBedConfiguration.Additive3.SelectedItem != (Object)null)
		{
			Additive3Icon.sprite = ((BaseItemDefinition)mushroomBedConfiguration.Additive3.SelectedItem).Icon;
			((Component)Additive3Icon).gameObject.SetActive(true);
		}
		else
		{
			((Component)Additive3Icon).gameObject.SetActive(false);
		}
		SetAssignedNPC(mushroomBedConfiguration.AssignedBotanist.SelectedNPC);
	}
}
