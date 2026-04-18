using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.StationFramework;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class UseSpawnStationBehaviour : Behaviour
{
	private const float TaskDuration = 6f;

	private const float ProximityThreshold = 0.6f;

	private const string AnimationBoolName = "UsePackagingStation";

	private bool _currentlyUsingStation;

	private Coroutine _workRoutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EUseSpawnStationBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EUseSpawnStationBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public MushroomSpawnStation Station { get; protected set; }

	public void AssignStation(MushroomSpawnStation station)
	{
		if (!((Object)(object)Station == (Object)(object)station))
		{
			if ((Object)(object)Station != (Object)null && ((IUsable)Station).IsInUseByNPC(base.Npc))
			{
				Station.SetNPCUser(null);
			}
			Station = station;
		}
	}

	public override void Activate()
	{
		base.Activate();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (_currentlyUsingStation)
		{
			StopWork();
		}
		Station = null;
	}

	public override void Resume()
	{
		base.Resume();
	}

	public override void Pause()
	{
		base.Pause();
		if (_currentlyUsingStation)
		{
			StopWork();
		}
	}

	public override void OnActiveTick()
	{
		base.OnActiveTick();
		if (!InstanceFinder.IsServer || _currentlyUsingStation)
		{
			return;
		}
		if (IsStationReady(Station))
		{
			if (!base.Npc.Movement.IsMoving)
			{
				if (IsAtStation())
				{
					BeginWork();
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

	public bool IsAtStation()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return base.Npc.Movement.IsAsCloseAsPossible(NavMeshUtility.GetReachableAccessPoint(Station, base.Npc).position, 0.6f);
	}

	public void GoToStation()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		SetDestination(NavMeshUtility.GetReachableAccessPoint(Station, base.Npc).position);
	}

	[ObserversRpc(RunLocally = true)]
	public void BeginWork()
	{
		RpcWriter___Observers_BeginWork_2166136261();
		RpcLogic___BeginWork_2166136261();
	}

	private void StopWork()
	{
		if (_workRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_workRoutine);
			_workRoutine = null;
		}
		base.Npc.SetAnimationBool("UsePackagingStation", val: false);
		if ((Object)(object)Station != (Object)null && InstanceFinder.IsServer && ((IUsable)Station).IsInUseByNPC(base.Npc))
		{
			Station.SetNPCUser(null);
		}
		_currentlyUsingStation = false;
	}

	public bool IsStationReady(MushroomSpawnStation station)
	{
		if ((Object)(object)station == (Object)null)
		{
			return false;
		}
		if (((IUsable)station).IsInUse && !((IUsable)station).IsInUseByNPC(base.Npc))
		{
			return false;
		}
		if (!station.DoesStationContainRequiredItems())
		{
			return false;
		}
		if (!station.DoesStationHaveOutputSpace())
		{
			return false;
		}
		if (!base.Npc.Movement.CanGetTo((ITransitEntity)station, 0.6f))
		{
			return false;
		}
		return true;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EUseSpawnStationBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EUseSpawnStationBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_BeginWork_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EUseSpawnStationBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EUseSpawnStationBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_BeginWork_2166136261()
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

	public void RpcLogic___BeginWork_2166136261()
	{
		if (!_currentlyUsingStation && IsStationReady(Station))
		{
			_currentlyUsingStation = true;
			if (InstanceFinder.IsServer)
			{
				Station.SetNPCUser(((NetworkBehaviour)base.Npc).NetworkObject);
			}
			_workRoutine = ((MonoBehaviour)this).StartCoroutine(Package());
		}
		IEnumerator Package()
		{
			base.Npc.SetAnimationBool("UsePackagingStation", val: true);
			float scaledTaskDuration = 6f / (base.Npc as Employee).CurrentWorkSpeed;
			float progress = 0f;
			while (progress < scaledTaskDuration)
			{
				progress += Time.deltaTime;
				base.Npc.Avatar.LookController.OverrideLookTarget(Station.UIPoint.position, 0, rotateBody: true);
				yield return null;
			}
			if (InstanceFinder.IsServer)
			{
				if ((Object)(object)Station != (Object)null && Station.DoesStationContainRequiredItems() && Station.DoesStationHaveOutputSpace())
				{
					SporeSyringeDefinition sporeSyringeDefinition = Station.SyringeSlot.ItemInstance.Definition as SporeSyringeDefinition;
					Station.SyringeSlot.ChangeQuantity(-1);
					Station.GrainBagSlot.ChangeQuantity(-1);
					Station.OutputSlot.AddItem(sporeSyringeDefinition.SpawnDefinition.GetDefaultInstance());
				}
				if (IsStationReady(Station))
				{
					_workRoutine = ((MonoBehaviour)this).StartCoroutine(Package());
				}
				else
				{
					Disable_Networked(null);
				}
			}
		}
	}

	private void RpcReader___Observers_BeginWork_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___BeginWork_2166136261();
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
