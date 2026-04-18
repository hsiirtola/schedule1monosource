using System.Collections.Generic;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.Building;

public class BuildUpdate_ProceduralGrid : BuildUpdate_Base
{
	public class Intersection
	{
		public FootprintTile footprintTile;

		public ProceduralTile procTile;
	}

	public GameObject GhostModel;

	public ProceduralGridItem ItemClass;

	public ItemInstance ItemInstance;

	[Header("Settings")]
	public float detectionRange = 6f;

	public LayerMask detectionMask;

	public float rotation_Smoothing = 5f;

	protected float currentRotation;

	protected bool validPosition;

	protected Material currentGhostMaterial;

	protected Intersection bestIntersection;

	protected virtual void Update()
	{
		CheckRotation();
		if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && validPosition)
		{
			Place();
		}
	}

	protected virtual void LateUpdate()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		validPosition = false;
		GhostModel.transform.up = Vector3.up;
		if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(detectionRange, out var hit, detectionMask))
		{
			GhostModel.transform.position = ((RaycastHit)(ref hit)).point - GhostModel.transform.InverseTransformPoint(((Component)ItemClass.BuildPoint).transform.position);
		}
		else
		{
			GhostModel.transform.position = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position + ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward * ItemClass.HoldDistance;
			if ((Object)(object)ItemClass.MidAirCenterPoint != (Object)null)
			{
				Transform transform = GhostModel.transform;
				transform.position += -GhostModel.transform.InverseTransformPoint(((Component)ItemClass.MidAirCenterPoint).transform.position);
			}
		}
		ApplyRotation();
		CheckGridIntersections();
		UpdateMaterials();
	}

	protected void CheckRotation()
	{
		if (GameInput.GetButtonDown(GameInput.ButtonCode.RotateLeft) && !GameInput.IsTyping)
		{
			currentRotation -= 90f;
		}
		if (GameInput.GetButtonDown(GameInput.ButtonCode.RotateRight) && !GameInput.IsTyping)
		{
			currentRotation += 90f;
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
		GhostModel.transform.rotation = Quaternion.Inverse(((Component)ItemClass.BuildPoint).transform.rotation) * GhostModel.transform.rotation;
		ProceduralTile nearbyProcTile = GetNearbyProcTile();
		float num = currentRotation;
		if ((Object)(object)nearbyProcTile != (Object)null)
		{
			num += ((Component)nearbyProcTile).transform.eulerAngles.y;
		}
		GhostModel.transform.Rotate(ItemClass.BuildPoint.up, num);
	}

	protected virtual void CheckGridIntersections()
	{
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		ItemClass.CalculateFootprintTileIntersections();
		List<Intersection> list = new List<Intersection>();
		for (int i = 0; i < ItemClass.CoordinateFootprintTilePairs.Count; i++)
		{
			for (int j = 0; j < ItemClass.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.intersectedProceduralTiles.Count; j++)
			{
				Intersection intersection = new Intersection();
				intersection.footprintTile = ItemClass.CoordinateFootprintTilePairs[i].footprintTile;
				intersection.procTile = ItemClass.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.intersectedProceduralTiles[j];
				list.Add(intersection);
			}
		}
		if (list.Count == 0)
		{
			ItemClass.SetFootprintTileVisiblity(visible: false);
			return;
		}
		ItemClass.SetFootprintTileVisiblity(visible: true);
		float num = 100f;
		bestIntersection = null;
		for (int k = 0; k < list.Count; k++)
		{
			if (Vector3.Distance(((Component)list[k].footprintTile).transform.position, ((Component)list[k].procTile).transform.position) < num)
			{
				num = Vector3.Distance(((Component)list[k].footprintTile).transform.position, ((Component)list[k].procTile).transform.position);
				bestIntersection = list[k];
			}
		}
		validPosition = true;
		GhostModel.transform.position = ((Component)bestIntersection.procTile).transform.position - (((Component)bestIntersection.footprintTile).transform.position - GhostModel.transform.position);
		ItemClass.CalculateFootprintTileIntersections();
		for (int l = 0; l < ItemClass.CoordinateFootprintTilePairs.Count; l++)
		{
			bool flag = false;
			ProceduralTile closestProceduralTile = ItemClass.CoordinateFootprintTilePairs[l].footprintTile.tileDetector.GetClosestProceduralTile();
			if (IsMatchValid(ItemClass.CoordinateFootprintTilePairs[l].footprintTile, closestProceduralTile))
			{
				flag = true;
			}
			if (flag)
			{
				ItemClass.CoordinateFootprintTilePairs[l].footprintTile.tileAppearance.SetColor(ETileColor.White);
				continue;
			}
			validPosition = false;
			ItemClass.CoordinateFootprintTilePairs[l].footprintTile.tileAppearance.SetColor(ETileColor.Red);
		}
	}

	protected void UpdateMaterials()
	{
		Material val = NetworkSingleton<BuildManager>.Instance.ghostMaterial_White;
		if (!validPosition)
		{
			val = NetworkSingleton<BuildManager>.Instance.ghostMaterial_Red;
		}
		if ((Object)(object)currentGhostMaterial != (Object)(object)val)
		{
			currentGhostMaterial = val;
			NetworkSingleton<BuildManager>.Instance.ApplyMaterial(GhostModel, val);
		}
	}

	private bool IsMatchValid(FootprintTile footprintTile, ProceduralTile matchedTile)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)footprintTile == (Object)null || (Object)(object)matchedTile == (Object)null)
		{
			return false;
		}
		if (Vector3.Distance(((Component)matchedTile).transform.position, ((Component)footprintTile).transform.position) < 0.01f && matchedTile.Occupants.Count == 0 && matchedTile.TileType == ItemClass.ProceduralTileType)
		{
			return true;
		}
		return false;
	}

	protected void Place()
	{
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		List<CoordinateProceduralTilePair> list = new List<CoordinateProceduralTilePair>();
		for (int i = 0; i < ItemClass.CoordinateFootprintTilePairs.Count; i++)
		{
			bool flag = false;
			ProceduralTile closestProceduralTile = ItemClass.CoordinateFootprintTilePairs[i].footprintTile.tileDetector.GetClosestProceduralTile();
			if (IsMatchValid(ItemClass.CoordinateFootprintTilePairs[i].footprintTile, closestProceduralTile))
			{
				flag = true;
			}
			if (!flag)
			{
				Console.LogWarning("Invalid placement!");
				return;
			}
			NetworkObject networkObject = ((NetworkBehaviour)closestProceduralTile.ParentBuildableItem).NetworkObject;
			int tileIndex = (closestProceduralTile.ParentBuildableItem as IProceduralTileContainer).ProceduralTiles.IndexOf(closestProceduralTile);
			list.Add(new CoordinateProceduralTilePair
			{
				coord = ItemClass.CoordinateFootprintTilePairs[i].coord,
				tileParent = networkObject,
				tileIndex = tileIndex
			});
		}
		float num = Vector3.SignedAngle(((Component)list[0].tile).transform.forward, GhostModel.transform.forward, ((Component)list[0].tile).transform.up);
		NetworkSingleton<BuildManager>.Instance.CreateProceduralGridItem(ItemInstance.GetCopy(1), Mathf.RoundToInt(num), list);
		PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ChangeQuantity(-1);
		NetworkSingleton<BuildManager>.Instance.PlayBuildSound((ItemInstance.Definition as BuildableItemDefinition).BuildSoundType, GhostModel.transform.position);
	}

	private ProceduralTile GetNearbyProcTile()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Collider[] array = Physics.OverlapSphere(GhostModel.transform.position, 1f, LayerMask.op_Implicit(detectionMask));
		for (int i = 0; i < array.Length; i++)
		{
			ProceduralTile component = ((Component)array[i]).GetComponent<ProceduralTile>();
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
		}
		return null;
	}
}
