using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class BotanistUIElement : WorldspaceUIElement
{
	[Header("References")]
	public Image SupplyIcon;

	public GameObject NoSupply;

	public TextMeshProUGUI SupplyLabel;

	public RectTransform[] PotRects;

	public Botanist AssignedBotanist { get; protected set; }

	public void Initialize(Botanist bot)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedBotanist = bot;
		AssignedBotanist.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		((TMP_Text)TitleLabel).text = bot.fullName;
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		BotanistConfiguration botanistConfiguration = AssignedBotanist.Configuration as BotanistConfiguration;
		NoSupply.gameObject.SetActive((Object)(object)botanistConfiguration.Supplies.SelectedObject == (Object)null);
		if ((Object)(object)botanistConfiguration.Supplies.SelectedObject != (Object)null)
		{
			SupplyIcon.sprite = ((BaseItemInstance)botanistConfiguration.Supplies.SelectedObject.ItemInstance).Icon;
			((Component)SupplyIcon).gameObject.SetActive(true);
		}
		else
		{
			((Component)SupplyIcon).gameObject.SetActive(false);
		}
		for (int i = 0; i < PotRects.Length; i++)
		{
			if (botanistConfiguration.Assigns.SelectedObjects.Count > i)
			{
				((Component)((Transform)PotRects[i]).Find("Icon")).GetComponent<Image>().sprite = ((BaseItemInstance)botanistConfiguration.Assigns.SelectedObjects[i].ItemInstance).Icon;
				((Component)((Transform)PotRects[i]).Find("Icon")).gameObject.SetActive(true);
			}
			else
			{
				((Component)((Transform)PotRects[i]).Find("Icon")).gameObject.SetActive(false);
			}
		}
	}
}
