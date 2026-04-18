using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class PackagerUIElement : WorldspaceUIElement
{
	[Header("References")]
	public RectTransform[] StationRects;

	public Packager AssignedPackager { get; protected set; }

	public void Initialize(Packager packager)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedPackager = packager;
		AssignedPackager.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		((TMP_Text)TitleLabel).text = packager.fullName;
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		PackagerConfiguration packagerConfiguration = AssignedPackager.Configuration as PackagerConfiguration;
		for (int i = 0; i < StationRects.Length; i++)
		{
			if (packagerConfiguration.Stations.SelectedObjects.Count > i)
			{
				((Component)((Transform)StationRects[i]).Find("Icon")).GetComponent<Image>().sprite = ((BaseItemInstance)packagerConfiguration.Stations.SelectedObjects[i].ItemInstance).Icon;
				((Component)((Transform)StationRects[i]).Find("Icon")).gameObject.SetActive(true);
			}
			else
			{
				((Component)((Transform)StationRects[i]).Find("Icon")).gameObject.SetActive(false);
			}
		}
	}
}
