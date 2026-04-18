using UnityEngine;
using VLB;

namespace VLB_Samples;

[RequireComponent(typeof(Collider), typeof(Rigidbody), typeof(MeshRenderer))]
public class CheckIfInsideBeam : MonoBehaviour
{
	private bool isInsideBeam;

	private Material m_Material;

	private Collider m_Collider;

	private void Start()
	{
		m_Collider = ((Component)this).GetComponent<Collider>();
		MeshRenderer component = ((Component)this).GetComponent<MeshRenderer>();
		if (Object.op_Implicit((Object)(object)component))
		{
			m_Material = ((Renderer)component).material;
		}
	}

	private void Update()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)m_Material))
		{
			m_Material.SetColor("_Color", isInsideBeam ? Color.green : Color.red);
		}
	}

	private void FixedUpdate()
	{
		isInsideBeam = false;
	}

	private void OnTriggerStay(Collider trigger)
	{
		DynamicOcclusionRaycasting component = ((Component)trigger).GetComponent<DynamicOcclusionRaycasting>();
		if (Object.op_Implicit((Object)(object)component))
		{
			isInsideBeam = !component.IsColliderHiddenByDynamicOccluder(m_Collider);
		}
		else
		{
			isInsideBeam = true;
		}
	}
}
