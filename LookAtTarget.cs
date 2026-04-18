using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
	[SerializeField]
	private Transform _target;

	[SerializeField]
	private float _speed = 0.5f;

	private Vector3 _lookAtTarget;

	private void Update()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_lookAtTarget = Vector3.Lerp(_lookAtTarget, _target.position, Time.deltaTime * _speed);
		((Component)this).transform.LookAt(_lookAtTarget);
	}
}
