using Unity.Collections;
using UnityEngine;

public class CircularQueue<T> where T : struct
{
	public NativeArray<T> q;

	private int idx;

	private int cap;

	private int length;

	public T this[int i] => q[i];

	public int Capacity => cap;

	public int Count => length;

	public CircularQueue(int capacity)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		q = new NativeArray<T>(capacity, (Allocator)4, (NativeArrayOptions)1);
		cap = capacity;
		length = 0;
		idx = 0;
	}

	public void Enqueue(T item)
	{
		q[idx] = item;
		idx = modulo(idx + 1, cap);
		length = Mathf.Min(length + 1, cap);
	}

	public void Dequeue()
	{
		if (length != 0)
		{
			q[idx] = default(T);
			idx = modulo(idx - 1, cap);
			length = Mathf.Max(length - 1, 0);
		}
	}

	public void Clear()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		q.Dispose();
		q = new NativeArray<T>(cap, (Allocator)4, (NativeArrayOptions)1);
		idx = 0;
		length = 0;
	}

	private int modulo(int i, int m)
	{
		int num = i % m;
		if (num >= 0)
		{
			return num;
		}
		return num + m;
	}
}
