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
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Cartel;

public class CartelGoon : NPC
{
	private List<CartelGoon> goonMates = new List<CartelGoon>();

	private CartelGoonAppearance appearance;

	public Action onDespawn;

	private bool NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelGoonAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ECartel_002ECartelGoonAssembly_002DCSharp_002Edll_Excuted;

	public bool IsGoonSpawned { get; private set; }

	public GoonPool GoonPool => NetworkSingleton<Cartel>.Instance.GoonPool;

	protected override void Start()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.Start();
		Health.onRevive.AddListener(new UnityAction(Despawn));
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		Despawn_Client(null);
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (IsGoonSpawned && !connection.IsHost && appearance != null)
		{
			ConfigureGoonSettings(connection, appearance, Movement.MoveSpeedMultiplier);
			Spawn_Client(connection);
		}
		else
		{
			Despawn_Client(connection);
		}
	}

	public void Spawn(GoonPool pool, Vector3 spawnPoint)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (InstanceFinder.IsServer)
		{
			if (IsGoonSpawned)
			{
				Debug.LogWarning((object)"CartelGoon.Spawn called on an already spawned goon. This goon is already active in the world.");
				return;
			}
			if ((Object)(object)pool == (Object)null)
			{
				Debug.LogWarning((object)"CartelGoon.Spawn called with null pool. Cannot spawn without a valid GoonPool.");
				return;
			}
			Inventory.Clear();
			Inventory.AddRandomItemsToInventory();
			appearance = pool.GetRandomAppearance();
			ConfigureGoonSettings(null, appearance, Random.Range(0.9f, 1.3f));
			Movement.Warp(spawnPoint);
			Movement.Agent.avoidancePriority = Random.Range(0, 50);
			Spawn_Client(null);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void Spawn_Client(NetworkConnection conn)
	{
		if (conn == null)
		{
			RpcWriter___Observers_Spawn_Client_328543758(conn);
			RpcLogic___Spawn_Client_328543758(conn);
		}
		else
		{
			RpcWriter___Target_Spawn_Client_328543758(conn);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void ConfigureGoonSettings(NetworkConnection conn, CartelGoonAppearance appearance, float moveSpeed)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ConfigureGoonSettings_3427656873(conn, appearance, moveSpeed);
			RpcLogic___ConfigureGoonSettings_3427656873(conn, appearance, moveSpeed);
		}
		else
		{
			RpcWriter___Target_ConfigureGoonSettings_3427656873(conn, appearance, moveSpeed);
		}
	}

	public void Despawn()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (!IsGoonSpawned)
		{
			Debug.LogWarning((object)"CartelGoon.Despawn called on a goon that is not spawned. This goon is not active in the world.");
			return;
		}
		if ((Object)(object)GoonPool != (Object)null)
		{
			GoonPool.ReturnToPool(this);
		}
		Despawn_Client(null);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void Despawn_Client(NetworkConnection conn)
	{
		if (conn == null)
		{
			RpcWriter___Observers_Despawn_Client_328543758(conn);
			RpcLogic___Despawn_Client_328543758(conn);
		}
		else
		{
			RpcWriter___Target_Despawn_Client_328543758(conn);
		}
	}

	public void AttackEntity(ICombatTargetable target, bool includeGoonMates = true)
	{
		if (!InstanceFinder.IsServer)
		{
			Debug.LogWarning((object)"CartelGoon.AttackEntity called on client. This should only be called on the server.");
			return;
		}
		Behaviour.CombatBehaviour.SetTargetAndEnable_Server(target.NetworkObject);
		if (!includeGoonMates)
		{
			return;
		}
		foreach (CartelGoon goonMate in goonMates)
		{
			goonMate.AttackEntity(target, includeGoonMates: false);
		}
	}

	public void AddGoonMate(CartelGoon goonMate)
	{
		if (!((Object)(object)goonMate == (Object)null) && !goonMates.Contains(goonMate))
		{
			goonMates.Add(goonMate);
			if (!goonMate.IsMatesWith(this))
			{
				goonMate.AddGoonMate(this);
			}
		}
	}

	public void RemoveGoonMate(CartelGoon goonMate)
	{
		if (!((Object)(object)goonMate == (Object)null) && goonMates.Contains(goonMate))
		{
			goonMates.Remove(goonMate);
			if (goonMate.IsMatesWith(this))
			{
				goonMate.RemoveGoonMate(this);
			}
		}
	}

	public bool IsMatesWith(CartelGoon otherGoon)
	{
		return goonMates.Contains(otherGoon);
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
		if (!NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelGoonAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelGoonAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(39u, new ClientRpcDelegate(RpcReader___Observers_Spawn_Client_328543758));
			((NetworkBehaviour)this).RegisterTargetRpc(40u, new ClientRpcDelegate(RpcReader___Target_Spawn_Client_328543758));
			((NetworkBehaviour)this).RegisterObserversRpc(41u, new ClientRpcDelegate(RpcReader___Observers_ConfigureGoonSettings_3427656873));
			((NetworkBehaviour)this).RegisterTargetRpc(42u, new ClientRpcDelegate(RpcReader___Target_ConfigureGoonSettings_3427656873));
			((NetworkBehaviour)this).RegisterObserversRpc(43u, new ClientRpcDelegate(RpcReader___Observers_Despawn_Client_328543758));
			((NetworkBehaviour)this).RegisterTargetRpc(44u, new ClientRpcDelegate(RpcReader___Target_Despawn_Client_328543758));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ECartel_002ECartelGoonAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ECartel_002ECartelGoonAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_Spawn_Client_328543758(NetworkConnection conn)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
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
			((NetworkBehaviour)this).SendObserversRpc(39u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Spawn_Client_328543758(NetworkConnection conn)
	{
		IsGoonSpawned = true;
		SetVisible(visible: true);
		Behaviour.GetBehaviour("Follow Schedule").Enable();
	}

	private void RpcReader___Observers_Spawn_Client_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Spawn_Client_328543758(null);
		}
	}

	private void RpcWriter___Target_Spawn_Client_328543758(NetworkConnection conn)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
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
			((NetworkBehaviour)this).SendTargetRpc(40u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_Spawn_Client_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Spawn_Client_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	private void RpcWriter___Observers_ConfigureGoonSettings_3427656873(NetworkConnection conn, CartelGoonAppearance appearance, float moveSpeed)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ECartel_002ECartelGoonAppearanceFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((Writer)writer).WriteSingle(moveSpeed, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(41u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ConfigureGoonSettings_3427656873(NetworkConnection conn, CartelGoonAppearance appearance, float moveSpeed)
	{
		if (appearance == null)
		{
			Console.LogError("CartelGoon.ConfigureGoonSettings called with null appearance. Cannot configure goon without a valid appearance.");
			return;
		}
		Avatar.LoadAvatarSettings(appearance.IsMale ? GoonPool.MaleClothing[appearance.ClothingIndex] : GoonPool.FemaleClothing[appearance.ClothingIndex]);
		Avatar.LoadNakedSettings(appearance.IsMale ? GoonPool.MaleBaseAppearances[appearance.BaseAppearanceIndex] : GoonPool.FemaleBaseAppearances[appearance.BaseAppearanceIndex], 100);
		VoiceOverEmitter.SetDatabase(appearance.IsMale ? GoonPool.MaleVoices[appearance.VoiceIndex] : GoonPool.FemaleVoices[appearance.VoiceIndex]);
		Movement.MoveSpeedMultiplier = moveSpeed;
	}

	private void RpcReader___Observers_ConfigureGoonSettings_3427656873(PooledReader PooledReader0, Channel channel)
	{
		CartelGoonAppearance cartelGoonAppearance = GeneratedReaders___Internal.Read___ScheduleOne_002ECartel_002ECartelGoonAppearanceFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		float moveSpeed = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ConfigureGoonSettings_3427656873(null, cartelGoonAppearance, moveSpeed);
		}
	}

	private void RpcWriter___Target_ConfigureGoonSettings_3427656873(NetworkConnection conn, CartelGoonAppearance appearance, float moveSpeed)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ECartel_002ECartelGoonAppearanceFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((Writer)writer).WriteSingle(moveSpeed, (AutoPackType)0);
			((NetworkBehaviour)this).SendTargetRpc(42u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ConfigureGoonSettings_3427656873(PooledReader PooledReader0, Channel channel)
	{
		CartelGoonAppearance cartelGoonAppearance = GeneratedReaders___Internal.Read___ScheduleOne_002ECartel_002ECartelGoonAppearanceFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		float moveSpeed = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ConfigureGoonSettings_3427656873(((NetworkBehaviour)this).LocalConnection, cartelGoonAppearance, moveSpeed);
		}
	}

	private void RpcWriter___Observers_Despawn_Client_328543758(NetworkConnection conn)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
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
			((NetworkBehaviour)this).SendObserversRpc(43u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Despawn_Client_328543758(NetworkConnection conn)
	{
		IsGoonSpawned = false;
		SetVisible(visible: false);
		Behaviour.GetBehaviour("Follow Schedule").Disable();
	}

	private void RpcReader___Observers_Despawn_Client_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Despawn_Client_328543758(null);
		}
	}

	private void RpcWriter___Target_Despawn_Client_328543758(NetworkConnection conn)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
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
			((NetworkBehaviour)this).SendTargetRpc(44u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_Despawn_Client_328543758(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___Despawn_Client_328543758(((NetworkBehaviour)this).LocalConnection);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
