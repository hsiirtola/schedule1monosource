using ScheduleOne.Misc;
using UnityEngine;

namespace ScheduleOne.Map.Infrastructure;

public class TrafficLight : MonoBehaviour
{
	public enum State
	{
		Red,
		Orange,
		Green
	}

	[SerializeField]
	private ToggleableLight _redLight;

	[SerializeField]
	private ToggleableLight _orangeLight;

	[SerializeField]
	private ToggleableLight _greenLight;

	private State _state;

	public State CurrentState
	{
		get
		{
			return _state;
		}
		set
		{
			if (_state != value)
			{
				_state = value;
				ApplyState();
			}
		}
	}

	protected virtual void ApplyState()
	{
		_redLight.isOn = _state == State.Red;
		_orangeLight.isOn = _state == State.Orange;
		_greenLight.isOn = _state == State.Green;
	}
}
