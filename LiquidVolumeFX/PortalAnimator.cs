using UnityEngine;

namespace LiquidVolumeFX;

public class PortalAnimator : MonoBehaviour
{
	public float delay = 2f;

	public float duration = 1f;

	public float delayFadeOut = 4f;

	private Vector3 scale;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		scale = ((Component)this).transform.localScale;
		((Component)this).transform.localScale = Vector3.zero;
	}

	private void Update()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (!(Time.time < delay))
		{
			float num = ((!(Time.time > delayFadeOut)) ? ((Time.time - delay) / duration) : (1f - (Time.time - delayFadeOut) / duration));
			((Component)this).transform.localScale = Mathf.Clamp01(num) * scale;
		}
	}
}
