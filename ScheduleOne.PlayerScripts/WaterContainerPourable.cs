using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.PlayerScripts;

public class WaterContainerPourable : Pourable
{
	[SerializeField]
	private WaterContainerVisualizer _visuals;

	private WaterContainerInstance _waterContainerItem;

	public void SetupWaterContainerPourable(WaterContainerInstance waterContainer)
	{
		_waterContainerItem = waterContainer;
		autoSetCurrentQuantity = false;
		base.CurrentQuantity = _waterContainerItem.CurrentFillAmount;
		if ((Object)(object)_visuals != (Object)null)
		{
			_visuals.AssignWaterContainer(_waterContainerItem);
		}
		base.Rb.isKinematic = false;
	}

	private void OnDestroy()
	{
		if ((Object)(object)_visuals != (Object)null)
		{
			_visuals.UnassignWaterContainer();
		}
	}

	protected override void PourAmount(float amount)
	{
		_waterContainerItem.ChangeFillAmount(0f - amount);
		base.PourAmount(amount);
	}
}
