using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.NPCs;

public class NPCSpeedController : MonoBehaviour
{
	[Serializable]
	public class SpeedControl
	{
		public string id;

		public int priority;

		public float speed;

		public SpeedControl(string id, int priority, float speed)
		{
			this.id = id;
			this.priority = priority;
			this.speed = speed;
		}
	}

	[Header("Settings")]
	[Range(0f, 1f)]
	public float DefaultWalkSpeed = 0.08f;

	[SerializeField]
	[FormerlySerializedAs("SpeedMultiplier")]
	private float _SpeedMultiplier = 1f;

	[Header("References")]
	public NPCMovement Movement;

	protected List<SpeedControl> speedControlStack = new List<SpeedControl>();

	public SpeedControl ActiveSpeedControl;

	public float SpeedMultiplier
	{
		get
		{
			return _SpeedMultiplier;
		}
		set
		{
			_SpeedMultiplier = value;
			UpdateActiveSpeedControl();
		}
	}

	private void Awake()
	{
		AddSpeedControl(new SpeedControl("default", 0, DefaultWalkSpeed));
	}

	public void AddSpeedControl(SpeedControl control)
	{
		SpeedControl speedControl = speedControlStack.Find((SpeedControl x) => x.id == control.id);
		if (speedControl != null)
		{
			speedControl.priority = control.priority;
			speedControl.speed = control.speed;
			UpdateActiveSpeedControl();
			return;
		}
		for (int num = 0; num < speedControlStack.Count; num++)
		{
			if (control.priority >= speedControlStack[num].priority)
			{
				speedControlStack.Insert(num, control);
				UpdateActiveSpeedControl();
				return;
			}
		}
		speedControlStack.Add(control);
		UpdateActiveSpeedControl();
	}

	public SpeedControl GetSpeedControl(string id)
	{
		return speedControlStack.Find((SpeedControl x) => x.id == id);
	}

	public bool DoesSpeedControlExist(string id)
	{
		return GetSpeedControl(id) != null;
	}

	public void RemoveSpeedControl(string id)
	{
		SpeedControl speedControl = speedControlStack.Find((SpeedControl x) => x.id == id);
		if (speedControl != null)
		{
			speedControlStack.Remove(speedControl);
			UpdateActiveSpeedControl();
		}
	}

	private void UpdateActiveSpeedControl()
	{
		if (speedControlStack.Count > 0)
		{
			ActiveSpeedControl = speedControlStack[0];
			Movement.MovementSpeedScale = ActiveSpeedControl.speed * SpeedMultiplier;
		}
	}
}
