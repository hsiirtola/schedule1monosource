using UnityEngine;

namespace LiquidVolumeFX;

public class RandomRotation : MonoBehaviour
{
	[Range(1f, 50f)]
	public float speed = 10f;

	[Range(1f, 30f)]
	public float randomChangeInterval = 10f;

	private float lastTime;

	private Vector3 v;

	private float randomization;

	private void Start()
	{
		randomization = Random.value;
	}

	private void Update()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (Time.time > lastTime)
		{
			lastTime = Time.time + randomChangeInterval + randomization;
			v = new Vector3(Random.value, Random.value, Random.value);
		}
		((Component)this).transform.Rotate(v * Time.deltaTime * speed);
	}
}
