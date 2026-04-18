using UnityEngine;

public class UnluckDistanceDisabler : MonoBehaviour
{
	public int _distanceDisable = 1000;

	public Transform _distanceFrom;

	public bool _distanceFromMainCam;

	public float _disableCheckInterval = 10f;

	public float _enableCheckInterval = 1f;

	public bool _disableOnStart;

	public void Start()
	{
		if (_distanceFromMainCam)
		{
			_distanceFrom = ((Component)Camera.main).transform;
		}
		((MonoBehaviour)this).InvokeRepeating("CheckDisable", _disableCheckInterval + Random.value * _disableCheckInterval, _disableCheckInterval);
		((MonoBehaviour)this).InvokeRepeating("CheckEnable", _enableCheckInterval + Random.value * _enableCheckInterval, _enableCheckInterval);
		((MonoBehaviour)this).Invoke("DisableOnStart", 0.01f);
	}

	public void DisableOnStart()
	{
		if (_disableOnStart)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public void CheckDisable()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).gameObject.activeInHierarchy)
		{
			Vector3 val = ((Component)this).transform.position - _distanceFrom.position;
			if (((Vector3)(ref val)).sqrMagnitude > (float)(_distanceDisable * _distanceDisable))
			{
				((Component)this).gameObject.SetActive(false);
			}
		}
	}

	public void CheckEnable()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!((Component)this).gameObject.activeInHierarchy)
		{
			Vector3 val = ((Component)this).transform.position - _distanceFrom.position;
			if (((Vector3)(ref val)).sqrMagnitude < (float)(_distanceDisable * _distanceDisable))
			{
				((Component)this).gameObject.SetActive(true);
			}
		}
	}
}
