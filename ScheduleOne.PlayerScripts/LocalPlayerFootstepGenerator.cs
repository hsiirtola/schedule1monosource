using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.PlayerScripts;

[RequireComponent(typeof(PlayerMovement))]
public class LocalPlayerFootstepGenerator : GenericFootstepDetector
{
	private const float DistancePerStep = 1.25f;

	private PlayerMovement _movement;

	private float _currentDistance;

	private Vector3 _lastFramePosition = Vector3.zero;

	private void Awake()
	{
		_movement = ((Component)this).GetComponent<PlayerMovement>();
	}

	protected void LateUpdate()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (!_movement.CanMove)
		{
			_currentDistance = 0f;
			_lastFramePosition = ((Component)this).transform.position;
			return;
		}
		Vector3 position = ((Component)this).transform.position;
		_currentDistance += Vector3.Distance(position, _lastFramePosition) * (_movement.IsSprinting ? 0.75f : 1f);
		if (_currentDistance >= 1.25f)
		{
			_currentDistance = 0f;
			_lastFramePosition = position;
			if (IsGrounded(out var surfaceType))
			{
				TriggerStep(surfaceType, position);
			}
		}
		_lastFramePosition = position;
	}
}
