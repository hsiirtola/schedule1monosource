using UnityEngine;

namespace LiquidVolumeFX;

public class CameraAnimator : MonoBehaviour
{
	public float baseHeight = 0.6f;

	public float speedY = 0.005f;

	public float speedX = 5f;

	public float distAcceleration = 0.0002f;

	public float distSpeed = 0.0001f;

	public Vector3 lookAt;

	private float y;

	private float dy;

	private float distDirection = 1f;

	private float distSum;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		y = ((Component)this).transform.position.y;
	}

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.RotateAround(lookAt, Vector3.up, Time.deltaTime * speedX);
		y += dy;
		dy -= (((Component)this).transform.position.y - baseHeight) * Time.deltaTime * speedY;
		((Component)this).transform.position = new Vector3(((Component)this).transform.position.x, y, ((Component)this).transform.position.z);
		Quaternion rotation = ((Component)this).transform.rotation;
		((Component)this).transform.LookAt(lookAt);
		((Component)this).transform.rotation = Quaternion.Lerp(rotation, ((Component)this).transform.rotation, 0.2f);
		Transform transform = ((Component)this).transform;
		transform.position += ((Component)this).transform.forward * distSum;
		distSum += distSpeed;
		distDirection = ((distSum < 0f) ? 1f : (-1f));
		distSpeed += Time.deltaTime * distDirection * distAcceleration;
	}
}
