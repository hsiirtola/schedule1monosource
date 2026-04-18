using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Ookii.Dialogs;

namespace SFB;

public class StandaloneFileBrowserWindows : IStandaloneFileBrowser
{
	[DllImport("user32.dll")]
	private static extern IntPtr GetActiveWindow();

	public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		VistaOpenFileDialog val = new VistaOpenFileDialog();
		((VistaFileDialog)val).Title = title;
		if (extensions != null)
		{
			((VistaFileDialog)val).Filter = GetFilterFromFileExtensionList(extensions);
			((VistaFileDialog)val).FilterIndex = 1;
		}
		else
		{
			((VistaFileDialog)val).Filter = string.Empty;
		}
		val.Multiselect = multiselect;
		if (!string.IsNullOrEmpty(directory))
		{
			((VistaFileDialog)val).FileName = GetDirectoryPath(directory);
		}
		string[] result = ((((CommonDialog)(object)val).ShowDialog((IWin32Window)new WindowWrapper(GetActiveWindow())) == DialogResult.OK) ? ((VistaFileDialog)val).FileNames : new string[0]);
		((Component)(object)val).Dispose();
		return result;
	}

	public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb)
	{
		cb(OpenFilePanel(title, directory, extensions, multiselect));
	}

	public string[] OpenFolderPanel(string title, string directory, bool multiselect)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		VistaFolderBrowserDialog val = new VistaFolderBrowserDialog();
		val.Description = title;
		if (!string.IsNullOrEmpty(directory))
		{
			val.SelectedPath = GetDirectoryPath(directory);
		}
		string[] result = ((((CommonDialog)(object)val).ShowDialog((IWin32Window)new WindowWrapper(GetActiveWindow())) != DialogResult.OK) ? new string[0] : new string[1] { val.SelectedPath });
		((Component)(object)val).Dispose();
		return result;
	}

	public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb)
	{
		cb(OpenFolderPanel(title, directory, multiselect));
	}

	public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		VistaSaveFileDialog val = new VistaSaveFileDialog();
		((VistaFileDialog)val).Title = title;
		string text = "";
		if (!string.IsNullOrEmpty(directory))
		{
			text = GetDirectoryPath(directory);
		}
		if (!string.IsNullOrEmpty(defaultName))
		{
			text += defaultName;
		}
		((VistaFileDialog)val).FileName = text;
		if (extensions != null)
		{
			((VistaFileDialog)val).Filter = GetFilterFromFileExtensionList(extensions);
			((VistaFileDialog)val).FilterIndex = 1;
			((VistaFileDialog)val).DefaultExt = extensions[0].Extensions[0];
			((VistaFileDialog)val).AddExtension = true;
		}
		else
		{
			((VistaFileDialog)val).DefaultExt = string.Empty;
			((VistaFileDialog)val).Filter = string.Empty;
			((VistaFileDialog)val).AddExtension = false;
		}
		string result = ((((CommonDialog)(object)val).ShowDialog((IWin32Window)new WindowWrapper(GetActiveWindow())) == DialogResult.OK) ? ((VistaFileDialog)val).FileName : "");
		((Component)(object)val).Dispose();
		return result;
	}

	public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb)
	{
		cb(SaveFilePanel(title, directory, defaultName, extensions));
	}

	private static string GetFilterFromFileExtensionList(ExtensionFilter[] extensions)
	{
		string text = "";
		for (int i = 0; i < extensions.Length; i++)
		{
			ExtensionFilter extensionFilter = extensions[i];
			text = text + extensionFilter.Name + "(";
			string[] extensions2 = extensionFilter.Extensions;
			foreach (string text2 in extensions2)
			{
				text = text + "*." + text2 + ",";
			}
			text = text.Remove(text.Length - 1);
			text += ") |";
			extensions2 = extensionFilter.Extensions;
			foreach (string text3 in extensions2)
			{
				text = text + "*." + text3 + "; ";
			}
			text += "|";
		}
		return text.Remove(text.Length - 1);
	}

	private static string GetDirectoryPath(string directory)
	{
		string text = Path.GetFullPath(directory);
		if (!text.EndsWith("\\"))
		{
			text += "\\";
		}
		if (Path.GetPathRoot(text) == text)
		{
			return directory;
		}
		return Path.GetDirectoryName(text) + Path.DirectorySeparatorChar;
	}
}
