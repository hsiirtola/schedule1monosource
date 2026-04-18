using System;
using System.Collections;
using FishNet;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Quests;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class DealerAttendDealBehaviour : Behaviour
{
	private Dealer _dealer;

	private Contract _contract;

	private Customer _customer;

	private Coroutine _handoverRoutine;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDealerAttendDealBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDealerAttendDealBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EDealerAttendDealBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public void AssignContract(Contract contract)
	{
		_contract = contract;
		_customer = ((Component)_contract.Customer).GetComponent<Customer>();
	}

	public override void Activate()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		base.Activate();
		SetDestination(GetStandPosition());
	}

	public override void Resume()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		base.Resume();
		SetDestination(GetStandPosition());
	}

	public override void Pause()
	{
		base.Pause();
		StopHandover();
		base.Npc.Movement.SpeedController.RemoveSpeedControl("urgent_contract");
	}

	public override void Deactivate()
	{
		base.Deactivate();
		StopHandover();
		_contract = null;
		_customer = null;
		base.Npc.Movement.SpeedController.RemoveSpeedControl("urgent_contract");
	}

	public override void OnActiveTick()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if ((Object)(object)_contract == (Object)null || _contract.State != EQuestState.Active)
		{
			if (InstanceFinder.IsServer)
			{
				Disable_Server();
			}
		}
		else
		{
			if (_handoverRoutine != null)
			{
				return;
			}
			if (_contract.GetMinsUntilExpiry() <= 90)
			{
				base.Npc.Movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("urgent_contract", 10, 0.8f));
			}
			if (base.Npc.Movement.IsMoving)
			{
				return;
			}
			if (IsAtDestination())
			{
				if (IsCustomerReadyForHandover())
				{
					BeginHandover();
				}
			}
			else
			{
				SetDestination(GetStandPosition());
			}
		}
	}

	private void BeginHandover()
	{
		if (_handoverRoutine == null)
		{
			_handoverRoutine = ((MonoBehaviour)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			base.Npc.Movement.FaceDirection(GetDirectionToFace());
			yield return (object)new WaitForSeconds(2f);
			yield return (object)new WaitUntil((Func<bool>)(() => _customer.IsAtDealLocation()));
			base.Npc.SetAnimationTrigger("GrabItem");
			if (InstanceFinder.IsServer)
			{
				_dealer.RemoveContractItems(_contract, _customer.CustomerData.Standards.GetCorrespondingQuality(), out var items);
				_customer.OfferDealItems(items, offeredByPlayer: false, out var accepted);
				if (!accepted)
				{
					foreach (ItemInstance item in items)
					{
						_dealer.AddItemToInventory(item);
					}
				}
				Disable_Server();
			}
			_handoverRoutine = null;
		}
	}

	private void StopHandover()
	{
		if (_handoverRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_handoverRoutine);
			_handoverRoutine = null;
		}
	}

	private bool IsAtDestination()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(base.Npc.Movement.FootPosition, GetStandPosition()) < 2f;
	}

	private bool IsCustomerReadyForHandover()
	{
		return _customer.IsAtDealLocation();
	}

	private Vector3 GetStandPosition()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_contract == (Object)null)
		{
			return Vector3.zero;
		}
		return _contract.DeliveryLocation.CustomerStandPoint.position + _contract.DeliveryLocation.CustomerStandPoint.forward * 1.2f;
	}

	private Vector3 GetDirectionToFace()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_contract == (Object)null)
		{
			return Vector3.zero;
		}
		return -_contract.DeliveryLocation.CustomerStandPoint.forward;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDealerAttendDealBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDealerAttendDealBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDealerAttendDealBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDealerAttendDealBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EDealerAttendDealBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		_dealer = base.Npc as Dealer;
	}
}
