using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class FaceTargetBehaviour : Behaviour
{
	public enum ETargetType
	{
		Player,
		Position
	}

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFaceTargetBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFaceTargetBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public ETargetType TargetType { get; private set; }

	public Player TargetPlayer { get; private set; }

	public Vector3 TargetPosition { get; private set; } = Vector3.zero;

	public float Countdown { get; private set; }

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetTarget(NetworkObject player, float countDown = 5f)
	{
		RpcWriter___Server_SetTarget_244313061(player, countDown);
		RpcLogic___SetTarget_244313061(player, countDown);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetTargetLocal(NetworkObject player)
	{
		RpcWriter___Observers_SetTargetLocal_3323014238(player);
		RpcLogic___SetTargetLocal_3323014238(player);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetTarget(Vector3 position, float countDown = 5f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_SetTarget_3661469815(position, countDown);
		RpcLogic___SetTarget_3661469815(position, countDown);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetTargetLocal(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_SetTargetLocal_4276783012(position);
		RpcLogic___SetTargetLocal_4276783012(position);
	}

	public override void Activate()
	{
		base.Activate();
		base.Npc.Movement.Stop();
	}

	public override void BehaviourUpdate()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.BehaviourUpdate();
		if (!base.Active)
		{
			return;
		}
		if ((Object)(object)TargetPlayer != (Object)null)
		{
			base.Npc.Avatar.LookController.OverrideLookTarget(GetTargetPosition(), 1, rotateBody: true);
		}
		if (InstanceFinder.IsServer)
		{
			Countdown -= Time.deltaTime;
			if (Countdown <= 0f)
			{
				Disable_Networked(null);
			}
		}
	}

	private Vector3 GetTargetPosition()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (TargetType == ETargetType.Player && (Object)(object)TargetPlayer != (Object)null)
		{
			return TargetPlayer.EyePosition;
		}
		return TargetPosition;
	}

	public override void Disable()
	{
		base.Disable();
		Deactivate();
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
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFaceTargetBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFaceTargetBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SetTarget_244313061));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetTargetLocal_3323014238));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_SetTarget_3661469815));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_SetTargetLocal_4276783012));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFaceTargetBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFaceTargetBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetTarget_244313061(NetworkObject player, float countDown = 5f)
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
			((Writer)writer).WriteNetworkObject(player);
			((Writer)writer).WriteSingle(countDown, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetTarget_244313061(NetworkObject player, float countDown = 5f)
	{
		Countdown = countDown;
		TargetPlayer = (((Object)(object)player != (Object)null) ? ((Component)player).GetComponent<Player>() : null);
		SetTargetLocal(player);
	}

	private void RpcReader___Server_SetTarget_244313061(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		float countDown = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetTarget_244313061(player, countDown);
		}
	}

	private void RpcWriter___Observers_SetTargetLocal_3323014238(NetworkObject player)
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
			((Writer)writer).WriteNetworkObject(player);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetTargetLocal_3323014238(NetworkObject player)
	{
		TargetType = ETargetType.Player;
		TargetPlayer = (((Object)(object)player != (Object)null) ? ((Component)player).GetComponent<Player>() : null);
	}

	private void RpcReader___Observers_SetTargetLocal_3323014238(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetTargetLocal_3323014238(player);
		}
	}

	private void RpcWriter___Server_SetTarget_3661469815(Vector3 position, float countDown = 5f)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(position);
			((Writer)writer).WriteSingle(countDown, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetTarget_3661469815(Vector3 position, float countDown = 5f)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Countdown = countDown;
		SetTargetLocal(position);
	}

	private void RpcReader___Server_SetTarget_3661469815(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		float countDown = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetTarget_3661469815(position, countDown);
		}
	}

	private void RpcWriter___Observers_SetTargetLocal_4276783012(Vector3 position)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(position);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetTargetLocal_4276783012(Vector3 position)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		TargetType = ETargetType.Position;
		TargetPosition = position;
	}

	private void RpcReader___Observers_SetTargetLocal_4276783012(PooledReader PooledReader0, Channel channel)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetTargetLocal_4276783012(position);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
