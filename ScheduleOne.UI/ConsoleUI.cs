using System.Collections;
using System.Collections.Generic;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ScheduleOne.UI;

public class ConsoleUI : MonoBehaviour
{
	[Header("References")]
	public Canvas canvas;

	public TMP_InputField InputField;

	public GameObject Container;

	private static List<string> _commandHistory = new List<string>();

	private int _currentCommandIndex = -1;

	public bool IS_CONSOLE_ENABLED
	{
		get
		{
			if (!NetworkSingleton<GameManager>.Instance.Settings.ConsoleEnabled || !InstanceFinder.IsServer)
			{
				return Application.isEditor;
			}
			return true;
		}
	}

	private void Awake()
	{
		((UnityEvent<string>)(object)InputField.onSubmit).AddListener((UnityAction<string>)Submit);
		Container.gameObject.SetActive(false);
		GameInput.RegisterExitListener(Exit, 5);
	}

	private void Update()
	{
		if (Input.GetKeyDown((KeyCode)96) && !Singleton<PauseMenu>.Instance.IsPaused && IS_CONSOLE_ENABLED)
		{
			SetIsOpen(!((Behaviour)canvas).enabled);
		}
		if (((Behaviour)canvas).enabled)
		{
			if (!Player.Local.Health.IsAlive)
			{
				SetIsOpen(open: false);
			}
			UpdateCommandHistory();
		}
	}

	private void UpdateCommandHistory()
	{
		int currentCommandIndex = _currentCommandIndex;
		if (Input.GetKeyDown((KeyCode)273))
		{
			_currentCommandIndex = Mathf.Clamp(_currentCommandIndex + 1, -1, _commandHistory.Count - 1);
		}
		if (Input.GetKeyDown((KeyCode)274))
		{
			_currentCommandIndex = Mathf.Clamp(_currentCommandIndex - 1, -1, _commandHistory.Count - 1);
		}
		if (_currentCommandIndex != currentCommandIndex)
		{
			if (_currentCommandIndex == -1)
			{
				InputField.SetTextWithoutNotify("");
				return;
			}
			InputField.SetTextWithoutNotify(_commandHistory[_currentCommandIndex]);
			InputField.caretPosition = InputField.text.Length;
		}
	}

	private void Exit(ExitAction exitAction)
	{
		if (!((Object)(object)canvas == (Object)null) && ((Behaviour)canvas).enabled && !exitAction.Used && exitAction.exitType == ExitType.Escape)
		{
			exitAction.Used = true;
			SetIsOpen(open: false);
		}
	}

	public void SetIsOpen(bool open)
	{
		if (InstanceFinder.IsHost || !((Object)(object)InstanceFinder.NetworkManager != (Object)null) || Application.isEditor || Debug.isDebugBuild)
		{
			((Behaviour)canvas).enabled = open;
			Container.gameObject.SetActive(open);
			_currentCommandIndex = -1;
			InputField.SetTextWithoutNotify("");
			if (open)
			{
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
				GameInput.IsTyping = true;
				((MonoBehaviour)this).StartCoroutine(Routine());
			}
			else
			{
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
				GameInput.IsTyping = false;
			}
		}
		IEnumerator Routine()
		{
			yield return null;
			EventSystem.current.SetSelectedGameObject((GameObject)null);
			EventSystem.current.SetSelectedGameObject(((Component)InputField).gameObject);
		}
	}

	public void Submit(string val)
	{
		if (((Behaviour)canvas).enabled)
		{
			if (!string.IsNullOrEmpty(val))
			{
				_commandHistory.Insert(0, val);
				Console.SubmitCommand(val);
			}
			SetIsOpen(open: false);
		}
	}
}
