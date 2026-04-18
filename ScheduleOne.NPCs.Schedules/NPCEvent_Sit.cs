using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using FluffyUnderware.DevTools.Extensions;
using ScheduleOne.AvatarFramework.Animation;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Schedules;

public class NPCEvent_Sit : NPCEvent
{
	public const float DESTINATION_THRESHOLD = 1.5f;

	public AvatarSeatSet SeatSet;

	public bool WarpIfSkipped;

	private bool seated;

	private AvatarSeat targetSeat;

	public UnityEvent onSeated;

	public UnityEvent onStandUp;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_SitAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_SitAssembly_002DCSharp_002Edll_Excuted;

	public new string ActionName => "Sit";

	public override string GetName()
	{
		string text = ActionName;
		if ((Object)(object)SeatSet == (Object)null)
		{
			text += "(no seat assigned)";
		}
		return text;
	}

	public override void Started()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		base.Started();
		seated = false;
		if (IsAtDestination())
		{
			WalkCallback(NPCMovement.WalkResult.Success);
			return;
		}
		targetSeat = SeatSet.GetRandomFreeSeat();
		SetDestination(targetSeat.AccessPoint.position);
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (base.IsActive && seated)
		{
			StartAction(connection, ArrayExt.IndexOf<AvatarSeat>(SeatSet.Seats, npc.Avatar.Animation.CurrentSeat));
		}
	}

	public override void LateStarted()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		base.LateStarted();
		seated = false;
		if (IsAtDestination())
		{
			WalkCallback(NPCMovement.WalkResult.Success);
			return;
		}
		targetSeat = SeatSet.GetRandomFreeSeat();
		SetDestination(targetSeat.AccessPoint.position);
	}

	public override void OnActiveTick()
	{
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (schedule.DEBUG_MODE)
		{
			Debug.Log((object)"ActiveMinPassed");
			Debug.Log((object)("Moving: " + npc.Movement.IsMoving));
			Debug.Log((object)("At destination: " + IsAtDestination()));
			Debug.Log((object)("Seated: " + seated));
		}
		if (!base.IsActive || npc.Movement.IsMoving)
		{
			return;
		}
		if (IsAtDestination() || seated)
		{
			if (!seated)
			{
				if (!npc.Movement.FaceDirectionInProgress)
				{
					npc.Movement.FaceDirection(targetSeat.SittingPoint.forward);
				}
				if (Vector3.Angle(((Component)npc.Movement).transform.forward, targetSeat.SittingPoint.forward) < 10f)
				{
					StartAction(null, ArrayExt.IndexOf<AvatarSeat>(SeatSet.Seats, SeatSet.GetRandomFreeSeat()));
				}
			}
			else if (!npc.Movement.FaceDirectionInProgress && Vector3.Angle(((Component)npc.Movement).transform.forward, targetSeat.SittingPoint.forward) > 15f)
			{
				npc.Movement.FaceDirection(targetSeat.SittingPoint.forward);
			}
		}
		else
		{
			SetDestination(targetSeat.AccessPoint.position);
		}
	}

	public override void JumpTo()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base.JumpTo();
		if (!IsAtDestination())
		{
			if (npc.Movement.IsMoving)
			{
				npc.Movement.Stop();
			}
			targetSeat = SeatSet.GetRandomFreeSeat();
			if (InstanceFinder.IsServer)
			{
				npc.Movement.Warp(targetSeat.AccessPoint.position);
				StartAction(null, ArrayExt.IndexOf<AvatarSeat>(SeatSet.Seats, SeatSet.GetRandomFreeSeat()));
			}
			npc.Movement.FaceDirection(targetSeat.AccessPoint.forward, 0f);
		}
	}

	public override void End()
	{
		base.End();
		if (InstanceFinder.IsServer && seated)
		{
			EndAction();
		}
	}

	public override void Interrupt()
	{
		base.Interrupt();
		if (InstanceFinder.IsServer)
		{
			if (npc.Movement.IsMoving)
			{
				npc.Movement.Stop();
			}
			if (seated)
			{
				EndAction();
			}
		}
	}

	public override void Resume()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		base.Resume();
		if (IsAtDestination())
		{
			WalkCallback(NPCMovement.WalkResult.Success);
			return;
		}
		targetSeat = SeatSet.GetRandomFreeSeat();
		SetDestination(targetSeat.AccessPoint.position);
	}

	public override void Skipped()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.Skipped();
		if (WarpIfSkipped)
		{
			targetSeat = SeatSet.GetRandomFreeSeat();
			npc.Movement.Warp(targetSeat.AccessPoint.position);
		}
	}

	private bool IsAtDestination()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetSeat == (Object)null)
		{
			return false;
		}
		return Vector3.Distance(npc.Movement.FootPosition, targetSeat.AccessPoint.position) < 1.5f;
	}

	protected override void WalkCallback(NPCMovement.WalkResult result)
	{
		base.WalkCallback(result);
		if (base.IsActive && result == NPCMovement.WalkResult.Success && InstanceFinder.IsServer)
		{
			StartAction(null, ArrayExt.IndexOf<AvatarSeat>(SeatSet.Seats, SeatSet.GetRandomFreeSeat()));
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	protected virtual void StartAction(NetworkConnection conn, int seatIndex)
	{
		if (conn == null)
		{
			RpcWriter___Observers_StartAction_2681120339(conn, seatIndex);
			RpcLogic___StartAction_2681120339(conn, seatIndex);
		}
		else
		{
			RpcWriter___Target_StartAction_2681120339(conn, seatIndex);
		}
	}

	[ObserversRpc(RunLocally = true)]
	protected virtual void EndAction()
	{
		RpcWriter___Observers_EndAction_2166136261();
		RpcLogic___EndAction_2166136261();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_SitAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_SitAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_StartAction_2681120339));
			((NetworkBehaviour)this).RegisterTargetRpc(1u, new ClientRpcDelegate(RpcReader___Target_StartAction_2681120339));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_EndAction_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_SitAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_SitAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_StartAction_2681120339(NetworkConnection conn, int seatIndex)
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
			((Writer)writer).WriteInt32(seatIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	protected virtual void RpcLogic___StartAction_2681120339(NetworkConnection conn, int seatIndex)
	{
		if (!seated)
		{
			seated = true;
			if (seatIndex >= 0 && seatIndex < SeatSet.Seats.Length)
			{
				targetSeat = SeatSet.Seats[seatIndex];
			}
			else
			{
				targetSeat = null;
			}
			npc.Movement.SetSeat(targetSeat);
			if (onSeated != null)
			{
				onSeated.Invoke();
			}
		}
	}

	private void RpcReader___Observers_StartAction_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int seatIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartAction_2681120339(null, seatIndex);
		}
	}

	private void RpcWriter___Target_StartAction_2681120339(NetworkConnection conn, int seatIndex)
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
			((Writer)writer).WriteInt32(seatIndex, (AutoPackType)1);
			((NetworkBehaviour)this).SendTargetRpc(1u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_StartAction_2681120339(PooledReader PooledReader0, Channel channel)
	{
		int seatIndex = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___StartAction_2681120339(((NetworkBehaviour)this).LocalConnection, seatIndex);
		}
	}

	private void RpcWriter___Observers_EndAction_2166136261()
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

	protected virtual void RpcLogic___EndAction_2166136261()
	{
		if (seated)
		{
			seated = false;
			npc.Movement.SetSeat(null);
			if (onStandUp != null)
			{
				onStandUp.Invoke();
			}
		}
	}

	private void RpcReader___Observers_EndAction_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___EndAction_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
