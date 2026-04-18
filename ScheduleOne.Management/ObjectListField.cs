using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.EntityFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.UI.Management;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Management;

public class ObjectListField(EntityConfiguration parentConfig) : ConfigField(parentConfig)
{
	public List<BuildableItem> SelectedObjects = new List<BuildableItem>();

	public int MaxItems = 1;

	public ObjectSelector.ObjectFilter objectFilter;

	public List<Type> TypeRequirements = new List<Type>();

	public UnityEvent<List<BuildableItem>> onListChanged = new UnityEvent<List<BuildableItem>>();

	public void SetList(List<BuildableItem> list, bool network)
	{
		if (SelectedObjects.SequenceEqual(list))
		{
			return;
		}
		for (int i = 0; i < SelectedObjects.Count; i++)
		{
			if (!((Object)(object)SelectedObjects[i] == (Object)null))
			{
				BuildableItem buildableItem = SelectedObjects[i];
				buildableItem.onDestroyedWithParameter = (Action<BuildableItem>)Delegate.Remove(buildableItem.onDestroyedWithParameter, new Action<BuildableItem>(SelectedObjectDestroyed));
			}
		}
		SelectedObjects = new List<BuildableItem>();
		SelectedObjects.AddRange(list);
		for (int j = 0; j < SelectedObjects.Count; j++)
		{
			if (!((Object)(object)SelectedObjects[j] == (Object)null))
			{
				BuildableItem buildableItem2 = SelectedObjects[j];
				buildableItem2.onDestroyedWithParameter = (Action<BuildableItem>)Delegate.Combine(buildableItem2.onDestroyedWithParameter, new Action<BuildableItem>(SelectedObjectDestroyed));
			}
		}
		if (network)
		{
			base.ParentConfig.ReplicateField(this);
		}
		if (onListChanged != null)
		{
			onListChanged.Invoke(list);
		}
	}

	public void AddItem(BuildableItem item)
	{
		if (!SelectedObjects.Contains(item))
		{
			if (SelectedObjects.Count >= MaxItems)
			{
				Console.LogWarning(((BaseItemInstance)item.ItemInstance).Name + " cannot be added to " + base.ParentConfig.GetType().Name + " because the maximum number of items has been reached");
				return;
			}
			List<BuildableItem> list = new List<BuildableItem>(SelectedObjects);
			list.Add(item);
			SetList(list, network: true);
		}
	}

	public void RemoveItem(BuildableItem item)
	{
		if (SelectedObjects.Contains(item))
		{
			List<BuildableItem> list = new List<BuildableItem>(SelectedObjects);
			list.Remove(item);
			SetList(list, network: true);
		}
	}

	private void SelectedObjectDestroyed(BuildableItem item)
	{
		if (!((Object)(object)item == (Object)null))
		{
			Console.Log("Removing destroyed object from " + base.ParentConfig.GetType().Name);
			RemoveItem(item);
		}
	}

	public override bool IsValueDefault()
	{
		if (SelectedObjects != null)
		{
			return SelectedObjects.Count == 0;
		}
		return true;
	}

	public ObjectListFieldData GetData()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < SelectedObjects.Count; i++)
		{
			list.Add(SelectedObjects[i].GUID.ToString());
		}
		return new ObjectListFieldData(list);
	}

	public void Load(ObjectListFieldData data)
	{
		if (data == null)
		{
			return;
		}
		List<BuildableItem> list = new List<BuildableItem>();
		for (int i = 0; i < data.ObjectGUIDs.Count; i++)
		{
			if (!string.IsNullOrEmpty(data.ObjectGUIDs[i]))
			{
				BuildableItem buildableItem = GUIDManager.GetObject<BuildableItem>(new Guid(data.ObjectGUIDs[i]));
				if ((Object)(object)buildableItem != (Object)null)
				{
					list.Add(buildableItem);
				}
			}
		}
		SetList(list, network: true);
	}
}
