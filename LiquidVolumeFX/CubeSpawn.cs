using System;
using UnityEngine;

namespace LiquidVolumeFX;

public class CubeSpawn : MonoBehaviour
{
	public int instances = 150;

	public float radius = 2f;

	public float jitter = 0.5f;

	public float expansion = 0.04f;

	public float laps = 2f;

	private void Start()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i <= instances; i++)
		{
			GameObject obj = Object.Instantiate<GameObject>(((Component)this).gameObject);
			((Behaviour)obj.GetComponent<CubeSpawn>()).enabled = false;
			((Object)obj).name = "Cube" + i;
			float num = (float)i / (float)instances * (float)Math.PI * 2f * laps;
			float num2 = (float)i * expansion;
			float num3 = Mathf.Cos(num) * (radius + num2);
			float num4 = Mathf.Sin(num) * (radius + num2);
			Vector3 val = Random.insideUnitSphere * jitter;
			obj.transform.position = ((Component)this).transform.position + new Vector3(num3, 0f, num4) + val;
			Transform transform = obj.transform;
			transform.localScale *= 1f - Random.value * jitter;
		}
	}
}
