using ScheduleOne.ItemFramework;
using ScheduleOne.Storage;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.ObjectScripts.WateringCan;

public class WaterContainerStored : StoredItem
{
	[SerializeField]
	private WaterContainerVisualizer _visuals;

	public override void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeStoredItem(_item, grid, _originCoordinate, _rotation);
		if ((Object)(object)_visuals != (Object)null)
		{
			_visuals.AssignWaterContainer(_item as WaterContainerInstance);
		}
	}

	public override void Destroy()
	{
		if ((Object)(object)_visuals != (Object)null)
		{
			_visuals.UnassignWaterContainer();
		}
		base.Destroy();
	}
}
