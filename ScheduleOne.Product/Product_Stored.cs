using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Product;

public class Product_Stored : StoredItem
{
	public ProductVisualsSetter Visuals;

	public override void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeStoredItem(_item, grid, _originCoordinate, _rotation);
		ProductItemInstance productInstance = _item as ProductItemInstance;
		Visuals.ApplyVisuals(productInstance);
	}
}
