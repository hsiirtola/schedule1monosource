using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class GenericDialogueBehaviour : Behaviour
{
	private Player targetPlayer;

	[Header("Settings")]
	public bool FaceConversationTarget = true;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EGenericDialogueBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EGenericDialogueBehaviourAssembly_002DCSharp_002Edll_Excuted;

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendTargetPlayer(NetworkObject player)
	{
		RpcWriter___Server_SendTargetPlayer_3323014238(player);
		RpcLogic___SendTargetPlayer_3323014238(player);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetTargetPlayer(NetworkObject player)
	{
		RpcWriter___Observers_SetTargetPlayer_3323014238(player);
		RpcLogic___SetTargetPlayer_3323014238(player);
	}

	public override void Enable()
	{
		base.Enable();
		base.beh.Update();
	}

	public override void Disable()
	{
		base.Disable();
		Deactivate();
	}

	public override void Activate()
	{
		base.Activate();
		if (base.Npc.Movement.IsMoving)
		{
			base.Npc.Movement.Stop();
		}
	}

	public override void Resume()
	{
		base.Resume();
		if (base.Npc.Movement.IsMoving)
		{
			base.Npc.Movement.Stop();
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		base.Npc.Movement.ResumeMovement();
	}

	public override void OnActiveTick()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (base.Npc.Movement.IsMoving)
		{
			base.Npc.Movement.Stop();
		}
		if (!FaceConversationTarget || base.Npc.Movement.FaceDirectionInProgress || !(base.Npc.Avatar.Animation.TimeSinceSitEnd >= 0.5f))
		{
			return;
		}
		float distance;
		Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, out distance);
		if (!((Object)(object)closestPlayer == (Object)null))
		{
			Vector3 val = ((Component)closestPlayer).transform.position - ((Component)base.Npc).transform.position;
			val.y = 0f;
			if (Vector3.Angle(((Component)base.Npc).transform.forward, val) > 10f)
			{
				base.Npc.Movement.FaceDirection(val);
			}
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EGenericDialogueBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EGenericDialogueBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SendTargetPlayer_3323014238));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetTargetPlayer_3323014238));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EGenericDialogueBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EGenericDialogueBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendTargetPlayer_3323014238(NetworkObject player)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendTargetPlayer_3323014238(NetworkObject player)
	{
		SetTargetPlayer(player);
	}

	private void RpcReader___Server_SendTargetPlayer_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendTargetPlayer_3323014238(player);
		}
	}

	private void RpcWriter___Observers_SetTargetPlayer_3323014238(NetworkObject player)
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

	private void RpcLogic___SetTargetPlayer_3323014238(NetworkObject player)
	{
		if (Singleton<DialogueCanvas>.Instance.isActive && (Object)(object)targetPlayer != (Object)null && ((NetworkBehaviour)targetPlayer).Owner.IsLocalClient && (Object)(object)player != (Object)null && !player.Owner.IsLocalClient)
		{
			Singleton<GameInput>.Instance.ExitAll();
		}
		if ((Object)(object)player != (Object)null)
		{
			targetPlayer = ((Component)player).GetComponent<Player>();
		}
		else
		{
			targetPlayer = null;
		}
	}

	private void RpcReader___Observers_SetTargetPlayer_3323014238(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetTargetPlayer_3323014238(player);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
