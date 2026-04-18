using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SFB;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

public static class PlayerLogExporter
{
	private static Action _onSuccess;

	private static Regex[] ExcludedRegexes = new Regex[6]
	{
		new Regex("(?:Failed to create agent because it is not close enough to the NavMesh\\r?\\nUnityEngine\\.AI\\.NavMesh:AddNavMeshData\\(NavMeshData, Vector3, Quaternion\\)\\r?\\nUnity\\.AI\\.Navigation\\.NavMeshSurface:AddData\\(\\)\\r?\\n\\r?\\n\\[ line -?\\d+\\]\\r?\\n)+", RegexOptions.Compiled),
		new Regex("Cannot complete action because (client|server) is not active\\.[\\s\\S]*?(?:\\r?\\n){2,}", RegexOptions.IgnoreCase | RegexOptions.Compiled),
		new Regex("Renderer '.+?' is registered with more than one LODGroup[\\s\\S]*?\\[.*?\\]\\r?\\n", RegexOptions.IgnoreCase | RegexOptions.Compiled),
		new Regex("(?:BoxCollider does not support negative scale or size\\.\\r?\\nThe effective box size has been forced positive and is likely to give unexpected collision geometry\\.\\r?\\nIf you absolutely need to use negative scaling you can use the convex MeshCollider\\. Scene hierarchy path .*\\r?\\n)+", RegexOptions.IgnoreCase | RegexOptions.Compiled),
		new Regex("Received a spawn objectId of \\d+ which was already found in spawned[\\s\\S]*?(?:\\r?\\n){2,}", RegexOptions.IgnoreCase | RegexOptions.Compiled),
		new Regex("(?:^Coroutine continue failure\\r?\\n)+", RegexOptions.Multiline | RegexOptions.Compiled)
	};

	public static void ExportPlayerLog(bool previous, Action onSuccess = null)
	{
		string text = (previous ? "Player-prev" : "Player");
		_onSuccess = onSuccess;
		Debug.Log((object)("Opening save file dialog for " + text + " export..."));
		StandaloneFileBrowser.SaveFilePanelAsync("Save " + text, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{text}_{DateTime.Now:yyyyMMdd_HHmmss}.log", "log", delegate(string savePath)
		{
			SavePathSelected(savePath, previous);
		});
	}

	private static void SavePathSelected(string savePath, bool previous)
	{
		if (string.IsNullOrEmpty(savePath))
		{
			return;
		}
		string text = (previous ? "Player-prev" : "Player");
		try
		{
			string text2 = ReadFileShared(GetLogPath(previous));
			Debug.Log((object)$"Read {text2.Length} characters from {text}.");
			string text3 = FilterLog(text2);
			Debug.Log((object)$"Filtered log length: {text3.Length} characters.");
			File.WriteAllText(savePath, text3, Encoding.UTF8);
			Debug.Log((object)(text + " exported to " + savePath));
			if (_onSuccess != null)
			{
				_onSuccess();
				_onSuccess = null;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"Failed to export {text}:\n{arg}");
		}
	}

	public static string FilterLog(string log)
	{
		Regex[] excludedRegexes = ExcludedRegexes;
		for (int i = 0; i < excludedRegexes.Length; i++)
		{
			log = excludedRegexes[i].Replace(log, string.Empty);
		}
		log = Regex.Replace(log, "(\\r?\\n){4,}", "\n");
		return log;
	}

	private static string ReadFileShared(string path)
	{
		using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using StreamReader streamReader = new StreamReader(stream);
		return streamReader.ReadToEnd();
	}

	public static string GetLogPath(bool previous)
	{
		return Path.Combine(Application.persistentDataPath, previous ? "Player-prev.log" : "Player.log");
	}
}
