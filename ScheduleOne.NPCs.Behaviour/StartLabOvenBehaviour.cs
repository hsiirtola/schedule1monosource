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
using ScheduleOne.StationFramework;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class StartLabOvenBehaviour : Behaviour
{
	public const float POUR_TIME = 5f;

	private Chemist chemist;

	private Coroutine cookRoutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public LabOven targetOven { get; private set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public void SetTargetOven(LabOven oven)
	{
		targetOven = oven;
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if ((Object)(object)targetOven != (Object)null)
		{
			targetOven.Door.SetPosition(0f);
		}
		if (cookRoutine != null)
		{
			StopCook();
		}
		Disable();
	}

	public override void OnActiveTick()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (cookRoutine == null && InstanceFinder.IsServer && !base.Npc.Movement.IsMoving)
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
		if (cookRoutine != null)
		{
			base.Npc.Avatar.LookController.OverrideLookTarget(targetOven.UIPoint.position, 5);
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
		if ((Object)(object)targetOven == (Object)null)
		{
			return false;
		}
		if (((IUsable)targetOven).IsInUse && (Object)(object)((IUsable)targetOven).NPCUserObject != (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject)
		{
			return false;
		}
		if (targetOven.CurrentOperation != null)
		{
			return false;
		}
		if (!targetOven.IsIngredientCookable())
		{
			return false;
		}
		return true;
	}

	private void StopCook()
	{
		if ((Object)(object)targetOven != (Object)null)
		{
			targetOven.SetNPCUser(null);
		}
		if (cookRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(cookRoutine);
			cookRoutine = null;
		}
	}

	private Vector3 GetStationAccessPoint()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetOven == (Object)null)
		{
			return ((Component)base.Npc).transform.position;
		}
		return ((ITransitEntity)targetOven).AccessPoints[0].position;
	}

	private bool IsAtStation()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetOven == (Object)null)
		{
			return false;
		}
		return Vector3.Distance(((Component)base.Npc).transform.position, GetStationAccessPoint()) < 1f;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_StartCook_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
		if (cookRoutine == null && !((Object)(object)targetOven == (Object)null))
		{
			cookRoutine = ((MonoBehaviour)this).StartCoroutine(CookRoutine());
		}
		IEnumerator CookRoutine()
		{
			targetOven.SetNPCUser(((NetworkBehaviour)base.Npc).NetworkObject);
			base.Npc.Movement.FacePoint(((Component)targetOven).transform.position);
			yield return (object)new WaitForSeconds(0.5f);
			if (!CanCookStart())
			{
				StopCook();
				Deactivate_Networked(null);
			}
			else
			{
				targetOven.Door.SetPosition(1f);
				yield return (object)new WaitForSeconds(0.5f);
				targetOven.WireTray.SetPosition(1f);
				yield return (object)new WaitForSeconds(5f / (base.Npc as Employee).CurrentWorkSpeed);
				targetOven.Door.SetPosition(0f);
				yield return (object)new WaitForSeconds(1f);
				ItemInstance itemInstance = targetOven.IngredientSlot.ItemInstance;
				if (itemInstance == null)
				{
					Console.LogWarning("No ingredient in oven!");
					StopCook();
					Deactivate_Networked(null);
				}
				else
				{
					int num = 1;
					if ((itemInstance.Definition as StorableItemDefinition).StationItem.GetModule<CookableModule>().CookType == CookableModule.ECookableType.Solid)
					{
						num = Mathf.Min(targetOven.IngredientSlot.Quantity, 10);
					}
					((BaseItemInstance)itemInstance).ChangeQuantity(-num);
					string iD = ((BaseItemDefinition)(itemInstance.Definition as StorableItemDefinition).StationItem.GetModule<CookableModule>().Product).ID;
					EQuality ingredientQuality = EQuality.Standard;
					if (itemInstance is QualityItemInstance)
					{
						ingredientQuality = (itemInstance as QualityItemInstance).Quality;
					}
					targetOven.SendCookOperation(new OvenCookOperation(((BaseItemInstance)itemInstance).ID, ingredientQuality, num, iD));
					StopCook();
					Deactivate_Networked(null);
				}
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

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EStartLabOvenBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		chemist = base.Npc as Chemist;
	}
}
