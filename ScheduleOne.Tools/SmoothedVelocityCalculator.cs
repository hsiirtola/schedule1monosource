using System.Collections;
using UnityEngine;

namespace ScheduleOne.Tools;

public class SmoothedVelocityCalculator : MonoBehaviour
{
	private const int sampleCount = 20;

	public bool DEBUG;

	[Header("Settings")]
	public float SampleLength = 0.2f;

	public float MaxReasonableVelocity = 25f;

	private RollingAverage<Vector3> velocityHistory = new RollingAverage<Vector3>(20, (Vector3 a, Vector3 b) => a + b, (Vector3 a, Vector3 b) => a - b, (Vector3 a, float c) => a / c);

	private Vector3 lastSamplePosition = Vector3.zero;

	private float timeOnLastSample;

	private float timeSinceLastSample;

	private bool zeroOut;

	private bool isTargetValid;

	public Transform Target { get; private set; }

	public virtual Vector3 Velocity
	{
		get
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (!isTargetValid)
			{
				return Vector3.zero;
			}
			if (zeroOut)
			{
				return Vector3.zero;
			}
			return velocityHistory.Average;
		}
	}

	private void Start()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Target == (Object)null)
		{
			Target = ((Component)this).transform;
			isTargetValid = true;
		}
		lastSamplePosition = Target.position;
	}

	protected virtual void FixedUpdate()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Target == (Object)null)
		{
			return;
		}
		timeSinceLastSample = Time.timeSinceLevelLoad - timeOnLastSample;
		if (!(timeSinceLastSample < SampleLength / 20f))
		{
			timeOnLastSample = Time.timeSinceLevelLoad;
			Vector3 value = (Target.position - lastSamplePosition) / timeSinceLastSample;
			if (((Vector3)(ref value)).sqrMagnitude <= MaxReasonableVelocity * MaxReasonableVelocity)
			{
				velocityHistory.Add(value);
			}
			lastSamplePosition = Target.position;
			if (DEBUG)
			{
				Vector3 velocity = Velocity;
				Debug.Log((object)$"Smoothed velocity: {((Vector3)(ref velocity)).magnitude}, This frame velocity: {((Vector3)(ref value)).magnitude}, History Count: {velocityHistory.Count}");
			}
		}
	}

	public void FlushBuffer()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		velocityHistory.Clear();
		if (Object.op_Implicit((Object)(object)Target))
		{
			lastSamplePosition = Target.position;
		}
	}

	public void ZeroOut(float duration)
	{
		zeroOut = true;
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return (object)new WaitForSeconds(duration);
			zeroOut = false;
		}
	}

	public void SetTarget(Transform target)
	{
		Target = target;
		isTargetValid = (Object)(object)Target == (Object)null;
		FlushBuffer();
	}
}
