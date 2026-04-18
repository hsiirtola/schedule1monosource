using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Building;

public class BuildUpdate_Surface : BuildUpdate_Base
{
	public GameObject GhostModel;

	public SurfaceItem BuildableItemClass;

	public ItemInstance ItemInstance;

	public float CurrentRotation;

	[Header("Settings")]
	public LayerMask DetectionMask;

	protected bool validPosition;

	protected Material currentGhostMaterial;

	protected Surface hoveredValidSurface;

	private float detectionRange => Mathf.Max(BuildableItemClass.HoldDistance, 4f);

	protected virtual void Start()
	{
		LateUpdate();
	}

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
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		validPosition = false;
		GhostModel.transform.up = Vector3.up;
		PositionObjectInFrontOfPlayer(BuildableItemClass.HoldDistance, sanitizeForward: true);
		Surface surface = null;
		if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast_ExcludeBuildables(detectionRange, out var hit, DetectionMask))
		{
			surface = ((Component)((RaycastHit)(ref hit)).collider).GetComponentInParent<Surface>();
		}
		if (IsSurfaceValidForItem(surface, ((RaycastHit)(ref hit)).collider, ((RaycastHit)(ref hit)).point))
		{
			hoveredValidSurface = surface;
			validPosition = true;
		}
		else
		{
			hoveredValidSurface = null;
		}
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
		UpdateMaterials();
	}

	protected void PositionObjectInFrontOfPlayer(float dist, bool sanitizeForward)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward;
		if (sanitizeForward)
		{
			forward.y = 0f;
		}
		Vector3 val = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position + forward * dist;
		Vector3 val2 = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position - val;
		Vector3 val3 = ((Vector3)(ref val2)).normalized;
		if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast_ExcludeBuildables(detectionRange, out var hit, DetectionMask))
		{
			val = ((RaycastHit)(ref hit)).point;
			val3 = ((RaycastHit)(ref hit)).normal;
		}
		else if ((Object)(object)BuildableItemClass.MidAirCenterPoint != (Object)null)
		{
			val += -GhostModel.transform.InverseTransformPoint(((Component)BuildableItemClass.MidAirCenterPoint).transform.position);
		}
		Quaternion val4 = Quaternion.LookRotation(val3, Vector3.up);
		GhostModel.transform.rotation = val4 * Quaternion.Inverse(((Component)BuildableItemClass.BuildPoint).transform.rotation);
		GhostModel.transform.RotateAround(((Component)BuildableItemClass.BuildPoint).transform.position, ((Component)BuildableItemClass.BuildPoint).transform.forward, CurrentRotation);
		GhostModel.transform.position = val - GhostModel.transform.InverseTransformPoint(((Component)BuildableItemClass.BuildPoint).transform.position);
	}

	private bool IsSurfaceValidForItem(Surface surface, Collider hitCollider, Vector3 hitPoint)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)surface == (Object)null)
		{
			return false;
		}
		if (!BuildableItemClass.ValidSurfaceTypes.Contains(surface.SurfaceType))
		{
			return false;
		}
		if ((Object)(object)surface.ParentProperty == (Object)null || !surface.ParentProperty.IsOwned)
		{
			return false;
		}
		if (!surface.IsPointValid(hitPoint, hitCollider))
		{
			return false;
		}
		return true;
	}

	protected void CheckRotation()
	{
		if (!BuildableItemClass.AllowRotation)
		{
			CurrentRotation = 0f;
			return;
		}
		if (GameInput.GetButtonDown(GameInput.ButtonCode.RotateLeft) && !GameInput.IsTyping)
		{
			CurrentRotation -= BuildableItemClass.RotationIncrement;
		}
		if (GameInput.GetButtonDown(GameInput.ButtonCode.RotateRight) && !GameInput.IsTyping)
		{
			CurrentRotation += BuildableItemClass.RotationIncrement;
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

	protected virtual void Place()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		Mathf.RoundToInt(CurrentRotation);
		Vector3 relativePosition = hoveredValidSurface.GetRelativePosition(GhostModel.transform.position);
		Quaternion relativeRotation = hoveredValidSurface.GetRelativeRotation(GhostModel.transform.rotation);
		NetworkSingleton<BuildManager>.Instance.CreateSurfaceItem(ItemInstance.GetCopy(1), hoveredValidSurface, relativePosition, relativeRotation);
		PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ChangeQuantity(-1);
		NetworkSingleton<BuildManager>.Instance.PlayBuildSound((ItemInstance.Definition as BuildableItemDefinition).BuildSoundType, GhostModel.transform.position);
	}
}
