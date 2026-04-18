using UnityEngine;

namespace Funly.SkyStudio;

[ExecuteInEditMode]
public class OrbitingBody : MonoBehaviour
{
	private Transform m_PositionTransform;

	private RotateBody m_RotateBody;

	private SpherePoint m_SpherePoint = new SpherePoint(0f, 0f);

	private Vector3 m_CachedWorldDirection = Vector3.right;

	private Light m_BodyLight;

	public Transform positionTransform
	{
		get
		{
			if ((Object)(object)m_PositionTransform == (Object)null)
			{
				m_PositionTransform = ((Component)this).transform.Find("Position");
			}
			return m_PositionTransform;
		}
	}

	public RotateBody rotateBody
	{
		get
		{
			if ((Object)(object)m_RotateBody == (Object)null)
			{
				Transform val = positionTransform;
				if (!Object.op_Implicit((Object)(object)val))
				{
					Debug.LogError((object)"Can't return rotation body without a position transform game object");
					return null;
				}
				m_RotateBody = ((Component)val).GetComponent<RotateBody>();
			}
			return m_RotateBody;
		}
	}

	public SpherePoint Point
	{
		get
		{
			return m_SpherePoint;
		}
		set
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			if (m_SpherePoint == null)
			{
				m_SpherePoint = new SpherePoint(0f, 0f);
			}
			else
			{
				m_SpherePoint = value;
			}
			m_CachedWorldDirection = m_SpherePoint.GetWorldDirection();
			LayoutOribit();
		}
	}

	public Vector3 BodyGlobalDirection => m_CachedWorldDirection;

	public Light BodyLight
	{
		get
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)m_BodyLight == (Object)null)
			{
				m_BodyLight = ((Component)((Component)this).transform).GetComponentInChildren<Light>();
				if ((Object)(object)m_BodyLight != (Object)null)
				{
					((Component)m_BodyLight).transform.localRotation = Quaternion.identity;
				}
			}
			return m_BodyLight;
		}
	}

	public void ResetOrbit()
	{
		LayoutOribit();
		m_PositionTransform = null;
	}

	public void LayoutOribit()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = Vector3.zero;
		((Component)this).transform.rotation = Quaternion.identity;
		((Component)this).transform.forward = BodyGlobalDirection * -1f;
	}

	private void OnValidate()
	{
		LayoutOribit();
	}
}
