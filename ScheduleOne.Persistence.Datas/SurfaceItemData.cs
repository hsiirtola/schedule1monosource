using System;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

[Serializable]
public class SurfaceItemData : BuildableItemData
{
	public string ParentSurfaceGUID;

	public Vector3 RelativePosition;

	public Quaternion RelativeRotation;

	public SurfaceItemData(Guid guid, ItemInstance item, int loadOrder, string parentSurfaceGUID, Vector3 pos, Quaternion rot)
		: base(guid, item, loadOrder)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		ParentSurfaceGUID = parentSurfaceGUID;
		RelativePosition = pos;
		RelativeRotation = rot;
	}
}
