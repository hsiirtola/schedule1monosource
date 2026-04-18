using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.NPCs.Behaviour;

public class FleeBehaviour : Behaviour
{
	public enum EFleeMode
	{
		Entity,
		Point
	}

	public const float FLEE_DIST_MIN = 20f;

	public const float FLEE_DIST_MAX = 40f;

	public const float FLEE_SPEED = 0.7f;

	private Vector3 currentFleeTarget = Vector3.zero;

	private float nextVO;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFleeBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFleeBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public NetworkObject EntityToFlee { get; private set; }

	public Vector3 PointToFlee
	{
		get
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (FleeMode != EFleeMode.Point)
			{
				return ((Component)EntityToFlee).transform.position;
			}
			return FleeOrigin;
		}
	}

	public EFleeMode FleeMode { get; private set; }

	public Vector3 FleeOrigin { get; private set; } = Vector3.zero;

	[ObserversRpc(RunLocally = true)]
	public void SetEntityToFlee(NetworkObject entity)
	{
		RpcWriter___Observers_SetEntityToFlee_3323014238(entity);
		RpcLogic___SetEntityToFlee_3323014238(entity);
	}

	[ObserversRpc(RunLocally = true)]
	public void SetPointToFlee(Vector3 point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_SetPointToFlee_4276783012(point);
		RpcLogic___SetPointToFlee_4276783012(point);
	}

	public override void Activate()
	{
		base.Activate();
		StartFlee();
		EVOLineType lineType = ((Random.Range(0, 2) == 0) ? EVOLineType.Scared : EVOLineType.Concerned);
		base.Npc.PlayVO(lineType);
	}

	public override void Resume()
	{
		base.Resume();
		StartFlee();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		Stop();
		base.Npc.Avatar.EmotionManager.RemoveEmotionOverride("fleeing");
	}

	public override void Pause()
	{
		base.Pause();
		Stop();
	}

	private void StartFlee()
	{
		Flee();
		base.Npc.Avatar.EmotionManager.AddEmotionOverride("Scared", "fleeing");
		base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("fleeing", 2, 0.7f));
		nextVO = Time.time + Random.Range(5f, 15f);
	}

	public override void OnActiveTick()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (FleeMode == EFleeMode.Entity && (Object)(object)EntityToFlee == (Object)null)
		{
			Deactivate();
			return;
		}
		if (!base.Npc.Movement.IsMoving && Vector3.Distance(((Component)this).transform.position, currentFleeTarget) < 3f)
		{
			Deactivate_Networked(null);
			Disable_Networked(null);
			return;
		}
		Vector3 val = PointToFlee - ((Component)this).transform.position;
		val.y = 0f;
		if (Vector3.Angle(val, base.Npc.Movement.Agent.desiredVelocity) < 30f)
		{
			Flee();
		}
	}

	public override void BehaviourUpdate()
	{
		base.BehaviourUpdate();
		if (Time.time > nextVO)
		{
			EVOLineType lineType = ((Random.Range(0, 2) == 0) ? EVOLineType.Scared : EVOLineType.Concerned);
			base.Npc.PlayVO(lineType);
			nextVO = Time.time + Random.Range(5f, 15f);
		}
	}

	private void Stop()
	{
		base.Npc.Movement.Stop();
		base.Npc.Movement.SpeedController.RemoveSpeedControl("fleeing");
	}

	private void Flee()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vector3 destination = (currentFleeTarget = GetFleePosition());
		base.Npc.Movement.SetDestination(destination);
	}

	public Vector3 GetFleePosition()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		float num2 = 0f;
		RaycastHit val2 = default(RaycastHit);
		while (true)
		{
			if (FleeMode == EFleeMode.Entity && (Object)(object)EntityToFlee == (Object)null)
			{
				return Vector3.zero;
			}
			Vector3 val = ((Component)this).transform.position - PointToFlee;
			val.y = 0f;
			val = Quaternion.AngleAxis(num2, Vector3.up) * val;
			float num3 = Random.Range(20f, 40f);
			if (Physics.Raycast(((Component)this).transform.position + ((Vector3)(ref val)).normalized * num3 + Vector3.up * 10f, Vector3.down, ref val2, 20f, LayerMask.GetMask(new string[1] { "Default" })) && NavMeshUtility.SamplePosition(((RaycastHit)(ref val2)).point, out var hit, 2f, -1))
			{
				return ((NavMeshHit)(ref hit)).position;
			}
			if (num > 10)
			{
				break;
			}
			num2 += 15f;
			num++;
		}
		Console.LogWarning("Failed to find a valid flee position, returning current position");
		return ((Component)this).transform.position;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFleeBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFleeBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_SetEntityToFlee_3323014238));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_SetPointToFlee_4276783012));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFleeBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFleeBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_SetEntityToFlee_3323014238(NetworkObject entity)
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
			((Writer)writer).WriteNetworkObject(entity);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetEntityToFlee_3323014238(NetworkObject entity)
	{
		EntityToFlee = entity;
		FleeMode = EFleeMode.Entity;
	}

	private void RpcReader___Observers_SetEntityToFlee_3323014238(PooledReader PooledReader0, Channel channel)
	{
		NetworkObject entity = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetEntityToFlee_3323014238(entity);
		}
	}

	private void RpcWriter___Observers_SetPointToFlee_4276783012(Vector3 point)
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
			((Writer)writer).WriteVector3(point);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetPointToFlee_4276783012(Vector3 point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		FleeOrigin = point;
		FleeMode = EFleeMode.Point;
	}

	private void RpcReader___Observers_SetPointToFlee_4276783012(PooledReader PooledReader0, Channel channel)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Vector3 point = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetPointToFlee_4276783012(point);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
