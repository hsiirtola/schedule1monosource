using UnityEngine;

namespace Funly.SkyStudio;

[ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{
	public Camera followCamera;

	public Vector3 offset = Vector3.zero;

	private void Update()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Camera val = ((!((Object)(object)followCamera != (Object)null)) ? Camera.main : followCamera);
		if (!((Object)(object)val == (Object)null))
		{
			((Component)this).transform.position = ((Component)val).transform.TransformPoint(offset);
		}
	}
}
