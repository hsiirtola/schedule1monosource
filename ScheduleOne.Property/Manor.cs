using System;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Property;

public class Manor : Property
{
	public enum EManorState
	{
		Original,
		Destroyed,
		Rebuilt
	}

	public const int REBUILD_AFTER_DAYS = 2;

	public const int REBUILD_DURATION_DAYS = 3;

	[Header("References")]
	public GameObject OriginalContainer;

	public GameObject DestroyedContainer;

	public GameObject RebuiltContainer;

	public GameObject DestructionFXContainer;

	public GameObject TunnelBlocker;

	public GameObject TunnelCollapse;

	public GameObject ConstructionContainer;

	public AudioSourceController[] ExplosionSounds;

	public GameObject[] DisableOnRebuild;

	public Action onRebuildComplete;

	private bool NetworkInitialize___EarlyScheduleOne_002EProperty_002EManorAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EProperty_002EManorAssembly_002DCSharp_002Edll_Excuted;

	public EManorState ManorState { get; private set; }

	public int DaysSinceStateChange { get; private set; } = 999;

	public bool TunnelDug { get; set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EProperty_002EManor_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (!connection.IsHost)
		{
			SetManorState(null, ManorState, resetStateChangeTimer: false);
			SetTunnelDug(null, TunnelDug);
		}
	}

	protected override void Start()
	{
		base.Start();
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onSleepEnd = (Action)Delegate.Combine(instance.onSleepEnd, new Action(OnSleepEnd));
	}

	protected override void RecieveOwned()
	{
		base.RecieveOwned();
		if (InstanceFinder.IsServer && ManorState != EManorState.Rebuilt)
		{
			SetManorState(null, EManorState.Rebuilt, resetStateChangeTimer: true);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void SetManorState(NetworkConnection conn, EManorState state, bool resetStateChangeTimer)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetManorState_365422978(conn, state, resetStateChangeTimer);
			RpcLogic___SetManorState_365422978(conn, state, resetStateChangeTimer);
		}
		else
		{
			RpcWriter___Target_SetManorState_365422978(conn, state, resetStateChangeTimer);
		}
	}

	[Button]
	public void Explode()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		SetManorState(null, EManorState.Destroyed, resetStateChangeTimer: true);
		DestructionFXContainer.gameObject.SetActive(true);
		if (InstanceFinder.IsServer)
		{
			ExplosionData data = new ExplosionData(30f, 400f, 1000f, checkLoS: false);
			NetworkSingleton<CombatManager>.Instance.CreateExplosion(((Component)this).transform.position, data);
			AudioSourceController[] explosionSounds = ExplosionSounds;
			for (int i = 0; i < explosionSounds.Length; i++)
			{
				explosionSounds[i].Play();
			}
		}
		PlayerSingleton<PlayerCamera>.Instance.StartCameraShake(3f, 2f);
	}

	[Button]
	public void Rebuild()
	{
		SetManorState(null, EManorState.Rebuilt, resetStateChangeTimer: true);
		if (onRebuildComplete != null)
		{
			onRebuildComplete();
		}
	}

	public void SetDestroyedIfOriginal()
	{
		if (ManorState == EManorState.Original)
		{
			SetManorState(null, EManorState.Destroyed, resetStateChangeTimer: true);
		}
	}

	public void DigTunnel()
	{
		SetTunnelDug(null, dug: true);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetTunnelDug(NetworkConnection conn, bool dug)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetTunnelDug_214505783(conn, dug);
			RpcLogic___SetTunnelDug_214505783(conn, dug);
		}
		else
		{
			RpcWriter___Target_SetTunnelDug_214505783(conn, dug);
		}
	}

	public override bool CanBePurchased()
	{
		if (ManorState != EManorState.Rebuilt)
		{
			return false;
		}
		return base.CanBePurchased();
	}

	private void OnSleepEnd()
	{
		DaysSinceStateChange++;
		DestructionFXContainer.gameObject.SetActive(false);
		if (ManorState == EManorState.Destroyed && DaysSinceStateChange >= 5)
		{
			Rebuild();
		}
		if (ManorState == EManorState.Destroyed && DaysSinceStateChange >= 2)
		{
			ConstructionContainer.SetActive(true);
		}
		else
		{
			ConstructionContainer.SetActive(false);
		}
	}

	public override bool ShouldSave()
	{
		return true;
	}

	public override string GetSaveString()
	{
		bool[] array = new bool[Switches.Count];
		for (int i = 0; i < Switches.Count; i++)
		{
			if (!((Object)(object)Switches[i] == (Object)null))
			{
				array[i] = Switches[i].isOn;
			}
		}
		bool[] array2 = new bool[Toggleables.Count];
		for (int j = 0; j < Toggleables.Count; j++)
		{
			if (!((Object)(object)Toggleables[j] == (Object)null))
			{
				array2[j] = Toggleables[j].IsActivated;
			}
		}
		return new ManorData(propertyCode, base.IsOwned, array, array2, GetEmployeeSaveDatas().ToArray(), GetObjectSaveDatas().ToArray(), ManorState, DaysSinceStateChange, TunnelDug).GetJson();
	}

	public override void Load(PropertyData propertyData, string dataString)
	{
		base.Load(propertyData, dataString);
		try
		{
			ManorData manorData = JsonUtility.FromJson<ManorData>(dataString);
			if (manorData != null)
			{
				SetManorState(null, manorData.ManorState, resetStateChangeTimer: false);
				DaysSinceStateChange = manorData.DaysSinceStateChange;
				SetTunnelDug(null, manorData.TunnelDug);
			}
		}
		catch (Exception ex)
		{
			Console.LogError("Error loading manor data: " + ex.Message);
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
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EProperty_002EManorAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EProperty_002EManorAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(5u, new ClientRpcDelegate(RpcReader___Observers_SetManorState_365422978));
			((NetworkBehaviour)this).RegisterTargetRpc(6u, new ClientRpcDelegate(RpcReader___Target_SetManorState_365422978));
			((NetworkBehaviour)this).RegisterObserversRpc(7u, new ClientRpcDelegate(RpcReader___Observers_SetTunnelDug_214505783));
			((NetworkBehaviour)this).RegisterTargetRpc(8u, new ClientRpcDelegate(RpcReader___Target_SetTunnelDug_214505783));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EProperty_002EManorAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EProperty_002EManorAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_SetManorState_365422978(NetworkConnection conn, EManorState state, bool resetStateChangeTimer)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EProperty_002EManor_002FEManorStateFishNet_002ESerializing_002EGenerated((Writer)(object)writer, state);
			((Writer)writer).WriteBoolean(resetStateChangeTimer);
			((NetworkBehaviour)this).SendObserversRpc(5u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetManorState_365422978(NetworkConnection conn, EManorState state, bool resetStateChangeTimer)
	{
		Console.Log($"Setting Manor state to {state}");
		ManorState = state;
		OriginalContainer.SetActive(state == EManorState.Original);
		DestroyedContainer.SetActive(state == EManorState.Destroyed);
		RebuiltContainer.SetActive(state == EManorState.Rebuilt);
		TunnelCollapse.SetActive(state == EManorState.Destroyed);
		((Component)ListingPoster).gameObject.SetActive(state == EManorState.Rebuilt && !base.IsOwned);
		if (state == EManorState.Rebuilt)
		{
			GameObject[] disableOnRebuild = DisableOnRebuild;
			for (int i = 0; i < disableOnRebuild.Length; i++)
			{
				disableOnRebuild[i].gameObject.SetActive(false);
			}
		}
		if (resetStateChangeTimer)
		{
			DaysSinceStateChange = 0;
		}
	}

	private void RpcReader___Observers_SetManorState_365422978(PooledReader PooledReader0, Channel channel)
	{
		EManorState state = GeneratedReaders___Internal.Read___ScheduleOne_002EProperty_002EManor_002FEManorStateFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool resetStateChangeTimer = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetManorState_365422978(null, state, resetStateChangeTimer);
		}
	}

	private void RpcWriter___Target_SetManorState_365422978(NetworkConnection conn, EManorState state, bool resetStateChangeTimer)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EProperty_002EManor_002FEManorStateFishNet_002ESerializing_002EGenerated((Writer)(object)writer, state);
			((Writer)writer).WriteBoolean(resetStateChangeTimer);
			((NetworkBehaviour)this).SendTargetRpc(6u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetManorState_365422978(PooledReader PooledReader0, Channel channel)
	{
		EManorState state = GeneratedReaders___Internal.Read___ScheduleOne_002EProperty_002EManor_002FEManorStateFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool resetStateChangeTimer = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetManorState_365422978(((NetworkBehaviour)this).LocalConnection, state, resetStateChangeTimer);
		}
	}

	private void RpcWriter___Observers_SetTunnelDug_214505783(NetworkConnection conn, bool dug)
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
			((Writer)writer).WriteBoolean(dug);
			((NetworkBehaviour)this).SendObserversRpc(7u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetTunnelDug_214505783(NetworkConnection conn, bool dug)
	{
		TunnelDug = dug;
		TunnelBlocker.SetActive(!dug);
	}

	private void RpcReader___Observers_SetTunnelDug_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool dug = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetTunnelDug_214505783(null, dug);
		}
	}

	private void RpcWriter___Target_SetTunnelDug_214505783(NetworkConnection conn, bool dug)
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
			((Writer)writer).WriteBoolean(dug);
			((NetworkBehaviour)this).SendTargetRpc(8u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetTunnelDug_214505783(PooledReader PooledReader0, Channel channel)
	{
		bool dug = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetTunnelDug_214505783(((NetworkBehaviour)this).LocalConnection, dug);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EProperty_002EManor_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		SetManorState(null, EManorState.Original, resetStateChangeTimer: false);
		((Component)ListingPoster).gameObject.SetActive(false);
		TunnelBlocker.SetActive(true);
		TunnelCollapse.SetActive(false);
	}
}
