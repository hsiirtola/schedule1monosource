using System.Collections.Generic;
using ScheduleOne.UI.MainMenu;
using TMPro;
using UnityEngine;

namespace ScheduleOne;

public class CommandListScreen : MainMenuScreen
{
	public RectTransform CommandEntryContainer;

	public RectTransform CommandEntryPrefab;

	private List<RectTransform> commandEntries = new List<RectTransform>();

	private void Start()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		if (commandEntries.Count == 0)
		{
			foreach (Console.ConsoleCommand command in Console.Commands)
			{
				RectTransform val = Object.Instantiate<RectTransform>(CommandEntryPrefab, (Transform)(object)CommandEntryContainer);
				((TMP_Text)((Component)((Transform)val).Find("Command")).GetComponent<TextMeshProUGUI>()).text = command.CommandWord;
				((TMP_Text)((Component)((Transform)val).Find("Description")).GetComponent<TextMeshProUGUI>()).text = command.CommandDescription;
				((TMP_Text)((Component)((Transform)val).Find("Example")).GetComponent<TextMeshProUGUI>()).text = command.ExampleUsage;
				commandEntries.Add(val);
			}
		}
		CommandEntryContainer.offsetMin = new Vector2(CommandEntryContainer.offsetMin.x, 0f);
		CommandEntryContainer.offsetMax = new Vector2(CommandEntryContainer.offsetMax.x, 0f);
	}
}
