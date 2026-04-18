using UnityEngine;

namespace Beautify.Universal;

public class CameraAnimator : MonoBehaviour
{
	private void Update()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.Rotate(new Vector3(0f, 0f, Time.deltaTime * 10f));
	}
}
