using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Effects;

[Serializable]
public class EffectSettingsWrapper
{
	public List<NumericParameter> NumericParameters = new List<NumericParameter>();

	public List<GradientParameter> GradientParameters = new List<GradientParameter>();

	public float GetNumericParameter(string variable)
	{
		NumericParameter numericParameter = NumericParameters.Find((NumericParameter p) => p.Variable == variable);
		if (numericParameter != null)
		{
			return numericParameter.Value;
		}
		Debug.LogWarning((object)("Numeric parameter '" + variable + "' not found."));
		return 0f;
	}

	public void SetNumericParameter(string variable, float value)
	{
		NumericParameter numericParameter = NumericParameters.Find((NumericParameter p) => p.Variable == variable);
		if (numericParameter != null)
		{
			numericParameter.Value = value;
		}
		else
		{
			Debug.LogWarning((object)("Numeric parameter '" + variable + "' not found."));
		}
	}

	public Gradient GetGradientParameter(string variable)
	{
		GradientParameter gradientParameter = GradientParameters.Find((GradientParameter p) => p.Variable == variable);
		if (gradientParameter != null)
		{
			return gradientParameter.Value;
		}
		Debug.LogWarning((object)("Gradient parameter '" + variable + "' not found."));
		return null;
	}

	public void SetGradientParameter(string variable, Gradient value)
	{
		GradientParameter gradientParameter = GradientParameters.Find((GradientParameter p) => p.Variable == variable);
		if (gradientParameter != null)
		{
			gradientParameter.Value = value;
		}
	}
}
