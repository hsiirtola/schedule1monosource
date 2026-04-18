using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLB;

[AddComponentMenu("")]
public class EffectAbstractBase : MonoBehaviour
{
	[Flags]
	public enum ComponentsToChange
	{
		UnityLight = 1,
		VolumetricLightBeam = 2,
		VolumetricDustParticles = 4
	}

	public const string ClassName = "EffectAbstractBase";

	public ComponentsToChange componentsToChange = (ComponentsToChange)2147483647;

	[FormerlySerializedAs("restoreBaseIntensity")]
	public bool restoreIntensityOnDisable = true;

	protected VolumetricLightBeamAbstractBase m_Beam;

	protected Light m_Light;

	protected VolumetricDustParticles m_Particles;

	protected float m_BaseIntensityBeamInside;

	protected float m_BaseIntensityBeamOutside;

	protected float m_BaseIntensityLight;

	[Obsolete("Use 'restoreIntensityOnDisable' instead")]
	public bool restoreBaseIntensity
	{
		get
		{
			return restoreIntensityOnDisable;
		}
		set
		{
			restoreIntensityOnDisable = value;
		}
	}

	public virtual void InitFrom(EffectAbstractBase Source)
	{
		if (Object.op_Implicit((Object)(object)Source))
		{
			componentsToChange = Source.componentsToChange;
			restoreIntensityOnDisable = Source.restoreIntensityOnDisable;
		}
	}

	private void GetIntensity(VolumetricLightBeamSD beam)
	{
		if (Object.op_Implicit((Object)(object)beam))
		{
			m_BaseIntensityBeamInside = beam.intensityInside;
			m_BaseIntensityBeamOutside = beam.intensityOutside;
		}
	}

	private void GetIntensity(VolumetricLightBeamHD beam)
	{
		if (Object.op_Implicit((Object)(object)beam))
		{
			m_BaseIntensityBeamOutside = beam.intensity;
		}
	}

	private void SetIntensity(VolumetricLightBeamSD beam, float additive)
	{
		if (Object.op_Implicit((Object)(object)beam))
		{
			beam.intensityInside = Mathf.Max(0f, m_BaseIntensityBeamInside + additive);
			beam.intensityOutside = Mathf.Max(0f, m_BaseIntensityBeamOutside + additive);
		}
	}

	private void SetIntensity(VolumetricLightBeamHD beam, float additive)
	{
		if (Object.op_Implicit((Object)(object)beam))
		{
			beam.intensity = Mathf.Max(0f, m_BaseIntensityBeamOutside + additive);
		}
	}

	protected void SetAdditiveIntensity(float additive)
	{
		if (componentsToChange.HasFlag(ComponentsToChange.VolumetricLightBeam) && Object.op_Implicit((Object)(object)m_Beam))
		{
			SetIntensity(m_Beam as VolumetricLightBeamSD, additive);
			SetIntensity(m_Beam as VolumetricLightBeamHD, additive);
		}
		if (componentsToChange.HasFlag(ComponentsToChange.UnityLight) && Object.op_Implicit((Object)(object)m_Light))
		{
			m_Light.intensity = Mathf.Max(0f, m_BaseIntensityLight + additive);
		}
		if (componentsToChange.HasFlag(ComponentsToChange.VolumetricDustParticles) && Object.op_Implicit((Object)(object)m_Particles))
		{
			m_Particles.alphaAdditionalRuntime = 1f + additive;
		}
	}

	private void Awake()
	{
		m_Beam = ((Component)this).GetComponent<VolumetricLightBeamAbstractBase>();
		m_Light = ((Component)this).GetComponent<Light>();
		m_Particles = ((Component)this).GetComponent<VolumetricDustParticles>();
		GetIntensity(m_Beam as VolumetricLightBeamSD);
		GetIntensity(m_Beam as VolumetricLightBeamHD);
		m_BaseIntensityLight = (Object.op_Implicit((Object)(object)m_Light) ? m_Light.intensity : 0f);
	}

	protected virtual void OnEnable()
	{
		((MonoBehaviour)this).StopAllCoroutines();
	}

	private void OnDisable()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		if (restoreIntensityOnDisable)
		{
			SetAdditiveIntensity(0f);
		}
	}
}
