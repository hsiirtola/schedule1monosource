using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Map.Infrastructure;

public class Intersection : MonoBehaviour
{
	private const float AmberTime = 3f;

	[Header("References")]
	[SerializeField]
	protected List<TrafficLight> path1Lights = new List<TrafficLight>();

	[SerializeField]
	protected List<TrafficLight> path2Lights = new List<TrafficLight>();

	[SerializeField]
	protected List<GameObject> path1Obstacles = new List<GameObject>();

	[SerializeField]
	protected List<GameObject> path2Obstacles = new List<GameObject>();

	[Header("Settings")]
	[SerializeField]
	protected float path1Time = 10f;

	[SerializeField]
	protected float path2Time = 10f;

	[SerializeField]
	protected float timeOffset;

	protected virtual void Start()
	{
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Run());
	}

	protected IEnumerator Run()
	{
		while ((Object)(object)this != (Object)null && (Object)(object)((Component)this).gameObject != (Object)null && ((Component)this).gameObject.activeInHierarchy)
		{
			SetPath1Lights(TrafficLight.State.Green);
			SetPath2Lights(TrafficLight.State.Red);
			if (timeOffset != 0f)
			{
				yield return (object)new WaitForSecondsRealtime(Mathf.Abs(timeOffset));
				timeOffset = 0f;
			}
			yield return (object)new WaitForSecondsRealtime(path1Time);
			SetPath1Lights(TrafficLight.State.Orange);
			yield return (object)new WaitForSecondsRealtime(3f);
			SetPath1Lights(TrafficLight.State.Red);
			yield return (object)new WaitForSecondsRealtime(1f);
			SetPath2Lights(TrafficLight.State.Green);
			yield return (object)new WaitForSecondsRealtime(path2Time);
			SetPath2Lights(TrafficLight.State.Orange);
			yield return (object)new WaitForSecondsRealtime(3f);
			SetPath2Lights(TrafficLight.State.Red);
			yield return (object)new WaitForSecondsRealtime(1f);
		}
	}

	protected void SetPath1Lights(TrafficLight.State state)
	{
		foreach (TrafficLight path1Light in path1Lights)
		{
			path1Light.CurrentState = state;
		}
		if (state == TrafficLight.State.Green)
		{
			foreach (GameObject path1Obstacle in path1Obstacles)
			{
				if (!((Object)(object)path1Obstacle == (Object)null))
				{
					path1Obstacle.gameObject.SetActive(false);
				}
			}
			return;
		}
		foreach (GameObject path1Obstacle2 in path1Obstacles)
		{
			if (!((Object)(object)path1Obstacle2 == (Object)null))
			{
				path1Obstacle2.gameObject.SetActive(true);
			}
		}
	}

	protected void SetPath2Lights(TrafficLight.State state)
	{
		foreach (TrafficLight path2Light in path2Lights)
		{
			path2Light.CurrentState = state;
		}
		if (state == TrafficLight.State.Green)
		{
			foreach (GameObject path2Obstacle in path2Obstacles)
			{
				if (!((Object)(object)path2Obstacle == (Object)null))
				{
					path2Obstacle.gameObject.SetActive(false);
				}
			}
			return;
		}
		foreach (GameObject path2Obstacle2 in path2Obstacles)
		{
			if (!((Object)(object)path2Obstacle2 == (Object)null))
			{
				path2Obstacle2.gameObject.SetActive(true);
			}
		}
	}
}
