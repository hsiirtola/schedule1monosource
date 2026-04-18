using System;
using UnityEngine;

namespace ScheduleOne.Temperature;

public class TemperatureEmitter : MonoBehaviour
{
	public const int DefaultAmbientTemperature = 20;

	public const int MinTemperature = 0;

	public const int MaxTemperature = 40;

	public Action OnEmitterChanged;

	[field: SerializeField]
	public float Temperature { get; private set; } = 20f;

	[field: SerializeField]
	public float Range { get; private set; } = 5f;

	public Vector3 EmissionPoint => ((Component)this).transform.position;

	public void SetPosition(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = position;
		NotifyChanged();
	}

	public void SetTemperature(float temperature)
	{
		Temperature = Mathf.Clamp(temperature, 0f, 40f);
		NotifyChanged();
	}

	public void SetRange(float range)
	{
		Range = Mathf.Clamp(range, 0.1f, 100f);
		NotifyChanged();
	}

	public void NotifyChanged()
	{
		if (OnEmitterChanged != null)
		{
			OnEmitterChanged();
		}
	}
}
