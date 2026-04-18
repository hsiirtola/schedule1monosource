using System.Collections;
using UnityEngine;

public class LandingSpot : MonoBehaviour
{
	[HideInInspector]
	public FlockChild landingChild;

	[HideInInspector]
	public bool landing;

	private int lerpCounter;

	[HideInInspector]
	public LandingSpotController _controller;

	private bool _idle;

	public Transform _thisT;

	public bool _gotcha;

	public void Start()
	{
		if ((Object)(object)_thisT == (Object)null)
		{
			_thisT = ((Component)this).transform;
		}
		if ((Object)(object)_controller == (Object)null)
		{
			_controller = ((Component)_thisT.parent).GetComponent<LandingSpotController>();
		}
		if (_controller._autoCatchDelay.x > 0f)
		{
			((MonoBehaviour)this).StartCoroutine(GetFlockChild(_controller._autoCatchDelay.x, _controller._autoCatchDelay.y));
		}
	}

	public void OnDrawGizmos()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_thisT == (Object)null)
		{
			_thisT = ((Component)this).transform;
		}
		if ((Object)(object)_controller == (Object)null)
		{
			_controller = ((Component)_thisT.parent).GetComponent<LandingSpotController>();
		}
		Gizmos.color = Color.yellow;
		if ((Object)(object)landingChild != (Object)null && landing)
		{
			Gizmos.DrawLine(_thisT.position, landingChild._thisT.position);
		}
		Quaternion rotation = _thisT.rotation;
		if (((Quaternion)(ref rotation)).eulerAngles.x == 0f)
		{
			rotation = _thisT.rotation;
			if (((Quaternion)(ref rotation)).eulerAngles.z == 0f)
			{
				goto IL_00e6;
			}
		}
		_thisT.eulerAngles = new Vector3(0f, _thisT.eulerAngles.y, 0f);
		goto IL_00e6;
		IL_00e6:
		Gizmos.DrawCube(new Vector3(_thisT.position.x, _thisT.position.y, _thisT.position.z), Vector3.one * _controller._gizmoSize);
		Gizmos.DrawCube(_thisT.position + _thisT.forward * _controller._gizmoSize, Vector3.one * _controller._gizmoSize * 0.5f);
		Gizmos.color = new Color(1f, 1f, 0f, 0.05f);
		Gizmos.DrawWireSphere(_thisT.position, _controller._maxBirdDistance);
	}

	public void LateUpdate()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)landingChild == (Object)null)
		{
			_gotcha = false;
			_idle = false;
			lerpCounter = 0;
			return;
		}
		if (_gotcha)
		{
			((Component)landingChild).transform.position = _thisT.position + landingChild._landingPosOffset;
			RotateBird();
			return;
		}
		if (((Component)_controller._flock).gameObject.activeInHierarchy && landing && (Object)(object)landingChild != (Object)null)
		{
			if (!((Component)landingChild).gameObject.activeInHierarchy)
			{
				((MonoBehaviour)this).Invoke("ReleaseFlockChild", 0f);
			}
			float num = Vector3.Distance(landingChild._thisT.position, _thisT.position + landingChild._landingPosOffset);
			if (num < 5f && num > 0.5f)
			{
				if (_controller._soarLand)
				{
					landingChild._model.GetComponent<Animation>().CrossFade(landingChild._spawner._soarAnimation, 0.5f);
					if (num < 2f)
					{
						landingChild._model.GetComponent<Animation>().CrossFade(landingChild._spawner._flapAnimation, 0.5f);
					}
				}
				landingChild._targetSpeed = landingChild._spawner._maxSpeed * _controller._landingSpeedModifier;
				landingChild._wayPoint = _thisT.position + landingChild._landingPosOffset;
				landingChild._damping = _controller._landingTurnSpeedModifier;
				landingChild._avoid = false;
			}
			else if (num <= 0.5f)
			{
				landingChild._wayPoint = _thisT.position + landingChild._landingPosOffset;
				if (num < _controller._snapLandDistance && !_idle)
				{
					_idle = true;
					landingChild._model.GetComponent<Animation>().CrossFade(landingChild._spawner._idleAnimation, 0.55f);
				}
				if (num > _controller._snapLandDistance)
				{
					landingChild._targetSpeed = landingChild._spawner._minSpeed * _controller._landingSpeedModifier;
					Transform thisT = landingChild._thisT;
					thisT.position += (_thisT.position + landingChild._landingPosOffset - landingChild._thisT.position) * Time.deltaTime * landingChild._speed * _controller._landingSpeedModifier * 2f;
				}
				else
				{
					_gotcha = true;
				}
				landingChild._move = false;
				RotateBird();
			}
			else
			{
				landingChild._wayPoint = _thisT.position + landingChild._landingPosOffset;
			}
			landingChild._damping += 0.01f;
		}
		StraightenBird();
	}

	public void StraightenBird()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (landingChild._thisT.eulerAngles.x != 0f)
		{
			Vector3 eulerAngles = landingChild._thisT.eulerAngles;
			eulerAngles.z = 0f;
			landingChild._thisT.eulerAngles = eulerAngles;
		}
	}

	public void RotateBird()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		if (!_controller._randomRotate || !_idle)
		{
			lerpCounter++;
			Quaternion rotation = landingChild._thisT.rotation;
			Vector3 eulerAngles = ((Quaternion)(ref rotation)).eulerAngles;
			Quaternion rotation2 = landingChild._thisT.rotation;
			float y = ((Quaternion)(ref rotation2)).eulerAngles.y;
			rotation2 = _thisT.rotation;
			eulerAngles.y = Mathf.LerpAngle(y, ((Quaternion)(ref rotation2)).eulerAngles.y, (float)lerpCounter * Time.deltaTime * _controller._landedRotateSpeed);
			((Quaternion)(ref rotation)).eulerAngles = eulerAngles;
			landingChild._thisT.rotation = rotation;
		}
	}

	public IEnumerator GetFlockChild(float minDelay, float maxDelay)
	{
		yield return (object)new WaitForSeconds(Random.Range(minDelay, maxDelay));
		if (!((Component)_controller._flock).gameObject.activeInHierarchy || !((Object)(object)landingChild == (Object)null))
		{
			yield break;
		}
		FlockChild flockChild = null;
		for (int i = 0; i < _controller._flock._roamers.Count; i++)
		{
			FlockChild flockChild2 = _controller._flock._roamers[i];
			if (flockChild2._landing || flockChild2._dived)
			{
				continue;
			}
			if (!_controller._onlyBirdsAbove)
			{
				if ((Object)(object)flockChild == (Object)null && _controller._maxBirdDistance > Vector3.Distance(flockChild2._thisT.position, _thisT.position) && _controller._minBirdDistance < Vector3.Distance(flockChild2._thisT.position, _thisT.position))
				{
					flockChild = flockChild2;
					if (!_controller._takeClosest)
					{
						break;
					}
				}
				else if ((Object)(object)flockChild != (Object)null && Vector3.Distance(flockChild._thisT.position, _thisT.position) > Vector3.Distance(flockChild2._thisT.position, _thisT.position))
				{
					flockChild = flockChild2;
				}
			}
			else if ((Object)(object)flockChild == (Object)null && flockChild2._thisT.position.y > _thisT.position.y && _controller._maxBirdDistance > Vector3.Distance(flockChild2._thisT.position, _thisT.position) && _controller._minBirdDistance < Vector3.Distance(flockChild2._thisT.position, _thisT.position))
			{
				flockChild = flockChild2;
				if (!_controller._takeClosest)
				{
					break;
				}
			}
			else if ((Object)(object)flockChild != (Object)null && flockChild2._thisT.position.y > _thisT.position.y && Vector3.Distance(flockChild._thisT.position, _thisT.position) > Vector3.Distance(flockChild2._thisT.position, _thisT.position))
			{
				flockChild = flockChild2;
			}
		}
		if ((Object)(object)flockChild != (Object)null)
		{
			landingChild = flockChild;
			landing = true;
			landingChild._landing = true;
			if (_controller._autoDismountDelay.x > 0f)
			{
				((MonoBehaviour)this).Invoke("ReleaseFlockChild", Random.Range(_controller._autoDismountDelay.x, _controller._autoDismountDelay.y));
			}
			_controller._activeLandingSpots++;
		}
		else if (_controller._autoCatchDelay.x > 0f)
		{
			((MonoBehaviour)this).StartCoroutine(GetFlockChild(_controller._autoCatchDelay.x, _controller._autoCatchDelay.y));
		}
	}

	public void InstantLand()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Component)_controller._flock).gameObject.activeInHierarchy || !((Object)(object)landingChild == (Object)null))
		{
			return;
		}
		FlockChild flockChild = null;
		for (int i = 0; i < _controller._flock._roamers.Count; i++)
		{
			FlockChild flockChild2 = _controller._flock._roamers[i];
			if (!flockChild2._landing && !flockChild2._dived)
			{
				flockChild = flockChild2;
			}
		}
		if ((Object)(object)flockChild != (Object)null)
		{
			landingChild = flockChild;
			landing = true;
			_controller._activeLandingSpots++;
			landingChild._landing = true;
			landingChild._thisT.position = _thisT.position + landingChild._landingPosOffset;
			landingChild._model.GetComponent<Animation>().Play(landingChild._spawner._idleAnimation);
			landingChild._thisT.Rotate(Vector3.up, Random.Range(0f, 360f));
			if (_controller._autoDismountDelay.x > 0f)
			{
				((MonoBehaviour)this).Invoke("ReleaseFlockChild", Random.Range(_controller._autoDismountDelay.x, _controller._autoDismountDelay.y));
			}
		}
		else if (_controller._autoCatchDelay.x > 0f)
		{
			((MonoBehaviour)this).StartCoroutine(GetFlockChild(_controller._autoCatchDelay.x, _controller._autoCatchDelay.y));
		}
	}

	public void ReleaseFlockChild()
	{
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)_controller._flock).gameObject.activeInHierarchy && (Object)(object)landingChild != (Object)null)
		{
			_gotcha = false;
			lerpCounter = 0;
			if ((Object)(object)_controller._featherPS != (Object)null)
			{
				_controller._featherPS.position = landingChild._thisT.position;
				((Component)_controller._featherPS).GetComponent<ParticleSystem>().Emit(Random.Range(0, 3));
			}
			landing = false;
			_idle = false;
			landingChild._avoid = true;
			landingChild._damping = landingChild._spawner._maxDamping;
			landingChild._model.GetComponent<Animation>().CrossFade(landingChild._spawner._flapAnimation, 0.2f);
			landingChild._dived = true;
			landingChild._speed = 0f;
			landingChild._move = true;
			landingChild._landing = false;
			landingChild.Flap();
			landingChild._wayPoint = new Vector3(landingChild._wayPoint.x, _thisT.position.y + 10f, landingChild._wayPoint.z);
			if (_controller._autoCatchDelay.x > 0f)
			{
				((MonoBehaviour)this).StartCoroutine(GetFlockChild(_controller._autoCatchDelay.x + 0.1f, _controller._autoCatchDelay.y + 0.1f));
			}
			landingChild = null;
			_controller._activeLandingSpots--;
		}
	}
}
