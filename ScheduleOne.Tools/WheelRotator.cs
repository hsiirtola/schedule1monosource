using System;
using ScheduleOne.Audio;
using UnityEngine;

namespace ScheduleOne.Tools;

[ExecuteInEditMode]
public class WheelRotator : MonoBehaviour
{
	public float Radius = 0.5f;

	public Transform Wheel;

	public bool Flip;

	public AudioSourceController Controller;

	public float AudioVolumeDivisor = 90f;

	public Vector3 RotationAxis = Vector3.up;

	[SerializeField]
	private Vector3 lastFramePosition = Vector3.zero;

	private void Start()
	{
		if ((Object)(object)Controller != (Object)null)
		{
			Controller.SetTime(Random.Range(0f, Controller.Clip.length));
		}
	}

	private void LateUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		float num = Vector3.Distance(position, lastFramePosition);
		if (num > 0f)
		{
			float num2 = num / ((float)System.Math.PI * 2f * Radius) * 360f;
			Wheel.Rotate(RotationAxis, num2 * (Flip ? (-1f) : 1f));
			float num3 = num2 / Time.deltaTime;
			if ((Object)(object)Controller != (Object)null)
			{
				Controller.VolumeMultiplier = num3 / AudioVolumeDivisor;
			}
		}
		lastFramePosition = position;
	}
}
