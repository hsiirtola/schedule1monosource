using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.GamePhysics;

public class PhysicsManager : NetworkSingleton<PhysicsManager>
{
	public const bool AutoSyncTransforms = true;

	private bool NetworkInitialize___EarlyScheduleOne_002EGamePhysics_002EPhysicsManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EGamePhysics_002EPhysicsManagerAssembly_002DCSharp_002Edll_Excuted;

	public float GravityMultiplier { get; private set; } = 1f;

	[field: SerializeField]
	public LayerMask GroundDetectionLayerMask { get; private set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EGamePhysics_002EPhysicsManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost && GravityMultiplier != 1f)
		{
			SetGravityMultiplier(connection, GravityMultiplier);
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetGravityMultiplier(NetworkConnection conn, float gravity)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetGravityMultiplier_530160725(conn, gravity);
			RpcLogic___SetGravityMultiplier_530160725(conn, gravity);
		}
		else
		{
			RpcWriter___Target_SetGravityMultiplier_530160725(conn, gravity);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EGamePhysics_002EPhysicsManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EGamePhysics_002EPhysicsManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_SetGravityMultiplier_530160725));
			((NetworkBehaviour)this).RegisterTargetRpc(1u, new ClientRpcDelegate(RpcReader___Target_SetGravityMultiplier_530160725));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EGamePhysics_002EPhysicsManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EGamePhysics_002EPhysicsManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_SetGravityMultiplier_530160725(NetworkConnection conn, float gravity)
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
			((Writer)writer).WriteSingle(gravity, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetGravityMultiplier_530160725(NetworkConnection conn, float gravity)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Console.Log("Setting gravity multiplier to " + gravity);
		GravityMultiplier = gravity;
		Physics.gravity = new Vector3(0f, -9.81f * gravity, 0f);
	}

	private void RpcReader___Observers_SetGravityMultiplier_530160725(PooledReader PooledReader0, Channel channel)
	{
		float gravity = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetGravityMultiplier_530160725(null, gravity);
		}
	}

	private void RpcWriter___Target_SetGravityMultiplier_530160725(NetworkConnection conn, float gravity)
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
			((Writer)writer).WriteSingle(gravity, (AutoPackType)0);
			((NetworkBehaviour)this).SendTargetRpc(1u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetGravityMultiplier_530160725(PooledReader PooledReader0, Channel channel)
	{
		float gravity = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetGravityMultiplier_530160725(((NetworkBehaviour)this).LocalConnection, gravity);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EGamePhysics_002EPhysicsManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		Physics.autoSyncTransforms = true;
		Console.Log("PhysicsManager Awake: autoSyncTransforms set to " + true);
	}
}
