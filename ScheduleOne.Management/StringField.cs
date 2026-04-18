using ScheduleOne.Persistence.Datas;
using UnityEngine.Events;

namespace ScheduleOne.Management;

public class StringField : ConfigField
{
	private string _defaultValue = string.Empty;

	private bool _canBeNullOrEmpty = true;

	public UnityEvent<string> onItemChanged = new UnityEvent<string>();

	public string Value { get; protected set; } = string.Empty;

	public int CharacterLimit { get; protected set; } = -1;

	public StringField(EntityConfiguration parentConfig, string defaultValue)
		: base(parentConfig)
	{
		_defaultValue = defaultValue;
		Value = defaultValue;
	}

	public void SetValue(string value, bool network)
	{
		if (_canBeNullOrEmpty || !string.IsNullOrEmpty(value))
		{
			Value = value;
			if (network)
			{
				base.ParentConfig.ReplicateField(this);
			}
			if (onItemChanged != null)
			{
				onItemChanged.Invoke(Value);
			}
		}
	}

	public void Configure(int characterLimit, bool canBeNullOrEmpty)
	{
		CharacterLimit = characterLimit;
		_canBeNullOrEmpty = canBeNullOrEmpty;
	}

	public override bool IsValueDefault()
	{
		return Value == _defaultValue;
	}

	public StringFieldData GetData()
	{
		return new StringFieldData(Value);
	}

	public void Load(StringFieldData data)
	{
		if (data != null)
		{
			SetValue(data.Value, network: true);
		}
	}
}
