using UnityEngine;

namespace ScheduleOne.DevUtilities;

public class AverageAcceleration : MonoBehaviour
{
	public Rigidbody Rb;

	public float TimeWindow = 0.5f;

	private Vector3[] accelerations;

	private int currentIndex;

	private float timer;

	private Vector3 prevVelocity;

	public Vector3 Acceleration { get; private set; } = Vector3.zero;

	private void Start()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Rb == (Object)null)
		{
			Rb = ((Component)this).GetComponent<Rigidbody>();
		}
		accelerations = (Vector3[])(object)new Vector3[Mathf.CeilToInt(TimeWindow / Time.fixedDeltaTime)];
		for (int i = 0; i < accelerations.Length; i++)
		{
			accelerations[i] = Vector3.zero;
		}
		prevVelocity = Rb.velocity;
	}

	private void FixedUpdate()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		timer += Time.fixedDeltaTime;
		if (timer >= TimeWindow)
		{
			timer -= Time.fixedDeltaTime;
			accelerations[currentIndex] = Vector3.zero;
			currentIndex = (currentIndex + 1) % accelerations.Length;
		}
		Vector3 val = (Rb.velocity - prevVelocity) / Time.fixedDeltaTime;
		accelerations[currentIndex] = val;
		prevVelocity = Rb.velocity;
		Vector3 val2 = Vector3.zero;
		for (int i = 0; i < accelerations.Length; i++)
		{
			val2 += accelerations[i];
		}
		Acceleration = val2 / (float)accelerations.Length;
	}
}
