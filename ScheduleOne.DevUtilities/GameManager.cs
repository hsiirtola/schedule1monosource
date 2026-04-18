using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Core;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ScheduleOne.DevUtilities;

public class GameManager : NetworkSingleton<GameManager>, IBaseSaveable, ISaveable
{
	public const bool IS_DEMO = false;

	public static bool IS_BETA;

	[SerializeField]
	private int seed;

	public string OrganisationName = "Organisation";

	public GameSettings Settings = new GameSettings();

	public Transform SpawnPoint;

	public Transform NoHomeRespawnPoint;

	public Transform Temp;

	public UnityEvent onSettingsLoaded = new UnityEvent();

	private GameDataLoader loader = new GameDataLoader();

	private bool NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002EGameManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EDevUtilities_002EGameManagerAssembly_002DCSharp_002Edll_Excuted;

	public static bool IS_TUTORIAL
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			Scene activeScene = SceneManager.GetActiveScene();
			return ((Scene)(ref activeScene)).name == "Tutorial";
		}
	}

	public static int Seed
	{
		get
		{
			if ((Object)(object)NetworkSingleton<GameManager>.Instance != (Object)null)
			{
				return NetworkSingleton<GameManager>.Instance.seed;
			}
			return 0;
		}
	}

	public Sprite OrganisationLogo { get; protected set; }

	public bool IsTutorial
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			Scene activeScene = SceneManager.GetActiveScene();
			return ((Scene)(ref activeScene)).name == "Tutorial";
		}
	}

	public string SaveFolderName => "Game";

	public string SaveFileName => "Game";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string> { "Logo.png" };

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public int LoadOrder { get; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EDevUtilities_002EGameManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost)
		{
			SetGameData(connection, new GameData(OrganisationName, seed, Settings));
		}
	}

	[TargetRpc]
	public void SetGameData(NetworkConnection conn, GameData data)
	{
		RpcWriter___Target_SetGameData_3076874643(conn, data);
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public virtual string GetSaveString()
	{
		return new GameData(OrganisationName, seed, Settings).GetJson();
	}

	public void Load(GameData data, string path)
	{
		OrganisationName = data.OrganisationName;
		seed = data.Seed;
		Settings = data.Settings;
		if (onSettingsLoaded != null)
		{
			onSettingsLoaded.Invoke();
		}
		HasChanged = true;
	}

	[Button]
	public void EndTutorial(bool natural)
	{
		if (IsTutorial)
		{
			Console.Log("Ending tutorial...");
			if (Singleton<LoadManager>.Instance.StoredSaveInfo != null && (!Singleton<Lobby>.Instance.IsInLobby || Singleton<Lobby>.Instance.IsHost))
			{
				Singleton<SaveManager>.Instance.DisablePlayTutorial(Singleton<LoadManager>.Instance.StoredSaveInfo);
				Singleton<LoadManager>.Instance.StoredSaveInfo.MetaData.PlayTutorial = false;
			}
			if (natural)
			{
				AchievementManager.UnlockAchievement(AchievementManager.EAchievement.COMPLETE_PROLOGUE);
			}
			Singleton<LoadManager>.Instance.ExitToMenu(Singleton<LoadManager>.Instance.StoredSaveInfo, null, preventLeaveLobby: true);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002EGameManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EDevUtilities_002EGameManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterTargetRpc(0u, new ClientRpcDelegate(RpcReader___Target_SetGameData_3076874643));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EDevUtilities_002EGameManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EDevUtilities_002EGameManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Target_SetGameData_3076874643(NetworkConnection conn, GameData data)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002EDatas_002EGameDataFishNet_002ESerializing_002EGenerated((Writer)(object)writer, data);
			((NetworkBehaviour)this).SendTargetRpc(0u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	public void RpcLogic___SetGameData_3076874643(NetworkConnection conn, GameData data)
	{
		OrganisationName = data.OrganisationName;
		seed = data.Seed;
		Settings = data.Settings;
		if (onSettingsLoaded != null)
		{
			onSettingsLoaded.Invoke();
		}
	}

	private void RpcReader___Target_SetGameData_3076874643(PooledReader PooledReader0, Channel channel)
	{
		GameData data = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002EDatas_002EGameDataFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetGameData_3076874643(((NetworkBehaviour)this).LocalConnection, data);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EDevUtilities_002EGameManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		CrashReportHandler.logBufferSize = 50u;
		InitializeSaveable();
	}
}
