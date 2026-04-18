using ScheduleOne.Core.Items.Framework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.ObjectScripts.Cash;

public class StoredItem_Cash : StoredItem
{
	protected CashInstance cashInstance;

	[Header("References")]
	public CashStackVisuals Visuals;

	public override void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeStoredItem(_item, grid, _originCoordinate, _rotation);
		cashInstance = base.item as CashInstance;
		RefreshShownBills();
		((BaseItemInstance)cashInstance).onDataChanged += RefreshShownBills;
	}

	public override void Destroy()
	{
		if (cashInstance != null)
		{
			((BaseItemInstance)cashInstance).onDataChanged -= RefreshShownBills;
		}
		base.Destroy();
	}

	private void RefreshShownBills()
	{
		if (!base.Destroyed)
		{
			Visuals.ShowAmount(cashInstance.Balance);
		}
	}
}
