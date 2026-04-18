using ScheduleOne.Product;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Packaging;

public class FilledPackaging_StoredItem : StoredItem
{
	public MultiTypeVisualsSetter Visuals;

	public override void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeStoredItem(_item, grid, _originCoordinate, _rotation);
		Visuals.ApplyVisuals(_item as ProductItemInstance);
	}
}
