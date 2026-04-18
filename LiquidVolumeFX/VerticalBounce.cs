using UnityEngine;

namespace LiquidVolumeFX;

public class VerticalBounce : MonoBehaviour
{
	[Range(0f, 0.1f)]
	public float acceleration = 0.1f;

	private float direction = 1f;

	private float y;

	private float speed = 0.01f;

	private void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = new Vector3(((Component)this).transform.localPosition.x, y, ((Component)this).transform.localPosition.z);
		y += speed;
		direction = ((y < 0f) ? 1f : (-1f));
		speed += Time.deltaTime * direction * acceleration;
	}
}
