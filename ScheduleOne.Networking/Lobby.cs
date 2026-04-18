using System;
using System.Linq;
using System.Text;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.UI;
using ScheduleOne.UI.MainMenu;
using Steamworks;
using UnityEngine;

namespace ScheduleOne.Networking;

public class Lobby : PersistentSingleton<Lobby>
{
	public const bool ENABLED = true;

	public const int PLAYER_LIMIT = 4;

	public const string JOIN_READY = "ready";

	public const string LOAD_TUTORIAL = "load_tutorial";

	public const string HOST_LOADING = "host_loading";

	public CSteamID[] Players = (CSteamID[])(object)new CSteamID[4];

	public Action onLobbyChange;

	private Callback<LobbyCreated_t> LobbyCreatedCallback;

	private Callback<LobbyEnter_t> LobbyEnteredCallback;

	private Callback<LobbyChatUpdate_t> ChatUpdateCallback;

	private Callback<GameLobbyJoinRequested_t> GameLobbyJoinRequestedCallback;

	private Callback<LobbyChatMsg_t> LobbyChatMessageCallback;

	public bool IsHost
	{
		get
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			if (IsInLobby)
			{
				if (Players.Length != 0)
				{
					return Players[0] == LocalPlayerID;
				}
				return false;
			}
			return true;
		}
	}

	public ulong LobbyID { get; private set; }

	public CSteamID LobbySteamID => new CSteamID(LobbyID);

	public bool IsInLobby => LobbyID != 0;

	public int PlayerCount
	{
		get
		{
			if (!IsInLobby)
			{
				return 1;
			}
			return Players.Count((CSteamID p) => p != CSteamID.Nil);
		}
	}

	public CSteamID LocalPlayerID { get; private set; } = CSteamID.Nil;

	protected override void Awake()
	{
		base.Awake();
		if (!((Object)(object)Singleton<Lobby>.Instance == (Object)null) && !((Object)(object)Singleton<Lobby>.Instance != (Object)(object)this))
		{
			_ = Destroyed;
		}
	}

	protected override void Start()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		if ((Object)(object)Singleton<Lobby>.Instance == (Object)null || (Object)(object)Singleton<Lobby>.Instance != (Object)(object)this || Destroyed)
		{
			return;
		}
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning((object)"Steamworks not initialized. Lobby will not be available.");
			return;
		}
		LocalPlayerID = SteamUser.GetSteamID();
		InitializeCallbacks();
		string launchLobby = GetLaunchLobby();
		if (launchLobby == null || !(launchLobby != string.Empty) || !SteamManager.Initialized)
		{
			return;
		}
		try
		{
			SteamMatchmaking.JoinLobby(new CSteamID(ulong.Parse(launchLobby)));
		}
		catch
		{
			Console.LogWarning("There is an issue with launch commands.");
		}
	}

	private void InitializeCallbacks()
	{
		LobbyCreatedCallback = Callback<LobbyCreated_t>.Create((DispatchDelegate<LobbyCreated_t>)OnLobbyCreated);
		LobbyEnteredCallback = Callback<LobbyEnter_t>.Create((DispatchDelegate<LobbyEnter_t>)OnLobbyEntered);
		ChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create((DispatchDelegate<LobbyChatUpdate_t>)PlayerEnterOrLeave);
		GameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create((DispatchDelegate<GameLobbyJoinRequested_t>)LobbyJoinRequested);
		LobbyChatMessageCallback = Callback<LobbyChatMsg_t>.Create((DispatchDelegate<LobbyChatMsg_t>)OnLobbyChatMessage);
	}

	public void TryOpenInviteInterface()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!IsInLobby)
		{
			Console.Log("Not currently in a lobby, creating one...");
			CreateLobby();
		}
		if (SteamMatchmaking.GetNumLobbyMembers(LobbySteamID) >= 4)
		{
			Debug.LogWarning((object)"Lobby already at max capacity!");
		}
		else
		{
			SteamFriends.ActivateGameOverlayInviteDialog(LobbySteamID);
		}
	}

	public void LeaveLobby()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (IsInLobby)
		{
			SteamMatchmaking.LeaveLobby(LobbySteamID);
			Console.Log("Leaving lobby: " + LobbyID);
		}
		LobbyID = 0uL;
		UpdateLobbyMembers();
		if (onLobbyChange != null)
		{
			onLobbyChange();
		}
	}

	private void CreateLobby()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		SteamMatchmaking.CreateLobby((ELobbyType)1, 4);
	}

	private string GetLaunchLobby()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "+connect_lobby" && commandLineArgs.Length > i + 1)
			{
				return commandLineArgs[i + 1];
			}
		}
		return string.Empty;
	}

	private void UpdateLobbyMembers()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Players.Length; i++)
		{
			Players[i] = CSteamID.Nil;
		}
		int num = (IsInLobby ? SteamMatchmaking.GetNumLobbyMembers(LobbySteamID) : 0);
		for (int j = 0; j < num; j++)
		{
			Players[j] = SteamMatchmaking.GetLobbyMemberByIndex(LobbySteamID, j);
		}
	}

	public void JoinAsClient(string steamId64)
	{
		Singleton<LoadManager>.Instance.LoadAsClient(steamId64);
	}

	public void SendLobbyMessage(string message)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (!IsInLobby)
		{
			Console.LogWarning("Not in a lobby, cannot send message.");
			return;
		}
		byte[] bytes = Encoding.ASCII.GetBytes(message);
		SteamMatchmaking.SendLobbyChatMsg(LobbySteamID, bytes, bytes.Length);
	}

	public void SetLobbyData(string key, string value)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!IsInLobby)
		{
			Console.LogWarning("Not in a lobby, cannot set data.");
		}
		else
		{
			SteamMatchmaking.SetLobbyData(LobbySteamID, key, value);
		}
	}

	private unsafe void OnLobbyCreated(LobbyCreated_t result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		if ((int)result.m_eResult == 1)
		{
			Console.Log("Lobby created: " + result.m_ulSteamIDLobby);
		}
		else
		{
			Console.LogWarning("Lobby creation failed: " + ((object)(*(EResult*)(&result.m_eResult))/*cast due to .constrained prefix*/).ToString());
		}
		LobbyID = result.m_ulSteamIDLobby;
		SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "owner", ((object)SteamUser.GetSteamID()/*cast due to .constrained prefix*/).ToString());
		SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "version", Application.version);
		SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "host_loading", "false");
		SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "ready", "false");
		UpdateLobbyMembers();
		if (onLobbyChange != null)
		{
			onLobbyChange();
		}
	}

	private void OnLobbyEntered(LobbyEnter_t result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		string lobbyData = SteamMatchmaking.GetLobbyData(new CSteamID(result.m_ulSteamIDLobby), "version");
		Console.Log("Lobby version: " + lobbyData + ", client version: " + Application.version);
		if (lobbyData != Application.version)
		{
			Console.LogWarning("Lobby version mismatch, cannot join.");
			if (Singleton<MainMenuPopup>.InstanceExists)
			{
				Singleton<MainMenuPopup>.Instance.Open("Version Mismatch", "Host version: " + lobbyData + "\nYour version: " + Application.version, isBad: true);
			}
			LeaveLobby();
			return;
		}
		Console.Log("Entered lobby: " + result.m_ulSteamIDLobby);
		LobbyID = result.m_ulSteamIDLobby;
		UpdateLobbyMembers();
		if (onLobbyChange != null)
		{
			onLobbyChange();
		}
		string lobbyData2 = SteamMatchmaking.GetLobbyData(LobbySteamID, "ready");
		bool flag = SteamMatchmaking.GetLobbyData(LobbySteamID, "load_tutorial") == "true";
		bool flag2 = SteamMatchmaking.GetLobbyData(LobbySteamID, "host_loading") == "true";
		if (lobbyData2 == "true" && !IsHost)
		{
			JoinAsClient(SteamMatchmaking.GetLobbyOwner(LobbySteamID).m_SteamID.ToString());
		}
		else if (flag && !IsHost)
		{
			Singleton<LoadManager>.Instance.LoadTutorialAsClient();
		}
		else if (flag2 && !IsHost)
		{
			Singleton<LoadManager>.Instance.SetWaitingForHostLoad();
			Singleton<LoadingScreen>.Instance.Open();
		}
	}

	private void PlayerEnterOrLeave(LobbyChatUpdate_t result)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Console.Log("Player join/leave: " + SteamFriends.GetFriendPersonaName(new CSteamID(result.m_ulSteamIDUserChanged)));
		UpdateLobbyMembers();
		if (result.m_ulSteamIDMakingChange == LobbySteamID.m_SteamID && result.m_ulSteamIDUserChanged != LocalPlayerID.m_SteamID)
		{
			Console.Log("Lobby owner left, leaving lobby.");
			LeaveLobby();
		}
		if (onLobbyChange != null)
		{
			onLobbyChange();
		}
	}

	private unsafe void LobbyJoinRequested(GameLobbyJoinRequested_t result)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		CSteamID steamIDLobby = result.m_steamIDLobby;
		Console.Log("Join requested: " + ((object)(*(CSteamID*)(&steamIDLobby))/*cast due to .constrained prefix*/).ToString());
		if (LobbyID != 0L)
		{
			LeaveLobby();
		}
		SteamMatchmaking.JoinLobby(result.m_steamIDLobby);
	}

	private void OnLobbyChatMessage(LobbyChatMsg_t result)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		byte[] array = new byte[128];
		int num = 128;
		CSteamID val = default(CSteamID);
		EChatEntryType val2 = default(EChatEntryType);
		SteamMatchmaking.GetLobbyChatEntry(new CSteamID(LobbyID), (int)result.m_iChatID, ref val, array, num, ref val2);
		string text = Encoding.ASCII.GetString(array);
		text = text.TrimEnd(default(char));
		Console.Log("Lobby chat message received: " + text);
		if (!IsHost && !Singleton<LoadManager>.Instance.IsGameLoaded)
		{
			switch (text)
			{
			case "ready":
				JoinAsClient(val.m_SteamID.ToString());
				break;
			case "load_tutorial":
				Singleton<LoadManager>.Instance.LoadTutorialAsClient();
				break;
			case "host_loading":
				Singleton<LoadManager>.Instance.SetWaitingForHostLoad();
				Singleton<LoadingScreen>.Instance.Open();
				break;
			}
		}
	}
}
