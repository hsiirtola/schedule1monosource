using System;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

public class ToggleableSurfaceItemData : SurfaceItemData
{
	public bool IsOn;

	public ToggleableSurfaceItemData(Guid guid, ItemInstance item, int loadOrder, string parentSurfaceGUID, Vector3 pos, Quaternion rot, bool isOn)
		: base(guid, item, loadOrder, parentSurfaceGUID, pos, rot)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		IsOn = isOn;
	}
}
