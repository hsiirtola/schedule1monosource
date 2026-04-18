using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace ScheduleOne.Effects;

public class VFXEffectHandler : EffectHandler
{
	[Header("Components")]
	[SerializeField]
	private List<VisualEffect> _visualEffects;

	public override void Activate()
	{
		if (_visualEffects != null && _visualEffects.Count != 0)
		{
			_visualEffects.ForEach(delegate(VisualEffect e)
			{
				((Component)e).gameObject.SetActive(true);
			});
		}
	}

	public override void Deactivate()
	{
		if (_visualEffects != null && _visualEffects.Count != 0)
		{
			_visualEffects.ForEach(delegate(VisualEffect e)
			{
				((Component)e).gameObject.SetActive(false);
			});
		}
	}

	public override void SetColorParameterForAll(string variable, Color value)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		foreach (VisualEffect visualEffect in _visualEffects)
		{
			if (visualEffect.HasVector4(variable))
			{
				visualEffect.SetVector4(variable, Color.op_Implicit(value));
			}
		}
	}

	public override void SetNumericParameter(string effectName, string variable, float value)
	{
		VisualEffect obj = _visualEffects.Find((VisualEffect e) => ((Object)((Component)e).gameObject).name == effectName);
		if (obj != null)
		{
			obj.SetFloat(variable, value);
		}
	}

	public override void SetNumericParameterForAll(string variable, float value)
	{
		foreach (VisualEffect visualEffect in _visualEffects)
		{
			if (visualEffect.HasFloat(variable))
			{
				visualEffect.SetFloat(variable, value);
			}
		}
	}

	public override void SetVectorParameter(string effectName, string variable, Vector3 value)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		VisualEffect obj = _visualEffects.Find((VisualEffect e) => ((Object)((Component)e).gameObject).name == effectName);
		if (obj != null)
		{
			obj.SetVector3(variable, value);
		}
	}

	public override void SetVectorParameter(string effectName, string variable, Vector2 value)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		VisualEffect obj = _visualEffects.Find((VisualEffect e) => ((Object)((Component)e).gameObject).name == effectName);
		if (obj != null)
		{
			obj.SetVector2(variable, value);
		}
	}

	public override void SetVectorParameterForAll(string variable, Vector3 value)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		foreach (VisualEffect visualEffect in _visualEffects)
		{
			if (visualEffect.HasVector3(variable))
			{
				visualEffect.SetVector3(variable, value);
			}
		}
	}

	public override void SetVectorParameterForAll(string variable, Vector2 value)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		foreach (VisualEffect visualEffect in _visualEffects)
		{
			if (visualEffect.HasVector2(variable))
			{
				visualEffect.SetVector2(variable, value);
			}
		}
	}
}
