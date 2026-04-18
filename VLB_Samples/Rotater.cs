using UnityEngine;
using UnityEngine.Serialization;

namespace VLB_Samples;

public class Rotater : MonoBehaviour
{
	[FormerlySerializedAs("m_EulerSpeed")]
	public Vector3 EulerSpeed = Vector3.zero;

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Quaternion rotation = ((Component)this).transform.rotation;
		Vector3 eulerAngles = ((Quaternion)(ref rotation)).eulerAngles;
		eulerAngles += EulerSpeed * Time.deltaTime;
		((Component)this).transform.rotation = Quaternion.Euler(eulerAngles);
	}
}
