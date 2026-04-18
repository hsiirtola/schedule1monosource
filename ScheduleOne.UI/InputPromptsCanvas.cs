using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.UI;

public class InputPromptsCanvas : Singleton<InputPromptsCanvas>
{
	[Serializable]
	public class Module
	{
		public string key;

		public GameObject module;
	}

	public RectTransform InputPromptsContainer;

	[Header("Input prompt modules")]
	public List<Module> Modules = new List<Module>();

	public string currentModuleLabel { get; protected set; } = string.Empty;

	public RectTransform currentModule { get; private set; }

	public void LoadModule(string key)
	{
		GameObject val = Modules.Find((Module x) => x.key.ToLower() == key.ToLower())?.module;
		if ((Object)(object)val == (Object)null)
		{
			Console.LogError("Input prompt module with key '" + key + "' not found!");
			return;
		}
		if ((Object)(object)currentModule != (Object)null)
		{
			UnloadModule();
		}
		currentModuleLabel = key;
		currentModule = Object.Instantiate<GameObject>(val, (Transform)(object)InputPromptsContainer).GetComponent<RectTransform>();
	}

	public void UnloadModule()
	{
		currentModuleLabel = string.Empty;
		if ((Object)(object)currentModule != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)currentModule).gameObject);
		}
	}
}
