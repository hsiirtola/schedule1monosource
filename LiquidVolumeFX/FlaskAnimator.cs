using UnityEngine;

namespace LiquidVolumeFX;

public class FlaskAnimator : MonoBehaviour
{
	public float speed = 0.01f;

	public Vector3 initialPosition = Vector3.down * 4f;

	public Vector3 finalPosition = Vector3.zero;

	public float duration = 5f;

	public float delay = 6f;

	[Range(0f, 1f)]
	public float level;

	[Range(0f, 1f)]
	public float minRange = 0.05f;

	[Range(0f, 1f)]
	public float maxRange = 0.95f;

	[Range(0f, 1f)]
	public float acceleration = 0.04f;

	[Range(0f, 1f)]
	public float rotationSpeed = 0.25f;

	[Range(0f, 2f)]
	public float alphaDuration = 2f;

	[Range(0f, 1f)]
	public float finalRefractionBlur = 0.75f;

	private LiquidVolume liquid;

	private float direction = 1f;

	private void Awake()
	{
		liquid = ((Component)this).GetComponent<LiquidVolume>();
		level = liquid.level;
		liquid.alpha = 0f;
	}

	private void Update()
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		float num = ((duration > 0f) ? ((Time.time - delay) / duration) : 1f);
		if (num >= 1f)
		{
			level += direction * speed;
			if (level < minRange || level > maxRange)
			{
				direction *= -1f;
			}
			direction += Mathf.Sign(0.5f - level) * acceleration;
			level = Mathf.Clamp(level, minRange, maxRange);
			liquid.level = level;
			num = ((alphaDuration > 0f) ? Mathf.Clamp01((Time.time - duration - delay) / alphaDuration) : 1f);
			liquid.alpha = num;
			liquid.blurIntensity = num * finalRefractionBlur;
		}
		else if (initialPosition != finalPosition)
		{
			((Component)this).transform.position = Vector3.Lerp(initialPosition, finalPosition, num);
		}
		((Component)this).transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed * 57.29578f, (Space)1);
	}
}
