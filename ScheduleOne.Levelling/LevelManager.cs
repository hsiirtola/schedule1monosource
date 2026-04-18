using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Levelling;

public class LevelManager : NetworkSingleton<LevelManager>, IBaseSaveable, ISaveable
{
	public const int TIERS_PER_RANK = 5;

	public const int XP_PER_TIER_MIN = 200;

	public const int XP_PER_TIER_MAX = 2500;

	private int rankCount;

	public Action<FullRank, FullRank> onRankUp;

	public Action<FullRank, FullRank> onRankChanged;

	public Dictionary<FullRank, List<Unlockable>> Unlockables = new Dictionary<FullRank, List<Unlockable>>();

	private RankLoader loader = new RankLoader();

	private bool NetworkInitialize___EarlyScheduleOne_002ELevelling_002ELevelManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ELevelling_002ELevelManagerAssembly_002DCSharp_002Edll_Excuted;

	public ERank Rank { get; private set; }

	public int Tier { get; private set; } = 1;

	public int XP { get; private set; }

	public int TotalXP { get; private set; }

	public float XPToNextTier => Mathf.Round(Mathf.Lerp(200f, 2500f, (float)Rank / (float)rankCount) / 25f) * 25f;

	public string SaveFolderName => "Rank";

	public string SaveFileName => "Rank";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public int LoadOrder { get; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ELevelling_002ELevelManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		base.Start();
		InitializeSaveable();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost)
		{
			SetData(connection, Rank, Tier, XP, TotalXP);
			SetUnlockedRegions(connection, Singleton<ScheduleOne.Map.Map>.Instance.GetUnlockedRegions());
		}
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	[ServerRpc(RequireOwnership = false)]
	public void AddXP(int xp)
	{
		RpcWriter___Server_AddXP_3316948804(xp);
	}

	[ObserversRpc]
	private void AddXPLocal(int xp)
	{
		RpcWriter___Observers_AddXPLocal_3316948804(xp);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetData(NetworkConnection conn, ERank rank, int tier, int xp, int totalXp)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetData_20965027(conn, rank, tier, xp, totalXp);
			RpcLogic___SetData_20965027(conn, rank, tier, xp, totalXp);
		}
		else
		{
			RpcWriter___Target_SetData_20965027(conn, rank, tier, xp, totalXp);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetUnlockedRegions(NetworkConnection conn, List<EMapRegion> unlockedRegions)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetUnlockedRegions_563230222(conn, unlockedRegions);
			RpcLogic___SetUnlockedRegions_563230222(conn, unlockedRegions);
		}
		else
		{
			RpcWriter___Target_SetUnlockedRegions_563230222(conn, unlockedRegions);
		}
	}

	[ObserversRpc]
	private void IncreaseTierNetworked(FullRank before, FullRank after)
	{
		RpcWriter___Observers_IncreaseTierNetworked_3953286437(before, after);
	}

	private void IncreaseTier()
	{
		XP -= (int)XPToNextTier;
		Tier++;
		if (Tier > 5 && Rank != ERank.Kingpin)
		{
			Tier = 1;
			Rank++;
		}
	}

	public virtual string GetSaveString()
	{
		return new RankData((int)Rank, Tier, XP, TotalXP, Singleton<ScheduleOne.Map.Map>.Instance.GetUnlockedRegions()).GetJson();
	}

	public FullRank GetFullRank()
	{
		return new FullRank(Rank, Tier);
	}

	public void AddUnlockable(Unlockable unlockable)
	{
		if (!Unlockables.ContainsKey(unlockable.Rank))
		{
			Unlockables.Add(unlockable.Rank, new List<Unlockable>());
		}
		if (Unlockables[unlockable.Rank].Find((Unlockable x) => x.Title.ToLower() == unlockable.Title.ToLower() && (Object)(object)x.Icon == (Object)(object)unlockable.Icon) == null)
		{
			Unlockables[unlockable.Rank].Add(unlockable);
		}
	}

	public int GetTotalXPForRank(FullRank fullrank)
	{
		int num = 0;
		ERank[] array = (ERank[])Enum.GetValues(typeof(ERank));
		foreach (ERank eRank in array)
		{
			int xPForTier = GetXPForTier(eRank);
			int num2 = 5;
			if (eRank == ERank.Kingpin)
			{
				num2 = 1000;
			}
			for (int j = 1; j <= num2; j++)
			{
				if (eRank == fullrank.Rank && j == fullrank.Tier)
				{
					return num;
				}
				num += xPForTier;
			}
		}
		Console.LogError("Rank not found: " + fullrank.ToString());
		return 0;
	}

	public FullRank GetFullRank(int totalXp)
	{
		int num = totalXp;
		ERank[] array = (ERank[])Enum.GetValues(typeof(ERank));
		foreach (ERank eRank in array)
		{
			int xPForTier = GetXPForTier(eRank);
			if (eRank == ERank.Kingpin)
			{
				for (int j = 1; j <= 1000; j++)
				{
					if (num < xPForTier)
					{
						return new FullRank(eRank, j);
					}
					num -= xPForTier;
				}
				continue;
			}
			for (int k = 1; k <= 5; k++)
			{
				if (num < xPForTier)
				{
					return new FullRank(eRank, k);
				}
				num -= xPForTier;
			}
		}
		Console.LogError("Rank not found for XP: " + totalXp);
		return new FullRank(ERank.Street_Rat, 1);
	}

	public int GetXPForTier(ERank rank)
	{
		return Mathf.RoundToInt(Mathf.Round(Mathf.Lerp(200f, 2500f, (float)rank / (float)rankCount) / 25f) * 25f);
	}

	public static float GetOrderLimitMultiplier(FullRank rank)
	{
		float rankOrderLimitMultiplier = GetRankOrderLimitMultiplier(rank.Rank);
		if (rank.Rank < ERank.Kingpin)
		{
			float rankOrderLimitMultiplier2 = GetRankOrderLimitMultiplier(rank.Rank + 1);
			float num = (float)(rank.Tier - 1) / 4f;
			return Mathf.Lerp(rankOrderLimitMultiplier, rankOrderLimitMultiplier2, num);
		}
		return Mathf.Clamp(GetRankOrderLimitMultiplier(ERank.Kingpin) + 0.1f * (float)(rank.Tier - 1), 1f, 10f);
	}

	private static float GetRankOrderLimitMultiplier(ERank rank)
	{
		return rank switch
		{
			ERank.Street_Rat => 1f, 
			ERank.Hoodlum => 1.25f, 
			ERank.Peddler => 1.5f, 
			ERank.Hustler => 1.75f, 
			ERank.Bagman => 2f, 
			ERank.Enforcer => 2.25f, 
			ERank.Shot_Caller => 2.5f, 
			ERank.Block_Boss => 2.75f, 
			ERank.Underlord => 3f, 
			ERank.Baron => 3.25f, 
			ERank.Kingpin => 3.5f, 
			_ => 1f, 
		};
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ELevelling_002ELevelManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ELevelling_002ELevelManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_AddXP_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_AddXPLocal_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_SetData_20965027));
			((NetworkBehaviour)this).RegisterTargetRpc(3u, new ClientRpcDelegate(RpcReader___Target_SetData_20965027));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_SetUnlockedRegions_563230222));
			((NetworkBehaviour)this).RegisterTargetRpc(5u, new ClientRpcDelegate(RpcReader___Target_SetUnlockedRegions_563230222));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_IncreaseTierNetworked_3953286437));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ELevelling_002ELevelManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ELevelling_002ELevelManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_AddXP_3316948804(int xp)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsClientInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteInt32(xp, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___AddXP_3316948804(int xp)
	{
		AddXPLocal(xp);
	}

	private void RpcReader___Server_AddXP_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int xp = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___AddXP_3316948804(xp);
		}
	}

	private void RpcWriter___Observers_AddXPLocal_3316948804(int xp)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(xp, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___AddXPLocal_3316948804(int xp)
	{
		NetworkSingleton<DailySummary>.Instance.AddXP(xp);
		XP += xp;
		TotalXP += xp;
		HasChanged = true;
		Console.Log("Rank progress: " + XP + "/" + XPToNextTier + " (Total " + TotalXP + ")");
		if (InstanceFinder.IsServer)
		{
			FullRank fullRank = GetFullRank();
			bool flag = false;
			while ((float)XP >= XPToNextTier)
			{
				IncreaseTier();
				flag = true;
			}
			SetData(null, Rank, Tier, XP, TotalXP);
			if (flag)
			{
				IncreaseTierNetworked(fullRank, GetFullRank());
			}
		}
	}

	private void RpcReader___Observers_AddXPLocal_3316948804(PooledReader PooledReader0, Channel channel)
	{
		int xp = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___AddXPLocal_3316948804(xp);
		}
	}

	private void RpcWriter___Observers_SetData_20965027(NetworkConnection conn, ERank rank, int tier, int xp, int totalXp)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerated((Writer)(object)writer, rank);
			((Writer)writer).WriteInt32(tier, (AutoPackType)1);
			((Writer)writer).WriteInt32(xp, (AutoPackType)1);
			((Writer)writer).WriteInt32(totalXp, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetData_20965027(NetworkConnection conn, ERank rank, int tier, int xp, int totalXp)
	{
		FullRank fullRank = GetFullRank();
		Rank = rank;
		Tier = tier;
		XP = xp;
		TotalXP = totalXp;
		if (onRankChanged != null)
		{
			onRankChanged(fullRank, GetFullRank());
		}
	}

	private void RpcReader___Observers_SetData_20965027(PooledReader PooledReader0, Channel channel)
	{
		ERank rank = GeneratedReaders___Internal.Read___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		int tier = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int xp = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int totalXp = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetData_20965027(null, rank, tier, xp, totalXp);
		}
	}

	private void RpcWriter___Target_SetData_20965027(NetworkConnection conn, ERank rank, int tier, int xp, int totalXp)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerated((Writer)(object)writer, rank);
			((Writer)writer).WriteInt32(tier, (AutoPackType)1);
			((Writer)writer).WriteInt32(xp, (AutoPackType)1);
			((Writer)writer).WriteInt32(totalXp, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(3u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetData_20965027(PooledReader PooledReader0, Channel channel)
	{
		ERank rank = GeneratedReaders___Internal.Read___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		int tier = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int xp = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		int totalXp = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetData_20965027(((NetworkBehaviour)this).LocalConnection, rank, tier, xp, totalXp);
		}
	}

	private void RpcWriter___Observers_SetUnlockedRegions_563230222(NetworkConnection conn, List<EMapRegion> unlockedRegions)
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
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMap_002EEMapRegion_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, unlockedRegions);
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetUnlockedRegions_563230222(NetworkConnection conn, List<EMapRegion> unlockedRegions)
	{
		foreach (EMapRegion unlockedRegion in unlockedRegions)
		{
			Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(unlockedRegion).SetUnlocked();
		}
	}

	private void RpcReader___Observers_SetUnlockedRegions_563230222(PooledReader PooledReader0, Channel channel)
	{
		List<EMapRegion> unlockedRegions = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMap_002EEMapRegion_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetUnlockedRegions_563230222(null, unlockedRegions);
		}
	}

	private void RpcWriter___Target_SetUnlockedRegions_563230222(NetworkConnection conn, List<EMapRegion> unlockedRegions)
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
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMap_002EEMapRegion_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, unlockedRegions);
			((NetworkBehaviour)this).SendTargetRpc(5u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetUnlockedRegions_563230222(PooledReader PooledReader0, Channel channel)
	{
		List<EMapRegion> unlockedRegions = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMap_002EEMapRegion_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetUnlockedRegions_563230222(((NetworkBehaviour)this).LocalConnection, unlockedRegions);
		}
	}

	private void RpcWriter___Observers_IncreaseTierNetworked_3953286437(FullRank before, FullRank after)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerated((Writer)(object)writer, before);
			GeneratedWriters___Internal.Write___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerated((Writer)(object)writer, after);
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___IncreaseTierNetworked_3953286437(FullRank before, FullRank after)
	{
		onRankUp?.Invoke(before, after);
		HasChanged = true;
		Console.Log("Ranked up to " + Rank.ToString() + ": " + Tier);
	}

	private void RpcReader___Observers_IncreaseTierNetworked_3953286437(PooledReader PooledReader0, Channel channel)
	{
		FullRank before = GeneratedReaders___Internal.Read___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		FullRank after = GeneratedReaders___Internal.Read___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___IncreaseTierNetworked_3953286437(before, after);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002ELevelling_002ELevelManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		rankCount = Enum.GetValues(typeof(ERank)).Length;
	}
}
