using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI.Management;

public class BrickPressUIElement : WorldspaceUIElement
{
	public BrickPress AssignedPress { get; protected set; }

	public void Initialize(BrickPress press)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedPress = press;
		AssignedPress.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		BrickPressConfiguration brickPressConfiguration = AssignedPress.Configuration as BrickPressConfiguration;
		((TMP_Text)TitleLabel).text = AssignedPress.GetManagementName();
		SetAssignedNPC(brickPressConfiguration.AssignedPackager.SelectedNPC);
	}
}
