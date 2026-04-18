using System.Collections.Generic;
using ScheduleOne.Effects;
using UnityEngine;

namespace ScheduleOne.Weather;

public class ShaderEffectHandler : EffectHandler
{
	[Header("Mesh Renderers")]
	[SerializeField]
	private List<MeshRenderer> _meshRenderers;

	private MaterialPropertyBlock[] _propertyBlocks;

	public override void Initialise()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		base.Initialise();
		_propertyBlocks = (MaterialPropertyBlock[])(object)new MaterialPropertyBlock[_meshRenderers.Count];
		for (int i = 0; i < _meshRenderers.Count; i++)
		{
			_propertyBlocks[i] = new MaterialPropertyBlock();
			((Renderer)_meshRenderers[i]).GetPropertyBlock(_propertyBlocks[i]);
		}
	}

	public override void Activate()
	{
	}

	public override void Deactivate()
	{
	}

	public override void SetVectorParameterForAll(string variable, Vector3 value)
	{
	}

	public override void SetVectorParameterForAll(string variable, Vector2 value)
	{
	}

	public override void SetNumericParameter(string effectName, string variable, float value)
	{
	}

	public override void SetNumericParameterForAll(string variable, float value)
	{
		for (int i = 0; i < _meshRenderers.Count; i++)
		{
			variable = AddPrefixToVariableName(variable);
			_propertyBlocks[i].SetFloat(variable, value);
			((Renderer)_meshRenderers[i]).SetPropertyBlock(_propertyBlocks[i]);
		}
	}

	public override void SetVectorParameter(string effectName, string variable, Vector3 value)
	{
	}

	public override void SetVectorParameter(string effectName, string variable, Vector2 value)
	{
	}

	public override void SetColorParameterForAll(string variable, Color value)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _meshRenderers.Count; i++)
		{
			variable = AddPrefixToVariableName(variable);
			_propertyBlocks[i].SetColor(variable, value);
			((Renderer)_meshRenderers[i]).SetPropertyBlock(_propertyBlocks[i]);
		}
	}
}
