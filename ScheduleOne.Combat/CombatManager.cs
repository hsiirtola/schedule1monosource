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
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Combat;

public class CombatManager : NetworkSingleton<CombatManager>
{
	public LayerMask MeleeLayerMask;

	public LayerMask ExplosionLayerMask;

	public LayerMask RangedWeaponLayerMask;

	public Explosion ExplosionPrefab;

	private List<int> explosionIDs = new List<int>();

	private bool NetworkInitialize___EarlyScheduleOne_002ECombat_002ECombatManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ECombat_002ECombatManagerAssembly_002DCSharp_002Edll_Excuted;

	[Button]
	public void CreateTestExplosion()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Vector3 origin = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position + ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward * 10f;
		if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(10f, out var hit, ExplosionLayerMask))
		{
			origin = ((RaycastHit)(ref hit)).point;
		}
		CreateExplosion(origin, ExplosionData.DefaultSmall);
	}

	public void CreateExplosion(Vector3 origin, ExplosionData data)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		int id = Random.Range(0, int.MaxValue);
		CreateExplosion(origin, data, id);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void CreateExplosion(Vector3 origin, ExplosionData data, int id)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_CreateExplosion_2907189355(origin, data, id);
		RpcLogic___CreateExplosion_2907189355(origin, data, id);
	}

	[ObserversRpc(RunLocally = true)]
	private void Explosion(Vector3 origin, ExplosionData data, int id)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_Explosion_2907189355(origin, data, id);
		RpcLogic___Explosion_2907189355(origin, data, id);
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ECombat_002ECombatManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ECombat_002ECombatManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_CreateExplosion_2907189355));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_Explosion_2907189355));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ECombat_002ECombatManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ECombat_002ECombatManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_CreateExplosion_2907189355(Vector3 origin, ExplosionData data, int id)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(origin);
			GeneratedWriters___Internal.Write___ScheduleOne_002ECombat_002EExplosionDataFishNet_002ESerializing_002EGenerated((Writer)(object)writer, data);
			((Writer)writer).WriteInt32(id, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___CreateExplosion_2907189355(Vector3 origin, ExplosionData data, int id)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Explosion(origin, data, id);
	}

	private void RpcReader___Server_CreateExplosion_2907189355(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector3 origin = ((Reader)PooledReader0).ReadVector3();
		ExplosionData data = GeneratedReaders___Internal.Read___ScheduleOne_002ECombat_002EExplosionDataFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		int id = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___CreateExplosion_2907189355(origin, data, id);
		}
	}

	private void RpcWriter___Observers_Explosion_2907189355(Vector3 origin, ExplosionData data, int id)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(origin);
			GeneratedWriters___Internal.Write___ScheduleOne_002ECombat_002EExplosionDataFishNet_002ESerializing_002EGenerated((Writer)(object)writer, data);
			((Writer)writer).WriteInt32(id, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Explosion_2907189355(Vector3 origin, ExplosionData data, int id)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!explosionIDs.Contains(id))
		{
			explosionIDs.Add(id);
			Explosion explosion = Object.Instantiate<Explosion>(ExplosionPrefab);
			explosion.Initialize(origin, data);
			Object.Destroy((Object)(object)((Component)explosion).gameObject, 3f);
		}
	}

	private void RpcReader___Observers_Explosion_2907189355(PooledReader PooledReader0, Channel channel)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Vector3 origin = ((Reader)PooledReader0).ReadVector3();
		ExplosionData data = GeneratedReaders___Internal.Read___ScheduleOne_002ECombat_002EExplosionDataFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		int id = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Explosion_2907189355(origin, data, id);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
