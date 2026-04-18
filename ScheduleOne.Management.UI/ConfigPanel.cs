using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Management.UI;

public class ConfigPanel : MonoBehaviour
{
	public void Bind(List<EntityConfiguration> configs, UIScreen screen = null)
	{
		BindInternal(configs);
		ConfigureScreen(screen);
	}

	protected virtual void BindInternal(List<EntityConfiguration> configs)
	{
	}

	private void ConfigureScreen(UIScreen screen)
	{
		if (!((Object)(object)screen == (Object)null))
		{
			UISelectable[] componentsInChildren = ((Component)this).gameObject.GetComponentsInChildren<UISelectable>(true);
			UIContentPanel uIContentPanel = ((Component)this).gameObject.GetComponent<UIContentPanel>();
			if (!Object.op_Implicit((Object)(object)uIContentPanel))
			{
				uIContentPanel = ((Component)this).gameObject.AddComponent<UIContentPanel>();
			}
			uIContentPanel.EnableSideNavigation(enabled: false);
			screen.AddPanel(uIContentPanel);
			UISelectable[] array = componentsInChildren;
			foreach (UISelectable selectable in array)
			{
				uIContentPanel.AddSelectable(selectable);
			}
			screen.SetCurrentSelectedPanel(uIContentPanel);
		}
	}
}
