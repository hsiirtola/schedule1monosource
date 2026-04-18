using UnityEngine;

namespace RadiantGI.Demos;

public class ShootGlowingBalls : MonoBehaviour
{
	public int count;

	public Transform center;

	public GameObject glowingBall;

	private void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < count; i++)
		{
			GameObject obj = Object.Instantiate<GameObject>(glowingBall, center.position + Vector3.right * (float)Random.Range(-4, 4) + Vector3.up * (5f + (float)i), Quaternion.identity);
			Color val = Random.ColorHSV();
			float value = Random.value;
			if (value < 0.33f)
			{
				val.r *= 0.2f;
			}
			else if (value < 0.66f)
			{
				val.g *= 0.2f;
			}
			else
			{
				val.b *= 0.2f;
			}
			Renderer component = obj.GetComponent<Renderer>();
			((Component)component).transform.localScale = Vector3.one * Random.Range(0.65f, 1f);
			component.material.color = val;
			component.material.SetColor("_EmissionColor", val * 2f);
		}
	}
}
