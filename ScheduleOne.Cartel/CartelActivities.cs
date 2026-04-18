using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using GameKit.Utilities;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Cartel;

public class CartelActivities : NetworkBehaviour
{
	public const int MAX_COOLDOWN_HOURS = 24;

	public const int MIN_COOLDOWN_HOURS = 6;

	[Header("References")]
	public List<CartelActivity> GlobalActivities = new List<CartelActivity>();

	public CartelRegionActivities[] RegionalActivities;

	private bool NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelActivitiesAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ECartel_002ECartelActivitiesAssembly_002DCSharp_002Edll_Excuted;

	public CartelActivity CurrentGlobalActivity { get; private set; }

	public int HoursUntilNextGlobalActivity { get; set; } = 12;

	private void Start()
	{
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onHourPass = (Action)Delegate.Combine(instance.onHourPass, new Action(HourPass));
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost && (Object)(object)CurrentGlobalActivity != (Object)null)
		{
			StartGlobalActivity(connection, CurrentGlobalActivity.Region, GlobalActivities.IndexOf(CurrentGlobalActivity));
		}
	}

	public CartelRegionActivities GetRegionalActivities(EMapRegion region)
	{
		CartelRegionActivities[] regionalActivities = RegionalActivities;
		foreach (CartelRegionActivities cartelRegionActivities in regionalActivities)
		{
			if (cartelRegionActivities.Region == region)
			{
				return cartelRegionActivities;
			}
		}
		Console.LogError($"No regional activities found for region: {region}");
		return null;
	}

	private void HourPass()
	{
		if (InstanceFinder.IsServer && NetworkSingleton<Cartel>.Instance.Status == ECartelStatus.Hostile && (Object)(object)CurrentGlobalActivity == (Object)null)
		{
			if (HoursUntilNextGlobalActivity > 0)
			{
				HoursUntilNextGlobalActivity--;
			}
			if (HoursUntilNextGlobalActivity <= 0)
			{
				Debug.Log((object)"[CartelActivities] trying to start new activity");
				TryStartActivity();
			}
		}
	}

	private void TryStartActivity()
	{
		HoursUntilNextGlobalActivity = GetNewCooldown();
		if (!CanNewActivityBegin())
		{
			Debug.Log((object)"[CartelActivities] Cannot start new global activity at this time. Rerolling cooldown.");
			return;
		}
		List<CartelActivity> activitiesReadyToStart = GetActivitiesReadyToStart();
		List<EMapRegion> validRegionsForActivity = GetValidRegionsForActivity();
		if (activitiesReadyToStart.Count == 0 || validRegionsForActivity.Count == 0)
		{
			Debug.Log((object)"[CartelActivities] No available activities or regions to start a global activity.");
			return;
		}
		validRegionsForActivity.Sort((EMapRegion a, EMapRegion b) => NetworkSingleton<Cartel>.Instance.Influence.GetInfluence(b).CompareTo(NetworkSingleton<Cartel>.Instance.Influence.GetInfluence(a)));
		EMapRegion region = EMapRegion.Northtown;
		bool flag = false;
		foreach (EMapRegion item in validRegionsForActivity)
		{
			float influence = NetworkSingleton<Cartel>.Instance.Influence.GetInfluence(item);
			if (Random.Range(0f, 1f) < influence * 0.8f)
			{
				region = item;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Debug.Log((object)"[CartelActivities] No region selected for global activity after influence check.");
			return;
		}
		Arrays.Shuffle<CartelActivity>(activitiesReadyToStart);
		for (int num = 0; num < activitiesReadyToStart.Count; num++)
		{
			if (activitiesReadyToStart[num].IsRegionValidForActivity(region))
			{
				StartGlobalActivity(null, region, num);
				break;
			}
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void StartGlobalActivity(NetworkConnection conn, EMapRegion region, int activityIndex)
	{
		if (conn == null)
		{
			RpcWriter___Observers_StartGlobalActivity_1796582335(conn, region, activityIndex);
			RpcLogic___StartGlobalActivity_1796582335(conn, region, activityIndex);
		}
		else
		{
			RpcWriter___Target_StartGlobalActivity_1796582335(conn, region, activityIndex);
		}
	}

	private void ActivityEnded()
	{
		CartelActivity currentGlobalActivity = CurrentGlobalActivity;
		currentGlobalActivity.onDeactivated = (Action)Delegate.Remove(currentGlobalActivity.onDeactivated, new Action(ActivityEnded));
		CurrentGlobalActivity = null;
	}

	private bool CanNewActivityBegin()
	{
		return true;
	}

	private List<CartelActivity> GetActivitiesReadyToStart()
	{
		List<CartelActivity> list = new List<CartelActivity>(GlobalActivities);
		for (int i = 0; i < GlobalActivities.Count; i++)
		{
			list.Add(GlobalActivities[i]);
		}
		return list;
	}

	private List<EMapRegion> GetValidRegionsForActivity()
	{
		List<EMapRegion> list = new List<EMapRegion>();
		EMapRegion[] array = (EMapRegion[])Enum.GetValues(typeof(EMapRegion));
		foreach (EMapRegion eMapRegion in array)
		{
			if (NetworkSingleton<Cartel>.Instance.Influence.GetInfluence(eMapRegion) <= 0.001f)
			{
				continue;
			}
			EMapRegion[] source = Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(eMapRegion).GetAdjacentRegions().ToArray();
			bool flag = false;
			for (int j = 0; j < Player.PlayerList.Count; j++)
			{
				EMapRegion currentRegion = Player.PlayerList[j].CurrentRegion;
				if (currentRegion == eMapRegion || source.Contains(currentRegion))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				list.Add(eMapRegion);
			}
		}
		return list;
	}

	public static int GetNewCooldown()
	{
		int num = 6;
		float num2 = Mathf.Clamp01((float)NetworkSingleton<Cartel>.Instance.HoursSinceStatusChange / 24f / 10f);
		float num3 = Mathf.Lerp(24f, (float)num, Mathf.Max(num2, 0.3f));
		float influenceFraction = GetInfluenceFraction();
		float num4 = Mathf.Lerp(24f, num3, influenceFraction * 0.5f);
		int num5 = Mathf.RoundToInt(Random.Range(num3, num4));
		Console.Log($"New global activity cooldown: {num5} hours. Possible range: {num3} - {num4} (Influence Fraction: {influenceFraction}, Time Since Hostile Began Fraction: {num2})");
		return num5;
	}

	private static float GetInfluenceFraction()
	{
		List<EMapRegion> list = new List<EMapRegion>();
		for (int i = 0; i < Singleton<ScheduleOne.Map.Map>.Instance.Regions.Length; i++)
		{
			if (Singleton<ScheduleOne.Map.Map>.Instance.Regions[i].IsUnlocked)
			{
				list.Add(Singleton<ScheduleOne.Map.Map>.Instance.Regions[i].Region);
			}
		}
		float num = 0f;
		for (int j = 0; j < list.Count; j++)
		{
			num += NetworkSingleton<Cartel>.Instance.Influence.GetInfluence(list[j]);
		}
		return num / (float)list.Count;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelActivitiesAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelActivitiesAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_StartGlobalActivity_1796582335));
			((NetworkBehaviour)this).RegisterTargetRpc(1u, new ClientRpcDelegate(RpcReader___Target_StartGlobalActivity_1796582335));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ECartel_002ECartelActivitiesAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ECartel_002ECartelActivitiesAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_StartGlobalActivity_1796582335(NetworkConnection conn, EMapRegion region, int activityIndex)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EMap_002EEMapRegionFishNet_002ESerializing_002EGenerated((Writer)(object)writer, region);
			((Writer)writer).WriteInt32(activityIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___StartGlobalActivity_1796582335(NetworkConnection conn, EMapRegion region, int activityIndex)
	{
		if (!((Object)(object)CurrentGlobalActivity != (Object)null))
		{
			CurrentGlobalActivity = GlobalActivities[activityIndex];
			CartelActivity currentGlobalActivity = CurrentGlobalActivity;
			currentGlobalActivity.onDeactivated = (Action)Delegate.Combine(currentGlobalActivity.onDeactivated, new Action(ActivityEnded));
			CurrentGlobalActivity.Activate(region);
			HoursUntilNextGlobalActivity = GetNewCooldown();
		}
	}

	private void RpcReader___Observers_StartGlobalActivity_1796582335(PooledReader PooledReader0, Channel channel)
	{
		EMapRegion region = GeneratedReaders___Internal.Read___ScheduleOne_002EMap_002EEMapRegionFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		int activityIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartGlobalActivity_1796582335(null, region, activityIndex);
		}
	}

	private void RpcWriter___Target_StartGlobalActivity_1796582335(NetworkConnection conn, EMapRegion region, int activityIndex)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EMap_002EEMapRegionFishNet_002ESerializing_002EGenerated((Writer)(object)writer, region);
			((Writer)writer).WriteInt32(activityIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(1u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_StartGlobalActivity_1796582335(PooledReader PooledReader0, Channel channel)
	{
		EMapRegion region = GeneratedReaders___Internal.Read___ScheduleOne_002EMap_002EEMapRegionFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		int activityIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___StartGlobalActivity_1796582335(((NetworkBehaviour)this).LocalConnection, region, activityIndex);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}
}
