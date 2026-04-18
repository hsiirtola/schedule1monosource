using System.IO;
using System.IO.Compression;
using ScheduleOne.Persistence;
using SFB;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.MainMenu;

[RequireComponent(typeof(Button))]
public class SaveExportButton : MonoBehaviour
{
	public int SaveSlotIndex;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener(new UnityAction(Clicked));
	}

	private void Clicked()
	{
		(new ExtensionFilter[1])[0] = new ExtensionFilter("Zip Files", "zip");
		SaveInfo saveInfo = LoadManager.SaveGames[SaveSlotIndex];
		string text = ShowSaveFileDialog(SaveManager.MakeFileSafe(saveInfo.OrganisationName));
		if (!string.IsNullOrEmpty(text))
		{
			Console.Log("Exporting save file to: " + text);
			ZipSaveFolder(saveInfo.SavePath, text);
			Debug.Log((object)("Save exported to: " + text));
		}
	}

	public static string ShowSaveFileDialog(string fileName)
	{
		ExtensionFilter[] extensions = new ExtensionFilter[1]
		{
			new ExtensionFilter("Zip Files", "zip")
		};
		return StandaloneFileBrowser.SaveFilePanel("Export Save File", "", fileName + ".zip", extensions);
	}

	public static void ZipSaveFolder(string sourceFolderPath, string destinationZipPath)
	{
		if (File.Exists(destinationZipPath))
		{
			File.Delete(destinationZipPath);
		}
		ZipFile.CreateFromDirectory(sourceFolderPath, destinationZipPath, CompressionLevel.Optimal, includeBaseDirectory: true);
	}
}
