using System.Collections;
using UnityEngine;

namespace VLB;

[ExecuteInEditMode]
[HelpURL("http://saladgamer.com/vlb-doc/comp-skewinghandle-sd/")]
public class SkewingHandleSD : MonoBehaviour
{
	public const string ClassName = "SkewingHandleSD";

	public VolumetricLightBeamSD volumetricLightBeam;

	public bool shouldUpdateEachFrame;

	public bool IsAttachedToSelf()
	{
		if ((Object)(object)volumetricLightBeam != (Object)null)
		{
			return (Object)(object)((Component)volumetricLightBeam).gameObject == (Object)(object)((Component)this).gameObject;
		}
		return false;
	}

	public bool CanSetSkewingVector()
	{
		if ((Object)(object)volumetricLightBeam != (Object)null)
		{
			return volumetricLightBeam.canHaveMeshSkewing;
		}
		return false;
	}

	public bool CanUpdateEachFrame()
	{
		if (CanSetSkewingVector())
		{
			return volumetricLightBeam.trackChangesDuringPlaytime;
		}
		return false;
	}

	private bool ShouldUpdateEachFrame()
	{
		if (shouldUpdateEachFrame)
		{
			return CanUpdateEachFrame();
		}
		return false;
	}

	private void OnEnable()
	{
		if (CanSetSkewingVector())
		{
			SetSkewingVector();
		}
	}

	private void Start()
	{
		if (Application.isPlaying && ShouldUpdateEachFrame())
		{
			((MonoBehaviour)this).StartCoroutine(CoUpdate());
		}
	}

	private IEnumerator CoUpdate()
	{
		while (ShouldUpdateEachFrame())
		{
			SetSkewingVector();
			yield return null;
		}
	}

	private void SetSkewingVector()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Vector3 skewingLocalForwardDirection = ((Component)volumetricLightBeam).transform.InverseTransformPoint(((Component)this).transform.position);
		volumetricLightBeam.skewingLocalForwardDirection = skewingLocalForwardDirection;
	}
}
