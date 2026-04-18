using System;
using UnityEngine;

namespace LiquidVolumeFX;

public class CylinderManager : MonoBehaviour
{
	public float startingDelay = 1f;

	public int numCylinders = 16;

	public float scale = 0.2f;

	public float heightMultiplier = 2f;

	public float circleRadius = 1.75f;

	private void Update()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		if (!(Time.time < startingDelay))
		{
			for (int i = 0; i < numCylinders; i++)
			{
				GameObject val = Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/CylinderFlask"));
				((Object)val).hideFlags = (HideFlags)52;
				val.transform.SetParent(((Component)this).transform, false);
				val.transform.localScale = new Vector3(scale, scale * heightMultiplier, scale);
				float num = Mathf.Cos((float)i / (float)numCylinders * (float)Math.PI * 2f) * circleRadius;
				float num2 = Mathf.Sin((float)i / (float)numCylinders * (float)Math.PI * 2f) * circleRadius;
				val.transform.position = new Vector3(num, -2f, num2);
				FlaskAnimator flaskAnimator = val.AddComponent<FlaskAnimator>();
				flaskAnimator.initialPosition = val.transform.position;
				flaskAnimator.finalPosition = val.transform.position + Vector3.up;
				flaskAnimator.duration = 5f + (float)i * 0.5f;
				flaskAnimator.acceleration = 0.001f;
				flaskAnimator.delay = 4f;
				LiquidVolume component = val.GetComponent<LiquidVolume>();
				component.liquidColor1 = new Color(Random.value, Random.value, Random.value, Random.value);
				component.liquidColor2 = new Color(Random.value, Random.value, Random.value, Random.value);
				component.turbulence2 = 0f;
				component.refractionBlur = false;
			}
			Object.Destroy((Object)(object)this);
		}
	}
}
