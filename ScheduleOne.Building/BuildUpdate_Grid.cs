using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Heatmap;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.Temperature;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Building;

public class BuildUpdate_Grid : BuildUpdate_Base
{
	[Header("Settings")]
	public float detectionRange = 6f;

	public LayerMask detectionMask;

	public float rotation_Smoothing = 5f;

	public bool AllowRotation = true;

	[Header("Temperature")]
	[SerializeField]
	private bool showTemperaturesByDefault;

	[SerializeField]
	private bool allowToggleShowTemperatures;

	protected bool _validPosition;

	protected Material _currentGhostMaterial;

	protected float _rotation;

	private TileIntersection _closestIntersection;

	private float verticalOffset;

	protected bool _showTemperatures;

	public GameObject GhostModel { get; private set; }

	public GridItem BuildableItemClass { get; private set; }

	public ItemInstance ItemInstance { get; private set; }

	public bool AllowToggleShowTemperatures
	{
		get
		{
			if (allowToggleShowTemperatures)
			{
				return TemperatureUtility.TemperatureSystemEnabled;
			}
			return false;
		}
	}

	protected TileIntersection closestIntersection
	{
		get
		{
			return _closestIntersection;
		}
		set
		{
			if (_closestIntersection != value)
			{
				OnClosestIntersectionChanged(_closestIntersection, value);
			}
			_closestIntersection = value;
		}
	}

	public virtual void Initialize(GridItem buildableItemClass, ItemInstance itemInstance, GameObject ghostModel)
	{
		BuildableItemClass = buildableItemClass;
		ItemInstance = itemInstance;
		GhostModel = ghostModel;
		_showTemperatures = showTemperaturesByDefault && AllowToggleShowTemperatures;
	}

	protected virtual void Start()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		LateUpdate();
		if (closestIntersection != null)
		{
			Vector3 forward = ((Component)closestIntersection.tile.OwnerGrid).transform.forward;
			Vector3 val = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position - ((Component)BuildableItemClass.BuildPoint).transform.position;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			normalized.y = 0f;
			float num = Vector3.SignedAngle(forward, normalized, Vector3.up);
			Debug.DrawRay(((Component)BuildableItemClass.BuildPoint).transform.position, forward, Color.red, 5f);
			Debug.DrawRay(((Component)BuildableItemClass.BuildPoint).transform.position, normalized, Color.green, 5f);
			float num2 = 90f;
			float rotation = (float)(int)Mathf.Round(num / num2) * num2;
			_rotation = rotation;
		}
	}

	protected virtual void Update()
	{
		CheckRotation();
		CheckToggleTemperatureDisplay();
		if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && _validPosition)
		{
			Place();
		}
	}

	private void CheckToggleTemperatureDisplay()
	{
		if (AllowToggleShowTemperatures && GameInput.GetButtonDown(GameInput.ButtonCode.VehicleToggleLights) && !GameInput.IsTyping)
		{
			ScheduleOne.Property.Property property = null;
			if (closestIntersection != null && (Object)(object)closestIntersection.tile != (Object)null)
			{
				property = closestIntersection.tile.OwnerGrid.ParentProperty;
			}
			SetShowTemperatures(!_showTemperatures, property);
		}
	}

	protected virtual void LateUpdate()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		_validPosition = false;
		GhostModel.transform.up = Vector3.up;
		float holdDistance = BuildableItemClass.HoldDistance;
		float num = (Mathf.Clamp(Vector3.Angle(Vector3.down, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward), 45f, 90f) - 45f) / 45f;
		float num2 = holdDistance * (1f + num);
		PositionObjectInFrontOfPlayer(num2, sanitizeForward: true, buildPointAsOrigin: true);
		if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast_ExcludeBuildables(num2, out var hit, detectionMask))
		{
			ApplyRotation();
		}
		if (PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(((Component)BuildableItemClass).transform.position + Vector3.up * 0.1f, Vector3.down, 3f, out hit, detectionMask, includeTriggers: false, 0f, 45f))
		{
			GhostModel.transform.position = ((RaycastHit)(ref hit)).point - GhostModel.transform.InverseTransformPoint(((Component)BuildableItemClass.BuildPoint).transform.position);
		}
		ApplyRotation();
		if ((!Application.isEditor || !Input.GetKey((KeyCode)308)) && BuildableItemClass.GetPenetration(out var x, out var z, out var y))
		{
			if (Vector3.Distance(GhostModel.transform.position - GhostModel.transform.right * x, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position) < Vector3.Distance(GhostModel.transform.position - GhostModel.transform.forward * z, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position))
			{
				Transform transform = GhostModel.transform;
				transform.position -= GhostModel.transform.right * x;
				if (BuildableItemClass.GetPenetration(out x, out z, out y))
				{
					Transform transform2 = GhostModel.transform;
					transform2.position -= GhostModel.transform.forward * z;
				}
			}
			else
			{
				Transform transform3 = GhostModel.transform;
				transform3.position -= GhostModel.transform.forward * z;
				if (BuildableItemClass.GetPenetration(out x, out z, out y))
				{
					Transform transform4 = GhostModel.transform;
					transform4.position -= GhostModel.transform.right * x;
				}
			}
			Transform transform5 = GhostModel.transform;
			transform5.position -= GhostModel.transform.up * y;
		}
		BuildableItemClass.CalculateFootprintTileIntersections();
		CheckIntersections();
		if (_validPosition)
		{
			verticalOffset = Mathf.MoveTowards(verticalOffset, 0f, Time.deltaTime * 1f);
		}
		else
		{
			verticalOffset = Mathf.MoveTowards(verticalOffset, 0.1f, Time.deltaTime * 1f);
		}
		Transform transform6 = ((Component)BuildableItemClass).transform;
		transform6.position += Vector3.up * verticalOffset;
		UpdateMaterials();
	}

	protected void PositionObjectInFrontOfPlayer(float dist, bool sanitizeForward, bool buildPointAsOrigin)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward;
		if (sanitizeForward)
		{
			forward.y = 0f;
		}
		Vector3 position = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position + forward * dist;
		GhostModel.transform.position = position;
		if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast_ExcludeBuildables(dist, out var hit, detectionMask))
		{
			GhostModel.transform.position = ((RaycastHit)(ref hit)).point;
			if (buildPointAsOrigin && Vector3.Angle(((RaycastHit)(ref hit)).normal, Vector3.up) < 1f)
			{
				Transform transform = GhostModel.transform;
				transform.position += -GhostModel.transform.InverseTransformPoint(((Component)BuildableItemClass.BuildPoint).transform.position);
			}
			else if ((Object)(object)BuildableItemClass.MidAirCenterPoint != (Object)null)
			{
				Transform transform2 = GhostModel.transform;
				transform2.position += -GhostModel.transform.InverseTransformPoint(((Component)BuildableItemClass.MidAirCenterPoint).transform.position);
			}
		}
		else if ((Object)(object)BuildableItemClass.MidAirCenterPoint != (Object)null)
		{
			Transform transform3 = GhostModel.transform;
			transform3.position += -GhostModel.transform.InverseTransformPoint(((Component)BuildableItemClass.MidAirCenterPoint).transform.position);
		}
	}

	protected void CheckRotation()
	{
		if (!AllowRotation)
		{
			_rotation = 0f;
			return;
		}
		if (GameInput.GetButtonDown(GameInput.ButtonCode.RotateLeft) && !GameInput.IsTyping)
		{
			_rotation -= 90f;
		}
		if (GameInput.GetButtonDown(GameInput.ButtonCode.RotateRight) && !GameInput.IsTyping)
		{
			_rotation += 90f;
		}
	}

	protected void ApplyRotation()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		GhostModel.transform.rotation = Quaternion.Inverse(((Component)BuildableItemClass.BuildPoint).transform.rotation) * GhostModel.transform.rotation;
		Grid hoveredGrid = GetHoveredGrid();
		float num = _rotation;
		if ((Object)(object)hoveredGrid != (Object)null)
		{
			num += ((Component)hoveredGrid).transform.eulerAngles.y;
		}
		GhostModel.transform.Rotate(BuildableItemClass.BuildPoint.up, num);
	}

	private List<TileIntersection> GetRelevantIntersections(FootprintTile tile)
	{
		List<TileIntersection> list = new List<TileIntersection>();
		List<Tile> intersectedTiles = tile.tileDetector.intersectedTiles;
		for (int i = 0; i < intersectedTiles.Count; i++)
		{
			TileIntersection tileIntersection = new TileIntersection();
			tileIntersection.footprint = tile;
			tileIntersection.tile = intersectedTiles[i];
			list.Add(tileIntersection);
		}
		return list;
	}

	protected virtual void CheckIntersections()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		List<TileIntersection> list = new List<TileIntersection>();
		for (int i = 0; i < BuildableItemClass.CoordinateFootprintTilePairs.Count; i++)
		{
			list.AddRange(GetRelevantIntersections(BuildableItemClass.CoordinateFootprintTilePairs[i].footprintTile));
		}
		if (list.Count == 0 || (Application.isEditor && Input.GetKey((KeyCode)306)))
		{
			BuildableItemClass.SetFootprintTileVisiblity(visible: false);
			closestIntersection = null;
			return;
		}
		BuildableItemClass.SetFootprintTileVisiblity(visible: true);
		float num = 100f;
		TileIntersection tileIntersection = null;
		for (int j = 0; j < list.Count; j++)
		{
			if (Vector3.Distance(((Component)list[j].tile).transform.position, ((Component)list[j].footprint).transform.position) < num)
			{
				num = Vector3.Distance(((Component)list[j].tile).transform.position, ((Component)list[j].footprint).transform.position);
				tileIntersection = list[j];
			}
		}
		closestIntersection = tileIntersection;
		List<Vector2> list2 = new List<Vector2>();
		GhostModel.transform.position = ((Component)closestIntersection.tile).transform.position + (GhostModel.transform.position - ((Component)closestIntersection.footprint).transform.position);
		_validPosition = true;
		for (int k = 0; k < BuildableItemClass.CoordinateFootprintTilePairs.Count; k++)
		{
			Coordinate matchedCoordinate = closestIntersection.tile.OwnerGrid.GetMatchedCoordinate(BuildableItemClass.CoordinateFootprintTilePairs[k].footprintTile);
			BuildableItemClass.CoordinateFootprintTilePairs[k].footprintTile.tileAppearance.SetColor(ETileColor.Red);
			if ((Object)(object)closestIntersection.tile.OwnerGrid.GetTile(matchedCoordinate) == (Object)null)
			{
				_validPosition = false;
				continue;
			}
			list2.Add(new Vector2((float)matchedCoordinate.x, (float)matchedCoordinate.y));
			if (BuildableItemClass.CoordinateFootprintTilePairs[k].footprintTile.AreCornerObstaclesBlocked(closestIntersection.tile.OwnerGrid.GetTile(matchedCoordinate)))
			{
				_validPosition = false;
			}
			else if (closestIntersection.tile.OwnerGrid.IsTileValidAtCoordinate(matchedCoordinate, BuildableItemClass.CoordinateFootprintTilePairs[k].footprintTile, BuildableItemClass))
			{
				BuildableItemClass.CoordinateFootprintTilePairs[k].footprintTile.tileAppearance.SetColor(ETileColor.White);
			}
			else
			{
				_validPosition = false;
			}
		}
		for (int l = 0; l < BuildableItemClass.CoordinateFootprintTilePairs.Count; l++)
		{
			Coordinate matchedCoordinate2 = closestIntersection.tile.OwnerGrid.GetMatchedCoordinate(BuildableItemClass.CoordinateFootprintTilePairs[l].footprintTile);
			Tile tile = closestIntersection.tile.OwnerGrid.GetTile(matchedCoordinate2);
			if (!((Object)(object)tile != (Object)null))
			{
				continue;
			}
			for (int m = 0; m < tile.OccupantTiles.Count; m++)
			{
				for (int n = 0; n < tile.OccupantTiles[m].Corners.Count; n++)
				{
					if (!tile.OccupantTiles[m].Corners[n].obstacleEnabled)
					{
						continue;
					}
					List<Tile> neighbourTiles = tile.OccupantTiles[m].Corners[n].GetNeighbourTiles(tile);
					int num2 = 0;
					foreach (Tile item in neighbourTiles)
					{
						if (list2.Contains(new Vector2((float)item.x, (float)item.y)))
						{
							num2++;
						}
					}
					if (num2 == 4)
					{
						_validPosition = false;
						for (int num3 = 0; num3 < BuildableItemClass.CoordinateFootprintTilePairs.Count; num3++)
						{
							BuildableItemClass.CoordinateFootprintTilePairs[num3].footprintTile.tileAppearance.SetColor(ETileColor.Red);
						}
						return;
					}
				}
			}
		}
	}

	protected void UpdateMaterials()
	{
		Material val = NetworkSingleton<BuildManager>.Instance.ghostMaterial_White;
		if (!_validPosition)
		{
			val = NetworkSingleton<BuildManager>.Instance.ghostMaterial_Red;
		}
		if ((Object)(object)_currentGhostMaterial != (Object)(object)val)
		{
			_currentGhostMaterial = val;
			NetworkSingleton<BuildManager>.Instance.ApplyMaterial(GhostModel, val);
		}
	}

	protected virtual GridItem Place()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		int rotation = Mathf.RoundToInt(Vector3.SignedAngle(((Component)closestIntersection.tile.OwnerGrid).transform.forward, BuildableItemClass.BuildPoint.forward, ((Component)closestIntersection.tile.OwnerGrid).transform.up));
		GridItem result = NetworkSingleton<BuildManager>.Instance.CreateGridItem(ItemInstance.GetCopy(1), closestIntersection.tile.OwnerGrid, GetOriginCoordinate(), rotation, string.Empty, OnPlacedObjectPreSpawn);
		PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ChangeQuantity(-1);
		NetworkSingleton<BuildManager>.Instance.PlayBuildSound((ItemInstance.Definition as BuildableItemDefinition).BuildSoundType, GhostModel.transform.position);
		return result;
	}

	protected virtual void OnPlacedObjectPreSpawn(GridItem item)
	{
	}

	protected virtual void OnClosestIntersectionChanged(TileIntersection previous, TileIntersection current)
	{
		if (previous != null && (Object)(object)previous.tile != (Object)null && Singleton<HeatmapManager>.InstanceExists)
		{
			Singleton<HeatmapManager>.Instance.SetHeatmapActive(previous.tile.OwnerGrid.ParentProperty, isActive: false);
		}
		if (current != null && (Object)(object)current.tile != (Object)null)
		{
			SetShowTemperatures(_showTemperatures, current.tile.OwnerGrid.ParentProperty);
		}
	}

	protected virtual void SetShowTemperatures(bool show, ScheduleOne.Property.Property property)
	{
		_showTemperatures = show;
		if ((Object)(object)property != (Object)null && Singleton<HeatmapManager>.InstanceExists)
		{
			Singleton<HeatmapManager>.Instance.SetHeatmapActive(property, _showTemperatures);
		}
	}

	private Vector2 GetOriginCoordinate()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		BuildableItemClass.OriginFootprint.tileDetector.CheckIntersections();
		TileIntersection tileIntersection = GetRelevantIntersections(BuildableItemClass.OriginFootprint)[0];
		return new Vector2((float)tileIntersection.tile.x, (float)tileIntersection.tile.y);
	}

	private Grid GetHoveredGrid()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Collider[] array = Physics.OverlapSphere(GhostModel.transform.position, 1.5f, LayerMask.op_Implicit(detectionMask));
		for (int i = 0; i < array.Length; i++)
		{
			Tile component = ((Component)array[i]).GetComponent<Tile>();
			if ((Object)(object)component != (Object)null)
			{
				return component.OwnerGrid;
			}
		}
		return null;
	}
}
