using UnityEngine;

namespace VLB;

[HelpURL("http://saladgamer.com/vlb-doc/comp-effect-from-profile/")]
public class EffectFromProfile : MonoBehaviour
{
	public const string ClassName = "EffectFromProfile";

	[SerializeField]
	private EffectAbstractBase m_EffectProfile;

	private EffectAbstractBase m_EffectInstance;

	public EffectAbstractBase effectProfile
	{
		get
		{
			return m_EffectProfile;
		}
		set
		{
			m_EffectProfile = value;
			InitInstanceFromProfile();
		}
	}

	public void InitInstanceFromProfile()
	{
		if (Object.op_Implicit((Object)(object)m_EffectInstance))
		{
			if (Object.op_Implicit((Object)(object)m_EffectProfile))
			{
				m_EffectInstance.InitFrom(m_EffectProfile);
			}
			else
			{
				((Behaviour)m_EffectInstance).enabled = false;
			}
		}
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)m_EffectInstance))
		{
			((Behaviour)m_EffectInstance).enabled = true;
		}
		else if (Object.op_Implicit((Object)(object)m_EffectProfile))
		{
			m_EffectInstance = ((Component)this).gameObject.AddComponent(((object)m_EffectProfile).GetType()) as EffectAbstractBase;
			InitInstanceFromProfile();
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)m_EffectInstance))
		{
			((Behaviour)m_EffectInstance).enabled = false;
		}
	}
}
