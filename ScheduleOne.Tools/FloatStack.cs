using System;
using System.Collections.Generic;

namespace ScheduleOne.Tools;

public class FloatStack
{
	public enum EStackMode
	{
		Additive,
		Override,
		Multiplicative
	}

	public class StackEntry
	{
		public string Label { get; private set; }

		public float Value { get; private set; }

		public EStackMode Mode { get; private set; }

		public int Order { get; private set; }

		public StackEntry(string label, float value, EStackMode mode, int order)
		{
			Label = label;
			Value = value;
			Mode = mode;
			Order = order;
		}
	}

	private float _defaultValue;

	private List<StackEntry> _stack = new List<StackEntry>();

	public float Value { get; private set; }

	public event Action<float> OnValueChanged;

	public FloatStack(float defaultValue)
	{
		_defaultValue = defaultValue;
		Recalculate();
	}

	public void Add(StackEntry entry)
	{
		_stack.RemoveAll((StackEntry e) => e.Label == entry.Label);
		_stack.Add(entry);
		Recalculate();
	}

	public void Remove(string label)
	{
		_stack.RemoveAll((StackEntry e) => e.Label == label);
		Recalculate();
	}

	public bool TryGetEntry(string label, out StackEntry entry)
	{
		entry = _stack.Find((StackEntry e) => e.Label == label);
		return entry != null;
	}

	private void Recalculate()
	{
		float value = Value;
		Value = _defaultValue;
		_stack.Sort((StackEntry a, StackEntry b) => a.Order.CompareTo(b.Order));
		foreach (StackEntry item in _stack)
		{
			switch (item.Mode)
			{
			case EStackMode.Additive:
				Value += item.Value;
				break;
			case EStackMode.Override:
				Value = item.Value;
				break;
			case EStackMode.Multiplicative:
				Value *= item.Value;
				break;
			}
		}
		if (!object.Equals(value, Value) && this.OnValueChanged != null)
		{
			this.OnValueChanged(Value);
		}
	}
}
