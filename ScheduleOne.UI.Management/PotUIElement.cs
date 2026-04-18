using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class PotUIElement : WorldspaceUIElement
{
	[Header("References")]
	public Image SeedIcon;

	public GameObject NoSeed;

	public Image Additive1Icon;

	public Image Additive2Icon;

	public Image Additive3Icon;

	public Pot AssignedPot { get; protected set; }

	public void Initialize(Pot pot)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedPot = pot;
		AssignedPot.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		PotConfiguration potConfiguration = AssignedPot.Configuration as PotConfiguration;
		((TMP_Text)TitleLabel).text = AssignedPot.GetManagementName();
		NoSeed.gameObject.SetActive((Object)(object)potConfiguration.Seed.SelectedItem == (Object)null);
		((Component)SeedIcon).gameObject.SetActive((Object)(object)potConfiguration.Seed.SelectedItem != (Object)null);
		if ((Object)(object)potConfiguration.Seed.SelectedItem != (Object)null)
		{
			SeedIcon.sprite = ((BaseItemDefinition)potConfiguration.Seed.SelectedItem).Icon;
		}
		if ((Object)(object)potConfiguration.Additive1.SelectedItem != (Object)null)
		{
			Additive1Icon.sprite = ((BaseItemDefinition)potConfiguration.Additive1.SelectedItem).Icon;
			((Component)Additive1Icon).gameObject.SetActive(true);
		}
		else
		{
			((Component)Additive1Icon).gameObject.SetActive(false);
		}
		if ((Object)(object)potConfiguration.Additive2.SelectedItem != (Object)null)
		{
			Additive2Icon.sprite = ((BaseItemDefinition)potConfiguration.Additive2.SelectedItem).Icon;
			((Component)Additive2Icon).gameObject.SetActive(true);
		}
		else
		{
			((Component)Additive2Icon).gameObject.SetActive(false);
		}
		if ((Object)(object)potConfiguration.Additive3.SelectedItem != (Object)null)
		{
			Additive3Icon.sprite = ((BaseItemDefinition)potConfiguration.Additive3.SelectedItem).Icon;
			((Component)Additive3Icon).gameObject.SetActive(true);
		}
		else
		{
			((Component)Additive3Icon).gameObject.SetActive(false);
		}
		SetAssignedNPC(potConfiguration.AssignedBotanist.SelectedNPC);
	}
}
