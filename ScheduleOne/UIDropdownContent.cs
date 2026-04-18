using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne;

[RequireComponent(typeof(UIContentPanel))]
public class UIDropdownContent : UIScreen
{
	protected override void OnStarted()
	{
		base.OnStarted();
		if (base.Panels.Count == 0)
		{
			Debug.LogError((object)"No panels found in UIDropdownContent. Please add a panel to the dropdown content.");
			return;
		}
		UIPanel uIPanel = base.Panels[0];
		UISelectable[] componentsInChildren = ((Component)this).GetComponentsInChildren<UISelectable>();
		UISelectable[] array = componentsInChildren;
		foreach (UISelectable selectable in array)
		{
			uIPanel.AddSelectable(selectable);
		}
		uIPanel.Select(componentsInChildren[0]);
		Singleton<UIScreenManager>.Instance?.AddScreen(this, Close);
	}

	private void Close()
	{
		Singleton<UIScreenManager>.Instance?.RemoveScreen(this);
		TMP_Dropdown componentInParent = ((Component)this).GetComponentInParent<TMP_Dropdown>();
		if ((Object)(object)componentInParent != (Object)null)
		{
			componentInParent.Hide();
		}
		Dropdown componentInParent2 = ((Component)this).GetComponentInParent<Dropdown>();
		if ((Object)(object)componentInParent2 != (Object)null)
		{
			componentInParent2.Hide();
		}
	}
}
