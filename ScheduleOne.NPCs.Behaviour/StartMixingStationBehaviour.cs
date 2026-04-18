using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Employees;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class StartMixingStationBehaviour : Behaviour
{
	public const float INSERT_INGREDIENT_BASE_TIME = 1f;

	private Chemist chemist;

	private Coroutine startRoutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public MixingStation targetStation { get; private set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public void AssignStation(MixingStation station)
	{
		targetStation = station;
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (startRoutine != null)
		{
			StopCook();
		}
		if ((Object)(object)targetStation != (Object)null)
		{
			targetStation.SetNPCUser(null);
		}
		Disable();
	}

	public override void Pause()
	{
		base.Pause();
		if ((Object)(object)targetStation != (Object)null)
		{
			targetStation.SetNPCUser(null);
		}
	}

	public override void OnActiveTick()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (startRoutine == null && InstanceFinder.IsServer && !base.Npc.Movement.IsMoving)
		{
			if (IsAtStation())
			{
				StartCook();
			}
			else
			{
				SetDestination(GetStationAccessPoint());
			}
		}
	}

	public override void BehaviourUpdate()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.BehaviourUpdate();
		if (startRoutine != null)
		{
			base.Npc.Avatar.LookController.OverrideLookTarget(targetStation.UIPoint.position, 5);
		}
	}

	[ObserversRpc(RunLocally = true)]
	private void StartCook()
	{
		RpcWriter___Observers_StartCook_2166136261();
		RpcLogic___StartCook_2166136261();
	}

	private bool CanCookStart()
	{
		if ((Object)(object)targetStation == (Object)null)
		{
			return false;
		}
		if (((IUsable)targetStation).IsInUse && (Object)(object)((IUsable)targetStation).NPCUserObject != (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject)
		{
			return false;
		}
		MixingStationConfiguration mixingStationConfiguration = targetStation.Configuration as MixingStationConfiguration;
		if ((float)targetStation.GetMixQuantity() < mixingStationConfiguration.StartThrehold.Value)
		{
			return false;
		}
		return true;
	}

	private void StopCook()
	{
		if ((Object)(object)targetStation != (Object)null)
		{
			targetStation.SetNPCUser(null);
		}
		base.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", value: false);
		if (startRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(startRoutine);
			startRoutine = null;
		}
	}

	private Vector3 GetStationAccessPoint()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetStation == (Object)null)
		{
			return ((Component)base.Npc).transform.position;
		}
		return ((ITransitEntity)targetStation).AccessPoints[0].position;
	}

	private bool IsAtStation()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetStation == (Object)null)
		{
			return false;
		}
		return Vector3.Distance(((Component)base.Npc).transform.position, GetStationAccessPoint()) < 1f;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_StartCook_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_StartCook_2166136261()
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

	private void RpcLogic___StartCook_2166136261()
	{
		if (startRoutine == null && !((Object)(object)targetStation == (Object)null))
		{
			startRoutine = ((MonoBehaviour)this).StartCoroutine(CookRoutine());
		}
		IEnumerator CookRoutine()
		{
			base.Npc.Movement.FacePoint(((Component)targetStation).transform.position);
			yield return (object)new WaitForSeconds(0.5f);
			if (!CanCookStart())
			{
				StopCook();
				Deactivate_Networked(null);
			}
			else
			{
				targetStation.SetNPCUser(((NetworkBehaviour)base.Npc).NetworkObject);
				base.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", value: true);
				QualityItemInstance product = targetStation.ProductSlot.ItemInstance as QualityItemInstance;
				ItemInstance mixer = targetStation.MixerSlot.ItemInstance;
				int mixQuantity = targetStation.GetMixQuantity();
				for (int i = 0; i < mixQuantity; i++)
				{
					yield return (object)new WaitForSeconds(1f / (base.Npc as Employee).CurrentWorkSpeed);
				}
				if (InstanceFinder.IsServer)
				{
					targetStation.ProductSlot.ChangeQuantity(-mixQuantity);
					targetStation.MixerSlot.ChangeQuantity(-mixQuantity);
					MixOperation operation = new MixOperation(((BaseItemInstance)product).ID, product.Quality, ((BaseItemInstance)mixer).ID, mixQuantity);
					targetStation.SendMixingOperation(operation, 0);
				}
				StopCook();
				Deactivate_Networked(null);
			}
		}
	}

	private void RpcReader___Observers_StartCook_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartCook_2166136261();
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EStartMixingStationBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		chemist = base.Npc as Chemist;
	}
}
