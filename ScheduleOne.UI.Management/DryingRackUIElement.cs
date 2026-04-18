using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class DryingRackUIElement : WorldspaceUIElement
{
	public Image TargetQualityIcon;

	public DryingRack AssignedRack { get; protected set; }

	public void Initialize(DryingRack rack)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedRack = rack;
		AssignedRack.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		DryingRackConfiguration dryingRackConfiguration = AssignedRack.Configuration as DryingRackConfiguration;
		((TMP_Text)TitleLabel).text = AssignedRack.GetManagementName();
		EQuality value = dryingRackConfiguration.TargetQuality.Value;
		((Graphic)TargetQualityIcon).color = ItemQuality.GetColor(value);
		SetAssignedNPC(dryingRackConfiguration.AssignedBotanist.SelectedNPC);
	}
}
