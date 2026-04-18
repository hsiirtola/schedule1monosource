using UnityEngine;

namespace VLB;

public abstract class VolumetricLightBeamAbstractBase : MonoBehaviour
{
	public enum AttachedLightType
	{
		NoLight,
		OtherLight,
		SpotLight
	}

	public const string ClassName = "VolumetricLightBeamAbstractBase";

	[SerializeField]
	protected int pluginVersion = -1;

	protected Light m_CachedLightSpot;

	public bool hasGeometry => (Object)(object)GetBeamGeometry() != (Object)null;

	public Bounds bounds
	{
		get
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)GetBeamGeometry() != (Object)null))
			{
				return new Bounds(Vector3.zero, Vector3.zero);
			}
			return ((Renderer)GetBeamGeometry().meshRenderer).bounds;
		}
	}

	public int _INTERNAL_pluginVersion => pluginVersion;

	public Light lightSpotAttached => m_CachedLightSpot;

	public abstract BeamGeometryAbstractBase GetBeamGeometry();

	protected abstract void SetBeamGeometryNull();

	public abstract bool IsScalable();

	public abstract Vector3 GetLossyScale();

	public Light GetLightSpotAttachedSlow(out AttachedLightType lightType)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Light component = ((Component)this).GetComponent<Light>();
		if (Object.op_Implicit((Object)(object)component))
		{
			if ((int)component.type == 0)
			{
				lightType = AttachedLightType.SpotLight;
				return component;
			}
			lightType = AttachedLightType.OtherLight;
			return null;
		}
		lightType = AttachedLightType.NoLight;
		return null;
	}

	protected void InitLightSpotAttachedCached()
	{
		m_CachedLightSpot = GetLightSpotAttachedSlow(out var _);
	}

	private void OnDestroy()
	{
		DestroyBeam();
	}

	protected void DestroyBeam()
	{
		if (Application.isPlaying)
		{
			BeamGeometryAbstractBase.DestroyBeamGeometryGameObject(GetBeamGeometry());
		}
		SetBeamGeometryNull();
	}
}
