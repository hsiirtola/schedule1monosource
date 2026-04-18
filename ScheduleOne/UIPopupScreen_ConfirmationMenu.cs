using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne;

public class UIPopupScreen_ConfirmationMenu : UIPopupScreen
{
	[SerializeField]
	private TMP_Text titleText;

	[SerializeField]
	private TMP_Text messageText;

	[SerializeField]
	private UISelectable confirmButton;

	[SerializeField]
	private UISelectable cancelButton;

	[SerializeField]
	private Canvas canvas;

	public override void Close()
	{
		Singleton<UIScreenManager>.Instance.RemoveScreen(this);
		((Behaviour)canvas).enabled = false;
		((UnityEventBase)confirmButton.OnTrigger).RemoveAllListeners();
		((UnityEventBase)cancelButton.OnTrigger).RemoveAllListeners();
	}

	private void Open()
	{
		Singleton<UIScreenManager>.Instance.AddScreen(this, Close);
		((Behaviour)canvas).enabled = true;
	}

	public override void Open(params object[] args)
	{
		string text = ((args.Length != 0 && args[0] is string) ? ((string)args[0]) : "Confirm");
		string text2 = ((args.Length > 1 && args[1] is string) ? ((string)args[1]) : "Are you sure?");
		Action onConfirm = ((args.Length > 2 && args[2] is Action) ? ((Action)args[2]) : null);
		Action onCancel = ((args.Length > 3 && args[3] is Action) ? ((Action)args[3]) : null);
		if ((Object)(object)titleText != (Object)null)
		{
			titleText.text = text;
		}
		if ((Object)(object)messageText != (Object)null)
		{
			messageText.text = text2;
		}
		((MonoBehaviour)this).StartCoroutine(RegisterInput(onConfirm, onCancel));
		Open();
		SelectPanel(confirmButton);
	}

	private IEnumerator RegisterInput(Action onConfirm, Action onCancel)
	{
		yield return null;
		if ((Object)(object)confirmButton != (Object)null)
		{
			confirmButton.OnTrigger.AddListener((UnityAction)delegate
			{
				onConfirm?.Invoke();
				Close();
			});
		}
		if ((Object)(object)cancelButton != (Object)null)
		{
			cancelButton.OnTrigger.AddListener((UnityAction)delegate
			{
				onCancel?.Invoke();
				Close();
			});
		}
	}

	private void SelectPanel(UISelectable selectable)
	{
		SetCurrentSelectedPanel(base.Panels[0]);
		base.Panels[0].SelectSelectable(selectable);
	}
}
