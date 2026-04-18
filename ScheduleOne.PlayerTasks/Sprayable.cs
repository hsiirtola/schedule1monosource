using System;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerTasks;

public class Sprayable : Draggable
{
	[Header("Sprayable Components")]
	[SerializeField]
	private Transform _sprayOrigin;

	[Header("Gizmos")]
	[SerializeField]
	private bool _drawGizmos = true;

	public Action _onSuccessfulSpray;

	public UnityEvent onSpray;

	private float _sprayRadius;

	private float _sprayDistance;

	private Vector3 _currentTargetPosition;

	public void Initialise(float sprayRadius, float sprayDistance)
	{
		_sprayRadius = sprayRadius;
		_sprayDistance = sprayDistance;
	}

	protected override void Update()
	{
		base.Update();
		if (GameInput.GetButtonDown(GameInput.ButtonCode.Jump) || GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
		{
			Spray();
		}
	}

	private void Spray()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)"Spraying");
		if (onSpray != null)
		{
			onSpray.Invoke();
		}
		if (DoesHitTarget(_sprayOrigin.position, _sprayOrigin.forward, _currentTargetPosition, _sprayRadius, _sprayDistance))
		{
			Debug.Log((object)"Spray hit target");
			_onSuccessfulSpray?.Invoke();
		}
	}

	public void SetCurrentTarget(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_currentTargetPosition = position;
	}

	private bool DoesHitTarget(Vector3 rayOrigin, Vector3 rayDirection, Vector3 sphereCenter, float sphereRadius, float maxDistance)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = sphereCenter - rayOrigin;
		if (((Vector3)(ref val)).magnitude > maxDistance + sphereRadius)
		{
			return false;
		}
		float num = Vector3.Dot(val, rayDirection);
		return ((Vector3)(ref val)).sqrMagnitude - num * num <= sphereRadius * sphereRadius;
	}

	public void SubscribeToSuccessfulSpray(Action callback)
	{
		_onSuccessfulSpray = (Action)Delegate.Combine(_onSuccessfulSpray, callback);
	}

	public void UnsubscribeFromSuccessfulSpray(Action callback)
	{
		_onSuccessfulSpray = (Action)Delegate.Remove(_onSuccessfulSpray, callback);
	}

	private void OnDrawGizmos()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (_drawGizmos)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(_currentTargetPosition, _sprayRadius);
		}
	}
}
