using System;
using System.Collections.Generic;
using ScheduleOne.EntityFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.UI.Management;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Management;

public class ObjectField(EntityConfiguration parentConfig) : ConfigField(parentConfig)
{
	public BuildableItem SelectedObject;

	public UnityEvent<BuildableItem> onObjectChanged = new UnityEvent<BuildableItem>();

	public ObjectSelector.ObjectFilter objectFilter;

	public List<Type> TypeRequirements = new List<Type>();

	public bool DrawTransitLine;

	public void SetObject(BuildableItem obj, bool network)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		if (!((Object)(object)SelectedObject == (Object)(object)obj))
		{
			if ((Object)(object)SelectedObject != (Object)null)
			{
				SelectedObject.onDestroyed.RemoveListener(new UnityAction(SelectedObjectDestroyed));
			}
			SelectedObject = obj;
			if ((Object)(object)SelectedObject != (Object)null)
			{
				SelectedObject.onDestroyed.AddListener(new UnityAction(SelectedObjectDestroyed));
			}
			if (network)
			{
				base.ParentConfig.ReplicateField(this);
			}
			if (onObjectChanged != null)
			{
				onObjectChanged.Invoke(obj);
			}
		}
	}

	public override bool IsValueDefault()
	{
		return (Object)(object)SelectedObject == (Object)null;
	}

	private void SelectedObjectDestroyed()
	{
		SetObject(null, network: false);
	}

	public void Load(ObjectFieldData data)
	{
		if (data != null && !string.IsNullOrEmpty(data.ObjectGUID))
		{
			BuildableItem buildableItem = GUIDManager.GetObject<BuildableItem>(new Guid(data.ObjectGUID));
			if ((Object)(object)buildableItem != (Object)null)
			{
				SetObject(buildableItem, network: true);
			}
		}
	}

	public ObjectFieldData GetData()
	{
		return new ObjectFieldData(((Object)(object)SelectedObject != (Object)null) ? SelectedObject.GUID.ToString() : "");
	}
}
