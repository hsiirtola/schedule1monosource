using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Dialogue;

public class DialogueModule : MonoBehaviour
{
	public EDialogueModule ModuleType;

	public List<Entry> Entries = new List<Entry>();

	public Entry GetEntry(string key)
	{
		return Entries.Find((Entry x) => x.Key == key);
	}

	public DialogueChain GetChain(string key)
	{
		Entry entry = GetEntry(key);
		if (entry.Chains == null || entry.Chains.Length == 0)
		{
			Debug.LogError((object)("DialogueModule.Get: No lines found for key: " + key));
		}
		return entry.GetRandomChain();
	}

	public bool HasChain(string key)
	{
		return GetEntry(key).Chains != null;
	}

	public string GetLine(string key)
	{
		Entry entry = GetEntry(key);
		if (entry.Chains == null || entry.Chains.Length == 0)
		{
			Debug.LogError((object)("DialogueModule.Get: No lines found for key: " + key));
			return string.Empty;
		}
		return entry.GetRandomLine();
	}

	public bool HasLine(string key)
	{
		Entry entry = GetEntry(key);
		if (entry.Chains == null || entry.Chains.Length == 0)
		{
			return false;
		}
		return true;
	}
}
