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
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;

namespace ScheduleOne.Cartel;

public class Cartel : NetworkSingleton<Cartel>, IBaseSaveable, ISaveable
{
	[Header("References")]
	public CartelActivities Activities;

	public CartelInfluence Influence;

	public GoonPool GoonPool;

	public CartelDealManager DealManager;

	public Action<ECartelStatus, ECartelStatus> OnStatusChange;

	private CartelLoader loader = new CartelLoader();

	private bool NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ECartel_002ECartelAssembly_002DCSharp_002Edll_Excuted;

	public ECartelStatus Status { get; private set; }

	public int HoursSinceStatusChange { get; private set; } = 9999;

	public string SaveFolderName => "Cartel";

	public string SaveFileName => "Cartel";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public int LoadOrder { get; } = 10;

	protected override void Start()
	{
		base.Start();
		InitializeSaveable();
		TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
		timeManager.onHourPass = (Action)Delegate.Combine(timeManager.onHourPass, new Action(HourPass));
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost)
		{
			SetStatus(connection, Status, resetStatusChangeTimer: false);
		}
	}

	private void HourPass()
	{
		HoursSinceStatusChange++;
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public virtual string GetSaveString()
	{
		CartelRegionalActivityData[] array = new CartelRegionalActivityData[Activities.RegionalActivities.Length];
		for (int i = 0; i < array.Length; i++)
		{
			try
			{
				array[i] = Activities.RegionalActivities[i].GetData();
			}
			catch (Exception ex)
			{
				EMapRegion eMapRegion = (EMapRegion)i;
				Console.LogError("Error getting regional activity data for region " + eMapRegion.ToString() + ": " + ex);
			}
		}
		return new CartelData(Status, HoursSinceStatusChange, Influence.GetAllRegionInfluence(), Activities.HoursUntilNextGlobalActivity, array, DealManager.ActiveDeal, DealManager.HoursUntilNextDealRequest).GetJson();
	}

	public void Load(CartelData data)
	{
		if (data == null)
		{
			Console.LogWarning("Attempted to load null cartel data.");
			return;
		}
		Status = data.Status;
		HoursSinceStatusChange = data.HoursSinceStatusChange;
		if (SaveManager.GetVersionNumber(Singleton<LoadManager>.Instance.ActiveSaveInfo.SaveVersion) < 42f && data.Status == ECartelStatus.Hostile)
		{
			Debug.Log((object)"Correcting westville influence for old save version...");
			for (int i = 0; i < data.RegionInfluence.Length; i++)
			{
				if (data.RegionInfluence[i].Region == EMapRegion.Westville)
				{
					if (data.RegionInfluence[i].Influence > 0.3f)
					{
						float num = Mathf.InverseLerp(1f, 0.3f, data.RegionInfluence[i].Influence);
						Debug.Log((object)("Normalized westville progress: " + num));
						float influence = Mathf.Lerp(0.5f, 0.3f, num);
						Debug.Log((object)("Correcting westville influence from " + data.RegionInfluence[i].Influence + " to " + influence));
						data.RegionInfluence[i].Influence = influence;
					}
					break;
				}
			}
		}
		CartelInfluence.RegionInfluenceData[] regionInfluence = data.RegionInfluence;
		foreach (CartelInfluence.RegionInfluenceData regionInfluenceData in regionInfluence)
		{
			if (regionInfluenceData != null)
			{
				Influence.SetInfluence(null, regionInfluenceData.Region, regionInfluenceData.Influence);
			}
		}
		Activities.HoursUntilNextGlobalActivity = data.HoursUntilNextGlobalActivity;
		if (data.RegionalActivityData != null)
		{
			CartelRegionalActivityData[] regionalActivityData = data.RegionalActivityData;
			foreach (CartelRegionalActivityData cartelRegionalActivityData in regionalActivityData)
			{
				if (cartelRegionalActivityData != null)
				{
					Activities.RegionalActivities[(int)cartelRegionalActivityData.Region].Load(cartelRegionalActivityData);
				}
			}
		}
		DealManager.Load(data);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetStatus_Server(ECartelStatus status, bool resetStatusChangedTimer)
	{
		RpcWriter___Server_SetStatus_Server_2366206100(status, resetStatusChangedTimer);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetStatus(NetworkConnection conn, ECartelStatus newStatus, bool resetStatusChangeTimer)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetStatus_3666943613(conn, newStatus, resetStatusChangeTimer);
			RpcLogic___SetStatus_3666943613(conn, newStatus, resetStatusChangeTimer);
		}
		else
		{
			RpcWriter___Target_SetStatus_3666943613(conn, newStatus, resetStatusChangeTimer);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SetStatus_Server_2366206100));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetStatus_3666943613));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_SetStatus_3666943613));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ECartel_002ECartelAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ECartel_002ECartelAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetStatus_Server_2366206100(ECartelStatus status, bool resetStatusChangedTimer)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ECartelStatusFishNet_002ESerializing_002EGenerated((Writer)(object)writer, status);
			((Writer)writer).WriteBoolean(resetStatusChangedTimer);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetStatus_Server_2366206100(ECartelStatus status, bool resetStatusChangedTimer)
	{
		SetStatus(null, status, resetStatusChangedTimer);
	}

	private void RpcReader___Server_SetStatus_Server_2366206100(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		ECartelStatus status = GeneratedReaders___Internal.Read___ECartelStatusFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool resetStatusChangedTimer = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SetStatus_Server_2366206100(status, resetStatusChangedTimer);
		}
	}

	private void RpcWriter___Observers_SetStatus_3666943613(NetworkConnection conn, ECartelStatus newStatus, bool resetStatusChangeTimer)
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
			GeneratedWriters___Internal.Write___ECartelStatusFishNet_002ESerializing_002EGenerated((Writer)(object)writer, newStatus);
			((Writer)writer).WriteBoolean(resetStatusChangeTimer);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetStatus_3666943613(NetworkConnection conn, ECartelStatus newStatus, bool resetStatusChangeTimer)
	{
		if (Status != newStatus)
		{
			ECartelStatus status = Status;
			Status = newStatus;
			Console.Log("New cartel status: " + Status);
			if (resetStatusChangeTimer)
			{
				HoursSinceStatusChange = 0;
			}
			if (OnStatusChange != null)
			{
				OnStatusChange(status, newStatus);
			}
		}
	}

	private void RpcReader___Observers_SetStatus_3666943613(PooledReader PooledReader0, Channel channel)
	{
		ECartelStatus newStatus = GeneratedReaders___Internal.Read___ECartelStatusFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool resetStatusChangeTimer = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetStatus_3666943613(null, newStatus, resetStatusChangeTimer);
		}
	}

	private void RpcWriter___Target_SetStatus_3666943613(NetworkConnection conn, ECartelStatus newStatus, bool resetStatusChangeTimer)
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
			GeneratedWriters___Internal.Write___ECartelStatusFishNet_002ESerializing_002EGenerated((Writer)(object)writer, newStatus);
			((Writer)writer).WriteBoolean(resetStatusChangeTimer);
			((NetworkBehaviour)this).SendTargetRpc(2u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetStatus_3666943613(PooledReader PooledReader0, Channel channel)
	{
		ECartelStatus newStatus = GeneratedReaders___Internal.Read___ECartelStatusFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool resetStatusChangeTimer = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetStatus_3666943613(((NetworkBehaviour)this).LocalConnection, newStatus, resetStatusChangeTimer);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
