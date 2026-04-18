using System;
using System.Collections.Generic;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Storage;

public class StorageGrid : MonoBehaviour
{
	public static float gridSize = 0.25f;

	public List<StorageTile> storageTiles = new List<StorageTile>();

	[HideInInspector]
	public List<CoordinateStorageTilePair> coordinateStorageTilePairs = new List<CoordinateStorageTilePair>();

	private int _unoccupiedTileCount = -1;

	private bool _unoccupiedTileCountDirty = true;

	public int UnoccupiedTileCount
	{
		get
		{
			if (_unoccupiedTileCountDirty)
			{
				_unoccupiedTileCount = CalculateUnoccupiedTileCount();
				_unoccupiedTileCountDirty = false;
			}
			return _unoccupiedTileCount;
		}
	}

	private void Awake()
	{
		for (int i = 0; i < storageTiles.Count; i++)
		{
			StorageTile storageTile = storageTiles[i];
			storageTile.onOccupantChanged = (Action)Delegate.Combine(storageTile.onOccupantChanged, new Action(TileOccupantChanged));
		}
		_unoccupiedTileCountDirty = true;
	}

	public void RegisterTile(StorageTile tile)
	{
		storageTiles.Add(tile);
		CoordinateStorageTilePair item = new CoordinateStorageTilePair
		{
			coord = new Coordinate(tile.x, tile.y),
			tile = tile
		};
		coordinateStorageTilePairs.Add(item);
	}

	public void DeregisterTile(StorageTile tile)
	{
		storageTiles.Remove(tile);
		for (int i = 0; i < coordinateStorageTilePairs.Count; i++)
		{
			if ((Object)(object)coordinateStorageTilePairs[i].tile == (Object)(object)tile)
			{
				coordinateStorageTilePairs.RemoveAt(i);
				i--;
				break;
			}
		}
	}

	public Coordinate GetMatchedCoordinate(FootprintTile tileToMatch)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.InverseTransformPoint(((Component)tileToMatch).transform.position);
		return new Coordinate(Mathf.RoundToInt(val.x / gridSize), Mathf.RoundToInt(val.z / gridSize));
	}

	public StorageTile GetTile(Coordinate coord)
	{
		for (int i = 0; i < coordinateStorageTilePairs.Count; i++)
		{
			if (coordinateStorageTilePairs[i].coord.Equals(coord))
			{
				return coordinateStorageTilePairs[i].tile;
			}
		}
		return null;
	}

	public int GetUserEndCapacity()
	{
		int actualY = GetActualY();
		int num = coordinateStorageTilePairs.Count / actualY;
		return (actualY - 1) * (num - 1);
	}

	public int GetActualY()
	{
		int result = 0;
		int num = 0;
		while (num < coordinateStorageTilePairs.Count)
		{
			if (coordinateStorageTilePairs[num].coord.x == 0)
			{
				num++;
				num++;
				continue;
			}
			result = num;
			break;
		}
		return result;
	}

	public int GetActualX()
	{
		return coordinateStorageTilePairs.Count / GetActualY();
	}

	public int GetTotalFootprintSize()
	{
		return coordinateStorageTilePairs.Count;
	}

	public bool TryFitItem(int sizeX, int sizeY, List<Coordinate> lockedCoordinates, out Coordinate originCoordinate, out float rotation)
	{
		originCoordinate = new Coordinate(0, 0);
		rotation = 0f;
		if (sizeX * sizeY > UnoccupiedTileCount)
		{
			return false;
		}
		foreach (CoordinateStorageTilePair coordinateStorageTilePair in coordinateStorageTilePairs)
		{
			if ((Object)(object)coordinateStorageTilePair.tile.occupant != (Object)null)
			{
				continue;
			}
			originCoordinate = coordinateStorageTilePair.coord;
			bool flag = true;
			rotation = 0f;
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					Coordinate coordinate = new Coordinate(coordinateStorageTilePair.tile.x + i, coordinateStorageTilePair.tile.y + j);
					for (int k = 0; k < lockedCoordinates.Count; k++)
					{
						if (coordinate.Equals(lockedCoordinates[k]))
						{
							flag = false;
						}
					}
					StorageTile tile = GetTile(coordinate);
					if ((Object)(object)tile == (Object)null || (Object)(object)tile.occupant != (Object)null)
					{
						flag = false;
					}
				}
			}
			if (flag)
			{
				return true;
			}
			flag = true;
			rotation = 90f;
			for (int l = 0; l < sizeX; l++)
			{
				for (int m = 0; m < sizeY; m++)
				{
					Coordinate coordinate2 = new Coordinate(coordinateStorageTilePair.tile.x + m, coordinateStorageTilePair.tile.y - l);
					for (int n = 0; n < lockedCoordinates.Count; n++)
					{
						if (coordinate2.Equals(lockedCoordinates[n]))
						{
							flag = false;
						}
					}
					StorageTile tile2 = GetTile(coordinate2);
					if ((Object)(object)tile2 == (Object)null || (Object)(object)tile2.occupant != (Object)null)
					{
						flag = false;
					}
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private int CalculateUnoccupiedTileCount()
	{
		int num = 0;
		for (int i = 0; i < storageTiles.Count; i++)
		{
			if ((Object)(object)storageTiles[i].occupant == (Object)null)
			{
				num++;
			}
		}
		return num;
	}

	private void TileOccupantChanged()
	{
		_unoccupiedTileCountDirty = true;
	}
}
