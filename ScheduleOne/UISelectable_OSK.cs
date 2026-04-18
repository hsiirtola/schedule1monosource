using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne;

public class UISelectable_OSK : UISelectable
{
	protected override void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		base.Awake();
		OnTrigger.AddListener(new UnityAction(ShowOSK));
	}

	private void ShowOSK()
	{
		OnScreenKeyboard.Show(OnSubmit, OnCancel);
	}

	private void OnSubmit(string text)
	{
		((Component)this).GetComponent<TMP_InputField>().text = text;
	}

	protected override bool DeselectOnPointerExit()
	{
		return false;
	}

	private void OnCancel()
	{
	}
}
