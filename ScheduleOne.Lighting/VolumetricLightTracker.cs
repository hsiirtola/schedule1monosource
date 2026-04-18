using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Serialization;
using VLB;

namespace ScheduleOne.Lighting;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
[RequireComponent(typeof(VolumetricLightBeamSD))]
public class VolumetricLightTracker : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("Override")]
	private bool _Override;

	[SerializeField]
	[FormerlySerializedAs("Enabled")]
	private bool _Enabled;

	public Light light;

	public OptimizedLight optimizedLight;

	public VolumetricLightBeamSD beam;

	public VolumetricDustParticles dust;

	public bool Override
	{
		get
		{
			return _Override;
		}
		set
		{
			_Override = value;
			UpdateEffectsState();
		}
	}

	public bool Enabled
	{
		get
		{
			return _Enabled;
		}
		set
		{
			_Enabled = value;
			UpdateEffectsState();
		}
	}

	private void AssignReferences()
	{
		if ((Object)(object)light == (Object)null)
		{
			light = ((Component)this).GetComponent<Light>();
		}
		if ((Object)(object)optimizedLight == (Object)null)
		{
			optimizedLight = ((Component)this).GetComponent<OptimizedLight>();
		}
		if ((Object)(object)beam == (Object)null)
		{
			beam = ((Component)this).GetComponent<VolumetricLightBeamSD>();
		}
		if ((Object)(object)dust == (Object)null)
		{
			dust = ((Component)this).GetComponent<VolumetricDustParticles>();
		}
	}

	private void UpdateEffectsState()
	{
		if (Override)
		{
			((Behaviour)beam).enabled = Enabled;
		}
		else if ((Object)(object)optimizedLight != (Object)null)
		{
			((Behaviour)beam).enabled = optimizedLight.Enabled;
		}
		else if ((Object)(object)light != (Object)null)
		{
			((Behaviour)beam).enabled = ((Behaviour)light).enabled;
		}
		if ((Object)(object)dust != (Object)null)
		{
			((Behaviour)dust).enabled = ((Behaviour)beam).enabled;
		}
	}

	private void Awake()
	{
		AssignReferences();
	}
}
