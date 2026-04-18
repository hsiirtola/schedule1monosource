using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using AeLa.EasyFeedback;
using AeLa.EasyFeedback.FormElements;
using AeLa.EasyFeedback.Utility;
using ScheduleOne.DevUtilities;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class FeedbackForm : FeedbackForm
{
	public CanvasGroup CanvasGroup;

	public Toggle ScreenshotToggle;

	public Toggle SaveFileToggle;

	public TMP_InputField SummaryField;

	public TMP_InputField DescriptionField;

	public RectTransform Cog;

	public TMP_Dropdown CategoryDropdown;

	public override void Awake()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		((FeedbackForm)this).Awake();
		ScreenshotToggle.SetIsOnWithoutNotify(base.IncludeScreenshot);
		((UnityEvent<bool>)(object)ScreenshotToggle.onValueChanged).AddListener((UnityAction<bool>)OnScreenshotToggle);
		SaveFileToggle.SetIsOnWithoutNotify(base.IncludeSaveFile);
		((UnityEvent<bool>)(object)SaveFileToggle.onValueChanged).AddListener((UnityAction<bool>)OnSaveFileToggle);
		base.OnSubmissionSucceeded.AddListener(new UnityAction(Clear));
	}

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		RectTransform cog = Cog;
		((Transform)cog).localEulerAngles = ((Transform)cog).localEulerAngles + new Vector3(0f, 0f, -180f * Time.unscaledDeltaTime);
	}

	public void PrepScreenshot()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		base.CurrentReport = new Report();
	}

	private void OnScreenshotToggle(bool value)
	{
		base.IncludeScreenshot = value;
	}

	private void OnSaveFileToggle(bool value)
	{
		base.IncludeSaveFile = value;
	}

	public void SetFormData(string title)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		if (base.CurrentReport == null)
		{
			base.CurrentReport = new Report();
		}
		base.CurrentReport.Title = title;
		((Component)((Component)this).GetComponentInChildren<ReportTitle>()).GetComponent<TMP_InputField>().SetTextWithoutNotify(title);
	}

	public void SetCategory(string categoryName)
	{
		for (int i = 0; i < base.Config.Board.CategoryNames.Length; i++)
		{
			if (base.Config.Board.CategoryNames[i].Contains(categoryName))
			{
				CategoryDropdown.SetValueWithoutNotify(i + 1);
				return;
			}
		}
		Console.LogWarning("Category not found: " + categoryName);
	}

	public override void Submit()
	{
		if (base.IncludeScreenshot)
		{
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0f);
			CanvasGroup.alpha = 0f;
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Wait());
		}
		if (File.Exists(Application.persistentDataPath + "/Player-prev.log"))
		{
			try
			{
				byte[] array = File.ReadAllBytes(Application.persistentDataPath + "/Player-prev.log");
				base.CurrentReport.AttachFile("Player-prev.txt", array);
			}
			catch (Exception ex)
			{
				Console.LogError("Failed to attach Player-prev.txt: " + ex.Message);
			}
		}
		if (base.IncludeSaveFile)
		{
			string loadedGameFolderPath = Singleton<LoadManager>.Instance.LoadedGameFolderPath;
			string text = loadedGameFolderPath + ".zip";
			try
			{
				if (File.Exists(text))
				{
					Console.Log("Deleting prior zip file: " + text);
					File.Delete(text);
				}
				ZipFile.CreateFromDirectory(loadedGameFolderPath, text, CompressionLevel.Optimal, includeBaseDirectory: true);
				byte[] array2 = File.ReadAllBytes(text);
				base.CurrentReport.AttachFile("SaveGame.zip", array2);
			}
			catch (Exception ex2)
			{
				Console.LogError("Failed to attach save file: " + ex2.Message);
			}
			finally
			{
				if (File.Exists(text))
				{
					File.Delete(text);
				}
			}
		}
		if ((Object)(object)Player.Local != (Object)null)
		{
			Report currentReport = base.CurrentReport;
			currentReport.Title = currentReport.Title + " (" + Player.Local.PlayerName + ")";
		}
		base.CurrentReport.AddSection("Game Info", 2);
		string text2 = "Singleplayer";
		if (Singleton<Lobby>.InstanceExists && Singleton<Lobby>.Instance.IsInLobby)
		{
			text2 = "Multiplayer";
			text2 = ((!Singleton<Lobby>.Instance.IsHost) ? (text2 + " (Client)") : (text2 + " (Host)"));
		}
		base.CurrentReport["Game Info"].AppendLine("Network Mode: " + text2);
		base.CurrentReport["Game Info"].AppendLine("Player Count: " + Player.PlayerList.Count);
		base.CurrentReport["Game Info"].AppendLine("Beta Branch: " + GameManager.IS_BETA);
		base.CurrentReport["Game Info"].AppendLine("Is Demo: " + false);
		base.CurrentReport["Game Info"].AppendLine("Load History: " + string.Join(", ", LoadManager.LoadHistory));
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(((FeedbackForm)this).SubmitAsync());
		((FeedbackForm)this).Submit();
		IEnumerator Wait()
		{
			yield return (object)new WaitForEndOfFrame();
			PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0f);
			CanvasGroup.alpha = 1f;
		}
	}

	protected override string GetTextToAppendToTitle()
	{
		string textToAppendToTitle = ((FeedbackForm)this).GetTextToAppendToTitle();
		textToAppendToTitle = textToAppendToTitle + " (" + Application.version + ")";
		if ((Object)(object)Player.Local != (Object)null)
		{
			textToAppendToTitle = textToAppendToTitle + " (" + Player.Local.PlayerName + ")";
		}
		return textToAppendToTitle;
	}

	private void Clear()
	{
		SummaryField.SetTextWithoutNotify(string.Empty);
		DescriptionField.SetTextWithoutNotify(string.Empty);
	}

	private IEnumerator ScreenshotAndOpenForm()
	{
		if (base.IncludeScreenshot)
		{
			yield return ScreenshotUtil.CaptureScreenshot(base.ScreenshotCaptureMode, base.ResizeLargeScreenshots, (Action<byte[]>)delegate(byte[] ss)
			{
				base.CurrentReport.AttachFile("screenshot.png", ss);
			}, (Action<string>)delegate(string err)
			{
				((UnityEvent<string>)(object)base.OnSubmissionError).Invoke(err);
			});
		}
		((FeedbackForm)this).EnableForm();
		((Component)base.Form).gameObject.SetActive(true);
		base.OnFormOpened.Invoke();
	}
}
