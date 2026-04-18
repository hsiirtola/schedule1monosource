using ScheduleOne.Audio;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.GamePhysics;
using ScheduleOne.Materials;
using UnityEngine;

namespace ScheduleOne.Tools;

public abstract class GenericFootstepDetector : MonoBehaviour
{
	private const float GroundDetectionRange = 0.15f;

	private const float GroundDetectionRayOriginShift = 0.5f;

	[SerializeField]
	private float _baseVolume = 1f;

	[SerializeField]
	private float _stepDetectionCooldown = 0.1f;

	[SerializeField]
	protected Transform _referencePoint;

	private float _timeOnLastStep;

	private static LayerMask _groundDetectionLayerMask = LayerMask.op_Implicit(-1);

	public float VolumeMultiplier { get; set; } = 1f;

	private void Awake()
	{
	}

	protected virtual void Start()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (LayerMask.op_Implicit(_groundDetectionLayerMask) == -1 && NetworkSingleton<PhysicsManager>.InstanceExists)
		{
			_groundDetectionLayerMask = NetworkSingleton<PhysicsManager>.Instance.GroundDetectionLayerMask;
			_groundDetectionLayerMask = LayerMask.op_Implicit(LayerMask.op_Implicit(_groundDetectionLayerMask) | (1 << LayerMask.NameToLayer("Water")));
		}
	}

	protected void TriggerStep(EMaterialType materialType, Vector3 stepPosition)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!(Time.timeSinceLevelLoad - _timeOnLastStep < _stepDetectionCooldown))
		{
			_timeOnLastStep = Time.timeSinceLevelLoad;
			if (Singleton<SFXManager>.InstanceExists)
			{
				Singleton<SFXManager>.Instance.PlayFootstepSound(materialType, _baseVolume * VolumeMultiplier, stepPosition);
			}
		}
	}

	protected bool IsCooldown()
	{
		return Time.timeSinceLevelLoad - _timeOnLastStep < _stepDetectionCooldown;
	}

	protected bool IsGrounded(out EMaterialType surfaceType)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected I4, but got Unknown
		surfaceType = (EMaterialType)0;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(_referencePoint.position + Vector3.up * 0.5f, Vector3.down, ref val, 0.65f, LayerMask.op_Implicit(_groundDetectionLayerMask), (QueryTriggerInteraction)2))
		{
			MaterialTag componentInParent = ((Component)((RaycastHit)(ref val)).collider).GetComponentInParent<MaterialTag>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				surfaceType = (EMaterialType)(int)componentInParent.MaterialType;
			}
			return true;
		}
		return false;
	}
}
