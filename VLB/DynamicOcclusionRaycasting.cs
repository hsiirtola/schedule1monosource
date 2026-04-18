using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLB;

[ExecuteInEditMode]
[HelpURL("http://saladgamer.com/vlb-doc/comp-dynocclusion-sd-raycasting/")]
public class DynamicOcclusionRaycasting : DynamicOcclusionAbstractBase
{
	public struct HitResult
	{
		public Vector3 point;

		public Vector3 normal;

		public float distance;

		private Collider2D collider2D;

		private Collider collider3D;

		public bool hasCollider
		{
			get
			{
				if (!Object.op_Implicit((Object)(object)collider2D))
				{
					return Object.op_Implicit((Object)(object)collider3D);
				}
				return true;
			}
		}

		public string name
		{
			get
			{
				if (Object.op_Implicit((Object)(object)collider3D))
				{
					return ((Object)collider3D).name;
				}
				if (Object.op_Implicit((Object)(object)collider2D))
				{
					return ((Object)collider2D).name;
				}
				return "null collider";
			}
		}

		public Bounds bounds
		{
			get
			{
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				if (Object.op_Implicit((Object)(object)collider3D))
				{
					return collider3D.bounds;
				}
				if (Object.op_Implicit((Object)(object)collider2D))
				{
					return collider2D.bounds;
				}
				return default(Bounds);
			}
		}

		public HitResult(ref RaycastHit hit3D)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			point = ((RaycastHit)(ref hit3D)).point;
			normal = ((RaycastHit)(ref hit3D)).normal;
			distance = ((RaycastHit)(ref hit3D)).distance;
			collider3D = ((RaycastHit)(ref hit3D)).collider;
			collider2D = null;
		}

		public HitResult(ref RaycastHit2D hit2D)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			point = Vector2.op_Implicit(((RaycastHit2D)(ref hit2D)).point);
			normal = Vector2.op_Implicit(((RaycastHit2D)(ref hit2D)).normal);
			distance = ((RaycastHit2D)(ref hit2D)).distance;
			collider2D = ((RaycastHit2D)(ref hit2D)).collider;
			collider3D = null;
		}

		public void SetNull()
		{
			collider2D = null;
			collider3D = null;
		}
	}

	private enum Direction
	{
		Up = 0,
		Down = 1,
		Left = 2,
		Right = 3,
		Max2D = 1,
		Max3D = 3
	}

	public new const string ClassName = "DynamicOcclusionRaycasting";

	public Dimensions dimensions;

	public LayerMask layerMask = Consts.DynOcclusion.LayerMaskDefault;

	public bool considerTriggers;

	public float minOccluderArea;

	public float minSurfaceRatio = 0.5f;

	public float maxSurfaceDot = 0.25f;

	public PlaneAlignment planeAlignment;

	public float planeOffset = 0.1f;

	[FormerlySerializedAs("fadeDistanceToPlane")]
	public float fadeDistanceToSurface = 0.25f;

	private HitResult m_CurrentHit;

	private float m_RangeMultiplier = 1f;

	private uint m_PrevNonSubHitDirectionId;

	[Obsolete("Use 'fadeDistanceToSurface' instead")]
	public float fadeDistanceToPlane
	{
		get
		{
			return fadeDistanceToSurface;
		}
		set
		{
			fadeDistanceToSurface = value;
		}
	}

	public Plane planeEquationWS { get; private set; }

	private QueryTriggerInteraction queryTriggerInteraction
	{
		get
		{
			if (considerTriggers)
			{
				return (QueryTriggerInteraction)2;
			}
			return (QueryTriggerInteraction)1;
		}
	}

	private float raycastMaxDistance => m_Master.raycastDistance * m_RangeMultiplier * m_Master.GetLossyScale().z;

	public bool IsColliderHiddenByDynamicOccluder(Collider collider)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!planeEquationWS.IsValid())
		{
			return false;
		}
		return !GeometryUtility.TestPlanesAABB((Plane[])(object)new Plane[1] { planeEquationWS }, collider.bounds);
	}

	protected override string GetShaderKeyword()
	{
		return "VLB_OCCLUSION_CLIPPING_PLANE";
	}

	protected override MaterialManager.SD.DynamicOcclusion GetDynamicOcclusionMode()
	{
		return MaterialManager.SD.DynamicOcclusion.ClippingPlane;
	}

	protected override void OnValidateProperties()
	{
		base.OnValidateProperties();
		minOccluderArea = Mathf.Max(minOccluderArea, 0f);
		fadeDistanceToSurface = Mathf.Max(fadeDistanceToSurface, 0f);
	}

	protected override void OnEnablePostValidate()
	{
		m_CurrentHit.SetNull();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		SetHitNull();
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			TriggerZone component = ((Component)this).GetComponent<TriggerZone>();
			if (Object.op_Implicit((Object)(object)component))
			{
				m_RangeMultiplier = Mathf.Max(1f, component.rangeMultiplier);
			}
		}
	}

	private Vector3 GetRandomVectorAround(Vector3 direction, float angleDiff)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		float num = angleDiff * 0.5f;
		return Quaternion.Euler(Random.Range(0f - num, num), Random.Range(0f - num, num), Random.Range(0f - num, num)) * direction;
	}

	private HitResult GetBestHit(Vector3 rayPos, Vector3 rayDir)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (dimensions != Dimensions.Dim2D)
		{
			return GetBestHit3D(rayPos, rayDir);
		}
		return GetBestHit2D(rayPos, rayDir);
	}

	private HitResult GetBestHit3D(Vector3 rayPos, Vector3 rayDir)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit[] array = Physics.RaycastAll(rayPos, rayDir, raycastMaxDistance, ((LayerMask)(ref layerMask)).value, queryTriggerInteraction);
		int num = -1;
		float num2 = float.MaxValue;
		for (int i = 0; i < array.Length; i++)
		{
			if ((Object)(object)((Component)((RaycastHit)(ref array[i])).collider).gameObject != (Object)(object)((Component)m_Master).gameObject && ((RaycastHit)(ref array[i])).collider.bounds.GetMaxArea2D() >= minOccluderArea && ((RaycastHit)(ref array[i])).distance < num2)
			{
				num2 = ((RaycastHit)(ref array[i])).distance;
				num = i;
			}
		}
		if (num != -1)
		{
			return new HitResult(ref array[num]);
		}
		return default(HitResult);
	}

	private HitResult GetBestHit2D(Vector3 rayPos, Vector3 rayDir)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit2D[] array = Physics2D.RaycastAll(new Vector2(rayPos.x, rayPos.y), new Vector2(rayDir.x, rayDir.y), raycastMaxDistance, ((LayerMask)(ref layerMask)).value);
		int num = -1;
		float num2 = float.MaxValue;
		for (int i = 0; i < array.Length; i++)
		{
			if ((considerTriggers || !((RaycastHit2D)(ref array[i])).collider.isTrigger) && (Object)(object)((Component)((RaycastHit2D)(ref array[i])).collider).gameObject != (Object)(object)((Component)m_Master).gameObject && ((RaycastHit2D)(ref array[i])).collider.bounds.GetMaxArea2D() >= minOccluderArea && ((RaycastHit2D)(ref array[i])).distance < num2)
			{
				num2 = ((RaycastHit2D)(ref array[i])).distance;
				num = i;
			}
		}
		if (num != -1)
		{
			return new HitResult(ref array[num]);
		}
		return default(HitResult);
	}

	private uint GetDirectionCount()
	{
		if (dimensions != Dimensions.Dim2D)
		{
			return 4u;
		}
		return 2u;
	}

	private Vector3 GetDirection(uint dirInt)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		dirInt %= GetDirectionCount();
		return (Vector3)(dirInt switch
		{
			0u => m_Master.raycastGlobalUp, 
			3u => m_Master.raycastGlobalRight, 
			1u => -m_Master.raycastGlobalUp, 
			2u => -m_Master.raycastGlobalRight, 
			_ => Vector3.zero, 
		});
	}

	private bool IsHitValid(ref HitResult hit, Vector3 forwardVec)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (hit.hasCollider)
		{
			return Vector3.Dot(hit.normal, -forwardVec) >= maxSurfaceDot;
		}
		return false;
	}

	protected override bool OnProcessOcclusion(ProcessOcclusionSource source)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		Vector3 raycastGlobalForward = m_Master.raycastGlobalForward;
		HitResult hit = GetBestHit(((Component)this).transform.position, raycastGlobalForward);
		if (IsHitValid(ref hit, raycastGlobalForward))
		{
			if (minSurfaceRatio > 0.5f)
			{
				float raycastDistance = m_Master.raycastDistance;
				for (uint num = 0u; num < GetDirectionCount(); num++)
				{
					Vector3 val = GetDirection(num + m_PrevNonSubHitDirectionId) * (minSurfaceRatio * 2f - 1f);
					((Vector3)(ref val)).Scale(((Component)this).transform.localScale);
					Vector3 val2 = ((Component)this).transform.position + val * m_Master.coneRadiusStart;
					Vector3 val3 = ((Component)this).transform.position + val * m_Master.coneRadiusEnd + raycastGlobalForward * raycastDistance;
					Vector3 val4 = val3 - val2;
					HitResult hit2 = GetBestHit(val2, ((Vector3)(ref val4)).normalized);
					if (IsHitValid(ref hit2, raycastGlobalForward))
					{
						if (hit2.distance > hit.distance)
						{
							hit = hit2;
						}
						continue;
					}
					m_PrevNonSubHitDirectionId = num;
					hit.SetNull();
					break;
				}
			}
		}
		else
		{
			hit.SetNull();
		}
		SetHit(ref hit);
		return hit.hasCollider;
	}

	private void SetHit(ref HitResult hit)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!hit.hasCollider)
		{
			SetHitNull();
			return;
		}
		PlaneAlignment planeAlignment = this.planeAlignment;
		if (planeAlignment != PlaneAlignment.Surface && planeAlignment == PlaneAlignment.Beam)
		{
			SetClippingPlane(new Plane(-m_Master.raycastGlobalForward, hit.point));
		}
		else
		{
			SetClippingPlane(new Plane(hit.normal, hit.point));
		}
		m_CurrentHit = hit;
	}

	private void SetHitNull()
	{
		SetClippingPlaneOff();
		m_CurrentHit.SetNull();
	}

	protected override void OnModifyMaterialCallback(MaterialModifier.Interface owner)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Plane val = planeEquationWS;
		owner.SetMaterialProp(ShaderProperties.SD.DynamicOcclusionClippingPlaneWS, new Vector4(((Plane)(ref val)).normal.x, ((Plane)(ref val)).normal.y, ((Plane)(ref val)).normal.z, ((Plane)(ref val)).distance));
		owner.SetMaterialProp(ShaderProperties.SD.DynamicOcclusionClippingPlaneProps, fadeDistanceToSurface);
	}

	private void SetClippingPlane(Plane planeWS)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		planeWS = planeWS.TranslateCustom(((Plane)(ref planeWS)).normal * planeOffset);
		SetPlaneWS(planeWS);
		m_Master._INTERNAL_SetDynamicOcclusionCallback(GetShaderKeyword(), m_MaterialModifierCallbackCached);
	}

	private void SetClippingPlaneOff()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		SetPlaneWS(default(Plane));
		m_Master._INTERNAL_SetDynamicOcclusionCallback(GetShaderKeyword(), null);
	}

	private void SetPlaneWS(Plane planeWS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		planeEquationWS = planeWS;
	}
}
