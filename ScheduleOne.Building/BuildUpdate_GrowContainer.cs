using ScheduleOne.EntityFramework;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Property;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Building;

public class BuildUpdate_GrowContainer : BuildUpdate_Grid
{
	private GrowContainer _gc;

	private static bool _showTemps = true;

	public override void Initialize(GridItem buildableItemClass, ItemInstance itemInstance, GameObject ghostModel)
	{
		base.Initialize(buildableItemClass, itemInstance, ghostModel);
		_gc = base.BuildableItemClass as GrowContainer;
		_gc.TemperatureDisplay.SetTemperatureGetter(GetTemperature);
		_gc.TemperatureDisplay.SetVisibilityGetter(GetTemperatureVisibility);
		_gc.TemperatureDisplay.SetEnabled(enabled: true);
		if (_showTemps && base.AllowToggleShowTemperatures)
		{
			_showTemperatures = true;
		}
	}

	private float GetTemperature()
	{
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < base.BuildableItemClass.CoordinateFootprintTilePairs.Count; i++)
		{
			Coordinate matchedCoordinate = base.closestIntersection.tile.OwnerGrid.GetMatchedCoordinate(base.BuildableItemClass.CoordinateFootprintTilePairs[i].footprintTile);
			Tile tile = base.closestIntersection.tile.OwnerGrid.GetTile(matchedCoordinate);
			if (!((Object)(object)tile == (Object)null))
			{
				num += tile.TileTemperature;
				num2++;
			}
		}
		if (num2 == 0)
		{
			return 0f;
		}
		return num / (float)num2;
	}

	private bool GetTemperatureVisibility()
	{
		if (_showTemperatures)
		{
			return _validPosition;
		}
		return false;
	}

	protected override void SetShowTemperatures(bool show, ScheduleOne.Property.Property property)
	{
		base.SetShowTemperatures(show, property);
		_showTemps = show;
	}
}
