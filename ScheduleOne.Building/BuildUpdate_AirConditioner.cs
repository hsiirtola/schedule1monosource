using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Property;
using ScheduleOne.Temperature;
using UnityEngine;

namespace ScheduleOne.Building;

public class BuildUpdate_AirConditioner : BuildUpdate_Grid
{
	private AirConditioner _ac;

	private AirConditioner.EMode _currentMode = AirConditioner.EMode.Heating;

	private ScheduleOne.Property.Property _currentProperty;

	public override void Initialize(GridItem buildableItemClass, ItemInstance itemInstance, GameObject ghostModel)
	{
		base.Initialize(buildableItemClass, itemInstance, ghostModel);
		_ac = base.BuildableItemClass as AirConditioner;
		_ac.SetMode(_currentMode);
	}

	protected override void Update()
	{
		base.Update();
		if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
		{
			CycleACMode();
		}
	}

	private void CycleACMode()
	{
		if (_ac.CurrentMode == AirConditioner.EMode.Cooling)
		{
			SetACMode(AirConditioner.EMode.Heating);
		}
		else
		{
			SetACMode(AirConditioner.EMode.Cooling);
		}
	}

	private void SetACMode(AirConditioner.EMode mode)
	{
		_ac.SetMode(mode);
		_currentMode = mode;
	}

	protected override void OnPlacedObjectPreSpawn(GridItem item)
	{
		base.OnPlacedObjectPreSpawn(item);
		(item as AirConditioner).SetMode(_currentMode);
	}

	protected override void OnClosestIntersectionChanged(TileIntersection previous, TileIntersection current)
	{
		base.OnClosestIntersectionChanged(previous, current);
		if (previous != null && (Object)(object)previous.tile != (Object)null)
		{
			RemoveFromPropery();
		}
		if (current != null && (Object)(object)current.tile != (Object)null)
		{
			AddToProperty(current.tile.OwnerGrid.ParentProperty);
		}
	}

	private void AddToProperty(ScheduleOne.Property.Property property)
	{
		if (!((Object)(object)property == (Object)null) && !((Object)(object)_currentProperty == (Object)(object)property))
		{
			if ((Object)(object)_currentProperty != (Object)null)
			{
				RemoveFromPropery();
			}
			_currentProperty = property;
			for (int i = 0; i < property.Grids.Count; i++)
			{
				property.Grids[i].AddTemperatureEmitter(_ac.TemperatureEmitter, onlyCosmetic: true);
			}
		}
	}

	public void RemoveFromPropery()
	{
		if (!((Object)(object)_currentProperty == (Object)null))
		{
			for (int i = 0; i < _currentProperty.Grids.Count; i++)
			{
				_currentProperty.Grids[i].RemoveTemperatureEmitter(_ac.TemperatureEmitter, onlyCosmetic: true);
			}
			_currentProperty = null;
		}
	}
}
