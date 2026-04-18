using UnityEngine;

public class FlockChild : MonoBehaviour
{
	[HideInInspector]
	public FlockController _spawner;

	[HideInInspector]
	public Vector3 _wayPoint;

	public float _speed;

	[HideInInspector]
	public bool _dived = true;

	[HideInInspector]
	public float _stuckCounter;

	[HideInInspector]
	public float _damping;

	[HideInInspector]
	public bool _soar = true;

	[HideInInspector]
	public bool _landing;

	[HideInInspector]
	public float _targetSpeed;

	[HideInInspector]
	public bool _move = true;

	public GameObject _model;

	public Transform _modelT;

	[HideInInspector]
	public float _avoidValue;

	[HideInInspector]
	public float _avoidDistance;

	private float _soarTimer;

	private bool _instantiated;

	private static int _updateNextSeed;

	private int _updateSeed = -1;

	[HideInInspector]
	public bool _avoid = true;

	public Transform _thisT;

	public Vector3 _landingPosOffset;

	public void Start()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		FindRequiredComponents();
		Wander(0f);
		SetRandomScale();
		_thisT.position = findWaypoint();
		RandomizeStartAnimationFrame();
		InitAvoidanceValues();
		_speed = _spawner._minSpeed;
		_spawner._activeChildren += 1f;
		_instantiated = true;
		if (_spawner._updateDivisor > 1)
		{
			int num = _spawner._updateDivisor - 1;
			_updateNextSeed++;
			_updateSeed = _updateNextSeed;
			_updateNextSeed %= num;
		}
	}

	public void Update()
	{
		if (_spawner._updateDivisor <= 1 || _spawner._updateCounter == _updateSeed)
		{
			SoarTimeLimit();
			CheckForDistanceToWaypoint();
			RotationBasedOnWaypointOrAvoidance();
			LimitRotationOfModel();
		}
	}

	public void OnDisable()
	{
		((MonoBehaviour)this).CancelInvoke();
		_spawner._activeChildren -= 1f;
	}

	public void OnEnable()
	{
		if (_instantiated)
		{
			_spawner._activeChildren += 1f;
			if (_landing)
			{
				_model.GetComponent<Animation>().Play(_spawner._idleAnimation);
			}
			else
			{
				_model.GetComponent<Animation>().Play(_spawner._flapAnimation);
			}
		}
	}

	public void FindRequiredComponents()
	{
		if ((Object)(object)_thisT == (Object)null)
		{
			_thisT = ((Component)this).transform;
		}
		if ((Object)(object)_model == (Object)null)
		{
			_model = ((Component)_thisT.Find("Model")).gameObject;
		}
		if ((Object)(object)_modelT == (Object)null)
		{
			_modelT = _model.transform;
		}
	}

	public void RandomizeStartAnimationFrame()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		foreach (AnimationState item in _model.GetComponent<Animation>())
		{
			AnimationState val = item;
			val.time = Random.value * val.length;
		}
	}

	public void InitAvoidanceValues()
	{
		_avoidValue = Random.Range(0.3f, 0.1f);
		if (_spawner._birdAvoidDistanceMax != _spawner._birdAvoidDistanceMin)
		{
			_avoidDistance = Random.Range(_spawner._birdAvoidDistanceMax, _spawner._birdAvoidDistanceMin);
		}
		else
		{
			_avoidDistance = _spawner._birdAvoidDistanceMin;
		}
	}

	public void SetRandomScale()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(_spawner._minScale, _spawner._maxScale);
		_thisT.localScale = new Vector3(num, num, num);
	}

	public void SoarTimeLimit()
	{
		if (_soar && _spawner._soarMaxTime > 0f)
		{
			if (_soarTimer > _spawner._soarMaxTime)
			{
				Flap();
				_soarTimer = 0f;
			}
			else
			{
				_soarTimer += _spawner._newDelta;
			}
		}
	}

	public void CheckForDistanceToWaypoint()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (!_landing)
		{
			Vector3 val = _thisT.position - _wayPoint;
			if (((Vector3)(ref val)).magnitude < _spawner._waypointDistance + _stuckCounter)
			{
				Wander(0f);
				_stuckCounter = 0f;
				return;
			}
		}
		if (!_landing)
		{
			_stuckCounter += _spawner._newDelta;
		}
		else
		{
			_stuckCounter = 0f;
		}
	}

	public void RotationBasedOnWaypointOrAvoidance()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = _wayPoint - _thisT.position;
		if (_targetSpeed > -1f && val != Vector3.zero)
		{
			Quaternion val2 = Quaternion.LookRotation(val);
			_thisT.rotation = Quaternion.Slerp(_thisT.rotation, val2, _spawner._newDelta * _damping);
		}
		if (_spawner._childTriggerPos)
		{
			Vector3 val3 = _thisT.position - _spawner._posBuffer;
			if (((Vector3)(ref val3)).magnitude < 1f)
			{
				_spawner.SetFlockRandomPosition();
			}
		}
		_speed = Mathf.Lerp(_speed, _targetSpeed, _spawner._newDelta * 2.5f);
		if (_move)
		{
			Transform thisT = _thisT;
			thisT.position += _thisT.forward * _speed * _spawner._newDelta;
			if (_avoid && _spawner._birdAvoid)
			{
				Avoidance();
			}
		}
	}

	public bool Avoidance()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		Vector3 forward = _modelT.forward;
		bool result = false;
		Quaternion rotation = Quaternion.identity;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		zero2 = _thisT.position;
		rotation = _thisT.rotation;
		Quaternion rotation2 = _thisT.rotation;
		zero = ((Quaternion)(ref rotation2)).eulerAngles;
		if (Physics.Raycast(_thisT.position, forward + _modelT.right * _avoidValue, ref val, _avoidDistance, LayerMask.op_Implicit(_spawner._avoidanceMask)))
		{
			zero.y -= (float)_spawner._birdAvoidHorizontalForce * _spawner._newDelta * _damping;
			((Quaternion)(ref rotation)).eulerAngles = zero;
			_thisT.rotation = rotation;
			result = true;
		}
		else if (Physics.Raycast(_thisT.position, forward + _modelT.right * (0f - _avoidValue), ref val, _avoidDistance, LayerMask.op_Implicit(_spawner._avoidanceMask)))
		{
			zero.y += (float)_spawner._birdAvoidHorizontalForce * _spawner._newDelta * _damping;
			((Quaternion)(ref rotation)).eulerAngles = zero;
			_thisT.rotation = rotation;
			result = true;
		}
		if (_spawner._birdAvoidDown && !_landing && Physics.Raycast(_thisT.position, -Vector3.up, ref val, _avoidDistance, LayerMask.op_Implicit(_spawner._avoidanceMask)))
		{
			zero.x -= (float)_spawner._birdAvoidVerticalForce * _spawner._newDelta * _damping;
			((Quaternion)(ref rotation)).eulerAngles = zero;
			_thisT.rotation = rotation;
			zero2.y += (float)_spawner._birdAvoidVerticalForce * _spawner._newDelta * 0.01f;
			_thisT.position = zero2;
			result = true;
		}
		else if (_spawner._birdAvoidUp && !_landing && Physics.Raycast(_thisT.position, Vector3.up, ref val, _avoidDistance, LayerMask.op_Implicit(_spawner._avoidanceMask)))
		{
			zero.x += (float)_spawner._birdAvoidVerticalForce * _spawner._newDelta * _damping;
			((Quaternion)(ref rotation)).eulerAngles = zero;
			_thisT.rotation = rotation;
			zero2.y -= (float)_spawner._birdAvoidVerticalForce * _spawner._newDelta * 0.01f;
			_thisT.position = zero2;
			result = true;
		}
		return result;
	}

	public void LimitRotationOfModel()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		Quaternion localRotation = Quaternion.identity;
		Vector3 zero = Vector3.zero;
		localRotation = _modelT.localRotation;
		zero = ((Quaternion)(ref localRotation)).eulerAngles;
		if ((((_soar && _spawner._flatSoar) || (_spawner._flatFly && !_soar)) && _wayPoint.y > _thisT.position.y) || _landing)
		{
			zero.x = Mathf.LerpAngle(_modelT.localEulerAngles.x, 0f - _thisT.localEulerAngles.x, _spawner._newDelta * 1.75f);
			((Quaternion)(ref localRotation)).eulerAngles = zero;
			_modelT.localRotation = localRotation;
		}
		else
		{
			zero.x = Mathf.LerpAngle(_modelT.localEulerAngles.x, 0f, _spawner._newDelta * 1.75f);
			((Quaternion)(ref localRotation)).eulerAngles = zero;
			_modelT.localRotation = localRotation;
		}
	}

	public void Wander(float delay)
	{
		if (!_landing)
		{
			_damping = Random.Range(_spawner._minDamping, _spawner._maxDamping);
			_targetSpeed = Random.Range(_spawner._minSpeed, _spawner._maxSpeed);
			((MonoBehaviour)this).Invoke("SetRandomMode", delay);
		}
	}

	public void SetRandomMode()
	{
		((MonoBehaviour)this).CancelInvoke("SetRandomMode");
		if (!_dived && Random.value < _spawner._soarFrequency)
		{
			Soar();
		}
		else if (!_dived && Random.value < _spawner._diveFrequency)
		{
			Dive();
		}
		else
		{
			Flap();
		}
	}

	public void Flap()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (_move)
		{
			if ((Object)(object)_model != (Object)null)
			{
				_model.GetComponent<Animation>().CrossFade(_spawner._flapAnimation, 0.5f);
			}
			_soar = false;
			animationSpeed();
			_wayPoint = findWaypoint();
			_dived = false;
		}
	}

	public Vector3 findWaypoint()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zero = Vector3.zero;
		zero.x = Random.Range(0f - _spawner._spawnSphere, _spawner._spawnSphere) + _spawner._posBuffer.x;
		zero.z = Random.Range(0f - _spawner._spawnSphereDepth, _spawner._spawnSphereDepth) + _spawner._posBuffer.z;
		zero.y = Random.Range(0f - _spawner._spawnSphereHeight, _spawner._spawnSphereHeight) + _spawner._posBuffer.y;
		return zero;
	}

	public void Soar()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (_move)
		{
			_model.GetComponent<Animation>().CrossFade(_spawner._soarAnimation, 1.5f);
			_wayPoint = findWaypoint();
			_soar = true;
		}
	}

	public void Dive()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (_spawner._soarAnimation != null)
		{
			_model.GetComponent<Animation>().CrossFade(_spawner._soarAnimation, 1.5f);
		}
		else
		{
			foreach (AnimationState item in _model.GetComponent<Animation>())
			{
				AnimationState val = item;
				if (_thisT.position.y < _wayPoint.y + 25f)
				{
					val.speed = 0.1f;
				}
			}
		}
		_wayPoint = findWaypoint();
		_wayPoint.y -= _spawner._diveValue;
		_dived = true;
	}

	public void animationSpeed()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		foreach (AnimationState item in _model.GetComponent<Animation>())
		{
			AnimationState val = item;
			if (!_dived && !_landing)
			{
				val.speed = Random.Range(_spawner._minAnimationSpeed, _spawner._maxAnimationSpeed);
			}
			else
			{
				val.speed = _spawner._maxAnimationSpeed;
			}
		}
	}
}
