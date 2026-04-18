using System;
using SFB;
using UnityEngine;

public class BasicSample : MonoBehaviour
{
	private string _path;

	private void OnGUI()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector((float)Screen.width / 800f, (float)Screen.height / 600f, 1f);
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, val);
		GUILayout.Space(20f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Space(20f);
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("Open File", Array.Empty<GUILayoutOption>()))
		{
			WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", "", "", multiselect: false));
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Open File Async", Array.Empty<GUILayoutOption>()))
		{
			StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", "", multiselect: false, delegate(string[] paths3)
			{
				WriteResult(paths3);
			});
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Open File Multiple", Array.Empty<GUILayoutOption>()))
		{
			WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", "", "", multiselect: true));
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Open File Extension", Array.Empty<GUILayoutOption>()))
		{
			WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", "", "txt", multiselect: true));
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Open File Directory", Array.Empty<GUILayoutOption>()))
		{
			WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", Application.dataPath, "", multiselect: true));
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Open File Filter", Array.Empty<GUILayoutOption>()))
		{
			ExtensionFilter[] extensions = new ExtensionFilter[3]
			{
				new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
				new ExtensionFilter("Sound Files", "mp3", "wav"),
				new ExtensionFilter("All Files", "*")
			};
			WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, multiselect: true));
		}
		GUILayout.Space(15f);
		if (GUILayout.Button("Open Folder", Array.Empty<GUILayoutOption>()))
		{
			string[] paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", multiselect: true);
			WriteResult(paths);
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Open Folder Async", Array.Empty<GUILayoutOption>()))
		{
			StandaloneFileBrowser.OpenFolderPanelAsync("Select Folder", "", multiselect: true, delegate(string[] paths3)
			{
				WriteResult(paths3);
			});
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Open Folder Directory", Array.Empty<GUILayoutOption>()))
		{
			string[] paths2 = StandaloneFileBrowser.OpenFolderPanel("Select Folder", Application.dataPath, multiselect: true);
			WriteResult(paths2);
		}
		GUILayout.Space(15f);
		if (GUILayout.Button("Save File", Array.Empty<GUILayoutOption>()))
		{
			_path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "", "");
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Save File Async", Array.Empty<GUILayoutOption>()))
		{
			StandaloneFileBrowser.SaveFilePanelAsync("Save File", "", "", "", delegate(string path)
			{
				WriteResult(path);
			});
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Save File Default Name", Array.Empty<GUILayoutOption>()))
		{
			_path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "MySaveFile", "");
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Save File Default Name Ext", Array.Empty<GUILayoutOption>()))
		{
			_path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "MySaveFile", "dat");
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Save File Directory", Array.Empty<GUILayoutOption>()))
		{
			_path = StandaloneFileBrowser.SaveFilePanel("Save File", Application.dataPath, "", "");
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Save File Filter", Array.Empty<GUILayoutOption>()))
		{
			ExtensionFilter[] extensions2 = new ExtensionFilter[2]
			{
				new ExtensionFilter("Binary", "bin"),
				new ExtensionFilter("Text", "txt")
			};
			_path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "MySaveFile", extensions2);
		}
		GUILayout.EndVertical();
		GUILayout.Space(20f);
		GUILayout.Label(_path, Array.Empty<GUILayoutOption>());
		GUILayout.EndHorizontal();
	}

	public void WriteResult(string[] paths)
	{
		if (paths.Length != 0)
		{
			_path = "";
			foreach (string text in paths)
			{
				_path = _path + text + "\n";
			}
		}
	}

	public void WriteResult(string path)
	{
		_path = path;
	}
}
