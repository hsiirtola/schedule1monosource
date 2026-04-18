using System;
using System.Collections.Generic;
using ScheduleOne.Core;
using ScheduleOne.Property;
using UnityEngine;

namespace ScheduleOne.Building;

public class Surface : MonoBehaviour, IGUIDRegisterable
{
	public enum ESurfaceType
	{
		Wall,
		Roof
	}

	public enum EFace
	{
		Front,
		Back,
		Top,
		Bottom,
		Left,
		Right
	}

	[Header("Settings")]
	public ESurfaceType SurfaceType;

	public List<EFace> ValidFaces = new List<EFace> { EFace.Front };

	[SerializeField]
	protected string BakedGUID = string.Empty;

	public Guid GUID { get; protected set; }

	public Transform Container => ((Component)ParentProperty.Container).transform;

	[field: SerializeField]
	public ScheduleOne.Property.Property ParentProperty { get; private set; }

	[Button]
	public void RegenerateGUID()
	{
		BakedGUID = Guid.NewGuid().ToString();
	}

	private void OnDrawGizmos()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		foreach (EFace validFace in ValidFaces)
		{
			Vector3 val = Vector3.zero;
			switch (validFace)
			{
			case EFace.Front:
				val = Vector3.forward;
				break;
			case EFace.Back:
				val = Vector3.back;
				break;
			case EFace.Top:
				val = Vector3.up;
				break;
			case EFace.Bottom:
				val = Vector3.down;
				break;
			case EFace.Left:
				val = Vector3.left;
				break;
			case EFace.Right:
				val = Vector3.right;
				break;
			}
			Vector3 val2 = ((Component)this).transform.TransformDirection(val);
			Vector3 val3 = ((Component)this).transform.position + ((Component)this).transform.rotation * val * 0.5f;
			Gizmos.color = Color.green;
			Gizmos.DrawRay(val3, val2 * 0.5f);
		}
	}

	protected virtual void Awake()
	{
		if (!GUIDManager.IsGUIDValid(BakedGUID))
		{
			Console.LogError(((Object)((Component)this).gameObject).name + "'s baked GUID is not valid!");
		}
		if (GUIDManager.IsGUIDAlreadyRegistered(new Guid(BakedGUID)))
		{
			Console.LogError(((Object)((Component)this).gameObject).name + "'s baked GUID is already registered!", (Object)(object)this);
		}
		GUID = new Guid(BakedGUID);
		GUIDManager.RegisterObject(this);
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	public Vector3 GetRelativePosition(Vector3 worldPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.InverseTransformPoint(worldPosition);
	}

	public Quaternion GetRelativeRotation(Quaternion worldRotation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Inverse(((Component)this).transform.rotation) * worldRotation;
	}

	public bool IsFrontFace(Vector3 point, Collider collider)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)collider).transform.InverseTransformPoint(point).z > 0f;
	}

	public bool IsPointValid(Vector3 point, Collider hitCollider)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		if (hitCollider is BoxCollider)
		{
			val = ((BoxCollider)((hitCollider is BoxCollider) ? hitCollider : null)).center;
		}
		else if (hitCollider is MeshCollider)
		{
			Bounds bounds = ((MeshCollider)((hitCollider is MeshCollider) ? hitCollider : null)).sharedMesh.bounds;
			val = ((Bounds)(ref bounds)).center;
		}
		Vector3 val2 = ((Component)hitCollider).transform.InverseTransformPoint(point) - val;
		foreach (EFace validFace in ValidFaces)
		{
			switch (validFace)
			{
			case EFace.Front:
				if (val2.z >= 0f)
				{
					return true;
				}
				break;
			case EFace.Back:
				if (val2.z <= 0f)
				{
					return true;
				}
				break;
			case EFace.Top:
				if (val2.y >= 0f)
				{
					return true;
				}
				break;
			case EFace.Bottom:
				if (val2.y <= 0f)
				{
					return true;
				}
				break;
			case EFace.Left:
				if (val2.x <= 0f)
				{
					return true;
				}
				break;
			case EFace.Right:
				if (val2.x >= 0f)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}
}
