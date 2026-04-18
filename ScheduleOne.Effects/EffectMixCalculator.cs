using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects.MixMaps;
using ScheduleOne.Product;
using ScheduleOne.StationFramework;
using UnityEngine;

namespace ScheduleOne.Effects;

public static class EffectMixCalculator
{
	private class Reaction
	{
		public Effect Existing;

		public Effect Output;
	}

	public const int MAX_PROPERTIES = 8;

	public const float MAX_DELTA_DIFFERENCE = 0.5f;

	public static List<Effect> MixProperties(List<Effect> existingProperties, Effect newProperty, EDrugType drugType)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		StationRecipe recipe = NetworkSingleton<ProductManager>.Instance.GetRecipe(existingProperties, newProperty);
		if ((Object)(object)recipe != (Object)null)
		{
			return (recipe.Product.Item as ProductDefinition).Properties;
		}
		Vector2 val = newProperty.MixDirection * newProperty.MixMagnitude;
		if (NetworkSingleton<GameManager>.Instance.Settings.UseRandomizedMixMaps)
		{
			val = Vector2.op_Implicit(Quaternion.Euler(0f, 0f, (float)(GameManager.Seed % 360)) * Vector2.op_Implicit(val));
		}
		MixerMap mixerMap = NetworkSingleton<ProductManager>.Instance.GetMixerMap(drugType);
		List<Reaction> list = new List<Reaction>();
		for (int i = 0; i < existingProperties.Count; i++)
		{
			Vector2 point = mixerMap.GetEffect(existingProperties[i]).Position + val;
			Effect effect = mixerMap.GetEffectAtPoint(point)?.Property;
			if ((Object)(object)effect != (Object)null)
			{
				Reaction item = new Reaction
				{
					Existing = existingProperties[i],
					Output = effect
				};
				list.Add(item);
			}
		}
		List<Effect> list2 = new List<Effect>(existingProperties);
		foreach (Reaction item2 in list)
		{
			if (!list2.Contains(item2.Output))
			{
				list2[list2.IndexOf(item2.Existing)] = item2.Output;
			}
		}
		if (!list2.Contains(newProperty) && list2.Count < 8)
		{
			list2.Add(newProperty);
		}
		return list2.Distinct().ToList();
	}

	public static void Shuffle<T>(List<T> list, int seed)
	{
		Random random = new Random(seed);
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = random.Next(num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}
}
