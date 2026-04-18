using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AeLa.EasyFeedback;
using FishNet;
using FishNet.Component.Scenes;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using FishNet.Transporting.Yak;
using FishySteamworks;
using Pathfinding;
using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework.Animation;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.Money;
using ScheduleOne.Networking;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.ItemLoaders;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.Quests;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using ScheduleOne.UI.MainMenu;
using ScheduleOne.UI.Phone;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ScheduleOne.Persistence;

public class LoadManager : PersistentSingleton<LoadManager>
{
	public enum ELoadStatus
	{
		None,
		LoadingScene,
		Initializing,
		LoadingData,
		SpawningPlayer,
		WaitingForHost
	}

	public const int LOADS_PER_FRAME = 50;

	public const bool DEBUG = false;

	public const float LOAD_ERROR_TIMEOUT = 90f;

	public const float NETWORK_TIMEOUT = 30f;

	public static List<string> LoadHistory = new List<string>();

	public static SaveInfo[] SaveGames = new SaveInfo[5];

	public static SaveInfo LastPlayedGame = null;

	private List<LoadRequest> loadRequests = new List<LoadRequest>();

	public List<ItemLoader> ItemLoaders = new List<ItemLoader>();

	public List<BuildableItemLoader> ObjectLoaders = new List<BuildableItemLoader>();

	public List<LegacyNPCLoader> LegacyNPCLoaders = new List<LegacyNPCLoader>();

	public List<NPCLoader> NPCLoaders = new List<NPCLoader>();

	public UnityEvent onPreSceneChange;

	public Action<string> OnLocalSaveLoadStart;

	public UnityEvent onPreLoad;

	public UnityEvent onLoadComplete;

	public UnityEvent onSaveInfoLoaded;

	private List<IStaggeredReplicator> staggeredReplicators = new List<IStaggeredReplicator>();

	public string DefaultTutorialSaveFolder => Path.Combine(Application.streamingAssetsPath, "DefaultTutorialSave");

	public bool IsInGameScene
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			Scene activeScene = SceneManager.GetActiveScene();
			if (!(((Scene)(ref activeScene)).name == "Main"))
			{
				activeScene = SceneManager.GetActiveScene();
				return ((Scene)(ref activeScene)).name == "Tutorial";
			}
			return true;
		}
	}

	public bool IsGameLoaded { get; protected set; }

	public bool IsLoading { get; protected set; }

	public float TimeSinceGameLoaded { get; protected set; }

	public bool DebugMode { get; protected set; }

	public ELoadStatus LoadStatus { get; protected set; }

	public string LoadedGameFolderPath { get; protected set; } = string.Empty;

	public SaveInfo ActiveSaveInfo { get; private set; }

	public SaveInfo StoredSaveInfo { get; private set; }

	public static event Action onLoadConfigurations;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		if ((Object)(object)Singleton<LoadManager>.Instance == (Object)null || (Object)(object)Singleton<LoadManager>.Instance != (Object)(object)this)
		{
			return;
		}
		Bananas();
		InitializeItemLoaders();
		InitializeObjectLoaders();
		InitializeNPCLoaders();
		Singleton<SaveManager>.Instance.CheckSaveFolderInitialized();
		RefreshSaveInfo();
		Scene activeScene = SceneManager.GetActiveScene();
		if (!(((Scene)(ref activeScene)).name == "Main"))
		{
			activeScene = SceneManager.GetActiveScene();
			if (!(((Scene)(ref activeScene)).name == "Tutorial"))
			{
				return;
			}
		}
		DebugMode = true;
		IsGameLoaded = true;
		LoadedGameFolderPath = Path.Combine(Singleton<SaveManager>.Instance.IndividualSavesContainerPath, "DevSave");
		if (!Directory.Exists(LoadedGameFolderPath))
		{
			Directory.CreateDirectory(LoadedGameFolderPath);
		}
	}

	private void Bananas()
	{
		string fullName = new DirectoryInfo(Application.dataPath).Parent.FullName;
		Console.Log("Game folder path: " + fullName);
		string path = Path.Combine(fullName, "OnlineFix.ini");
		if (!File.Exists(path))
		{
			return;
		}
		string[] array;
		try
		{
			array = File.ReadAllLines(path);
		}
		catch (Exception ex)
		{
			Console.LogWarning("Error reading INI file: " + ex.Message);
			return;
		}
		int num = -1;
		int num2 = -1;
		string text = null;
		string text2 = null;
		for (int i = 0; i < array.Length; i++)
		{
			string text3 = array[i].Trim();
			if (text3.StartsWith("RealAppId="))
			{
				num = i;
				text = text3.Substring("RealAppId=".Length);
			}
			else if (text3.StartsWith("FakeAppId="))
			{
				num2 = i;
				text2 = text3.Substring("FakeAppId=".Length);
			}
		}
		if (num == -1 || num2 == -1)
		{
			return;
		}
		array[num] = "RealAppId=" + text2;
		array[num2] = "FakeAppId=" + text;
		try
		{
			File.WriteAllLines(path, array);
		}
		catch (Exception ex2)
		{
			Console.LogError("Error writing INI file: " + ex2.Message);
		}
	}

	private void InitializeItemLoaders()
	{
		new ItemLoader();
		new WateringCanLoader();
		new CashLoader();
		new QualityItemLoader();
		new ProductItemLoader();
		new WeedLoader();
		new MethLoader();
		new CocaineLoader();
		new ShroomLoader();
		new IntegerItemLoader();
		new TrashGrabberLoader();
		new ClothingLoader();
	}

	private void InitializeObjectLoaders()
	{
		new BuildableItemLoader();
		new GridItemLoader();
		new ProceduralGridItemLoader();
		new SurfaceItemLoader();
		new ToggleableItemLoader();
		new PotLoader();
		new MushroomBedLoader();
		new PackagingStationLoader();
		new PlaceableStorageEntityLoader();
		new ChemistryStationLoader();
		new LabOvenLoader();
		new BrickPressLoader();
		new MixingStationLoader();
		new CauldronLoader();
		new TrashContainerLoader();
		new SoilPourerLoader();
		new DryingRackLoader();
		new JukeboxLoader();
		new ToggleableSurfaceItemLoader();
		new StorageSurfaceItemLoader();
		new LabelledSurfaceItemLoader();
		new SpawnStationLoader();
		new AirConditionerLoader();
	}

	private void InitializeNPCLoaders()
	{
		new NPCsLoader();
		new EmployeeLoader();
		new PackagerLoader();
		new BotanistLoader();
		new ChemistLoader();
		new CleanerLoader();
		new LegacyNPCLoader();
		new LegacyEmployeeLoader();
		new LegacyPackagerLoader();
		new LegacyBotanistLoader();
		new LegacyChemistLoader();
		new LegacyCleanerLoader();
	}

	public void Update()
	{
		if (IsGameLoaded && LoadedGameFolderPath != string.Empty && Input.GetKeyDown((KeyCode)287) && (Application.isEditor || Debug.isDebugBuild))
		{
			NetworkManager obj = Object.FindObjectOfType<NetworkManager>();
			obj.ClientManager.StopConnection();
			obj.ServerManager.StopConnection(false);
			StartGame(new SaveInfo(LoadedGameFolderPath, -1, "Test Org", DateTime.Now, DateTime.Now, 0f, Application.version, new MetaData(null, null, string.Empty, string.Empty, playTutorial: false)), allowLoadStacking: true);
		}
		if (IsGameLoaded && LoadStatus == ELoadStatus.None)
		{
			TimeSinceGameLoaded += Time.deltaTime;
		}
	}

	public void QueueLoadRequest(LoadRequest request)
	{
		loadRequests.Add(request);
	}

	public void DequeueLoadRequest(LoadRequest request)
	{
		loadRequests.Remove(request);
	}

	public ItemLoader GetItemLoader(string itemType)
	{
		ItemLoader itemLoader = ItemLoaders.Find((ItemLoader loader) => loader.ItemType == itemType);
		if (itemLoader == null)
		{
			Console.LogError("No item loader found for data type: " + itemType);
			return null;
		}
		return itemLoader;
	}

	public BuildableItemLoader GetObjectLoader(string objectType)
	{
		BuildableItemLoader buildableItemLoader = ObjectLoaders.Find((BuildableItemLoader loader) => loader.ItemType == objectType);
		if (buildableItemLoader == null)
		{
			Console.LogError("No object loader found for data type: " + objectType);
			return null;
		}
		return buildableItemLoader;
	}

	public LegacyNPCLoader GetLegacyNPCLoader(string npcType)
	{
		LegacyNPCLoader legacyNPCLoader = LegacyNPCLoaders.Find((LegacyNPCLoader loader) => loader.NPCType == npcType);
		if (legacyNPCLoader == null)
		{
			Console.LogError("No NPC loader found for NPC type: " + npcType);
			return null;
		}
		return legacyNPCLoader;
	}

	public NPCLoader GetNPCLoader(string npcType)
	{
		NPCLoader nPCLoader = NPCLoaders.Find((NPCLoader loader) => loader.NPCType == npcType);
		if (nPCLoader == null)
		{
			Console.LogError("No NPC loader found for NPC type: " + npcType);
			return null;
		}
		return nPCLoader;
	}

	public string GetLoadStatusText()
	{
		switch (LoadStatus)
		{
		case ELoadStatus.LoadingScene:
			return "Loading world...";
		case ELoadStatus.Initializing:
			if (InstanceFinder.IsServer)
			{
				return "Initializing...";
			}
			if (NetworkSingleton<ReplicationQueue>.InstanceExists)
			{
				return "Syncing " + NetworkSingleton<ReplicationQueue>.Instance.CurrentReplicationTask + "...";
			}
			return "Syncing...";
		case ELoadStatus.SpawningPlayer:
			return "Spawning player...";
		case ELoadStatus.LoadingData:
			return "Loading data...";
		case ELoadStatus.WaitingForHost:
			return "Waiting for host to finish loading...";
		default:
			return string.Empty;
		}
	}

	public void StartGame(SaveInfo info, bool allowLoadStacking = false, bool allowSaveBackup = true)
	{
		if (IsGameLoaded && !allowLoadStacking)
		{
			Console.LogWarning("Game already loaded, cannot start another");
			return;
		}
		if (info == null)
		{
			Console.LogWarning("Save info is null, cannot start game");
			return;
		}
		string savePath = info.SavePath;
		if (!Directory.Exists(savePath))
		{
			Console.LogWarning("Save game does not exist at " + savePath);
			return;
		}
		Singleton<MusicManager>.Instance.StopAndDisableTracks();
		Console.Log("Starting game!");
		ActiveSaveInfo = info;
		IsLoading = true;
		TimeSinceGameLoaded = 0f;
		LoadedGameFolderPath = info.SavePath;
		if (OnLocalSaveLoadStart != null)
		{
			OnLocalSaveLoadStart(LoadedGameFolderPath);
		}
		if (LoadManager.onLoadConfigurations != null)
		{
			LoadManager.onLoadConfigurations();
		}
		LoadHistory.Add("Loading game: " + ActiveSaveInfo.OrganisationName);
		((MonoBehaviour)this).StartCoroutine(LoadRoutine());
		IEnumerator LoadRoutine()
		{
			bool playingTutorial = info.MetaData.PlayTutorial;
			if (Singleton<Lobby>.Instance.IsInLobby && Singleton<Lobby>.Instance.IsHost)
			{
				Console.Log("Sending host loading message to lobby");
				if (playingTutorial)
				{
					Singleton<Lobby>.Instance.SetLobbyData("load_tutorial", "true");
					Singleton<Lobby>.Instance.SendLobbyMessage("load_tutorial");
				}
				Singleton<Lobby>.Instance.SetLobbyData("host_loading", "true");
				Singleton<Lobby>.Instance.SendLobbyMessage("host_loading");
			}
			LoadStatus = ELoadStatus.LoadingScene;
			Singleton<LoadingScreen>.Instance.Open(playingTutorial);
			yield return (object)new WaitForSecondsRealtime(1.25f);
			if (Singleton<Settings>.Instance.OtherSettings.AutoBackupSaves)
			{
				float versionNumber = SaveManager.GetVersionNumber(info.SaveVersion);
				float versionNumber2 = SaveManager.GetVersionNumber(Application.version);
				if (versionNumber < versionNumber2)
				{
					Console.Log("Creating backup of save before loading (Auto Backup Saves is enabled).");
					Singleton<SaveManager>.Instance.CreateSaveBackup(info);
				}
			}
			if (InstanceFinder.IsServer)
			{
				InstanceFinder.NetworkManager.ServerManager.StopConnection(false);
			}
			if (InstanceFinder.IsClient)
			{
				InstanceFinder.NetworkManager.ClientManager.StopConnection();
			}
			if (onPreSceneChange != null)
			{
				onPreSceneChange.Invoke();
			}
			CleanUp();
			string text = "Main";
			if (playingTutorial)
			{
				StoredSaveInfo = info;
				text = "Tutorial";
				LoadedGameFolderPath = DefaultTutorialSaveFolder;
				((Component)InstanceFinder.NetworkManager).gameObject.GetComponent<DefaultScene>().SetOnlineScene("Tutorial");
			}
			else
			{
				StoredSaveInfo = null;
				if ((Object)(object)InstanceFinder.NetworkManager != (Object)null)
				{
					((Component)InstanceFinder.NetworkManager).gameObject.GetComponent<DefaultScene>().SetOnlineScene("Main");
				}
			}
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(text);
			while (!asyncLoad.isDone)
			{
				yield return (object)new WaitForEndOfFrame();
			}
			Scene activeScene = SceneManager.GetActiveScene();
			Console.Log("Scene loaded: " + ((Scene)(ref activeScene)).name);
			LoadStatus = ELoadStatus.Initializing;
			if (onPreLoad != null)
			{
				onPreLoad.Invoke();
			}
			Console.Log("Starting server...");
			global::FishySteamworks.FishySteamworks fishy;
			ushort port;
			if (Singleton<Lobby>.Instance.IsInLobby && Singleton<Lobby>.Instance.IsHost)
			{
				fishy = InstanceFinder.TransportManager.GetTransport<Multipass>().GetTransport<global::FishySteamworks.FishySteamworks>();
				((Transport)fishy).SetClientAddress(((object)Singleton<Lobby>.Instance.LocalPlayerID/*cast due to .constrained prefix*/).ToString());
				port = ((Transport)fishy).GetPort();
				((Transport)fishy).OnServerConnectionState += Done;
				((Transport)fishy).StartConnection(true);
			}
			else
			{
				Transport transport = (Transport)(object)InstanceFinder.TransportManager.GetTransport<Multipass>().GetTransport<Yak>();
				if (Application.isEditor || Debug.isDebugBuild)
				{
					Debug.Log((object)"Using Tugboat transport for local testing");
					transport = (Transport)(object)InstanceFinder.TransportManager.GetTransport<Multipass>().GetTransport<Tugboat>();
				}
				transport.SetPort((ushort)38465);
				transport.StartConnection(true);
				yield return (object)new WaitUntil((Func<bool>)(() => InstanceFinder.IsServer));
				Console.Log("Server initialized");
				InstanceFinder.TransportManager.GetTransport<Multipass>().SetClientTransport(transport);
				transport.SetClientAddress("localhost");
				transport.StartConnection(false);
			}
			yield return (object)new WaitUntil((Func<bool>)(() => InstanceFinder.NetworkManager.IsClient));
			Console.Log("Network initialized");
			LoadStatus = ELoadStatus.SpawningPlayer;
			yield return (object)new WaitUntil((Func<bool>)(() => (Object)(object)Player.Local != (Object)null));
			Console.Log("Local player spawned");
			LoadStatus = ELoadStatus.LoadingData;
			Console.Log("Load start!");
			foreach (IBaseSaveable item in Singleton<SaveManager>.Instance.BaseSaveables.OrderBy((IBaseSaveable x) => x.LoadOrder).ToList())
			{
				new LoadRequest(Path.Combine(LoadedGameFolderPath, item.SaveFolderName), item.Loader);
			}
			while (loadRequests.Count > 0)
			{
				for (int num = 0; num < 50; num++)
				{
					if (loadRequests.Count <= 0)
					{
						break;
					}
					LoadRequest loadRequest = loadRequests[0];
					try
					{
						loadRequest.Complete();
					}
					catch (Exception ex)
					{
						Console.LogError("LOAD ERROR for load request: " + loadRequest.Path + " : " + ex.Message + "\nSite: " + ex.TargetSite);
						if (loadRequests.FirstOrDefault() == loadRequest)
						{
							loadRequests.RemoveAt(0);
						}
					}
				}
				yield return (object)new WaitForEndOfFrame();
			}
			yield return (object)new WaitForEndOfFrame();
			yield return (object)new WaitForEndOfFrame();
			if (onLoadComplete != null)
			{
				onLoadComplete.Invoke();
			}
			yield return (object)new WaitForSeconds(2f);
			LoadStatus = ELoadStatus.None;
			Console.Log("Game loaded");
			Singleton<LoadingScreen>.Instance.Close();
			IsLoading = false;
			IsGameLoaded = true;
			if (Singleton<Lobby>.Instance.IsInLobby && Singleton<Lobby>.Instance.IsHost)
			{
				Singleton<Lobby>.Instance.SetLobbyData("host_loading", "false");
				if (!playingTutorial)
				{
					Console.Log("Sending join ready message to lobby");
					Singleton<Lobby>.Instance.SetLobbyData("ready", "true");
					Singleton<Lobby>.Instance.SendLobbyMessage("ready");
				}
			}
			unsafe void Done(ServerConnectionStateArgs args)
			{
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				//IL_003a: Invalid comparison between Unknown and I4
				Console.Log("Server connection state: " + ((object)(*(LocalConnectionState*)(&args.ConnectionState))/*cast due to .constrained prefix*/).ToString() + " and transport index: " + args.TransportIndex);
				if ((int)args.ConnectionState == 2)
				{
					Console.Log("Server intialized");
					((Transport)fishy).OnServerConnectionState -= Done;
					Console.Log("Starting FishySteamworks client connection: " + fishy.LocalUserSteamID);
					InstanceFinder.TransportManager.GetTransport<Multipass>().SetClientTransport<global::FishySteamworks.FishySteamworks>();
					InstanceFinder.NetworkManager.ClientManager.StartConnection(fishy.LocalUserSteamID.ToString(), port);
					InstanceFinder.TransportManager.Transport.SetTimeout(30f, true);
				}
			}
		}
	}

	public void LoadTutorialAsClient()
	{
		bool waitForExit = false;
		if (IsGameLoaded)
		{
			Console.LogWarning("Game already loaded, exiting");
			waitForExit = true;
			ExitToMenu();
		}
		((MonoBehaviour)this).StartCoroutine(LoadRoutine());
		IEnumerator LoadRoutine()
		{
			if (waitForExit)
			{
				yield return (object)new WaitUntil((Func<bool>)delegate
				{
					//IL_000d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0012: Unknown result type (might be due to invalid IL or missing references)
					if (!IsLoading)
					{
						Scene activeScene2 = SceneManager.GetActiveScene();
						return ((Scene)(ref activeScene2)).name == "Menu";
					}
					return false;
				});
			}
			LoadHistory.Add("Loading as client to tutorial");
			ActiveSaveInfo = null;
			IsLoading = true;
			TimeSinceGameLoaded = 0f;
			LoadedGameFolderPath = string.Empty;
			LoadStatus = ELoadStatus.LoadingScene;
			Singleton<LoadingScreen>.Instance.Open(loadingTutorial: true);
			yield return (object)new WaitForSecondsRealtime(1.25f);
			if (InstanceFinder.IsServer)
			{
				InstanceFinder.NetworkManager.ServerManager.StopConnection(false);
			}
			if (InstanceFinder.IsClient)
			{
				InstanceFinder.NetworkManager.ClientManager.StopConnection();
			}
			if (onPreSceneChange != null)
			{
				onPreSceneChange.Invoke();
			}
			CleanUp();
			string text = "Tutorial";
			LoadedGameFolderPath = DefaultTutorialSaveFolder;
			((Component)InstanceFinder.NetworkManager).gameObject.GetComponent<DefaultScene>().SetOnlineScene("Tutorial");
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(text);
			while (!asyncLoad.isDone)
			{
				yield return (object)new WaitForEndOfFrame();
			}
			Scene activeScene = SceneManager.GetActiveScene();
			Console.Log("Scene loaded: " + ((Scene)(ref activeScene)).name);
			LoadStatus = ELoadStatus.Initializing;
			if (onPreLoad != null)
			{
				onPreLoad.Invoke();
			}
			Console.Log("Starting server");
			Yak yak = InstanceFinder.TransportManager.GetTransport<Multipass>().GetTransport<Yak>();
			((Transport)yak).SetPort((ushort)38465);
			((Transport)yak).StartConnection(true);
			yield return (object)new WaitUntil((Func<bool>)(() => InstanceFinder.IsServer));
			Console.Log("Server initialized");
			InstanceFinder.TransportManager.GetTransport<Multipass>().SetClientTransport((Transport)(object)yak);
			((Transport)yak).SetClientAddress("localhost");
			((Transport)yak).StartConnection(false);
			yield return (object)new WaitUntil((Func<bool>)(() => InstanceFinder.NetworkManager.IsClient));
			Console.Log("Network initialized");
			LoadStatus = ELoadStatus.SpawningPlayer;
			yield return (object)new WaitUntil((Func<bool>)(() => (Object)(object)Player.Local != (Object)null));
			Console.Log("Local player spawned");
			LoadStatus = ELoadStatus.LoadingData;
			Console.Log("Load start!");
			foreach (IBaseSaveable baseSaveable in Singleton<SaveManager>.Instance.BaseSaveables)
			{
				new LoadRequest(Path.Combine(LoadedGameFolderPath, baseSaveable.SaveFolderName), baseSaveable.Loader);
			}
			while (loadRequests.Count > 0)
			{
				for (int num = 0; num < 50; num++)
				{
					if (loadRequests.Count <= 0)
					{
						break;
					}
					LoadRequest loadRequest = loadRequests[0];
					try
					{
						loadRequest.Complete();
					}
					catch (Exception ex)
					{
						Console.LogError("LOAD ERROR for load request: " + loadRequest.Path + " : " + ex.Message + "\nSite: " + ex.TargetSite);
						if (loadRequests.FirstOrDefault() == loadRequest)
						{
							loadRequests.RemoveAt(0);
						}
					}
				}
				yield return (object)new WaitForEndOfFrame();
			}
			yield return (object)new WaitForEndOfFrame();
			yield return (object)new WaitForEndOfFrame();
			if (onLoadComplete != null)
			{
				onLoadComplete.Invoke();
			}
			yield return (object)new WaitForSeconds(1f);
			LoadStatus = ELoadStatus.None;
			Console.Log("Game loaded");
			Singleton<LoadingScreen>.Instance.Close();
			IsLoading = false;
			IsGameLoaded = true;
		}
	}

	public void LoadAsClient(string steamId64)
	{
		bool waitForExit = false;
		if (IsGameLoaded)
		{
			Console.LogWarning("Game already loaded, exiting");
			waitForExit = true;
			ExitToMenu();
		}
		((MonoBehaviour)this).StartCoroutine(LoadRoutine());
		IEnumerator LoadRoutine()
		{
			if (waitForExit)
			{
				yield return (object)new WaitUntil((Func<bool>)delegate
				{
					//IL_000d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0012: Unknown result type (might be due to invalid IL or missing references)
					if (!IsLoading)
					{
						Scene activeScene2 = SceneManager.GetActiveScene();
						return ((Scene)(ref activeScene2)).name == "Menu";
					}
					return false;
				});
			}
			Console.Log("Joining as client to: " + steamId64);
			LoadHistory.Add("Loading as client to: " + steamId64);
			ActiveSaveInfo = null;
			IsLoading = true;
			TimeSinceGameLoaded = 0f;
			LoadedGameFolderPath = string.Empty;
			LoadStatus = ELoadStatus.LoadingScene;
			Singleton<LoadingScreen>.Instance.Open();
			StartLoadErrorAutosubmit();
			if (onPreSceneChange != null)
			{
				onPreSceneChange.Invoke();
			}
			CleanUp();
			if (steamId64 == "localhost")
			{
				InstanceFinder.TransportManager.GetTransport<Multipass>().SetClientTransport<Tugboat>();
			}
			else
			{
				InstanceFinder.TransportManager.GetTransport<Multipass>().SetClientTransport<global::FishySteamworks.FishySteamworks>();
			}
			InstanceFinder.TransportManager.Transport.SetTimeout(30f, false);
			if (steamId64 == "localhost")
			{
				Tugboat transport = InstanceFinder.TransportManager.GetTransport<Multipass>().GetTransport<Tugboat>();
				((Transport)transport).SetPort((ushort)38465);
				((Transport)transport).SetClientAddress("localhost");
				((Transport)transport).StartConnection(false);
			}
			else
			{
				InstanceFinder.ClientManager.StartConnection(steamId64);
			}
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
			yield return (object)new WaitUntil((Func<bool>)delegate
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				Scene activeScene2 = SceneManager.GetActiveScene();
				return ((Scene)(ref activeScene2)).name == "Main";
			});
			Scene activeScene = SceneManager.GetActiveScene();
			Console.Log("Scene loaded: " + ((Scene)(ref activeScene)).name);
			if (onPreLoad != null)
			{
				onPreLoad.Invoke();
			}
			LoadStatus = ELoadStatus.SpawningPlayer;
			yield return (object)new WaitUntil((Func<bool>)(() => (Object)(object)Player.Local != (Object)null));
			Console.Log("Local player spawned");
			LoadStatus = ELoadStatus.LoadingData;
			yield return (object)new WaitUntil((Func<bool>)(() => Player.Local.playerDataRetrieveReturned));
			Console.Log("Player data retrieved");
			LoadStatus = ELoadStatus.Initializing;
			yield return (object)new WaitUntil((Func<bool>)(() => NetworkSingleton<ReplicationQueue>.Instance.ReplicationDoneForLocalPlayer || NetworkSingleton<ReplicationQueue>.Instance.LocalPlayerReplicationTimedOut));
			if (NetworkSingleton<ReplicationQueue>.Instance.LocalPlayerReplicationTimedOut)
			{
				Console.LogWarning("Local player replication timed out. Current task: " + NetworkSingleton<ReplicationQueue>.Instance.CurrentReplicationTask);
			}
			if (onLoadComplete != null)
			{
				onLoadComplete.Invoke();
			}
			yield return (object)new WaitForSeconds(1f);
			LoadStatus = ELoadStatus.None;
			Console.Log("Game loaded as client");
			Singleton<LoadingScreen>.Instance.Close();
			IsLoading = false;
			IsGameLoaded = true;
		}
		static void PlayerSpawned()
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
			Console.Log("Local player spawned");
		}
	}

	private void StartLoadErrorAutosubmit()
	{
		((MonoBehaviour)this).StartCoroutine(Wait());
		IEnumerator Wait()
		{
			for (float t = 0f; t < 90f; t += Time.deltaTime)
			{
				if (LoadStatus == ELoadStatus.None)
				{
					yield break;
				}
				yield return (object)new WaitForEndOfFrame();
			}
			if (Singleton<PauseMenu>.InstanceExists)
			{
				Console.LogError("Load error timeout reached, submitting error report");
				Singleton<PauseMenu>.Instance.FeedbackForm.SetFormData("[AUTOREPORT] Load as client error");
				Singleton<PauseMenu>.Instance.FeedbackForm.SetCategory("Bugs - Multiplayer");
				((FeedbackForm)Singleton<PauseMenu>.Instance.FeedbackForm).IncludeScreenshot = false;
				((FeedbackForm)Singleton<PauseMenu>.Instance.FeedbackForm).IncludeSaveFile = false;
				((FeedbackForm)Singleton<PauseMenu>.Instance.FeedbackForm).Submit();
			}
		}
	}

	public void SetWaitingForHostLoad()
	{
		IsLoading = true;
		LoadStatus = ELoadStatus.WaitingForHost;
	}

	public void LoadLastSave()
	{
		if (ActiveSaveInfo == null)
		{
			Console.LogWarning("No active save info, cannot load last save");
		}
		else
		{
			StartGame(ActiveSaveInfo, allowLoadStacking: true);
		}
	}

	private void CleanUp()
	{
		Debug.Log((object)"Cleaning up...");
		GUIDManager.Clear();
		Quest.Quests.Clear();
		Quest.ActiveQuests.Clear();
		NodeLink.validNodeLinks.Clear();
		Player.onLocalPlayerSpawned = null;
		Player.PlayerList.Clear();
		SupplierLocation.AllLocations.Clear();
		Phone.ActiveApp = null;
		ATM.WeeklyDepositSum = 0f;
		NavMeshUtility.ClearCache();
		Business.OwnedBusinesses.Clear();
		Business.UnownedBusinesses.Clear();
		Business.onOperationFinished = null;
		Business.onOperationStarted = null;
		ScheduleOne.Property.Property.onPropertyAcquired = null;
		ScheduleOne.Property.Property.OwnedProperties.Clear();
		ScheduleOne.Property.Property.UnownedProperties.Clear();
		PlayerMovement.StaticMoveSpeedMultiplier = 1f;
		AvatarLookController.TempContainer = null;
		Customer.onCustomerUnlocked = null;
		Customer.UnlockedCustomers.Clear();
		Customer.LockedCustomers.Clear();
		staggeredReplicators.Clear();
		ManagementClipboard_Equippable.ResetHeatmapToggle();
	}

	public void ExitToMenu(SaveInfo autoLoadSave = null, MainMenuPopup.Data mainMenuPopup = null, bool preventLeaveLobby = false)
	{
		if (!IsGameLoaded)
		{
			Console.LogWarning("Game not loaded, cannot exit to menu");
			return;
		}
		Console.Log("Exiting to menu");
		LoadHistory.Add("Exiting to menu");
		if ((Object)(object)Player.Local != (Object)null && InstanceFinder.IsServer)
		{
			Player.Local.HostExitedGame();
		}
		if (Singleton<Lobby>.InstanceExists && Singleton<Lobby>.Instance.IsInLobby && !preventLeaveLobby)
		{
			Singleton<Lobby>.Instance.LeaveLobby();
		}
		Cursor.lockState = (CursorLockMode)0;
		Cursor.visible = true;
		if (Singleton<CursorManager>.InstanceExists)
		{
			Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
		}
		IsGameLoaded = false;
		ActiveSaveInfo = null;
		IsLoading = true;
		Time.timeScale = 1f;
		Singleton<MusicManager>.Instance.StopAndDisableTracks();
		((MonoBehaviour)this).StartCoroutine(Load());
		IEnumerator Load()
		{
			Singleton<LoadingScreen>.Instance.Open();
			if (!InstanceFinder.IsServer)
			{
				Console.Log("Requesting server to save player data");
				Player.Local.RequestSavePlayer();
				float maxWait = 3f;
				float timeOnWaitStart = Time.realtimeSinceStartup;
				yield return (object)new WaitUntil((Func<bool>)(() => Player.Local.playerSaveRequestReturned || Time.realtimeSinceStartup - timeOnWaitStart > maxWait));
				Console.Log("Player data saved");
			}
			yield return (object)new WaitForSecondsRealtime(1.25f);
			try
			{
				if (onPreSceneChange != null)
				{
					onPreSceneChange.Invoke();
				}
			}
			catch (Exception ex)
			{
				Console.LogError("Error invoking pre scene change event: " + ex.Message);
			}
			Console.Log("Pre scene change event invoked");
			InstanceFinder.NetworkManager.ServerManager.StopConnection(true);
			InstanceFinder.NetworkManager.ClientManager.StopConnection();
			Console.Log("Connection stopped");
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Menu");
			while (!asyncLoad.isDone)
			{
				yield return (object)new WaitForEndOfFrame();
			}
			Console.Log("Menu scene loaded");
			bool flag = Singleton<Lobby>.Instance.IsInLobby && !Singleton<Lobby>.Instance.IsHost;
			if (autoLoadSave != null || flag)
			{
				if (Singleton<Lobby>.Instance.IsInLobby)
				{
					if (Singleton<Lobby>.Instance.IsHost)
					{
						IsLoading = false;
						StartGame(autoLoadSave, allowLoadStacking: false, allowSaveBackup: false);
						Console.Log("Disabling load_tutorial flag");
						Singleton<Lobby>.Instance.SetLobbyData("load_tutorial", "false");
					}
					else if (SteamMatchmaking.GetLobbyData(Singleton<Lobby>.Instance.LobbySteamID, "ready") == "true")
					{
						LoadAsClient(SteamMatchmaking.GetLobbyOwner(Singleton<Lobby>.Instance.LobbySteamID).m_SteamID.ToString());
					}
					else
					{
						SetWaitingForHostLoad();
					}
				}
				else
				{
					IsLoading = false;
					StartGame(autoLoadSave, allowLoadStacking: false, allowSaveBackup: false);
				}
			}
			else
			{
				RefreshSaveInfo();
				yield return (object)new WaitForSeconds(0.5f);
				Cursor.lockState = (CursorLockMode)0;
				Cursor.visible = true;
				if (mainMenuPopup != null && Singleton<MainMenuPopup>.InstanceExists)
				{
					Singleton<MainMenuPopup>.Instance.Open(mainMenuPopup);
				}
				Singleton<LoadingScreen>.Instance.Close();
				IsLoading = false;
			}
		}
	}

	public static bool TryLoadSaveInfo(string saveFolderPath, int saveSlotIndex, out SaveInfo saveInfo, bool requireGameFile = false)
	{
		saveInfo = null;
		if (Directory.Exists(saveFolderPath))
		{
			string path = Path.Combine(saveFolderPath, "Metadata.json");
			MetaData metaData = null;
			if (File.Exists(path))
			{
				string text = string.Empty;
				try
				{
					text = File.ReadAllText(path);
				}
				catch (Exception ex)
				{
					Console.LogError("Error reading save metadata: " + ex.Message);
				}
				if (!string.IsNullOrEmpty(text))
				{
					try
					{
						metaData = JsonUtility.FromJson<MetaData>(text);
					}
					catch (Exception ex2)
					{
						metaData = null;
						Console.LogError("Error parsing save metadata: " + ex2.Message);
					}
				}
				else
				{
					Console.LogWarning("Metadata is empty");
				}
			}
			string path2 = Path.Combine(saveFolderPath, "Game.json");
			GameData gameData = null;
			if (File.Exists(path2))
			{
				string text2 = string.Empty;
				try
				{
					text2 = File.ReadAllText(path2);
				}
				catch (Exception ex3)
				{
					Console.LogError("Error reading save game data: " + ex3.Message);
				}
				if (!string.IsNullOrEmpty(text2))
				{
					try
					{
						gameData = JsonUtility.FromJson<GameData>(text2);
					}
					catch (Exception ex4)
					{
						gameData = null;
						Console.LogError("Error parsing save game data: " + ex4.Message);
					}
				}
				else
				{
					Console.LogWarning("Game data is empty");
				}
			}
			float networth = 0f;
			string path3 = Path.Combine(saveFolderPath, "Money.json");
			MoneyData moneyData = null;
			if (File.Exists(path3))
			{
				string text3 = string.Empty;
				try
				{
					text3 = File.ReadAllText(path3);
				}
				catch (Exception ex5)
				{
					Console.LogError("Error reading save money data: " + ex5.Message);
				}
				if (!string.IsNullOrEmpty(text3))
				{
					try
					{
						moneyData = JsonUtility.FromJson<MoneyData>(text3);
					}
					catch (Exception ex6)
					{
						moneyData = null;
						Console.LogError("Error parsing save money data: " + ex6.Message);
					}
				}
				else
				{
					Console.LogWarning("Money data is empty");
				}
				if (moneyData != null)
				{
					networth = moneyData.Networth;
				}
			}
			if (metaData == null)
			{
				Console.LogWarning("Failed to load metadata. Setting default");
				metaData = new MetaData(new DateTimeData(DateTime.Now), new DateTimeData(DateTime.Now), Application.version, Application.version, playTutorial: false);
				try
				{
					File.WriteAllText(path, metaData.GetJson());
				}
				catch (Exception)
				{
				}
			}
			if (gameData == null)
			{
				if (requireGameFile)
				{
					return false;
				}
				Console.LogWarning("Failed to load game data. Setting default");
				gameData = new GameData("Unknown", Random.Range(0, int.MaxValue), new GameSettings());
				try
				{
					File.WriteAllText(path2, gameData.GetJson());
				}
				catch (Exception)
				{
				}
			}
			saveInfo = new SaveInfo(saveFolderPath, saveSlotIndex, gameData.OrganisationName, metaData.CreationDate.GetDateTime(), metaData.LastPlayedDate.GetDateTime(), networth, metaData.LastSaveVersion, metaData);
			return true;
		}
		return false;
	}

	public void RefreshSaveInfo()
	{
		for (int i = 0; i < 5; i++)
		{
			SaveGames[i] = null;
			if (TryLoadSaveInfo(Path.Combine(Singleton<SaveManager>.Instance.IndividualSavesContainerPath, "SaveGame_" + (i + 1)), i + 1, out var saveInfo))
			{
				SaveGames[i] = saveInfo;
			}
			else
			{
				SaveGames[i] = null;
			}
		}
		LastPlayedGame = null;
		for (int j = 0; j < SaveGames.Length; j++)
		{
			if (SaveGames[j] != null && (LastPlayedGame == null || SaveGames[j].DateLastPlayed > LastPlayedGame.DateLastPlayed))
			{
				LastPlayedGame = SaveGames[j];
			}
		}
		if (onSaveInfoLoaded != null)
		{
			onSaveInfoLoaded.Invoke();
		}
	}

	public void AddStaggeredReplicator(IStaggeredReplicator replicator)
	{
		if (!staggeredReplicators.Contains(replicator))
		{
			staggeredReplicators.Add(replicator);
		}
	}
}
