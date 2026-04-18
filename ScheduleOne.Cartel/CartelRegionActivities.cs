using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using GameKit.Utilities;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.Cartel;

public class CartelRegionActivities : NetworkBehaviour
{
	public const int MIN_COOLDOWN = 12;

	public const int MAX_COOLDOWN = 48;

	public bool TEST_MODE;

	[Header("Settings")]
	public bool Active = true;

	public EMapRegion Region;

	public List<CartelActivity> Activities = new List<CartelActivity>();

	[Header("References")]
	public CartelAmbushLocation[] AmbushLocations;

	public CartelDealer CartelDealer;

	[Header("Development & Debugging")]
	public int _debugActivityIndex;

	private bool NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelRegionActivitiesAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ECartel_002ECartelRegionActivitiesAssembly_002DCSharp_002Edll_Excuted;

	public CartelActivity CurrentActivity { get; private set; }

	public int HoursUntilNextActivity { get; set; } = 12;

	protected override void OnValidate()
	{
		((NetworkBehaviour)this).OnValidate();
		((Object)((Component)this).gameObject).name = $"Cartel Region Activities - {Region}";
	}

	private void Start()
	{
		HoursUntilNextActivity = GetNewCooldown(Region);
		if (TEST_MODE && Application.isEditor)
		{
			HoursUntilNextActivity = 2;
		}
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onHourPass = (Action)Delegate.Combine(instance.onHourPass, new Action(HourPass));
		Cartel instance2 = NetworkSingleton<Cartel>.Instance;
		instance2.OnStatusChange = (Action<ECartelStatus, ECartelStatus>)Delegate.Combine(instance2.OnStatusChange, new Action<ECartelStatus, ECartelStatus>(CartelStatusChange));
		if ((Object)(object)CartelDealer != (Object)null)
		{
			CartelDealer.Region = Region;
		}
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost && (Object)(object)CurrentActivity != (Object)null)
		{
			StartActivity(connection, Activities.IndexOf(CurrentActivity));
		}
	}

	public void HourPass()
	{
		if (InstanceFinder.IsServer && Active && NetworkSingleton<Cartel>.Instance.Status == ECartelStatus.Hostile && !(NetworkSingleton<Cartel>.Instance.Influence.GetInfluence(Region) <= 0f) && Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(Region).IsUnlocked)
		{
			HoursUntilNextActivity--;
			if (HoursUntilNextActivity <= 0)
			{
				TryStartActivity();
			}
		}
	}

	private void TryStartActivity()
	{
		Debug.Log((object)$"[CartelActivities] Attempting to start activity in region {Region}");
		HoursUntilNextActivity = GetNewCooldown(Region);
		List<CartelActivity> list = new List<CartelActivity>(Activities);
		Arrays.Shuffle<CartelActivity>(list);
		foreach (CartelActivity item in list)
		{
			if (item.IsRegionValidForActivity(Region))
			{
				Debug.Log((object)$"[CartelActivities] Starting activity {((object)item).GetType().Name} in region {Region}");
				StartActivity(null, Activities.IndexOf(item));
				return;
			}
		}
		Debug.Log((object)$"[CartelActivities] No valid activity could be started in region {Region} at this time.");
	}

	[Button]
	public void StartActivity()
	{
		StartAcivity(_debugActivityIndex);
	}

	private void StartAcivity(int activityIndex)
	{
		StartActivity(null, activityIndex);
	}

	[Button]
	public void ActivateDeal()
	{
		StartActivity(null, 2);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void StartActivity(NetworkConnection conn, int activityIndex)
	{
		if (conn == null)
		{
			RpcWriter___Observers_StartActivity_2681120339(conn, activityIndex);
			RpcLogic___StartActivity_2681120339(conn, activityIndex);
		}
		else
		{
			RpcWriter___Target_StartActivity_2681120339(conn, activityIndex);
		}
	}

	private void ActivityEnded()
	{
		if ((Object)(object)CurrentActivity != (Object)null)
		{
			CartelActivity currentActivity = CurrentActivity;
			currentActivity.onDeactivated = (Action)Delegate.Remove(currentActivity.onDeactivated, new Action(ActivityEnded));
			CurrentActivity = null;
		}
	}

	public CartelRegionalActivityData GetData()
	{
		int currentActivityIndex = -1;
		if ((Object)(object)CurrentActivity != (Object)null)
		{
			currentActivityIndex = Activities.IndexOf(CurrentActivity);
		}
		return new CartelRegionalActivityData(Region, currentActivityIndex, HoursUntilNextActivity);
	}

	public void Load(CartelRegionalActivityData data)
	{
		HoursUntilNextActivity = data.HoursUntilNextActivity;
		if (data.CurrentActivityIndex >= 0 && data.CurrentActivityIndex < Activities.Count)
		{
			StartActivity(null, data.CurrentActivityIndex);
		}
		else
		{
			CurrentActivity = null;
		}
	}

	public static int GetNewCooldown(EMapRegion region)
	{
		float influence = NetworkSingleton<Cartel>.Instance.Influence.GetInfluence(region);
		float num = Mathf.Clamp01((float)NetworkSingleton<ProductManager>.Instance.GetContractReceipts(region, new List<EContractParty>
		{
			EContractParty.Player,
			EContractParty.PlayerDealer
		}, 4320).Count / 10f);
		float num2 = influence * 0.5f + num * 0.5f;
		float num3 = Mathf.Lerp(48f, 24f, num2);
		int num4 = Mathf.RoundToInt(Random.Range(12f, num3));
		Console.Log($"New cooldown for {region}: {num4} hours (Influence: {influence}, Player Activity Factor: {num})");
		return num4;
	}

	private void CartelStatusChange(ECartelStatus oldStatus, ECartelStatus newStatus)
	{
		if (oldStatus != ECartelStatus.Hostile && newStatus == ECartelStatus.Hostile)
		{
			HoursUntilNextActivity = GetNewCooldown(Region);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelRegionActivitiesAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelRegionActivitiesAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_StartActivity_2681120339));
			((NetworkBehaviour)this).RegisterTargetRpc(1u, new ClientRpcDelegate(RpcReader___Target_StartActivity_2681120339));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ECartel_002ECartelRegionActivitiesAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ECartel_002ECartelRegionActivitiesAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_StartActivity_2681120339(NetworkConnection conn, int activityIndex)
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
			((Writer)writer).WriteInt32(activityIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___StartActivity_2681120339(NetworkConnection conn, int activityIndex)
	{
		if (activityIndex < 0 || activityIndex >= Activities.Count)
		{
			Console.LogError($"Invalid activity index {activityIndex} for region {Region}. Cannot start activity.");
			return;
		}
		CartelActivity cartelActivity = Activities[activityIndex];
		Console.Log($"Starting regional activity {((object)cartelActivity).GetType().Name} in {Region}");
		CurrentActivity = cartelActivity;
		CartelActivity currentActivity = CurrentActivity;
		currentActivity.onDeactivated = (Action)Delegate.Remove(currentActivity.onDeactivated, new Action(ActivityEnded));
		CartelActivity currentActivity2 = CurrentActivity;
		currentActivity2.onDeactivated = (Action)Delegate.Combine(currentActivity2.onDeactivated, new Action(ActivityEnded));
		CurrentActivity.Activate(Region);
	}

	private void RpcReader___Observers_StartActivity_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int activityIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartActivity_2681120339(null, activityIndex);
		}
	}

	private void RpcWriter___Target_StartActivity_2681120339(NetworkConnection conn, int activityIndex)
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
			((Writer)writer).WriteInt32(activityIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(1u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_StartActivity_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int activityIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___StartActivity_2681120339(((NetworkBehaviour)this).LocalConnection, activityIndex);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}
}
