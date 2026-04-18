using System.Collections.Generic;

namespace ScheduleOne.DevUtilities;

public static class Extensions
{
	public static T[] ShiftLeft<T>(this T[] array)
	{
		if (array.Length == 0)
		{
			return array;
		}
		T val = array[0];
		for (int i = 0; i < array.Length - 1; i++)
		{
			array[i] = array[i + 1];
		}
		array[array.Length - 1] = val;
		return array;
	}

	public static T[] ShiftRight<T>(this T[] array)
	{
		if (array.Length == 0)
		{
			return array;
		}
		T val = array[array.Length - 1];
		for (int num = array.Length - 1; num > 0; num--)
		{
			array[num] = array[num - 1];
		}
		array[0] = val;
		return array;
	}

	public static List<T> ShiftLeft<T>(this List<T> array)
	{
		if (array.Count == 0)
		{
			return array;
		}
		T value = array[0];
		for (int i = 0; i < array.Count - 1; i++)
		{
			array[i] = array[i + 1];
		}
		array[array.Count - 1] = value;
		return array;
	}

	public static List<T> ShiftRight<T>(this List<T> array)
	{
		if (array.Count == 0)
		{
			return array;
		}
		T value = array[array.Count - 1];
		for (int num = array.Count - 1; num > 0; num--)
		{
			array[num] = array[num - 1];
		}
		array[0] = value;
		return array;
	}
}
