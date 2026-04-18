using System;
using System.Collections.Generic;
using ScheduleOne.Effects;
using UnityEngine;

namespace ScheduleOne.Weather;

public class ParticleEffectHandler : EffectHandler
{
	[Header("Components")]
	[SerializeField]
	private List<ParticleSystem> _particleSystems;

	public override void Activate()
	{
		if (_particleSystems != null && _particleSystems.Count != 0)
		{
			_particleSystems.ForEach(delegate(ParticleSystem ps)
			{
				((Component)ps).gameObject.SetActive(true);
			});
		}
	}

	public override void Deactivate()
	{
		if (_particleSystems != null && _particleSystems.Count != 0)
		{
			_particleSystems.ForEach(delegate(ParticleSystem ps)
			{
				((Component)ps).gameObject.SetActive(false);
			});
		}
	}

	public override void SetColorParameterForAll(string variable, Color value)
	{
		throw new NotImplementedException();
	}

	public override void SetNumericParameter(string effectName, string variable, float value)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		ParticleSystem val = _particleSystems.Find((ParticleSystem p) => ((Object)((Component)p).gameObject).name == effectName);
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogWarning((object)("Particle system with name '" + effectName + "' not found in handler '" + Id + "'"));
		}
		else if (variable == "EmissionRateOverTime")
		{
			EmissionModule emission = val.emission;
			((EmissionModule)(ref emission)).rateOverTime = MinMaxCurve.op_Implicit(value);
		}
	}

	public override void SetNumericParameterForAll(string variable, float value)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		foreach (ParticleSystem particleSystem in _particleSystems)
		{
			_ = particleSystem.main;
			if (variable == "EmissionRateOverTime")
			{
				EmissionModule emission = particleSystem.emission;
				((EmissionModule)(ref emission)).rateOverTime = MinMaxCurve.op_Implicit(value);
			}
		}
	}

	public override void SetVectorParameter(string effectName, string variable, Vector3 value)
	{
		throw new NotImplementedException();
	}

	public override void SetVectorParameter(string effectName, string variable, Vector2 value)
	{
		throw new NotImplementedException();
	}

	public override void SetVectorParameterForAll(string variable, Vector3 value)
	{
		throw new NotImplementedException();
	}

	public override void SetVectorParameterForAll(string variable, Vector2 value)
	{
		throw new NotImplementedException();
	}
}
