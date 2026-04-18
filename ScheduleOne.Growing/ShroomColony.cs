using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.Growing;

[RequireComponent(typeof(NetworkTransform))]
public class ShroomColony : NetworkBehaviour
{
	public const float MaxTemperatureForGrowth = 15f;

	private const float MinSoilMoistureForGrowth = 0.0001f;

	private const float RandomRotationRange = 15f;

	private const float RandomVerticalShift = 0.02f;

	[SerializeField]
	private ShroomSpawnDefinition _spawnDefinition;

	[SerializeField]
	private int _growTime = 18;

	[SerializeField]
	private Transform[] _shroomAlignments;

	[SerializeField]
	private GrowingMushroom[] _growingShroomPrefabs;

	[SerializeField]
	private AudioSourceController _snipSound;

	[SerializeField]
	private ParticleSystem _fullyGrownParticles;

	public Action onFullyHarvested;

	private List<GrowingMushroom> _growingShrooms = new List<GrowingMushroom>();

	private Dictionary<GrowingMushroom, int> _growingShroomPositions = new Dictionary<GrowingMushroom, int>();

	private List<int> _takenAlignmentIndices = new List<int>();

	private MushroomBed _parentBed;

	private bool _shroomsInitiallySpawned;

	private bool NetworkInitialize___EarlyScheduleOne_002EGrowing_002EShroomColonyAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EGrowing_002EShroomColonyAssembly_002DCSharp_002Edll_Excuted;

	[field: SerializeField]
	public int BaseShroomYield { get; private set; } = 12;

	public float GrowthProgress { get; private set; }

	public bool IsFullyGrown => GrowthProgress >= 1f;

	public bool IsTooHotToGrow
	{
		get
		{
			if ((Object)(object)_parentBed != (Object)null)
			{
				return _parentBed.GetAverageTileTemperature() > 15f;
			}
			return false;
		}
	}

	public int GrownMushroomCount => _growingShrooms.Count;

	public AudioSourceController SnipSound => _snipSound;

	public float NormalizedQuality { get; private set; } = 0.5f;

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!_shroomsInitiallySpawned && !Singleton<LoadManager>.Instance.IsLoading)
		{
			_shroomsInitiallySpawned = true;
			Debug.Log((object)"Spawning initial mushrooms for ShroomColony.");
			for (int i = 0; i < BaseShroomYield; i++)
			{
				int randomAvailableAlignmentIndex = GetRandomAvailableAlignmentIndex();
				if (randomAvailableAlignmentIndex != -1)
				{
					AddShroomAtPosition_Local(randomAvailableAlignmentIndex);
				}
			}
		}
		if (!connection.IsHost)
		{
			SetColonyState(connection, _takenAlignmentIndices.ToArray(), GrowthProgress, NormalizedQuality);
		}
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		_parentBed = ((Component)this).GetComponentInParent<MushroomBed>();
		if ((Object)(object)_parentBed == (Object)null)
		{
			Debug.LogError((object)"ShroomColony could not find its parent MushroomBed.");
			return;
		}
		_parentBed.AssignColony(this);
		MushroomBed parentBed = _parentBed;
		parentBed.onMinPass = (Action)Delegate.Combine(parentBed.onMinPass, new Action(OnMinPass));
		MushroomBed parentBed2 = _parentBed;
		parentBed2.onTimeSkip = (Action<int>)Delegate.Combine(parentBed2.onTimeSkip, new Action<int>(OnTimeSkipped));
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)_parentBed))
		{
			MushroomBed parentBed = _parentBed;
			parentBed.onMinPass = (Action)Delegate.Remove(parentBed.onMinPass, new Action(OnMinPass));
			MushroomBed parentBed2 = _parentBed;
			parentBed2.onTimeSkip = (Action<int>)Delegate.Remove(parentBed2.onTimeSkip, new Action<int>(OnTimeSkipped));
		}
	}

	private void OnMinPass()
	{
		if (!NetworkSingleton<TimeManager>.Instance.IsEndOfDay)
		{
			ChangeGrowthPercentage(GetCurrentGrowthRate() / ((float)_growTime * 60f));
		}
	}

	private void OnTimeSkipped(int mins)
	{
		ChangeGrowthPercentage(GetCurrentGrowthRate() / ((float)_growTime * 60f) * (float)mins);
		if (InstanceFinder.IsServer)
		{
			SetGrowthPercentage_Local(null, GrowthProgress);
		}
	}

	public void SetColonyVisible(bool visible)
	{
		((Component)this).gameObject.SetActive(visible);
	}

	private float GetCurrentGrowthRate()
	{
		if (IsTooHotToGrow)
		{
			return 0f;
		}
		if (_parentBed.NormalizedMoistureAmount <= 0.0001f)
		{
			return 0f;
		}
		return 1f;
	}

	private void ChangeGrowthPercentage(float change)
	{
		SetGrowthPercentage(GrowthProgress + change);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetFullyGrown()
	{
		RpcWriter___Server_SetFullyGrown_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetGrowthPercentage_Local(NetworkConnection conn, float percent)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetGrowthPercentage_Local_530160725(conn, percent);
			RpcLogic___SetGrowthPercentage_Local_530160725(conn, percent);
		}
		else
		{
			RpcWriter___Target_SetGrowthPercentage_Local_530160725(conn, percent);
		}
	}

	private void SetGrowthPercentage(float percent)
	{
		if (Mathf.Approximately(percent, GrowthProgress))
		{
			return;
		}
		GrowthProgress = Mathf.Clamp01(percent);
		foreach (GrowingMushroom growingShroom in _growingShrooms)
		{
			growingShroom.SetGrowthPercent(GrowthProgress);
		}
		if (IsFullyGrown)
		{
			if (!_fullyGrownParticles.isPlaying)
			{
				_fullyGrownParticles.Play();
			}
		}
		else if (_fullyGrownParticles.isPlaying)
		{
			_fullyGrownParticles.Stop();
		}
	}

	private void ChangeQuality(float change)
	{
		NormalizedQuality = Mathf.Clamp01(NormalizedQuality + change);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void AddShroomAtPosition_Server(int alignmentIndex)
	{
		RpcWriter___Server_AddShroomAtPosition_Server_3316948804(alignmentIndex);
		RpcLogic___AddShroomAtPosition_Server_3316948804(alignmentIndex);
	}

	[ObserversRpc(RunLocally = true)]
	private void AddShroomAtPosition_Local(int alignmentIndex)
	{
		RpcWriter___Observers_AddShroomAtPosition_Local_3316948804(alignmentIndex);
		RpcLogic___AddShroomAtPosition_Local_3316948804(alignmentIndex);
	}

	private void AddShroomAtPosition(int alignmentIndex)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (!_takenAlignmentIndices.Contains(alignmentIndex))
		{
			if (alignmentIndex < 0 || alignmentIndex >= _shroomAlignments.Length)
			{
				Debug.LogWarning((object)("Invalid alignment index for adding mushroom: " + alignmentIndex));
				return;
			}
			GrowingMushroom growingMushroom = Object.Instantiate<GrowingMushroom>(_growingShroomPrefabs[Random.Range(0, _growingShroomPrefabs.Length)], _shroomAlignments[alignmentIndex].position - Vector3.up * Random.Range(0f, 0.02f), _shroomAlignments[alignmentIndex].rotation, ((Component)this).transform);
			_growingShrooms.Add(growingMushroom);
			_growingShroomPositions[growingMushroom] = alignmentIndex;
			_takenAlignmentIndices.Add(alignmentIndex);
			growingMushroom.VerticalScaleMultiplier = Random.Range(0.8f, 1.2f);
			growingMushroom.LateralScaleMultiplier = Random.Range(0.8f, 1.2f);
			growingMushroom.MaxCapExpansion = Random.Range(0.8f, 1.1f);
			growingMushroom.Initialize(this, alignmentIndex);
			((Component)growingMushroom).transform.Rotate(Random.Range(-15f, 15f), (float)Random.Range(0, 360), Random.Range(-15f, 15f));
			growingMushroom.SetGrowthPercent(GrowthProgress);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void RemoveShroom_Server(int alignmentIndex)
	{
		RpcWriter___Server_RemoveShroom_Server_3316948804(alignmentIndex);
		RpcLogic___RemoveShroom_Server_3316948804(alignmentIndex);
	}

	public void RemoveRandomShroom()
	{
		int index = Random.Range(0, _growingShrooms.Count);
		RemoveShroom_Server(_growingShroomPositions[_growingShrooms[index]]);
	}

	[ObserversRpc(RunLocally = true)]
	private void RemoveShoom_Client(int alignmentIndex)
	{
		RpcWriter___Observers_RemoveShoom_Client_3316948804(alignmentIndex);
		RpcLogic___RemoveShoom_Client_3316948804(alignmentIndex);
	}

	private void RemoveShroom(int alignmentIndex)
	{
		GrowingMushroom growingMushroom = null;
		foreach (KeyValuePair<GrowingMushroom, int> growingShroomPosition in _growingShroomPositions)
		{
			if (growingShroomPosition.Value == alignmentIndex)
			{
				growingMushroom = growingShroomPosition.Key;
				break;
			}
		}
		if ((Object)(object)growingMushroom != (Object)null)
		{
			RemoveShroom(growingMushroom);
		}
	}

	private void RemoveShroom(GrowingMushroom shroom)
	{
		if (_growingShrooms.Contains(shroom))
		{
			int item = _growingShroomPositions[shroom];
			_takenAlignmentIndices.Remove(item);
			_growingShroomPositions.Remove(shroom);
			_growingShrooms.Remove(shroom);
			Object.Destroy((Object)(object)((Component)shroom).gameObject);
		}
		else
		{
			Debug.LogWarning((object)"Attempted to remove a mushroom that is not part of this colony.");
		}
		if (_growingShrooms.Count == 0)
		{
			SetFullyHarvested();
		}
	}

	public void SetFullyHarvested()
	{
		onFullyHarvested?.Invoke();
	}

	private int GetRandomAvailableAlignmentIndex()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < _shroomAlignments.Length; i++)
		{
			if (!_takenAlignmentIndices.Contains(i))
			{
				list.Add(i);
			}
		}
		if (list.Count == 0)
		{
			Debug.LogWarning((object)"No available alignment indices.");
			return -1;
		}
		return list[Random.Range(0, list.Count)];
	}

	public ShroomInstance GetHarvestedShroom(int quantity = 1)
	{
		ShroomInstance obj = _spawnDefinition.Shroom.GetDefaultInstance(quantity) as ShroomInstance;
		obj.SetQuality(ItemQuality.GetQuality(NormalizedQuality));
		return obj;
	}

	public void AdditiveApplied(AdditiveDefinition additive, bool isInitialApplication)
	{
		if (additive.QualityChange != 0f)
		{
			ChangeQuality(additive.QualityChange);
		}
		if (!isInitialApplication)
		{
			return;
		}
		if (additive.YieldMultiplier > 1f && InstanceFinder.IsServer)
		{
			int num = Mathf.RoundToInt((float)BaseShroomYield * (additive.YieldMultiplier - 1f));
			for (int i = 0; i < num; i++)
			{
				AddShroomAtPosition_Local(GetRandomAvailableAlignmentIndex());
			}
		}
		if (additive.InstantGrowth > 0f)
		{
			ChangeGrowthPercentage(additive.InstantGrowth);
		}
	}

	[TargetRpc]
	public void SetColonyState(NetworkConnection conn, int[] _activeMushroomIndices, float growthProgress, float quality)
	{
		RpcWriter___Target_SetColonyState_4288818029(conn, _activeMushroomIndices, growthProgress, quality);
	}

	public ShroomColonyData GetSaveData()
	{
		return new ShroomColonyData(((BaseItemDefinition)_spawnDefinition).ID, GrowthProgress, NormalizedQuality, new List<int>(_takenAlignmentIndices).ToArray());
	}

	public void Load(ShroomColonyData data)
	{
		NormalizedQuality = data.Quality;
		int[] activeMushroomAlignmentIndices = data.ActiveMushroomAlignmentIndices;
		foreach (int alignmentIndex in activeMushroomAlignmentIndices)
		{
			AddShroomAtPosition(alignmentIndex);
		}
		_shroomsInitiallySpawned = true;
		SetGrowthPercentage(data.GrowthProgress);
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
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EGrowing_002EShroomColonyAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EGrowing_002EShroomColonyAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SetFullyGrown_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetGrowthPercentage_Local_530160725));
			((NetworkBehaviour)this).RegisterTargetRpc(2u, new ClientRpcDelegate(RpcReader___Target_SetGrowthPercentage_Local_530160725));
			((NetworkBehaviour)this).RegisterServerRpc(3u, new ServerRpcDelegate(RpcReader___Server_AddShroomAtPosition_Server_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(4u, new ClientRpcDelegate(RpcReader___Observers_AddShroomAtPosition_Local_3316948804));
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_RemoveShroom_Server_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_RemoveShoom_Client_3316948804));
			((NetworkBehaviour)this).RegisterTargetRpc(7u, new ClientRpcDelegate(RpcReader___Target_SetColonyState_4288818029));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EGrowing_002EShroomColonyAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EGrowing_002EShroomColonyAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetFullyGrown_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
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
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetFullyGrown_2166136261()
	{
		SetGrowthPercentage_Local(null, 1f);
	}

	private void RpcReader___Server_SetFullyGrown_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SetFullyGrown_2166136261();
		}
	}

	private void RpcWriter___Observers_SetGrowthPercentage_Local_530160725(NetworkConnection conn, float percent)
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
			((Writer)writer).WriteSingle(percent, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetGrowthPercentage_Local_530160725(NetworkConnection conn, float percent)
	{
		SetGrowthPercentage(percent);
	}

	private void RpcReader___Observers_SetGrowthPercentage_Local_530160725(PooledReader PooledReader0, Channel channel)
	{
		float percent = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetGrowthPercentage_Local_530160725(null, percent);
		}
	}

	private void RpcWriter___Target_SetGrowthPercentage_Local_530160725(NetworkConnection conn, float percent)
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
			((Writer)writer).WriteSingle(percent, (AutoPackType)0);
			((NetworkBehaviour)this).SendTargetRpc(2u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetGrowthPercentage_Local_530160725(PooledReader PooledReader0, Channel channel)
	{
		float percent = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetGrowthPercentage_Local_530160725(((NetworkBehaviour)this).LocalConnection, percent);
		}
	}

	private void RpcWriter___Server_AddShroomAtPosition_Server_3316948804(int alignmentIndex)
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
			((Writer)writer).WriteInt32(alignmentIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(3u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___AddShroomAtPosition_Server_3316948804(int alignmentIndex)
	{
		AddShroomAtPosition_Local(alignmentIndex);
	}

	private void RpcReader___Server_AddShroomAtPosition_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int alignmentIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___AddShroomAtPosition_Server_3316948804(alignmentIndex);
		}
	}

	private void RpcWriter___Observers_AddShroomAtPosition_Local_3316948804(int alignmentIndex)
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
			((Writer)writer).WriteInt32(alignmentIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(4u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___AddShroomAtPosition_Local_3316948804(int alignmentIndex)
	{
		AddShroomAtPosition(alignmentIndex);
	}

	private void RpcReader___Observers_AddShroomAtPosition_Local_3316948804(PooledReader PooledReader0, Channel channel)
	{
		int alignmentIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___AddShroomAtPosition_Local_3316948804(alignmentIndex);
		}
	}

	private void RpcWriter___Server_RemoveShroom_Server_3316948804(int alignmentIndex)
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
			((Writer)writer).WriteInt32(alignmentIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___RemoveShroom_Server_3316948804(int alignmentIndex)
	{
		RemoveShoom_Client(alignmentIndex);
	}

	private void RpcReader___Server_RemoveShroom_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int alignmentIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___RemoveShroom_Server_3316948804(alignmentIndex);
		}
	}

	private void RpcWriter___Observers_RemoveShoom_Client_3316948804(int alignmentIndex)
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
			((Writer)writer).WriteInt32(alignmentIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___RemoveShoom_Client_3316948804(int alignmentIndex)
	{
		RemoveShroom(alignmentIndex);
	}

	private void RpcReader___Observers_RemoveShoom_Client_3316948804(PooledReader PooledReader0, Channel channel)
	{
		int alignmentIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___RemoveShoom_Client_3316948804(alignmentIndex);
		}
	}

	private void RpcWriter___Target_SetColonyState_4288818029(NetworkConnection conn, int[] _activeMushroomIndices, float growthProgress, float quality)
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
			GeneratedWriters___Internal.Write___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerated((Writer)(object)writer, _activeMushroomIndices);
			((Writer)writer).WriteSingle(growthProgress, (AutoPackType)0);
			((Writer)writer).WriteSingle(quality, (AutoPackType)0);
			((NetworkBehaviour)this).SendTargetRpc(7u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	public void RpcLogic___SetColonyState_4288818029(NetworkConnection conn, int[] _activeMushroomIndices, float growthProgress, float quality)
	{
		NormalizedQuality = quality;
		foreach (int alignmentIndex in _activeMushroomIndices)
		{
			AddShroomAtPosition(alignmentIndex);
		}
		SetGrowthPercentage(growthProgress);
	}

	private void RpcReader___Target_SetColonyState_4288818029(PooledReader PooledReader0, Channel channel)
	{
		int[] activeMushroomIndices = GeneratedReaders___Internal.Read___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		float growthProgress = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		float quality = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetColonyState_4288818029(((NetworkBehaviour)this).LocalConnection, activeMushroomIndices, growthProgress, quality);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}
}
