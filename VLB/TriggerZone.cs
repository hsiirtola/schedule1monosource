using UnityEngine;

namespace VLB;

[DisallowMultipleComponent]
[RequireComponent(typeof(VolumetricLightBeamAbstractBase))]
[HelpURL("http://saladgamer.com/vlb-doc/comp-triggerzone/")]
public class TriggerZone : MonoBehaviour
{
	private enum TriggerZoneUpdateRate
	{
		OnEnable,
		OnOcclusionChange
	}

	public const string ClassName = "TriggerZone";

	public bool setIsTrigger = true;

	public float rangeMultiplier = 1f;

	private const int kMeshColliderNumSides = 8;

	private VolumetricLightBeamAbstractBase m_Beam;

	private DynamicOcclusionRaycasting m_DynamicOcclusionRaycasting;

	private PolygonCollider2D m_PolygonCollider2D;

	private TriggerZoneUpdateRate updateRate
	{
		get
		{
			if (UtilsBeamProps.GetDimensions(m_Beam) == Dimensions.Dim3D)
			{
				return TriggerZoneUpdateRate.OnEnable;
			}
			if (!((Object)(object)m_DynamicOcclusionRaycasting != (Object)null))
			{
				return TriggerZoneUpdateRate.OnEnable;
			}
			return TriggerZoneUpdateRate.OnOcclusionChange;
		}
	}

	private void OnEnable()
	{
		m_Beam = ((Component)this).GetComponent<VolumetricLightBeamAbstractBase>();
		m_DynamicOcclusionRaycasting = ((Component)this).GetComponent<DynamicOcclusionRaycasting>();
		switch (updateRate)
		{
		case TriggerZoneUpdateRate.OnEnable:
			ComputeZone();
			((Behaviour)this).enabled = false;
			break;
		case TriggerZoneUpdateRate.OnOcclusionChange:
			if (Object.op_Implicit((Object)(object)m_DynamicOcclusionRaycasting))
			{
				m_DynamicOcclusionRaycasting.onOcclusionProcessed += OnOcclusionProcessed;
			}
			break;
		}
	}

	private void OnOcclusionProcessed()
	{
		ComputeZone();
	}

	private void ComputeZone()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)m_Beam))
		{
			return;
		}
		float coneRadiusStart = UtilsBeamProps.GetConeRadiusStart(m_Beam);
		float num = UtilsBeamProps.GetFallOffEnd(m_Beam) * rangeMultiplier;
		float num2 = Mathf.LerpUnclamped(coneRadiusStart, UtilsBeamProps.GetConeRadiusEnd(m_Beam), rangeMultiplier);
		if (UtilsBeamProps.GetDimensions(m_Beam) == Dimensions.Dim3D)
		{
			MeshCollider orAddComponent = ((Component)this).gameObject.GetOrAddComponent<MeshCollider>();
			Mathf.Min(UtilsBeamProps.GetGeomSides(m_Beam), 8);
			Mesh val = MeshGenerator.GenerateConeZ_Radii_DoubleCaps(num, coneRadiusStart, num2, 8, inverted: false);
			((Object)val).hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
			orAddComponent.sharedMesh = val;
			orAddComponent.convex = setIsTrigger;
			((Collider)orAddComponent).isTrigger = setIsTrigger;
			return;
		}
		if ((Object)(object)m_PolygonCollider2D == (Object)null)
		{
			m_PolygonCollider2D = ((Component)this).gameObject.GetOrAddComponent<PolygonCollider2D>();
		}
		Vector2[] array = (Vector2[])(object)new Vector2[4]
		{
			new Vector2(0f, 0f - coneRadiusStart),
			new Vector2(num, 0f - num2),
			new Vector2(num, num2),
			new Vector2(0f, coneRadiusStart)
		};
		if (Object.op_Implicit((Object)(object)m_DynamicOcclusionRaycasting) && m_DynamicOcclusionRaycasting.planeEquationWS.IsValid())
		{
			Plane planeEquationWS = m_DynamicOcclusionRaycasting.planeEquationWS;
			if (Utils.IsAlmostZero(((Plane)(ref planeEquationWS)).normal.z))
			{
				Vector3 val2 = planeEquationWS.ClosestPointOnPlaneCustom(Vector3.zero);
				Vector3 val3 = planeEquationWS.ClosestPointOnPlaneCustom(Vector3.up);
				if (Utils.IsAlmostZero(Vector3.SqrMagnitude(val2 - val3)))
				{
					val2 = planeEquationWS.ClosestPointOnPlaneCustom(Vector3.right);
				}
				val2 = ((Component)this).transform.InverseTransformPoint(val2);
				val3 = ((Component)this).transform.InverseTransformPoint(val3);
				PolygonHelper.Plane2D plane2D = PolygonHelper.Plane2D.FromPoints(val2, val3);
				if (plane2D.normal.x > 0f)
				{
					plane2D.Flip();
				}
				array = plane2D.CutConvex(array);
			}
		}
		m_PolygonCollider2D.points = array;
		((Collider2D)m_PolygonCollider2D).isTrigger = setIsTrigger;
	}
}
