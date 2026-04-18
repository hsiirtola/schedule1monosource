using System;
using System.Collections.Generic;

namespace ScheduleOne.DevUtilities;

public static class ListExtensions
{
	public static void Shuffle<T>(this IList<T> list, int seed = -1)
	{
		Random random = ((seed == -1) ? new Random() : new Random(seed));
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = random.Next(0, num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}
}
