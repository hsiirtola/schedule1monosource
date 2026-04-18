using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Map;

public class SewerMushrooms : NetworkBehaviour
{
	[Serializable]
	public class SewerMushroomSpawnSettings
	{
		public int MaxSpawnAmount = 12;

		[Range(0f, 1f)]
		public float RespawnAmountPerdayAsPercentage = 0.2f;
	}

	[Header("Mushroom Spawning")]
	public ItemPickup MushroomObjectPrefab;

	public SewerMushroomSpawnSettings MushroomSpawnSettings;

	public List<Transform> MushroomAreas;

	public List<Transform> MushroomLocations;

	[Header("Development & Debugging")]
	[SerializeField]
	private bool _debugMode;

	[SyncObject]
	private readonly SyncList<int> _activeMushroomLocationIndices = new SyncList<int>();

	private Dictionary<int, ItemPickup> _spawnedMushroomItems = new Dictionary<int, ItemPickup>();

	private List<int> _availableMushroomSpawnLocationIndices = new List<int>();

	private List<int> _mushroomSpawnLocationAmountPerArea = new List<int> { 4, 5, 6 };

	private int _lastMushroomSpanwnLocationIndex = -1;

	private bool NetworkInitialize___EarlyScheduleOne_002EMap_002ESewerMushroomsAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EMap_002ESewerMushroomsAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EMap_002ESewerMushrooms_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		SetupEvents();
	}

	private void SetupEvents()
	{
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepStart = (Action)Delegate.Combine(instance.onSleepStart, new Action(RegenerateMushrooms));
	}

	private void MushroomIndicesChanged(SyncListOperation op, int index, int oldItem, int newItem, bool asServer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Invalid comparison between Unknown and I4
		if ((int)op != 0)
		{
			if ((int)op == 3)
			{
				Debug.Log((object)$"[Sewer] Mushroom index removed: {oldItem}");
				DespawnMushroom(oldItem);
			}
		}
		else
		{
			if (_debugMode)
			{
				Debug.Log((object)$"[Sewer] Mushroom index added: {newItem}");
			}
			SpawnMushroom(newItem);
		}
	}

	private void SpawnMushroom(int locationIndex)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		Transform val = MushroomLocations[locationIndex];
		if (_debugMode)
		{
			Debug.Log((object)("[Sewer] Mushroom spawned at location index: " + locationIndex));
		}
		if (_spawnedMushroomItems.ContainsKey(locationIndex))
		{
			if (_debugMode)
			{
				Debug.Log((object)"[Sewer] Existing mushroom found, using that one");
			}
			((Component)_spawnedMushroomItems[locationIndex]).gameObject.SetActive(true);
			return;
		}
		if (_debugMode)
		{
			Debug.Log((object)"[Sewer] No existing mushroom found, creating a new one");
		}
		ItemPickup itemPickup = Object.Instantiate<ItemPickup>(MushroomObjectPrefab, val.position, val.rotation, NetworkSingleton<GameManager>.Instance.Temp);
		((Component)val).GetComponent<SewerMushroomLocation>().SetMushroomsFromData(((Component)itemPickup).gameObject);
		itemPickup.onPickup.AddListener((UnityAction)delegate
		{
			((Component)_spawnedMushroomItems[locationIndex]).gameObject.SetActive(false);
			SetMushroomSpawnLocationAvailable(locationIndex);
			_activeMushroomLocationIndices.Remove(locationIndex);
		});
		_spawnedMushroomItems.Add(locationIndex, itemPickup);
	}

	private void DespawnMushroom(int locationIndex)
	{
		if (_debugMode)
		{
			Debug.Log((object)("[Sewer] Mushroom despawned at location index: " + locationIndex));
		}
		((Component)_spawnedMushroomItems[locationIndex]).gameObject.SetActive(false);
	}

	[ServerRpc]
	private void SetMushroomSpawnLocationAvailable(int locationIndex)
	{
		RpcWriter___Server_SetMushroomSpawnLocationAvailable_3316948804(locationIndex);
	}

	private void RegenerateMushrooms()
	{
		int num = Mathf.CeilToInt((float)MushroomSpawnSettings.MaxSpawnAmount * MushroomSpawnSettings.RespawnAmountPerdayAsPercentage);
		num = Mathf.Min(num, MushroomSpawnSettings.MaxSpawnAmount - _activeMushroomLocationIndices.Count);
		if (_debugMode)
		{
			Debug.Log((object)$"[Sewer] Mushrooms regenerating - spawn amount: {num}");
		}
		for (int i = 0; i < num; i++)
		{
			int nextSpawnLocation = GetNextSpawnLocation();
			if (nextSpawnLocation != -1)
			{
				_activeMushroomLocationIndices.Add(nextSpawnLocation);
				continue;
			}
			break;
		}
	}

	public void Load(SewerData sewerData)
	{
		if (_debugMode)
		{
			Debug.Log((object)"[Sewer] Loading in mushrooms");
		}
		foreach (Transform mushroomArea in MushroomAreas)
		{
			_mushroomSpawnLocationAmountPerArea.Add(mushroomArea.childCount);
		}
		sewerData.ActiveMushroomLocationIndices.ForEach(delegate(int x)
		{
			_availableMushroomSpawnLocationIndices.Remove(x);
		});
		_activeMushroomLocationIndices.AddRange((IEnumerable<int>)sewerData.ActiveMushroomLocationIndices);
		if (_activeMushroomLocationIndices.Count <= 0)
		{
			RegenerateMushrooms();
		}
	}

	public List<int> GetActiveMushroomLocationIndices()
	{
		return new List<int>((IEnumerable<int>)_activeMushroomLocationIndices);
	}

	private int GetNextSpawnLocation()
	{
		if (_availableMushroomSpawnLocationIndices.Count <= 0)
		{
			if (_debugMode)
			{
				Debug.Log((object)"[Sewer] No available mushroom spawn locations.");
			}
			return -1;
		}
		int index = 0;
		int num = _availableMushroomSpawnLocationIndices[index];
		if (_lastMushroomSpanwnLocationIndex != -1)
		{
			if (_debugMode)
			{
				Debug.Log((object)"[Sewer] There was a previous mushroom location. Trying to avoid same area spawn.");
			}
			for (int i = 0; i < 5; i++)
			{
				index = i % _availableMushroomSpawnLocationIndices.Count;
				num = _availableMushroomSpawnLocationIndices[index];
				if (!AreLocationsInSameArea(_lastMushroomSpanwnLocationIndex, num))
				{
					break;
				}
			}
		}
		if (_debugMode)
		{
			Debug.Log((object)("[Sewer] Removing location at array index " + index + "."));
		}
		_availableMushroomSpawnLocationIndices.RemoveAt(index);
		_lastMushroomSpanwnLocationIndex = num;
		return num;
	}

	private bool AreLocationsInSameArea(int locationIndexA, int locationIndexB)
	{
		int locationIndex = GetLocationIndex(locationIndexA);
		int locationIndex2 = GetLocationIndex(locationIndexB);
		return locationIndex == locationIndex2;
	}

	private bool CanSpawnMushroom()
	{
		if (_activeMushroomLocationIndices.Count < MushroomSpawnSettings.MaxSpawnAmount)
		{
			return _availableMushroomSpawnLocationIndices.Count > 0;
		}
		return false;
	}

	private int GetLocationIndex(int index)
	{
		int num = 0;
		for (int i = 0; i < _mushroomSpawnLocationAmountPerArea.Count; i++)
		{
			int num2 = num + _mushroomSpawnLocationAmountPerArea[i];
			if (index >= num && index < num2)
			{
				return i;
			}
			num += _mushroomSpawnLocationAmountPerArea[i];
		}
		return -1;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EMap_002ESewerMushroomsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EMap_002ESewerMushroomsAssembly_002DCSharp_002Edll_Excuted = true;
			((SyncBase)_activeMushroomLocationIndices).InitializeInstance((NetworkBehaviour)(object)this, 0u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, true);
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SetMushroomSpawnLocationAvailable_3316948804));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EMap_002ESewerMushroomsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EMap_002ESewerMushroomsAssembly_002DCSharp_002Edll_Excuted = true;
			((SyncBase)_activeMushroomLocationIndices).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetMushroomSpawnLocationAvailable_3316948804(int locationIndex)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteInt32(locationIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SetMushroomSpawnLocationAvailable_3316948804(int locationIndex)
	{
		if (_debugMode)
		{
			Debug.Log((object)("[Sewer] Mushroom spawn location index now available: " + locationIndex));
		}
		if (!_availableMushroomSpawnLocationIndices.Contains(locationIndex))
		{
			_availableMushroomSpawnLocationIndices.Add(locationIndex);
		}
	}

	private void RpcReader___Server_SetMushroomSpawnLocationAvailable_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int locationIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn))
		{
			RpcLogic___SetMushroomSpawnLocationAvailable_3316948804(locationIndex);
		}
	}

	private void Awake_UserLogic_ScheduleOne_002EMap_002ESewerMushrooms_Assembly_002DCSharp_002Edll()
	{
		_activeMushroomLocationIndices.OnChange += MushroomIndicesChanged;
		for (int i = 0; i < MushroomLocations.Count; i++)
		{
			_availableMushroomSpawnLocationIndices.Add(i);
		}
		_availableMushroomSpawnLocationIndices.Shuffle();
	}
}
