using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Tiles;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScheduleOne.Storage;

public class StoredItem : MonoBehaviour
{
	[Header("References")]
	public Transform buildPoint;

	public List<CoordinateStorageFootprintTilePair> CoordinateFootprintTilePairs = new List<CoordinateStorageFootprintTilePair>();

	private int footprintX = -1;

	private int footprintY = -1;

	protected List<CoordinatePair> coordinatePairs = new List<CoordinatePair>();

	protected float rotation;

	public int xSize;

	public int ySize;

	public StorableItemInstance item { get; protected set; }

	public bool Destroyed { get; private set; }

	public FootprintTile OriginFootprint => CoordinateFootprintTilePairs[0].tile;

	public int FootprintX
	{
		get
		{
			if (footprintX == -1)
			{
				footprintX = CoordinateFootprintTilePairs.OrderByDescending((CoordinateStorageFootprintTilePair c) => c.coord.x).FirstOrDefault().coord.x + 1;
			}
			return footprintX;
		}
	}

	public int FootprintY
	{
		get
		{
			if (footprintY == -1)
			{
				footprintY = CoordinateFootprintTilePairs.OrderByDescending((CoordinateStorageFootprintTilePair c) => c.coord.y).FirstOrDefault().coord.y + 1;
			}
			return footprintY;
		}
	}

	public StorageGrid parentGrid { get; protected set; }

	public List<CoordinatePair> CoordinatePairs => coordinatePairs;

	public float Rotation => rotation;

	public int totalArea => CoordinateFootprintTilePairs.Count;

	protected virtual void Awake()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		MeshRenderer[] componentsInChildren = ((Component)this).GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if ((int)((Renderer)componentsInChildren[i]).shadowCastingMode == 3)
			{
				((Renderer)componentsInChildren[i]).enabled = false;
			}
			else
			{
				((Renderer)componentsInChildren[i]).shadowCastingMode = (ShadowCastingMode)0;
			}
		}
	}

	public virtual void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)grid == (Object)null)
		{
			Console.LogError("InitializeStoredItem: grid is null!");
			Destroy();
		}
		else
		{
			if ((Object)(object)this == (Object)null || (Object)(object)((Component)this).gameObject == (Object)null)
			{
				return;
			}
			item = _item;
			parentGrid = grid;
			rotation = _rotation;
			LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("StoredItem"));
			coordinatePairs = Coordinate.BuildCoordinateMatches(new Coordinate(_originCoordinate), FootprintX, FootprintY, Rotation);
			RefreshTransform();
			for (int i = 0; i < coordinatePairs.Count; i++)
			{
				StorageTile tile = parentGrid.GetTile(coordinatePairs[i].coord2);
				if ((Object)(object)tile == (Object)null)
				{
					Console.LogError("Failed to find tile at " + coordinatePairs[i].coord2?.ToString() + " when initializing stored item!");
					Destroy();
					return;
				}
				if ((Object)(object)tile.occupant != (Object)null)
				{
					Destroy();
					return;
				}
				tile.SetOccupant(this);
			}
			StoredItemRandomRotation storedItemRandomRotation = default(StoredItemRandomRotation);
			if (((Component)this).TryGetComponent<StoredItemRandomRotation>(ref storedItemRandomRotation))
			{
				storedItemRandomRotation.ApplyRotation();
			}
			SetFootprintTileVisiblity(visible: false);
		}
	}

	private void RefreshTransform()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		FootprintTile tile = GetTile(coordinatePairs[0].coord1);
		StorageTile tile2 = parentGrid.GetTile(coordinatePairs[0].coord2);
		((Component)this).transform.rotation = ((Component)parentGrid).transform.rotation * (Quaternion.Inverse(((Component)buildPoint).transform.rotation) * ((Component)this).transform.rotation);
		((Component)this).transform.Rotate(buildPoint.up, rotation + ((Component)tile2).transform.localEulerAngles.y);
		((Component)this).transform.position = ((Component)tile2).transform.position - (((Component)tile).transform.position - ((Component)this).transform.position);
	}

	public virtual void Destroy()
	{
		Destroyed = true;
		ClearFootprintOccupancy();
		if ((Object)(object)this != (Object)null && (Object)(object)((Component)this).gameObject != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	public void ClearFootprintOccupancy()
	{
		if ((Object)(object)parentGrid == (Object)null)
		{
			return;
		}
		for (int i = 0; i < coordinatePairs.Count; i++)
		{
			StorageTile tile = parentGrid.GetTile(coordinatePairs[i].coord2);
			if (!((Object)(object)tile == (Object)null))
			{
				tile.SetOccupant(null);
			}
		}
	}

	public void SetFootprintTileVisiblity(bool visible)
	{
		for (int i = 0; i < CoordinateFootprintTilePairs.Count; i++)
		{
			CoordinateFootprintTilePairs[i].tile.tileAppearance.SetVisible(visible);
		}
	}

	public void CalculateFootprintTileIntersections()
	{
		for (int i = 0; i < CoordinateFootprintTilePairs.Count; i++)
		{
			CoordinateFootprintTilePairs[i].tile.tileDetector.CheckIntersections();
		}
	}

	public FootprintTile GetTile(Coordinate coord)
	{
		for (int i = 0; i < CoordinateFootprintTilePairs.Count; i++)
		{
			if (CoordinateFootprintTilePairs[i].coord.Equals(coord))
			{
				return CoordinateFootprintTilePairs[i].tile;
			}
		}
		return null;
	}
}
