using System.Collections.Generic;
using FishNet.Connection;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Management;

public class EntityConfiguration
{
	private const int NameCharacterLimit = 28;

	public List<ConfigField> Fields = new List<ConfigField>();

	public UnityEvent onChanged = new UnityEvent();

	public ConfigurationReplicator Replicator { get; protected set; }

	public IConfigurable Configurable { get; protected set; }

	public bool IsSelected { get; protected set; }

	public StringField Name { get; private set; }

	public virtual bool AllowRename()
	{
		return true;
	}

	public EntityConfiguration(ConfigurationReplicator replicator, IConfigurable configurable, string defaultName)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		Replicator = replicator;
		Replicator.Configuration = this;
		Configurable = configurable;
		Name = new StringField(this, defaultName);
		Name.Configure(28, canBeNullOrEmpty: false);
		Name.onItemChanged.AddListener((UnityAction<string>)delegate
		{
			InvokeChanged();
		});
	}

	protected void InvokeChanged()
	{
		if (onChanged != null)
		{
			onChanged.Invoke();
		}
	}

	public void ReplicateField(ConfigField field, NetworkConnection conn = null)
	{
		Replicator.ReplicateField(field, conn);
	}

	public void ReplicateAllFields(NetworkConnection conn = null, bool replicateDefaults = true)
	{
		foreach (ConfigField field in Fields)
		{
			if (field == null)
			{
				Debug.LogError((object)"Null field in configuration");
			}
			else if (replicateDefaults || !field.IsValueDefault())
			{
				ReplicateField(field, conn);
			}
		}
	}

	public virtual void Destroy()
	{
		Reset();
	}

	public virtual void Reset()
	{
	}

	public virtual void Selected()
	{
		IsSelected = true;
	}

	public virtual void Deselected()
	{
		IsSelected = false;
	}

	public virtual bool ShouldSave()
	{
		if (!Name.IsValueDefault())
		{
			return true;
		}
		return false;
	}

	public virtual string GetSaveString()
	{
		return new RenamableConfigurationData(Name.GetData()).GetJson();
	}

	public T GetField<T>() where T : ConfigField
	{
		foreach (ConfigField field in Fields)
		{
			if (field is T result)
			{
				return result;
			}
		}
		return null;
	}
}
