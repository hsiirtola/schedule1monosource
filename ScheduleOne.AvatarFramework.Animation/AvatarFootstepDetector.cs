using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Animation;

[RequireComponent(typeof(Avatar))]
public class AvatarFootstepDetector : GenericFootstepDetector
{
	private const float StepThreshold = 0.125f;

	[SerializeField]
	private float _detectionRange = 20f;

	private Avatar _avatar;

	private bool _leftDown;

	private bool _rightDown;

	private float _detectionRangeSqr;

	private Transform _leftBone => _avatar.LeftFootBone;

	private Transform _rightBone => _avatar.RightFootBone;

	private void Awake()
	{
		_avatar = ((Component)this).GetComponent<Avatar>();
		_detectionRangeSqr = _detectionRange * _detectionRange;
	}

	protected virtual void LateUpdate()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_avatar == (Object)null || (Object)(object)_avatar.Animation == (Object)null || (Object)(object)_avatar.Animation.animator == (Object)null)
		{
			return;
		}
		if (!((Behaviour)_avatar.Animation.animator).enabled)
		{
			_leftDown = false;
			_rightDown = false;
		}
		else
		{
			if (!PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				return;
			}
			if (Vector3.SqrMagnitude(_referencePoint.position - ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position) > _detectionRangeSqr)
			{
				_leftDown = false;
				_rightDown = false;
			}
			else
			{
				if (IsCooldown())
				{
					return;
				}
				if (_leftBone.position.y - _referencePoint.position.y < 0.125f)
				{
					if (!_leftDown)
					{
						_leftDown = true;
						if (IsGrounded(out var surfaceType))
						{
							TriggerStep(surfaceType, ((Component)_leftBone).transform.position);
						}
					}
				}
				else
				{
					_leftDown = false;
				}
				if (_rightBone.position.y - _referencePoint.position.y < 0.125f)
				{
					if (!_rightDown)
					{
						_rightDown = true;
						if (IsGrounded(out var surfaceType2))
						{
							TriggerStep(surfaceType2, ((Component)_rightBone).transform.position);
						}
					}
				}
				else
				{
					_rightDown = false;
				}
			}
		}
	}
}
