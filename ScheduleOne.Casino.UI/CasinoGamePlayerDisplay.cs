using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Casino.UI;

public class CasinoGamePlayerDisplay : MonoBehaviour
{
	public CasinoGamePlayers BindedPlayers;

	[Header("References")]
	public TextMeshProUGUI TitleLabel;

	public RectTransform[] PlayerEntries;

	public void RefreshPlayers()
	{
		int currentPlayerCount = BindedPlayers.CurrentPlayerCount;
		((TMP_Text)TitleLabel).text = "Players (" + currentPlayerCount + "/" + BindedPlayers.PlayerLimit + ")";
		for (int i = 0; i < PlayerEntries.Length; i++)
		{
			Player player = BindedPlayers.GetPlayer(i);
			if ((Object)(object)player != (Object)null)
			{
				((TMP_Text)((Component)((Transform)PlayerEntries[i]).Find("Container/Name")).GetComponent<TextMeshProUGUI>()).text = player.PlayerName;
				((Component)((Transform)PlayerEntries[i]).Find("Container")).gameObject.SetActive(true);
			}
			else
			{
				((Component)((Transform)PlayerEntries[i]).Find("Container")).gameObject.SetActive(false);
			}
		}
		RefreshScores();
	}

	public void RefreshScores()
	{
		int currentPlayerCount = BindedPlayers.CurrentPlayerCount;
		for (int i = 0; i < PlayerEntries.Length; i++)
		{
			if (currentPlayerCount > i)
			{
				((TMP_Text)((Component)((Transform)PlayerEntries[i]).Find("Container/Score")).GetComponent<TextMeshProUGUI>()).text = MoneyManager.FormatAmount(BindedPlayers.GetPlayerScore(BindedPlayers.GetPlayer(i)));
			}
		}
	}

	public void Bind(CasinoGamePlayers players)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		BindedPlayers = players;
		BindedPlayers.onPlayerListChanged.AddListener(new UnityAction(RefreshPlayers));
		BindedPlayers.onPlayerScoresChanged.AddListener(new UnityAction(RefreshScores));
		RefreshPlayers();
	}

	public void Unbind()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		if (!((Object)(object)BindedPlayers == (Object)null))
		{
			BindedPlayers.onPlayerListChanged.RemoveListener(new UnityAction(RefreshPlayers));
			BindedPlayers.onPlayerScoresChanged.RemoveListener(new UnityAction(RefreshScores));
			BindedPlayers = null;
		}
	}
}
