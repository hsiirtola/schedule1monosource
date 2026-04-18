using System;
using System.Collections;
using UnityEngine;

namespace ScheduleOne.Effects;

public abstract class EffectHandler : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private string _id;

	[SerializeField]
	private bool _scaleToParent;

	[SerializeField]
	private bool _positionToParent;

	[SerializeField]
	private bool _activeByDefault = true;

	private Coroutine _delayDeactivateCoroutine;

	public virtual string Id => _id;

	public virtual bool ScaleToParent => _scaleToParent;

	public virtual bool PositionToParent => _positionToParent;

	public abstract void Activate();

	public abstract void Deactivate();

	public abstract void SetNumericParameter(string effectName, string variable, float value);

	public abstract void SetNumericParameterForAll(string variable, float value);

	public abstract void SetVectorParameter(string effectName, string variable, Vector3 value);

	public abstract void SetVectorParameter(string effectName, string variable, Vector2 value);

	public abstract void SetVectorParameterForAll(string variable, Vector3 value);

	public abstract void SetVectorParameterForAll(string variable, Vector2 value);

	public abstract void SetColorParameterForAll(string variable, Color value);

	public virtual void Initialise()
	{
		if (_activeByDefault)
		{
			Activate();
		}
		else
		{
			Deactivate();
		}
	}

	public void SetPosition(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = position;
	}

	public void SetSize(Vector3 size)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localScale = size;
	}

	public void DelayDeactivate(float duration, Action onComplete = null)
	{
		if (_delayDeactivateCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_delayDeactivateCoroutine);
		}
		_delayDeactivateCoroutine = ((MonoBehaviour)this).StartCoroutine(DoDelayDeactivate(duration, onComplete));
	}

	private IEnumerator DoDelayDeactivate(float duration, Action onComplete = null)
	{
		yield return (object)new WaitForSeconds(duration);
		Deactivate();
		_delayDeactivateCoroutine = null;
		onComplete?.Invoke();
	}

	protected string AddPrefixToVariableName(string variable)
	{
		if (!variable.StartsWith("_"))
		{
			variable = variable.Insert(0, "_");
		}
		return variable;
	}
}
