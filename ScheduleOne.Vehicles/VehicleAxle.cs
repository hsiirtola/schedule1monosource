using UnityEngine;

namespace ScheduleOne.Vehicles;

public class VehicleAxle : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	protected Wheel wheel;

	private Transform model;

	protected virtual void Awake()
	{
		model = ((Component)this).transform.Find("Model");
	}

	protected virtual void LateUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		Vector3 position2 = wheel.axleConnectionPoint.position;
		((Component)model).transform.position = (position + position2) / 2f;
		((Component)this).transform.LookAt(position2);
		((Component)model).transform.localScale = new Vector3(((Component)model).transform.localScale.x, 0.5f * Vector3.Distance(position, position2), ((Component)model).transform.localScale.z);
	}
}
