using System;
using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using FluffyUnderware.DevTools.Extensions;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.Map;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules;

public class NPCEvent_StayInBuilding : NPCEvent
{
	public NPCEnterableBuilding Building;

	[Header("Optionally specify door to use. Otherwise closest door will be used.")]
	public StaticDoor Door;

	private bool IsEntering;

	private Coroutine enterRoutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuildingAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuildingAssembly_002DCSharp_002Edll_Excuted;

	public new string ActionName => "Stay in Building";

	private bool InBuilding => (Object)(object)npc.CurrentBuilding == (Object)(object)Building;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuilding_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override string GetName()
	{
		if ((Object)(object)Building == (Object)null)
		{
			return ActionName + " (No building set)";
		}
		return ActionName + " (" + Building.BuildingName + ")";
	}

	public override void Started()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		base.Started();
		if (base.IsActive && !((Object)(object)Building == (Object)null) && InstanceFinder.IsServer)
		{
			SetDestination(GetEntryPoint().position);
		}
	}

	public override void OnActiveTick()
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (!base.IsActive || !InstanceFinder.IsServer)
		{
			return;
		}
		if (schedule.DEBUG_MODE)
		{
			Debug.Log((object)"StayInBuilding: ActiveMinPassed");
			Debug.Log((object)("In building: " + InBuilding));
			Debug.Log((object)("Is entering: " + IsEntering));
		}
		if (!((Object)(object)Building == (Object)null) && Building.Doors.Length != 0 && !InBuilding && !IsEntering && (!npc.Movement.IsMoving || Vector3.Distance(npc.Movement.CurrentDestination, GetEntryPoint().position) > 2f))
		{
			if (Vector3.Distance(((Component)npc).transform.position, GetEntryPoint().position) < 0.5f)
			{
				PlayEnterAnimation();
			}
			else if (npc.Movement.CanMove())
			{
				SetDestination(GetEntryPoint().position);
			}
		}
	}

	public override void LateStarted()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		base.LateStarted();
		if (!((Object)(object)Building == (Object)null) && Building.Doors.Length != 0 && InstanceFinder.IsServer)
		{
			SetDestination(GetEntryPoint().position);
		}
	}

	public override void JumpTo()
	{
		base.JumpTo();
		if (InstanceFinder.IsServer)
		{
			if (npc.Movement.IsMoving)
			{
				npc.Movement.Stop();
			}
			PlayEnterAnimation();
		}
	}

	public override void End()
	{
		base.End();
		CancelEnter();
		if (InBuilding)
		{
			ExitBuilding();
		}
		else
		{
			npc.Movement.Stop();
		}
	}

	public override void Interrupt()
	{
		base.Interrupt();
		CancelEnter();
		if (InBuilding)
		{
			ExitBuilding();
		}
		else
		{
			npc.Movement.Stop();
		}
	}

	public override void Skipped()
	{
		base.Skipped();
	}

	public override void Resume()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		base.Resume();
		if (!InBuilding && InstanceFinder.IsServer)
		{
			SetDestination(GetEntryPoint().position);
		}
	}

	protected override void WalkCallback(NPCMovement.WalkResult result)
	{
		base.WalkCallback(result);
		if (base.IsActive && InstanceFinder.IsServer && (result == NPCMovement.WalkResult.Success || result == NPCMovement.WalkResult.Partial))
		{
			PlayEnterAnimation();
		}
	}

	[ObserversRpc(RunLocally = true)]
	private void PlayEnterAnimation()
	{
		RpcWriter___Observers_PlayEnterAnimation_2166136261();
		RpcLogic___PlayEnterAnimation_2166136261();
	}

	private void CancelEnter()
	{
		IsEntering = false;
		if (enterRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(enterRoutine);
		}
	}

	protected virtual void EnterBuilding(int doorIndex)
	{
		if ((Object)(object)Building == (Object)null)
		{
			Console.LogWarning("Building is null in StayInBuilding event");
		}
		else if (InstanceFinder.IsServer)
		{
			npc.EnterBuilding(null, Building.GUID.ToString(), doorIndex);
		}
	}

	private void ExitBuilding()
	{
		if (InstanceFinder.IsServer)
		{
			npc.ExitBuilding();
		}
	}

	private Transform GetEntryPoint()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Door != (Object)null)
		{
			return Door.AccessPoint;
		}
		if ((Object)(object)Building == (Object)null)
		{
			return null;
		}
		StaticDoor closestDoor = Building.GetClosestDoor(npc.Movement.FootPosition, useableOnly: true);
		if ((Object)(object)closestDoor == (Object)null)
		{
			return null;
		}
		return closestDoor.AccessPoint;
	}

	private StaticDoor GetDoor(out int doorIndex)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		doorIndex = -1;
		if ((Object)(object)Door != (Object)null)
		{
			doorIndex = ArrayExt.IndexOf<StaticDoor>(Building.Doors, Door);
			return Door;
		}
		if ((Object)(object)Building == (Object)null)
		{
			return null;
		}
		if ((Object)(object)npc == (Object)null)
		{
			return null;
		}
		StaticDoor closestDoor = Building.GetClosestDoor(npc.Movement.FootPosition, useableOnly: true);
		doorIndex = ArrayExt.IndexOf<StaticDoor>(Building.Doors, closestDoor);
		return closestDoor;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuildingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuildingAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_PlayEnterAnimation_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuildingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuildingAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_PlayEnterAnimation_2166136261()
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

	private void RpcLogic___PlayEnterAnimation_2166136261()
	{
		if (!IsEntering)
		{
			enterRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Enter());
		}
		IEnumerator Enter()
		{
			IsEntering = true;
			yield return (object)new WaitUntil((Func<bool>)(() => !npc.Movement.IsMoving));
			int doorIndex;
			StaticDoor door = GetDoor(out doorIndex);
			if ((Object)(object)door != (Object)null)
			{
				Transform faceDir = ((Component)door).transform;
				npc.Movement.FacePoint(faceDir.position, 0.3f);
				float t = 0f;
				while (Vector3.SignedAngle(((Component)npc.Avatar).transform.forward, faceDir.position - npc.Avatar.CenterPoint, Vector3.up) > 15f && t < 1f)
				{
					yield return (object)new WaitForEndOfFrame();
					t += Time.deltaTime;
				}
			}
			npc.Avatar.Animation.SetTrigger("GrabItem");
			yield return (object)new WaitForSeconds(0.6f);
			IsEntering = false;
			enterRoutine = null;
			EnterBuilding(doorIndex);
		}
	}

	private void RpcReader___Observers_PlayEnterAnimation_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___PlayEnterAnimation_2166136261();
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCEvent_StayInBuilding_Assembly_002DCSharp_002Edll()
	{
		((NPCAction)this).Awake();
	}
}
