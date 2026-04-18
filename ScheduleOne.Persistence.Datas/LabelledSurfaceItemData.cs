using System;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

public class LabelledSurfaceItemData : SurfaceItemData
{
	public string Message;

	public LabelledSurfaceItemData(Guid guid, ItemInstance item, int loadOrder, string parentSurfaceGUID, Vector3 pos, Quaternion rot, string message)
		: base(guid, item, loadOrder, parentSurfaceGUID, pos, rot)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Message = message;
	}
}
