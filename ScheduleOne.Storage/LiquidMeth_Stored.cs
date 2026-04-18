using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.Storage;

public class LiquidMeth_Stored : StoredItem
{
	public LiquidMethVisuals Visuals;

	public override void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeStoredItem(_item, grid, _originCoordinate, _rotation);
		LiquidMethDefinition def = _item.Definition as LiquidMethDefinition;
		if ((Object)(object)Visuals != (Object)null)
		{
			Visuals.Setup(def);
		}
	}
}
