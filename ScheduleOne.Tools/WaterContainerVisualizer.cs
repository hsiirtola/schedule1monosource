using ScheduleOne.Core.Items.Framework;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.Tools;

public class WaterContainerVisualizer : MonoBehaviour
{
	[SerializeField]
	private TransformLerp[] _waterTransformLerps;

	private WaterContainerInstance _assignedWaterContainer;

	public void AssignWaterContainer(WaterContainerInstance waterContainer)
	{
		if (_assignedWaterContainer != null)
		{
			UnassignWaterContainer();
		}
		_assignedWaterContainer = waterContainer;
		if (_assignedWaterContainer == null)
		{
			SetFillLevel(0f);
			return;
		}
		((BaseItemInstance)_assignedWaterContainer).onDataChanged += WaterContainerChanged;
		SetFillLevel(waterContainer.NormalizedFillAmount);
	}

	public void UnassignWaterContainer()
	{
		if (_assignedWaterContainer != null)
		{
			((BaseItemInstance)_assignedWaterContainer).onDataChanged -= WaterContainerChanged;
			_assignedWaterContainer = null;
		}
	}

	private void WaterContainerChanged()
	{
		SetFillLevel(_assignedWaterContainer.NormalizedFillAmount);
	}

	private void SetFillLevel(float normalizedFillLevel)
	{
		TransformLerp[] waterTransformLerps = _waterTransformLerps;
		for (int i = 0; i < waterTransformLerps.Length; i++)
		{
			waterTransformLerps[i].SetLerpValue(normalizedFillLevel);
		}
	}
}
