using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Misc;

public class ToggleableLight : MonoBehaviour
{
	private enum State
	{
		NotInitialized,
		On,
		Off
	}

	[SerializeField]
	[FormerlySerializedAs("isOn")]
	private bool _isOn;

	[Header("References")]
	[SerializeField]
	protected OptimizedLight[] lightSources;

	[SerializeField]
	protected MeshRenderer[] lightSurfacesMeshes;

	public int MaterialIndex;

	[Header("Materials")]
	[SerializeField]
	protected Material lightOnMat;

	[SerializeField]
	protected Material lightOffMat;

	private State state;

	public bool isOn
	{
		get
		{
			return _isOn;
		}
		set
		{
			_isOn = value;
			SetLights();
		}
	}

	protected virtual void Awake()
	{
		SetLights();
	}

	public void TurnOn()
	{
		isOn = true;
	}

	public void TurnOff()
	{
		isOn = false;
	}

	protected virtual void SetLights()
	{
		switch (state)
		{
		case State.On:
			if (isOn)
			{
				return;
			}
			break;
		case State.Off:
			if (!isOn)
			{
				return;
			}
			break;
		}
		state = (isOn ? State.On : State.Off);
		OptimizedLight[] array = lightSources;
		foreach (OptimizedLight optimizedLight in array)
		{
			if (!((Object)(object)optimizedLight == (Object)null))
			{
				optimizedLight.Enabled = isOn;
			}
		}
		Material val = (isOn ? lightOnMat : lightOffMat);
		MeshRenderer[] array2 = lightSurfacesMeshes;
		foreach (MeshRenderer val2 in array2)
		{
			if (!((Object)(object)val2 == (Object)null))
			{
				Material[] sharedMaterials = ((Renderer)val2).sharedMaterials;
				sharedMaterials[MaterialIndex] = val;
				((Renderer)val2).materials = sharedMaterials;
			}
		}
	}
}
