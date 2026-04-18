using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class CleanerUIElement : WorldspaceUIElement
{
	[Header("References")]
	public Image[] StationsIcons;

	public Cleaner AssignedCleaner { get; protected set; }

	public void Initialize(Cleaner cleaner)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedCleaner = cleaner;
		AssignedCleaner.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		((TMP_Text)TitleLabel).text = cleaner.fullName;
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		CleanerConfiguration cleanerConfiguration = AssignedCleaner.Configuration as CleanerConfiguration;
		for (int i = 0; i < StationsIcons.Length; i++)
		{
			if (cleanerConfiguration.Bins.SelectedObjects.Count > i)
			{
				StationsIcons[i].sprite = ((BaseItemInstance)cleanerConfiguration.Bins.SelectedObjects[i].ItemInstance).Icon;
				((Behaviour)StationsIcons[i]).enabled = true;
			}
			else
			{
				((Behaviour)StationsIcons[i]).enabled = false;
			}
		}
	}
}
