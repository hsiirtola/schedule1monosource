using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Networking;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Multiplayer;

public class LobbyInterface : PersistentSingleton<LobbyInterface>
{
	[Header("References")]
	public Lobby Lobby;

	public Canvas Canvas;

	public TextMeshProUGUI LobbyTitle;

	public RectTransform[] PlayerSlots;

	public Button InviteButton;

	public Button LeaveButton;

	public GameObject InviteHint;

	protected override void Awake()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		base.Awake();
		if (!((Object)(object)Singleton<LobbyInterface>.Instance == (Object)null) && !((Object)(object)Singleton<LobbyInterface>.Instance != (Object)(object)this))
		{
			((UnityEvent)InviteButton.onClick).AddListener(new UnityAction(InviteClicked));
			((UnityEvent)LeaveButton.onClick).AddListener(new UnityAction(LeaveClicked));
			Lobby lobby = Lobby;
			lobby.onLobbyChange = (Action)Delegate.Combine(lobby.onLobbyChange, (Action)delegate
			{
				UpdateButtons();
				UpdatePlayers();
				((TMP_Text)LobbyTitle).text = "Lobby (" + Lobby.PlayerCount + "/" + 4 + ")";
			});
		}
	}

	protected override void Start()
	{
		base.Start();
		if (!((Object)(object)Singleton<LobbyInterface>.Instance == (Object)null) && !((Object)(object)Singleton<LobbyInterface>.Instance != (Object)(object)this))
		{
			UpdateButtons();
			UpdatePlayers();
			if (PlayerPrefs.GetInt("InviteHintShown", 0) == 0)
			{
				InviteHint.SetActive(true);
			}
			else
			{
				InviteHint.SetActive(false);
			}
		}
	}

	private void LateUpdate()
	{
		if (Singleton<PauseMenu>.InstanceExists)
		{
			((Behaviour)Canvas).enabled = Singleton<PauseMenu>.Instance.IsPaused && Lobby.IsInLobby && !GameManager.IS_TUTORIAL;
			if (((Behaviour)Canvas).enabled)
			{
				((Component)LeaveButton).gameObject.SetActive(false);
			}
		}
		else
		{
			((Behaviour)Canvas).enabled = true;
			((Component)LeaveButton).gameObject.SetActive(!Lobby.IsHost);
		}
	}

	public void SetVisible(bool visible)
	{
		((Behaviour)Canvas).enabled = visible;
	}

	public void LeaveClicked()
	{
		Lobby.LeaveLobby();
	}

	public void InviteClicked()
	{
		PlayerPrefs.SetInt("InviteHintShown", 1);
		InviteHint.SetActive(false);
		Lobby.TryOpenInviteInterface();
	}

	private void UpdateButtons()
	{
		((Component)InviteButton).gameObject.SetActive(Lobby.IsHost && Lobby.PlayerCount < 4);
		((Component)LeaveButton).gameObject.SetActive(!Lobby.IsHost);
	}

	private void UpdatePlayers()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (Lobby.IsInLobby)
		{
			for (int i = 0; i < PlayerSlots.Length; i++)
			{
				if (Lobby.Players[i] != CSteamID.Nil)
				{
					SetPlayer(i, Lobby.Players[i]);
				}
				else
				{
					ClearPlayer(i);
				}
			}
		}
		else
		{
			SetPlayer(0, Lobby.LocalPlayerID);
			for (int j = 1; j < PlayerSlots.Length; j++)
			{
				ClearPlayer(j);
			}
		}
	}

	public void SetPlayer(int index, CSteamID player)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Lobby.Players[index] = player;
		((Component)((Transform)PlayerSlots[index]).Find("Frame/Avatar")).GetComponent<RawImage>().texture = (Texture)(object)GetAvatar(player);
		((Component)PlayerSlots[index]).gameObject.SetActive(true);
	}

	public void ClearPlayer(int index)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Lobby.Players[index] = CSteamID.Nil;
		((Component)PlayerSlots[index]).gameObject.SetActive(false);
	}

	private Texture2D GetAvatar(CSteamID user)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		if (!SteamManager.Initialized)
		{
			return new Texture2D(0, 0);
		}
		int mediumFriendAvatar = SteamFriends.GetMediumFriendAvatar(user);
		uint num = default(uint);
		uint num2 = default(uint);
		if (SteamUtils.GetImageSize(mediumFriendAvatar, ref num, ref num2) && num != 0 && num2 != 0)
		{
			byte[] array = new byte[num * num2 * 4];
			Texture2D val = new Texture2D((int)num, (int)num2, (TextureFormat)4, false, false);
			if (SteamUtils.GetImageRGBA(mediumFriendAvatar, array, (int)(num * num2 * 4)))
			{
				val.LoadRawTextureData(array);
				val.Apply();
			}
			return val;
		}
		Debug.LogWarning((object)"Couldn't get avatar.");
		return new Texture2D(0, 0);
	}
}
