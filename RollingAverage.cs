using System;

public class RollingAverage<T>
{
	private readonly T[] buffer;

	private readonly Func<T, T, T> add;

	private readonly Func<T, T, T> sub;

	private readonly Func<T, float, T> div;

	private int head;

	private int count;

	private T sum;

	public T Average
	{
		get
		{
			if (count != 0)
			{
				return div(sum, count);
			}
			return default(T);
		}
	}

	public int Count => count;

	public int Capacity => buffer.Length;

	public RollingAverage(int capacity, Func<T, T, T> add, Func<T, T, T> sub, Func<T, float, T> div)
	{
		if (capacity <= 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		buffer = new T[capacity];
		this.add = add;
		this.sub = sub;
		this.div = div;
		sum = default(T);
		head = 0;
		count = 0;
	}

	public void Add(T value)
	{
		if (count < buffer.Length)
		{
			buffer[head] = value;
			sum = add(sum, value);
			count++;
		}
		else
		{
			sum = sub(sum, buffer[head]);
			buffer[head] = value;
			sum = add(sum, value);
		}
		head = (head + 1) % buffer.Length;
	}

	public void Clear()
	{
		Array.Clear(buffer, 0, buffer.Length);
		sum = default(T);
		head = 0;
		count = 0;
	}
}
