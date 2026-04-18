using UnityEngine;
using VLB;

namespace VLB_Samples;

[RequireComponent(typeof(Camera))]
public class CameraToggleBeamVisibility : MonoBehaviour
{
	[SerializeField]
	private KeyCode m_KeyCode = (KeyCode)32;

	private void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown(m_KeyCode))
		{
			Camera component = ((Component)this).GetComponent<Camera>();
			int geometryLayerID = Config.Instance.geometryLayerID;
			int num = 1 << geometryLayerID;
			if ((component.cullingMask & num) == num)
			{
				component.cullingMask &= ~num;
			}
			else
			{
				component.cullingMask |= num;
			}
		}
	}
}
