using UnityEngine;

namespace LiquidVolumeFX;

public class RandomMove : MonoBehaviour
{
	[Range(-10f, 10f)]
	public float right = 2f;

	[Range(-10f, 10f)]
	public float left = -2f;

	[Range(-10f, 10f)]
	public float back = 2f;

	[Range(-10f, 10f)]
	public float front = -1f;

	[Range(0f, 0.2f)]
	public float speed = 0.5f;

	[Range(0f, 2f)]
	public float rotationSpeed = 1f;

	[Range(0.1f, 2f)]
	public float randomSpeed;

	public bool automatic;

	private Vector3 velocity = Vector3.zero;

	private int flaskType;

	private void Update()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown((KeyCode)102))
		{
			flaskType++;
			if (flaskType >= 3)
			{
				flaskType = 0;
			}
			((Component)((Component)this).transform.Find("SphereFlask")).gameObject.SetActive(flaskType == 0);
			((Component)((Component)this).transform.Find("CylinderFlask")).gameObject.SetActive(flaskType == 1);
			((Component)((Component)this).transform.Find("CubeFlask")).gameObject.SetActive(flaskType == 2);
		}
		Vector3 val = Vector3.zero;
		if (automatic)
		{
			if (Random.value > 0.99f)
			{
				val = Vector3.right * (speed + (Random.value - 0.5f) * randomSpeed);
			}
		}
		else
		{
			if (Input.GetKey((KeyCode)275))
			{
				val += Vector3.right * speed;
			}
			if (Input.GetKey((KeyCode)276))
			{
				val += Vector3.left * speed;
			}
			if (Input.GetKey((KeyCode)273))
			{
				val += Vector3.forward * speed;
			}
			if (Input.GetKey((KeyCode)274))
			{
				val += Vector3.back * speed;
			}
		}
		float num = 60f * Time.deltaTime;
		velocity += val * num;
		float num2 = 0.005f * num;
		if (((Vector3)(ref velocity)).magnitude > num2)
		{
			velocity -= ((Vector3)(ref velocity)).normalized * num2;
		}
		else
		{
			velocity = Vector3.zero;
		}
		Transform transform = ((Component)this).transform;
		transform.localPosition += velocity * num;
		if (Input.GetKey((KeyCode)119))
		{
			((Component)this).transform.Rotate(0f, 0f, rotationSpeed * num);
		}
		else if (Input.GetKey((KeyCode)115))
		{
			((Component)this).transform.Rotate(0f, 0f, (0f - rotationSpeed) * num);
		}
		if (((Component)this).transform.localPosition.x > right)
		{
			((Component)this).transform.localPosition = new Vector3(right, ((Component)this).transform.localPosition.y, ((Component)this).transform.localPosition.z);
			((Vector3)(ref velocity)).Set(0f, 0f, 0f);
		}
		if (((Component)this).transform.localPosition.x < left)
		{
			((Component)this).transform.localPosition = new Vector3(left, ((Component)this).transform.localPosition.y, ((Component)this).transform.localPosition.z);
			((Vector3)(ref velocity)).Set(0f, 0f, 0f);
		}
		if (((Component)this).transform.localPosition.z > back)
		{
			((Component)this).transform.localPosition = new Vector3(((Component)this).transform.localPosition.x, ((Component)this).transform.localPosition.y, back);
			((Vector3)(ref velocity)).Set(0f, 0f, 0f);
		}
		if (((Component)this).transform.localPosition.z < front)
		{
			((Component)this).transform.localPosition = new Vector3(((Component)this).transform.localPosition.x, ((Component)this).transform.localPosition.y, front);
			((Vector3)(ref velocity)).Set(0f, 0f, 0f);
		}
	}
}
