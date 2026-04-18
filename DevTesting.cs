using System;
using ScheduleOne.UI;
using UnityEngine;

public class DevTesting : MonoBehaviour
{
	[Header("Spread Testing")]
	[SerializeField]
	private float _spreadAngle = 30f;

	[SerializeField]
	private ReticleUI _reticleUI;

	[SerializeField]
	private GameObject _sphere;

	[SerializeField]
	private float _testDistance = 10f;

	private void Update()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		float num = _spreadAngle * ((float)Math.PI / 180f);
		Vector3 position = ((Component)Camera.main).transform.position + ((Component)Camera.main).transform.forward * _testDistance;
		_sphere.transform.position = position;
		float num2 = Mathf.Tan(num) * _testDistance;
		_sphere.transform.localScale = Vector3.one * num2;
		_reticleUI.Set(_spreadAngle);
	}
}
