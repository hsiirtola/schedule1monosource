using UnityEngine;

namespace Funly.SkyStudio;

public class RotateBody : MonoBehaviour
{
	private float m_SpinSpeed;

	private bool m_AllowSpinning;

	public float SpinSpeed
	{
		get
		{
			return m_SpinSpeed;
		}
		set
		{
			m_SpinSpeed = value;
			UpdateOrbitBodyRotation();
		}
	}

	public bool AllowSpinning
	{
		get
		{
			return m_AllowSpinning;
		}
		set
		{
			m_AllowSpinning = value;
			UpdateOrbitBodyRotation();
		}
	}

	public void UpdateOrbitBodyRotation()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		float num = (m_AllowSpinning ? 1 : 0);
		Quaternion localRotation = ((Component)this).transform.localRotation;
		Vector3 eulerAngles = ((Quaternion)(ref localRotation)).eulerAngles;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(0f, -180f, (eulerAngles.z + -10f * SpinSpeed * Time.deltaTime) * num);
		((Component)this).transform.localRotation = Quaternion.Euler(val);
	}
}
