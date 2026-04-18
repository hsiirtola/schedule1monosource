using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Tools;

public class MotionEvent
{
	public List<Action> Actions = new List<Action>();

	public Vector3 LastUpdatedDistance = Vector3.zero;

	public void Update(Vector3 newPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		LastUpdatedDistance = newPosition;
		foreach (Action action in Actions)
		{
			action();
		}
	}
}
