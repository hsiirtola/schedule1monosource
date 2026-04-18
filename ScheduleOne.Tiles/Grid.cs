using System;
using System.Collections.Generic;
using ScheduleOne.Core;
using ScheduleOne.EntityFramework;
using ScheduleOne.Property;
using ScheduleOne.Temperature;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Tiles;

public class Grid : MonoBehaviour, IGUIDRegisterable
{
	public const float TileSize = 0.5f;

	public List<Tile> Tiles = new List<Tile>();

	public List<CoordinateTilePair> CoordinateTilePairs = new List<CoordinateTilePair>();

	[SerializeField]
	private ScheduleOne.Property.Property _parentProperty;

	[FormerlySerializedAs("StaticGUID")]
	[SerializeField]
	private string _guid = string.Empty;

	public Action<string, TemperatureEmitterInfo[]> OnCosmeticTemperatureEmittersChanged;

	public Action<TemperatureEmitterInfo[]> OnTemperatureEmittersChanged;

	protected Dictionary<Coordinate, Tile> _coordinateToTile = new Dictionary<Coordinate, Tile>();

	protected List<TemperatureEmitter> _cosmeticTemperatureEmitters = new List<TemperatureEmitter>();

	protected List<TemperatureEmitter> _temperatureEmitters = new List<TemperatureEmitter>();

	private bool _cosmeticEmittersChangedThisFrame;

	private bool _emittersChangedThisFrame;

	public Guid GUID { get; protected set; }

	public ScheduleOne.Property.Property ParentProperty => _parentProperty;

	public Transform Container => ((Component)ParentProperty.Container).transform;

	public Vector3 Origin => ((Component)this).transform.position;

	public TemperatureEmitterInfo[] TemperatureEmitterInfos { get; private set; }

	public int Width { get; private set; }

	public int Height { get; private set; }

	protected virtual void Awake()
	{
		if (!GUIDManager.IsGUIDValid(_guid))
		{
			Console.LogError("Static GUID is not valid.");
		}
		if ((Object)(object)ParentProperty == (Object)null)
		{
			Console.LogWarning("Parent property not assigned, attempting to auto-assign.");
			_parentProperty = ((Component)this).GetComponentInParent<ScheduleOne.Property.Property>();
		}
		_parentProperty.Grids.Add(this);
		((IGUIDRegisterable)this).SetGUID(_guid);
		SetInvisible();
		ProcessCoordinateDataPairs();
		SetGridSize();
	}

	private void LateUpdate()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (_cosmeticEmittersChangedThisFrame)
		{
			_cosmeticEmittersChangedThisFrame = false;
			TemperatureEmitterInfos = new TemperatureEmitterInfo[_cosmeticTemperatureEmitters.Count];
			for (int i = 0; i < _cosmeticTemperatureEmitters.Count; i++)
			{
				TemperatureEmitterInfos[i] = new TemperatureEmitterInfo(_cosmeticTemperatureEmitters[i].Temperature, _cosmeticTemperatureEmitters[i].Range * _cosmeticTemperatureEmitters[i].Range, _cosmeticTemperatureEmitters[i].EmissionPoint);
			}
			if (OnCosmeticTemperatureEmittersChanged != null)
			{
				OnCosmeticTemperatureEmittersChanged(_parentProperty.PropertyCode, TemperatureEmitterInfos);
			}
		}
		if (_emittersChangedThisFrame)
		{
			_emittersChangedThisFrame = false;
			TemperatureEmitterInfos = new TemperatureEmitterInfo[_temperatureEmitters.Count];
			for (int j = 0; j < _temperatureEmitters.Count; j++)
			{
				TemperatureEmitterInfos[j] = new TemperatureEmitterInfo(_temperatureEmitters[j].Temperature, _temperatureEmitters[j].Range * _temperatureEmitters[j].Range, _temperatureEmitters[j].EmissionPoint);
			}
			if (OnTemperatureEmittersChanged != null)
			{
				OnTemperatureEmittersChanged(TemperatureEmitterInfos);
			}
		}
	}

	private void ProcessCoordinateDataPairs()
	{
		foreach (CoordinateTilePair coordinateTilePair in CoordinateTilePairs)
		{
			_coordinateToTile.Add(coordinateTilePair.coord, coordinateTilePair.tile);
		}
	}

	public void RegisterTile(Tile tile)
	{
		Tiles.Add(tile);
		CoordinateTilePair item = new CoordinateTilePair
		{
			coord = new Coordinate(tile.x, tile.y),
			tile = tile
		};
		CoordinateTilePairs.Add(item);
	}

	public void DeregisterTile(Tile tile)
	{
		Console.Log("Deregistering tile: " + tile.x + ", " + tile.y);
		Tiles.Remove(tile);
		for (int i = 0; i < CoordinateTilePairs.Count; i++)
		{
			if ((Object)(object)CoordinateTilePairs[i].tile == (Object)(object)tile)
			{
				CoordinateTilePairs.RemoveAt(i);
				i--;
				break;
			}
		}
	}

	[Button]
	public void RegenerateGUID()
	{
		_guid = Guid.NewGuid().ToString();
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	public Coordinate GetMatchedCoordinate(FootprintTile tileToMatch)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.InverseTransformPoint(((Component)tileToMatch).transform.position);
		return new Coordinate(Mathf.RoundToInt(val.x / 0.5f), Mathf.RoundToInt(val.z / 0.5f));
	}

	public bool IsTileValidAtCoordinate(Coordinate gridCoord, FootprintTile tile, GridItem tileOwner = null)
	{
		if (!_coordinateToTile.ContainsKey(gridCoord))
		{
			return false;
		}
		Tile tile2 = _coordinateToTile[gridCoord];
		if (tile2.BuildableOccupants.Count > 0 && ((Object)(object)tileOwner == (Object)null || !tileOwner.CanShareTileWith(tile2.BuildableOccupants)))
		{
			return false;
		}
		if (tile2.AvailableOffset != 0f && tile.RequiredOffset != 0f && tile2.AvailableOffset < tile.RequiredOffset)
		{
			return false;
		}
		return tile2.CanBeBuiltOn();
	}

	public Tile GetTile(Coordinate coord)
	{
		if (!_coordinateToTile.ContainsKey(coord))
		{
			return null;
		}
		return _coordinateToTile[coord];
	}

	[Button]
	public void SetVisible()
	{
		for (int i = 0; i < CoordinateTilePairs.Count; i++)
		{
			CoordinateTilePairs[i].tile.SetVisible(vis: true);
		}
	}

	[Button]
	public void SetInvisible()
	{
		for (int i = 0; i < CoordinateTilePairs.Count; i++)
		{
			CoordinateTilePairs[i].tile.SetVisible(vis: false);
		}
	}

	public void AddTemperatureEmitter(TemperatureEmitter emitter, bool onlyCosmetic)
	{
		if (!_cosmeticTemperatureEmitters.Contains(emitter))
		{
			_cosmeticTemperatureEmitters.Add(emitter);
			emitter.OnEmitterChanged = (Action)Delegate.Combine(emitter.OnEmitterChanged, new Action(CosmeticTemperatureEmittersChanged));
			CosmeticTemperatureEmittersChanged();
			if (!onlyCosmetic)
			{
				_temperatureEmitters.Add(emitter);
				emitter.OnEmitterChanged = (Action)Delegate.Combine(emitter.OnEmitterChanged, new Action(TemperatureEmittersChanged));
				TemperatureEmittersChanged();
			}
		}
	}

	public void RemoveTemperatureEmitter(TemperatureEmitter emitter, bool onlyCosmetic)
	{
		if (_cosmeticTemperatureEmitters.Contains(emitter))
		{
			_cosmeticTemperatureEmitters.Remove(emitter);
			emitter.OnEmitterChanged = (Action)Delegate.Remove(emitter.OnEmitterChanged, new Action(CosmeticTemperatureEmittersChanged));
			CosmeticTemperatureEmittersChanged();
			if (!onlyCosmetic)
			{
				_temperatureEmitters.Remove(emitter);
				emitter.OnEmitterChanged = (Action)Delegate.Remove(emitter.OnEmitterChanged, new Action(TemperatureEmittersChanged));
				TemperatureEmittersChanged();
			}
		}
	}

	private void CosmeticTemperatureEmittersChanged()
	{
		_cosmeticEmittersChangedThisFrame = true;
	}

	private void TemperatureEmittersChanged()
	{
		_emittersChangedThisFrame = true;
	}

	private void SetGridSize()
	{
		int num = int.MinValue;
		int num2 = int.MinValue;
		foreach (Tile tile in Tiles)
		{
			if (tile.x > num)
			{
				num = tile.x;
			}
			if (tile.y > num2)
			{
				num2 = tile.y;
			}
		}
		Width = num + 1;
		Height = num2 + 1;
	}
}
