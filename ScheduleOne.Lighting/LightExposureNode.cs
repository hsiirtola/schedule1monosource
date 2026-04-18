using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Lighting;

public class LightExposureNode : MonoBehaviour
{
	public float ambientExposure;

	public Dictionary<UsableLightSource, float> sources = new Dictionary<UsableLightSource, float>();

	public float GetTotalExposure(out float growSpeedMultiplier)
	{
		float num = ambientExposure;
		int num2 = 0;
		growSpeedMultiplier = 0f;
		foreach (UsableLightSource key in sources.Keys)
		{
			if ((Object)(object)key != (Object)null && key.isEmitting)
			{
				num2++;
				num += sources[key];
				growSpeedMultiplier += key.GrowSpeedMultiplier;
			}
		}
		if (num2 > 0)
		{
			growSpeedMultiplier /= num2;
		}
		return num;
	}

	public void AddSource(UsableLightSource source, float lightAmount)
	{
		if (sources.ContainsKey(source))
		{
			sources[source] = lightAmount;
		}
		else
		{
			sources.Add(source, lightAmount);
		}
	}

	public void RemoveSource(UsableLightSource source)
	{
		sources.Remove(source);
	}

	private void OnDrawGizmos()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		float growSpeedMultiplier;
		float totalExposure = GetTotalExposure(out growSpeedMultiplier);
		if (totalExposure > ambientExposure)
		{
			Gizmos.color = new Color(1f, 1f, 1f, totalExposure);
			Gizmos.DrawSphere(((Component)this).transform.position, 0.1f);
		}
	}
}
