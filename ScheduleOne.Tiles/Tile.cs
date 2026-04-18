using System;
using System.Collections.Generic;
using ScheduleOne.EntityFramework;
using ScheduleOne.Lighting;
using ScheduleOne.Property;
using ScheduleOne.Temperature;
using UnityEngine;

namespace ScheduleOne.Tiles;

[Serializable]
public class Tile : MonoBehaviour
{
	public delegate void TileChange(Tile thisTile);

	public int x;

	public int y;

	[Header("Settings")]
	public float AvailableOffset = 1000f;

	[Header("References")]
	public Grid OwnerGrid;

	public LightExposureNode LightExposureNode;

	[Header("Occupants")]
	public List<GridItem> BuildableOccupants = new List<GridItem>();

	public List<FootprintTile> OccupantTiles = new List<FootprintTile>();

	public TileChange onTileChanged;

	public Action<Tile, float> onTileTemperatureChanged;

	private float _cosmeticTileTemperature = 20f;

	private TemperatureEmitterInfo[] _cachedCosmeticTemperatureEmitters;

	private float _tileTemperature = 20f;

	private TemperatureEmitterInfo[] _cachedTemperatureEmitters;

	public float CosmeticTileTemperature
	{
		get
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			if (_cachedCosmeticTemperatureEmitters != null)
			{
				_cosmeticTileTemperature = TemperatureAlgorithm.GetTemperatureAtPoint(OwnerGrid.ParentProperty.AmbientTemperature, OwnerGrid.Origin, ((Component)this).transform.position, _cachedCosmeticTemperatureEmitters);
				_cachedCosmeticTemperatureEmitters = null;
			}
			return _cosmeticTileTemperature;
		}
	}

	public float TileTemperature
	{
		get
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			if (_cachedTemperatureEmitters != null)
			{
				_tileTemperature = TemperatureAlgorithm.GetTemperatureAtPoint(OwnerGrid.ParentProperty.AmbientTemperature, OwnerGrid.Origin, ((Component)this).transform.position, _cachedTemperatureEmitters);
				_cachedTemperatureEmitters = null;
				if (onTileTemperatureChanged != null)
				{
					onTileTemperatureChanged(this, _tileTemperature);
				}
			}
			return _tileTemperature;
		}
	}

	public void InitializePropertyTile(int _x, int _y, float _available_Offset, Grid _ownerGrid)
	{
		x = _x;
		y = _y;
		AvailableOffset = _available_Offset;
		OwnerGrid = _ownerGrid;
	}

	private void Awake()
	{
		Grid ownerGrid = OwnerGrid;
		ownerGrid.OnCosmeticTemperatureEmittersChanged = (Action<string, TemperatureEmitterInfo[]>)Delegate.Combine(ownerGrid.OnCosmeticTemperatureEmittersChanged, new Action<string, TemperatureEmitterInfo[]>(OnCosmeticTemperatureEmittersChanged));
		Grid ownerGrid2 = OwnerGrid;
		ownerGrid2.OnTemperatureEmittersChanged = (Action<TemperatureEmitterInfo[]>)Delegate.Combine(ownerGrid2.OnTemperatureEmittersChanged, new Action<TemperatureEmitterInfo[]>(OnTemperatureEmittersChanged));
		_cosmeticTileTemperature = OwnerGrid.ParentProperty.AmbientTemperature;
		_tileTemperature = OwnerGrid.ParentProperty.AmbientTemperature;
	}

	public void AddOccupant(GridItem occ, FootprintTile tile)
	{
		BuildableOccupants.Remove(occ);
		BuildableOccupants.Add(occ);
		OccupantTiles.Remove(tile);
		OccupantTiles.Add(tile);
		if (onTileChanged != null)
		{
			onTileChanged(this);
		}
	}

	public void RemoveOccupant(GridItem occ, FootprintTile tile)
	{
		BuildableOccupants.Remove(occ);
		OccupantTiles.Remove(tile);
		if (onTileChanged != null)
		{
			onTileChanged(this);
		}
	}

	public virtual bool CanBeBuiltOn()
	{
		if ((Object)(object)((Component)OwnerGrid).GetComponentInParent<ScheduleOne.Property.Property>() != (Object)null && !((Component)OwnerGrid).GetComponentInParent<ScheduleOne.Property.Property>().IsOwned)
		{
			return false;
		}
		return true;
	}

	public List<Tile> GetSurroundingTiles()
	{
		List<Tile> list = new List<Tile>();
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Tile tile = OwnerGrid.GetTile(new Coordinate(x + i - 1, y + j - 1));
				if ((Object)(object)tile != (Object)null && (Object)(object)tile != (Object)(object)this && !list.Contains(tile))
				{
					list.Add(tile);
				}
			}
		}
		return list;
	}

	public virtual bool IsIndoorTile()
	{
		return false;
	}

	public void SetVisible(bool vis)
	{
		((Component)((Component)this).transform.Find("Model")).gameObject.SetActive(vis);
	}

	private void OnCosmeticTemperatureEmittersChanged(string propertyCode, TemperatureEmitterInfo[] emitters)
	{
		_cachedCosmeticTemperatureEmitters = emitters;
	}

	private void OnTemperatureEmittersChanged(TemperatureEmitterInfo[] emitters)
	{
		_cachedTemperatureEmitters = emitters;
	}
}
