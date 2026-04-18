using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Effects.MixMaps;

[Serializable]
public class MixerMap : ScriptableObject
{
	public float MapRadius;

	public List<MixerMapEffect> Effects;

	public MixerMapEffect GetEffectAtPoint(Vector2 point)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (((Vector2)(ref point)).magnitude > MapRadius)
		{
			return null;
		}
		for (int i = 0; i < Effects.Count; i++)
		{
			if (Effects[i].IsPointInEffect(point))
			{
				return Effects[i];
			}
		}
		return null;
	}

	public MixerMapEffect GetEffect(Effect property)
	{
		for (int i = 0; i < Effects.Count; i++)
		{
			if ((Object)(object)Effects[i].Property == (Object)(object)property)
			{
				return Effects[i];
			}
		}
		return null;
	}
}
