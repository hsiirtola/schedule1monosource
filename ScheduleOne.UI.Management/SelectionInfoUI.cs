using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class SelectionInfoUI : MonoBehaviour
{
	[Header("References")]
	public Image Icon;

	public TextMeshProUGUI Title;

	[Header("Settings")]
	public bool SelfUpdate = true;

	public Sprite NonUniformTypeSprite;

	public Sprite CrossSprite;

	private void Update()
	{
		if (((Component)this).gameObject.activeInHierarchy && SelfUpdate)
		{
			List<IConfigurable> list = new List<IConfigurable>();
			list.AddRange(Singleton<ManagementWorldspaceCanvas>.Instance.SelectedConfigurables);
			if (Singleton<ManagementWorldspaceCanvas>.Instance.HoveredConfigurable != null && !list.Contains(Singleton<ManagementWorldspaceCanvas>.Instance.HoveredConfigurable))
			{
				list.Add(Singleton<ManagementWorldspaceCanvas>.Instance.HoveredConfigurable);
			}
			Set(list);
		}
	}

	public void Set(List<IConfigurable> Configurables)
	{
		if (Configurables.Count == 0)
		{
			Icon.sprite = CrossSprite;
			((TMP_Text)Title).text = "Nothing selected";
			return;
		}
		bool flag = true;
		if (Configurables.Count > 1)
		{
			for (int i = 0; i < Configurables.Count - 1; i++)
			{
				if (Configurables[i].ConfigurableType != Configurables[i + 1].ConfigurableType)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			bool flag2 = true;
			if (Configurables.Count > 1)
			{
				for (int j = 0; j < Configurables.Count - 1; j++)
				{
					if (Configurables[j].Configuration.Name.Value != Configurables[j + 1].Configuration.Name.Value)
					{
						flag2 = false;
						break;
					}
				}
			}
			Icon.sprite = Configurables[0].TypeIcon;
			if (flag2)
			{
				((TMP_Text)Title).text = Configurables.Count + "x " + Configurables[0].Configuration.Name.Value;
			}
			else
			{
				((TMP_Text)Title).text = Configurables.Count + "x Mixed names";
			}
		}
		else
		{
			Icon.sprite = NonUniformTypeSprite;
			((TMP_Text)Title).text = Configurables.Count + "x Mixed types";
		}
	}
}
