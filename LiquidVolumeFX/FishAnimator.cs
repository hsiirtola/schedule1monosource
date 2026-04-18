using UnityEngine;

namespace LiquidVolumeFX;

public class FishAnimator : MonoBehaviour
{
	private void Update()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)Camera.main).transform.position;
		((Component)this).transform.LookAt(new Vector3(0f - position.x, ((Component)this).transform.position.y, 0f - position.z));
	}
}
