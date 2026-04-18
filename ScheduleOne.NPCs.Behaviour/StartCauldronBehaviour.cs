using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Employees;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class StartCauldronBehaviour : Behaviour
{
	public const float START_CAULDRON_TIME = 15f;

	private Coroutine workRoutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartCauldronBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartCauldronBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public Cauldron Station { get; protected set; }

	public bool WorkInProgress { get; protected set; }

	public override void Activate()
	{
		base.Activate();
		StartWork();
	}

	public override void Resume()
	{
		base.Resume();
		StartWork();
	}

	public override void Pause()
	{
		base.Pause();
		if (WorkInProgress)
		{
			StopCauldron();
		}
		if (InstanceFinder.IsServer && (Object)(object)Station != (Object)null && (Object)(object)Station.NPCUserObject == (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject)
		{
			Station.SetNPCUser(null);
		}
	}

	public override void Disable()
	{
		base.Disable();
		if (base.Active)
		{
			Deactivate();
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (WorkInProgress)
		{
			StopCauldron();
		}
		if (InstanceFinder.IsServer && (Object)(object)Station != (Object)null && (Object)(object)Station.NPCUserObject == (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject)
		{
			Station.SetNPCUser(null);
		}
	}

	public override void OnActiveTick()
	{
		base.OnActiveTick();
		if (!InstanceFinder.IsServer || WorkInProgress)
		{
			return;
		}
		if (IsStationReady(Station))
		{
			if (!base.Npc.Movement.IsMoving)
			{
				if (IsAtStation())
				{
					BeginCauldron();
				}
				else
				{
					GoToStation();
				}
			}
		}
		else
		{
			Disable_Networked(null);
		}
	}

	private void StartWork()
	{
		if (InstanceFinder.IsServer)
		{
			if (!IsStationReady(Station))
			{
				Console.LogWarning(base.Npc.fullName + " has no station to work with");
				Disable_Networked(null);
			}
			else
			{
				Station.SetNPCUser(((NetworkBehaviour)base.Npc).NetworkObject);
			}
		}
	}

	public void AssignStation(Cauldron station)
	{
		if (!((Object)(object)Station == (Object)(object)station))
		{
			if ((Object)(object)Station != (Object)null && (Object)(object)Station.NPCUserObject == (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject)
			{
				Console.Log("Clearing NPC user from previous rack: " + ((Object)Station).name);
				Station.SetNPCUser(null);
			}
			Station = station;
		}
	}

	public bool IsAtStation()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return base.Npc.Movement.IsAsCloseAsPossible(Station.StandPoint.position);
	}

	public void GoToStation()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		SetDestination(Station.StandPoint.position);
	}

	[ObserversRpc(RunLocally = true)]
	public void BeginCauldron()
	{
		RpcWriter___Observers_BeginCauldron_2166136261();
		RpcLogic___BeginCauldron_2166136261();
	}

	private void StopCauldron()
	{
		if (workRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(workRoutine);
		}
		if (InstanceFinder.IsServer && (Object)(object)Station != (Object)null && (Object)(object)Station.NPCUserObject == (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject)
		{
			Station.SetNPCUser(null);
		}
		WorkInProgress = false;
	}

	public bool IsStationReady(Cauldron station)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)station == (Object)null)
		{
			return false;
		}
		if (station.GetState() != Cauldron.EState.Ready)
		{
			return false;
		}
		if (((IUsable)station).IsInUse && ((Object)(object)station.PlayerUserObject != (Object)null || (Object)(object)station.NPCUserObject != (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject))
		{
			return false;
		}
		if (!base.Npc.Movement.CanGetTo(station.StandPoint.position))
		{
			return false;
		}
		return true;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartCauldronBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartCauldronBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_BeginCauldron_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartCauldronBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartCauldronBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_BeginCauldron_2166136261()
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

	public void RpcLogic___BeginCauldron_2166136261()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!WorkInProgress && !((Object)(object)Station == (Object)null))
		{
			WorkInProgress = true;
			base.Npc.Movement.FaceDirection(Station.StandPoint.forward);
			workRoutine = ((MonoBehaviour)this).StartCoroutine(Package());
		}
		IEnumerator Package()
		{
			yield return (object)new WaitForEndOfFrame();
			base.Npc.Avatar.Animation.SetBool("UseChemistryStation", value: true);
			float packageTime = 15f;
			packageTime /= (base.Npc as Employee).CurrentWorkSpeed;
			for (float i = 0f; i < packageTime; i += Time.deltaTime)
			{
				base.Npc.Avatar.LookController.OverrideLookTarget(Station.LinkOrigin.position, 0);
				yield return (object)new WaitForEndOfFrame();
			}
			base.Npc.Avatar.Animation.SetBool("UseChemistryStation", value: false);
			if (InstanceFinder.IsServer)
			{
				EQuality quality = Station.RemoveIngredients();
				Station.StartCookOperation(null, Station.CookTime, quality);
			}
			WorkInProgress = false;
			workRoutine = null;
		}
	}

	private void RpcReader___Observers_BeginCauldron_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___BeginCauldron_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
