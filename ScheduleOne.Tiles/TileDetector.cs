using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Tiles;

public class TileDetector : MonoBehaviour
{
	public float detectionRadius = 0.25f;

	public ETileDetectionMode tileDetectionMode;

	public List<Tile> intersectedTiles = new List<Tile>();

	public List<Tile> intersectedOutdoorTiles = new List<Tile>();

	public List<Tile> intersectedIndoorTiles = new List<Tile>();

	public List<StorageTile> intersectedStorageTiles = new List<StorageTile>();

	public List<ProceduralTile> intersectedProceduralTiles = new List<ProceduralTile>();

	public virtual void CheckIntersections(bool sort = true)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		intersectedTiles.Clear();
		intersectedOutdoorTiles.Clear();
		intersectedIndoorTiles.Clear();
		intersectedStorageTiles.Clear();
		intersectedProceduralTiles.Clear();
		LayerMask val = LayerMask.op_Implicit(LayerMask.op_Implicit(default(LayerMask)) | (1 << LayerMask.NameToLayer("Tile")));
		Collider[] array = Physics.OverlapSphere(((Component)this).transform.position, detectionRadius, LayerMask.op_Implicit(val));
		for (int i = 0; i < array.Length; i++)
		{
			if (tileDetectionMode == ETileDetectionMode.Tile)
			{
				Tile componentInParent = ((Component)array[i]).GetComponentInParent<Tile>();
				if ((Object)(object)componentInParent != (Object)null && !intersectedTiles.Contains(componentInParent))
				{
					intersectedTiles.Add(componentInParent);
				}
			}
			if (tileDetectionMode == ETileDetectionMode.OutdoorTile)
			{
				Tile componentInParent2 = ((Component)array[i]).GetComponentInParent<Tile>();
				if ((Object)(object)componentInParent2 != (Object)null && !(componentInParent2 is IndoorTile) && !intersectedOutdoorTiles.Contains(componentInParent2))
				{
					intersectedOutdoorTiles.Add(componentInParent2);
				}
			}
			if (tileDetectionMode == ETileDetectionMode.IndoorTile)
			{
				IndoorTile componentInParent3 = ((Component)array[i]).GetComponentInParent<IndoorTile>();
				if ((Object)(object)componentInParent3 != (Object)null && !intersectedIndoorTiles.Contains(componentInParent3))
				{
					intersectedIndoorTiles.Add(componentInParent3);
				}
			}
			if (tileDetectionMode == ETileDetectionMode.StorageTile)
			{
				StorageTile componentInParent4 = ((Component)array[i]).GetComponentInParent<StorageTile>();
				if ((Object)(object)componentInParent4 != (Object)null && !intersectedStorageTiles.Contains(componentInParent4))
				{
					intersectedStorageTiles.Add(componentInParent4);
				}
			}
			if (tileDetectionMode == ETileDetectionMode.ProceduralTile)
			{
				ProceduralTile componentInParent5 = ((Component)array[i]).GetComponentInParent<ProceduralTile>();
				if ((Object)(object)componentInParent5 != (Object)null && !intersectedProceduralTiles.Contains(componentInParent5))
				{
					intersectedProceduralTiles.Add(componentInParent5);
				}
			}
		}
		if (sort)
		{
			intersectedTiles = OrderList(intersectedTiles);
			intersectedOutdoorTiles = OrderList(intersectedOutdoorTiles);
			intersectedIndoorTiles = OrderList(intersectedIndoorTiles);
			intersectedStorageTiles = OrderList(intersectedStorageTiles);
			intersectedProceduralTiles = OrderList(intersectedProceduralTiles);
		}
	}

	public List<T> OrderList<T>(List<T> list) where T : MonoBehaviour
	{
		return list.OrderBy((T x) => Vector3.Distance(((Component)(object)x).transform.position, ((Component)this).transform.position)).ToList();
	}

	public Tile GetClosestTile()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Tile result = null;
		float num = 100f;
		for (int i = 0; i < intersectedTiles.Count; i++)
		{
			if (Vector3.Distance(((Component)intersectedTiles[i]).transform.position, ((Component)this).transform.position) < num)
			{
				result = intersectedTiles[i];
				num = Vector3.Distance(((Component)intersectedTiles[i]).transform.position, ((Component)this).transform.position);
			}
		}
		return result;
	}

	public ProceduralTile GetClosestProceduralTile()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		ProceduralTile result = null;
		float num = 100f;
		for (int i = 0; i < intersectedProceduralTiles.Count; i++)
		{
			if (Vector3.Distance(((Component)intersectedProceduralTiles[i]).transform.position, ((Component)this).transform.position) < num)
			{
				result = intersectedProceduralTiles[i];
				num = Vector3.Distance(((Component)intersectedProceduralTiles[i]).transform.position, ((Component)this).transform.position);
			}
		}
		return result;
	}
}
