using FishNet.Object;
using UnityEngine;

namespace ScheduleOne.Effects;

public abstract class EffectController : NetworkBehaviour
{
	protected float _distanceToPlayerNormalised = -1f;

	protected float _enclosureBlend = -1f;

	protected Vector3 _playerPosition = Vector3.zero;

	protected Vector3 _anchoredPosition = Vector3.zero;

	private bool NetworkInitialize___EarlyScheduleOne_002EEffects_002EEffectControllerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEffects_002EEffectControllerAssembly_002DCSharp_002Edll_Excuted;

	public bool IsActive { get; protected set; }

	public abstract void Activate();

	public abstract void Deactivate();

	public virtual void UpdateProperties(Vector3 anchorPosition, Vector3 playerPosition, float sqrDistanceToPlayer, float enclosureBlend)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		_anchoredPosition = anchorPosition;
		_playerPosition = playerPosition;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EEffects_002EEffectControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEffects_002EEffectControllerAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEffects_002EEffectControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEffects_002EEffectControllerAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}
}
