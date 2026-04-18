using UnityEngine;

namespace Beautify.Universal;

public class SphereAnimator : MonoBehaviour
{
	private Rigidbody rb;

	private void Start()
	{
		rb = ((Component)this).GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).transform.position.z < 2.5f)
		{
			rb.AddForce(Vector3.forward * 200f * Time.fixedDeltaTime);
		}
		else if (((Component)this).transform.position.z > 8f)
		{
			rb.AddForce(Vector3.back * 200f * Time.fixedDeltaTime);
		}
	}
}
