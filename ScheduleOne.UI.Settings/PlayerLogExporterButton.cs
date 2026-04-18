using System.IO;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Settings;

[RequireComponent(typeof(Button))]
public class PlayerLogExporterButton : MonoBehaviour
{
	[SerializeField]
	private bool _exportPreviousLog;

	[SerializeField]
	private UnityEvent OnSuccess;

	private Button _button;

	private void Awake()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		_button = ((Component)this).GetComponent<Button>();
		((UnityEvent)_button.onClick).AddListener(new UnityAction(OnButtonClick));
	}

	private void OnEnable()
	{
		((Selectable)_button).interactable = DoesLogExist();
	}

	private void OnButtonClick()
	{
		PlayerLogExporter.ExportPlayerLog(_exportPreviousLog, Success);
	}

	private void Success()
	{
		if (OnSuccess != null)
		{
			OnSuccess.Invoke();
		}
	}

	private bool DoesLogExist()
	{
		return File.Exists(PlayerLogExporter.GetLogPath(_exportPreviousLog));
	}
}
