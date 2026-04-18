using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Persistence;

[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
{
	[SerializeField]
	private List<TKey> keys = new List<TKey>();

	[SerializeField]
	private List<TValue> values = new List<TValue>();

	private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

	public TValue this[TKey key]
	{
		get
		{
			return dictionary[key];
		}
		set
		{
			dictionary[key] = value;
		}
	}

	public ICollection<TKey> Keys => dictionary.Keys;

	public ICollection<TValue> Values => dictionary.Values;

	public int Count => dictionary.Count;

	public bool IsReadOnly => false;

	public void OnBeforeSerialize()
	{
		keys.Clear();
		values.Clear();
		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			keys.Add(item.Key);
			values.Add(item.Value);
		}
	}

	public void OnAfterDeserialize()
	{
		dictionary = new Dictionary<TKey, TValue>();
		for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
		{
			if (!dictionary.ContainsKey(keys[i]))
			{
				dictionary.Add(keys[i], values[i]);
			}
		}
	}

	public void Add(TKey key, TValue value)
	{
		dictionary.Add(key, value);
	}

	public bool ContainsKey(TKey key)
	{
		return dictionary.ContainsKey(key);
	}

	public bool Remove(TKey key)
	{
		return dictionary.Remove(key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return dictionary.TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		dictionary.Add(item.Key, item.Value);
	}

	public void Clear()
	{
		dictionary.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		return dictionary.ContainsKey(item.Key);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return dictionary.Remove(item.Key);
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return dictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return dictionary.GetEnumerator();
	}
}
