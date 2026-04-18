using UnityEngine;

namespace VLB;

[DisallowMultipleComponent]
[RequireComponent(typeof(Light), typeof(VolumetricLightBeamHD))]
[HelpURL("http://saladgamer.com/vlb-doc/comp-trackrealtimechanges-hd/")]
public class TrackRealtimeChangesOnLightHD : MonoBehaviour
{
	public const string ClassName = "TrackRealtimeChangesOnLightHD";

	private VolumetricLightBeamHD m_Master;

	private void Awake()
	{
		m_Master = ((Component)this).GetComponent<VolumetricLightBeamHD>();
	}

	private void Update()
	{
		if (((Behaviour)m_Master).enabled)
		{
			m_Master.AssignPropertiesFromAttachedSpotLight();
		}
	}
}
