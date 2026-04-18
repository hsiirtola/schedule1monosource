using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.TV;

public class TVHomeScreen : TVApp
{
	[Header("References")]
	public TVInterface Interface;

	public TVApp[] Apps;

	public RectTransform AppButtonContainer;

	public RectTransform[] PlayerDisplays;

	public TextMeshProUGUI TimeLabel;

	[Header("Prefabs")]
	public GameObject AppButtonPrefab;

	private bool skipExit;

	protected override void Awake()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		base.Awake();
		TVApp[] apps = Apps;
		foreach (TVApp app in apps)
		{
			app.PreviousScreen = this;
			app.CanvasGroup.alpha = 0f;
			GameObject obj = Object.Instantiate<GameObject>(AppButtonPrefab, (Transform)(object)AppButtonContainer);
			((Component)obj.transform.Find("Icon")).GetComponent<Image>().sprite = app.Icon;
			((TMP_Text)((Component)obj.transform.Find("Name")).GetComponent<TextMeshProUGUI>()).text = app.AppName;
			((UnityEvent)obj.GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				AppSelected(app);
			});
			app.Close();
		}
		Interface.onPlayerAdded.AddListener((UnityAction<Player>)PlayerChange);
		Interface.onPlayerRemoved.AddListener((UnityAction<Player>)PlayerChange);
		Close();
	}

	public override void Open()
	{
		base.Open();
		UpdateTimeLabel();
	}

	public override void Close()
	{
		base.Close();
		if (skipExit)
		{
			skipExit = false;
		}
		else
		{
			Interface.Close();
		}
	}

	protected override void ActiveMinPass()
	{
		base.ActiveMinPass();
		UpdateTimeLabel();
	}

	private void UpdateTimeLabel()
	{
		((TMP_Text)TimeLabel).text = TimeManager.Get12HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime);
	}

	private void AppSelected(TVApp app)
	{
		skipExit = true;
		Close();
		app.Open();
	}

	private void PlayerChange(Player player)
	{
		for (int i = 0; i < PlayerDisplays.Length; i++)
		{
			if (Interface.Players.Count > i)
			{
				((TMP_Text)((Component)((Transform)PlayerDisplays[i]).Find("Name")).GetComponent<TextMeshProUGUI>()).text = Interface.Players[i].PlayerName;
				((Component)PlayerDisplays[i]).gameObject.SetActive(true);
			}
			else
			{
				((Component)PlayerDisplays[i]).gameObject.SetActive(false);
			}
		}
	}
}
