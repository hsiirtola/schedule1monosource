using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Schedules;

public class NPCEvent_Conversate : NPCEvent
{
	private EVOLineType[] ConversationLines = new EVOLineType[8]
	{
		EVOLineType.Greeting,
		EVOLineType.Question,
		EVOLineType.Surprised,
		EVOLineType.Alerted,
		EVOLineType.Annoyed,
		EVOLineType.Acknowledge,
		EVOLineType.Think,
		EVOLineType.No
	};

	private string[] AnimationTriggers = new string[4] { "ThumbsUp", "DisagreeWave", "Nod", "ConversationGesture1" };

	public const float DESTINATION_THRESHOLD = 1f;

	public const float TIME_BEFORE_WAIT_START = 3f;

	public ConversationLocation Location;

	private bool IsConversating;

	private Coroutine conversateRoutine;

	private bool IsWaiting;

	public UnityEvent OnWaitStart;

	public UnityEvent OnWaitEnd;

	private float timeAtDestination;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_ConversateAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_ConversateAssembly_002DCSharp_002Edll_Excuted;

	public new string ActionName => "Conversate";

	private Transform StandPoint => Location.GetStandPoint(npc);

	public override string GetName()
	{
		if ((Object)(object)Location == (Object)null)
		{
			return ActionName + " (No destination set)";
		}
		return ActionName + " (" + ((Object)((Component)Location).gameObject).name + ")";
	}

	protected override void Start()
	{
		base.Start();
		Location.NPCs.Add(npc);
	}

	public override void Started()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		base.Started();
		if (IsAtDestination())
		{
			WalkCallback(NPCMovement.WalkResult.Success);
		}
		else
		{
			SetDestination(StandPoint.position);
		}
	}

	public override void ActiveUpdate()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		base.ActiveUpdate();
		if (!npc.Movement.IsMoving)
		{
			if (IsAtDestination())
			{
				Location.SetNPCReady(npc, ready: true);
				timeAtDestination += Time.deltaTime;
			}
			else
			{
				Location.SetNPCReady(npc, ready: false);
				timeAtDestination = 0f;
				SetDestination(StandPoint.position);
			}
		}
		else
		{
			Location.SetNPCReady(npc, ready: false);
			timeAtDestination = 0f;
		}
	}

	public override void MinPassed()
	{
		base.MinPassed();
		if (InstanceFinder.IsServer)
		{
			if (!IsConversating && timeAtDestination >= 0.1f && CanConversationStart())
			{
				StartConversate();
			}
			if (!IsConversating && !IsWaiting && timeAtDestination >= 3f && !CanConversationStart())
			{
				StartWait();
			}
			if (IsConversating && !CanConversationStart())
			{
				EndConversate();
			}
		}
	}

	public override void LateStarted()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		base.LateStarted();
		if (IsAtDestination())
		{
			WalkCallback(NPCMovement.WalkResult.Success);
		}
		else
		{
			SetDestination(StandPoint.position);
		}
	}

	public override void JumpTo()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		base.JumpTo();
		if (!IsAtDestination())
		{
			if (npc.Movement.IsMoving)
			{
				npc.Movement.Stop();
			}
			if (InstanceFinder.IsServer)
			{
				npc.Movement.Warp(StandPoint.position);
			}
			npc.Movement.FaceDirection(StandPoint.forward);
		}
	}

	public override void End()
	{
		base.End();
		Location.SetNPCReady(npc, ready: false);
		if (IsWaiting)
		{
			EndWait();
		}
		if (IsConversating)
		{
			EndConversate();
		}
	}

	public override void Interrupt()
	{
		base.Interrupt();
		Location.SetNPCReady(npc, ready: false);
		if (npc.Movement.IsMoving)
		{
			npc.Movement.Stop();
		}
		if (IsWaiting)
		{
			EndWait();
		}
		if (IsConversating)
		{
			EndConversate();
		}
	}

	public override void Resume()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		base.Resume();
		if (IsAtDestination())
		{
			WalkCallback(NPCMovement.WalkResult.Success);
		}
		else
		{
			SetDestination(StandPoint.position);
		}
	}

	private bool IsAtDestination()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(npc.Movement.FootPosition, StandPoint.position) < 1f;
	}

	private bool CanConversationStart()
	{
		return Location.NPCsReady;
	}

	protected override void WalkCallback(NPCMovement.WalkResult result)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.WalkCallback(result);
		if (base.IsActive && result == NPCMovement.WalkResult.Success)
		{
			npc.Movement.FaceDirection(StandPoint.forward);
		}
	}

	[ObserversRpc(RunLocally = true)]
	protected virtual void StartWait()
	{
		RpcWriter___Observers_StartWait_2166136261();
		RpcLogic___StartWait_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	protected virtual void EndWait()
	{
		RpcWriter___Observers_EndWait_2166136261();
		RpcLogic___EndWait_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	protected virtual void StartConversate()
	{
		RpcWriter___Observers_StartConversate_2166136261();
		RpcLogic___StartConversate_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	protected virtual void EndConversate()
	{
		RpcWriter___Observers_EndConversate_2166136261();
		RpcLogic___EndConversate_2166136261();
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
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_ConversateAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_ConversateAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_StartWait_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_EndWait_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_StartConversate_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_EndConversate_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_ConversateAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_ConversateAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_StartWait_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	protected virtual void RpcLogic___StartWait_2166136261()
	{
		if (!IsWaiting)
		{
			IsWaiting = true;
			if (OnWaitStart != null)
			{
				OnWaitStart.Invoke();
			}
		}
	}

	private void RpcReader___Observers_StartWait_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartWait_2166136261();
		}
	}

	private void RpcWriter___Observers_EndWait_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	protected virtual void RpcLogic___EndWait_2166136261()
	{
		if (IsWaiting)
		{
			IsWaiting = false;
			if (OnWaitEnd != null)
			{
				OnWaitEnd.Invoke();
			}
		}
	}

	private void RpcReader___Observers_EndWait_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___EndWait_2166136261();
		}
	}

	private void RpcWriter___Observers_StartConversate_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	protected virtual void RpcLogic___StartConversate_2166136261()
	{
		if (!IsConversating)
		{
			if (IsWaiting)
			{
				EndWait();
			}
			IsConversating = true;
			conversateRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			while (IsConversating)
			{
				Random.InitState(npc.fullName.GetHashCode() + (int)Time.time);
				float wait = Random.Range(2f, 8f);
				NPC otherNPC = Location.GetOtherNPC(npc);
				if ((Object)(object)otherNPC == (Object)null)
				{
					Debug.LogError((object)"Other NPC is null, cannot conversate");
					break;
				}
				for (float t = 0f; t < wait; t += Time.deltaTime)
				{
					if (!IsConversating)
					{
						yield break;
					}
					npc.Avatar.LookController.OverrideLookTarget(otherNPC.Avatar.LookController.HeadBone.position, 1);
					yield return (object)new WaitForEndOfFrame();
				}
				npc.VoiceOverEmitter.Play(ConversationLines[Random.Range(0, ConversationLines.Length)]);
				npc.Avatar.Animation.SetTrigger(AnimationTriggers[Random.Range(0, AnimationTriggers.Length)]);
			}
		}
	}

	private void RpcReader___Observers_StartConversate_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartConversate_2166136261();
		}
	}

	private void RpcWriter___Observers_EndConversate_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	protected virtual void RpcLogic___EndConversate_2166136261()
	{
		if (IsConversating)
		{
			IsConversating = false;
			timeAtDestination = 0f;
			if (conversateRoutine != null)
			{
				((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(conversateRoutine);
			}
		}
	}

	private void RpcReader___Observers_EndConversate_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___EndConversate_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
