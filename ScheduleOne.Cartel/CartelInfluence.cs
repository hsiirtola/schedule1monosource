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
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Cartel;

public class CartelInfluence : NetworkBehaviour
{
	[Serializable]
	public class RegionInfluenceData
	{
		private string name;

		public EMapRegion Region;

		[Range(0f, 1f)]
		public float Influence;

		public RegionInfluenceData(EMapRegion region, float influence = 0f)
		{
			name = region.ToString();
			Region = region;
			Influence = influence;
		}
	}

	public const float INFLUENCE_TO_UNLOCK_NEXT_REGION = 0.3f;

	public const float WESTVILLE_MAX_INFLUENCE = 0.5f;

	[Header("Settings")]
	public RegionInfluenceData[] DefaultRegionInfluence;

	private List<RegionInfluenceData> regionInfluence = new List<RegionInfluenceData>();

	public Action<EMapRegion, float, float> OnInfluenceChanged;

	private bool NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelInfluenceAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ECartel_002ECartelInfluenceAssembly_002DCSharp_002Edll_Excuted;

	public RegionInfluenceData[] GetAllRegionInfluence()
	{
		return regionInfluence.ToArray();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ECartel_002ECartelInfluence_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (connection.IsHost)
		{
			return;
		}
		foreach (RegionInfluenceData item in regionInfluence)
		{
			SetInfluence(connection, item.Region, item.Influence);
		}
	}

	protected override void OnValidate()
	{
		((NetworkBehaviour)this).OnValidate();
		if (DefaultRegionInfluence != null && DefaultRegionInfluence.Length != 0)
		{
			return;
		}
		DefaultRegionInfluence = new RegionInfluenceData[Enum.GetValues(typeof(EMapRegion)).Length];
		foreach (EMapRegion value in Enum.GetValues(typeof(EMapRegion)))
		{
			if (DefaultRegionInfluence[(int)value] == null)
			{
				DefaultRegionInfluence[(int)value] = new RegionInfluenceData(value);
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void ChangeInfluence(EMapRegion region, float amount)
	{
		RpcWriter___Server_ChangeInfluence_2792544924(region, amount);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetInfluence(NetworkConnection conn, EMapRegion region, float influence)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetInfluence_2071772313(conn, region, influence);
			RpcLogic___SetInfluence_2071772313(conn, region, influence);
		}
		else
		{
			RpcWriter___Target_SetInfluence_2071772313(conn, region, influence);
		}
	}

	public float GetInfluence(EMapRegion region)
	{
		return GetRegionData(region)?.Influence ?? 0f;
	}

	[ObserversRpc(RunLocally = true)]
	private void ChangeInfluence(EMapRegion region, float oldInfluence, float newInfluence)
	{
		RpcWriter___Observers_ChangeInfluence_1267088319(region, oldInfluence, newInfluence);
		RpcLogic___ChangeInfluence_1267088319(region, oldInfluence, newInfluence);
	}

	private RegionInfluenceData GetRegionData(EMapRegion region)
	{
		return regionInfluence.Find((RegionInfluenceData data) => data.Region == region);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelInfluenceAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelInfluenceAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_ChangeInfluence_2792544924));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetInfluence_2071772313));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_SetInfluence_2071772313));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_ChangeInfluence_1267088319));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ECartel_002ECartelInfluenceAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ECartel_002ECartelInfluenceAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_ChangeInfluence_2792544924(EMapRegion region, float amount)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EMap_002EEMapRegionFishNet_002ESerializing_002EGenerated((Writer)(object)writer, region);
			((Writer)writer).WriteSingle(amount, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___ChangeInfluence_2792544924(EMapRegion region, float amount)
	{
		RegionInfluenceData regionData = GetRegionData(region);
		ChangeInfluence(region, regionData.Influence, Mathf.Clamp01(regionData.Influence + amount));
	}

	private void RpcReader___Server_ChangeInfluence_2792544924(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		EMapRegion region = GeneratedReaders___Internal.Read___ScheduleOne_002EMap_002EEMapRegionFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		float amount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___ChangeInfluence_2792544924(region, amount);
		}
	}

	private void RpcWriter___Observers_SetInfluence_2071772313(NetworkConnection conn, EMapRegion region, float influence)
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
			((Writer)writer).WriteSingle(influence, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetInfluence_2071772313(NetworkConnection conn, EMapRegion region, float influence)
	{
		if (region == EMapRegion.Westville)
		{
			influence = Mathf.Min(influence, 0.5f);
		}
		RegionInfluenceData regionData = GetRegionData(region);
		if (regionData != null)
		{
			regionData.Influence = Mathf.Clamp(influence, 0f, 1f);
		}
		else
		{
			Debug.LogWarning((object)$"Region {region} not found in influence data.");
		}
	}

	private void RpcReader___Observers_SetInfluence_2071772313(PooledReader PooledReader0, Channel channel)
	{
		EMapRegion region = GeneratedReaders___Internal.Read___ScheduleOne_002EMap_002EEMapRegionFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		float influence = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetInfluence_2071772313(null, region, influence);
		}
	}

	private void RpcWriter___Target_SetInfluence_2071772313(NetworkConnection conn, EMapRegion region, float influence)
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
			((Writer)writer).WriteSingle(influence, (AutoPackType)0);
			((NetworkBehaviour)this).SendTargetRpc(2u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetInfluence_2071772313(PooledReader PooledReader0, Channel channel)
	{
		EMapRegion region = GeneratedReaders___Internal.Read___ScheduleOne_002EMap_002EEMapRegionFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		float influence = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetInfluence_2071772313(((NetworkBehaviour)this).LocalConnection, region, influence);
		}
	}

	private void RpcWriter___Observers_ChangeInfluence_1267088319(EMapRegion region, float oldInfluence, float newInfluence)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteSingle(oldInfluence, (AutoPackType)0);
			((Writer)writer).WriteSingle(newInfluence, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ChangeInfluence_1267088319(EMapRegion region, float oldInfluence, float newInfluence)
	{
		if (region == EMapRegion.Westville)
		{
			newInfluence = Mathf.Min(newInfluence, 0.5f);
		}
		RegionInfluenceData regionData = GetRegionData(region);
		if (regionData != null)
		{
			if (newInfluence != regionData.Influence)
			{
				regionData.Influence = Mathf.Clamp(newInfluence, 0f, 1f);
				if (regionData.Influence <= 0.3f && oldInfluence > 0.3f && region != EMapRegion.Uptown)
				{
					MapRegionData regionData2 = Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(region + 1);
					regionData2.SetUnlocked();
					Singleton<RegionUnlockedCanvas>.Instance.QueueUnlocked(regionData2.Region);
				}
				if (oldInfluence != regionData.Influence)
				{
					OnInfluenceChanged?.Invoke(region, oldInfluence, regionData.Influence);
				}
			}
		}
		else
		{
			Debug.LogWarning((object)$"Region {region} not found in influence data.");
		}
	}

	private void RpcReader___Observers_ChangeInfluence_1267088319(PooledReader PooledReader0, Channel channel)
	{
		EMapRegion region = GeneratedReaders___Internal.Read___ScheduleOne_002EMap_002EEMapRegionFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		float oldInfluence = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		float newInfluence = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ChangeInfluence_1267088319(region, oldInfluence, newInfluence);
		}
	}

	private void Awake_UserLogic_ScheduleOne_002ECartel_002ECartelInfluence_Assembly_002DCSharp_002Edll()
	{
		RegionInfluenceData[] defaultRegionInfluence = DefaultRegionInfluence;
		foreach (RegionInfluenceData regionInfluenceData in defaultRegionInfluence)
		{
			if (regionInfluenceData != null)
			{
				regionInfluence.Add(regionInfluenceData);
			}
			else
			{
				Debug.LogWarning((object)"DefaultRegionInfluence contains a null entry. Please check the configuration.");
			}
		}
	}
}
